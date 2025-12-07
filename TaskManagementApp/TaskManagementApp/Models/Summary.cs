using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.Models
{
    public class Summary
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime Date { get; set; }

       
    }
}
