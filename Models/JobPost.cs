﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SJOB_EXE201.Models;

public partial class JobPost
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required(ErrorMessage = "Tiêu đề không được bỏ trống")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Tiêu đề phải từ 10 đến 100 ký tự")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Mô tả không được bỏ trống")]
    public string Description { get; set; } = null!;

    [StringLength(500)]
    public string? Requirements { get; set; }

    [StringLength(500)]
    public string? Benefits { get; set; }

    public string? Location { get; set; }

    [Range(10000, 100000000, ErrorMessage = "Lương tối thiểu phải là 10.000 đồng")]
    public decimal? SalaryMin { get; set; }

    [Range(11000, 100000000, ErrorMessage = "Lương tối đa phải trên 10.000 đồng")]
    public decimal? SalaryMax { get; set; }

    [StringLength(20, ErrorMessage = "tối đa 20 kí tự")]
    [Column(TypeName = ("NVARCHAR(255)"))]
    public string? JobType { get; set; }

    [Column(TypeName=("NVARCHAR(255)"))]
    public string? ExperienceLevel { get; set; }

    public DateOnly? Deadline { get; set; }

    public string ImageMain { get; set; } = "iamge";

    public string? Image2 { get; set; }

    public string? Image3 { get; set; }

    public string? Image4 { get; set; }

    public string? PostType { get; set; }

    public string? Status { get; set; }

    public int? PriorityLevel { get; set; }

    public int? ViewCount { get; set; }

    public bool? IsFeatured { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? PushedToTopUntil { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<JobPostCategory> JobPostCategories { get; set; } = new List<JobPostCategory>();

    public virtual ICollection<ServiceUsage> ServiceUsages { get; set; } = new List<ServiceUsage>();

    public virtual User? User { get; set; } = null!;

    public virtual ICollection<WorkerVisit> WorkerVisits { get; set; } = new List<WorkerVisit>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
