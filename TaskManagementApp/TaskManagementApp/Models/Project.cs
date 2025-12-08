using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime Date { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<Task> Tasks { get; set; }

    }
}