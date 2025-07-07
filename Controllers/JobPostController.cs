using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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

        //lấy số lượt đăng bài còn lại
        var userPostCredit = _context.UserPostCredits
        .FirstOrDefault(upc => upc.UserId == userId);

        if (userPostCredit != null)
        {
            ViewBag.Silver = userPostCredit.SilverPostsAvailable;
            ViewBag.Gold = userPostCredit.GoldPostsAvailable;
            ViewBag.Diamond = userPostCredit.DiamondPostsAvailable;
            ViewBag.PushToTop = userPostCredit.PushToTopAvailable;
            ViewBag.verify = userPostCredit.AuthenLogoAvailable;

        }
        else
        {
            ViewBag.Silver = 0;
            ViewBag.Gold = 0;
            ViewBag.Diamond = 0;
        }

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

        ViewBag.Status = status;
        return View(sortedPosts);
    }


    // GET: JobPost/Create
    public IActionResult Create()
    {
        var userId = GetCurrentUserId();

        var userPostCredit = _context.UserPostCredits
            .FirstOrDefault(upc => upc.UserId == userId);

        // Nếu không có dữ liệu hoặc không còn bất kỳ lượt đăng nào
        if (userPostCredit == null ||
            (userPostCredit.SilverPostsAvailable <= 0 &&
             userPostCredit.GoldPostsAvailable <= 0 &&
             userPostCredit.DiamondPostsAvailable <= 0))
        {
            TempData["ShowBuyPopup"] = "true";
            return RedirectToAction("Index", "JobPost");
        }

        // Gửi số lượt còn lại vào ViewBag để hiển thị trong dropdown
        ViewBag.SilverCredits = userPostCredit.SilverPostsAvailable;
        ViewBag.GoldCredits = userPostCredit.GoldPostsAvailable;
        ViewBag.DiamondCredits = userPostCredit.DiamondPostsAvailable;

        return View(new JobPost());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobPost jobPost,
                                            string Province, string District, string Ward,
                                            int JobCategoryId,
                                            IFormFile ImageMainFile, IFormFile? Image2File,
                                            IFormFile? Image3File, IFormFile? Image4File)
    {
        ModelState.Remove("User");
        if (!ModelState.IsValid)
        {
            return View(jobPost);
        }

        var userId = GetCurrentUserId();

        var user = await _context.Users
            .Include(u => u.UserPostCredit)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var userPostCredit = user?.UserPostCredit;

        if (userPostCredit == null ||
            (userPostCredit.SilverPostsAvailable <= 0 &&
             userPostCredit.GoldPostsAvailable <= 0 &&
             userPostCredit.DiamondPostsAvailable <= 0))
        {
            TempData["ShowBuyPopup"] = "true";
            return RedirectToAction("Index", "JobPost");
        }

        // Tạo bài đăng
        jobPost.UserId = userId;
        jobPost.User = user;
        jobPost.CreatedAt = DateTime.Now;
        jobPost.Deadline = DateOnly.FromDateTime(DateTime.Now).AddDays(5);
        jobPost.Status = "active";

        //địa chỉ 
        var specificAddress = Request.Form["SpecificAddress"];
        jobPost.Location = $"{specificAddress} - {Ward} - {District} - {Province}";

        // Gán PriorityLevel dựa trên loại tin
        switch (jobPost.PostType?.ToLower())
        {
            case "diamond":
                jobPost.PriorityLevel = 3;
                break;
            case "gold":
                jobPost.PriorityLevel = 2;
                break;
            case "silver":
            default:
                jobPost.PriorityLevel = 1;
                break;
        }

        // thêm ảnh 
        string UploadImage(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine("wwwroot/uploads", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return "/uploads/" + fileName;
        }

        if (ImageMainFile != null)
            jobPost.ImageMain = UploadImage(ImageMainFile);
        if (Image2File != null)
            jobPost.Image2 = UploadImage(Image2File);
        if (Image3File != null)
            jobPost.Image3 = UploadImage(Image3File);
        if (Image4File != null)
            jobPost.Image4 = UploadImage(Image4File);

        _context.JobPosts.Add(jobPost);
        await _context.SaveChangesAsync();

        // Thêm job category relationship
        if (JobCategoryId > 0)
        {
            var jobPostCategory = new JobPostCategory
            {
                JobPostId = jobPost.Id,
                CategoryId = JobCategoryId,
                CreatedAt = DateTime.Now
            };
            _context.JobPostCategories.Add(jobPostCategory);
        }

        // Trừ lượt theo loại bài đăng
        if (userPostCredit != null)
        {
            switch (jobPost.PostType?.ToLower())
            {
                case "silver":
                    if (userPostCredit.SilverPostsAvailable > 0)
                        userPostCredit.SilverPostsAvailable--;
                    break;
                case "gold":
                    if (userPostCredit.GoldPostsAvailable > 0)
                        userPostCredit.GoldPostsAvailable--;
                    break;
                case "diamond":
                    if (userPostCredit.DiamondPostsAvailable > 0)
                        userPostCredit.DiamondPostsAvailable--;
                    break;
            }

            userPostCredit.LastUpdated = DateTime.Now;
            _context.UserPostCredits.Update(userPostCredit);
        }

        await AddCreditTransaction(
            userId,
            $"use_{jobPost.PostType.ToLower()}_post",
            jobPost.Id,
            $"Sử dụng 1 lượt đăng bài {jobPost.PostType} cho bài '{jobPost.Title}'"
        );

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đăng bài thành công!";

        return RedirectToAction("Index");
    }


    public async Task<IActionResult> Details(int id)
    {
        var jobPost = await _context.JobPosts
            .Include(j => j.User)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (jobPost == null || jobPost.UserId != GetCurrentUserId())
        {
            return NotFound();
        }

        return View(jobPost);
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
            // Cập nhật các trường được phép chỉnh sửa
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

            // Không cho phép sửa PostType hoặc PriorityLevel
            // original.PostType và original.PriorityLevel giữ nguyên

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
        var userID = GetCurrentUserId();
        var pushtotopAvailable = await _context.UserPostCredits.FirstOrDefaultAsync(u => u.UserId == userID);
        var jobPost = await _context.JobPosts.FirstOrDefaultAsync(p => p.Id == id);

        if (jobPost == null || jobPost.UserId != userID)
        {
            return NotFound();
        }

        // Kiểm tra xem bài đăng đã được đẩy lên trong 1 giờ qua chưa
        if (jobPost.PushedToTopUntil.HasValue && jobPost.PushedToTopUntil.Value.AddHours(1) > DateTime.Now)
        {
            TempData["InfoMessage"] = "Bài đăng đã được đẩy lên gần đây và vẫn đang ở vị trí ưu tiên.";
            return RedirectToAction("Index");
        }

        if (pushtotopAvailable?.PushToTopAvailable < 1)
        {
            TempData["ErrorMessage"] = "Không đủ lượt đẩy bài, vui lòng mua thêm!";
            return RedirectToAction("Index");
        }

        //trừ lượt đăng 
        jobPost.PushedToTopUntil = DateTime.Now;
        pushtotopAvailable.PushToTopAvailable--;

        // Trong PushToTop sau khi trừ lượt
        await AddCreditTransaction(
            userID,
            "use_push_to_top",
            jobPost.Id,
            $"Sử dụng 1 lượt đẩy bài cho bài '{jobPost.Title}'"
        );

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đẩy bài thành công!";
        return RedirectToAction("Index");
    }

    //ẩn bài đăng
    [HttpPost]
    public async Task<IActionResult> Hidden(int id)
    {
        var userId = GetCurrentUserId();
        var post = await _context.JobPosts.FindAsync(id);

        if (post == null || post.UserId != userId)
        {
            return NotFound();
        }

        post.Status = "hidden";
        _context.Update(post);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Bài đăng đã được ẩn.";
        return RedirectToAction("Index");
    }

    //hiện bài đăng 
    [HttpPost]
    public async Task<IActionResult> Unhide(int id)
    {
        var post = await _context.JobPosts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        // Kiểm tra nếu bài đăng đang bị ẩn
        if (post.Status == "hidden")
        {
            post.Status = "active"; // Đặt lại trạng thái thành "active" để bài đăng hiển thị
            _context.Update(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bài đăng đã được hiển thị lại.";
        }
        else
        {
            TempData["ErrorMessage"] = "Bài đăng đã ở trạng thái hiển thị.";
        }

        return RedirectToAction("Index");
    }

    //thêm vào các lượt giao dịch 
    private async Task AddCreditTransaction(int userId, string type, int referenceId, string description)
    {
        var transaction = new CreditTransaction
        {
            UserId = userId,
            Amount = 0, // Không liên quan đến tiền
            TransactionType = type,
            ReferenceId = referenceId,
            ReferenceType = "job_post",
            Description = description,
            CreatedAt = DateTime.Now
        };

        _context.CreditTransactions.Add(transaction);
        await _context.SaveChangesAsync();
    }
}

