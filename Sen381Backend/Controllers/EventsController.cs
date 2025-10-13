using Microsoft.AspNetCore.Mvc;
using Supabase;
using Sen381Backend.Models;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetForUser(int userId, [FromServices] Client client)
        {
            try
            {
                var resp = await client
                    .From<UserEvent>()
                    .Where(e => e.UserId == userId)
                    .Order("when_at", Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();
                var result = resp.Models.Select(m => new CreateEventDto
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    Title = m.Title,
                    WhenAt = m.WhenAt,
                    DurationMinutes = m.DurationMinutes
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        public class CreateEventDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string Title { get; set; } = string.Empty;
            public DateTime WhenAt { get; set; }
            public int DurationMinutes { get; set; } = 60;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto, [FromServices] Client client)
        {
            if (dto.UserId <= 0 || string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Invalid event");
            if (dto.WhenAt.Date < DateTime.Today) return BadRequest(new { error = "Cannot create events in the past." });

            var model = new UserEvent
            {
                UserId = dto.UserId,
                Title = dto.Title,
                WhenAt = dto.WhenAt,
                DurationMinutes = dto.DurationMinutes,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                // Limit to 5 events per day
                var dayStart = new DateTime(dto.WhenAt.Year, dto.WhenAt.Month, dto.WhenAt.Day, 0, 0, 0, DateTimeKind.Unspecified);
                var dayEnd = dayStart.AddDays(1);
                var allUserEvents = await client
                    .From<UserEvent>()
                    .Where(e => e.UserId == dto.UserId)
                    .Get();
                var existing = allUserEvents.Models.Where(e => e.WhenAt >= dayStart && e.WhenAt < dayEnd);
                if (existing.Count() >= 5)
                    return BadRequest(new { error = "You can only have up to 5 events per day." });

                var inserted = await client.From<UserEvent>().Insert(model);
                var saved = inserted.Models.FirstOrDefault() ?? model;
                var dtoOut = new CreateEventDto
                {
                    Id = saved.Id,
                    UserId = saved.UserId,
                    Title = saved.Title,
                    WhenAt = saved.WhenAt,
                    DurationMinutes = saved.DurationMinutes
                };
                return Ok(dtoOut);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateEventDto dto, [FromServices] Client client)
        {
            try
            {
                if (dto.WhenAt.Date < DateTime.Today) return BadRequest(new { error = "Cannot move events to the past." });

                // Limit to 5 events per day (exclude current event being updated)
                var dayStart = new DateTime(dto.WhenAt.Year, dto.WhenAt.Month, dto.WhenAt.Day, 0, 0, 0, DateTimeKind.Unspecified);
                var dayEnd = dayStart.AddDays(1);
                var allUserEvents = await client
                    .From<UserEvent>()
                    .Where(e => e.UserId == dto.UserId)
                    .Get();
                var clash = allUserEvents.Models.Where(e => e.WhenAt >= dayStart && e.WhenAt < dayEnd && e.Id != id);
                if (clash.Count() >= 5)
                    return BadRequest(new { error = "You can only have up to 5 events per day." });

                var ev = new UserEvent { Id = id, Title = dto.Title, WhenAt = dto.WhenAt, DurationMinutes = dto.DurationMinutes };
                var updated = await client.From<UserEvent>().Update(ev);
                var saved = updated.Models.FirstOrDefault() ?? ev;
                return Ok(new CreateEventDto { Id = saved.Id, UserId = saved.UserId, Title = saved.Title, WhenAt = saved.WhenAt, DurationMinutes = saved.DurationMinutes });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, [FromServices] Client client)
        {
            try
            {
                await client.From<UserEvent>().Where(e => e.Id == id).Delete();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}


