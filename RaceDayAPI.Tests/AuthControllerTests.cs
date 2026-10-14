
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using RaceDayAPI.Models.DTOs;
using RaceDay_API.Controllers;
using System.Security.Claims;
using Xunit;

namespace RaceDayAPI.Tests
{
    public class AuthControllerTests : IDisposable
    {
        private readonly RaceDayDbContext _context;
        private readonly AuthController _controller;
        private readonly ServiceProvider _serviceProvider;

        public AuthControllerTests()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new RaceDayDbContext(options);

            var services = new ServiceCollection();

            services.AddLogging();
            services.AddAuthentication(
                CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            services.AddDataProtection();

            _serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };

            _controller = new AuthController(_context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        // TEST 1: Register a new user successfully
        [Fact]
        public async Task Register_ValidDetails_ReturnsOk()
        {
            var dto = new RegisterDto
            {
                Name = "John",
                Surname = "Smith",
                Email = "john@example.com",
                Password = "Test123!",
                Role = "Participant"
            };

            var result = await _controller.Register(dto);

            Assert.IsType<OkObjectResult>(result);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            Assert.NotNull(user);
            Assert.Equal("John", user.Name);
            Assert.Equal("Smith", user.Surname);
            Assert.Equal("Participant", user.Role);

            // Password must be hashed, not stored as plain text
            Assert.NotEqual(dto.Password, user.PasswordHash);
        }

        // TEST 2: Register without an email
        [Fact]
        public async Task Register_MissingEmail_ReturnsBadRequest()
        {
            var dto = new RegisterDto
            {
                Name = "John",
                Surname = "Smith",
                Email = "",
                Password = "Test123!",
                Role = "Participant"
            };

            var result = await _controller.Register(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // TEST 3: Register without a password
        [Fact]
        public async Task Register_MissingPassword_ReturnsBadRequest()
        {
            var dto = new RegisterDto
            {
                Name = "John",
                Surname = "Smith",
                Email = "john@example.com",
                Password = "",
                Role = "Participant"
            };

            var result = await _controller.Register(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // TEST 4: Register with an invalid role
        [Fact]
        public async Task Register_InvalidRole_ReturnsBadRequest()
        {
            var dto = new RegisterDto
            {
                Name = "John",
                Surname = "Smith",
                Email = "john@example.com",
                Password = "Test123!",
                Role = "Admin"
            };

            var result = await _controller.Register(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // TEST 5: Register an email that already exists
        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            var dto = new RegisterDto
            {
                Name = "John",
                Surname = "Smith",
                Email = "john@example.com",
                Password = "Test123!",
                Role = "Participant"
            };

            await _controller.Register(dto);

            var duplicateDto = new RegisterDto
            {
                Name = "Jane",
                Surname = "Smith",
                Email = "john@example.com",
                Password = "Another123!",
                Role = "Organiser"
            };

            var result = await _controller.Register(duplicateDto);

            Assert.IsType<ConflictObjectResult>(result);
        }

        // TEST 6: Login with an unknown email
        [Fact]
        public async Task Login_UnknownEmail_ReturnsUnauthorized()
        {
            var dto = new LoginDto
            {
                Email = "unknown@example.com",
                Password = "Test123!"
            };

            var result = await _controller.Login(dto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // TEST 7: Login with the wrong password
        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            await _controller.Register(new RegisterDto
            {
                Name = "John",
                Surname = "Smith",
                Email = "john@example.com",
                Password = "Correct123!",
                Role = "Participant"
            });

            var dto = new LoginDto
            {
                Email = "john@example.com",
                Password = "Wrong123!"
            };

            var result = await _controller.Login(dto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // TEST 8: Login with correct details
        [Fact]
        public async Task Login_ValidDetails_ReturnsOk()
        {
            await _controller.Register(new RegisterDto
            {
                Name = "John",
                Surname = "Smith",
                Email = "john@example.com",
                Password = "Correct123!",
                Role = "Participant"
            });

            var dto = new LoginDto
            {
                Email = "john@example.com",
                Password = "Correct123!"
            };

            var result = await _controller.Login(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 9: Logout
        [Fact]
        public async Task Logout_ReturnsOk()
        {
            var result = await _controller.Logout();

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 10: Get the current user
        [Fact]
        public void GetCurrentUser_ValidClaims_ReturnsOk()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "john@example.com"),
                new Claim(ClaimTypes.Role, "Participant")
            };

            _controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(
                    new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults.AuthenticationScheme));

            var result = _controller.GetCurrentUser();

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 11: Dispose resources after each test
        public void Dispose()
        {
            _context.Dispose();
            _serviceProvider.Dispose();
        }

    }
}