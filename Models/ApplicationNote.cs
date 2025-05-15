using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SJOB_EXE201.Models
{
    public class ApplicationNote
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Application")]
        public int ApplicationId { get; set; }

        [Required]
        public string Note { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property
        public virtual Application Application { get; set; }
    }
}
