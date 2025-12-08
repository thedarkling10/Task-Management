using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class ApplicationUser : IdentityUser 
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        //relatii:
        //proiecte create daca e organizator

        public ICollection<Project> OrganizedProjects { get; set; }

        //proiecte la care e membru
        public ICollection<ProjectMember> ProjectMemberships{ get; set; }

        // taskuri asignate
        public ICollection<Task> AssignedTasks { get; set; }

        //invitatii primite
        public ICollection<ProjectInvitation> Invitations { get; set; }

    }
}
