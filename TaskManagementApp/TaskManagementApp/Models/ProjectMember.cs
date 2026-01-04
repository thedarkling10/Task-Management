using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.Models
{
    //tabel asociativ pentru relatia  many to many intre Project si ApplicationUser
    public class ProjectMember
    {
        //public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public bool IsAccepted { get; set; } = false;
    }
}

