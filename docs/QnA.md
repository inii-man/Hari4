# Q & A: ProductWeb Application

Berikut adalah daftar pertanyaan dan jawaban seputar struktur, arsitektur, dan cara kerja aplikasi **ProductWeb (ASP.NET Core MVC)** beserta API backend-nya.

### 1. Di mana JWT (JSON Web Token) disimpan di frontend?
**Jawaban:**
JWT disimpan di dalam **Session State** (`HttpContext.Session.SetString("JwtToken", result.Token)`). 
Berbeda dengan Single Page Application (SPA / React / Vue) yang sering menyimpannya di *Local Storage*, aplikasi MVC berbasis server menyimpan token ke dalam sesi. Sesi ini dikelola melalui Cookie Session ASP.NET Core secara internal. 
Nantinya, setiap kali *Controller* MVC ingin mengambil atau memanipulasi data dari API, *Service* akan membaca JWT ini dari sesi dan menjadikannya sebagai header `Authorization: Bearer <token>` menggunakan `HttpClient`.

### 2. Bagaimana arsitektur aplikasi ini dibangun?
**Jawaban:**
Aplikasi dipisahkan menjadi dua bagian (Client-Server Architecture):
- **Backend (Web API):** Bertugas menangani operasi ke Database, enkripsi, dan Authentication. Outputnya hanya berupa data JSON. (Tanpa UI).
- **Frontend (Web MVC):** Bertugas hanya untuk merender tampilan UI dengan *Razor View*. Aplikasi MVC mengambil data dari Backend API menggunakan protokol HTTP (`HttpClient`) untuk kemudian merendernya dalam bentuk tabel, daftar, dll menggunakan HTML & Bootstrap.

### 3. Bagaimana MVC berkomunikasi dengan Web API?
**Jawaban:**
Interaksi dilakukan menggunakan **HttpClient** yang dibungkus dalam *Class Service* khusus (yaitu `ApiService.cs`). 
Contohnya, saat MVC Controller menerima input formulir tambah produk, Controller tidak menyimpan langsung ke database. Akan tetapi, Controller mengkonversi objek `Product` menjadi format JSON lalu mengirim HTTP request dengan metode **POST** ke Web API. Jika API merespons dengan eksekusi berhasil (`2xx Success`), maka MVC akan me-redirect pengguna kembali ke tampilan daftar produk.

### 4. Apa library UI / Frontend framework yang dipakai di sini?
**Jawaban:**
Aplikasi ini hanya menggunakan file *CSS* dan *Javascript* **Bootstrap 5** melalui CDN yang disisipkan ke dalam layout Master (`_Layout.cshtml`). Menggunakan Grid System bawaan Bootstrap, tabel, dan komponen bawaannya membuat tampilan sudah seketika reponsif di perangkat *desktop* maupun *mobile* tanpa membutuhkan framework JS mutakhir terpisah seperti React atau Angular.

### 5. Apa yang terjadi ketika pengguna menekan tombol Logout (Keluar)?
**Jawaban:**
*Controller* Autentikasi (`AuthController`) akan memanggil fungsi `HttpContext.Session.Clear()`. Hal ini akan langsung menghancurkan / membersihkan seluruh **key-value** di dalam memory session server milik pengguna (termasuk memori JWT Token, Username, dan Role), sehingga pengguna akan kehilangan otorisasi untuk menarik halaman terproteksi di request-nya nanti, lalu pengguna dikembalikan ke halaman *Login*.

### 6. Bagaimana cara *Role-Based Access Control* (RBAC) ditangani di sisi MVC Frontend?
**Jawaban:**
Informasi identitas *Role* pengguna (misal: "Admin" atau "User") ikut disimpan di *Session* murni pada saat login sukses. Frontend di Razor View (`.cshtml`) dapat dengan leluasa membaca status ini dan menggunakan kondisional logika `if` sederhana di blok C# (`@if (Context.Session.GetString("Role") == "Admin") { ... }`) untuk merender dan merangkai halaman secara dinamis -- seperti menyembunyikan atau memunculkan tombol tertentu (contoh: tombol *Create*, *Edit*, dan *Delete*).
Tentu saja secara fisik, *Backend Web API* juga memvalidasi ketat Role yang tertanam secara kriptografis di dalam *Payload Token*.

### 7. Mengapa `HttpClient` didaftarkan di dalam `Program.cs` (`AddHttpClient()`), dan kenapa tidak dilakukan dengan instance `new HttpClient()` saja secara manual?
**Jawaban:**
Pembuatan instance `new HttpClient()` manual di setiap method memakan memory (sering disebut sebagai isu *socket exhaustion*). Dengan mendaftarkan `builder.Services.AddHttpClient()`, ASP.NET Core MVC menggunakan mekanisme yang disebut **IHttpClientFactory**. Pabrik ini mengontrol *pool* dari koneksi-koneksi HTTP di balik layar, mengolahnya agar koneksi lama dipakai kembali / *reusable*, sehingga pemanggilan ke API eksternal jauh lebih cepat dan stabil untuk *request-request* skala besar.

### 8. Bagaimana cara `ApiService` tahu harus menyisipkan *Token JWT* di setiap lemparan reques?
**Jawaban:**
Di dalam `ApiService`, terdapat fitur _Helper Method_ bernama `AttachToken()`. Sebelum pemanggilan ke API (misal pada `GetProductsAsync` atau `CreateProductAsync`), method ini pertama kali dipanggil untuk melacak JWT pada keranjang *Session*. 
Fungsi tersebut memakai `_httpContextAccessor.HttpContext?.Session.GetString("JwtToken")` untuk menyalin token dan menyuntikkannya ke `DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token)`. Secara spesifik hal itu membuat *HttpClient API* mengirimkan identitas login kita di Headers di belakang layar.

### 9. Bagaimana aplikasi memilah link pemanggilan Backend-nya? Apakah tersimpan di `Program.cs`?
**Jawaban:**
Link URL Web API tidak di-"hardcode" atau ditanam langsung di *source code*, melainkan di-inject melalui file konfigurasi `appsettings.json`.
Saat Class `ApiService` di-instansiasi (*injection* lewat Konstruktor), framework juga menyertakan `IConfiguration config`. Code selanjutnya tinggal membaca propertinya layaknya config dictionary:
`var baseUrl = config[$"ApiSettings:Profiles:{activeProfile}"]` 
Sehingga ketika pindah server/hosting, developer bebas mengganti URL API pada file `appsettings.json` tanpa perlu *re-compile* *source code* C#.

### 10. Mengapa registrasi User di MVC juga perlu dilempar ke Backend, lalu menunggu Return Bool (True/False)?
**Jawaban:**
Web MVC hanya sebatas sebuah "Layar Client" dan tidak memiliki akses _koneksi-string_ langsung ke dalam Database (SQL Server). Seluruh prosedur penentuan kelayakan email/username dan proses Hashing Password dikerjakan 100% oleh ranah Back-End (ProductCatalogAPI). MVC hanya menyodorkan input String Mentah (Username, Email, Role, Password). Jika validasinya di Backend API berhasil dan *row* tertanam di DB, API meresponse sukses, baru kemudian MVC akan memandu halaman pindah ke rute *Login*.

### 11. Bagaimana cara mengamankan Cookie agar menjadi *HTTP-Only*?
**Jawaban:**
*HTTP-Only* artinya Cookie tidak bisa dibaca, disentuh, maupun dirampas oleh skrip *Javascript* di *browser* pengguna (sangat manjur untuk mencegah serangan XSS atau *Cross-Site Scripting*). 
Jika kamu perhatikan di aplikasi MVC kita (`Program.cs`), fitur Session yang kita daftarkan sudah dikonfigurasi seperti ini:
```csharp
options.Cookie.HttpOnly = true;
```
Itu berarti ID Cookie dari Memory Sesi kita dijamin aman disembunyikan dari manipulasi *Javascript*. Jika kita ingin bermain dengan *cookie* mentah secara manual di Controller, polanya juga sama: `Response.Cookies.Append("NamaKey", "Value", new CookieOptions { HttpOnly = true });`.

### 12. Di Aplikasi Web, bagaimana cara menyimpan data ke *Local Storage* dan *Session Storage*?
**Jawaban:**
Karena aplikasi kita adalah *Server-Side Rendering* (MVC C#), kita lebih mengandalkan *Session* di Server. 
Tetapi, jika kamu sedang membangun spesifik *Frontend Javascript Framework / SPA* (atau butuh fitur interaktif JS murni di MVC), kamu bisa menyimpannya menggunakan script UI. *Local Storage* bertahan permanen di browser walau *tab*-nya di-close, sedangkan *Session Storage* akan hilang saat tab di-close.
*Cara simpan menggunakan Javascript:*
```javascript
// Simpan di Local Storage (Permanen)
localStorage.setItem('TemaAplikasi', 'Dark-Mode');

// Mengambilnya
var token = localStorage.getItem('TemaAplikasi');

// Simpan di Session Storage (Sementara 1 Tab)
sessionStorage.setItem('DraftKeranjang', 'Baju Koko');
```

### 13. Bagaimana cara melakukan Debugging di Frontend (FE) maupun Backend (BE)?
**Jawaban:**
Mengingat aplikasi kita bercabang layaknya *dua negara yang berbeda*, cara mencari kesalahan (*bugs*)-nya pun terpisah:
- **DEBUGGING BACKEND (API C#):**
  - **Swagger UI:** Merupakan fasilitas terbaik untuk mengetes API kita sebelum MVC menyentuhnya. Jika di-run (`dotnet run` atau jalankan project di Visual Studio), kamu mendapatkan UI Swagger untuk mengetes Endpoint (Post, Get) satu demi satu dan memastikan response JSON-nya benar.
  - **Breakpoints (Titik Merah):** Pasang breakpoint di baris kodemu dalam Visual Studio / VS Code, lalu jalankan dalam mode *Debug / Start Debugging (F5)*. Coba panggil Endpoint dari Postman atau MVC, eksekusi akan berhenti di kode backend-mu dan bisa di-inspeksi baris per baris (*Step Over*).
- **DEBUGGING FRONTEND (MVC C# & BROWSER):**
  - **Breakpoints MVC:** Kamu juga tetap bisa menaruh *breakpoint* di `Controllers` MVC untuk melacak apakah *Form Action* menangkap data input dengan benar.
  - **Browser Developer Tools (F12):** Ini senjata wajib untuk Frontend (*HTML/CSS/JS*):
    - **Tab "Console":** Untuk melihat error *Javascript* lokal.
    - **Tab "Network":** Ini wajib dipantau saat terjadi kegagalan pemanggilan HTTP. Kamu bisa melihat persis apa Request Header/Body yang dikirim Frontend, dan JSON Response apa yang dipantulkan Backend *(kode 404, 500, atau 401)*.
    - **Tab "Elements":** Untuk melakukan *inspect* Bootstrap CSS jika *styling* / tampilannya berantakan.

---

> [!TIP]
> **Praktik Terbaik dalam Arsitektur:**
> Arsitektur yang memisahkan **Tampilan (MVC/Frontend)** dan **Dapur Data (API/Backend)** ini memungkinkan tim dapat menciptakan **Frontend Alternatif** sewaktu-waktu. (Misal di masa mendatang akan dibuatkan Sistem Web SPA dengan *Vue.js*, atau dirancangkan *Mobile App iOS*). Keduanya dapat langsung "mengonsumsi" REST API ProductCatalogAPI dengan cara identik, karena inti *bisnis-logika* dan *security JWT* di-sentralisasi pada 1 backend tunggal.
