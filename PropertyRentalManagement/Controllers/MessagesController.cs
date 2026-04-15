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
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Messages
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();
            var messagesQuery = _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.SentAt)
                .AsQueryable();

            if (User.IsInRole(UserRoles.Tenant))
            {
                messagesQuery = messagesQuery.Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId);
            }

            return View(await messagesQuery.ToListAsync());
        }

        // GET: Messages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null) return NotFound();
            if (!CanAccessMessage(message)) return Forbid();

            if (!message.IsRead && message.ReceiverId == GetCurrentUserId())
            {
                message.IsRead = true;
                _context.Update(message);
                await _context.SaveChangesAsync();
            }

            return View(message);
        }

        // GET: Messages/Create
        public IActionResult Create()
        {
            var currentUserId = GetCurrentUserId();
            var receiverQuery = GetAllowedReceiversQuery(currentUserId);
            ViewBag.ReceiverId = new SelectList(receiverQuery.ToList(), "Id", "Name");
            return View();
        }

        // POST: Messages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Message message)
        {
            var currentUserId = GetCurrentUserId();
            message.SenderId = currentUserId;
            var receiverAllowed = await GetAllowedReceiversQuery(currentUserId).AnyAsync(u => u.Id == message.ReceiverId);
            if (!receiverAllowed)
            {
                ModelState.AddModelError(nameof(message.ReceiverId), "You cannot send message to this user.");
            }

            if (ModelState.IsValid)
            {
                message.SentAt = DateTime.Now;
                _context.Add(message);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ReceiverId = new SelectList(await GetAllowedReceiversQuery(currentUserId).ToListAsync(), "Id", "Name", message.ReceiverId);
            return View(message);
        }

        // GET: Messages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message == null) return NotFound();
            if (!CanAccessMessage(message)) return Forbid();

            return View(message);
        }

        // POST: Messages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null) return RedirectToAction(nameof(Index));
            if (!CanAccessMessage(message)) return Forbid();

            _context.Messages.Remove(message);
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

        private IQueryable<User> GetAllowedReceiversQuery(int currentUserId)
        {
            if (User.IsInRole(UserRoles.Tenant))
            {
                return _context.Users.Where(u =>
                    u.Id != currentUserId &&
                    (u.Role == UserRoles.Manager || u.Role == UserRoles.Owner));
            }

            return _context.Users.Where(u => u.Id != currentUserId);
        }

        private bool CanAccessMessage(Message message)
        {
            if (User.IsInRole(UserRoles.Owner) || User.IsInRole(UserRoles.Manager))
            {
                return true;
            }

            var currentUserId = GetCurrentUserId();
            return message.SenderId == currentUserId || message.ReceiverId == currentUserId;
        }
    }
}
