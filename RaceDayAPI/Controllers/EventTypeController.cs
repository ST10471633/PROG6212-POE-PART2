using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDay_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // require login for everything by default
    public class EventTypeController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventTypeController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/eventtype
        [HttpGet]
        public async Task<IActionResult> GetAllEventTypes()
        {
            var eventTypes = await _context.EventTypes.ToListAsync();
            return Ok(eventTypes);
        }

        // GET: api/eventtype/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventTypeById(int id)
        {
            var eventType = await _context.EventTypes
                .FirstOrDefaultAsync(et => et.EventTypeID == id);

            if (eventType == null)
            {
                return NotFound();
            }

            return Ok(eventType);
        }

        // POST: api/eventtype
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateEventType([FromBody] EventType newEventType)
        {
            if (newEventType == null)
            {
                return BadRequest();
            }

            _context.EventTypes.Add(newEventType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventTypeById), new { id = newEventType.EventTypeID }, newEventType);
        }

        // PUT: api/eventtype/1
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEventType(int id, [FromBody] EventType updatedEventType)
        {
            var eventType = await _context.EventTypes.FindAsync(id);

            if (eventType == null)
            {
                return NotFound();
            }

            eventType.TypeName = updatedEventType.TypeName;

            await _context.SaveChangesAsync();

            return Ok(eventType);
        }

        // DELETE: api/eventtype/1
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEventType(int id)
        {
            var eventType = await _context.EventTypes.FindAsync(id);

            if (eventType == null)
            {
                return NotFound();
            }

            _context.EventTypes.Remove(eventType);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}