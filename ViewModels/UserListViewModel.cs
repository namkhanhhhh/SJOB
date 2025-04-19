using SJOB_EXE201.Models;
using System.Collections.Generic;

namespace SJOB_EXE201.Models
{
    public class UserListViewModel
    {
        public List<User> Users { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string SearchTerm { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
    }
}
