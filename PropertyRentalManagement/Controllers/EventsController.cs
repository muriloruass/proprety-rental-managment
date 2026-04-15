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
    [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.ReportedBy)
                .Include(e => e.Apartment)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();
            return View(events);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.ReportedBy)
                .Include(e => e.Apartment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            var apartments = _context.Apartments.Include(a => a.Building).Select(a => new {
                Id = a.Id,
                DisplayText = "Apt " + a.AptNumber + " - " + (a.Building != null ? a.Building.Name : "No Building")
            }).ToList();
            ViewBag.ApartmentId = new SelectList(apartments, "Id", "DisplayText");
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event)
        {
            @event.ReportedById = GetCurrentUserId();
            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ApartmentId = new SelectList(_context.Apartments, "Id", "AptNumber", @event.ApartmentId);
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();

            var apartments = _context.Apartments.Include(a => a.Building).Select(a => new {
                Id = a.Id,
                DisplayText = "Apt " + a.AptNumber + " - " + (a.Building != null ? a.Building.Name : "No Building")
            }).ToList();
            ViewBag.ApartmentId = new SelectList(apartments, "Id", "DisplayText", @event.ApartmentId);
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event @event)
        {
            if (id != @event.Id) return NotFound();

            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (existingEvent == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingEvent.Title = @event.Title;
                existingEvent.Description = @event.Description;
                existingEvent.EventDate = @event.EventDate;
                existingEvent.Severity = @event.Severity;
                existingEvent.ApartmentId = @event.ApartmentId;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ApartmentId = new SelectList(_context.Apartments, "Id", "AptNumber", @event.ApartmentId);
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.ReportedBy)
                .Include(e => e.Apartment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null) _context.Events.Remove(@event);
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
