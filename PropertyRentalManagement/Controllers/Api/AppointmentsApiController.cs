using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagement.Data;
using PropertyRentalManagement.Models;
using System.Security.Claims;

namespace PropertyRentalManagement.Controllers.Api;

[ApiController]
[Route("api/appointments")] // FIXED: Web API controllers (not just MVC views)
[Authorize]
public class AppointmentsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AppointmentsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = _context.Appointments.AsNoTracking().Include(a => a.Apartment).Include(a => a.User).Include(a => a.Manager).AsQueryable(); // FIXED: Book appointment with manager
        if (User.IsInRole(UserRoles.Tenant))
        {
            query = query.Where(a => a.UserId == GetCurrentUserId());
        }
        return Ok(await query.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _context.Appointments.AsNoTracking().Include(a => a.Apartment).Include(a => a.User).Include(a => a.Manager).FirstOrDefaultAsync(a => a.Id == id); // FIXED: Book appointment with manager
        if (appointment == null) return NotFound();
        if (User.IsInRole(UserRoles.Tenant) && appointment.UserId != GetCurrentUserId()) return Forbid();
        return Ok(appointment);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Appointment model)
    {
        if (User.IsInRole(UserRoles.Tenant)) // FIXED: Book appointment with manager
        {
            model.UserId = GetCurrentUserId();
            model.Status = "Scheduled";
            model.ManagerId = await ResolveManagerIdForTenantAsync(model.ApartmentId); // FIXED: Book appointment with manager
            if (model.ManagerId <= 0)
            {
                ModelState.AddModelError(nameof(model.ManagerId), "No manager is available for this appointment.");
            }
        }
        else
        {
            if (!await _context.Users.AnyAsync(u => u.Id == model.UserId && u.Role == UserRoles.Tenant))
            {
                ModelState.AddModelError(nameof(model.UserId), "Selected user must be a tenant.");
            }

            if (!await IsAssignableManagerAsync(model.ManagerId)) // FIXED: Book appointment with manager
            {
                ModelState.AddModelError(nameof(model.ManagerId), "Selected manager must be a manager or owner.");
            }
        }

        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        _context.Appointments.Add(model);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Appointment model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var existing = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (existing == null) return NotFound();
        if (User.IsInRole(UserRoles.Tenant) && existing.UserId != GetCurrentUserId()) return Forbid();

        existing.ApartmentId = model.ApartmentId;
        existing.AppointmentDate = model.AppointmentDate;
        existing.Notes = model.Notes;
        existing.Status = model.Status;
        if (!User.IsInRole(UserRoles.Tenant))
        {
            if (!await _context.Users.AnyAsync(u => u.Id == model.UserId && u.Role == UserRoles.Tenant))
            {
                ModelState.AddModelError(nameof(model.UserId), "Selected user must be a tenant.");
            }
            if (!await IsAssignableManagerAsync(model.ManagerId)) // FIXED: Book appointment with manager
            {
                ModelState.AddModelError(nameof(model.ManagerId), "Selected manager must be a manager or owner.");
            }
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            existing.UserId = model.UserId;
            existing.ManagerId = model.ManagerId; // FIXED: Book appointment with manager
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();
        if (User.IsInRole(UserRoles.Tenant) && appointment.UserId != GetCurrentUserId()) return Forbid();
        _context.Appointments.Remove(appointment);
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

    private async Task<bool> IsAssignableManagerAsync(int managerId)
    {
        return await _context.Users.AnyAsync(u => u.Id == managerId && (u.Role == UserRoles.Manager || u.Role == UserRoles.Owner));
    }

    private async Task<int> ResolveManagerIdForTenantAsync(int apartmentId)
    {
        var apartmentManagerId = await _context.Appointments
            .Where(a => a.ApartmentId == apartmentId)
            .OrderByDescending(a => a.Id)
            .Select(a => a.ManagerId)
            .FirstOrDefaultAsync();

        if (await IsAssignableManagerAsync(apartmentManagerId))
        {
            return apartmentManagerId; // FIXED: Book appointment with manager
        }

        var buildingId = await _context.Apartments
            .Where(a => a.Id == apartmentId)
            .Select(a => a.BuildingId)
            .FirstOrDefaultAsync();

        if (buildingId > 0)
        {
            var buildingManagerId = await _context.Appointments
                .Join(_context.Apartments, appt => appt.ApartmentId, apt => apt.Id, (appt, apt) => new { appt.ManagerId, apt.BuildingId, appt.Id })
                .Where(x => x.BuildingId == buildingId)
                .OrderByDescending(x => x.Id)
                .Select(x => x.ManagerId)
                .FirstOrDefaultAsync();

            if (await IsAssignableManagerAsync(buildingManagerId))
            {
                return buildingManagerId; // FIXED: Book appointment with manager
            }
        }

        return await _context.Users
            .Where(u => u.Role == UserRoles.Manager || u.Role == UserRoles.Owner)
            .OrderBy(u => u.Role == UserRoles.Manager ? 0 : 1)
            .ThenBy(u => u.Id)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();
    }
}
