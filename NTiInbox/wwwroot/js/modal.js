// Escape closes the topmost open modal only, so a confirmation opened above
// another dialog does not close both.

const stack = [];

document.addEventListener("keydown", (event) => {
    if (event.key === "Escape" && stack.length > 0) {
        stack[stack.length - 1].reference.invokeMethodAsync("Escape");
    }
});

export function register(id, reference) {
    stack.push({ id, reference });
}

export function unregister(id) {
    const index = stack.findIndex((entry) => entry.id === id);

    if (index >= 0) {
        stack.splice(index, 1);
    }
}
