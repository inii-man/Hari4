using System.Text;
using Newtonsoft.Json;
using ProductWeb.Models;

namespace ProductWeb.Services;

/// <summary>
/// Service wrapper untuk semua komunikasi dengan ProductCatalogAPI.
/// Di-inject via DI ke setiap Controller yang membutuhkan data dari API.
/// </summary>
public class ApiService
{
    private readonly HttpClient _client;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiService(IHttpClientFactory factory, IConfiguration config, IHttpContextAccessor httpContextAccessor)
    {
        _client = factory.CreateClient();
        _httpContextAccessor = httpContextAccessor;

        // Membaca profile yang sedang aktif
        var activeProfile = config["ApiSettings:ActiveProfile"] ?? "LocalHttp";
        
        // Membaca URL berdasarkan profile yang aktif
        var baseUrl = config[$"ApiSettings:Profiles:{activeProfile}"] ?? "http://localhost:5289";
        
        _client.BaseAddress = new Uri(baseUrl);
    }

    // ─── Helper: Tambahkan JWT Bearer token dari session ke header ────────────
    private void AttachToken()
    {
        var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
        _client.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrEmpty(token))
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    // ─── AUTH ─────────────────────────────────────────────────────────────────

    // =========================================================================
    //  AUTH
    // =========================================================================

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register", content);
        
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Login ke API dan kembalikan JWT token.
    /// Return null jika credentials salah.
    /// </summary>
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var json    = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);
        if (!response.IsSuccessStatusCode) return null;

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<LoginResponse>(responseJson);
    }

    // ─── PRODUCTS ─────────────────────────────────────────────────────────────

    /// <summary>Ambil semua produk dari API.</summary>
    public async Task<List<Product>> GetProductsAsync()
    {
        AttachToken();
        var response = await _client.GetAsync("/api/products");
        if (!response.IsSuccessStatusCode) return new List<Product>();

        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Product>>(json) ?? new List<Product>();
    }

    /// <summary>Ambil satu produk berdasarkan ID.</summary>
    public async Task<Product?> GetProductAsync(int id)
    {
        AttachToken();
        var response = await _client.GetAsync($"/api/products/{id}");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<Product>(json);
    }

    /// <summary>Tambah produk baru. Return true jika berhasil.</summary>
    public async Task<bool> CreateProductAsync(Product product)
    {
        AttachToken();
        var json    = JsonConvert.SerializeObject(product);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/products", content);
        return response.IsSuccessStatusCode;
    }

    /// <summary>Update produk. Return true jika berhasil.</summary>
    public async Task<bool> UpdateProductAsync(int id, Product product)
    {
        AttachToken();
        var json    = JsonConvert.SerializeObject(product);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"/api/products/{id}", content);
        return response.IsSuccessStatusCode;
    }

    /// <summary>Hapus produk berdasarkan ID. Return true jika berhasil.</summary>
    public async Task<bool> DeleteProductAsync(int id)
    {
        AttachToken();
        var response = await _client.DeleteAsync($"/api/products/{id}");
        return response.IsSuccessStatusCode;
    }
}
