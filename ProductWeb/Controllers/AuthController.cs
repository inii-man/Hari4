using Microsoft.AspNetCore.Mvc;
using ProductWeb.Models;
using ProductWeb.Services;

namespace ProductWeb.Controllers;

/// <summary>
/// Menangani alur autentikasi: tampil form login, proses login,
/// simpan JWT ke session, dan logout.
/// </summary>
public class AuthController : Controller
{
    private readonly ApiService _api;

    public AuthController(ApiService api)
    {
        _api = api;
    }

    // GET /Auth/Register — Tampilkan form pendaftaran
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterRequest());
    }

    // POST /Auth/Register — Proses pendaftaran ke API
    [HttpPost]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var success = await _api.RegisterAsync(request);

        if (!success)
        {
            ModelState.AddModelError("", "Pendaftaran Gagal. Mungkin Username sudah terpakai.");
            return View(request);
        }

        TempData["Success"] = "Registrasi Berhasil! Silakan Login menggunakan Akun Anda.";
        return RedirectToAction("Login");
    }

    // GET /Auth/Login — Tampilkan form login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Jika sudah login, langsung redirect ke produk
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            return RedirectToAction("Index", "Product");

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // POST /Auth/Login — Proses login ke API
    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(request);

        var result = await _api.LoginAsync(request);

        if (result == null)
        {
            ModelState.AddModelError("", "Username atau password salah.");
            return View(request);
        }

        // Simpan token dan info user ke session
        HttpContext.Session.SetString("JwtToken", result.Token);
        HttpContext.Session.SetString("Username", result.Username);
        HttpContext.Session.SetString("Role", result.Role);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Product");
    }

    // POST /Auth/Logout — Hapus session dan redirect ke login
    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
