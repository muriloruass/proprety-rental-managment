using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace PropertyRentalManagement.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }

        public int ReceiverId { get; set; }
        [ForeignKey("ReceiverId")]
        public virtual User? Receiver { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}
