using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Models;

namespace SJOB_EXE201.Controllers;
[Authorize(Roles = "Employer")]
public class HistoryTransactionController : Controller
{
    private readonly SjobContext _context;

    public HistoryTransactionController(SjobContext context)
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

    // history transaction
    public async Task<IActionResult> Index()
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

