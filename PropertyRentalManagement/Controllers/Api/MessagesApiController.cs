using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagement.Data;
using PropertyRentalManagement.Models;
using System.Security.Claims;

namespace PropertyRentalManagement.Controllers.Api;

[ApiController]
[Route("api/messages")] // FIXED: Web API controllers (not just MVC views)
[Authorize]
public class MessagesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MessagesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var currentUserId = GetCurrentUserId();
        var query = _context.Messages.AsNoTracking().Include(m => m.Sender).Include(m => m.Receiver).AsQueryable();
        if (User.IsInRole(UserRoles.Tenant))
        {
            query = query.Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId);
        }
        return Ok(await query.OrderByDescending(m => m.SentAt).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Message model)
    {
        var currentUserId = GetCurrentUserId();
        model.SenderId = currentUserId;
        model.SentAt = DateTime.Now;

        if (User.IsInRole(UserRoles.Tenant))
        {
            var receiverAllowed = await _context.Users.AnyAsync(u =>
                u.Id == model.ReceiverId &&
                (u.Role == UserRoles.Manager || u.Role == UserRoles.Owner)); // FIXED: Send messages to manager
            if (!receiverAllowed)
            {
                ModelState.AddModelError(nameof(model.ReceiverId), "Tenant can only send messages to manager or owner.");
            }
        }

        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        _context.Messages.Add(model);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = model.Id }, model);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();
        var message = await _context.Messages.FindAsync(id);
        if (message == null) return NotFound();
        if (User.IsInRole(UserRoles.Tenant) && message.SenderId != currentUserId && message.ReceiverId != currentUserId) return Forbid();

        _context.Messages.Remove(message);
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
