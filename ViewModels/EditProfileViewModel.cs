using System.ComponentModel.DataAnnotations;

namespace SJOB_EXE201.ViewModels
{
    public class EditProfileViewModel
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        public string Avatar { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(255)]
        public string Headline { get; set; }

        public int? ExperienceYears { get; set; }

        public string Education { get; set; }

        public string Skills { get; set; }

        [StringLength(255)]
        public string DesiredPosition { get; set; }

        public decimal? DesiredSalary { get; set; }

        [StringLength(255)]
        public string DesiredLocation { get; set; }

        public string Availability { get; set; }

        public string Bio { get; set; }
    }

}
