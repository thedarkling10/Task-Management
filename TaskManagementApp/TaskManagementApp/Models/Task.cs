using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul task-ului este obligatoriu.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Descrierea task-ului este obligatorie.")]
        public string Description { get; set; }

        //data de start si de end nu stiu daca trebuie sa fie obligatorii
        [Required(ErrorMessage = "Data de început este obligatorie.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "Data de finalizare este obligatorie.")]
        public DateTime EndDate { get; set; }

        //continutul este optional
        public string? Content { get; set; }
        public string Status { get; set; }
      
        //cheie straina catre userul aignat
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        //relatie de navigare catre comentarii
        public ICollection<Comment> Comments { get; set; }



    }
}
