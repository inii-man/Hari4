using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductWeb.Controllers;

namespace ProductWeb.Tests.Controllers;

/// <summary>
/// Unit test untuk AuthController.
/// Konvensi: MethodName_Scenario_ExpectedResult
/// Pola: AAA (Arrange - Act - Assert)
/// </summary>
public class AuthControllerTests
{
    // =========================================================================
    //  Helper: Buat AuthController dengan session mock
    // =========================================================================
    private AuthController CreateController(string? existingToken = null)
    {
        var sessionMock = new Mock<ISession>();

        // Setup GetString("JwtToken") → TryGetValue internal
        var tokenBytes = existingToken != null
            ? System.Text.Encoding.UTF8.GetBytes(existingToken)
            : null;

        sessionMock
            .Setup(s => s.TryGetValue("JwtToken", out tokenBytes))
            .Returns(existingToken != null);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);

        var controller = new AuthController(null!);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            httpContextMock.Object,
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
        );

        return controller;
    }

    // =========================================================================
    //  SECTION 1 — LOGIN (GET): Redirect jika sudah login
    // =========================================================================

    [Fact]
    public void Login_WhenAlreadyAuthenticated_RedirectsToProductIndex()
    {
        // Arrange — user sudah punya JWT di session
        var controller = CreateController(existingToken: "existing_valid_jwt");

        // Act
        var result = controller.Login(returnUrl: null);

        // Assert — tidak perlu login lagi, redirect ke produk
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index",   redirect.ActionName);
        Assert.Equal("Product", redirect.ControllerName);
    }

    [Fact]
    public void Login_WhenNotAuthenticated_ReturnsLoginView()
    {
        // Arrange — user belum login (token null)
        var controller = CreateController(existingToken: null);

        // Act
        var result = controller.Login(returnUrl: null);

        // Assert — tampilkan form login
        Assert.IsType<ViewResult>(result);
    }

    // =========================================================================
    //  SECTION 2 — REGISTER (GET): Selalu return view dengan model kosong
    // =========================================================================

    [Fact]
    public void Register_Get_ReturnsViewWithEmptyModel()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Register();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
    }

    // =========================================================================
    //  SECTION 3 — LOGOUT: Session harus dibersihkan
    // =========================================================================

    [Fact]
    public void Logout_Always_RedirectsToLogin()
    {
        // Arrange — user sedang login
        var sessionMock    = new Mock<ISession>();
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);

        var controller = new AuthController(null!);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // Act
        var result = controller.Logout();

        // Assert — session di-clear dan redirect ke login
        sessionMock.Verify(s => s.Clear(), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);
    }
}
