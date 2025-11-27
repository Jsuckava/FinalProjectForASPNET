document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.getElementById("login-form");
    const loginMessage = document.getElementById("login-message");
    const API_URL = "https://localhost:7296/api/Auth/login";

    loginForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        loginMessage.textContent = "";

        const username = document.getElementById("username").value;
        const password = document.getElementById("password").value;

        try {
            const res = await fetch(API_URL, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    username: username,
                    password: password
                })
            });

            if (!res.ok) {
                const errorData = await res.json();
                throw new Error(errorData.message || "Login failed.");
            }

            const authData = await res.json();
            localStorage.setItem("authToken", authData.token);
            localStorage.setItem("authUsername", authData.username);
            window.location.href = "AdminPage.html";

        } catch (error) {
            loginMessage.textContent = error.message;
        }
    });
});