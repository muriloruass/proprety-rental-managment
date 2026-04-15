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

        public async Task<IActionResult> Index()
        {
            var appointments = _context.Appointments
                .Include(a => a.Apartment)
                .Include(a => a.User)
                .AsQueryable();

            if (User.IsInRole(UserRoles.Tenant))
            {
                var currentUserId = GetCurrentUserId();
                appointments = appointments.Where(a => a.UserId == currentUserId);
            }

            return View(await appointments.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Apartment)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null) return NotFound();
            if (User.IsInRole(UserRoles.Tenant) && appointment.UserId != GetCurrentUserId()) return Forbid();

            return View(appointment);
        }

        public IActionResult Create()
        {
            var apartments = _context.Apartments.Include(a => a.Building).Select(a => new {
                Id = a.Id,
                DisplayText = "Apt " + a.AptNumber + " - " + (a.Building != null ? a.Building.Name : "No Building")
            }).ToList();
            ViewBag.ApartmentId = new SelectList(apartments, "Id", "DisplayText");
            var canSelectUser = !User.IsInRole(UserRoles.Tenant);
            ViewBag.CanSelectUser = canSelectUser;
            if (canSelectUser)
            {
                ViewBag.UserId = new SelectList(_context.Users.Where(u => u.Role == UserRoles.Tenant), "Id", "Name");
            }
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
            }
            else
            {
                var tenantExists = await _context.Users.AnyAsync(u => u.Id == appointment.UserId && u.Role == UserRoles.Tenant);
                if (!tenantExists)
                {
                    ModelState.AddModelError(nameof(appointment.UserId), "Selected user must be a tenant.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var apartments = _context.Apartments.Include(a => a.Building).Select(a => new {
                Id = a.Id,
                DisplayText = "Apt " + a.AptNumber + " - " + (a.Building != null ? a.Building.Name : "No Building")
            }).ToList();
            ViewBag.ApartmentId = new SelectList(apartments, "Id", "DisplayText", appointment.ApartmentId);
            ViewBag.CanSelectUser = !isTenant;
            if (!isTenant)
            {
                ViewBag.UserId = new SelectList(_context.Users.Where(u => u.Role == UserRoles.Tenant), "Id", "Name", appointment.UserId);
            }
            return View(appointment);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            if (User.IsInRole(UserRoles.Tenant) && appointment.UserId != GetCurrentUserId()) return Forbid();

            var apartments = _context.Apartments.Include(a => a.Building).Select(a => new {
                Id = a.Id,
                DisplayText = "Apt " + a.AptNumber + " - " + (a.Building != null ? a.Building.Name : "No Building")
            }).ToList();
            ViewBag.ApartmentId = new SelectList(apartments, "Id", "DisplayText", appointment.ApartmentId);
            var canSelectUser = !User.IsInRole(UserRoles.Tenant);
            ViewBag.CanSelectUser = canSelectUser;
            if (canSelectUser)
            {
                ViewBag.UserId = new SelectList(_context.Users.Where(u => u.Role == UserRoles.Tenant), "Id", "Name", appointment.UserId);
            }
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

            if (!isTenant)
            {
                var tenantExists = await _context.Users.AnyAsync(u => u.Id == appointment.UserId && u.Role == UserRoles.Tenant);
                if (!tenantExists)
                {
                    ModelState.AddModelError(nameof(appointment.UserId), "Selected user must be a tenant.");
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
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var apartments = _context.Apartments.Include(a => a.Building).Select(a => new {
                Id = a.Id,
                DisplayText = "Apt " + a.AptNumber + " - " + (a.Building != null ? a.Building.Name : "No Building")
            }).ToList();
            ViewBag.ApartmentId = new SelectList(apartments, "Id", "DisplayText", appointment.ApartmentId);
            ViewBag.CanSelectUser = !isTenant;
            if (!isTenant)
            {
                ViewBag.UserId = new SelectList(_context.Users.Where(u => u.Role == UserRoles.Tenant), "Id", "Name", appointment.UserId);
            }
            return View(appointment);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Apartment)
                .Include(a => a.User)
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
    }
}
