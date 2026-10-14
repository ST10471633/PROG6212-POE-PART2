using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using System.Security.Claims;
using UserModel = RaceDayAPI.Models.User; // avoids clash with ControllerBase.User

namespace RaceDay_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrollmentController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/enrollment
        [Authorize(Roles = "Organiser")]
        [HttpGet]
        public async Task<IActionResult> GetAllEnrollments()
        {
            var organiserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var enrollments = await _context.Enrollments
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .ToListAsync();

            return Ok(enrollments);
        }

        // GET: api/enrollment/1
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnrollmentById(int id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EnrollmentID == id);

            if (enrollment == null)
            {
                return NotFound();
            }

            return Ok(enrollment);
        }

        // POST: api/enrollment
        [Authorize(Roles = "Participant")]
        [HttpPost]
        public async Task<IActionResult> CreateEnrollment([FromBody] Enrollment newEnrollment)
        {
            if (newEnrollment == null)
            {
                return BadRequest();
            }

            var participantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            newEnrollment.ParticipantID = participantId;
            newEnrollment.EnrollmentDate = DateTime.UtcNow;

            _context.Enrollments.Add(newEnrollment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnrollmentById), new { id = newEnrollment.EnrollmentID }, newEnrollment);
        }

        // PUT: api/enrollment/1
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEnrollment(int id, [FromBody] Enrollment updatedEnrollment)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);

            if (enrollment == null)
            {
                return NotFound();
            }

            enrollment.EnrollmentDate = updatedEnrollment.EnrollmentDate;
            enrollment.ParticipantID = updatedEnrollment.ParticipantID;
            enrollment.EventID = updatedEnrollment.EventID;
            enrollment.CategoryID = updatedEnrollment.CategoryID;

            await _context.SaveChangesAsync();

            return Ok(enrollment);
        }

        // DELETE: api/enrollment/1
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);

            if (enrollment == null)
            {
                return NotFound();
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}