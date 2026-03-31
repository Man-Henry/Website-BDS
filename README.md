# 📋 BÁO CÁO DỰ ÁN — Website Quản Lý Phòng Trọ (QLPT)

> **Phiên bản:** 2.0 | **Cập nhật lần cuối:** 31/03/2026  
> **Framework:** ASP.NET Core MVC (.NET 10) | **Database:** SQL Server Express  
> **Repository:** [github.com/Man-Henry/Website-BDS](https://github.com/Man-Henry/Website-BDS)

---

## 1. 🛠️ Công Nghệ Sử Dụng

| Thành phần | Công nghệ | Phiên bản |
|-----------|----------|-----------|
| **Framework** | ASP.NET Core MVC | .NET 10.0 |
| **Database** | SQL Server Express (LocalDB) | SQLEXPRESS |
| **ORM** | Entity Framework Core | 10.0.3 |
| **Authentication** | ASP.NET Identity + JWT Bearer | 10.0.3 / 10.0.5 |
| **Real-time** | SignalR (2 Hubs) | Built-in |
| **Background Jobs** | Hangfire + SQL Server Storage | 1.8.18 |
| **API Documentation** | Swagger / Swashbuckle | 10.1.5 |
| **API Versioning** | Asp.Versioning.Mvc | 8.1.0 |
| **PDF Generation** | QuestPDF | 2026.2.4 |
| **Excel Export** | ClosedXML | 0.105.0 |
| **Logging** | Serilog (Console + File Sink) | 8.0.3 |
| **Resilience** | Polly (Circuit Breaker + Retry) | 3.0.0 |
| **Health Check** | AspNetCore.HealthChecks.UI | 9.0.0 |
| **Pagination** | X.PagedList.Mvc.Core | 10.5.9 |
| **Map Engine** | Leaflet.js (local) + MarkerCluster + Routing Machine | 1.9.4 |
| **Geocoding** | Nominatim (OpenStreetMap) API | — |
| **Architecture Test** | NetArchTest.Rules | 1.3.2 |
| **Unit Testing** | xUnit + FluentAssertions + Moq | 2.9.3 / 8.9.0 / 4.20.72 |
| **Code Coverage** | coverlet.collector | 6.0.4 |
| **Frontend** | Bootstrap 5 + Bootstrap Icons + Google Fonts (Inter) | — |
| **PWA** | Service Worker + Manifest | — |
| **Containerization** | Docker + Docker Compose | Multi-stage |

---

## 2. 📦 Gói NuGet Packages

### Main Project (22 packages)

| # | Package | Version | Mục đích |
|---|---------|---------|----------|
| 1 | `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.3 | ORM kết nối SQL Server |
| 2 | `Microsoft.EntityFrameworkCore.Tools` | 10.0.3 | Migration & scaffolding |
| 3 | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.3 | Hệ thống xác thực Identity |
| 4 | `Microsoft.AspNetCore.Identity.UI` | 10.0.3 | UI đăng nhập/đăng ký |
| 5 | `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.5 | JWT cho API authentication |
| 6 | `Hangfire.AspNetCore` | 1.8.18 | Background job scheduling |
| 7 | `Hangfire.Core` | 1.8.18 | Core engine Hangfire |
| 8 | `Hangfire.SqlServer` | 1.8.18 | Lưu trữ job trong SQL Server |
| 9 | `Serilog.AspNetCore` | 8.0.3 | Structured logging |
| 10 | `Serilog.Sinks.File` | 6.0.0 | Ghi log ra file |
| 11 | `Serilog.Enrichers.Environment` | 3.0.1 | Bổ sung thông tin môi trường |
| 12 | `Swashbuckle.AspNetCore` | 10.1.5 | Swagger API documentation |
| 13 | `Asp.Versioning.Mvc` | 8.1.0 | API versioning |
| 14 | `Asp.Versioning.Mvc.ApiExplorer` | 8.1.0 | API version explorer cho Swagger |
| 15 | `QuestPDF` | 2026.2.4 | Xuất hóa đơn PDF |
| 16 | `ClosedXML` | 0.105.0 | Xuất báo cáo Excel |
| 17 | `X.PagedList.Mvc.Core` | 10.5.9 | Phân trang danh sách |
| 18 | `Microsoft.Extensions.Http.Polly` | 10.0.5 | HTTP resilience policies |
| 19 | `Polly.Extensions.Http` | 3.0.0 | Circuit breaker & retry |
| 20 | `AspNetCore.HealthChecks.UI.Client` | 9.0.0 | Health check dashboard |
| 21 | `NetArchTest.Rules` | 1.3.2 | Kiểm tra kiến trúc code |
| 22 | `Microsoft.VisualStudio.Web.CodeGeneration.Design` | 10.0.2 | Code scaffolding |

### Test Project (9 packages)

| # | Package | Version | Mục đích |
|---|---------|---------|----------|
| 1 | `xunit` | 2.9.3 | Unit testing framework |
| 2 | `xunit.runner.visualstudio` | 3.1.4 | Test runner cho Visual Studio |
| 3 | `Microsoft.NET.Test.Sdk` | 17.14.1 | Test SDK |
| 4 | `FluentAssertions` | 8.9.0 | Assertion library |
| 5 | `Moq` | 4.20.72 | Mocking framework |
| 6 | `Microsoft.EntityFrameworkCore.InMemory` | 10.0.5 | In-memory DB cho testing |
| 7 | `Microsoft.AspNetCore.Mvc.Testing` | 10.0.5 | Integration testing |
| 8 | `coverlet.collector` | 6.0.4 | Code coverage |
| 9 | `NetArchTest.Rules` | 1.3.2 | Architecture tests |

---

## 3. 🌐 Danh Sách API

### 3.1 — REST API Controllers (`/api/v1/`) — 11 Controllers

| # | Controller | Endpoint | Method | Mô tả | Auth |
|---|-----------|----------|--------|--------|------|
| 1 | `AuthApiController` | `/api/v1/auth/login` | POST | Đăng nhập, trả JWT token | ❌ Public |
| 2 | `RoomsController` | `/api/v1/rooms` | GET | Danh sách phòng (pagination, filter) | ❌ Public |
| 3 | `RoomsController` | `/api/v1/rooms/{id}` | GET | Chi tiết phòng theo ID | ❌ Public |
| 4 | `PropertiesController` | `/api/v1/properties` | GET | Danh sách khu nhà trọ | ❌ Public |
| 5 | `InvoicesApiController` | `/api/v1/invoices/export-pdf/{id}` | GET | Xuất hóa đơn PDF | 🔒 JWT |
| 6 | `DashboardApiController` | `/api/v1/dashboard` | GET | Dữ liệu thống kê tổng quan | 🔒 JWT (Admin/Landlord) |
| 7 | `ChatApiController` | `/api/v1/chat` | GET/POST | Chat nội bộ (lịch sử + gửi) | 🔒 JWT |
| 8 | `NotificationsApiController` | `/api/v1/notifications` | GET/PUT | Thông báo real-time | 🔒 JWT |
| 9 | `MaintenanceApiController` | `/api/v1/maintenance` | GET/POST | Quản lý sự cố kỹ thuật | 🔒 JWT |
| 10 | `LocationsApiController` | `/api/v1/locations` | GET | Tìm kiếm tỉnh/huyện/xã | ❌ Public |
| 11 | `OcrApiController` | `/api/v1/ocr` | POST | OCR đọc chỉ số điện/nước | 🔒 JWT (Admin/Landlord) |
| 12 | `TenantPortalApiController` | `/api/v1/tenant-portal` | GET | Portal khách thuê | 🔒 JWT (Tenant) |

#### JWT Authentication Flow
```
1. POST /api/v1/auth/login  →  Body: { "email": "...", "password": "..." }
2. Response: { "token": "eyJhbG...", "expiration": "...", "roles": ["Admin"] }
3. Gửi request kèm header: Authorization: Bearer <token>
```

#### Rate Limiting
| Policy | Giới hạn | Áp dụng cho |
|--------|----------|-------------|
| `AuthPolicy` | 10 requests / phút | Login, Register |
| `UploadPolicy` | 60 requests / phút | Upload ảnh phòng, sự cố |

### 3.2 — MVC Controllers (Server-side rendering) — 15 Controllers

| # | Controller | File | Chức năng chính |
|---|-----------|------|----------------|
| 1 | `HomeController` | 5.3 KB | Trang chủ, Tìm phòng, Bản đồ, Chi tiết phòng |
| 2 | `DashboardController` | 8.3 KB | Dashboard tổng quan + System Reset |
| 3 | `PropertiesController` | 5.7 KB | CRUD khu nhà / dãy trọ |
| 4 | `RoomsController` | 12.9 KB | CRUD phòng + Upload ảnh |
| 5 | `TenantsController` | 8.6 KB | CRUD khách thuê |
| 6 | `ContractsController` | 11.0 KB | CRUD hợp đồng thuê |
| 7 | `InvoicesController` | 21.0 KB | CRUD hóa đơn + In PDF + Thanh toán |
| 8 | `MaintenanceTicketsController` | 12.7 KB | Quản lý sự cố kỹ thuật |
| 9 | `PaymentConfigsController` | 6.3 KB | Cấu hình VNPay/MoMo/PayOS |
| 10 | `PaymentController` | 15.0 KB | Xử lý thanh toán online + Webhook |
| 11 | `ChatController` | 1.1 KB | Chat nội bộ (SignalR) |
| 12 | `ReportsController` | 10.6 KB | Xuất báo cáo Excel |
| 13 | `RoomReviewsController` | 3.6 KB | Đánh giá phòng (1-5 ⭐) |
| 14 | `TenantDashboardController` | 3.0 KB | Dashboard dành cho khách thuê |
| 15 | `SitemapController` | 2.5 KB | SEO Sitemap XML |

### 3.3 — Health Check Endpoints

| Endpoint | Mục đích | Response |
|----------|----------|----------|
| `/health` | Full health (app + database) | `{"status":"Healthy"}` |
| `/health/live` | Liveness probe (app alive?) | HTTP 200 |
| `/health/ready` | Readiness probe (DB connected?) | Healthy/Unhealthy |

### 3.4 — SignalR Hubs (Real-time)

| Hub | Path | Chức năng |
|-----|------|-----------|
| `AppHub` | `/app-hub` | Chat real-time nội bộ |
| `NotificationHub` | `/notificationHub` | Push notification thời gian thực |

---

## 4. 🏗️ Cấu Trúc Hệ Thống

```
Website_QLPT/                          # Root project
├── Controllers/                       # MVC Controllers (15 files)
│   └── Api/                           # REST API Controllers (11 files)
├── Models/                            # Entity Models (18 files)
│   ├── Property.cs                    #   Khu nhà / Dãy trọ (+ Latitude, Longitude)
│   ├── Room.cs                        #   Phòng trọ
│   ├── RoomImage.cs                   #   Ảnh phòng
│   ├── RoomReview.cs                  #   Đánh giá phòng
│   ├── Tenant.cs                      #   Khách thuê
│   ├── Contract.cs                    #   Hợp đồng
│   ├── Invoice.cs                     #   Hóa đơn (+ payment fields)
│   ├── MaintenanceTicket.cs           #   Sự cố kỹ thuật
│   ├── MaintenanceRequest.cs          #   Yêu cầu bảo trì
│   ├── ChatMessage.cs                 #   Tin nhắn chat
│   ├── AppNotification.cs             #   Thông báo hệ thống
│   ├── PaymentConfig.cs               #   Cấu hình thanh toán
│   ├── PaymentProvider.cs             #   Nhà cung cấp thanh toán
│   ├── UtilityTier.cs                 #   Bậc thang điện/nước EVN
│   ├── LandlordProfile.cs            #   Hồ sơ chủ trọ
│   ├── AuditLog.cs                    #   Nhật ký hệ thống
│   ├── ISoftDelete.cs                 #   Interface xoá mềm
│   └── ErrorViewModel.cs             #   Error model
├── Data/
│   ├── ApplicationDbContext.cs        # DbContext + Query Filters + Soft Delete
│   └── SeedData.cs                    # Tạo tài khoản mặc định (không có demo data)
├── Views/                             # 15 thư mục Views
│   ├── Home/                          #   Trang công cộng (Index, AllRooms, Map, RoomDetails)
│   ├── Dashboard/                     #   Dashboard admin (Index, SystemReset)
│   ├── Properties/                    #   CRUD khu nhà (+ Bản đồ Leaflet + Geocoding)
│   ├── Rooms/                         #   CRUD phòng
│   ├── Tenants/                       #   CRUD khách thuê
│   ├── Contracts/                     #   CRUD hợp đồng
│   ├── Invoices/                      #   CRUD hóa đơn + Print + QR Modal
│   ├── MaintenanceTickets/            #   Quản lý sự cố
│   ├── Payment/                       #   Mock checkout
│   ├── PaymentConfigs/                #   Cấu hình thanh toán
│   ├── Chat/                          #   Chat nội bộ
│   ├── Reports/                       #   Báo cáo
│   ├── RoomReviews/                   #   Đánh giá phòng
│   ├── TenantDashboard/               #   Dashboard khách thuê
│   └── Shared/                        #   Layouts + Partials
│       ├── _PublicLayout.cshtml       #     Layout trang công cộng (dark premium theme)
│       ├── _Layout.cshtml             #     Layout admin (dark sidebar + user profile)
│       ├── _AuthLayout.cshtml         #     Layout đăng nhập/đăng ký
│       ├── _TenantLayout.cshtml       #     Layout khách thuê (đồng nhất với admin)
│       ├── _LoginPartial.cshtml       #     Partial đăng nhập
│       └── _TenantChatWidget.cshtml   #     Widget chat tenant
├── Services/                          # Business Logic (10 thư mục + 4 files)
│   ├── Billing/                       #   Tính tiền hóa đơn
│   ├── Email/                         #   Email templates
│   ├── Export/                        #   Xuất PDF (QuestPDF)
│   ├── Invoice/                       #   Invoice calculator
│   ├── Jobs/                          #   Hangfire jobs (nhắc nhở, tự tạo hóa đơn)
│   ├── Notification/                  #   SignalR notifications
│   ├── Ocr/                           #   OCR đọc chỉ số điện nước
│   ├── Payment/                       #   VNPay, MoMo, PayOS gateway
│   ├── Security/                      #   Mã hoá dữ liệu nhạy cảm
│   ├── Storage/                       #   Lưu trữ file (Local / S3)
│   └── Zalo/                          #   Zalo ZNS thông báo
├── Middleware/                        # Custom Middleware
│   └── IdempotencyMiddleware.cs       #   Chống duplicate request
├── Filters/                           # Action Filters
│   ├── HangfireAdminAuthFilter.cs     #   Auth filter cho Hangfire Dashboard
│   └── CheckSubscriptionLimitAttribute.cs  # Giới hạn subscription
├── Hubs/                              # SignalR Hubs
│   ├── AppHub.cs                      #   Chat hub
│   └── NotificationHub.cs            #   Notification hub
├── ViewModels/                        # View Models (6 files)
├── Migrations/                        # EF Core Migrations (11 migrations)
├── Website_QLPT.Tests/                # xUnit Test Project
│   ├── Controllers/                   #   Unit tests (Auth, Rooms, Invoices)
│   ├── Architecture/                  #   NetArchTest rules (7 rules)
│   └── Helpers/                       #   Test DB factory
├── wwwroot/                           # Static Files
│   ├── css/                           #   admin.css, site.css
│   ├── js/                            #   admin.js, site.js, notification.js
│   ├── lib/                           #   Bootstrap 5, jQuery, bootstrap-icons, Leaflet
│   │   └── leaflet/                   #   Leaflet.js (local) + MarkerCluster + Routing Machine
│   │       ├── leaflet.js             #     Map engine (147KB)
│   │       ├── leaflet.css            #     Map styles
│   │       ├── leaflet.markercluster.js  #  Nhóm marker trên bản đồ
│   │       ├── leaflet-routing-machine.js # Chỉ đường
│   │       └── images/                #     Marker icons
│   ├── icons/                         #   PWA icons
│   ├── uploads/rooms/                 #   Ảnh phòng upload
│   ├── manifest.json                  #   PWA manifest
│   ├── sw.js                          #   Service Worker
│   └── robots.txt                     #   SEO robots
├── Areas/Identity/                    # ASP.NET Identity UI (Login, Register, Logout)
├── Program.cs                         # Entry point + Middleware pipeline + CSP headers
├── Website_QLPT.csproj                # Project file (22 NuGet packages)
├── Website_QLPT.sln                   # Solution file
├── Dockerfile                         # Docker multi-stage build
├── docker-compose.yml                 # Docker Compose orchestration
├── .dockerignore                      # Docker build exclusions
├── .gitignore                         # Git exclusions
└── appsettings.Example.json           # Configuration template
```

### Thống kê dự án

| Thành phần | Số lượng |
|-----------|----------|
| MVC Controllers | 15 |
| API Controllers | 11 |
| Entity Models | 18 |
| Views (thư mục) | 15 |
| Services (thư mục) | 10 |
| ViewModels | 6 |
| SignalR Hubs | 2 |
| Middleware | 1 |
| Filters | 2 |
| Migrations | 11 |
| Unit Tests | 22 |
| NuGet Packages (Main) | 22 |
| NuGet Packages (Test) | 9 |

---

## 5. 👥 Quyền Của Loại Tài Khoản

### 3 Roles trong hệ thống:

| Quyền | Admin | Landlord | Tenant |
|-------|:-----:|:--------:|:------:|
| **Quản lý Khu nhà / Dãy trọ** | ✅ | ✅ | ❌ |
| **Quản lý Phòng + Upload ảnh** | ✅ | ✅ | ❌ |
| **Quản lý Khách thuê** | ✅ | ✅ | ❌ |
| **Quản lý Hợp đồng** | ✅ | ✅ | ❌ |
| **Tạo / Sửa / Xoá Hóa đơn** | ✅ | ✅ | ❌ |
| **Xem Hóa đơn (của mình)** | ✅ | ✅ | ✅ |
| **Thanh toán Online** | ❌ | ❌ | ✅ |
| **Dashboard Tổng quan** | ✅ | ✅ | ❌ |
| **Dashboard Khách thuê** | ❌ | ❌ | ✅ |
| **Báo cáo Sự cố** | ✅ | ✅ | ✅ |
| **Xử lý Sự cố** | ✅ | ✅ | ❌ |
| **Chat Nội bộ** | ✅ | ✅ | ❌ |
| **Cấu hình Thanh toán** | ✅ | ✅ | ❌ |
| **OCR Đọc chỉ số điện/nước** | ✅ | ✅ | ❌ |
| **Xuất Báo cáo Excel** | ✅ | ✅ | ❌ |
| **In Hóa đơn PDF** | ✅ | ✅ | ❌ |
| **🔴 Reset Hệ Thống** | ✅ | ✅ | ❌ |
| **Xem trang công cộng** | ✅ | ✅ | ✅ |
| **Đánh giá phòng** | ❌ | ❌ | ✅ |

### 🔑 Tài khoản mặc định

```
📧 Email:    admin@qlpt.dev
🔒 Password: Admin@123456
🛡️ Roles:    Admin, Landlord

📧 Email:   chunha@gmail.com
🔒 Password: 19062004mM
🛡️ Roles:    Landlord

📧 Email:   khachthue@gmail.com
🔒 Password: 19062004mM
🛡️ Roles:    Tenant
```

> ⚠️ **Production**: Credentials nên đọc từ biến môi trường `QLPT_ADMIN_EMAIL` và `QLPT_ADMIN_PASSWORD`.

---

## 6. 🗺️ Tính Năng Bản Đồ & Geocoding

### 6.1 — Auto-Geocoding (Tìm vị trí từ địa chỉ)

| Trang | Tính năng |
|-------|-----------|
| `Properties/Create` | Nhập địa chỉ → Nhấn **"Tìm trên bản đồ"** → Tự động xác định tọa độ |
| `Properties/Edit` | Sửa địa chỉ → Cập nhật lại tọa độ trên bản đồ |
| `Home/Map` | Hiển thị tất cả khu nhà trên bản đồ với MarkerCluster |
| `Home/RoomDetails` | Bản đồ + Chỉ đường từ vị trí người dùng đến khu nhà |

### 6.2 — Luồng Geocoding

```
1. Chủ nhà nhập địa chỉ khu nhà
2. Nhấn nút "Tìm trên bản đồ"
3. Gọi Nominatim API: https://nominatim.openstreetmap.org/search?q=<địa chỉ>
4. API trả về tọa độ (Latitude, Longitude)
5. Marker tự động đặt lên bản đồ Leaflet
6. Click trên bản đồ → Reverse geocode → Cập nhật lại ô địa chỉ
```

### 6.3 — Thư viện bản đồ (Local — không phụ thuộc CDN)

```
wwwroot/lib/leaflet/
├── leaflet.js                    # Map engine (Leaflet 1.9.4)
├── leaflet.css                   # Map styles
├── leaflet.markercluster.js      # Nhóm marker (trang Map)
├── leaflet-routing-machine.js    # Chỉ đường (trang RoomDetails)
├── leaflet-routing-machine.css
├── MarkerCluster.css
├── MarkerCluster.Default.css
└── images/
    ├── marker-icon.png
    ├── marker-icon-2x.png
    └── marker-shadow.png
```

> 📌 Tất cả thư viện bản đồ được tải về **local** — không phụ thuộc CDN `unpkg.com`. Tránh lỗi CSP và đảm bảo hoạt động offline.

---

## 7. 🎨 Giao Diện Người Dùng (UI/UX)

### 7.1 — Thiết kế Sidebar (Dark Theme — Đồng nhất)

| Layout | Đặc điểm |
|--------|----------|
| **Admin** (`_Layout.cshtml`) | Dark sidebar + User profile (avatar gradient xanh-tím) + Badge "Admin"/"Chủ nhà" + "Về Trang Chủ" |
| **Tenant** (`_TenantLayout.cshtml`) | Dark sidebar + User profile (avatar gradient xanh lá) + Badge "Khách thuê" + "Về Trang Chủ" |
| **Public** (`_PublicLayout.cshtml`) | Dark navy header + Gold accent + Premium feel + Nút "Quản Trị"/"Trang Của Tôi" |

### 7.2 — Luồng điều hướng theo Role

```
Đăng nhập:
  Admin    → /Dashboard (sidebar admin)
  Landlord → /Dashboard (sidebar admin, badge "Chủ nhà")
  Tenant   → /TenantDashboard (sidebar tenant, badge "Khách thuê")

Đăng xuất:
  Tất cả   → Quay về trang chủ (/)

Trên trang chủ (navbar):
  Admin/Landlord → Nút "Quản Trị" → /Dashboard
  Tenant         → Nút "Trang Của Tôi" → /TenantDashboard
  Guest          → "Đăng nhập" / "Đăng ký"
```

---

## 8. 🚀 Hướng Dẫn Cài Đặt & Chạy

### 8.1 — Yêu cầu hệ thống
| Phần mềm | Bắt buộc | Tùy chọn |
|----------|:--------:|:--------:|
| .NET 10 SDK | ✅ | |
| SQL Server Express | ✅ | |
| Docker Desktop | | ✅ |
| Node.js | | ✅ (nếu cần libman) |

### 8.2 — Chạy Local (Development)

```powershell
# 1. Clone repository
git clone https://github.com/Man-Henry/Website-BDS.git
cd Website-BDS

# 2. Cấu hình connection string
copy appsettings.Example.json appsettings.json
copy appsettings.Example.json appsettings.Development.json
# → Sửa connection string phù hợp với SQL Server của bạn

# 3. Tạo database (apply tất cả migrations)
dotnet ef database update

# 4. Build & Chạy
dotnet build
dotnet run --launch-profile http
```

### 8.3 — Truy cập Website

| URL | Mô tả |
|-----|--------|
| `https://localhost:7182` | 🌐 Website chính |
| `https://localhost:7182/swagger` | 📄 Swagger API Documentation |
| `https://localhost:7182/health` | ❤️ Health Check |
| `https://localhost:7182/admin/jobs` | ⚙️ Hangfire Job Scheduler |

### 8.4 — Chạy bằng Docker

```powershell
# Build & chạy (bao gồm SQL Server container)
docker-compose up --build -d

# Kiểm tra trạng thái
docker-compose ps

# Truy cập: http://localhost:8080

# Dừng
docker-compose down

# Dừng + xoá toàn bộ data
docker-compose down -v
```

### 8.5 — Câu lệnh thường dùng

```powershell
# ─── Database ─────────────────────────────────
dotnet ef database update           # Tạo / cập nhật database
dotnet ef database drop --force     # Xoá database
dotnet ef dbcontext info            # Xem thông tin DbContext

# ─── Build & Run ──────────────────────────────
dotnet build                        # Build project
dotnet clean && dotnet build        # Clean build
dotnet run --launch-profile http    # Chạy website

# ─── Testing ──────────────────────────────────
dotnet test                         # Chạy toàn bộ tests
dotnet test --verbosity normal      # Chạy với chi tiết
dotnet test --collect:"XPlat Code Coverage"  # Chạy với coverage

# ─── Kiểm tra Database ───────────────────────
sqlcmd -S localhost\SQLEXPRESS -d QLPT_DB -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES"
```

---

## 9. 🔒 Bảo Mật (Security Hardening)

| # | Feature | Chi tiết | Trạng thái |
|---|---------|----------|:----------:|
| 1 | **HTTPS Redirect** | HTTP → HTTPS tự động | ✅ |
| 2 | **HSTS** | `max-age=31536000; includeSubDomains; preload` | ✅ |
| 3 | **CSP** | Content-Security-Policy (script, style, font, img, connect) | ✅ |
| 4 | **X-Frame-Options** | `DENY` — chống clickjacking | ✅ |
| 5 | **X-Content-Type-Options** | `nosniff` — chống MIME sniffing | ✅ |
| 6 | **X-XSS-Protection** | `0` (modern best practice) | ✅ |
| 7 | **Permissions-Policy** | Restrict: camera, mic, geolocation, payment, USB | ✅ |
| 8 | **Referrer-Policy** | `strict-origin-when-cross-origin` | ✅ |
| 9 | **Rate Limiting** | Auth: 10/min, Upload: 60/min, Queue: FIFO | ✅ |
| 10 | **Password Policy** | 8+ chars, uppercase, lowercase, digit | ✅ |
| 11 | **JWT Token** | HmacSHA256, 7-day expiry, role claims | ✅ |
| 12 | **Account Lockout** | Auto-lock sau nhiều lần login sai | ✅ |
| 13 | **Non-root Docker** | Container chạy UID 1001 (`appuser`) | ✅ |
| 14 | **Soft Delete** | Dữ liệu xoá mềm, không mất vĩnh viễn | ✅ |
| 15 | **Data Isolation** | Tenant chỉ thấy dữ liệu của mình | ✅ |
| 16 | **Encrypted Config** | Payment config mã hoá AES | ✅ |
| 17 | **Idempotency** | Middleware chống duplicate request | ✅ |
| 18 | **CORS Policy** | Whitelist origins, AllowCredentials | ✅ |

---

## 10. 🧪 Kết Quả Kiểm Thử

### 10.1 — Unit Tests (22 tests — 100% PASSED)

| # | Module | Test Class | Số tests | Kết quả |
|---|--------|-----------|----------|---------|
| 1 | **Auth** | `AuthApiControllerTests` | 5 | 🟢 5/5 |
| 2 | **Rooms** | `RoomsControllerTests` | 5 | 🟢 5/5 |
| 3 | **Invoices** | `InvoicesApiControllerTests` | 4 | 🟢 4/4 |
| 4 | **Architecture** | `ArchitectureTests` | 7 | 🟢 7/7 |
| 5 | **Features** | Notification + Integration | 1 | 🟢 1/1 |
| | **TỔNG** | | **22** | 🟢 **22/22** |

### 10.2 — UAT (User Acceptance Testing — 24 tests)

| Role | Số tests | Passed | Pass Rate |
|------|----------|--------|-----------|
| **Admin** | 14 | 14 | 🟢 100% |
| **Tenant** | 10 | 10 | 🟢 100% |
| **TỔNG** | **24** | **24** | 🟢 **100%** |

### 10.3 — Bug Backlog (6/6 CLOSED)

| Bug ID | Severity | Mô tả | Sprint | Trạng thái |
|--------|----------|-------|--------|:----------:|
| BUG-001 | 🔴 CRITICAL | Connection String Error | Sprint 1 | ✅ CLOSED |
| BUG-002 | 🔴 CRITICAL | JWT Configuration Invalid | Sprint 1 | ✅ CLOSED |
| BUG-003 | 🔴 CRITICAL | SeedData Crash trên DB rỗng | Sprint 1 | ✅ CLOSED |
| BUG-004 | 🟠 HIGH | CORS Blocking API Calls | Sprint 1 | ✅ CLOSED |
| BUG-005 | 🟡 MEDIUM | Health Check luôn Healthy | Sprint 2 | ✅ CLOSED |
| BUG-006 | 🟡 MEDIUM | Swagger Conflict Actions | Sprint 2 | ✅ CLOSED |
| BUG-007 | 🟠 HIGH | Landlord "Access Denied" Dashboard | Phase 05 | ✅ CLOSED |
| BUG-008 | 🟡 MEDIUM | Logout hiển thị trang trắng | Phase 05 | ✅ CLOSED |
| BUG-009 | 🟡 MEDIUM | Bản đồ Leaflet không hiển thị (CDN bị CSP chặn) | Phase 05 | ✅ CLOSED |
| BUG-010 | 🟡 MEDIUM | SeedData tự tạo lại demo data khi restart | Phase 05 | ✅ CLOSED |

---

## 11. ⚠️ Vấn Đề Đã Biết & Giới Hạn

| # | Vấn đề | Mức độ | Giải pháp |
|---|--------|:------:|-----------|
| 1 | OCR service dùng DummyOcrService (mock) | 🟡 | Tích hợp Google Vision / Azure CV khi deploy |
| 2 | Zalo ZNS cần AccessToken thật | 🟡 | Đăng ký Zalo OA và cấu hình token |
| 3 | VNPay/MoMo dùng sandbox mode | 🟡 | Chuyển sang production credentials |
| 4 | Email sender cần Gmail App Password | 🟡 | Cấu hình SMTP trong appsettings |
| 5 | S3 storage cần AWS credentials | 🟡 | Hiện dùng LocalFileStorageService |
| 6 | Docker Desktop cần cài riêng | 🟢 | Chạy trực tiếp bằng `dotnet run` |
| 7 | Nominatim geocoding giới hạn 1 req/s | 🟢 | Đã implement debounce; dùng Google Maps API nếu cần scale |

---

## 12. 🗺️ Roadmap v2.0

| Phase | Module | Mô tả | Ưu tiên |
|-------|--------|--------|:-------:|
| v2.1 | **CI/CD Pipeline** | GitHub Actions: auto test → build Docker → push registry → deploy | 🔴 Cao |
| v2.2 | **MAUI Mobile App** | .NET MAUI cho Tenant: xem invoice, thanh toán, báo sự cố | 🟡 Trung bình |
| v2.3 | **AI Chatbot** | Hỗ trợ tenant hỏi đáp tự động (hóa đơn, hợp đồng, sự cố) | 🟡 Trung bình |
| v2.4 | **Multi-tenant SaaS** | Nhiều landlord dùng chung platform, data isolation | 🟢 Tương lai |

---

## 13. 📋 Checklist Bàn Giao Dự Án

| # | Hạng mục | Vị trí | Trạng thái |
|---|----------|--------|:----------:|
| 1 | Source Code | [GitHub Repository](https://github.com/Man-Henry/Website-BDS) | ✅ |
| 2 | API Documentation | Swagger `/swagger` | ✅ |
| 3 | README Report | `README.md` (tài liệu này) | ✅ |
| 4 | Configuration Template | `appsettings.Example.json` | ✅ |
| 5 | Docker Setup | `Dockerfile` + `docker-compose.yml` | ✅ |
| 6 | Unit Test Suite | `Website_QLPT.Tests/` (22 tests) | ✅ |
| 7 | Test Results Report | Mục 10 trong README | ✅ |
| 8 | Bug Backlog | Mục 10.3 trong README (10 bugs — ALL CLOSED) | ✅ |
| 9 | UAT Results | Mục 10.2 trong README | ✅ |
| 10 | Roadmap v2.0 | Mục 12 trong README | ✅ |
| 11 | Security Audit | Mục 9 trong README | ✅ |
| 12 | Known Issues | Mục 11 trong README | ✅ |
| 13 | Map & Geocoding Docs | Mục 6 trong README | ✅ |
| 14 | UI/UX Design Docs | Mục 7 trong README | ✅ |

---

## 14. 📞 Thông Tin Liên Hệ

| | Thông tin |
|---|-----------|
| 📧 **Email** | Tvm19624@gamil.com |
| 📱 **Điện thoại** | 0358513269 |
| 📍 **Địa chỉ** | TP. Hồ Chí Minh, Việt Nam |
| 🔗 **GitHub** | [github.com/Man-Henry](https://github.com/Man-Henry) |

---

> **Tác giả:** ManHenry | **License:** All rights reserved © 2026  
> **Ngày hoàn thành:** 31/03/2026 | **Trạng thái:** ✅ Production-ready
