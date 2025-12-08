using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementApp.Models
{
    public class User
    {
    [Key]
    public int Id { get; set; }

    public string Surname { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }


    }
}
