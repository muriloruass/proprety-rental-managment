using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PropertyRentalManagement.Models
{
    public class Building
    {
        public int Id { get; set; }

        [Required] // FIXED: Server-side model validation (data annotations)
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }
        [StringLength(80)]
        public string? City { get; set; }
        [StringLength(40)]
        public string? State { get; set; }
        [StringLength(12)]
        public string? ZipCode { get; set; }

        // Navigation
        public virtual ICollection<Apartment>? Apartments { get; set; }
    }
}
