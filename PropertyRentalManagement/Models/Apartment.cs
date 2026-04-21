using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyRentalManagement.Models
{
    public class Apartment
    {
        public int Id { get; set; }

        [Required] // FIXED: Server-side model validation (data annotations)
        [StringLength(20)]
        public string AptNumber { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int BuildingId { get; set; }

        [ForeignKey("BuildingId")]
        public virtual Building? Building { get; set; }

        [Range(0, 20)]
        public int Rooms { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "10")]
        public decimal Bathrooms { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0", "1000000")]
        public decimal Rent { get; set; }

        [Required]
        [StringLength(40)]
        public string Status { get; set; } = "Available";
    }
}
