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

        public int UserId { get; set; }

        public string Content { get; set; }

        public DateTime Date { get; set; }

        public virtual Task Task { get; set; }

        public virtual User User { get; set; }
    }
}