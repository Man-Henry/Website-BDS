# 🧪 Test Cases — Auth + Rooms + Invoices

> **Tổng:** 14 Test Cases | **Modules:** Auth (5), Rooms (5), Invoices (4)
> **Cập nhật:** 28/03/2026

---

## Module 1: Authentication (Auth)

### TC-AUTH-001: Login with Valid Credentials

| Field | Value |
|-------|-------|
| **ID** | TC-AUTH-001 |
| **Priority** | HIGH |
| **Precondition** | Admin user đã seed: admin@qlpt.dev / Admin@123456 |

**Steps:**
1. POST `{{base_url}}/api/v1/auth/login`
2. Body (JSON): `{"email": "admin@qlpt.dev", "password": "Admin@123456"}`

**Expected Result:**
- HTTP Status: `200 OK`
- Response body chứa `token` (JWT string)
- Token decode có claims: `sub`, `email`, `roles` (Admin, Landlord)
- Token expiry > 1 hour

**Status:** ⏳ PENDING

---

### TC-AUTH-002: Login with Invalid Credentials

| Field | Value |
|-------|-------|
| **ID** | TC-AUTH-002 |
| **Priority** | HIGH |
| **Precondition** | Server running |

**Steps:**
1. POST `{{base_url}}/api/v1/auth/login`
2. Body (JSON): `{"email": "admin@qlpt.dev", "password": "WrongPassword"}`

**Expected Result:**
- HTTP Status: `401 Unauthorized`
- Response body chứa error message

**Status:** ⏳ PENDING

---

### TC-AUTH-003: Register New User

| Field | Value |
|-------|-------|
| **ID** | TC-AUTH-003 |
| **Priority** | MEDIUM |
| **Precondition** | Email `newuser@test.dev` chưa tồn tại |

**Steps:**
1. POST `{{base_url}}/api/v1/auth/register`
2. Body (JSON): `{"email": "newuser@test.dev", "password": "Test@12345", "fullName": "Test User"}`

**Expected Result:**
- HTTP Status: `200 OK` hoặc `201 Created`
- User mới được tạo trong database
- Default role: `Tenant`

**Status:** ⏳ PENDING

---

### TC-AUTH-004: Register Duplicate Email

| Field | Value |
|-------|-------|
| **ID** | TC-AUTH-004 |
| **Priority** | MEDIUM |
| **Precondition** | Email `admin@qlpt.dev` đã tồn tại |

**Steps:**
1. POST `{{base_url}}/api/v1/auth/register`
2. Body (JSON): `{"email": "admin@qlpt.dev", "password": "Test@12345", "fullName": "Duplicate"}`

**Expected Result:**
- HTTP Status: `400 Bad Request` hoặc `409 Conflict`
- Response có error message rõ ràng (e.g., "Email already exists")

**Status:** ⏳ PENDING

---

### TC-AUTH-005: Access Protected Endpoint Without Token

| Field | Value |
|-------|-------|
| **ID** | TC-AUTH-005 |
| **Priority** | HIGH |
| **Precondition** | Không có Authorization header |

**Steps:**
1. GET `{{base_url}}/api/v1/rooms` (không có header Authorization)

**Expected Result:**
- HTTP Status: `401 Unauthorized`
- Không trả về data

**Status:** ⏳ PENDING

---

## Module 2: Rooms

### TC-ROOM-001: Get All Rooms

| Field | Value |
|-------|-------|
| **ID** | TC-ROOM-001 |
| **Priority** | HIGH |
| **Precondition** | JWT token hợp lệ (Admin role) |

**Steps:**
1. GET `{{base_url}}/api/v1/rooms`
2. Header: `Authorization: Bearer {{jwt_token}}`

**Expected Result:**
- HTTP Status: `200 OK`
- Response body là array hoặc paged list chứa rooms
- Mỗi room có: id, name, area, price, status, propertyId

**Status:** ⏳ PENDING

---

### TC-ROOM-002: Create New Room

| Field | Value |
|-------|-------|
| **ID** | TC-ROOM-002 |
| **Priority** | HIGH |
| **Precondition** | JWT token (Admin), Property ID hợp lệ |

**Steps:**
1. POST `{{base_url}}/api/v1/rooms`
2. Header: `Authorization: Bearer {{jwt_token}}`
3. Body (JSON):
```json
{
  "name": "P.201",
  "area": 25,
  "price": 4000000,
  "propertyId": {{property_id}},
  "status": "Available"
}
```

**Expected Result:**
- HTTP Status: `201 Created`
- Response chứa room mới với `id` generated
- Room xuất hiện trong GET /api/v1/rooms

**Status:** ⏳ PENDING

---

### TC-ROOM-003: Update Room

| Field | Value |
|-------|-------|
| **ID** | TC-ROOM-003 |
| **Priority** | MEDIUM |
| **Precondition** | Room ID hợp lệ, JWT token (Admin) |

**Steps:**
1. PUT `{{base_url}}/api/v1/rooms/{{room_id}}`
2. Header: `Authorization: Bearer {{jwt_token}}`
3. Body (JSON): `{"name": "P.101 (Updated)", "price": 4500000}`

**Expected Result:**
- HTTP Status: `200 OK`
- Room data được cập nhật trong database
- GET room by id trả về data mới

**Status:** ⏳ PENDING

---

### TC-ROOM-004: Delete Room (Soft Delete)

| Field | Value |
|-------|-------|
| **ID** | TC-ROOM-004 |
| **Priority** | MEDIUM |
| **Precondition** | Room ID hợp lệ (không có active contract), JWT token (Admin) |

**Steps:**
1. DELETE `{{base_url}}/api/v1/rooms/{{room_id}}`
2. Header: `Authorization: Bearer {{jwt_token}}`

**Expected Result:**
- HTTP Status: `200 OK` hoặc `204 No Content`
- Room IsDeleted = true trong database
- Room không hiển thị trong GET /api/v1/rooms (query filter)

**Status:** ⏳ PENDING

---

### TC-ROOM-005: Get Room by Invalid ID

| Field | Value |
|-------|-------|
| **ID** | TC-ROOM-005 |
| **Priority** | LOW |
| **Precondition** | JWT token hợp lệ |

**Steps:**
1. GET `{{base_url}}/api/v1/rooms/99999`
2. Header: `Authorization: Bearer {{jwt_token}}`

**Expected Result:**
- HTTP Status: `404 Not Found`
- Response có error message

**Status:** ⏳ PENDING

---

## Module 3: Invoices

### TC-INV-001: Get All Invoices

| Field | Value |
|-------|-------|
| **ID** | TC-INV-001 |
| **Priority** | HIGH |
| **Precondition** | JWT token (Admin role) |

**Steps:**
1. GET `{{base_url}}/api/v1/invoices`
2. Header: `Authorization: Bearer {{jwt_token}}`

**Expected Result:**
- HTTP Status: `200 OK`
- Response chứa danh sách invoices
- Mỗi invoice có: id, contractId, month, year, roomFee, status

**Status:** ⏳ PENDING

---

### TC-INV-002: Create Invoice

| Field | Value |
|-------|-------|
| **ID** | TC-INV-002 |
| **Priority** | HIGH |
| **Precondition** | JWT token (Admin), ContractId hợp lệ |

**Steps:**
1. POST `{{base_url}}/api/v1/invoices`
2. Header: `Authorization: Bearer {{jwt_token}}`
3. Body (JSON):
```json
{
  "contractId": {{contract_id}},
  "month": 4,
  "year": 2026,
  "roomFee": 3500000,
  "electricityOld": 145,
  "electricityNew": 200,
  "waterOld": 26,
  "waterNew": 32
}
```

**Expected Result:**
- HTTP Status: `201 Created`
- Invoice status = `Unpaid` (default)
- ElectricityFee và WaterFee được tính tự động

**Status:** ⏳ PENDING

---

### TC-INV-003: Tenant Views Own Invoices Only

| Field | Value |
|-------|-------|
| **ID** | TC-INV-003 |
| **Priority** | HIGH |
| **Precondition** | JWT token (Tenant role: tenant1@qlpt.dev) |

**Steps:**
1. GET `{{base_url}}/api/v1/invoices`
2. Header: `Authorization: Bearer {{tenant_jwt_token}}`

**Expected Result:**
- HTTP Status: `200 OK`
- Response chỉ chứa invoices thuộc tenant đang login
- KHÔNG chứa invoices của tenant khác (data isolation)

**Status:** ⏳ PENDING

---

### TC-INV-004: Generate Invoice PDF

| Field | Value |
|-------|-------|
| **ID** | TC-INV-004 |
| **Priority** | MEDIUM |
| **Precondition** | Invoice ID hợp lệ, user đã login (MVC session) |

**Steps:**
1. GET `{{base_url}}/Invoices/ExportPdf/{{invoice_id}}`
2. Cookies: `.QLPT.Auth` (MVC authentication cookie)

**Expected Result:**
- HTTP Status: `200 OK`
- Content-Type: `application/pdf`
- File PDF tải về thành công
- PDF chứa thông tin hóa đơn chính xác

**Status:** ⏳ PENDING

---

## Tổng Kết Test Cases

| Module | Total | High | Medium | Low | Status |
|--------|:-----:|:----:|:------:|:---:|:------:|
| Auth | 5 | 3 | 2 | 0 | ⏳ All Pending |
| Rooms | 5 | 2 | 2 | 1 | ⏳ All Pending |
| Invoices | 4 | 3 | 1 | 0 | ⏳ All Pending |
| **TOTAL** | **14** | **8** | **5** | **1** | |
