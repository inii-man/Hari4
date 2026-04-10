using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductWeb.Controllers;

namespace ProductWeb.Tests.Controllers;

public class ProductControllerTests
{
    private ProductController CreateControllerWithSession(string? jwtToken)
    {
        // Mock Session
        var sessionMock = new Mock<ISession>();
        var tokenBytes = jwtToken != null ? System.Text.Encoding.UTF8.GetBytes(jwtToken) : null;
        
        sessionMock.Setup(s => s.TryGetValue("JwtToken", out tokenBytes))
                   .Returns(jwtToken != null);

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

        return controller;
    }

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

    [Fact]
    public void CreateGet_WithoutAdminRole_RedirectsToIndexAccessDenied()
    {
        // Arrange
        var sessionMock = new Mock<ISession>();
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes("fake_token");
        var roleBytes = System.Text.Encoding.UTF8.GetBytes("User"); // Not Admin

        sessionMock.Setup(s => s.TryGetValue("JwtToken", out tokenBytes)).Returns(true);
        sessionMock.Setup(s => s.TryGetValue("Role", out roleBytes)).Returns(true);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Session).Returns(sessionMock.Object);

        var controller = new ProductController(null!);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContextMock.Object };
        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            httpContextMock.Object, 
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
        );

        // Act
        var result = controller.Create();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Akses ditolak. Hanya Administrator yang diizinkan.", controller.TempData["Error"]);
    }
}
