using Microsoft.EntityFrameworkCore;

namespace SJOB_EXE201.Utils
{
    public class DatabaseValidation
    {
        //check status active_inactive in database
        public static bool CheckStatus<T>(DbContext context, int id, bool expectedStatus) where T : class
        {
            if (id <= 0)
            {
                return false; // ID không hợp lệ
            }

            // Lấy đối tượng từ database theo ID
            var entity = context.Set<T>().Find(id);
            if (entity == null)
            {
                return false; // Không tìm thấy object
            }

            // Kiểm tra nếu object có thuộc tính "Status"
            var statusProperty = typeof(T).GetProperty("Status");
            if (statusProperty == null)
            {
                throw new InvalidOperationException("Object does not have a Status property.");
            }

            // Lấy giá trị của thuộc tính Status và so sánh
            var statusValue = (bool)statusProperty.GetValue(entity);
            return statusValue == expectedStatus;
        }


        //Parse date
        public const string DateFormat = "yyyy-MM-dd";
        public static DateTime? ParseDate(string dateString)
        {
            if (DateTime.TryParseExact(dateString, DateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate;
            }
            return null;
        }

        public static int? GetYear(string dateString)
        {
            return ParseDate(dateString)?.Year;
        }

        public static int? GetMonth(string dateString)
        {
            return ParseDate(dateString)?.Month;
        }

        public static int? GetDay(string dateString)
        {
            return ParseDate(dateString)?.Day;
        }
    }
}
