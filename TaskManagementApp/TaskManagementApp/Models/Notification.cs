using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } 

        public string? Link { get; set; } //link unde duce notificarea

        public string Type { get; set; } //invite sau comment

        public int? RelatedEntityId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        //destinatar
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        //expeditor
        public string? SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual ApplicationUser? Sender { get; set; }
    }
}
