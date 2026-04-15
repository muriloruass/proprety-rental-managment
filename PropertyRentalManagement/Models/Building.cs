using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PropertyRentalManagement.Models
{
    public class Building
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }

        // Navigation
        public virtual ICollection<Apartment>? Apartments { get; set; }
    }
}
