using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Filters;
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
            // Get current user if authenticated
            User user = null;
            List<int> wishlistJobIds = new List<int>();

            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                int userId = 0;
                if (userIdClaim != null)
                {
                    userId = int.Parse(userIdClaim.Value);
                }
                else
                {
                    // Handle the case when the claim is not found
                    // For example, redirect to login or set a default value
                    return RedirectToAction("Index", "Login");
                }
                user = await _context.Users
                    .Include(u => u.UserDetails)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                // Get user's wishlist job IDs
                wishlistJobIds = await _context.Wishlists
                    .Where(w => w.UserId == userId)
                    .Select(w => w.JobPostId)
                    .ToListAsync();

                ViewBag.WishlistJobIds = wishlistJobIds;
            }

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

        [HttpGet]
        public async Task<IActionResult> SearchResults(
    string keyword = "",
    string location = "",
    string jobType = "",
    decimal? minSalary = null,
    decimal? maxSalary = null,
    int categoryId = 0,
    int page = 1)
        {
            // Validate that at least one search parameter is provided
            bool hasSearchParameters =
                !string.IsNullOrWhiteSpace(keyword) ||
                !string.IsNullOrWhiteSpace(location) ||
                !string.IsNullOrWhiteSpace(jobType) ||
                minSalary.HasValue ||
                maxSalary.HasValue ||
                categoryId > 0;

            if (!hasSearchParameters)
            {
                return RedirectToAction("Index");
            }

            // Build query for jobs with filtering
            var query = _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.CompanyProfiles)
                .Include(p => p.JobPostCategories)
                .ThenInclude(pc => pc.Category)
                .Where(p => p.Status == "active")
                .AsQueryable();

            // Apply keyword search only if provided
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p =>
                    p.Title.Contains(keyword) ||
                    p.Description.Contains(keyword) ||
                    p.Requirements.Contains(keyword));
            }

            // Apply additional filters
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
            var searchResults = await query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Generate keyword suggestions based on job titles and categories
            var suggestedKeywords = await _context.JobPosts
                .Where(p => p.Status == "active" &&
                       (string.IsNullOrWhiteSpace(keyword) || p.Title.Contains(keyword) ||
                        p.Description.Contains(keyword) ||
                        p.Requirements.Contains(keyword)))
                .SelectMany(p => p.JobPostCategories.Select(pc => pc.Category.Name))
                .Distinct()
                .Take(10)
                .ToListAsync();

            // Add some keywords from job titles if we don't have enough
            if (suggestedKeywords.Count < 5)
            {
                var titleKeywords = await _context.JobPosts
                    .Where(p => p.Status == "active" &&
                          (string.IsNullOrWhiteSpace(keyword) || p.Title.Contains(keyword)))
                    .Select(p => p.Title)
                    .Take(10)
                    .ToListAsync();

                foreach (var title in titleKeywords)
                {
                    // Extract meaningful words from titles
                    var words = title.Split(' ')
                        .Where(w => w.Length > 3 && !suggestedKeywords.Contains(w))
                        .Take(3);

                    suggestedKeywords.AddRange(words);

                    if (suggestedKeywords.Count >= 10)
                        break;
                }
            }

            // Get popular categories with job counts
            var popularCategories = await _context.JobCategories
                .Where(c => c.JobPostCategories.Any(pc => pc.JobPost.Status == "active"))
                .Select(c => new SearchResultsViewModel.CategoryWithCount
                {
                    Id = c.Id,
                    Name = c.Name,
                    JobCount = c.JobPostCategories.Count(pc => pc.JobPost.Status == "active")
                })
                .OrderByDescending(c => c.JobCount)
                .Take(8)
                .ToListAsync();

            // Create view model
            var viewModel = new SearchResultsViewModel
            {
                Keyword = keyword,
                Location = location,
                JobType = jobType,
                MinSalary = minSalary,
                MaxSalary = maxSalary,
                CategoryId = categoryId,
                Jobs = searchResults,
                TotalJobs = totalItems,
                CurrentPage = page,
                TotalPages = totalPages,
                SuggestedKeywords = suggestedKeywords.Distinct().Take(10).ToList(),
                PopularCategories = popularCategories
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

        [AuthorizationRequired(Roles = "Worker")]
        [HttpGet]
        public async Task<IActionResult> Wishlist()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int userId = 0;
            if (userIdClaim != null)
            {
                userId = int.Parse(userIdClaim.Value);
            }
            else
            {
                // Handle the case when the claim is not found
                // For example, redirect to login or set a default value
                return RedirectToAction("Index", "Login");
            }
            var wishlistJobs = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Include(w => w.JobPost)
                .ThenInclude(p => p.User)
                .ThenInclude(u => u.CompanyProfiles)
                .Include(w => w.JobPost.JobPostCategories)
                .ThenInclude(pc => pc.Category)
                .Select(w => w.JobPost)
                .ToListAsync();

            var viewModel = new WishlistViewModel
            {
                WishlistJobs = wishlistJobs,
                TotalJobs = wishlistJobs.Count
            };

            return View(viewModel);
        }

        [AuthorizationRequired(Roles = "Worker")]
        [HttpPost]
        public async Task<IActionResult> ToggleWishlist(int jobId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Login");
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int userId = 0;
            if (userIdClaim != null)
            {
                userId = int.Parse(userIdClaim.Value);
            }
            else
            {
                // Handle the case when the claim is not found
                // For example, redirect to login or set a default value
                return RedirectToAction("Index", "Login");
            }
            // Check if job exists in wishlist
            var existingWishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.JobPostId == jobId);

            if (existingWishlistItem != null)
            {
                // Remove from wishlist
                _context.Wishlists.Remove(existingWishlistItem);
                await _context.SaveChangesAsync();
                return Json(new { isInWishlist = false });
            }
            else
            {
                // Add to wishlist
                var wishlistItem = new Wishlist
                {
                    UserId = userId,
                    JobPostId = jobId,
                    CreatedAt = DateTime.Now
                };

                _context.Wishlists.Add(wishlistItem);
                await _context.SaveChangesAsync();
                return Json(new { isInWishlist = true });
            }
        }

        [AuthorizationRequired(Roles = "Worker")]
        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int jobId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int userId = 0;
            if (userIdClaim != null)
            {
                userId = int.Parse(userIdClaim.Value);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.JobPostId == jobId);

            if (wishlistItem != null)
            {
                _context.Wishlists.Remove(wishlistItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Job removed from wishlist.";
            }

            return RedirectToAction("Wishlist");
        }

        // Modify the JobDetails method to include wishlist status
        [HttpGet]
        public async Task<IActionResult> JobDetails(int id)
        {
            // Get current user id if authenticated
            int? userId = null;
            bool isAuthenticated = User.Identity.IsAuthenticated;
            bool isInWishlist = false;

            if (isAuthenticated)
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim != null)
                {
                    userId = int.Parse(userIdClaim.Value);
                }
                else
                {
                    // Handle the case when the claim is not found
                    // For example, redirect to login or set a default value
                    return RedirectToAction("Index", "Login");
                }
                // Check if job is in user's wishlist
                isInWishlist = await _context.Wishlists
                    .AnyAsync(w => w.UserId == userId.Value && w.JobPostId == id);
            }

            var jobPost = await _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.CompanyProfiles)
                .Include(p => p.User.UserDetails)
                .Include(p => p.JobPostCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (jobPost == null)
            {
                return NotFound();
            }

            // Record the visit and update view count only if user is authenticated
            if (isAuthenticated && userId.HasValue)
            {
                // Check if this is the first time this user is viewing this job post
                var existingVisit = await _context.WorkerVisits
                    .AnyAsync(v => v.JobPostId == id && v.UserId == userId.Value);

                if (!existingVisit)
                {
                    // This is the first visit - increment view count and mark as first view
                    _context.WorkerVisits.Add(new WorkerVisit
                    {
                        JobPostId = id,
                        UserId = userId.Value,
                        VisitTime = DateTime.Now,
                        IsFirstView = true
                    });

                    // Increment view count
                    jobPost.ViewCount++;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // This is a subsequent visit - record it but don't increment view count
                    _context.WorkerVisits.Add(new WorkerVisit
                    {
                        JobPostId = id,
                        UserId = userId.Value,
                        VisitTime = DateTime.Now,
                        IsFirstView = false
                    });
                    await _context.SaveChangesAsync();
                }
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

            // Check if user has already applied (only for authenticated users)
            bool hasApplied = false;
            if (isAuthenticated && userId.HasValue)
            {
                hasApplied = await _context.Applications
                    .AnyAsync(a => a.JobPostId == id && a.UserId == userId.Value);
            }

            var viewModel = new JobDetailsViewModel
            {
                JobPost = jobPost,
                RelatedJobs = relatedJobs,
                HasApplied = hasApplied,
                IsInWishlist = isInWishlist
            };

            return View(viewModel);
        }

        [AuthorizationRequired(Roles = "Worker")] // New
        [HttpPost]
        public async Task<IActionResult> ApplyJob(int jobId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int userId = 0;
            if (userIdClaim != null)
            {
                userId = int.Parse(userIdClaim.Value);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
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

        [AuthorizationRequired(Roles = "Worker")] // New
        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int userId = 0;
            if (userIdClaim != null)
            {
                userId = int.Parse(userIdClaim.Value);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            var applications = await _context.Applications
                .Include(a => a.JobPost)
                .ThenInclude(p => p.User)
                .ThenInclude(u => u.CompanyProfiles)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(applications);
        }

        [AuthorizationRequired(Roles = "Worker")] // New
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int userId = 0;
            if (userIdClaim != null)
            {
                userId = int.Parse(userIdClaim.Value);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            var user = await _context.Users
                .Include(u => u.UserDetails)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [AuthorizationRequired(Roles = "Worker")] // New
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int userId = 0;
            if (userIdClaim != null)
            {
                userId = int.Parse(userIdClaim.Value);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
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

        [AuthorizationRequired(Roles = "Worker")]
        [HttpPost]
        [ValidateAntiForgeryToken] // Add this to ensure form security
        public async Task<IActionResult> EditProfile(EditProfileViewModel model, IFormFile avatarFile)
        {
            // Debug information
            Console.WriteLine("EditProfile POST action called");
            Console.WriteLine($"Model state is valid: {ModelState.IsValid}");

            try
            {
                Console.WriteLine($"Processing profile update for user ID: {model.UserId}");

                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                int userId = 0;
                if (userIdClaim != null)
                {
                    userId = int.Parse(userIdClaim.Value);
                }
                else
                {
                    // Handle the case when the claim is not found
                    // For example, redirect to login or set a default value
                    return RedirectToAction("Index", "Login");
                }
                Console.WriteLine($"Current user ID from claims: {userId}");

                var user = await _context.Users
                    .Include(u => u.UserDetails)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    Console.WriteLine("User not found");
                    return NotFound();
                }

                Console.WriteLine("User found, updating profile");

                // Update user info
                user.Username = model.Username;
                user.Email = model.Email;

                // Handle avatar upload
                if (avatarFile != null && avatarFile.Length > 0)
                {
                    Console.WriteLine("Processing avatar upload");
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");

                    // Ensure directory exists
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    user.Avatar = "/images/avatars/" + fileName;
                    Console.WriteLine($"Avatar saved to: {user.Avatar}");
                }

                // Update or create user details
                var userDetail = user.UserDetails.FirstOrDefault();
                if (userDetail == null)
                {
                    Console.WriteLine("Creating new user details");
                    userDetail = new UserDetail
                    {
                        UserId = user.Id,
                        FirstName = model.FirstName ?? "",
                        LastName = model.LastName ?? "",
                        CreatedAt = DateTime.Now
                    };
                    _context.UserDetails.Add(userDetail);
                }
                else
                {
                    Console.WriteLine("Updating existing user details");
                    userDetail.FirstName = model.FirstName ?? userDetail.FirstName;
                    userDetail.LastName = model.LastName ?? userDetail.LastName;
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
                }

                // Save changes to database
                Console.WriteLine("Saving changes to database");
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChanges result: {result} entities modified");

                // Add success message
                TempData["Success"] = "Your profile has been updated successfully!";
                Console.WriteLine("Profile updated successfully");

                // Redirect to profile page
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error updating profile: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                // Add error message for user
                ModelState.AddModelError("", "An error occurred while updating your profile. Please try again.");
                TempData["Error"] = "Failed to update profile. Please try again.";

                // Return to form with current values
                return View(model);
            }
        }
        [AuthorizationRequired(Roles = "Worker")] // New
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [AuthorizationRequired(Roles = "Worker")] // New
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int userId = 0;
            if (userIdClaim != null)
            {
                userId = int.Parse(userIdClaim.Value);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
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

        [AuthorizationRequired(Roles = "Worker")] // New
        [HttpGet]
        public async Task<IActionResult> EmployerProfile(int id)
        {
            // Get employer user
            var employer = await _context.Users
                .Include(u => u.UserDetails)
                .Include(u => u.CompanyProfiles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (employer == null)
            {
                return NotFound();
            }

            // Get employer's job posts
            var jobPosts = await _context.JobPosts
                .Include(p => p.User)
                .ThenInclude(u => u.CompanyProfiles)
                .Include(p => p.JobPostCategories)
                .ThenInclude(pc => pc.Category)
                .Where(p => p.UserId == id && p.Status == "active")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Get wishlist job IDs if user is authenticated
            List<int> wishlistJobIds = new List<int>();
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                int userId = 0;
                if (userIdClaim != null)
                {
                    userId = int.Parse(userIdClaim.Value);
                }
                else
                {
                    // Handle the case when the claim is not found
                    // For example, redirect to login or set a default value
                    return RedirectToAction("Index", "Login");
                }
                wishlistJobIds = await _context.Wishlists
                    .Where(w => w.UserId == userId)
                    .Select(w => w.JobPostId)
                    .ToListAsync();
            }

            // Count profile views
            int profileViewCount = await _context.WorkerVisits
                .Where(v => v.JobPost.UserId == id)
                .Select(v => v.UserId)
                .Distinct()
                .CountAsync();

            // Record this visit if authenticated
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                int userId = 0;
                if (userIdClaim != null)
                {
                    userId = int.Parse(userIdClaim.Value);
                }
                else
                {
                    // Handle the case when the claim is not found
                    // For example, redirect to login or set a default value
                    return RedirectToAction("Index", "Login");
                }
                // We'll use the first job post to record the visit, or create a dummy record
                var firstJobPost = jobPosts.FirstOrDefault();
                if (firstJobPost != null)
                {
                    _context.WorkerVisits.Add(new WorkerVisit
                    {
                        JobPostId = firstJobPost.Id,
                        UserId = userId,
                        VisitTime = DateTime.Now,
                        IsFirstView = false
                    });
                    await _context.SaveChangesAsync();
                }
            }

            // Create view model
            var viewModel = new EmployerProfileViewModel
            {
                Employer = employer,
                CompanyProfile = employer.CompanyProfiles.FirstOrDefault(),
                TotalJobPosts = jobPosts.Count,
                JobPosts = jobPosts,
                ProfileViewCount = profileViewCount,
                JoinedDate = employer.UserDetails.FirstOrDefault()?.CreatedAt ?? DateTime.Now,
                WishlistJobIds = wishlistJobIds
            };

            return View(viewModel);
        }
    }
}