using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyRentalManagement.Models
{
    public class Apartment
    {
        public int Id { get; set; }

        [Required]
        public string AptNumber { get; set; }

        public int BuildingId { get; set; }

        [ForeignKey("BuildingId")]
        public virtual Building? Building { get; set; }

        public int Rooms { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Bathrooms { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Rent { get; set; }

        public string Status { get; set; } = "Available";
    }
}
