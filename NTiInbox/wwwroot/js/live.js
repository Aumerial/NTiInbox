// Browser notifications for messages waiting for a reply.
// Nothing is notified while the tab is in the foreground: the user
// already sees the list update.

export function permission() {
    return "Notification" in window ? Notification.permission : "unsupported";
}

export async function requestPermission() {
    if (!("Notification" in window)) {
        return "unsupported";
    }

    if (Notification.permission !== "default") {
        return Notification.permission;
    }

    // The permission can only be requested right after a user gesture.
    return await Notification.requestPermission();
}

export function notify(title, body, tag) {
    if (!("Notification" in window) || Notification.permission !== "granted") {
        return false;
    }

    if (document.visibilityState === "visible") {
        return false;
    }

    const notification = new Notification(title, {
        body: body,
        tag: tag,
        icon: "/favicon.png",
        requireInteraction: false
    });

    notification.onclick = () => {
        window.focus();
        notification.close();
    };

    return true;
}
