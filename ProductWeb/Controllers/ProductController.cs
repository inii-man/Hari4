using Microsoft.AspNetCore.Mvc;
using ProductWeb.Models;
using ProductWeb.Services;

namespace ProductWeb.Controllers;

/// <summary>
/// Controller utama untuk fitur CRUD produk.
/// Semua action memerlukan JWT yang valid (cek via session).
/// </summary>
public class ProductController : Controller
{
    private readonly ApiService _api;

    public ProductController(ApiService api)
    {
        _api = api;
    }

    // ─── Guard: Cek apakah user sudah login ──────────────────────────────────
    private bool IsAuthenticated()
        => !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

    // ─── Guard: Cek apakah user adalah Admin ─────────────────────────────────
    private bool IsAdmin()
        => HttpContext.Session.GetString("Role") == "Admin";

    private IActionResult RequireLogin()
        => RedirectToAction("Login", "Auth", new { returnUrl = Request.Path });

    private IActionResult AccessDenied()
    {
        TempData["Error"] = "Akses ditolak. Hanya Administrator yang diizinkan.";
        return RedirectToAction("Index");
    }

    // =========================================================================
    //  INDEX — Daftar semua produk + search
    // =========================================================================
    // GET /Product
    public async Task<IActionResult> Index(string? search, string? category, int page = 1)
    {
        if (!IsAuthenticated()) return RequireLogin();

        var products = await _api.GetProductsAsync();

        // Filter pencarian — dilakukan di sisi MVC (bukan di API)
        if (!string.IsNullOrWhiteSpace(search))
            products = products.Where(p =>
                p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(search, StringComparison.OrdinalIgnoreCase)
            ).ToList();

        if (!string.IsNullOrWhiteSpace(category))
            products = products.Where(p =>
                p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
            ).ToList();

        // Kirim data ke ViewBag untuk dropdown kategori dan value search saat ini
        ViewBag.Search   = search;
        ViewBag.Category = category;
        ViewBag.Categories = (await _api.GetProductsAsync())
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        // Pagination logic
        int pageSize = 5;
        int totalItems = products.Count;
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        return View(pagedProducts);
    }

    // =========================================================================
    //  DETAIL — Detail satu produk
    // =========================================================================
    // GET /Product/Detail/5
    public async Task<IActionResult> Detail(int id)
    {
        if (!IsAuthenticated()) return RequireLogin();

        var product = await _api.GetProductAsync(id);
        if (product == null) return NotFound();

        return View(product);
    }

    // =========================================================================
    //  CREATE — Form & submit tambah produk
    // =========================================================================
    // GET /Product/Create
    [HttpGet]
    public IActionResult Create()
    {
        if (!IsAuthenticated()) return RequireLogin();
        if (!IsAdmin()) return AccessDenied();
        return View(new Product());
    }

    // POST /Product/Create
    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        if (!IsAuthenticated()) return RequireLogin();
        if (!IsAdmin()) return AccessDenied();

        if (!ModelState.IsValid) return View(product);

        var success = await _api.CreateProductAsync(product);
        if (!success)
        {
            ModelState.AddModelError("", "Gagal menyimpan produk. Coba lagi.");
            return View(product);
        }

        TempData["Success"] = $"Produk \"{product.Name}\" berhasil ditambahkan!";
        return RedirectToAction("Index");
    }

    // =========================================================================
    //  EDIT — Form & submit ubah produk
    // =========================================================================
    // GET /Product/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsAuthenticated()) return RequireLogin();
        if (!IsAdmin()) return AccessDenied();

        var product = await _api.GetProductAsync(id);
        if (product == null) return NotFound();

        return View(product);
    }

    // POST /Product/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (!IsAuthenticated()) return RequireLogin();
        if (!IsAdmin()) return AccessDenied();

        if (!ModelState.IsValid) return View(product);

        var success = await _api.UpdateProductAsync(id, product);
        if (!success)
        {
            ModelState.AddModelError("", "Gagal mengupdate produk. Coba lagi.");
            return View(product);
        }

        TempData["Success"] = $"Produk \"{product.Name}\" berhasil diupdate!";
        return RedirectToAction("Index");
    }

    // =========================================================================
    //  DELETE — Hapus produk via form POST (HTML tidak support DELETE)
    // =========================================================================
    // POST /Product/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAuthenticated()) return RequireLogin();
        if (!IsAdmin()) return AccessDenied();

        var success = await _api.DeleteProductAsync(id);
        TempData[success ? "Success" : "Error"] = success
            ? "Produk berhasil dihapus."
            : "Gagal menghapus produk.";

        return RedirectToAction("Index");
    }
}
