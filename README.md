# 📋 BÁO CÁO DỰ ÁN — Website Quản Lý Phòng Trọ (QLPT)

> **Phiên bản:** 1.0 | **Cập nhật:** 26/03/2026  
> **Framework:** ASP.NET Core MVC (.NET 10) | **Database:** SQL Server Express

---

## 1. 🛠️ Công Nghệ Sử Dụng

| Thành phần | Công nghệ | Phiên bản |
|-----------|----------|-----------|
| **Framework** | ASP.NET Core MVC | .NET 10 |
| **Database** | SQL Server Express (LocalDB) | SQLEXPRESS |
| **ORM** | Entity Framework Core | 10.x |
| **Authentication** | ASP.NET Identity + JWT Bearer | |
| **Real-time** | SignalR | |
| **Background Jobs** | Hangfire + SQL Server Storage | |
| **API Documentation** | Swagger / Swashbuckle | |
| **PDF Generation** | QuestPDF | |
| **Excel Export** | ClosedXML | |
| **Logging** | Serilog (File Sink) | |
| **Resilience** | Polly (Circuit Breaker, Retry) | |
| **Health Check** | AspNetCore.HealthChecks.UI | |
| **API Versioning** | Asp.Versioning.Mvc | |
| **Pagination** | X.PagedList.Mvc.Core | |
| **Architecture Test** | NetArchTest.Rules | |
| **Frontend** | Bootstrap 5 + Bootstrap Icons + Google Fonts (Inter) | |
| **PWA** | Service Worker + Manifest | |
| **Containerization** | Docker + Docker Compose | |

---

## 2. 📦 Gói NuGet Packages

| # | Package | Mục đích |
|---|---------|----------|
| 1 | `Microsoft.EntityFrameworkCore.SqlServer` | ORM kết nối SQL Server |
| 2 | `Microsoft.EntityFrameworkCore.Tools` | Migration & scaffolding |
| 3 | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Hệ thống xác thực Identity |
| 4 | `Microsoft.AspNetCore.Identity.UI` | UI đăng nhập/đăng ký |
| 5 | `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT cho API authentication |
| 6 | `Hangfire.AspNetCore` | Background job scheduling |
| 7 | `Hangfire.Core` | Core engine Hangfire |
| 8 | `Hangfire.SqlServer` | Lưu trữ job trong SQL Server |
| 9 | `Serilog.AspNetCore` | Structured logging |
| 10 | `Serilog.Sinks.File` | Ghi log ra file |
| 11 | `Serilog.Enrichers.Environment` | Bổ sung thông tin môi trường vào log |
| 12 | `Swashbuckle.AspNetCore` | Swagger API documentation |
| 13 | `Asp.Versioning.Mvc` | API versioning |
| 14 | `Asp.Versioning.Mvc.ApiExplorer` | API version explorer cho Swagger |
| 15 | `QuestPDF` | Xuất hóa đơn PDF |
| 16 | `ClosedXML` | Xuất báo cáo Excel |
| 17 | `X.PagedList.Mvc.Core` | Phân trang danh sách |
| 18 | `Microsoft.Extensions.Http.Polly` | HTTP resilience policies |
| 19 | `Polly.Extensions.Http` | Circuit breaker & retry |
| 20 | `AspNetCore.HealthChecks.UI.Client` | Health check dashboard |
| 21 | `NetArchTest.Rules` | Kiểm tra kiến trúc code |
| 22 | `Microsoft.VisualStudio.Web.CodeGeneration.Design` | Code scaffolding |

---

## 3. 🌐 Danh Sách API

### API Controllers (`/api/v1/`)

| Controller | Endpoint | Mô tả |
|-----------|----------|--------|
| `AuthApiController` | `/api/v1/auth` | Đăng nhập JWT, Đăng ký |
| `RoomsController` | `/api/v1/rooms` | CRUD phòng trọ |
| `PropertiesController` | `/api/v1/properties` | CRUD khu nhà |
| `InvoicesApiController` | `/api/v1/invoices` | Quản lý hóa đơn |
| `DashboardApiController` | `/api/v1/dashboard` | Dữ liệu thống kê |
| `ChatApiController` | `/api/v1/chat` | Chat nội bộ |
| `NotificationsApiController` | `/api/v1/notifications` | Thông báo real-time |
| `MaintenanceApiController` | `/api/v1/maintenance` | Báo cáo sự cố |
| `LocationsApiController` | `/api/v1/locations` | Tìm kiếm địa điểm |
| `OcrApiController` | `/api/v1/ocr` | OCR đọc chỉ số điện/nước |
| `TenantPortalApiController` | `/api/v1/tenant-portal` | Portal khách thuê |

### MVC Controllers (Server-side rendering)

| Controller | Chức năng |
|-----------|----------|
| `HomeController` | Trang chủ, Tìm phòng, Bản đồ, Chi tiết phòng |
| `DashboardController` | Dashboard tổng quan + System Reset |
| `PropertiesController` | Quản lý khu nhà / dãy trọ |
| `RoomsController` | Quản lý phòng |
| `TenantsController` | Quản lý khách thuê |
| `ContractsController` | Quản lý hợp đồng |
| `InvoicesController` | Quản lý hóa đơn + In PDF |
| `MaintenanceTicketsController` | Quản lý sự cố |
| `PaymentConfigsController` | Cấu hình thanh toán VNPay/MoMo |
| `PaymentController` | Xử lý thanh toán online |
| `ChatController` | Chat nội bộ (SignalR) |
| `ReportsController` | Xuất báo cáo Excel |
| `RoomReviewsController` | Đánh giá phòng |
| `TenantDashboardController` | Dashboard khách thuê |
| `SitemapController` | SEO Sitemap |

---

## 4. 🏗️ Cấu Trúc Hệ Thống

```
Website_QLPT/
├── Controllers/           # MVC Controllers (15 files)
│   └── Api/               # REST API Controllers (11 files)
├── Models/                # Entity models (18 files)
│   ├── Property.cs        # Khu nhà / Dãy trọ
│   ├── Room.cs            # Phòng trọ
│   ├── Tenant.cs          # Khách thuê
│   ├── Contract.cs        # Hợp đồng
│   ├── Invoice.cs         # Hóa đơn
│   ├── MaintenanceTicket.cs  # Sự cố
│   ├── ChatMessage.cs     # Tin nhắn
│   ├── PaymentConfig.cs   # Cấu hình thanh toán
│   ├── UtilityTier.cs     # Bậc thang điện/nước
│   └── ISoftDelete.cs     # Interface xoá mềm
├── Data/
│   ├── ApplicationDbContext.cs  # DbContext + Query Filters
│   └── SeedData.cs              # Dữ liệu demo
├── Views/
│   ├── Home/              # Trang công cộng
│   ├── Dashboard/         # Dashboard admin
│   └── Shared/
│       ├── _PublicLayout.cshtml  # Layout công cộng (mới)
│       ├── _Layout.cshtml        # Layout admin
│       └── _TenantLayout.cshtml  # Layout khách thuê
├── Services/              # Business logic services
├── Middleware/             # Custom middleware (Idempotency...)
├── Filters/               # Action filters
├── Hubs/                  # SignalR hubs (Chat, Notification)
├── ViewModels/            # View models
├── Migrations/            # EF Core migrations
├── wwwroot/               # Static files (CSS, JS, Images)
│   ├── lib/bootstrap/     # Bootstrap 5 (local)
│   ├── css/               # Custom CSS
│   ├── js/                # Custom JS
│   └── sw.js              # Service Worker (PWA)
├── Program.cs             # Entry point + Middleware pipeline
├── appsettings.json       # Configuration
└── Dockerfile             # Docker containerization
```

---

## 5. 👥 Quyền Của Loại Tài Khoản

### 3 Roles trong hệ thống:

| Quyền | Admin | Landlord | Tenant |
|-------|:-----:|:--------:|:------:|
| **Quản lý Khu nhà / Dãy trọ** | ✅ | ✅ | ❌ |
| **Quản lý Phòng** | ✅ | ✅ | ❌ |
| **Quản lý Khách thuê** | ✅ | ❌ | ❌ |
| **Quản lý Hợp đồng** | ✅ | ❌ | ❌ |
| **Tạo / Sửa / Xoá Hóa đơn** | ✅ | ❌ | ❌ |
| **Xem Hóa đơn (của mình)** | ✅ | ❌ | ✅ |
| **Thanh toán Online** | ❌ | ❌ | ✅ |
| **Dashboard Tổng quan** | ✅ | ✅ | ❌ |
| **Dashboard Khách thuê** | ❌ | ❌ | ✅ |
| **Báo cáo Sự cố** | ✅ | ❌ | ✅ |
| **Xử lý Sự cố** | ✅ | ❌ | ❌ |
| **Chat Nội bộ** | ✅ | ✅ | ❌ |
| **Cấu hình Thanh toán** | ✅ | ❌ | ❌ |
| **OCR Đọc chỉ số** | ✅ | ❌ | ❌ |
| **Xuất Báo cáo Excel** | ✅ | ❌ | ❌ |
| **In Hóa đơn PDF** | ✅ | ❌ | ❌ |
| **🔴 Reset Hệ Thống** | ✅ | ❌ | ❌ |
| **Xem trang công cộng** | ✅ | ✅ | ✅ |
| **Đánh giá phòng** | ❌ | ❌ | ✅ |

### 🔑 Tài khoản Admin (Quản trị toàn hệ thống)

```
📧 Email:    admin@qlpt.dev
🔒 Password: Admin@123456
🛡️ Roles:    Admin, Landlord
```

> **Admin có toàn quyền**: Quản lý khu nhà, phòng, khách thuê, hợp đồng, hóa đơn, sự cố, cấu hình thanh toán, dashboard, xuất báo cáo, **Reset toàn bộ hệ thống**.

> ⚠️ **Production**: Admin credentials được đọc từ biến môi trường `QLPT_ADMIN_EMAIL` và `QLPT_ADMIN_PASSWORD`, không hardcode trong code.

---

## 6. 🚀 Cách Chạy Hệ Thống Website

### Yêu cầu hệ thống
- **.NET 10 SDK** (hoặc cao hơn)
- **SQL Server Express** (LocalDB hoặc SQLEXPRESS)
- **Node.js** (nếu cần libman restore)

### Câu lệnh chính

#### 📌 Tạo Database mới
```powershell
dotnet ef database update
```

#### 📌 Xoá Database cũ
```powershell
dotnet ef database drop --force
```

#### 📌 Xoá và tạo lại Database sạch
```powershell
dotnet ef database drop --force
dotnet ef database update
```

#### 📌 Kiểm tra toàn bộ dữ liệu trong Database
```powershell
# Mở SQL Server Management Studio (SSMS) hoặc dùng sqlcmd:
sqlcmd -S localhost\SQLEXPRESS -d QLPT_DB -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME"

# Xem dữ liệu từng bảng:
sqlcmd -S localhost\SQLEXPRESS -d QLPT_DB -Q "SELECT * FROM Properties"
sqlcmd -S localhost\SQLEXPRESS -d QLPT_DB -Q "SELECT * FROM Rooms"
sqlcmd -S localhost\SQLEXPRESS -d QLPT_DB -Q "SELECT * FROM Tenants"
sqlcmd -S localhost\SQLEXPRESS -d QLPT_DB -Q "SELECT * FROM Contracts"
sqlcmd -S localhost\SQLEXPRESS -d QLPT_DB -Q "SELECT * FROM Invoices"

# Hoặc dùng EF Core CLI:
dotnet ef dbcontext info
```

#### 📌 Build Website
```powershell
# Build bình thường
dotnet build

# Clean + Build (xoá cache cũ)
dotnet clean
dotnet build
```

#### 📌 Chạy Website
```powershell
# Chạy với launch profile (khuyến nghị)
dotnet run --launch-profile http

# Chạy không rebuild (nhanh hơn)
dotnet run --launch-profile http --no-build
```

#### 📌 Truy cập Website
- **Public**: https://localhost:7182
- **Admin Dashboard**: Đăng nhập → tự động vào Dashboard
- **Swagger API**: https://localhost:7182/swagger
- **Health Check**: https://localhost:7182/health
- **Hangfire Dashboard**: https://localhost:7182/hangfire

---

## 7. 🐳 Chạy bằng Docker (Tuỳ chọn)

```powershell
# Build và chạy
docker-compose up --build -d

# Dừng
docker-compose down
```

---

> **Tác giả:** ManHenry | **License:** All rights reserved © 2026
