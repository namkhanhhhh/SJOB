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
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } // application, job_post, system, etc.

        public int? ReferenceId { get; set; } // ID of the related entity (application, job post, etc.)

        [Required]
        [StringLength(50)]
        public string ReferenceType { get; set; } // The type of entity referenced (application, job_post, etc.)

        [Required]
        [StringLength(255)]
        public string Url { get; set; } // URL to navigate to when clicking the notification

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property
        public virtual User User { get; set; }
    }
}
