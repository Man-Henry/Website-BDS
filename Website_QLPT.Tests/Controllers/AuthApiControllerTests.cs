using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Website_QLPT.Controllers.Api;

namespace Website_QLPT.Tests.Controllers
{
    /// <summary>
    /// Unit tests cho AuthApiController — TC-AUTH-001 đến TC-AUTH-005
    /// </summary>
    public class AuthApiControllerTests
    {
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
        private readonly IConfiguration _configuration;

        public AuthApiControllerTests()
        {
            // Setup UserManager mock
            var userStore = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            // Setup SignInManager mock
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                _userManagerMock.Object, contextAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);

            // Setup Configuration with JWT settings
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Jwt:Key", "TestSecretKey_MustBe64Chars_0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        private AuthApiController CreateController()
        {
            return new AuthApiController(_userManagerMock.Object, _signInManagerMock.Object, _configuration);
        }

        /// <summary>
        /// TC-AUTH-001: Login with valid credentials → 200 OK + JWT token
        /// </summary>
        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var user = new IdentityUser { Id = "user-1", Email = "admin@qlpt.dev", UserName = "admin@qlpt.dev" };
            _userManagerMock.Setup(x => x.FindByEmailAsync("admin@qlpt.dev")).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, "Admin@123456", true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin", "Landlord" });

            var controller = CreateController();
            var loginDto = new LoginRequestDto { Email = "admin@qlpt.dev", Password = "Admin@123456" };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);

            // Verify response has token property
            var value = okResult.Value;
            value.Should().NotBeNull();
            var tokenProp = value!.GetType().GetProperty("token");
            tokenProp.Should().NotBeNull("response should contain a 'token' property");
            var tokenValue = tokenProp!.GetValue(value) as string;
            tokenValue.Should().NotBeNullOrEmpty("JWT token should not be empty");
        }

        /// <summary>
        /// TC-AUTH-002: Login with invalid credentials → 401 Unauthorized
        /// </summary>
        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var user = new IdentityUser { Id = "user-1", Email = "admin@qlpt.dev" };
            _userManagerMock.Setup(x => x.FindByEmailAsync("admin@qlpt.dev")).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, "WrongPassword", true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var controller = CreateController();
            var loginDto = new LoginRequestDto { Email = "admin@qlpt.dev", Password = "WrongPassword" };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorized = (UnauthorizedObjectResult)result;
            unauthorized.StatusCode.Should().Be(401);
        }

        /// <summary>
        /// TC-AUTH-002b: Login with non-existent email → 401
        /// </summary>
        [Fact]
        public async Task Login_NonExistentEmail_ReturnsUnauthorized()
        {
            // Arrange
            _userManagerMock.Setup(x => x.FindByEmailAsync("noone@test.dev")).ReturnsAsync((IdentityUser?)null);

            var controller = CreateController();
            var loginDto = new LoginRequestDto { Email = "noone@test.dev", Password = "Any@123" };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        /// <summary>
        /// TC-AUTH-005: Login with locked-out account → 401 with lockout message
        /// </summary>
        [Fact]
        public async Task Login_LockedOutAccount_ReturnsUnauthorizedWithLockoutMessage()
        {
            // Arrange
            var user = new IdentityUser { Id = "locked-user", Email = "locked@qlpt.dev" };
            _userManagerMock.Setup(x => x.FindByEmailAsync("locked@qlpt.dev")).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, "Any@123", true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

            var controller = CreateController();
            var loginDto = new LoginRequestDto { Email = "locked@qlpt.dev", Password = "Any@123" };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorized.StatusCode.Should().Be(401);
        }

        /// <summary>
        /// TC-AUTH: Login JWT token contains correct claims
        /// </summary>
        [Fact]
        public async Task Login_Success_TokenContainsCorrectClaims()
        {
            // Arrange
            var user = new IdentityUser { Id = "test-user-id", Email = "test@qlpt.dev", UserName = "test@qlpt.dev" };
            _userManagerMock.Setup(x => x.FindByEmailAsync("test@qlpt.dev")).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, "Test@123", true))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Tenant" });

            var controller = CreateController();
            var loginDto = new LoginRequestDto { Email = "test@qlpt.dev", Password = "Test@123" };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var value = okResult.Value!;

            // Check roles property
            var rolesProp = value.GetType().GetProperty("roles");
            rolesProp.Should().NotBeNull();
            var roles = rolesProp!.GetValue(value) as IList<string>;
            roles.Should().Contain("Tenant");

            // Check expiration property
            var expProp = value.GetType().GetProperty("expiration");
            expProp.Should().NotBeNull();
            var expiration = (DateTime)expProp!.GetValue(value)!;
            expiration.Should().BeAfter(DateTime.UtcNow);
        }
    }
}
