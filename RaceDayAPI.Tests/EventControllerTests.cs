
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
    public class EventControllerTests : IDisposable
    {
        private readonly RaceDayDbContext _context;
        private readonly EventController _controller;

        public EventControllerTests()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new RaceDayDbContext(options);

            var httpContext = new DefaultHttpContext();

            httpContext.User = CreateUser("Organiser");

            _controller = new EventController(_context)
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

        // TEST 1: Get all events
        [Fact]
        public async Task GetAllEvents_ReturnsOk()
        {
            var result = await _controller.GetAllEvents();

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 2: Get an existing event
        [Fact]
        public async Task GetEventById_ExistingEvent_ReturnsOk()
        {
            _context.Users.Add(new User
            {
                UserID = 1, // use your real key name
                Name = "Olivia",
                Surname = "Organiser",
                Email = "organiser@example.com",
                Role = "Organiser"
                // other required properties
            });

            _context.EventTypes.Add(new EventType
            {
                EventTypeID = 1,
                // e.g. TypeName = "Marathon", plus any other required properties
            });

            _context.Events.Add(new Event
            {
                EventID = 1,
                EventName = "City Marathon",
                Description = "Annual marathon",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Pretoria",
                Distance = 42,
                OrganiserID = 1,
                EventTypeID = 1
            });

            await _context.SaveChangesAsync();

            var result = await _controller.GetEventById(1);

            Assert.IsType<OkObjectResult>(result);
        }

        // TEST 3: Get a nonexistent event
        [Fact]
        public async Task GetEventById_NotFound_ReturnsNotFound()
        {
            var result = await _controller.GetEventById(999);

            Assert.IsType<NotFoundResult>(result);
        }

        // TEST 4: Create an event
        [Fact]
        public async Task CreateEvent_ValidEvent_ReturnsCreated()
        {
            var newEvent = new Event
            {
                EventName = "Fun Run",
                Description = "A five kilometre fun run",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "Pretoria",
                Distance = 5,
                EventTypeID = 1,
                OrganiserID = 999
            };

            var result = await _controller.CreateEvent(newEvent);

            Assert.IsType<CreatedAtActionResult>(result);

            var savedEvent = await _context.Events
                .FirstOrDefaultAsync();

            Assert.NotNull(savedEvent);

            // Organiser ID comes from the logged-in user's claim.
            Assert.Equal(1, savedEvent.OrganiserID);
        }

        // TEST 5: Create a null event
        [Fact]
        public async Task CreateEvent_NullEvent_ReturnsBadRequest()
        {
            var result = await _controller.CreateEvent(null!);

            Assert.IsType<BadRequestResult>(result);
        }

        // TEST 6: Update an existing event
        [Fact]
        public async Task UpdateEvent_ExistingEvent_ReturnsOk()
        {
            _context.Events.Add(new Event
            {
                EventID = 1,
                EventName = "Old Event",
                Description = "Old description",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "Pretoria",
                Distance = 5,
                OrganiserID = 1,
                EventTypeID = 1
            });

            await _context.SaveChangesAsync();

            var updatedEvent = new Event
            {
                EventName = "Updated Event",
                Description = "Updated description",
                EventDate = DateTime.UtcNow.AddDays(20),
                Location = "Johannesburg",
                Distance = 10,
                EventTypeID = 2
            };

            var result = await _controller.UpdateEvent(1, updatedEvent);

            Assert.IsType<OkObjectResult>(result);

            var savedEvent = await _context.Events.FindAsync(1);

            Assert.NotNull(savedEvent);
            Assert.Equal("Updated Event", savedEvent.EventName);
            Assert.Equal("Updated description", savedEvent.Description);
            Assert.Equal("Johannesburg", savedEvent.Location);
            Assert.Equal(10, savedEvent.Distance);
            Assert.Equal(2, savedEvent.EventTypeID);
        }

        // TEST 7: Update a nonexistent event
        [Fact]
        public async Task UpdateEvent_NotFound_ReturnsNotFound()
        {
            var updatedEvent = new Event
            {
                EventName = "Updated Event",
                Description = "Updated description",
                EventDate = DateTime.UtcNow,
                Location = "Pretoria",
                Distance = 10,
                EventTypeID = 1
            };

            var result = await _controller.UpdateEvent(999, updatedEvent);

            Assert.IsType<NotFoundResult>(result);
        }

        // TEST 8: Delete an existing event
        [Fact]
        public async Task DeleteEvent_ExistingEvent_ReturnsNoContent()
        {
            _context.Events.Add(new Event
            {
                EventID = 1,
                EventName = "Event to Delete",
                Description = "Test event",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "Pretoria",
                Distance = 5,
                OrganiserID = 1,
                EventTypeID = 1
            });

            await _context.SaveChangesAsync();

            var result = await _controller.DeleteEvent(1);

            Assert.IsType<NoContentResult>(result);

            Assert.Null(await _context.Events.FindAsync(1));
        }

        // TEST 9: Delete a nonexistent event
        [Fact]
        public async Task DeleteEvent_NotFound_ReturnsNotFound()
        {
            var result = await _controller.DeleteEvent(999);

            Assert.IsType<NotFoundResult>(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
