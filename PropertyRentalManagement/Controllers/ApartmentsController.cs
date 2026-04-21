using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagement.Data;
using PropertyRentalManagement.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PropertyRentalManagement.Controllers
{
    [Authorize]
    public class ApartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? status)
        {
            var apartments = _context.Apartments.Include(a => a.Building).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                apartments = apartments.Where(a =>
                    a.AptNumber.ToLower().Contains(term) ||
                    (a.Building != null && a.Building.Name.ToLower().Contains(term))); // FIXED: Search/view apartments
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                apartments = apartments.Where(a => a.Status == status);
            }

            ViewBag.Search = search;
            ViewBag.Status = status;
            return View(await apartments.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var apartment = await _context.Apartments
                .Include(a => a.Building)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (apartment == null) return NotFound();

            return View(apartment);
        }

        [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
        public IActionResult Create()
        {
            ViewBag.BuildingId = new SelectList(_context.Buildings, "Id", "Name");
            return View();
        }

        [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Apartment apartment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(apartment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.BuildingId = new SelectList(_context.Buildings, "Id", "Name", apartment.BuildingId);
            return View(apartment);
        }

        [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var apartment = await _context.Apartments.FindAsync(id);
            if (apartment == null) return NotFound();

            ViewBag.BuildingId = new SelectList(_context.Buildings, "Id", "Name", apartment.BuildingId);
            return View(apartment);
        }

        [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Apartment apartment)
        {
            if (id != apartment.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(apartment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Apartments.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.BuildingId = new SelectList(_context.Buildings, "Id", "Name", apartment.BuildingId);
            return View(apartment);
        }

        [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var apartment = await _context.Apartments
                .Include(a => a.Building)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (apartment == null) return NotFound();

            return View(apartment);
        }

        [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var apartment = await _context.Apartments.FindAsync(id);
            if (apartment != null) _context.Apartments.Remove(apartment);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
