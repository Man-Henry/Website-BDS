# 📊 Gantt Chart — Kế Hoạch Kiểm Tra 4 Sprints (8 Tuần)

> **Bắt đầu:** 28/03/2026 | **Kết thúc:** 23/05/2026 | **4 Sprints × 2 Tuần**

---

## Timeline Overview

```
Sprint 1 ██████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░  Tuần 1-2 (28/03 - 11/04)
Sprint 2 ░░░░░░░░░░██████████░░░░░░░░░░░░░░░░░░░░  Tuần 3-4 (14/04 - 25/04)
Sprint 3 ░░░░░░░░░░░░░░░░░░░░██████████░░░░░░░░░░  Tuần 5-6 (28/04 - 09/05)
Sprint 4 ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░██████████  Tuần 7-8 (12/05 - 23/05)
```

---

## Sprint 1: Fix CRITICAL Bugs (Tuần 1-2: 28/03 → 11/04)

| Tuần | Task | Deliverable |
|:----:|------|-------------|
| 1 | Fix BUG-001: Swagger JSON 500 Error | Swagger UI hiển thị 11 controllers |
| 1 | Fix BUG-002: JWT Configuration | JWT config trong appsettings, login trả token |
| 1 | Fix BUG-004: CORS Policy | AddCors/UseCors trong Program.cs |
| 2 | Fix BUG-003: SeedData mở rộng | 2 properties, 5 rooms, 3 tenants, 3 invoices |
| 2 | Fix CRITICAL bugs phát sinh | Regression test |
| 2 | Sprint 1 Review | Build ✅, Run ✅, Health ✅, Swagger ✅, JWT ✅ |

**🏁 Milestone S1:** _"Hệ thống khởi động thành công, API accessible"_

---

## Sprint 2: Fix HIGH + Test Infrastructure (Tuần 3-4: 14/04 → 25/04)

| Tuần | Task | Deliverable |
|:----:|------|-------------|
| 3 | Fix BUG-005: Database Health Check | Actual DB ping trong health check |
| 3 | Tạo test project Website_QLPT.Tests | xUnit + FluentAssertions + Moq |
| 3 | Viết unit tests Auth + Rooms | 10 test cases cho Auth/Rooms controllers |
| 4 | Viết unit tests Invoices + NetArchTest | 4 test cases Invoices + 5 arch rules |
| 4 | Tạo Postman Collection | 11 folders, 14+ requests |
| 4 | Sprint 2 Review | `dotnet test` all pass, Postman collection ready |

**🏁 Milestone S2:** _"Test suite cơ bản hoạt động, API collection hoàn thành"_

---

## Sprint 3: Fix MEDIUM + Feature Development (Tuần 5-6: 28/04 → 09/05)

| Tuần | Task | Deliverable |
|:----:|------|-------------|
| 5 | Fix BUG-006: Hangfire URL Documentation | README cập nhật |
| 5 | Phát triển Notification System | SignalR notifications cho invoice/maintenance |
| 5 | Phát triển Tenant Dashboard | Dashboard view cho tenant |
| 6 | Phát triển Room Review | Rating 1-5 + comment system |
| 6 | Viết tests cho features mới | Unit tests cho 3 features mới |
| 6 | Sprint 3 Review | Features functional, no regression |

**🏁 Milestone S3:** _"Features mới hoàn thành, MEDIUM bugs fixed"_

---

## Sprint 4: Docker + Security Hardening (Tuần 7-8: 12/05 → 23/05)

| Tuần | Task | Deliverable |
|:----:|------|-------------|
| 7 | Validate Docker build & run | docker-compose up thành công |
| 7 | Security headers verification | CSP, HSTS, X-Frame-Options verified |
| 7 | Rate limiting trên auth endpoints | Login/Register rate limited |
| 8 | UAT Admin flow | Admin UAT checklist all PASS |
| 8 | UAT Tenant flow | Tenant UAT checklist all PASS |
| 8 | Final Sprint Review | Production-ready, all tests green |

**🏁 Milestone S4:** _"Production-ready, UAT passed"_

---

## Sprint Velocity Tracking

| Sprint | Bugs Target | Bugs Fixed | Tests Written | Tests Passing | Features |
|:------:|:-----------:|:----------:|:-------------:|:-------------:|:--------:|
| 1 | 4 CRITICAL | _pending_ | 0 | N/A | 0 |
| 2 | 1 HIGH | _pending_ | 19 | _pending_ | 0 |
| 3 | 1 MEDIUM | _pending_ | +6 | _pending_ | 3 |
| 4 | 0 | _pending_ | +0 | _pending_ | 0 (hardening) |
