using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.Models
{
    public class Summary
    {
        [Key]
        public int Id { get; set; }

<<<<<<< HEAD
        public int ProjectId { get; set; }

=======
        [Required]
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        [Required]
>>>>>>> 7b94f914442b998218c85c5a8eb48d8790678d15
        public string Content { get; set; }

        public DateTime Date { get; set; }

<<<<<<< HEAD
        public virtual Project Project { get; set; }
=======
       
>>>>>>> 7b94f914442b998218c85c5a8eb48d8790678d15
    }
}