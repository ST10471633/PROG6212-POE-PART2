using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using System.Security.Claims;

namespace RaceDay_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // require login for everything by default
    public class ResultController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/result — Organiser sees results only for events they organise
        [Authorize(Roles = "Organiser")]
        [HttpGet]
        public async Task<IActionResult> GetAllResults()
        {
            var organiserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var results = await _context.Results
                .Include(r => r.Enrollment)
                    .ThenInclude(e => e!.Event)
                .Where(r => r.Enrollment!.Event!.OrganiserID == organiserId)
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/result/my-results — Participant views only THEIR OWN results
        [Authorize(Roles = "Participant")]
        [HttpGet("my-results")]
        public async Task<IActionResult> GetMyResults()
        {
            var participantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var results = await _context.Results
                .Include(r => r.Enrollment)
                .Where(r => r.Enrollment!.ParticipantID == participantId)
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/result/1 — Organiser only, used for CreatedAtAction and direct lookups
        [Authorize(Roles = "Organiser")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResultById(int id)
        {
            var result = await _context.Results
                .Include(r => r.Enrollment)
                .FirstOrDefaultAsync(r => r.ResultsID == id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // POST: api/result
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateResult([FromBody] Result newResult)
        {
            if (newResult == null)
            {
                return BadRequest();
            }

            _context.Results.Add(newResult);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResultById), new { id = newResult.ResultsID }, newResult);
        }

        // PUT: api/result/1
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] Result updatedResult)
        {
            var result = await _context.Results.FindAsync(id);

            if (result == null)
            {
                return NotFound();
            }

            result.FinishTime = updatedResult.FinishTime;
            result.FinishPosition = updatedResult.FinishPosition;
            result.EnrollmentID = updatedResult.EnrollmentID;

            await _context.SaveChangesAsync();

            return Ok(result);
        }

        // DELETE: api/result/1
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            var result = await _context.Results.FindAsync(id);

            if (result == null)
            {
                return NotFound();
            }

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}