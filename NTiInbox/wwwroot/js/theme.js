// Theme choice: dark (default), light or system. Stored in the browser, so it
// follows the workstation and not the IBM i profile.

const KEY = "nti-inbox-theme";

export function get() {
    return localStorage.getItem(KEY) || "dark";
}

export function set(value) {
    localStorage.setItem(KEY, value);
    apply(value);
    return value;
}

export function apply(value) {
    const dark = value === "dark"
        || (value === "system" && window.matchMedia("(prefers-color-scheme: dark)").matches);

    document.documentElement.setAttribute("data-theme", dark ? "dark" : "light");
    return dark;
}
