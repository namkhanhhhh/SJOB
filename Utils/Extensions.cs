using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;

namespace SJOB_EXE201.Utils
{
    //alert re-used
    public static class Extensions
    {
        public static void SetNotification(this ITempDataDictionary tempData, string message, string alertClass)
        {
            tempData["Message"] = message;
            tempData["AlertClass"] = alertClass;
        }

        //paging
        public static async Task<List<T>> Paging<T>(DbContext context, ViewDataDictionary viewData, int page = 1, int pageSize = 6,
                string? includeProperties = null) where T : class
        {
            var query = context.Set<T>().AsQueryable();

            // Thêm Include nếu có
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(','))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            // Đếm tổng số phần tử
            int totalItems = await query.CountAsync();

            // Thực hiện phân trang
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Gán ViewBag tự động
            viewData["CurrentPage"] = page;
            viewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);

            return data;
        }

    }
}
