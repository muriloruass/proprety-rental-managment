using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace PropertyRentalManagement.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)] // FIXED: Server-side model validation (data annotations)
        public int ApartmentId { get; set; }

        [ForeignKey("ApartmentId")]
        public virtual Apartment? Apartment { get; set; }

        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required] // FIXED: Book appointment with manager
        [Range(1, int.MaxValue)] // FIXED: Book appointment with manager
        public int ManagerId { get; set; }

        [ForeignKey("ManagerId")] // FIXED: Book appointment with manager
        public virtual User? Manager { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Scheduled";

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
