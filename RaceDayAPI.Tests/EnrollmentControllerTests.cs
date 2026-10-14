
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using RaceDay_API.Controllers;
using System.Security.Claims;
using Xunit;

namespace RaceDayAPI.Tests
{
    public class EnrollmentControllerTests : IDisposable
    {
        private readonly RaceDayDbContext _context;
        private readonly EnrollmentController _controller;

        public EnrollmentControllerTests()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new RaceDayDbContext(options);

            var httpContext = new DefaultHttpContext();

            // Simulate a logged-in participant
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "participant@example.com"),
                new Claim(ClaimTypes.Role, "Participant")
            };

            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(claims, "Test"));

            _controller = new EnrollmentController(_context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        // TEST 1: Get all enrollments
        [Fact]
        public async Task GetAllEnrollments_ReturnsOk()
        {
            // This endpoint expects an organiser claim.
            SetUser("Organiser");

            var result = await _controller.GetAllEnrollments();

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 2: Get an existing enrollment
        [Fact]
        public async Task GetEnrollmentById_ExistingEnrollment_ReturnsOk()
        {
            _context.Events.Add(new Event
            {
                EventID = 1,
                // other required properties
            });

            _context.Categories.Add(new Category
            {
                CategoryID = 1,
                CategoryName = "Open",
                CategoryType = "18-59",
                EventID = 1
            });

            _context.Users.Add(new User
            {
                UserID = 1, // use your real key name
                Name = "John",
                Surname = "Smith",
                Email = "john@example.com",
                Role = "Participant"
                // other required properties
            });

            _context.Enrollments.Add(new Enrollment
            {
                EnrollmentID = 1,
                ParticipantID = 1,
                EventID = 1,
                CategoryID = 1,
                EnrollmentDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var result = await _controller.GetEnrollmentById(1);

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 3: Get a nonexistent enrollment
        [Fact]
        public async Task GetEnrollmentById_NotFound_ReturnsNotFound()
        {
            var result = await _controller.GetEnrollmentById(999);

            Assert.IsType<NotFoundResult>(result);
        }

        // TEST 4: Create an enrollment
        [Fact]
        public async Task CreateEnrollment_ValidEnrollment_ReturnsCreated()
        {
            var enrollment = new Enrollment
            {
                ParticipantID = 99,
                EventID = 1,
                CategoryID = 1
            };

            var result = await _controller.CreateEnrollment(enrollment);

            Assert.IsType<CreatedAtActionResult>(result);

            var savedEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync();

            Assert.NotNull(savedEnrollment);

            // Participant ID should come from the logged-in user's claim.
            Assert.Equal(1, savedEnrollment.ParticipantID);

            Assert.True(
                savedEnrollment.EnrollmentDate <= DateTime.UtcNow);
        }

        // TEST 5: Create a null enrollment
        [Fact]
        public async Task CreateEnrollment_NullEnrollment_ReturnsBadRequest()
        {
            var result = await _controller.CreateEnrollment(null!);

            Assert.IsType<BadRequestResult>(result);
        }

        // TEST 6: Update an existing enrollment
        [Fact]
        public async Task UpdateEnrollment_ExistingEnrollment_ReturnsOk()
        {
            _context.Enrollments.Add(new Enrollment
            {
                EnrollmentID = 1,
                ParticipantID = 1,
                EventID = 1,
                CategoryID = 1,
                EnrollmentDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var updatedEnrollment = new Enrollment
            {
                ParticipantID = 2,
                EventID = 2,
                CategoryID = 2,
                EnrollmentDate = DateTime.UtcNow
            };

            var result = await _controller.UpdateEnrollment(
                1, updatedEnrollment);

            Assert.IsType<OkObjectResult>(result);

            var savedEnrollment = await _context.Enrollments.FindAsync(1);

            Assert.NotNull(savedEnrollment);
            Assert.Equal(2, savedEnrollment.ParticipantID);
            Assert.Equal(2, savedEnrollment.EventID);
            Assert.Equal(2, savedEnrollment.CategoryID);
        }

        // TEST 7: Update a nonexistent enrollment
        [Fact]
        public async Task UpdateEnrollment_NotFound_ReturnsNotFound()
        {
            var updatedEnrollment = new Enrollment
            {
                ParticipantID = 1,
                EventID = 1,
                CategoryID = 1,
                EnrollmentDate = DateTime.UtcNow
            };

            var result = await _controller.UpdateEnrollment(
                999, updatedEnrollment);

            Assert.IsType<NotFoundResult>(result);
        }

        // TEST 8: Delete an existing enrollment
        [Fact]
        public async Task DeleteEnrollment_ExistingEnrollment_ReturnsNoContent()
        {
            _context.Enrollments.Add(new Enrollment
            {
                EnrollmentID = 1,
                ParticipantID = 1,
                EventID = 1,
                CategoryID = 1,
                EnrollmentDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var result = await _controller.DeleteEnrollment(1);

            Assert.IsType<NoContentResult>(result);

            Assert.Null(await _context.Enrollments.FindAsync(1));
        }

        // TEST 9: Delete a nonexistent enrollment
        [Fact]
        public async Task DeleteEnrollment_NotFound_ReturnsNotFound()
        {
            var result = await _controller.DeleteEnrollment(999);

            Assert.IsType<NotFoundResult>(result);
        }

        private void SetUser(string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "test@example.com"),
                new Claim(ClaimTypes.Role, role)
            };

            _controller.ControllerContext.HttpContext.User =
                new ClaimsPrincipal(
                    new ClaimsIdentity(claims, "Test"));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
