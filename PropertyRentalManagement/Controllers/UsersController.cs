using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagement.Data;
using PropertyRentalManagement.Models;
using PropertyRentalManagement.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace PropertyRentalManagement.Controllers
{
    [Authorize(Roles = UserRoles.Owner)]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            UserRoles.Owner,
            UserRoles.Manager,
            UserRoles.Tenant
        };

        public UsersController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<IActionResult> Index(string? search, string? role)
        {
            var usersQuery = _context.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                usersQuery = usersQuery.Where(u => u.Name.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(role) && ValidRoles.Contains(role))
            {
                var normalizedRole = ValidRoles.First(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
                usersQuery = usersQuery.Where(u => u.Role == normalizedRole);
            }

            ViewBag.Search = search;
            ViewBag.Role = role;
            return View(await usersQuery.OrderBy(u => u.Name).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null) return NotFound();

            return View(user);
        }

        public IActionResult Create()
        {
            return View(new UserFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            var email = model.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Password is required.");
            }
            if (!ValidRoles.Contains(model.Role ?? string.Empty))
            {
                ModelState.AddModelError(nameof(model.Role), "Invalid role.");
            }
            if (!string.IsNullOrWhiteSpace(email) && await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email is already in use.");
            }

            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Name = model.Name.Trim(),
                    Email = email!,
                    Role = model.Role ?? UserRoles.Tenant
                };
                user.Password = _passwordHasher.HashPassword(user, model.Password);
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(new UserFormViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Password = string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (existingUser == null) return NotFound();

            var normalizedEmail = model.Email?.Trim().ToLowerInvariant();
            if (!ValidRoles.Contains(model.Role ?? string.Empty))
            {
                ModelState.AddModelError(nameof(model.Role), "Invalid role.");
            }
            if (!string.IsNullOrWhiteSpace(normalizedEmail) &&
                await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail && u.Id != id))
            {
                ModelState.AddModelError(nameof(model.Email), "Email is already in use.");
            }

            if (ModelState.IsValid)
            {
                existingUser.Name = model.Name.Trim();
                existingUser.Email = normalizedEmail!;
                existingUser.Role = model.Role ?? UserRoles.Tenant;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    existingUser.Password = _passwordHasher.HashPassword(existingUser, model.Password);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            model.Password = string.Empty;
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var parsedId)
                ? parsedId
                : 0;
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            if (user.Role == UserRoles.Owner)
            {
                var ownerCount = await _context.Users.CountAsync(u => u.Role == UserRoles.Owner);
                if (ownerCount <= 1)
                {
                    TempData["Error"] = "At least one owner account is required.";
                    return RedirectToAction(nameof(Index));
                }
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
