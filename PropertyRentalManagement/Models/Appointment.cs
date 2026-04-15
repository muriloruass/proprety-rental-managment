using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace PropertyRentalManagement.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public int ApartmentId { get; set; }

        [ForeignKey("ApartmentId")]
        public virtual Apartment? Apartment { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string Status { get; set; } = "Scheduled";

        public string Notes { get; set; }
    }
}
