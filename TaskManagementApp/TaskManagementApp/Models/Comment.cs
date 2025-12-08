using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }
<<<<<<< HEAD

        public int UserId { get; set; }

        public string Content { get; set; }

        public DateTime Date { get; set; }

        public virtual Task Task { get; set; }

        public virtual User User { get; set; }
=======
        public virtual Task Task { get; set; }

        //cheie straina catre userul care a postat comentariul
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required(ErrorMessage = "Conținutul comentariului este obligatoriu.")]
        public string Content { get; set; }

        public DateTime Date { get; set; }
        
>>>>>>> 7b94f914442b998218c85c5a8eb48d8790678d15
    }
}