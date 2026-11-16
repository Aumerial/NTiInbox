// Stores the language in the standard ASP.NET Core culture cookie, so the next visit
// starts in the same language, and keeps <html lang> in step for accessibility.
window.ntiLang = {
    save: function (code) {
        document.cookie = ".AspNetCore.Culture=c=" + code + "|uic=" + code
            + "; max-age=31536000; path=/; samesite=lax";
        document.documentElement.lang = code;
    }
};
