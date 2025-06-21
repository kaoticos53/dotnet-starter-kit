using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Finbuckle.MultiTenant;
using FSH.Framework.Core.Auth.Jwt;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Core.Identity.Tokens.Features.Generate;
using FSH.Framework.Core.Identity.Tokens.Features.Refresh;
using FSH.Framework.Infrastructure.Identity.Tokens;
using FSH.Framework.Infrastructure.Identity.Users;
using FSH.Framework.Infrastructure.Tenant;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace FSH.Framework.Infrastructure.Tests.Identity.Tokens;

/// <summary>
/// Pruebas para el servicio de generación y refresco de tokens JWT
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Infrastructure")]
[Trait("Component", "Identity")]
public class TokenServiceTests
{
    private readonly ITestOutputHelper _output;

    public TokenServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }
    private readonly Mock<UserManager<FshUser>> _userManagerMock;
    private readonly Mock<IMultiTenantContextAccessor<FshTenantInfo>> _multiTenantContextAccessorMock;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly TokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public TokenServiceTests()
    {
        _userManagerMock = new Mock<UserManager<FshUser>>(
            Mock.Of<IUserStore<FshUser>>(),
            null, null, null, null, null, null, null, null);

        _multiTenantContextAccessorMock = new Mock<IMultiTenantContextAccessor<FshTenantInfo>>();
        _publisherMock = new Mock<IPublisher>();

        _jwtOptions = new JwtOptions
        {
            Key = "ThisIsASecretKeyForTestingPurposes1234567890",
            TokenExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        };

        var jwtOptionsMock = new Mock<IOptions<JwtOptions>>();
        jwtOptionsMock.Setup(x => x.Value).Returns(_jwtOptions);

        var tenantInfo = new FshTenantInfo
        {
            Id = "test-tenant",
            IsActive = true,
            ValidUpto = DateTime.UtcNow.AddYears(1)
        };

        var multiTenantContext = new MultiTenantContext<FshTenantInfo> { TenantInfo = tenantInfo };
        _multiTenantContextAccessorMock.Setup(x => x.MultiTenantContext).Returns(multiTenantContext);

        _tokenService = new TokenService(
            jwtOptionsMock.Object,
            _userManagerMock.Object,
            _multiTenantContextAccessorMock.Object,
            _publisherMock.Object);
    }


    [Theory]
    [InlineData("test@test.com", "Test", "User", true)]
    [InlineData("admin@admin.com", "Admin", "User", true)]
    [Trait("Feature", "TokenGeneration")]
    public async Task GenerateTokenAsync_WithValidCredentials_ReturnsTokenResponse(
        string email, string firstName, string lastName, bool isActive)
    {
        // Arrange
        _output.WriteLine($"Probando generación de token para: {email}");
        
        var user = new FshUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            IsActive = isActive,
            EmailConfirmed = true
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(true);

        var request = new TokenGenerationCommand("test@test.com", "password");
        var ipAddress = "127.0.0.1";

        // Act
        var result = await _tokenService.GenerateTokenAsync(request, ipAddress, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.RefreshToken);
        Assert.True(result.RefreshTokenExpiryTime > DateTime.UtcNow);
    }


    [Fact]
    [Trait("Feature", "TokenGeneration")]
    [Trait("Scenario", "InactiveUser")]
    public async Task GenerateTokenAsync_WithInactiveUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new FshUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@test.com",
            Email = "test@test.com",
            IsActive = false,
            EmailConfirmed = true
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(true);

        var request = new TokenGenerationCommand("test@test.com", "password");
        var ipAddress = "127.0.0.1";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _tokenService.GenerateTokenAsync(request, ipAddress, CancellationToken.None));
    }

    [Fact]
    [Trait("Feature", "TokenGeneration")]
    [Trait("Scenario", "UnconfirmedEmail")]
    public async Task GenerateTokenAsync_WithUnconfirmedEmail_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new FshUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@test.com",
            Email = "test@test.com",
            IsActive = true,
            EmailConfirmed = false
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(true);

        var request = new TokenGenerationCommand("test@test.com", "password");
        var ipAddress = "127.0.0.1";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _tokenService.GenerateTokenAsync(request, ipAddress, CancellationToken.None));
        Assert.Equal("email not confirmed", exception.Message);
    }

    [Fact]
    [Trait("Feature", "TokenRefresh")]
    [Trait("Scenario", "ValidToken")]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokenResponse()
    {
        // Arrange
        var user = new FshUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@test.com",
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            EmailConfirmed = true,
            RefreshToken = "valid-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        var token = GenerateTestToken(user.Id, user.Email);
        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        var request = new RefreshTokenCommand(token, user.RefreshToken);
        var ipAddress = "127.0.0.1";

        // Act
        var result = await _tokenService.RefreshTokenAsync(request, ipAddress, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.RefreshToken);
        Assert.True(result.RefreshTokenExpiryTime > DateTime.UtcNow);
    }

    [Fact]
    [Trait("Feature", "TokenRefresh")]
    [Trait("Scenario", "ExpiredToken")]
    public async Task RefreshTokenAsync_WithExpiredRefreshToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new FshUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@test.com",
            Email = "test@test.com",
            IsActive = true,
            EmailConfirmed = true,
            RefreshToken = "expired-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1) // Expired
        };

        var token = GenerateTestToken(user.Id, user.Email);
        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        var request = new RefreshTokenCommand(token, user.RefreshToken);
        var ipAddress = "127.0.0.1";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _tokenService.RefreshTokenAsync(request, ipAddress, CancellationToken.None));
        Assert.Equal("Invalid Refresh Token", exception.Message);
    }

    [Theory]
    [InlineData("invalid-token")]
    [InlineData("")]
    [InlineData(null)]
    public async Task RefreshTokenAsync_WithInvalidRefreshToken_ThrowsUnauthorizedException(string invalidToken)
    {
        // Arrange
        _output.WriteLine($"Probando token de refresco inválido: {invalidToken ?? "null"}");
        
        var user = new FshUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@test.com",
            Email = "test@test.com",
            IsActive = true,
            EmailConfirmed = true,
            RefreshToken = "valid-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        var token = GenerateTestToken(user.Id, user.Email);
        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        var request = new RefreshTokenCommand(token, invalidToken);
        var ipAddress = "127.0.0.1";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _tokenService.RefreshTokenAsync(request, ipAddress, CancellationToken.None));
            
        _output.WriteLine($"Excepción esperada recibida: {exception.Message}");
        Assert.Equal("Invalid Refresh Token", exception.Message);
    }

    private string GenerateTestToken(string userId, string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtOptions.Key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email)
            }),
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = JwtAuthConstants.Issuer,
            Audience = JwtAuthConstants.Audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
