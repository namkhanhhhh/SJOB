// Authentication Notification Functions
function showAuthNotification() {
    const overlay = document.getElementById("authNotificationOverlay")
    if (overlay) {
        overlay.classList.add("show")

        // Add animation classes
        const modal = overlay.querySelector(".auth-notification-modal")
        modal.classList.add("animate__animated", "animate__fadeInDown")

        // Add pulse animation to login button
        const loginBtn = overlay.querySelector(".auth-login-btn")
        loginBtn.classList.add("pulse")

        // Prevent body scrolling
        document.body.style.overflow = "hidden"
    }
}

function closeAuthNotification() {
    const overlay = document.getElementById("authNotificationOverlay")
    if (overlay) {
        const modal = overlay.querySelector(".auth-notification-modal")

        // Change animation to fade out
        modal.classList.remove("animate__fadeInDown")
        modal.classList.add("animate__fadeOutUp")

        // Wait for animation to complete before hiding
        setTimeout(() => {
            overlay.classList.remove("show")
            modal.classList.remove("animate__fadeOutUp")
            modal.classList.add("animate__fadeInDown")
            document.body.style.overflow = ""
        }, 500)
    }
}

// Close notification when clicking outside the modal
document.addEventListener("DOMContentLoaded", () => {
    const overlay = document.getElementById("authNotificationOverlay")
    if (overlay) {
        overlay.addEventListener("click", (e) => {
            if (e.target === overlay) {
                closeAuthNotification()
            }
        })
    }
})
