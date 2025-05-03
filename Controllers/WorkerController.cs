using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Models;
using SJOB_EXE201.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SJOB_EXE201.Controllers
{
    [Authorize(Roles = "Worker")]
    public class WorkerController : Controller
    {
        private readonly SjobContext _context;
        private readonly int _pageSize = 10;

        public WorkerController(SjobContext context)
        {
            _context = context;
        }

        [HttpGet]
        // Changed from Route attribute to regular action
        // This will respond to both the conventional route and the explicit route
        public async Task<IActionResult> Index(
            string keyword = "",
            string location = "",
            string jobType = "",
            decimal? minSalary = null,
            decimal? maxSalary = null,
            int categoryId = 0,
            int page = 1)
        {
            // Get current user id
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.Id == userId);

            // Get featured categories
            var featuredCategories = await _context.JobCategories
                .Where(c => c.ParentId == null)
                .Take(8)
                .ToListAsync();

            // Get diamond posts (highest priority)
            var diamondPosts = await _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.CompanyProfiles)
                .Where(p => p.PostType == "diamond" && p.Status == "active")
                .OrderByDescending(p => p.PriorityLevel)
                .ThenByDescending(p => p.PushedToTopUntil)
                .ThenByDescending(p => p.CreatedAt)
                .Take(6)
                .ToListAsync();

            // Get most viewed posts
            var mostViewedPosts = await _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.CompanyProfiles)
                .Where(p => p.Status == "active")
                .OrderByDescending(p => p.ViewCount)
                .Take(6)
                .ToListAsync();

            // Build query for all posts with filtering
            var query = _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.CompanyProfiles)
                .Include(p => p.JobPostCategories)
                .ThenInclude(pc => pc.Category)
                .Where(p => p.Status == "active")
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p =>
                    p.Title.Contains(keyword) ||
                    p.Description.Contains(keyword) ||
                    p.Requirements.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(p => p.Location.Contains(location));
            }

            if (!string.IsNullOrEmpty(jobType))
            {
                query = query.Where(p => p.JobType == jobType);
            }

            if (minSalary.HasValue)
            {
                query = query.Where(p => p.SalaryMin >= minSalary);
            }

            if (maxSalary.HasValue)
            {
                query = query.Where(p => p.SalaryMax <= maxSalary);
            }

            if (categoryId > 0)
            {
                query = query.Where(p => p.JobPostCategories.Any(pc => pc.CategoryId == categoryId));
            }

            // Order by priority and post type
            query = query.OrderByDescending(p => p.PostType == "diamond" ? 3 : p.PostType == "gold" ? 2 : 1)
                .ThenByDescending(p => p.PriorityLevel)
                .ThenByDescending(p => p.PushedToTopUntil)
                .ThenByDescending(p => p.CreatedAt);

            // Get total count for pagination
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)_pageSize);

            // Apply pagination
            var allPosts = await query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Create view model
            var viewModel = new WorkerViewModel
            {
                User = user,
                FeaturedCategories = featuredCategories,
                DiamondPosts = diamondPosts,
                MostViewedPosts = mostViewedPosts,
                AllPosts = allPosts,
                TotalPages = totalPages,
                CurrentPage = page,
                Keyword = keyword,
                Location = location,
                JobType = jobType,
                MinSalary = minSalary,
                MaxSalary = maxSalary,
                CategoryId = categoryId
            };

            return View(viewModel);
        }

        // Add a route attribute to redirect /HomePage to the Index action
        [HttpGet]
        [Route("/HomePage")]
        public IActionResult HomePage()
        {
            // Redirect to the Index action, which will maintain all the query parameters
            return RedirectToAction("Index");
        }

        // Rest of your controller remains unchanged
        // ...

        [HttpGet]
        public async Task<IActionResult> JobDetails(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var jobPost = await _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.CompanyProfiles)
                .Include(p => p.JobPostCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (jobPost == null)
            {
                return NotFound();
            }

            // Record the visit
            var existingVisit = await _context.WorkerVisits
                .FirstOrDefaultAsync(v => v.JobPostId == id && v.UserId == userId);

            if (existingVisit == null)
            {
                // First visit
                _context.WorkerVisits.Add(new WorkerVisit
                {
                    JobPostId = id,
                    UserId = userId,
                    VisitTime = DateTime.Now,
                    IsFirstView = true
                });

                // Increment view count
                jobPost.ViewCount++;
                await _context.SaveChangesAsync();
            }
            else
            {
                // Subsequent visit
                _context.WorkerVisits.Add(new WorkerVisit
                {
                    JobPostId = id,
                    UserId = userId,
                    VisitTime = DateTime.Now,
                    IsFirstView = false
                });
                await _context.SaveChangesAsync();
            }

            // Get related jobs
            var categoryIds = jobPost.JobPostCategories.Select(pc => pc.CategoryId).ToList();
            var relatedJobs = await _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.CompanyProfiles)
                .Include(p => p.JobPostCategories)
                .Where(p => p.Id != id && p.Status == "active" &&
                       p.JobPostCategories.Any(pc => categoryIds.Contains(pc.CategoryId)))
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();

            // Check if user has already applied
            var hasApplied = await _context.Applications
                .AnyAsync(a => a.JobPostId == id && a.UserId == userId);

            var viewModel = new JobDetailsViewModel
            {
                JobPost = jobPost,
                RelatedJobs = relatedJobs,
                HasApplied = hasApplied
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyJob(int jobId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Check if already applied
            var existingApplication = await _context.Applications
                .FirstOrDefaultAsync(a => a.JobPostId == jobId && a.UserId == userId);

            if (existingApplication != null)
            {
                TempData["Error"] = "You have already applied for this job.";
                return RedirectToAction("JobDetails", new { id = jobId });
            }

            // Create new application
            _context.Applications.Add(new Application
            {
                JobPostId = jobId,
                UserId = userId,
                Status = "pending",
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Your application has been submitted successfully!";
            return RedirectToAction("JobDetails", new { id = jobId });
        }

        [HttpGet]
        public async Task<IActionResult> CategoryJobs(int id, int page = 1)
        {
            var category = await _context.JobCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var query = _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.CompanyProfiles)
                .Include(p => p.JobPostCategories)
                .Where(p => p.Status == "active" &&
                       p.JobPostCategories.Any(pc => pc.CategoryId == id))
                .OrderByDescending(p => p.PostType == "diamond" ? 3 : p.PostType == "gold" ? 2 : 1)
                .ThenByDescending(p => p.PriorityLevel)
                .ThenByDescending(p => p.CreatedAt);

            // Get total count for pagination
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)_pageSize);

            // Apply pagination
            var categoryJobs = await query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            var viewModel = new CategoryJobsViewModel
            {
                Category = category,
                Jobs = categoryJobs,
                TotalPages = totalPages,
                CurrentPage = page
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var applications = await _context.Applications
                .Include(a => a.JobPost)
                .ThenInclude(p => p.User)
                .ThenInclude(u => u.CompanyProfiles)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(applications);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new EditProfileViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Avatar = user.Avatar,
                FirstName = user.UserDetails.FirstOrDefault()?.FirstName,
                LastName = user.UserDetails.FirstOrDefault()?.LastName,
                PhoneNumber = user.UserDetails.FirstOrDefault()?.PhoneNumber,
                Address = user.UserDetails.FirstOrDefault()?.Address,
                Headline = user.UserDetails.FirstOrDefault()?.Headline,
                ExperienceYears = user.UserDetails.FirstOrDefault()?.ExperienceYears,
                Education = user.UserDetails.FirstOrDefault()?.Education,
                Skills = user.UserDetails.FirstOrDefault()?.Skills,
                DesiredPosition = user.UserDetails.FirstOrDefault()?.DesiredPosition,
                DesiredSalary = user.UserDetails.FirstOrDefault()?.DesiredSalary,
                DesiredLocation = user.UserDetails.FirstOrDefault()?.DesiredLocation,
                Availability = user.UserDetails.FirstOrDefault()?.Availability,
                Bio = user.UserDetails.FirstOrDefault()?.Bio
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model, IFormFile avatarFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            // Update user info
            user.Username = model.Username;
            user.Email = model.Email;

            // Handle avatar upload
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/avatars", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                user.Avatar = "/images/avatars/" + fileName;
            }

            // Update or create user details
            var userDetail = user.UserDetails.FirstOrDefault();
            if (userDetail == null)
            {
                userDetail = new UserDetail
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.Now
                };
                _context.UserDetails.Add(userDetail);
            }

            userDetail.FirstName = model.FirstName;
            userDetail.LastName = model.LastName;
            userDetail.PhoneNumber = model.PhoneNumber;
            userDetail.Address = model.Address;
            userDetail.Headline = model.Headline;
            userDetail.ExperienceYears = model.ExperienceYears;
            userDetail.Education = model.Education;
            userDetail.Skills = model.Skills;
            userDetail.DesiredPosition = model.DesiredPosition;
            userDetail.DesiredSalary = model.DesiredSalary;
            userDetail.DesiredLocation = model.DesiredLocation;
            userDetail.Availability = model.Availability;
            userDetail.Bio = model.Bio;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Your profile has been updated successfully!";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Verify current password
            if (user.Password != HashPassword(model.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
                return View(model);
            }

            // Update password
            user.Password = HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your password has been changed successfully!";
            return RedirectToAction("Profile");
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }
    }
}