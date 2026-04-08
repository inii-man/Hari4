# 📚 Product Catalog Web App — ASP.NET Core MVC

Aplikasi frontend ini dibuat menggunakan **ASP.NET Core MVC** untuk mengakses data dari backend `ProductCatalogAPI` yang dirancang di materi Hari 3. Aplikasi menggunakan Framework CSS Bootstrap 5 dengan desain moderen dan *glassmorphism layout*.

## 🌟 Fitur Aplikasi

1. **Session & JWT Auth**: Menggunakan JSON Web Token yang dipanggil lewat endpoint `/api/auth/login` yang kemudian tokennya di simpan dalam *memory Session* browser dan otomatis dimasukkan ke header setiap request API berikutnya.
2. **Read Products (Pagination & Search)**: Menampilkan list produk dengan layout rapi dari memanggil endpoint GET API dengan batasan data (*pagination*) dan sistem form pencarian (berdasarkan teks dan kategori dropdown).
3. **Detail & Inventory Check**: Menampilkan ringkasan status kelangkaan produk (Tersedia, Stok Rendah, Habis) secara visual menggunakan Progress bar.
4. **Create / Edit / Delete Product**: Fungsi operasional Modifikasi produk berdasar CRUD flow yang melemparkan Payload data ke Endpoint Web API yang memicu EF Core menanamnya di PostgreSQL database.

---

## 🛠 Panduan Instalasi dan Menjalankan (Installation & Usage)

Sebelum mengeksekusi aplikasi Web Frontend ini, **pastikan Backend Anda sudah berjalan**!

### Prasyarat (Prerequisites)
1. [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download) telah terpasang.
2. Project `ProductCatalogAPI` (Day 3) yang terhubung ke PostgreSQL sudah dijalankan dan "Listen" (biasanya) di alamat localhost `https://localhost:7001` (atau port 5001).

### Langkah Menjalankan Frontend

1. Buka Terminal atau Command Prompt di OS anda.
2. Navigasikan (cd) ke path folder `ProductWeb` ini (*`NQA DOTNET DJP/Hari4/ProductWeb`*).
   ```bash
   cd "Hari4/ProductWeb"
   ```
3. Modifikasi konfigurasi `appsettings.json`. Arsitektur proyek kami men-support multiple-environment. Ganti *"ActiveProfile"* ke salah satu profile yang sesuai dengan port/lokasi Web API target anda.
   ```json
  "ApiSettings": {
    "ActiveProfile": "LocalHttp",
    "Profiles": {
      "LocalHttp": "http://localhost:5289",
      "LocalHttps": "https://localhost:7001",
      "Production": "https://api.produkkatalog.com"
    }
  }
   ```
4. Build aplikasi untuk memastikan tidak ada kesalahan kompilasi:
   ```bash
   dotnet build
   ```
5. Jalankan aplikasi web:
   ```bash
   dotnet run
   ```
6. Buka aplikasi web di URL yang tersaji di log command (umumnya `https://localhost:5033` atau `https://localhost:71xx` dsb).
7. Aplikasi ini membutuhkan Anda untuk `Login` dengan akun yang ter-registrasi (bisa akun admin / user) di Database Hari ke 3. Setelah login, seluruh menu modifikasi siap dipakai.
