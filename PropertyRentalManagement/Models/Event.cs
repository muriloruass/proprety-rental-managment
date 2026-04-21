using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace PropertyRentalManagement.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required] // FIXED: Server-side model validation (data annotations)
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = "Low"; // Low, Medium, High

        public int ReportedById { get; set; }
        [ForeignKey("ReportedById")]
        public virtual User? ReportedBy { get; set; }

        public int? ApartmentId { get; set; }
        [ForeignKey("ApartmentId")]
        public virtual Apartment? Apartment { get; set; }
    }
}
