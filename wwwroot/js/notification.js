// Thêm vào cuối file notification.js hiện tại
function addNewNotificationEffect() {
    const badge = $(".notification-badge");
    if (badge.length > 0) {
        badge.addClass("new-notification");

        // Xóa hiệu ứng sau 5 giây
        setTimeout(() => {
            badge.removeClass("new-notification");
        }, 5000);
    }
}

// Thêm gọi hàm này trong hàm pollNotifications khi phát hiện thông báo mới
function pollNotifications() {
    $.ajax({
        url: "/Notification/GetUnreadCount",
        type: "GET",
        success: (response) => {
            var badge = $(".notification-badge");
            var currentCount = parseInt(badge.text()) || 0;

            if (response.count > 0) {
                badge.text(response.count > 99 ? "99+" : response.count);
                badge.show();

                // Nếu số lượng thông báo tăng lên, thêm hiệu ứng
                if (response.count > currentCount) {
                    addNewNotificationEffect();
                }

                // Refresh notification dropdown content if it's open
                if ($(".notification-menu").hasClass("show")) {
                    refreshNotificationDropdown();
                }
            } else {
                badge.hide();
            }
        },
        complete: () => {
            setTimeout(pollNotifications, 30000); // Poll every 30 seconds
        },
    });
}