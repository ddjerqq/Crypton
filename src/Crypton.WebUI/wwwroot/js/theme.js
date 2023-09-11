document.addEventListener("DOMContentLoaded", () => {
    let theme = localStorage.getItem("theme");

    if (theme === null || theme !== "light" && theme !== "dark") {
        theme = window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }

    window.setTheme(theme);
});

window.setTheme = (theme) => {
    document.documentElement.setAttribute("data-bs-theme", theme);
    localStorage.setItem("theme", theme);
}
