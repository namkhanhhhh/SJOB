using SJOB_EXE201.Models;
using System.Collections.Generic;

namespace SJOB_EXE201.ViewModels
{
    public class NotificationViewModel
    {
        public List<Notification> Notifications { get; set; }
        public int UnreadCount { get; set; }
    }
}
