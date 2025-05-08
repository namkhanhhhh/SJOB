using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SJOB_EXE201.Models
{
    [Table("wishlists")]
    public class Wishlist
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("job_post_id")]
        public int JobPostId { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("JobPostId")]
        public virtual JobPost JobPost { get; set; }
    }
}