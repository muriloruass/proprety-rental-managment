using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagement.Data;
using PropertyRentalManagement.Models;
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

        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            var email = user.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(user.Name))
            {
                ModelState.AddModelError(nameof(user.Name), "Name is required.");
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(nameof(user.Email), "Email is required.");
            }
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                ModelState.AddModelError(nameof(user.Password), "Password is required.");
            }
            if (!ValidRoles.Contains(user.Role ?? string.Empty))
            {
                ModelState.AddModelError(nameof(user.Role), "Invalid role.");
            }
            if (!string.IsNullOrWhiteSpace(email) && await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
            {
                ModelState.AddModelError(nameof(user.Email), "Email is already in use.");
            }

            if (ModelState.IsValid)
            {
                user.Name = user.Name.Trim();
                user.Email = email!;
                user.Password = _passwordHasher.HashPassword(user, user.Password);
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Password = string.Empty;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id) return NotFound();

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (existingUser == null) return NotFound();

            var normalizedEmail = user.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(user.Name))
            {
                ModelState.AddModelError(nameof(user.Name), "Name is required.");
            }
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                ModelState.AddModelError(nameof(user.Email), "Email is required.");
            }
            if (!ValidRoles.Contains(user.Role ?? string.Empty))
            {
                ModelState.AddModelError(nameof(user.Role), "Invalid role.");
            }
            if (!string.IsNullOrWhiteSpace(normalizedEmail) &&
                await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail && u.Id != id))
            {
                ModelState.AddModelError(nameof(user.Email), "Email is already in use.");
            }

            if (ModelState.IsValid)
            {
                existingUser.Name = user.Name.Trim();
                existingUser.Email = normalizedEmail!;
                existingUser.Role = user.Role ?? UserRoles.Tenant;

                if (!string.IsNullOrWhiteSpace(user.Password))
                {
                    existingUser.Password = _passwordHasher.HashPassword(existingUser, user.Password);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            user.Password = string.Empty;
            return View(user);
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
