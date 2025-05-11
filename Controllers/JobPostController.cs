using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Models;

namespace SJOB_EXE201.Controllers;
[Authorize(Roles = "Employer")]
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

    // view
    public async Task<IActionResult> Index(string? viewMode, string? search = null, DateTime? from = null, DateTime? to = null, string? status = null)
    {
        var userId = GetCurrentUserId();
        // Lưu viewMode vào session nếu có
        if (!string.IsNullOrEmpty(viewMode))
        {
            HttpContext.Session.SetString("viewMode", viewMode);
        }
        else
        {
            viewMode = HttpContext.Session.GetString("viewMode") ?? "recruiter";
        }

        ViewBag.ViewMode = viewMode;  // Lưu giá trị vào ViewBag

        var posts = _context.JobPosts.AsQueryable();

        // Nếu chế độ là "recruiter", chỉ lấy bài đăng của người dùng hiện tại
        if (viewMode == "recruiter")
        {
            posts = posts.Include(u => u.User).Where(p => p.UserId == userId);
        }

        // Nếu chế độ là "jobseeker", lấy tất cả bài đăng có status = true
        else if (viewMode == "jobseeker")
        {
            posts = posts.Include(u => u.User).Where(p => p.Status == "active");
        }

        // Lấy bản sao để thống kê
        var allPosts = posts.ToList();

        ViewBag.CountAll = allPosts.Count;
        ViewBag.CountActive = allPosts.Count(p => p.Status == "active");
        ViewBag.CountHidden = allPosts.Count(p => p.Status == "hidden");
        ViewBag.CountExpired = allPosts.Count(p => p.Deadline.HasValue && p.Deadline < DateOnly.FromDateTime(DateTime.Today));

        //lọc bài viết theo status
        if (!string.IsNullOrEmpty(status))
        {
            switch (status)
            {
                case "active":
                    posts = posts.Where(p => p.Status == "active");
                    break;
                case "hidden":
                    posts = posts.Where(p => p.Status == "hidden");
                    break;
                case "expired":
                    posts = posts.Where(p => p.Deadline.HasValue && p.Deadline < DateOnly.FromDateTime(DateTime.Today));
                    break;
            }
        }

        // Bộ lọc theo tiêu đề
        if (!string.IsNullOrWhiteSpace(search))
            posts = posts.Where(p => p.Title.Contains(search));

        // Bộ lọc theo ngày bắt đầu
        if (from.HasValue)
            posts = posts.Where(p => p.CreatedAt >= from.Value);

        // Bộ lọc theo ngày kết thúc
        if (to.HasValue)
            posts = posts.Where(p => p.CreatedAt <= to.Value);

        // Sắp xếp theo priority và thời gian
        var sortedPosts = posts
            .OrderByDescending(p => p.PriorityLevel)
            .ThenByDescending(p => p.PushedToTopUntil ?? p.CreatedAt)
            .ToList();

        var postCredits = _context.ServiceOrders
        .Count(s => s.UserId == userId && s.JobPostId == null && s.Status == "active");

        ViewBag.PostCredits = postCredits;

        //lấy số tiền của người dùng
        var userCredit = await _context.UserCredits.FirstOrDefaultAsync(x => x.UserId == userId);
        if (userCredit != null)
        {
            ViewData["Balance"] = userCredit.Balance;
        }
        else
        {
            ViewData["Balance"] = 0;
        }

        ViewBag.Status = status;
        return View(sortedPosts);
    }


    // create
    public IActionResult Create()
    {
        var userId = GetCurrentUserId(); // Hàm của bạn để lấy ID người dùng hiện tại

/*        var postCredits = _context.ServiceOrders
            .Count(s => s.UserId == userId && s.JobPostId == null && s.Status == "active");

        if (postCredits <= 0)
        {
            TempData["ShowBuyPopup"] = "true";
            return RedirectToAction("Index", "JobPost");
        }*/

        return View( new JobPost());
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobPost jobPost)
    {
        ModelState.Remove("User");
        if (!ModelState.IsValid)
        {
            return View(jobPost);
        }

        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        // Kiểm tra lượt đăng còn lại
/*        var availableCredit = await _context.ServiceOrders
            .Where(s => s.UserId == userId && s.JobPostId == null && s.Status == "active")
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        if (availableCredit == null)
        {
            // Không còn lượt đăng -> có thể hiển thị popup/mẫu redirect
            TempData["ShowBuyPopup"] = "true";
            return RedirectToAction("Index", "JobPost");
        }*/

        // Tạo bài đăng
        jobPost.UserId = userId;
        jobPost.User = user;
        jobPost.CreatedAt = DateTime.Now;
        jobPost.Status = "active";
        jobPost.PriorityLevel = 1;

        _context.JobPosts.Add(jobPost);
        await _context.SaveChangesAsync();

        // Gán bài đăng vào ServiceOrder để trừ lượt
/*        availableCredit.JobPostId = jobPost.Id;
        availableCredit.Status = "use";
        _context.ServiceOrders.Update(availableCredit);*/
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }




    public async Task<IActionResult> Edit(int id)
    {
        ModelState.Remove("User");
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

        if (!ModelState.IsValid)
        {
            return View(jobPost);
        }

        var original = await _context.JobPosts.FirstOrDefaultAsync(j => j.Id == id);
        if (original == null || original.UserId != GetCurrentUserId())
        {
            return NotFound();
        }

        try
        {
            // Cập nhật từng trường cần thiết
            original.Title = jobPost.Title;
            original.Description = jobPost.Description;
            original.Requirements = jobPost.Requirements;
            original.Benefits = jobPost.Benefits;
            original.Location = jobPost.Location;
            original.SalaryMin = jobPost.SalaryMin;
            original.SalaryMax = jobPost.SalaryMax;
            original.JobType = jobPost.JobType;
            original.ExperienceLevel = jobPost.ExperienceLevel;
            original.Deadline = jobPost.Deadline;
            original.PostType = jobPost.PostType;

            // Các trường không được chỉnh sửa vẫn giữ nguyên
            // original.UserId, CreatedAt, Status, ViewCount, IsFeatured giữ nguyên

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.JobPosts.Any(j => j.Id == id))
            {
                return NotFound();
            }
            throw;
        }
    }

    // delete 
    /*    [HttpPost]
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
        }*/

    [HttpPost]
    public async Task<IActionResult> PushToTop(int id)
    {
        var jobPost = await _context.JobPosts.FirstOrDefaultAsync(p => p.Id == id);
        if (jobPost == null || jobPost.UserId != GetCurrentUserId())
        {
            return NotFound();
        }

        jobPost.PushedToTopUntil = DateTime.Now;
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // history transaction
    public async Task<IActionResult> History()
    {
        int userId = GetCurrentUserId();
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return NotFound();
        }

        var transactions = await _context.CreditTransactions
            .Where(t => t.UserId == user.Id)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return View(transactions);
    }
}