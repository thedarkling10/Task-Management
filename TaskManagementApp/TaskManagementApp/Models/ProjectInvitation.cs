using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.Models
{
    public class ProjectInvitation
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        [Required]
        public string InvitedUserId { get; set; }
        public ApplicationUser InvitedUser { get; set; }

        // Poate fi un enum sau o constantă string: "Pending", "Accepted", "Rejected"
        [Required]
        public string Status { get; set; }

        public string OrganizerId { get; set; }
        public ApplicationUser Organizer { get; set; }

        public DateTime DateSent { get; set; } = DateTime.Now;
    }
}
