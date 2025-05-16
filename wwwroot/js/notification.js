// Notification handling script
$(document).ready(() => {
    // Mark notification as read when clicked
    $(document).on("click", ".notification-item", function (e) {
        var notificationId = $(this).data("notification-id")

        // Don't mark as read if clicking on a button inside the notification
        if ($(e.target).is("button") || $(e.target).closest("button").length) {
            return
        }

        $.ajax({
            url: "/Notification/MarkAsRead",
            type: "POST",
            data: { id: notificationId },
            success: () => {
                // Update UI
                $(e.currentTarget).removeClass("unread")

                // Update badge count
                updateNotificationBadge()
            },
        })
    })

    // Mark all notifications as read
    $(document).on("click", ".mark-all-read", (e) => {
        e.preventDefault()
        e.stopPropagation()

        $.ajax({
            url: "/Notification/MarkAllAsRead",
            type: "POST",
            success: () => {
                // Update UI
                $(".notification-item").removeClass("unread")
                $(".notification-badge").hide()
            },
        })
    })

    // Function to update notification badge
    function updateNotificationBadge() {
        $.ajax({
            url: "/Notification/GetUnreadCount",
            type: "GET",
            success: (response) => {
                var badge = $(".notification-badge")
                if (response.count > 0) {
                    badge.text(response.count > 99 ? "99+" : response.count)
                    badge.show()
                } else {
                    badge.hide()
                }
            },
        })
    }

    // Poll for new notifications every 30 seconds
    function pollNotifications() {
        $.ajax({
            url: "/Notification/GetUnreadCount",
            type: "GET",
            success: (response) => {
                var badge = $(".notification-badge")
                if (response.count > 0) {
                    badge.text(response.count > 99 ? "99+" : response.count)
                    badge.show()

                    // Refresh notification dropdown content if it's open
                    if ($(".notification-menu").hasClass("show")) {
                        refreshNotificationDropdown()
                    }
                } else {
                    badge.hide()
                }
            },
            complete: () => {
                setTimeout(pollNotifications, 30000) // Poll every 30 seconds
            },
        })
    }

    // Function to refresh notification dropdown content
    function refreshNotificationDropdown() {
        $.ajax({
            url: "/Notification/GetLatestNotifications",
            type: "GET",
            success: (notifications) => {
                var notificationList = $(".notification-list")
                notificationList.empty()

                if (notifications.length > 0) {
                    notifications.forEach((notification) => {
                        var icon = getNotificationIcon(notification.type)
                        var timeDisplay = formatNotificationTime(notification.createdAt)

                        var notificationItem = `
                            <a href="${notification.url}" class="dropdown-item notification-item ${notification.isRead ? "" : "unread"}" data-notification-id="${notification.id}">
                                <div class="notification-icon">
                                    <i class="${icon}"></i>
                                </div>
                                <div class="notification-content">
                                    <div class="notification-title">${notification.title}</div>
                                    <div class="notification-message">${notification.message}</div>
                                    <div class="notification-time">${timeDisplay}</div>
                                </div>
                            </a>
                        `

                        notificationList.append(notificationItem)
                    })
                } else {
                    notificationList.append(`
                        <div class="dropdown-item no-notifications">
                            <p class="text-center text-muted my-2">Không có thông báo nào</p>
                        </div>
                    `)
                }
            },
        })
    }

    // Helper function to get icon class based on notification type
    function getNotificationIcon(type) {
        switch (type) {
            case "application":
                return "fas fa-file-alt"
            case "new_application":
                return "fas fa-user-plus"
            case "application_note":
                return "fas fa-sticky-note"
            case "wishlist":
                return "fas fa-heart"
            default:
                return "fas fa-bell"
        }
    }

    // Helper function to format notification time
    function formatNotificationTime(dateString) {
        var date = new Date(dateString)
        var now = new Date()
        var yesterday = new Date(now)
        yesterday.setDate(yesterday.getDate() - 1)

        if (date.toDateString() === now.toDateString()) {
            // Today - show time
            return date.getHours().toString().padStart(2, "0") + ":" + date.getMinutes().toString().padStart(2, "0")
        } else if (date.toDateString() === yesterday.toDateString()) {
            // Yesterday
            return "Hôm qua"
        } else {
            // Other days - show date
            return (
                date.getDate().toString().padStart(2, "0") +
                "/" +
                (date.getMonth() + 1).toString().padStart(2, "0") +
                "/" +
                date.getFullYear()
            )
        }
    }

    // Start polling for notifications
    pollNotifications()

    // Toggle notification dropdown
    $(".notification-btn").click((e) => {
        e.preventDefault()
        e.stopPropagation()

        $(".notification-menu").toggleClass("show")

        // If opening the dropdown, refresh the content
        if ($(".notification-menu").hasClass("show")) {
            refreshNotificationDropdown()
        }
    })

    // Close dropdown when clicking outside
    $(document).on("click", (e) => {
        if (!$(e.target).closest(".notification-dropdown").length) {
            $(".notification-menu").removeClass("show")
        }
    })
})
