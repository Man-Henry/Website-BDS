# ✅ UAT Checklist & Roadmap v2.0 — Website QLPT

> **Cập nhật:** 28/03/2026 | **Phase 05 — Kết Thúc & Nâng Cấp**

---

## Part 1: Admin UAT Checklist

> Thực hiện bởi: Admin (admin@qlpt.dev / Admin@123456)

### 1.1 Login Flow
- [ ] Đăng nhập thành công với admin credentials
- [ ] Redirect tự động đến Dashboard
- [ ] Navbar hiển thị đúng menu cho Admin role
- [ ] Đăng xuất thành công, redirect về trang công cộng

### 1.2 Properties CRUD
- [ ] Xem danh sách khu nhà
- [ ] Tạo khu nhà mới (tên, địa chỉ, mô tả)
- [ ] Sửa thông tin khu nhà
- [ ] Xóa khu nhà (soft delete)

### 1.3 Rooms CRUD
- [ ] Xem danh sách phòng theo khu nhà
- [ ] Tạo phòng mới (tên, diện tích, giá)
- [ ] Sửa thông tin phòng
- [ ] Xóa phòng (soft delete)

### 1.4 Tenants CRUD
- [ ] Xem danh sách khách thuê
- [ ] Thêm khách thuê mới
- [ ] Sửa thông tin khách thuê
- [ ] Xóa khách thuê

### 1.5 Contracts CRUD
- [ ] Tạo hợp đồng cho khách thuê (gán phòng)
- [ ] Xem chi tiết hợp đồng
- [ ] Cập nhật trạng thái hợp đồng
- [ ] Kết thúc hợp đồng

### 1.6 Invoices CRUD + Export
- [ ] Tạo hóa đơn (nhập chỉ số điện/nước)
- [ ] Xem danh sách hóa đơn
- [ ] Cập nhật trạng thái hóa đơn (Paid/Unpaid)
- [ ] Xuất hóa đơn PDF
- [ ] Tính tiền điện bậc thang chính xác

### 1.7 Reports & Export
- [ ] Xuất báo cáo Excel (danh sách phòng, hóa đơn)
- [ ] Dữ liệu trong Excel chính xác

### 1.8 Maintenance
- [ ] Xem danh sách sự cố
- [ ] Cập nhật trạng thái sự cố (Received → In Progress → Resolved)

### 1.9 Dashboard
- [ ] Thống kê tổng quan hiển thị đúng (tổng phòng, occupancy rate, revenue)
- [ ] Charts render đúng data

### 1.10 System Reset
- [ ] System Reset xóa toàn bộ business data
- [ ] Admin account được giữ lại sau reset
- [ ] System khởi động clean sau reset

---

## Part 2: Tenant UAT Checklist

> Thực hiện bởi: Tenant (tenant1@qlpt.dev / Tenant@123456)

### 2.1 Login Flow
- [ ] Đăng nhập thành công với tenant credentials
- [ ] Redirect đến Tenant Dashboard
- [ ] Navbar hiển thị đúng menu cho Tenant role
- [ ] Không thấy admin menus (Properties, Reports, System Reset)

### 2.2 View Invoices
- [ ] Xem danh sách hóa đơn của mình
- [ ] Chi tiết hóa đơn hiển thị đúng (room fee, electricity, water, total)
- [ ] Không thấy hóa đơn của tenant khác

### 2.3 Online Payment
- [ ] Click "Thanh toán" → redirect đến payment gateway
- [ ] Sau thanh toán, status chuyển "Paid"
- [ ] Payment history hiển thị

### 2.4 Maintenance Ticket
- [ ] Tạo sự cố mới (title, description)
- [ ] Xem danh sách sự cố đã tạo
- [ ] Theo dõi trạng thái sự cố

### 2.5 Room Review
- [ ] Đánh giá phòng (1-5 sao + comment)
- [ ] Review hiển thị trên trang public room detail

### 2.6 Tenant Dashboard
- [ ] Dashboard hiển thị room info
- [ ] Dashboard hiển thị invoices pending
- [ ] Dashboard hiển thị maintenance tickets

---

## Part 3: Roadmap v2.0

### 🟦 Module 1: .NET MAUI Mobile App

| Item | Description |
|------|-------------|
| **Scope** | Native mobile app cho Tenant role |
| **Features** | Xem hóa đơn, Thanh toán online, Báo sự cố, Push notifications |
| **Tech** | .NET MAUI (cross-platform iOS/Android) |
| **API** | Sử dụng existing JWT API endpoints |
| **Timeline** | Q3 2026 (3 tháng) |
| **Priority** | HIGH |

### 🟪 Module 2: AI Chatbot

| Item | Description |
|------|-------------|
| **Scope** | Hỗ trợ tenant auto-reply |
| **Features** | FAQ tự động (hóa đơn, hợp đồng, sự cố), NLP tiếng Việt |
| **Tech** | OpenAI API / Azure Bot Service |
| **Integration** | Tích hợp vào SignalR Chat |
| **Timeline** | Q4 2026 (2 tháng) |
| **Priority** | MEDIUM |

### 🟧 Module 3: Multi-tenant SaaS

| Item | Description |
|------|-------------|
| **Scope** | Cho phép nhiều landlord dùng cùng platform |
| **Features** | Tenant isolation (schema-per-tenant), Custom branding, Subscription plans |
| **Tech** | EF Core multi-tenant middleware, Stripe/PayOS subscriptions |
| **Database** | Schema-based isolation hoặc Row-level security |
| **Timeline** | Q1 2027 (4 tháng) |
| **Priority** | MEDIUM |

### 🟩 Module 4: CI/CD Pipeline

| Item | Description |
|------|-------------|
| **Scope** | Automated build, test, deploy |
| **Features** | GitHub Actions workflow, Auto test on PR, Docker build & push, Staging deploy |
| **Tech** | GitHub Actions, Docker Hub/ACR, Azure App Service |
| **Stages** | Build → Test → Docker Build → Push → Deploy Staging → Deploy Production |
| **Timeline** | Q2 2026 (2 tuần setup) |
| **Priority** | HIGH |

### Roadmap Timeline

```
Q2 2026  ████ CI/CD Pipeline
Q3 2026  ████████████ .NET MAUI Mobile App
Q4 2026  ████████ AI Chatbot
Q1 2027  ████████████████ Multi-tenant SaaS
```

---

## Part 4: Project Handover Checklist

### Documentation
- [ ] README.md cập nhật đầy đủ (tech stack, setup, API list, roles)
- [ ] API documentation qua Swagger (11 controllers)
- [ ] Database schema documentation
- [ ] Deployment guide (local + Docker)
- [ ] Admin user guide

### Source Code
- [ ] Source code đẩy lên GitHub (Man-Henry/Website-BDS)
- [ ] .gitignore properly configured
- [ ] Không có secrets trong code (appsettings.Example.json)
- [ ] Docker & Docker Compose files included

### Testing Artifacts
- [ ] Postman Collection (docs/Website_QLPT.postman_collection.json)
- [ ] Postman Environment (docs/Website_QLPT.postman_environment.json)
- [ ] Test Cases document (docs/test-cases.md)
- [ ] Test results report (docs/test-matrix-dashboard.md)

### Quality Artifacts
- [ ] Bug Backlog (docs/bug-backlog.md) — closed + remaining
- [ ] Role-Function Matrix (docs/role-function-matrix.md)
- [ ] Gantt Chart (docs/gantt-chart.md)
- [ ] Roadmap v2.0 (trong file này)

### Infrastructure
- [ ] Database migration scripts (Migrations/)
- [ ] SeedData cho demo environment
- [ ] Health Check endpoints verified
- [ ] Docker build verified
