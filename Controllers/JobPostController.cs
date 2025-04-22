using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Models;

namespace SJOB_EXE201.Controllers;
[Authorize]
public class JobPostController : Controller
{
    private readonly SjobContext _context;

    public JobPostController(SjobContext context)
    {
        _context = context;
    }

    // sử dụng lúc có tài khoản 
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("UserId claim is missing or user not authenticated.");

        return int.Parse(userIdClaim);
    }

    public async Task<IActionResult> Index()
    {
        int userId = GetCurrentUserId();

        var jobPosts = await _context.JobPosts
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.PriorityLevel)
            .ThenByDescending(j => j.PushedToTopUntil)
            .ToListAsync();

        return View(jobPosts);
    }

    public IActionResult Create()
    {
        return View(new JobPost());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobPost jobPost)
    {
        if (ModelState.IsValid)
        {
            jobPost.UserId = GetCurrentUserId();
            jobPost.CreatedAt = DateTime.Now;
            jobPost.Status = "Active";
            jobPost.ViewCount = 0;
            jobPost.IsFeatured = false;

            _context.Add(jobPost);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(jobPost);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var jobPost = await _context.JobPosts.FindAsync(id);
        if (jobPost == null || jobPost.UserId != GetCurrentUserId())
        {
            return NotFound();
        }

        return View(jobPost);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, JobPost jobPost)
    {
        if (id != jobPost.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var original = await _context.JobPosts.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id);
                if (original == null || original.UserId != GetCurrentUserId())
                {
                    return NotFound();
                }

                jobPost.UserId = original.UserId;
                jobPost.CreatedAt = original.CreatedAt;
                jobPost.Status = original.Status;
                jobPost.ViewCount = original.ViewCount;
                jobPost.IsFeatured = original.IsFeatured;

                _context.Update(jobPost);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.JobPosts.Any(j => j.Id == id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(jobPost);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var jobPost = await _context.JobPosts.FindAsync(id);
        if (jobPost == null || jobPost.UserId != GetCurrentUserId())
        {
            return NotFound();
        }

        _context.JobPosts.Remove(jobPost);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> PushToTop(int id)
    {
        var jobPost = await _context.JobPosts.FindAsync(id);
        if (jobPost == null || jobPost.UserId != GetCurrentUserId())
        {
            return NotFound();
        }

        jobPost.PushedToTopUntil = DateTime.Now;
        _context.Update(jobPost);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}