
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
    public class EventTypeControllerTests : IDisposable
    {
        private readonly RaceDayDbContext _context;
        private readonly EventTypeController _controller;

        public EventTypeControllerTests()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new RaceDayDbContext(options);

            var httpContext = new DefaultHttpContext();

            httpContext.User = CreateUser("Organiser");

            _controller = new EventTypeController(_context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        private static ClaimsPrincipal CreateUser(string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "organiser@example.com"),
                new Claim(ClaimTypes.Role, role)
            };

            return new ClaimsPrincipal(
                new ClaimsIdentity(claims, "Test"));
        }

        // TEST 1: Get all event types
        [Fact]
        public async Task GetAllEventTypes_ReturnsOk()
        {
            var result = await _controller.GetAllEventTypes();

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 2: Get an existing event type
        [Fact]
        public async Task GetEventTypeById_ExistingEventType_ReturnsOk()
        {
            _context.EventTypes.Add(new EventType
            {
                EventTypeID = 1,
                TypeName = "Marathon"
            });

            await _context.SaveChangesAsync();

            var result = await _controller.GetEventTypeById(1);

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 3: Get a nonexistent event type
        [Fact]
        public async Task GetEventTypeById_NotFound_ReturnsNotFound()
        {
            var result = await _controller.GetEventTypeById(999);

            Assert.IsType<NotFoundResult>(result);
        }

        // TEST 4: Create an event type
        [Fact]
        public async Task CreateEventType_ValidEventType_ReturnsCreated()
        {
            var eventType = new EventType
            {
                TypeName = "Fun Run"
            };

            var result = await _controller.CreateEventType(eventType);

            Assert.IsType<CreatedAtActionResult>(result);

            var savedEventType = await _context.EventTypes
                .FirstOrDefaultAsync();

            Assert.NotNull(savedEventType);
            Assert.Equal("Fun Run", savedEventType.TypeName);
        }

        // TEST 5: Create a null event type
        [Fact]
        public async Task CreateEventType_NullEventType_ReturnsBadRequest()
        {
            var result = await _controller.CreateEventType(null!);

            Assert.IsType<BadRequestResult>(result);
        }

        // TEST 6: Update an existing event type
        [Fact]
        public async Task UpdateEventType_ExistingEventType_ReturnsOk()
        {
            _context.EventTypes.Add(new EventType
            {
                EventTypeID = 1,
                TypeName = "Old Name"
            });

            await _context.SaveChangesAsync();

            var updatedEventType = new EventType
            {
                TypeName = "Triathlon"
            };

            var result = await _controller.UpdateEventType(
                1, updatedEventType);

            Assert.IsType<OkObjectResult>(result);

            var savedEventType = await _context.EventTypes.FindAsync(1);

            Assert.NotNull(savedEventType);
            Assert.Equal("Triathlon", savedEventType.TypeName);
        }

        // TEST 7: Update a nonexistent event type
        [Fact]
        public async Task UpdateEventType_NotFound_ReturnsNotFound()
        {
            var updatedEventType = new EventType
            {
                TypeName = "Triathlon"
            };

            var result = await _controller.UpdateEventType(
                999, updatedEventType);

            Assert.IsType<NotFoundResult>(result);
        }

        // TEST 8: Delete an existing event type
        [Fact]
        public async Task DeleteEventType_ExistingEventType_ReturnsNoContent()
        {
            _context.EventTypes.Add(new EventType
            {
                EventTypeID = 1,
                TypeName = "Fun Run"
            });

            await _context.SaveChangesAsync();

            var result = await _controller.DeleteEventType(1);

            Assert.IsType<NoContentResult>(result);

            Assert.Null(await _context.EventTypes.FindAsync(1));
        }

        // TEST 9: Delete a nonexistent event type
        [Fact]
        public async Task DeleteEventType_NotFound_ReturnsNotFound()
        {
            var result = await _controller.DeleteEventType(999);

            Assert.IsType<NotFoundResult>(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
