using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagement.Data;
using PropertyRentalManagement.Models.ViewModels;
using System.Threading.Tasks;

namespace PropertyRentalManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalBuildings = await _context.Buildings.CountAsync(),
                TotalApartments = await _context.Apartments.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalAppointments = await _context.Appointments.CountAsync()
            };
            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
