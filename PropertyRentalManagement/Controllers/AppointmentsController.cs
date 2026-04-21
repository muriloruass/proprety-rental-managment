using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagement.Data;
using PropertyRentalManagement.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PropertyRentalManagement.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var appointments = _context.Appointments
                .Include(a => a.Apartment)
                .Include(a => a.User)
                .Include(a => a.Manager) // FIXED: Book appointment with manager
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                appointments = appointments.Where(a =>
                    a.User.Name.ToLower().Contains(term) ||
                    a.Apartment.AptNumber.ToLower().Contains(term) ||
                    (a.Notes != null && a.Notes.ToLower().Contains(term))); // FIXED: Schedule tenant appointments
            }

            if (User.IsInRole(UserRoles.Tenant))
            {
                var currentUserId = GetCurrentUserId();
                appointments = appointments.Where(a => a.UserId == currentUserId);
            }

            ViewBag.Search = search;
            return View(await appointments.OrderByDescending(a => a.AppointmentDate).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Apartment)
                .Include(a => a.User)
                .Include(a => a.Manager) // FIXED: Book appointment with manager
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();
            if (User.IsInRole(UserRoles.Tenant) && appointment.UserId != GetCurrentUserId()) return Forbid();

            return View(appointment);
        }

        public IActionResult Create()
        {
            var canSelectUser = !User.IsInRole(UserRoles.Tenant);
            PopulateCreateEditViewBags(canSelectUser, null, null, null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            var isTenant = User.IsInRole(UserRoles.Tenant);
            if (isTenant)
            {
                appointment.UserId = GetCurrentUserId();
                appointment.Status = "Scheduled";
                appointment.ManagerId = await ResolveManagerIdForTenantAsync(appointment.ApartmentId); // FIXED: Book appointment with manager
                ModelState.Remove(nameof(appointment.UserId)); // FIXED: Book appointment with manager
                ModelState.Remove(nameof(appointment.ManagerId)); // FIXED: Book appointment with manager
                if (appointment.ManagerId <= 0)
                {
                    ModelState.AddModelError(nameof(appointment.ManagerId), "No manager is available for this appointment.");
                }
            }
            else
            {
                var tenantExists = await _context.Users.AnyAsync(u => u.Id == appointment.UserId && u.Role == UserRoles.Tenant);
                if (!tenantExists)
                {
                    ModelState.AddModelError(nameof(appointment.UserId), "Selected user must be a tenant.");
                }

                if (!await IsAssignableManagerAsync(appointment.ManagerId)) // FIXED: Book appointment with manager
                {
                    ModelState.AddModelError(nameof(appointment.ManagerId), "Selected manager must be a manager or owner.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateCreateEditViewBags(!isTenant, appointment.ApartmentId, appointment.UserId, appointment.ManagerId);
            return View(appointment);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            if (User.IsInRole(UserRoles.Tenant) && appointment.UserId != GetCurrentUserId()) return Forbid();

            var canSelectUser = !User.IsInRole(UserRoles.Tenant);
            PopulateCreateEditViewBags(canSelectUser, appointment.ApartmentId, appointment.UserId, appointment.ManagerId);
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id) return NotFound();

            var existingAppointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (existingAppointment == null) return NotFound();

            var isTenant = User.IsInRole(UserRoles.Tenant);
            if (isTenant && existingAppointment.UserId != GetCurrentUserId()) return Forbid();
            if (isTenant)
            {
                appointment.UserId = existingAppointment.UserId;
                appointment.ManagerId = existingAppointment.ManagerId; // FIXED: Book appointment with manager
                ModelState.Remove(nameof(appointment.UserId));
                ModelState.Remove(nameof(appointment.ManagerId));
            }

            if (!isTenant)
            {
                var tenantExists = await _context.Users.AnyAsync(u => u.Id == appointment.UserId && u.Role == UserRoles.Tenant);
                if (!tenantExists)
                {
                    ModelState.AddModelError(nameof(appointment.UserId), "Selected user must be a tenant.");
                }

                if (!await IsAssignableManagerAsync(appointment.ManagerId)) // FIXED: Book appointment with manager
                {
                    ModelState.AddModelError(nameof(appointment.ManagerId), "Selected manager must be a manager or owner.");
                }
            }

            if (ModelState.IsValid)
            {
                existingAppointment.ApartmentId = appointment.ApartmentId;
                existingAppointment.AppointmentDate = appointment.AppointmentDate;
                existingAppointment.Notes = appointment.Notes;
                existingAppointment.Status = appointment.Status;
                if (!isTenant)
                {
                    existingAppointment.UserId = appointment.UserId;
                    existingAppointment.ManagerId = appointment.ManagerId; // FIXED: Book appointment with manager
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateCreateEditViewBags(!isTenant, appointment.ApartmentId, appointment.UserId, appointment.ManagerId);
            return View(appointment);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Apartment)
                .Include(a => a.User)
                .Include(a => a.Manager) // FIXED: Book appointment with manager
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();
            if (User.IsInRole(UserRoles.Tenant) && appointment.UserId != GetCurrentUserId()) return Forbid();

            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return RedirectToAction(nameof(Index));
            if (User.IsInRole(UserRoles.Tenant) && appointment.UserId != GetCurrentUserId()) return Forbid();

            _context.Appointments.Remove(appointment);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

        private void PopulateCreateEditViewBags(bool canSelectUser, int? apartmentId, int? userId, int? managerId)
        {
            var apartments = _context.Apartments.Include(a => a.Building).Select(a => new
            {
                Id = a.Id,
                DisplayText = "Apt " + a.AptNumber + " - " + (a.Building != null ? a.Building.Name : "No Building")
            }).ToList();
            ViewBag.ApartmentId = new SelectList(apartments, "Id", "DisplayText", apartmentId);
            ViewBag.CanSelectUser = canSelectUser;
            ViewBag.CanSelectManager = canSelectUser; // FIXED: Book appointment with manager
            if (canSelectUser)
            {
                ViewBag.UserId = new SelectList(_context.Users.Where(u => u.Role == UserRoles.Tenant), "Id", "Name", userId);
                ViewBag.ManagerId = new SelectList( // FIXED: Book appointment with manager
                    _context.Users.Where(u => u.Role == UserRoles.Manager || u.Role == UserRoles.Owner),
                    "Id",
                    "Name",
                    managerId);
            }
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
}
