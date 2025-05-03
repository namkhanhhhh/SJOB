using SJOB_EXE201.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SJOB_EXE201.ViewModels
{
    public class WorkerViewModel
    {
        public User User { get; set; }
        public List<JobCategory> FeaturedCategories { get; set; }
        public List<JobPost> DiamondPosts { get; set; }
        public List<JobPost> MostViewedPosts { get; set; }
        public List<JobPost> AllPosts { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string Keyword { get; set; }
        public string Location { get; set; }
        public string JobType { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public int CategoryId { get; set; }
    }

    public class JobDetailsViewModel
    {
        public JobPost JobPost { get; set; }
        public List<JobPost> RelatedJobs { get; set; }
        public bool HasApplied { get; set; }
    }

    public class CategoryJobsViewModel
    {
        public JobCategory Category { get; set; }
        public List<JobPost> Jobs { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

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

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
