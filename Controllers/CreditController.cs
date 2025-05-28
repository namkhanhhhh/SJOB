using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Net.payOS.Types;
using Net.payOS;
using SJOB_EXE201.Models;
using System.Security.Policy;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Employer")]
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
                    LastUpdated = GetVietnamTime()
                };
                _context.UserCredits.Add(userCredit);
            }
            else
            {
                userCredit.Balance += amount;
                userCredit.LastUpdated = GetVietnamTime();
            }

            var creditTransaction = new CreditTransaction
            {
                UserId = userId,
                Amount = amount,
                TransactionType = "NapTien",
                BalanceAfter = userCredit.Balance ?? 0,
                Description = "Thanh toán thành công qua PayOS",
                CreatedAt = GetVietnamTime()
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
        return View();
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

    // hàm chính 
    [HttpPost]
    [Route("payment/webhook")]
    public async Task<IActionResult> Webhook([FromBody] WebhookPayload payload, [FromHeader(Name = "x-payos-signature")] string signature)
    {
        if (!_payos.VerifyWebhookSignature(payload, signature))
            return Unauthorized();

        if (payload.Status == "PAID")
        {
            if (!payload.Description.StartsWith("NAPTIENSJOB+"))
                return BadRequest("Invalid description format");

            var userIdStr = payload.Description.Replace("NAPTIENSJOB+", "");
            if (!int.TryParse(userIdStr, out int userId))
                return BadRequest("Invalid userId");

            var amount = payload.Amount;
            var userCredit = await _context.UserCredits.FirstOrDefaultAsync(x => x.UserId == userId);

            if (userCredit == null)
            {
                userCredit = new UserCredit
                {
                    UserId = userId,
                    Balance = amount,
                    LastUpdated = GetVietnamTime()
                };
                _context.UserCredits.Add(userCredit);
            }
            else
            {
                userCredit.Balance += amount;
                userCredit.LastUpdated = GetVietnamTime();
            }

            var creditTransaction = new CreditTransaction
            {
                UserId = userId,
                Amount = amount,
                TransactionType = "Topup",
                BalanceAfter = userCredit.Balance ?? 0,
                Description = "Thanh toán thành công qua PayOS (Webhook)",
                CreatedAt = GetVietnamTime()
            };
            _context.CreditTransactions.Add(creditTransaction);

            await _context.SaveChangesAsync();
        }


        // Trả về phản hồi 200 OK cho webhook sau khi xử lý xong
        return Ok();
    }

    //hàm test 
   /* [HttpPost]
    [Route("payment/webhook")]
    public async Task<IActionResult> Webhook([FromBody] WebhookPayload payload, [FromHeader(Name = "x-payos-signature")] string signature)
    {
        try
        {
            if (!_payos.VerifyWebhookSignature(payload, signature))
            {
                Console.WriteLine("Webhook signature verification failed.");
                return Unauthorized();
            }

            if (payload.Status != "PAID")
            {
                Console.WriteLine($"Webhook ignored because status is {payload.Status}");
                return Ok();
            }

            if (!payload.Description.StartsWith("NAPTIENSJOB+"))
            {
                Console.WriteLine("Invalid description format: " + payload.Description);
                return BadRequest("Invalid description format");
            }

            var userIdStr = payload.Description.Replace("NAPTIENSJOB+", "");
            if (!int.TryParse(userIdStr, out int userId))
            {
                Console.WriteLine("Invalid userId in description: " + userIdStr);
                return BadRequest("Invalid userId");
            }

            var orderCode = payload.OrderCode.ToString();
            var exists = await _context.CreditTransactions.AnyAsync(x => x.Description.Contains(orderCode));
            if (exists)
            {
                Console.WriteLine("Order code already processed: " + orderCode);
                return Ok();
            }

            var userCredit = await _context.UserCredits.FirstOrDefaultAsync(x => x.UserId == userId);
            var amount = payload.Amount;

            if (userCredit == null)
            {
                userCredit = new UserCredit
                {
                    UserId = userId,
                    Balance = amount,
                    LastUpdated = GetVietnamTime()
                };
                _context.UserCredits.Add(userCredit);
                Console.WriteLine($"Created new UserCredit for user {userId} with amount {amount}");
            }
            else
            {
                userCredit.Balance += amount;
                userCredit.LastUpdated = GetVietnamTime();
                Console.WriteLine($"Updated UserCredit for user {userId}, added amount {amount}, new balance {userCredit.Balance}");
            }

            var creditTransaction = new CreditTransaction
            {
                UserId = userId,
                Amount = amount,
                TransactionType = "Topup",
                BalanceAfter = userCredit.Balance ?? 0,
                Description = $"Thanh toán qua PayOS (Webhook), mã giao dịch {orderCode}",
                CreatedAt = GetVietnamTime()
            };
            _context.CreditTransactions.Add(creditTransaction);

            await _context.SaveChangesAsync();

            Console.WriteLine("Saved changes for user " + userId + ", orderCode: " + orderCode);

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception in webhook: " + ex.Message);
            return StatusCode(500, "Internal Server Error");
        }
    }*/



    private DateTime GetVietnamTime()
    {
        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
    }


}
