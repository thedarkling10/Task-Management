namespace TaskManagementApp.Models
{
    public class UserProject
    {
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        public bool IsAccepted { get; set; } = false; 
    }
}
