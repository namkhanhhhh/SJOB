using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Net.payOS.Types;
using Net.payOS;
using SJOB_EXE201.Models;
using System.Security.Policy;

public class CreditController : Controller
{
    private readonly SjobContext _context;
    private readonly PayOSService _payos;

    public CreditController(SjobContext context, PayOSService payos)
    {
        _context = context;
        _payos = payos;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User not authenticated.");
        return int.Parse(userIdClaim);
    }

    public IActionResult Deposit()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment(decimal amount)
    {
        int userId = GetCurrentUserId();

        // Lưu số tiền vào Session
        HttpContext.Session.SetString("Amount", amount.ToString());

        var result = await _payos.CreatePaymentAsync(
            userId,
            amount,
            Url.Action("Cancel", "Credit", null, Request.Scheme),   // Gọi từ Controller
            Url.Action("Success", "Credit", null, Request.Scheme)  // Gọi từ Controller
        );
        if (result == null)
        {
            TempData["ErrorMessage"] = "Không thể tạo giao dịch thanh toán. Vui lòng thử lại sau.";
            return RedirectToAction("Error", "Credit");
        }

        return Redirect(result.checkoutUrl);
    }

    public async Task<IActionResult> Success()
    {
        try
        {
          
            // Lấy số tiền từ Session
            var amountStr = HttpContext.Session.GetString("Amount");
            string orderCode = Request.Query["orderCode"];
            string status = Request.Query["status"];
            string cancelStr = Request.Query["cancel"];

            // Giao dịch bị hủy
            if (string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "cancel", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(cancelStr, "true", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Cancel");
            }

            // Kiểm tra số tiền hợp lệ
            if (!decimal.TryParse(amountStr, out var amount))
            {
                TempData["ErrorMessage"] = "Số tiền không hợp lệ.";
                return RedirectToAction("Error", "Credit");
            }

            int userId = GetCurrentUserId();

            var userCredit = await _context.UserCredits.FirstOrDefaultAsync(x => x.UserId == userId);
            if (userCredit == null)
            {
                userCredit = new UserCredit
                {
                    UserId = userId,
                    Balance = amount,
                    LastUpdated = DateTime.UtcNow
                };
                _context.UserCredits.Add(userCredit);
            }
            else
            {
                userCredit.Balance += amount;
                userCredit.LastUpdated = DateTime.UtcNow;
            }

            var creditTransaction = new CreditTransaction
            {
                UserId = userId,
                Amount = amount,
                TransactionType = "Topup",
                BalanceAfter = userCredit.Balance ?? 0,
                Description = "Thanh toán thành công qua PayOS",
                CreatedAt = DateTime.UtcNow
            };
            _context.CreditTransactions.Add(creditTransaction);

            await _context.SaveChangesAsync();

            return View("Success");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi xử lý giao dịch: {ex.Message}";
            return RedirectToAction("Error", "Credit");
        }
    }


    public IActionResult Error()
    {
        ViewBag.ErrorMessage = TempData["ErrorMessage"];
        return View();
    }

    public IActionResult Cancel()
    {
        // Thực hiện xử lý khi giao dịch bị hủy
        return View();  // Hiển thị trang Cancel
    }

    [HttpPost]
    [Route("payment/webhook")]
    public async Task<IActionResult> Webhook([FromBody] WebhookPayload payload, [FromHeader(Name = "x-payos-signature")] string signature)
    {
        if (!_payos.VerifyWebhookSignature(payload, signature))
            return Unauthorized();

        if (payload.Status == "PAID")
        {
            int userId;
            if (!int.TryParse(payload.Description, out userId)) return BadRequest("Invalid description");

            var amount = payload.Amount;
            var userCredit = await _context.UserCredits.FirstOrDefaultAsync(x => x.UserId == userId);

            if (userCredit == null)
            {
                userCredit = new UserCredit { UserId = userId, Balance = amount, LastUpdated = DateTime.UtcNow };
                _context.UserCredits.Add(userCredit);
            }
            else
            {
                userCredit.UserId = userId;
                userCredit.Balance += amount;
                userCredit.LastUpdated = DateTime.UtcNow;
            }

            var creditTransaction = new CreditTransaction
            {
                UserId = userId,
                Amount = amount,
                TransactionType = "Topup",
                BalanceAfter = userCredit.Balance ?? 0,
                Description = "Thanh toán thành công qua PayOS (Webhook)",
                CreatedAt = DateTime.UtcNow
            };
            _context.CreditTransactions.Add(creditTransaction);

            await _context.SaveChangesAsync();
        }

        // Trả về phản hồi 200 OK cho webhook sau khi xử lý xong
        return Ok();
    }

}
