// ============================================================
//  Program.cs — Entry Point & Konfigurasi ProductWeb MVC
//  Day 4: Frontend Integration
// ============================================================

using ProductWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// ── 1. MVC + Views ────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── 2. HttpClient — Untuk komunikasi ke ProductCatalogAPI ─────────────────────
// AddHttpClient mendaftarkan IHttpClientFactory ke DI container.
// ApiService akan mengambil instance HttpClient dari factory ini.
builder.Services.AddHttpClient();

// ── 3. IHttpContextAccessor — Dibutuhkan ApiService untuk baca Session ────────
builder.Services.AddHttpContextAccessor();

// ── 4. Session — Menyimpan JWT token setelah login ────────────────────────────
// Session disimpan di memory server (in-memory).
// IdleTimeout: session hangus jika tidak ada aktivitas selama 2 jam.
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly    = true;  // Tidak bisa diakses via JavaScript (keamanan XSS)
    options.Cookie.IsEssential = true;  // Cookie tidak memerlukan consent GDPR
});

// ── 5. ApiService — Service wrapper untuk semua call ke API ───────────────────
// Didaftarkan sebagai Scoped: satu instance per HTTP request.
builder.Services.AddScoped<ApiService>();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();
// ─────────────────────────────────────────────────────────────────────────────

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();  // Sajikan file dari wwwroot/ (CSS, JS, gambar)
app.UseRouting();

app.UseSession();      // ⚠️ Session HARUS sebelum UseAuthorization & MapControllerRoute

app.UseAuthorization();

// Route default: /Controller/Action/Id
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");

app.Run();
