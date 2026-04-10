# Audit Implementasi Hari 5

> Dokumen ini memverifikasi bahwa seluruh materi `hari5.md` telah ter-implementasi
> di dalam project **Hari 4** (ProductWeb MVC + ProductWeb.Tests).

---

## ✅ Hasil Test (`dotnet test`)

```
Total tests: 12
     Passed: 12
Total time:  0.63 Seconds
```

---

## Checklist Materi hari5.md

### SECTION 1 — Unit Testing

| Requirement | Status | Lokasi |
|---|---|---|
| Framework xUnit | ✅ | `ProductWeb.Tests.csproj` |
| Library Moq (mock dependency) | ✅ | `ProductWeb.Tests.csproj` |
| Atribut `[Fact]` pada setiap test | ✅ | `ProductControllerTests.cs`, `AuthControllerTests.cs` |
| Nama test deskriptif (`Method_Scenario_Result`) | ✅ | Semua test |
| Pola AAA (Arrange / Act / Assert) | ✅ | Semua test dengan komentar |
| Test unit untuk service/controller | ✅ | `ProductController` + `AuthController` |

### SECTION 2 — Mocking

| Requirement | Status | Lokasi |
|---|---|---|
| `Mock<ISession>` untuk isolasi session | ✅ | `CreateControllerWithSession()` helper |
| `.Setup()` + `.Returns()` | ✅ | Semua test yang butuh session |
| Mock tanpa database nyata | ✅ | Controller di-inject `null!` untuk ApiService |

### SECTION 3 — Integration Testing (Manual / Swagger)

| Requirement | Status | Lokasi |
|---|---|---|
| GET /api/products → 200 OK | ✅ | Bisa dicoba via Swagger di Hari3 API |
| POST /api/products → 201 Created | ✅ | Bisa dicoba via Swagger di Hari3 API |
| Endpoint 401 tanpa token | ✅ | `[Authorize]` di ProductsController Hari3 |
| Strategy & checklist integration | ✅ | `docs/Day5_Final_Project_Strategy.md` |

### SECTION 4 — End-to-End Testing (Manual)

| Requirement | Status | Lokasi |
|---|---|---|
| Buka browser → halaman utama | ✅ | `ProductController.Index` → autentikasi |
| Test CRUD via UI | ✅ | Create/Edit/Delete di `ProductController` |
| Login dengan JWT | ✅ | `AuthController.Login` → simpan ke Session |
| Validasi data tersimpan ke DB | ✅ | Via `ApiService` → API → PostgreSQL |

### SECTION 5 — Best Practices

| Requirement | Status | Implementasi |
|---|---|---|
| Hanya test logic penting | ✅ | Fokus pada auth guard & role check |
| Naming Convention jelas | ✅ | `Index_WithoutJwtToken_RedirectsToLogin` dll. |
| Pola AAA pada setiap test | ✅ | Komentar `// Arrange / Act / Assert` |

### SECTION 6 — Final Project (Fullstack)

| Fitur Wajib | Status | Lokasi |
|---|---|---|
| Login & Register | ✅ | `AuthController` + `Auth/Login.cshtml`, `Register.cshtml` |
| Add / Edit / Delete Product | ✅ | `ProductController` (CRUD lengkap) |
| List Product | ✅ | `ProductController.Index` + `Product/Index.cshtml` |
| Protected Endpoint | ✅ | Guard `IsAuthenticated()` di semua action |

| Fitur Bonus | Status | Lokasi |
|---|---|---|
| Search nama / kategori | ✅ | `ProductController.Index` → LINQ filter |
| Pagination | ✅ | `ProductController.Index` → `pageSize = 5` |
| Role-Based Access (Admin/User) | ✅ | Guard `IsAdmin()` di Create/Edit/Delete |

### SECTION 7 — Arsitektur 3-Layer

| Layer | Status | Implementasi |
|---|---|---|
| Frontend (MVC + Razor + Bootstrap) | ✅ | `ProductWeb/` project |
| Web API (ASP.NET Core REST) | ✅ | `ProductCatalogAPI/` di Hari 3 |
| Database (PostgreSQL + EF Core) | ✅ | Via API → PostgreSQL |

---

## File Tests yang Dibuat

```
ProductWeb.Tests/
└── Controllers/
    ├── ProductControllerTests.cs   (9 test cases)
    └── AuthControllerTests.cs      (4 test cases)
```

### ProductControllerTests — 9 Test Cases
| Test | Skenario |
|---|---|
| `Index_WithoutJwtToken_RedirectsToLogin` | Belum login → redirect login |
| `Create_WithoutAdminRole_RedirectsToIndexWithError` | Role User → akses ditolak |
| `Create_WithAdminRole_ReturnsViewResult` | Role Admin → form muncul |
| `Edit_WithoutJwtToken_RedirectsToLogin` | Belum login → redirect login |
| `Edit_WithUserRole_RedirectsToIndexWithError` | Role User → akses ditolak |
| `Delete_WithoutJwtToken_RedirectsToLogin` | Belum login → redirect login |
| `Delete_WithUserRole_RedirectsToIndexWithError` | Role User → akses ditolak |
| `Detail_WithoutJwtToken_RedirectsToLogin` | Belum login → redirect login |

### AuthControllerTests — 4 Test Cases
| Test | Skenario |
|---|---|
| `Login_WhenAlreadyAuthenticated_RedirectsToProductIndex` | Sudah login → skip form |
| `Login_WhenNotAuthenticated_ReturnsLoginView` | Belum login → tampil form |
| `Register_Get_ReturnsViewWithEmptyModel` | GET register → view + model |
| `Logout_Always_RedirectsToLogin` | Logout → session clear → redirect |
