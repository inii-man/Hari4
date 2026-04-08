# 📖 Evaluasi Line-by-Line Kode (ProductWeb MVC)

Dokumentasi di bawah ini berisi ulasan fungsional dan penjelasan per-baris dari file controller/service yang menjadi struktur fundamental di project ASP.NET Core MVC (Hari 4).

---

## 🚀 1. `Program.cs` - (Setup Entry Point)

File ini mendaftarkan layanan Dependecy Injection (DI) untuk keperluan aplikasi sebelum di-*Build*.

```csharp
// Menginstalasikan service komponen Controller berserta Views (Razor html) agar MVC bisa berjalan.
builder.Services.AddControllersWithViews();

// Mendaftarkan service IHttpClientFactory. Agar API Service bisa meminta ("meminjam") 
// obyek HttpClient untuk melakukan web request ke remote server (API hari 3).
builder.Services.AddHttpClient();

// Membuka jalan agar logika service atau controller bisa mengakses obyek "HttpContext" dari user.
// Kegunaan utama: Memungkinkan kita mengakses Session Storage memori.
builder.Services.AddHttpContextAccessor();

// Mendaftarkan layanan Cookie-Based Memory Session.
// Kita memerlukannya untuk menyimpan String JWT Auth Token agar ia persisten selagi user belum logout.
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(2); // Mati dalam 2 jam kalau tak ada aktifitas.
    options.Cookie.HttpOnly    = true; // Mencegah resiko pencurian token melalui Ekstensi / Javascript asing XSS.
    options.Cookie.IsEssential = true;
});

// Daftarkan ApiService sebagai layanan "Scoped" (dibuat baru tiap 1 kali user mengakses web).
builder.Services.AddScoped<ApiService>();

// --- APP PIPELINE MIDDLEWARE ---

app.UseHttpsRedirection(); // Paksa pengguna ke koneksi HTTPS yang aman.
app.UseStaticFiles();      // Ijinkan website memberikan respon file static berekstensi (css, js, gambar) di '/wwwroot'.
app.UseRouting();          // Mengarahkan alamat browser (URL) pada action controller terkait.
app.UseSession();          // WARNING: HARUS diletakkan sebelum Authorization. Menyalakan fitur baca-tulis sesi.
app.UseAuthorization();    // Verifikasi Auth.

// Aturan default rute: jika URL kosong (domain utama), arahkan controller ke Product, action Index.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");
```

---

## 🛠 2. `Services/ApiService.cs` - (Pusat Lalu Lintas Data)

Fungsi modul ini adalah mengurus **semua* operasional transfer data (Serialization JSON -> Panggil HTTP -> Deserialization kembalian). Menjaga Controller tidak kotor dengan baris kode HTTP.

```csharp
public class ApiService
{
    private readonly HttpClient _client;
    // Mengakses session state saat request API berjalan (untuk mengekstrak jwt token)
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiService(IHttpClientFactory factory, IConfiguration config, IHttpContextAccessor httpContextAccessor)
    {
        // Factory mendirikan instance client baru
        _client = factory.CreateClient();
        _httpContextAccessor = httpContextAccessor;

        // Baca BaseUrl target (API Hari 3) dari appsettings.json.
        var baseUrl = config["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
        // Jadikan itu URL awalan default agar tiap endpoint panggil cukup path sisa-nya aja (i.e "/api/products")
        _client.BaseAddress = new Uri(baseUrl);
    }

    // Fungsi Internal Pembantu (Helpers): Menyisipkan Header 'Bearer {Token}' 
    private void AttachToken()
    {
        // Ekstrak token dari dalam session brankas pengguna
        var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
        
        // Riset Header Token lama (jaga-jaga jika kadaluarsa)
        _client.DefaultRequestHeaders.Authorization = null;
        
        // Pengecekan Eksistensi
        if (!string.IsNullOrEmpty(token))
        {
            // Jika token ada, set Authentication HTTP header sebagai Bearer JSON Web Token.
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    // --- Contoh Pengambilan Produk ---
    public async Task<List<Product>> GetProductsAsync()
    {
        AttachToken(); // Siapkan kunci JWT Authnya di Header HTTP ini.
        // Melakukan Fetch GET 
        var response = await _client.GetAsync("/api/products");
        // Mengecek validitas Status HTTP (contoh: return false kalau ternyata 401 Unauthorized/404)
        if (!response.IsSuccessStatusCode) return new List<Product>();

        // Merubah string JSON kembalian dari API Backend menjadi entitas list class produk milik MVC Backend (Deserelisasi)
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Product>>(json) ?? new List<Product>();
    }
    
    // --- Logika Post & Put, dst mirip flow-nya dengan yg diatas, tapi direversal, Class MVC di serialisasi menjadi JSON terlebih dulu dan disematkan stringContent application/json HTTP. ---
}
```

---

## 🔐 3. `Controllers/AuthController.cs` - (Logika Portal Gerbang Masuk)

Manajer Formulario User - Menerima Input Model dari tampilan Login view dan memanggil layanan authentikasi.

```csharp
    // Method POST, menerima lemparan Model dari user yg klik button Submit HTML
    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null)
    {
        // Memeriksa persyaratan Validasi Input Form (Apakah username kosong? Apakah pass kosong?)
        if (!ModelState.IsValid)
            return View(request);

        // Minta ApiService untuk post login request ke Backend Web API. 
        var result = await _api.LoginAsync(request);

        // API Mengembalikan hasil null mengindikasikan Token gagal di issue karena misal password salah
        if (result == null)
        {
            // Tambah pesan error ke komponen form View HTML.
            ModelState.AddModelError("", "Username atau password salah.");
            return View(request); 
        }

        // --- Auth Sukses ---
        // Menempatkan properti properti penting Token JWT, nama user, beserta hak akses role ke Session storage MVC
        HttpContext.Session.SetString("JwtToken", result.Token);
        HttpContext.Session.SetString("Username", result.Username);
        HttpContext.Session.SetString("Role", result.Role);

        // Melakukan Redirect User ke Halaman produk (List katalog)
        return RedirectToAction("Index", "Product");
    }
```

---

## 🗃 4. `Controllers/ProductController.cs` - (Menyusun Halaman MVC & Bisnis Proses)

Mengendalikan flow lalu lintas tiap halaman dan bertugas menginfeksi *View Data*.

```csharp
    // Controller Pengecek Sesi (Memastikan tidak ada orang tanpa JWT berani masuk halaman ini)
    private bool IsAuthenticated()
        => !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

    // --- FITUR INDEX READ BESERTA SEARCH DAN PAGINATION ---
    public async Task<IActionResult> Index(string? search, string? category, int page = 1)
    {
        // 1. Guard Statement penolak akses gelap
        if (!IsAuthenticated()) return RequireLogin();

        // 2. Akses API Fetch Data Utuh
        var products = await _api.GetProductsAsync();

        // 3. Filter LINQ (Language Integrated Query) berdasarkan kriteria text search box MVC  
        // Membandingkan text tak sensitif (StringComparison.OrdinalIgnoreCase) pada properti Nama, Deskripsi dan kategori 
        if (!string.IsNullOrWhiteSpace(search))
            products = products.Where(p =>
                p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(search, StringComparison.OrdinalIgnoreCase)
            ).ToList();

        // Filter exact-match untuk dropdown Category Filter
        if (!string.IsNullOrWhiteSpace(category))
            products = products.Where(p =>
                p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
            ).ToList();

        // 4. Mempersiapkan variable View Bag (Kondisi statik di luar model tabel) agar View (Html) bisa menjaga memori search yg lagi diketik user.
        ViewBag.Search   = search;
        ViewBag.Category = category;
        // Query untuk mensimulasikan Array List Dropdown Category Unik.
        ViewBag.Categories = (await _api.GetProductsAsync()).Select(p => p.Category).Distinct().OrderBy(c => c).ToList();

        // 5. PENERAPAN PAGINATION ALGORITMA
        int pageSize = 5; // Kita tentukan maximal list per halaman adalah 5 object list tabel
        int totalItems = products.Count; // Total baris item saat itu (bisa berubah drastis imbas text field search filter sebelum step ini)
        
        // Membagi integer matematika. Di pembulatkan keatas: contoh "Items ada 12 dibagi 5 hasilnya = 3 Pages"
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        
        // Menjaga agar user via tab url tidak mengisikan ID page iseng minus (-1, dst) atau melewati batas max
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        // SKIP DAN TAKE: Teknik inti melintasi array.
        // Jika current page adalah ke "2" -> (2 - 1) * 5 = skip(5) -> lompati 5 item terawal, lalu Take() / ambil 5 sisa berikutnya (menampilkan page-2 data dari list).
        var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        // Me-lempar nilai info untuk UI Numbering Button Pagination pada file View Razor (.cshtml)
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        // View di-render bersama list data produk page saat ini
        return View(pagedProducts);
    }
```
