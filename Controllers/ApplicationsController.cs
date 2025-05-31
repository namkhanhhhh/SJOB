using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Filters;
using SJOB_EXE201.Models;
using SJOB_EXE201.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SJOB_EXE201.Controllers
{
    [AuthorizationRequired(Roles = "Employer")]
    public class ApplicationsController : Controller
    {
        private readonly SjobContext _context;

        public ApplicationsController(SjobContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("UserId claim is missing or user not authenticated.");

            return int.Parse(userIdClaim);
        }

        // GET: Application/Index
        [HttpGet]
        public async Task<IActionResult> Index(int? jobId = null, string status = null, string search = null)
        {
            var userId = GetCurrentUserId();

            // Get all job posts by this employer
            var jobPosts = await _context.JobPosts
                .Where(jp => jp.UserId == userId)
                .OrderByDescending(jp => jp.CreatedAt)
                .ToListAsync();

            // Build query for applications
            var query = _context.Applications
                .Include(a => a.JobPost)
                .Include(a => a.User)
                .ThenInclude(u => u.UserDetails)
                .Where(a => a.JobPost.UserId == userId);

            // Apply filters
            if (jobId.HasValue)
            {
                query = query.Where(a => a.JobPostId == jobId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a =>
                    a.User.Username.Contains(search) ||
                    a.User.UserDetails.Any(ud =>
                        ud.FirstName.Contains(search) ||
                        ud.LastName.Contains(search)));
            }

            // Get applications
            var applications = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // Count applications by status
            var pendingCount = await query.CountAsync(a => a.Status == "pending");
            var acceptedCount = await query.CountAsync(a => a.Status == "accepted");
            var rejectedCount = await query.CountAsync(a => a.Status == "rejected");
            var totalCount = await query.CountAsync();

            // Create view model
            var viewModel = new ApplicationsViewModel
            {
                JobPosts = jobPosts,
                Applications = applications,
                SelectedJobId = jobId,
                SelectedStatus = status,
                SearchTerm = search,
                PendingCount = pendingCount,
                AcceptedCount = acceptedCount,
                RejectedCount = rejectedCount,
                TotalCount = totalCount
            };

            //lấy số tiền của người dùng
            var userCredit = await _context.UserCredits.FirstOrDefaultAsync(x => x.UserId == userId);
            if (userCredit != null)
            {
                ViewData["Balance"] = userCredit.Balance / 1000;
            }
            else
            {
                ViewData["Balance"] = 0;
            }

            return View(viewModel);
        }

        // GET: Application/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();

            var application = await _context.Applications
                .Include(a => a.JobPost)
                .Include(a => a.User)
                .ThenInclude(u => u.UserDetails)
                .Include(a => a.ApplicationNotes)
                .FirstOrDefaultAsync(a => a.Id == id && a.JobPost.UserId == userId);

            if (application == null)
            {
                return NotFound();
            }

            return View(application);
        }

        // POST: Application/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string note)
        {
            var userId = GetCurrentUserId();

            var application = await _context.Applications
                .Include(a => a.JobPost)
                .FirstOrDefaultAsync(a => a.Id == id && a.JobPost.UserId == userId);

            if (application == null)
            {
                return NotFound();
            }

            // Update status
            application.Status = status;

            // Add note if provided
            if (!string.IsNullOrEmpty(note))
            {
                var applicationNote = new ApplicationNote
                {
                    ApplicationId = application.Id,
                    Note = note,
                    CreatedAt = DateTime.Now
                };

                _context.ApplicationNotes.Add(applicationNote);
            }

            // Create notification for the worker
            await NotificationController.CreateNotification(
                _context,
                application.UserId,
                $"Đơn ứng tuyển của bạn đã được cập nhật",
                status == "accepted"
                    ? $"Đơn ứng tuyển của bạn cho vị trí {application.JobPost.Title} đã được chấp nhận."
                    : $"Đơn ứng tuyển của bạn cho vị trí {application.JobPost.Title} đã bị từ chối.",
                "application",
                application.Id,
                "application",
                $"/Worker/MyApplications"
            );

            await _context.SaveChangesAsync();

            TempData["Success"] = "Trạng thái đơn ứng tuyển đã được cập nhật thành công.";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // POST: Application/AddNote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int applicationId, string note)
        {
            var userId = GetCurrentUserId();

            var application = await _context.Applications
                .Include(a => a.JobPost)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPost.UserId == userId);

            if (application == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(note))
            {
                var applicationNote = new ApplicationNote
                {
                    ApplicationId = applicationId,
                    Note = note,
                    CreatedAt = DateTime.Now
                };

                _context.ApplicationNotes.Add(applicationNote);

                // Create notification for the worker about the new note
                await NotificationController.CreateNotification(
                    _context,
                    application.UserId,
                    "Ghi chú mới cho đơn ứng tuyển của bạn",
                    $"Nhà tuyển dụng đã thêm ghi chú mới cho đơn ứng tuyển vị trí {application.JobPost.Title}.",
                    "application_note",
                    applicationId,
                    "application",
                    $"/Worker/MyApplications"
                );

                await _context.SaveChangesAsync();

                TempData["Success"] = "Ghi chú đã được thêm thành công.";
            }

            return RedirectToAction(nameof(Details), new { id = applicationId });
        }

        // GET: Application/JobStats/5
        [HttpGet]
        public async Task<IActionResult> JobStats(int id)
        {
            var userId = GetCurrentUserId();

            var jobPost = await _context.JobPosts
                .Include(jp => jp.Applications)
                .FirstOrDefaultAsync(jp => jp.Id == id && jp.UserId == userId);

            if (jobPost == null)
            {
                return NotFound();
            }

            // Get application statistics
            var totalApplications = jobPost.Applications.Count;
            var pendingApplications = jobPost.Applications.Count(a => a.Status == "pending");
            var acceptedApplications = jobPost.Applications.Count(a => a.Status == "accepted");
            var rejectedApplications = jobPost.Applications.Count(a => a.Status == "rejected");

            // Get view statistics
            var totalViews = await _context.WorkerVisits
                .Where(v => v.JobPostId == id)
                .CountAsync();

            var uniqueViewers = await _context.WorkerVisits
                .Where(v => v.JobPostId == id)
                .Select(v => v.UserId)
                .Distinct()
                .CountAsync();

            // Calculate conversion rate (applications / unique viewers)
            var conversionRate = uniqueViewers > 0
                ? (double)totalApplications / uniqueViewers * 100
                : 0;

            // Create view model
            var viewModel = new JobStatsViewModel
            {
                JobPost = jobPost,
                TotalApplications = totalApplications,
                PendingApplications = pendingApplications,
                AcceptedApplications = acceptedApplications,
                RejectedApplications = rejectedApplications,
                TotalViews = totalViews,
                UniqueViewers = uniqueViewers,
                ConversionRate = conversionRate
            };

            return View(viewModel);
        }
    }
}
