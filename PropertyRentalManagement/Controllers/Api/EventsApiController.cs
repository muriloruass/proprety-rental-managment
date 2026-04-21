using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagement.Data;
using PropertyRentalManagement.Models;
using System.Security.Claims;

namespace PropertyRentalManagement.Controllers.Api;

[ApiController]
[Route("api/events")] // FIXED: Web API controllers (not just MVC views)
[Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
public class EventsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EventsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _context.Events.AsNoTracking()
            .Include(e => e.ReportedBy)
            .Include(e => e.Apartment)
            .OrderByDescending(e => e.EventDate)
            .ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var @event = await _context.Events.AsNoTracking()
            .Include(e => e.ReportedBy)
            .Include(e => e.Apartment)
            .FirstOrDefaultAsync(e => e.Id == id);
        return @event == null ? NotFound() : Ok(@event);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Event model)
    {
        model.ReportedById = GetCurrentUserId(); // FIXED: Report events to owner
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        _context.Events.Add(model);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Event model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var existing = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (existing == null) return NotFound();

        existing.Title = model.Title;
        existing.Description = model.Description;
        existing.EventDate = model.EventDate;
        existing.Severity = model.Severity;
        existing.ApartmentId = model.ApartmentId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var @event = await _context.Events.FindAsync(id);
        if (@event == null) return NotFound();
        _context.Events.Remove(@event);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new InvalidOperationException("Authenticated user id claim is missing.");
        }

        return userId;
    }
}
