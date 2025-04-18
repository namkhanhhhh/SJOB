﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using SJOB_EXE201.Models;
using SJOB_EXE201.Utils;
using System.Security.Claims;

namespace SJOB_EXE201.Middleware
{
    public class CheckUserStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public CheckUserStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, SjobContext dbContext)
        {
            if (context.User.Identity.IsAuthenticated)
            {
              
                var userId = context.User.FindFirst("Id")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await dbContext.Users
                        .Where(u => u.Id == int.Parse(userId))
                        .Select(u =>  u.Status )
                        .FirstOrDefaultAsync();

                    if (user != null && DbConstants.User_Status_Inactive.Equals(user.ToString().ToLower()))// Nếu tài khoản bị vô hiệu hóa
                    {
                        await context.SignOutAsync(); // Đăng xuất
                        context.Session.SetString("BanMessage", "Your account has been banned.");
                        context.Response.Redirect("/Login/Index"); // Chuyển hướng về trang login
                        return;
                    }
                }
            }

            await _next(context); // Chuyển tiếp request 
        }
    }
}
