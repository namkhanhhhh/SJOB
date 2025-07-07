// employer.js - JavaScript for employer dashboard

document.addEventListener("DOMContentLoaded", () => {
    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    tooltipTriggerList.map((tooltipTriggerEl) => new bootstrap.Tooltip(tooltipTriggerEl))

    // Animate elements when they come into view
    const animateOnScroll = () => {
        const elements = document.querySelectorAll(".animate-on-scroll")

        elements.forEach((element) => {
            const elementPosition = element.getBoundingClientRect().top
            const windowHeight = window.innerHeight

            if (elementPosition < windowHeight - 50) {
                element.classList.add("animate")
            }
        })
    }

    // Initially call the function
    animateOnScroll()

    // Call the function on scroll
    window.addEventListener("scroll", animateOnScroll)

    // Add ripple effect to buttons
    const buttons = document.querySelectorAll(".btn")

    buttons.forEach((button) => {
        button.addEventListener("click", function (e) {
            const x = e.clientX - e.target.getBoundingClientRect().left
            const y = e.clientY - e.target.getBoundingClientRect().top

            const ripple = document.createElement("span")
            ripple.classList.add("ripple")
            ripple.style.left = `${x}px`
            ripple.style.top = `${y}px`

            this.appendChild(ripple)

            setTimeout(() => {
                ripple.remove()
            }, 600)
        })
    })

    // Toggle mobile sidebar
    const sidebarToggle = document.querySelector(".sidebar-toggle")
    if (sidebarToggle) {
        sidebarToggle.addEventListener("click", () => {
            document.querySelector(".employer-dashboard").classList.toggle("sidebar-collapsed")
        })
    }

    // Dropdown hover effect for desktop
    if (window.innerWidth >= 992) {
        const dropdowns = document.querySelectorAll(".dropdown")

        dropdowns.forEach((dropdown) => {
            dropdown.addEventListener("mouseenter", function () {
                const dropdownMenu = this.querySelector(".dropdown-menu")
                if (dropdownMenu) {
                    dropdownMenu.classList.add("show")
                }
            })

            dropdown.addEventListener("mouseleave", function () {
                const dropdownMenu = this.querySelector(".dropdown-menu")
                if (dropdownMenu) {
                    dropdownMenu.classList.remove("show")
                }
            })
        })
    }
})
