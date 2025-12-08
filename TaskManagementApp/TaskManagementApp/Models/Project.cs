using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

<<<<<<< HEAD
        public string Title { get; set; }

=======
        [Required(ErrorMessage = "Titlul proiectului este obligatoriu.")]
        [StringLength(100, ErrorMessage = "Titlul nu poate depăși 100 de caractere.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Descrierea proiectului este obligatorie.")]
>>>>>>> 7b94f914442b998218c85c5a8eb48d8790678d15
        public string Description { get; set; }

        public DateTime Date { get; set; }

<<<<<<< HEAD
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<Task> Tasks { get; set; }

=======
        [Required]
        //cheie straina catre organizatorul proiectului
        public string OrganizerId { get; set; }

        public virtual ApplicationUser Organizer { get; set; }

        public virtual ICollection<Task> Tasks { get; set; }

        public ICollection<ProjectMember> ProjectMembers { get; set; }
        public ICollection<Summary> ProjectSummaries { get; set; } // Pentru AI Summary
        public ICollection<ProjectInvitation> Invitations { get; set; } // Invitațiile trimise din acest proiect

>>>>>>> 7b94f914442b998218c85c5a8eb48d8790678d15
    }
}