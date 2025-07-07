using System.ComponentModel.DataAnnotations;

namespace SJOB_EXE201.ViewModels
{
    public class EditEmployerProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        public string Avatar { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string CompanyName { get; set; }

        public string CompanyDescription { get; set; }

        public string CompanyLogo { get; set; }

        [Url(ErrorMessage = "URL website không hợp lệ")]
        public string CompanyWebsite { get; set; }

        public string CompanySize { get; set; }

        public string Industry { get; set; }
    }
}
