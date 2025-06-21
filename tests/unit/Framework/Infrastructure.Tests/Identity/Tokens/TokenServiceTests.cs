using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Finbuckle.MultiTenant;
using FSH.Framework.Core.Auth.Jwt;
using FSH.Framework.Core.Identity.Tokens.Features.Generate;
using FSH.Framework.Core.Identity.Tokens.Features.Refresh;
using FSH.Framework.Infrastructure.Identity.Users;
using FSH.Framework.Infrastructure.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Core.Auth.Jwt;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Core.Identity.Tokens.Features.Generate;
using FSH.Framework.Core.Identity.Tokens.Features.Refresh;
using FSH.Framework.Infrastructure.Auth.Jwt;
using FSH.Framework.Infrastructure.Identity.Audit;
using FSH.Framework.Infrastructure.Identity.Tokens;
using FSH.Framework.Infrastructure.Identity.Users;
using FSH.Framework.Infrastructure.Tenant;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace FSH.Framework.Infrastructure.Tests.Identity.Tokens;

/// <summary>
/// Pruebas unitarias para el servicio de generación de tokens JWT.
/// </summary>
public class TokenServiceTests
{
    private readonly Mock<UserManager<FshUser>> _userManagerMock;
    private readonly Mock<IMultiTenantContextAccessor<FshTenantInfo>> _multiTenantContextAccessorMock;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly TokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public TokenServiceTests()
    {
        _userManagerMock = new Mock<UserManager<FshUser>>(
            Mock.Of<IUserStore<FshUser>>(), null, null, null, null, null, null, null, null);

        _multiTenantContextAccessorMock = new Mock<IMultiTenantContextAccessor<FshTenantInfo>>();
        
        // Setup default tenant info for tests
        var tenantInfo = new FshTenantInfo(
            id: "tenant1",
            name: "Test Tenant",
            connectionString: "Server=localhost;Database=testdb;User Id=test;Password=test;",
            adminEmail: "admin@test.com")
        {
            IsActive = true,
            ValidUpto = DateTime.UtcNow.AddMonths(1)
        };
        
        var multiTenantContext = new MultiTenantContext<FshTenantInfo> { TenantInfo = tenantInfo };
        _multiTenantContextAccessorMock.Setup(x => x.MultiTenantContext).Returns(multiTenantContext);
        _publisherMock = new Mock<IPublisher>();
        
        _jwtOptions = new JwtOptions
        {
            Key = "YourSuperSecretKeyForTesting12345678901234567890",
            TokenExpirationInMinutes = 30,
            RefreshTokenExpirationInDays = 7
        };

        var options = Options.Create(_jwtOptions);
        
        _tokenService = new TokenService(
            options,
            _userManagerMock.Object,
            _multiTenantContextAccessorMock.Object,
            _publisherMock.Object);
    }

    [Fact]
    public async Task GenerateTokenAsync_WithValidCredentials_ReturnsTokenResponse()
    {
        // Arrange
        var email = "test@example.com";
        var password = "P@ssw0rd!";
        var ipAddress = "127.0.0.1";
        
        var user = new FshUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser",
            Email = email,
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            EmailConfirmed = true
        };
        
        var request = new TokenGenerationCommand(email, password);
        
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
            
        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, password))
            .ReturnsAsync(true);
            
        // Act
        var result = await _tokenService.GenerateTokenAsync(request, ipAddress, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshTokenExpiryTime.Should().BeAfter(DateTime.UtcNow);
        
        // Verify the token is a valid JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(result.Token);
        
        jwtToken.Should().NotBeNull();
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
    }
    
    [Fact]
    public async Task GenerateTokenAsync_WithInvalidCredentials_ThrowsUnauthorizedException()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "WrongPassword123";
        var ipAddress = "127.0.0.1";
        
        var request = new TokenGenerationCommand(email, password);
        
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((FshUser)null);
            
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _tokenService.GenerateTokenAsync(request, ipAddress, CancellationToken.None));
    }
    
    [Fact]
    public async Task GenerateTokenAsync_WithInactiveUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var email = "inactive@example.com";
        var password = "P@ssw0rd!";
        var ipAddress = "127.0.0.1";
        
        var user = new FshUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "inactiveuser",
            Email = email,
            FirstName = "Inactive",
            LastName = "User",
            IsActive = false, // Inactive user
            EmailConfirmed = true
        };
        
        var request = new TokenGenerationCommand(email, password);
        
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
            
        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, password))
            .ReturnsAsync(true);
            
        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _tokenService.GenerateTokenAsync(request, ipAddress, CancellationToken.None));
            
        exception.Message.Should().Contain("user is deactivated");
    }
    
    [Fact]
    public async Task GenerateTokenAsync_WithUnconfirmedEmail_ThrowsUnauthorizedException()
    {
        // Arrange
        var email = "unconfirmed@example.com";
        var password = "P@ssw0rd!";
        var ipAddress = "127.0.0.1";
        
        var user = new FshUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "unconfirmeduser",
            Email = email,
            FirstName = "Unconfirmed",
            LastName = "User",
            IsActive = true,
            EmailConfirmed = false // Email not confirmed
        };
        
        var request = new TokenGenerationCommand(email, password);
        
        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
            
        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, password))
            .ReturnsAsync(true);
            
        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _tokenService.GenerateTokenAsync(request, ipAddress, CancellationToken.None));
            
        exception.Message.Should().Contain("email not confirmed");
    }

    [Fact]
    public async Task GenerateTokenAsync_WithInactiveTenant_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new FshUser { IsActive = true, EmailConfirmed = true };
        var tenantInfo = new FshTenantInfo { Id = "inactive-tenant", IsActive = false, ValidUpto = DateTime.UtcNow.AddDays(30) };
        SetupMocks(user, tenantInfo, "password", true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _tokenService.GenerateTokenAsync(new TokenGenerationCommand("test@test.com", "password"), "127.0.0.1", CancellationToken.None));
        
        Assert.Contains("is deactivated", exception.Message);
    }

    [Fact]
    public async Task GenerateTokenAsync_WithExpiredTenant_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new FshUser { IsActive = true, EmailConfirmed = true };
        var tenantInfo = new FshTenantInfo { Id = "expired-tenant", IsActive = true, ValidUpto = DateTime.UtcNow.AddDays(-1) };
        SetupMocks(user, tenantInfo, "password", true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _tokenService.GenerateTokenAsync(new TokenGenerationCommand("test@test.com", "password"), "127.0.0.1", CancellationToken.None));
        
        Assert.Contains("validity has expired", exception.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewToken()
    {
        // Arrange
        var user = new FshUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@test.com",
            RefreshToken = "valid-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        var token = GenerateJwtToken(user.Id, user.Email);
        var request = new RefreshTokenCommand(token, "valid-refresh-token");

        // Act
        var result = await _tokenService.RefreshTokenAsync(request, "127.0.0.1", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.RefreshToken);
        Assert.True(result.RefreshTokenExpiryTime > DateTime.UtcNow);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredRefreshToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new FshUser
        {
            Id = Guid.NewGuid().ToString(),
            RefreshToken = "expired-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1) // Expired
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        var token = GenerateJwtToken(user.Id, "test@test.com");
        var request = new RefreshTokenCommand(token, "expired-refresh-token");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _tokenService.RefreshTokenAsync(request, "127.0.0.1", CancellationToken.None));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidRefreshToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new FshUser
        {
            Id = Guid.NewGuid().ToString(),
            RefreshToken = "valid-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        var token = GenerateJwtToken(user.Id, "test@test.com");
        var request = new RefreshTokenCommand(token, "invalid-refresh-token");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _tokenService.RefreshTokenAsync(request, "127.0.0.1", CancellationToken.None));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithNonExistentUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((FshUser)null!);

        var token = GenerateJwtToken(userId, "nonexistent@test.com");
        var request = new RefreshTokenCommand(token, "any-refresh-token");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _tokenService.RefreshTokenAsync(request, "127.0.0.1", CancellationToken.None));
    }

    private void SetupMocks(FshUser user, FshTenantInfo tenantInfo, string password, bool passwordValid)
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, password))
            .ReturnsAsync(passwordValid);

        var multiTenantContext = new MultiTenantContext<FshTenantInfo>
        {
            TenantInfo = tenantInfo
        };

        _multiTenantContextAccessorMock.SetupGet(x => x.MultiTenantContext)
            .Returns(multiTenantContext);

        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<FshUser>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    private string GenerateJwtToken(string userId, string email)
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
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = TestJwtAuthConstants.Issuer,
            Audience = TestJwtAuthConstants.Audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
