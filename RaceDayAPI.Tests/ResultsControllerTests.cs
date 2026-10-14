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
    public class ResultControllerTests : IDisposable
    {
        private readonly RaceDayDbContext _context;
        private readonly ResultController _controller;

        public ResultControllerTests()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new RaceDayDbContext(options);

            var httpContext = new DefaultHttpContext();

            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim(ClaimTypes.Name, "organiser@example.com"),
                        new Claim(ClaimTypes.Role, "Organiser")
                    },
                    "TestAuthentication"));

            _controller = new ResultController(_context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        private void SetUser(int userId, string role)
        {
            var httpContext = new DefaultHttpContext();

            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                        new Claim(ClaimTypes.Name, "test@example.com"),
                        new Claim(ClaimTypes.Role, role)
                    },
                    "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        private async Task SeedResultData()
        {
            var eventType = new EventType
            {
                EventTypeID = 1,
                TypeName = "Marathon"
            };

            var raceEvent = new Event
            {
                EventID = 1,
                EventName = "City Marathon",
                Description = "Annual marathon",
                EventDate = DateTime.Today,
                Location = "Johannesburg",
                Distance = 42,
                OrganiserID = 1,
                EventTypeID = 1
            };

            var enrollment = new Enrollment
            {
                EnrollmentID = 1,
                ParticipantID = 2,
                EventID = 1,
                CategoryID = 1,
                EnrollmentDate = DateTime.Today
            };

            var result = new Result
            {
                ResultsID = 1,
                EnrollmentID = 1,
                FinishPosition = 1
            };

            _context.EventTypes.Add(eventType);
            _context.Events.Add(raceEvent);
            _context.Enrollments.Add(enrollment);
            _context.Results.Add(result);

            await _context.SaveChangesAsync();
        }

        // TEST 1: Get all results for an organiser
        [Fact]
        public async Task GetAllResults_ReturnsOk()
        {
            await SeedResultData();
            SetUser(1, "Organiser");

            var response = await _controller.GetAllResults();

            Assert.IsType<OkObjectResult>(response);
        }

        // TEST 2: Get only the current participant's results
        [Fact]
        public async Task GetMyResults_ReturnsOk()
        {
            await SeedResultData();
            SetUser(2, "Participant");

            var response = await _controller.GetMyResults();

            Assert.IsType<OkObjectResult>(response);
        }

        // TEST 3: Get an existing result
        [Fact]
        public async Task GetResultById_ExistingResult_ReturnsOk()
        {
            await SeedResultData();

            var response = await _controller.GetResultById(1);

            Assert.IsType<OkObjectResult>(response);
        }

        // TEST 4: Return NotFound when the result does not exist
        [Fact]
        public async Task GetResultById_NotFound_ReturnsNotFound()
        {
            var response = await _controller.GetResultById(999);

            Assert.IsType<NotFoundResult>(response);
        }

        // TEST 5: Create a result
        [Fact]
        public async Task CreateResult_ValidResult_ReturnsCreated()
        {
            var result = new Result
            {
                ResultsID = 2,
                EnrollmentID = 1,
                FinishPosition = 2
            };

            var response = await _controller.CreateResult(result);

            Assert.IsType<CreatedAtActionResult>(response);
            Assert.Equal(1, await _context.Results.CountAsync());
        }

        // TEST 6: Return BadRequest when the result is null
        [Fact]
        public async Task CreateResult_NullResult_ReturnsBadRequest()
        {
            var response = await _controller.CreateResult(null!);

            Assert.IsType<BadRequestResult>(response);
        }

        // TEST 7: Update an existing result
        [Fact]
        public async Task UpdateResult_ExistingResult_ReturnsOk()
        {
            await SeedResultData();

            var updatedResult = new Result
            {
                ResultsID = 1,
                EnrollmentID = 1,
                FinishPosition = 5
            };

            var response = await _controller.UpdateResult(1, updatedResult);

            Assert.IsType<OkObjectResult>(response);

            var resultInDatabase = await _context.Results.FindAsync(1);
            Assert.Equal(5, resultInDatabase!.FinishPosition);
        }

        // TEST 8: Return NotFound when updating a missing result
        [Fact]
        public async Task UpdateResult_NotFound_ReturnsNotFound()
        {
            var updatedResult = new Result
            {
                EnrollmentID = 1,
                FinishPosition = 5
            };

            var response = await _controller.UpdateResult(999, updatedResult);

            Assert.IsType<NotFoundResult>(response);
        }

        // TEST 9: Delete an existing result
        [Fact]
        public async Task DeleteResult_ExistingResult_ReturnsNoContent()
        {
            await SeedResultData();

            var response = await _controller.DeleteResult(1);

            Assert.IsType<NoContentResult>(response);
            Assert.Equal(0, await _context.Results.CountAsync());
        }

        // TEST 10: Return NotFound when deleting a missing result
        [Fact]
        public async Task DeleteResult_NotFound_ReturnsNotFound()
        {
            var response = await _controller.DeleteResult(999);

            Assert.IsType<NotFoundResult>(response);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}