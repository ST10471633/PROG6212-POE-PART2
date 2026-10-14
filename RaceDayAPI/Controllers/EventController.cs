using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RaceDay_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/event
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.EventType)
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/event/1
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var evt = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (evt == null)
            {
                return NotFound();
            }

            return Ok(evt);
        }

        // POST: api/event
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Event newEvent)
        {
            if (newEvent == null)
            {
                return BadRequest();
            }

            var organiserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            newEvent.OrganiserID = organiserId;

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventID }, newEvent);
        }

        // PUT: api/event/1
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event updatedEvent)
        {
            var evt = await _context.Events.FindAsync(id);

            if (evt == null)
            {
                return NotFound();
            }

            evt.EventName = updatedEvent.EventName;
            evt.Description = updatedEvent.Description;
            evt.EventDate = updatedEvent.EventDate;
            evt.Location = updatedEvent.Location;
            evt.Distance = updatedEvent.Distance;
            evt.EventTypeID = updatedEvent.EventTypeID;

            await _context.SaveChangesAsync();

            return Ok(evt);
        }

        // DELETE: api/event/1
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var evt = await _context.Events.FindAsync(id);

            if (evt == null)
            {
                return NotFound();
            }

            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}