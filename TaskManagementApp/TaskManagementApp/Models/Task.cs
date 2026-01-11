using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        //data de start si de end nu stiu daca trebuie sa fie obligatorii
        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; set; }
        [Required(ErrorMessage = "Content is required.")]
        //continutul este optional
        public string? Content { get; set; }
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; }
      
        //cheie straina catre userul aignat
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public int? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        //relatie de navigare catre comentarii
        public ICollection<Comment>? Comments { get; set; }



    }
}
