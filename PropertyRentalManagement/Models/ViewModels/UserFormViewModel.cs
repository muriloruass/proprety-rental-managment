using System.ComponentModel.DataAnnotations;
using PropertyRentalManagement.Models;

namespace PropertyRentalManagement.Models.ViewModels
{
    public class UserFormViewModel
    {
        public int Id { get; set; }

        [Required] // FIXED: ViewModels used properly
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(120)]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = UserRoles.Tenant;
    }
}
