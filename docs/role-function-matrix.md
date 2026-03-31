# 🔐 Ma Trận Kiểm Tra Role × Function — Website QLPT

> **Cập nhật:** 28/03/2026 | **Matrix:** 3 Roles × 10 Functions = 30 test points

---

## Ma Trận Quyền Truy Cập (Expected)

| # | Chức năng | Route/Endpoint | Admin | Landlord | Tenant |
|---|-----------|----------------|:-----:|:--------:|:------:|
| 1 | Quản lý Khu nhà | `/Properties`, `/api/v1/properties` | ✅ ALLOW | ✅ ALLOW | ❌ DENY |
| 2 | Quản lý Phòng | `/Rooms`, `/api/v1/rooms` | ✅ ALLOW | ✅ ALLOW | ❌ DENY |
| 3 | Quản lý Khách thuê | `/Tenants` | ✅ ALLOW | ❌ DENY | ❌ DENY |
| 4 | Quản lý Hợp đồng | `/Contracts` | ✅ ALLOW | ❌ DENY | ❌ DENY |
| 5 | CRUD Hóa đơn | `/Invoices`, `/api/v1/invoices` | ✅ ALLOW | ❌ DENY | 🔶 VIEW ONLY |
| 6 | Dashboard Tổng quan | `/Dashboard`, `/api/v1/dashboard` | ✅ ALLOW | ✅ ALLOW | ❌ DENY |
| 7 | Xử lý Sự cố | `/MaintenanceTickets` | ✅ ALLOW | ❌ DENY | 🔶 CREATE ONLY |
| 8 | Chat Nội bộ | `/Chat` | ✅ ALLOW | ✅ ALLOW | ❌ DENY |
| 9 | Xuất Báo cáo | `/Reports` | ✅ ALLOW | ❌ DENY | ❌ DENY |
| 10 | Reset Hệ thống | `/Dashboard/SystemReset` | ✅ ALLOW | ❌ DENY | ❌ DENY |

---

## Kết Quả Kiểm Tra Thực Tế

### Admin Role (admin@qlpt.dev / Admin@123456)

| # | Chức năng | Expected | Actual | Status |
|---|-----------|----------|--------|:------:|
| 1 | Quản lý Khu nhà | ALLOW | ALLOW — Truy cập `/Properties` thành công | ✅ PASS |
| 2 | Quản lý Phòng | ALLOW | ALLOW — Truy cập `/Rooms` thành công | ✅ PASS |
| 3 | Quản lý Khách thuê | ALLOW | ALLOW — Truy cập `/Tenants` thành công | ✅ PASS |
| 4 | Quản lý Hợp đồng | ALLOW | ALLOW — Truy cập `/Contracts` thành công | ✅ PASS |
| 5 | CRUD Hóa đơn | ALLOW | ALLOW — Truy cập `/Invoices` thành công | ✅ PASS |
| 6 | Dashboard Tổng quan | ALLOW | ALLOW — Truy cập `/Dashboard` thành công | ✅ PASS |
| 7 | Xử lý Sự cố | ALLOW | ALLOW — Truy cập `/MaintenanceTickets` thành công | ✅ PASS |
| 8 | Chat Nội bộ | ALLOW | ALLOW — Truy cập `/Chat` thành công | ✅ PASS |
| 9 | Xuất Báo cáo | ALLOW | ALLOW — Truy cập `/Reports` thành công | ✅ PASS |
| 10 | Reset Hệ thống | ALLOW | ALLOW — Truy cập `/Dashboard/SystemReset` thành công | ✅ PASS |

**Admin Score: 10/10 PASS** ✅

### Landlord Role

| # | Chức năng | Expected | Actual | Status |
|---|-----------|----------|--------|:------:|
| 1 | Quản lý Khu nhà | ALLOW | _Chưa test_ (chưa có Landlord account riêng) | ⏳ PENDING |
| 2 | Quản lý Phòng | ALLOW | _Chưa test_ | ⏳ PENDING |
| 3 | Quản lý Khách thuê | DENY | _Chưa test_ | ⏳ PENDING |
| 4 | Quản lý Hợp đồng | DENY | _Chưa test_ | ⏳ PENDING |
| 5 | CRUD Hóa đơn | DENY | _Chưa test_ | ⏳ PENDING |
| 6 | Dashboard Tổng quan | ALLOW | _Chưa test_ | ⏳ PENDING |
| 7 | Xử lý Sự cố | DENY | _Chưa test_ | ⏳ PENDING |
| 8 | Chat Nội bộ | ALLOW | _Chưa test_ | ⏳ PENDING |
| 9 | Xuất Báo cáo | DENY | _Chưa test_ | ⏳ PENDING |
| 10 | Reset Hệ thống | DENY | _Chưa test_ | ⏳ PENDING |

**Landlord Score: 0/10 TESTED** ⏳ — Cần tạo Landlord account riêng trong SeedData

### Tenant Role (tenant1@qlpt.dev / Tenant@123456)

| # | Chức năng | Expected | Actual | Status |
|---|-----------|----------|--------|:------:|
| 1 | Quản lý Khu nhà | DENY | _Chưa test_ | ⏳ PENDING |
| 2 | Quản lý Phòng | DENY | _Chưa test_ | ⏳ PENDING |
| 3 | Quản lý Khách thuê | DENY | _Chưa test_ | ⏳ PENDING |
| 4 | Quản lý Hợp đồng | DENY | _Chưa test_ | ⏳ PENDING |
| 5 | Xem Hóa đơn | VIEW ONLY | _Chưa test_ | ⏳ PENDING |
| 6 | Dashboard Tổng quan | DENY | _Chưa test_ | ⏳ PENDING |
| 7 | Báo cáo Sự cố | CREATE ONLY | _Chưa test_ | ⏳ PENDING |
| 8 | Chat Nội bộ | DENY | _Chưa test_ | ⏳ PENDING |
| 9 | Xuất Báo cáo | DENY | _Chưa test_ | ⏳ PENDING |
| 10 | Reset Hệ thống | DENY | _Chưa test_ | ⏳ PENDING |

**Tenant Score: 0/10 TESTED** ⏳

---

## Tổng Kết

| Role | Tested | Passed | Failed | Pending |
|------|:------:|:------:|:------:|:-------:|
| Admin | 10 | 10 | 0 | 0 |
| Landlord | 0 | 0 | 0 | 10 |
| Tenant | 0 | 0 | 0 | 10 |
| **TOTAL** | **10** | **10** | **0** | **20** |

> ⚠️ **Note:** Landlord và Tenant roles cần được test sau khi SeedData được mở rộng (BUG-003) để tạo dedicated Landlord account. Admin role verified thông qua browser test thành công.
