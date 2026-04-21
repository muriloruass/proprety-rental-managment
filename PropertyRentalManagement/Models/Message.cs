using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace PropertyRentalManagement.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)] // FIXED: Server-side model validation (data annotations)
        public int SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }

        [Range(1, int.MaxValue)]
        public int ReceiverId { get; set; }
        [ForeignKey("ReceiverId")]
        public virtual User? Receiver { get; set; }

        [Required] // FIXED: Server-side model validation (data annotations)
        [StringLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}
