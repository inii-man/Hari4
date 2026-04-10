# Day 5: Final Project Integration & Testing Strategy

This guide provides the optimal structure and implementation strategy to handle the Day 5 Final Project. It keeps the **Backend API** and **Frontend Web** as two separate repositories while ensuring they run, test, and integrate seamlessly for a full-stack demo.

---

## 1. Recommended Folder & Workspace Strategy

To satisfy the constraint of keeping repositories separate while simplifying the developer experience, use a **Sibling Folder Structure** combined with a **VS Code Multi-Root Workspace**.

### Directory Layout
```text
/NQA_DOTNET_Bootcamp (or any parent folder)
│
├── ProductCatalogAPI/      (Repository 1 - Backend)
│   ├── .git/
│   ├── ProductCatalogAPI.csproj
│   └── ...
│
├── ProductWeb/             (Repository 2 - Frontend)
│   ├── .git/
│   ├── ProductWeb.csproj
│   └── ...
│
└── FinalProject.code-workspace  <-- (Optional) Workspace file
```

### Workspace Strategy
Do **NOT** use a single `.sln` file containing both unless you intend them to be one repository. Instead, you can have students create a `FinalProject.code-workspace` file in the parent directory:
```json
{
  "folders": [
    { "path": "ProductCatalogAPI" },
    { "path": "ProductWeb" }
  ]
}
```
* **Why this is best:** Students get one VS Code window with two separate source control sections. They remain completely unmerged but easy to navigate.

---

## 2. Integration Checklist

For the two applications to communicate flawlessly, verify the following configuration points:

### Backend (API) Configuration
- [ ] **Enable CORS:** The backend must explicitly trust the frontend's origin to accept cross-origin requests.
```csharp
  // Program.cs (API)
  builder.Services.AddCors(options => {
      options.AddPolicy("AllowFrontend", policy => {
          policy.WithOrigins("https://localhost:[FRONTEND_PORT]") // Update with actual port
                .AllowAnyHeader()
                .AllowAnyMethod();
      });
  });
  // ...
  app.UseCors("AllowFrontend");
```
- [ ] **JWT Settings:** Ensure the `Jwt:Key`, `Issuer`, and `Audience` are correctly loaded from `appsettings.json` and are identical in logic.

### Frontend (MVC) Configuration
- [ ] **API Base URL:** Do not hardcode the API URL in controllers. Put it in `appsettings.json` of the MVC project.
```json
  "ApiSettings": {
    "BaseUrl": "https://localhost:[BACKEND_PORT]"
  }
```
- [ ] **HTTP Client Factory:** Register `HttpClient` centrally in `Program.cs` of the MVC app so it's injected automatically.
```csharp
  builder.Services.AddHttpClient("ApiClient", client => {
      client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
  });
```

### JWT Handling (Frontend -> Backend)
- [ ] **Storage:** Upon successful `/api/auth/login`, store the returned JWT securely. In an MVC app, storing it in **HTTP-only Cookies** or inside the built-in standard **Session State** is recommended and easier than manual HTML/LocalStorage.
- [ ] **Attaching the Token:** Every time the MVC app uses `HttpClient` to call a protected API endpoint (like POST `/api/products`), it must attach the token.
```csharp
  client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", storedToken);
```

---

## 3. Testing Strategy for Day 5

Divide the testing plan explicitly into these segments:

### A. Backend Unit Tests
* **Focus:** Business logic and Data access validation without needing a live network or UI.
* **Tools:** xUnit + Moq (Optional for Day 5, keep to basics).
* **Target:** Test one core Controller (e.g., `ProductsController`) to see if a valid product creation returns `201 Created`.

### B. Backend Integration Tests (API Testing)
* **Focus:** Verify endpoints are properly secured and connected to the Database.
* **Tools:** Swagger (Interactive) & Postman.
* **Checklist:**
  * Endpoint returns `401 Unauthorized` without token.
  * Login returns valid JWT.
  * Passing JWT creates product in DB (Verify directly in pgAdmin/DBeaver).

### C. Frontend Manual Testing
* **Focus:** View rendering and routing independent of API validation.
* **Checklist:** 
  * Responsive layout using Bootstrap 5.
  * Forms handle empty inputs without crashing (validation triggers).
  * Error banners show up if the backend network is down.

### D. End-to-End (E2E) Integration
* **Focus:** The complete flow bridging both Repositories.
* **Checklist:**
  * Login via Web UI -> Receive JWT from API -> Web UI stores token -> Web UI fetches Products List using token -> Table renders valid API data.

---

## 4. Final Project Demo Flow

Instruct the students to present their project in this exact sequential flow to prove all integration points are successful:

1. **Infrastructure Prep:**
   * Open PostgreSQL (e.g., DBeaver or pgAdmin) to prove the tables are currently empty or show initial state.
2. **Start Backend & Swagger:**
   * Run API (`dotnet run`). Show the Swagger UI to prove the API is alive.
3. **Start Frontend Web:**
   * Run MVC app (`dotnet run`). Open the MVC URL in the browser.
4. **Unauthorized Access Proof (Security Demo):**
   * Attempt to access the "Create Product" page or click a protected button on the frontend before logging in. Prove it restricts access or redirects to login.
5. **Authentication Flow:**
   * Register a new user / Login as Admin via the Frontend UI.
   * Point out the session/cookie being set if requested.
6. **CRUD Operations:**
   * **CREATE:** Add a product in the Frontend. See it appear in the UI list.
   * **DATABASE VERIFICATION:** Switch to the DB viewer, run `SELECT * FROM Products;` and show the exact row there.
   * **EDIT/DELETE:** Update the product in UI, refresh the page, show consistency.

---

## 5. Suggested Minimal Improvements (Before Demo)

Before freezing the code for the presentation, recommend these practical cleanup tasks:

1. **Global Error Handling:** Instead of yellow-screen developer exceptions, ensure the MVC app catches HTTP 500s from the API and displays a clean Bootstrap alert (`"Unable to connect to the server at this time."`).
2. **Clean Configuration:** Eliminate hardcoded connection strings or local ports from the C# code. Move *everything* volatile to `appsettings.Development.json`.
3. **Startup "Double Boot" Script (Optional):** Give them a small `run-both.bat` (Windows) or `run-both.sh` (Mac/Linux) just to impress during the demo.
4. **Logout Functionality:** Ensure there is a mechanism to clear the JWT cookie/session to switch users for the demo.

---

## 6. Deliverables: README Templates

Every final project repo needs a solid README. Use these templates.

### `Backend-API/README.md`
```markdown
# Product Catalog API (Backend)

An ASP.NET Core Web API serving as the backend for the Product Catalog System, built with PostgreSQL and secured by JWT authentication.

## Technologies
- ASP.NET Core 8 Web API
- Entity Framework Core & PostgreSQL
- BCrypt (Password Hashing)
- JWT Bearer Authentication

## How to Run locally
1. Update `appsettings.json` with your PostgreSQL database credentials.
2. In the terminal, run migrations: `dotnet ef database update`
3. Run the API: `dotnet watch` 
4. Navigate to `https://localhost:<port>/swagger` to view endpoints.
```

### `Frontend-Web/README.md`
```markdown
# Product Web (Frontend)

An ASP.NET Core MVC application providing the User Interface for the Product Catalog System. It consumes the Product Catalog API.

## Technologies
- ASP.NET Core 8 MVC
- Razor Pages & Bootstrap 5
- HttpClient (REST consumption)

## Integration Setup
1. Ensure the **Backend API** is running first.
2. Edit `appsettings.json` and set `"ApiSettings:BaseUrl"` to match the Backend API's local URL.
3. Run the Webb App: `dotnet watch`
4. Access the web interface at `https://localhost:<port>`. Note: Register an account or log in to view protected product data.
```
