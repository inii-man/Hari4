using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductWeb.Controllers;

namespace ProductWeb.Tests.Controllers;

/// <summary>
/// Unit test untuk ProductController.
/// Mengikuti konvensi penamaan: MethodName_Scenario_ExpectedResult
/// Mengikuti pola AAA (Arrange - Act - Assert)
/// </summary>
public class ProductControllerTests
{
    // =========================================================================
    //  Helper: Buat controller dengan session JWT (atau null = belum login)
    // =========================================================================
    private ProductController CreateControllerWithSession(string? jwtToken, string? role = null)
    {
        // Arrange: Mock Session
        var sessionMock = new Mock<ISession>();
        var tokenBytes = jwtToken != null ? System.Text.Encoding.UTF8.GetBytes(jwtToken) : null;
        var roleBytes  = role    != null ? System.Text.Encoding.UTF8.GetBytes(role)     : null;

        sessionMock.Setup(s => s.TryGetValue("JwtToken", out tokenBytes))
                   .Returns(jwtToken != null);
        sessionMock.Setup(s => s.TryGetValue("Role", out roleBytes))
                   .Returns(role != null);

        // Mock Request
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(r => r.Path).Returns("/Product");

        // Mock HttpContext
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);
        httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);

        var controller = new ProductController(null!);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // TempData (dibutuhkan oleh AccessDenied dan beberapa action)
        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            httpContextMock.Object,
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
        );

        return controller;
    }

    // =========================================================================
    //  SECTION 1 — INDEX: Proteksi Login
    // =========================================================================

    [Fact]
    public async Task Index_WithoutJwtToken_RedirectsToLogin()
    {
        // Arrange
        var controller = CreateControllerWithSession(null);

        // Act
        var result = await controller.Index(null, null);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Auth", redirectResult.ControllerName);
    }

    // =========================================================================
    //  SECTION 2 — CREATE (GET): Role-Based Access Control
    // =========================================================================

    [Fact]
    public void Create_WithoutAdminRole_RedirectsToIndexWithError()
    {
        // Arrange
        var controller = CreateControllerWithSession("valid_token", role: "User"); // Role bukan Admin

        // Act
        var result = controller.Create();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Akses ditolak. Hanya Administrator yang diizinkan.", controller.TempData["Error"]);
    }

    [Fact]
    public void Create_WithAdminRole_ReturnsViewResult()
    {
        // Arrange
        var controller = CreateControllerWithSession("valid_token", role: "Admin");

        // Act
        var result = controller.Create();

        // Assert — Admin bisa mengakses form Create
        Assert.IsType<ViewResult>(result);
    }

    // =========================================================================
    //  SECTION 3 — EDIT (GET): Proteksi tanpa JWT
    // =========================================================================

    [Fact]
    public async Task Edit_WithoutJwtToken_RedirectsToLogin()
    {
        // Arrange
        var controller = CreateControllerWithSession(null);

        // Act
        var result = await controller.Edit(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Auth", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Edit_WithUserRole_RedirectsToIndexWithError()
    {
        // Arrange — User biasa tidak boleh edit
        var controller = CreateControllerWithSession("valid_token", role: "User");

        // Act
        var result = await controller.Edit(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Akses ditolak. Hanya Administrator yang diizinkan.", controller.TempData["Error"]);
    }

    // =========================================================================
    //  SECTION 4 — DELETE (POST): Proteksi Role dan Session
    // =========================================================================

    [Fact]
    public async Task Delete_WithoutJwtToken_RedirectsToLogin()
    {
        // Arrange
        var controller = CreateControllerWithSession(null);

        // Act
        var result = await controller.Delete(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Auth", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Delete_WithUserRole_RedirectsToIndexWithError()
    {
        // Arrange — Role User tidak punya akses hapus
        var controller = CreateControllerWithSession("valid_token", role: "User");

        // Act
        var result = await controller.Delete(99);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Akses ditolak. Hanya Administrator yang diizinkan.", controller.TempData["Error"]);
    }

    // =========================================================================
    //  SECTION 5 — DETAIL: Proteksi JWT
    // =========================================================================

    [Fact]
    public async Task Detail_WithoutJwtToken_RedirectsToLogin()
    {
        // Arrange
        var controller = CreateControllerWithSession(null);

        // Act
        var result = await controller.Detail(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Auth", redirectResult.ControllerName);
    }
}
