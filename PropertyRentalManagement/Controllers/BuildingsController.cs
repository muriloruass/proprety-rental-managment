using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagement.Data;
using PropertyRentalManagement.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PropertyRentalManagement.Controllers
{
    [Authorize]
    public class BuildingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BuildingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Buildings.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var building = await _context.Buildings
                .Include(b => b.Apartments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (building == null) return NotFound();

            return View(building);
        }

        [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,City,State,ZipCode")] Building building)
        {
            ModelState.Remove("Apartments");
            if (ModelState.IsValid)
            {
                _context.Add(building);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(building);
        }

        [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var building = await _context.Buildings.FindAsync(id);
            if (building == null) return NotFound();

            return View(building);
        }

        [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,City,State,ZipCode")] Building building)
        {
            if (id != building.Id) return NotFound();

            ModelState.Remove("Apartments");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(building);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Buildings.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(building);
        }

        [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var building = await _context.Buildings
                .FirstOrDefaultAsync(m => m.Id == id);

            if (building == null) return NotFound();

            return View(building);
        }

        [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building != null) _context.Buildings.Remove(building);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
