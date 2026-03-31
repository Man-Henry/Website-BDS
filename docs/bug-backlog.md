# 🐛 Bug Backlog — Website QLPT

> **Cập nhật:** 28/03/2026 | **Tổng bugs:** 6 | **CRITICAL:** 4 (4 FIXED) | **HIGH:** 1 | **MEDIUM:** 1

---

## Trạng thái Bug

| Status | Mô tả |
|--------|-------|
| 🔴 OPEN | Bug chưa được xử lý |
| 🟡 IN_PROGRESS | Đang fix |
| 🟢 FIXED | Đã fix, chờ verify |
| ✅ VERIFIED | Đã verify, đóng bug |

---

## BUG-001: Swagger JSON Generation Error (500)

| Field | Value |
|-------|-------|
| **ID** | BUG-001 |
| **Title** | Swagger JSON trả về HTTP 500 Internal Server Error |
| **Severity** | 🔴 CRITICAL |
| **Module** | API Documentation (Swagger) |
| **Status** | 🟢 FIXED |
| **Sprint** | Sprint 1 |

**Description:**
Swagger UI (`/swagger`) load được giao diện nhưng không tải được API definition. Request đến `/swagger/v1/swagger.json` trả về HTTP 500. Có thể do conflict trong API versioning configuration hoặc invalid endpoint annotations.

**Steps to Reproduce:**
1. Chạy `dotnet run --launch-profile http`
2. Truy cập `https://localhost:7182/swagger`
3. Quan sát error message trên Swagger UI

**Expected:** Swagger UI hiển thị đầy đủ 11 API controllers với tất cả endpoints.
**Actual:** `Fetch error: response status is 500 /swagger/v1/swagger.json`

---

## BUG-002: JWT Configuration Fallback Hardcoded

| Field | Value |
|-------|-------|
| **ID** | BUG-002 |
| **Title** | JWT Secret Key sử dụng fallback hardcoded không an toàn |
| **Severity** | 🔴 CRITICAL |
| **Module** | Authentication (JWT) |
| **Status** | 🟢 FIXED |
| **Sprint** | Sprint 1 |

**Description:**
Trong `Program.cs` dòng 87, JWT key dùng fallback hardcoded: `"Website_QLPT_SuperSecretKey_1234567890..."`. Trong `appsettings.Development.json` không có section `Jwt` với `Key`, `Issuer`, `Audience` → luôn dùng fallback. Đây là security risk và cần configuration rõ ràng.

**Steps to Reproduce:**
1. Kiểm tra `appsettings.Development.json` → không có section `Jwt`
2. Trong `Program.cs` line 87-101, JWT config dùng `??` fallback values

**Expected:** JWT Key, Issuer, Audience được cấu hình rõ ràng trong appsettings.Development.json.
**Actual:** Sử dụng hardcoded fallback values, không có section `Jwt` trong config file.

---

## BUG-003: SeedData Chỉ Tạo 1 Property (Không Đủ Test Data)

| Field | Value |
|-------|-------|
| **ID** | BUG-003 |
| **Title** | SeedData chỉ tạo 1 Property, 3 Rooms, 2 Tenants — thiếu cho testing |
| **Severity** | 🔴 CRITICAL |
| **Module** | Data / SeedData |
| **Status** | 🟢 FIXED |
| **Sprint** | Sprint 1 |

**Description:**
`SeedData.cs` chỉ tạo 1 Property (thay vì 2), 3 Rooms (thay vì 5), 2 Tenants (thay vì 3), 2 Contracts, 2 Invoices. Cần mở rộng để có đủ test data cho tất cả test cases.

**Steps to Reproduce:**
1. Chạy `dotnet ef database drop --force && dotnet ef database update`
2. Start application → SeedData chạy
3. Kiểm tra database → chỉ có 1 Property, 3 Rooms, 2 Tenants

**Expected:** SeedData tạo ít nhất 2 properties, 5 rooms, 3 tenants, 2 contracts, 3 invoices.
**Actual:** Chỉ tạo 1 property, 3 rooms, 2 tenants, 2 contracts, 2 invoices.

---

## BUG-004: CORS Policy Không Được Cấu Hình

| Field | Value |
|-------|-------|
| **ID** | BUG-004 |
| **Title** | Không có CORS policy cho API endpoints |
| **Severity** | 🔴 CRITICAL |
| **Module** | API / CORS |
| **Status** | 🟢 FIXED |
| **Sprint** | Sprint 1 |

**Description:**
Trong `Program.cs`, không có `builder.Services.AddCors()` và `app.UseCors()`. Nếu frontend app (mobile, SPA) gọi API sẽ bị browser block bởi Same-Origin Policy. Hiện tại chỉ có MVC Views nên chưa thấy lỗi, nhưng khi dùng JWT API sẽ gặp vấn đề.

**Steps to Reproduce:**
1. Search "AddCors" trong Program.cs → không tìm thấy
2. Search "UseCors" trong Program.cs → không tìm thấy
3. Từ browser (khác origin) gọi `/api/v1/auth/login` → bị CORS block

**Expected:** CORS policy cho phép specified origins gọi API.
**Actual:** Không có CORS configuration, API bị block khi gọi cross-origin.

---

## BUG-005: Health Check Database Không Thực Sự Test Kết Nối

| Field | Value |
|-------|-------|
| **ID** | BUG-005 |
| **Title** | Database Health Check luôn trả về Healthy dù DB không kết nối được |
| **Severity** | 🟡 HIGH |
| **Module** | Health Check |
| **Status** | 🟢 FIXED |
| **Sprint** | Sprint 2 |

**Description:**
Trong `Program.cs` dòng 174-179, database health check chỉ trả về `Healthy("Database connection string configured.")` mà không thực sự test kết nối đến SQL Server. Nếu DB down, health check vẫn báo Healthy.

**Steps to Reproduce:**
1. Xem `Program.cs` line 174-179
2. Health check return `HealthCheckResult.Healthy()` hardcoded
3. Dừng SQL Server → `/health` vẫn trả Healthy

**Expected:** Health check thực sự ping database, trả Unhealthy nếu không kết nối được.
**Actual:** Luôn trả Healthy bất kể trạng thái database thực tế.

---

## BUG-006: Hangfire Dashboard URL Không Khớp README

| Field | Value |
|-------|-------|
| **ID** | BUG-006 |
| **Title** | Hangfire Dashboard URL trong README sai — actual `/admin/jobs` vs documented `/hangfire` |
| **Severity** | 🟠 MEDIUM |
| **Module** | Documentation |
| **Status** | 🟢 FIXED |
| **Sprint** | Sprint 2 |

**Description:**
README.md ghi Hangfire Dashboard URL là `https://localhost:7182/hangfire`, nhưng trong `Program.cs` dòng 372-376 thực tế map là `/admin/jobs`.

**Steps to Reproduce:**
1. Xem README.md dòng 252: `https://localhost:7182/hangfire`
2. Xem Program.cs dòng 372: `app.UseHangfireDashboard("/admin/jobs", ...)`
3. Truy cập `/hangfire` → 404

**Expected:** README và code khớp nhau.
**Actual:** README ghi `/hangfire`, code dùng `/admin/jobs`.

---

## Tổng Kết

| Severity | Count | Sprint |
|----------|-------|--------|
| 🔴 CRITICAL | 4 | Sprint 1 |
| 🟡 HIGH | 1 | Sprint 2 |
| 🟠 MEDIUM | 1 | Sprint 3 |
| 🟢 LOW | 0 | Backlog |
| **TOTAL** | **6** | |
