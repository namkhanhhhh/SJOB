using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SJOB_EXE201.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public string Type { get; set; } // "application", "system", etc.

        public int? ReferenceId { get; set; } // ID of the related entity (e.g., ApplicationId)

        public string ReferenceType { get; set; } // Type of the reference (e.g., "Application")

        public string Url { get; set; } // URL to navigate to when clicked

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property
        public virtual User User { get; set; }
    }
}
