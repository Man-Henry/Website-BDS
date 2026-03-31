using Asp.Versioning;
using Hangfire;
using Hangfire.SqlServer;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using System.Threading.RateLimiting;
using Website_QLPT.Data;
using Website_QLPT.Hubs;
using Website_QLPT.Middleware;
using Website_QLPT.Services;
using Website_QLPT.Services.Jobs;
using Website_QLPT.Services.Security;

// ─────────────────────────────────────────────────────────────────────────────
// ① SERILOG — bootstrap logger (catches startup errors too)
// ─────────────────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ─── Serilog full setup (reads "Serilog" section from appsettings) ────────
    builder.Host.UseSerilog((ctx, services, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("App", "QLPT")
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/qlpt-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

    // ─────────────────────────────────────────────────────────────────────────
    // ② DATABASE + IDENTITY
    // ─────────────────────────────────────────────────────────────────────────
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // ─── Cookie settings & JWT Bearer ──────────────────────────────────────────────────────
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Identity/Account/Login";
        options.LogoutPath = "/Identity/Account/Logout";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Name = ".QLPT.Auth";
    });

    var jwtKey = builder.Configuration["Jwt:Key"] ?? "Website_QLPT_SuperSecretKey_1234567890_Website_QLPT_SuperSecretKey_1234567890";
    builder.Services.AddAuthentication()
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Website_QLPT",
                ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Website_QLPT_Mobile",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

    // ─────────────────────────────────────────────────────────────────────────
    // ③ APPLICATION SERVICES
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddScoped<Website_QLPT.Services.Billing.IInvoiceCalculatorService, Website_QLPT.Services.Billing.InvoiceCalculatorService>();
    builder.Services.AddScoped<Website_QLPT.Services.Export.IPdfExportService, Website_QLPT.Services.Export.PdfExportService>();
    builder.Services.Configure<Website_QLPT.Services.Zalo.ZaloOaOptions>(builder.Configuration.GetSection("ZaloOA"));
    
    // ─── POLLY CIRCUIT BREAKER CHO ZALO API ─────────────────────────────────
    var retryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    var circuitBreakerPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 3, durationOfBreak: TimeSpan.FromSeconds(30));

    builder.Services.AddHttpClient<Website_QLPT.Services.Zalo.IZaloZnsService, Website_QLPT.Services.Zalo.ZaloZnsService>(client =>
    {
        client.BaseAddress = new Uri("https://openapi.zalo.me/v2.0/");
    })
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

    builder.Services.AddTransient<SmtpEmailSender>();
    builder.Services.AddTransient<IEmailSenderService>(sp => sp.GetRequiredService<SmtpEmailSender>());
    builder.Services.AddTransient<IEmailSender>(sp => sp.GetRequiredService<SmtpEmailSender>());
    builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();
    builder.Services.AddScoped<Website_QLPT.Services.Email.IEmailTemplateService, Website_QLPT.Services.Email.EmailTemplateService>();
    builder.Services.AddScoped<Website_QLPT.Services.Payment.IInvoicePaymentService, Website_QLPT.Services.Payment.InvoicePaymentService>();
    builder.Services.AddScoped<Website_QLPT.Services.Payment.IVnPayService, Website_QLPT.Services.Payment.VnPayService>();
    builder.Services.AddScoped<Website_QLPT.Services.Ocr.IOcrService, Website_QLPT.Services.Ocr.DummyOcrService>();
    builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

    // Payment Multi-Gateway
    builder.Services.AddScoped<Website_QLPT.Services.Payment.IPaymentProvider, Website_QLPT.Services.Payment.MoMoPaymentProvider>();
    builder.Services.AddScoped<Website_QLPT.Services.Payment.IPaymentProvider, Website_QLPT.Services.Payment.PayOSPaymentProvider>();
    builder.Services.AddScoped<Website_QLPT.Services.Payment.IPaymentServiceResolver, Website_QLPT.Services.Payment.PaymentServiceResolver>();

    // Cloud Storage
    builder.Services.AddScoped<Website_QLPT.Services.Storage.IFileStorageService, Website_QLPT.Services.Storage.LocalFileStorageService>();
    builder.Services.AddSingleton<Website_QLPT.Services.Notification.INotificationService, Website_QLPT.Services.Notification.NotificationService>();


    builder.Services.AddSignalR();

    // ─── HANGFIRE ───────────────────────────────────────────────────
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));
    builder.Services.AddHangfireServer();

    // Đăng ký Jobs là Scoped service để Hangfire tự inject
    builder.Services.AddScoped<InvoiceReminderJob>();
    builder.Services.AddScoped<InvoiceAutoGenerateJob>();

    // ─────────────────────────────────────────────────────────────────────────
    // ④ HEALTH CHECKS
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddCheck("database", () =>
        {
            // Fix BUG-005: Actually test database connectivity
            // Note: This uses the connection string to attempt a raw SQL connection
            var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connStr))
                return HealthCheckResult.Unhealthy("No connection string configured.");
            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.ExecuteScalar();
                return HealthCheckResult.Healthy("Database is reachable.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database connection failed.", ex);
            }
        }, tags: ["ready"]);

    // ─────────────────────────────────────────────────────────────────────────
    // ⑤ API VERSIONING
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddApiVersioning(opt =>
    {
        opt.DefaultApiVersion = new ApiVersion(1, 0);
        opt.AssumeDefaultVersionWhenUnspecified = true;
        opt.ReportApiVersions = true;
        opt.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"));
    })
    .AddApiExplorer(opt =>
    {
        opt.GroupNameFormat = "'v'VVV";
        opt.SubstituteApiVersionInUrl = true;
    });

    // ─────────────────────────────────────────────────────────────────────────
    // ⑥ SWAGGER with JWT Bearer auth button
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title = "Website_QLPT API",
            Version = "v1",
            Description = "Public API endpoints cho hệ thống Quản Lý Phòng Trọ"
        });

        // Fix BUG-001: Resolve conflicting controller names (MVC vs API)
        c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

        // Chỉ hiển thị API controllers (có [ApiController] attribute)
        c.DocInclusionPredicate((docName, apiDesc) =>
        {
            if (apiDesc.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor controllerDesc)
            {
                return controllerDesc.ControllerTypeInfo.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.ApiControllerAttribute), true).Any();
            }
            return false;
        });
    });

    // ─── CORS POLICY (Fix BUG-004) ────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ApiCorsPolicy", policy =>
        {
            policy.WithOrigins(
                    "https://localhost:7182",
                    "http://localhost:5113",
                    "http://localhost:3000",  // React/Vue dev server
                    "http://localhost:8080")  // Mobile dev
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // ─────────────────────────────────────────────────────────────────────────
    // ⑦ MVC + RATE LIMITER
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddMemoryCache();
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages(options =>
    {
        options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
        options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
    });
    // SignalR đã đăng ký ở dòng trên (section ③)
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("UploadPolicy", opt =>
        {
            opt.PermitLimit = 60;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 2;
        });

        // Fix BUG-004 enhancement: Rate limit auth endpoints
        options.AddFixedWindowLimiter("AuthPolicy", opt =>
        {
            opt.PermitLimit = 10;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 0;
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    // ─────────────────────────────────────────────────────────────────────────
    // BUILD
    // ─────────────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ─────────────────────────────────────────────────────────────────────────
    // ⑧ HTTP SECURITY HEADERS + CSP (trước mọi middleware khác)
    // ─────────────────────────────────────────────────────────────────────────
    app.Use(async (ctx, next) =>
    {
        // ─── Core Security Headers ───────────────────────────────────────────
        ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        ctx.Response.Headers.Append("X-Frame-Options", "DENY");
        ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        ctx.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");

        // Sprint 4: Disable legacy XSS filter (modern best practice — avoids false positives)
        ctx.Response.Headers.Append("X-XSS-Protection", "0");

        // Sprint 4: Restrict browser features (camera, microphone, geolocation, payment)
        ctx.Response.Headers.Append("Permissions-Policy",
            "camera=(), microphone=(), geolocation=(), payment=(), usb=(), magnetometer=(), gyroscope=()");

        // Sprint 4: HSTS — force HTTPS for 1 year + subdomains (production only)
        if (!ctx.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            ctx.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        }

        // ─── Content Security Policy ─────────────────────────────────────────
        ctx.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://unpkg.com; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net https://unpkg.com; " +
            "font-src 'self' data: https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
            "img-src 'self' data: blob: https://images.unsplash.com https://*.unsplash.com https://*.tile.openstreetmap.org; " +
            "connect-src 'self' https://provinces.open-api.vn https://fonts.googleapis.com https://fonts.gstatic.com https://nominatim.openstreetmap.org wss:; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self';");
        await next();
    });

    // ─────────────────────────────────────────────────────────────────────────
    // ⑨ GLOBAL ERROR HANDLER (Problem Details — RFC 7807)
    // ─────────────────────────────────────────────────────────────────────────
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                ctx.Response.ContentType = "application/problem+json";

                var exceptionHandlerFeature = ctx.Features.Get<IExceptionHandlerFeature>();
                var ex = exceptionHandlerFeature?.Error;

                Log.Error(ex, "Unhandled exception on {Method} {Path}", ctx.Request.Method, ctx.Request.Path);

                var problem = new
                {
                    type = "https://tools.ietf.org/html/rfc7807",
                    title = "Internal Server Error",
                    status = 500,
                    detail = app.Environment.IsDevelopment() ? ex?.Message : "Đã xảy ra lỗi. Vui lòng thử lại sau.",
                    instance = ctx.Request.Path.ToString()
                };

                await ctx.Response.WriteAsJsonAsync(problem);
            });
        });
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();

    // ─── Dev: disable browser cache to always show latest changes ────────
    if (app.Environment.IsDevelopment())
    {
        app.Use(async (ctx, next) =>
        {
            ctx.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            ctx.Response.Headers.Append("Pragma", "no-cache");
            ctx.Response.Headers.Append("Expires", "0");
            await next();
        });
    }

    app.UseStaticFiles();
    app.UseRouting();

    // Fix BUG-004: Enable CORS for API endpoints
    app.UseCors("ApiCorsPolicy");

    app.UseIdempotency();
    app.UseRateLimiter();

    // ─────────────────────────────────────────────────────────────────────────
    // ⑩ SWAGGER (cả dev lẫn staging — chỉ tắt production nếu cần)
    // ─────────────────────────────────────────────────────────────────────────
    app.UseSwagger();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Website_QLPT API V1");
            c.DisplayRequestDuration();
        });
    }

    app.UseAuthentication();
    app.UseAuthorization();

    // ─────────────────────────────────────────────────────────────────────────
    // ⑪ HEALTH CHECK ENDPOINTS
    // ─────────────────────────────────────────────────────────────────────────
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        ResultStatusCodes =
        {
            [HealthStatus.Healthy]   = StatusCodes.Status200OK,
            [HealthStatus.Degraded]  = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
        }
    });

    // Liveness — chỉ check app đang chạy (không check DB)
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false   // Không chạy bất kỳ check nào, chỉ trả 200 nếu app alive
    });

    // Readiness — check DB và external deps
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapStaticAssets();
    app.MapControllers();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.MapRazorPages();
    app.MapHub<AppHub>("/app-hub");
    app.MapHub<NotificationHub>("/notificationHub");

    // ─── HANGFIRE DASHBOARD & RECURRING JOBS ──────────────────────────────
    app.UseHangfireDashboard("/admin/jobs", new DashboardOptions
    {
        DashboardTitle = "QLPT - Job Scheduler",
        Authorization = new[] { new Website_QLPT.Filters.HangfireAdminAuthFilter() }
    });

    // Lịch chạy Jobs định kỳ (Cron Expression)
    // Job 1: Mỗi ngày 08:00 gửi email nhắc nhở
    RecurringJob.AddOrUpdate<InvoiceReminderJob>(
        "invoice-reminder-daily",
        job => job.ExecuteAsync(),
        "0 8 * * *");  // Mỗi ngày lúc 8:00 sáng

    // Job 2: Ngày mùng 1 hàng tháng 00:01 tự tạo hóa đơn mời
    RecurringJob.AddOrUpdate<InvoiceAutoGenerateJob>(
        "invoice-auto-generate-monthly",
        job => job.ExecuteAsync(),
        "1 0 1 * *");  // Mùng 1 mỗi tháng lúc 00:01

    // ─────────────────────────────────────────────────────────────────────────
    // ⑫ DATABASE SEED
    // ─────────────────────────────────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            await Website_QLPT.Data.SeedData.Initialize(services);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred seeding the database.");
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
