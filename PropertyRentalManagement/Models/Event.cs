using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace PropertyRentalManagement.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime EventDate { get; set; } = DateTime.Now;

        public string Severity { get; set; } = "Low"; // Low, Medium, High

        public int ReportedById { get; set; }
        [ForeignKey("ReportedById")]
        public virtual User? ReportedBy { get; set; }

        public int? ApartmentId { get; set; }
        [ForeignKey("ApartmentId")]
        public virtual Apartment? Apartment { get; set; }
    }
}
