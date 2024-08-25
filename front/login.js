document.addEventListener("DOMContentLoaded", function() {
    const loginForm = document.getElementById("loginForm");
    const errorMessage = document.getElementById("errorMessage");

    loginForm.addEventListener("submit", async function(event) {
        event.preventDefault();

        const login = document.getElementById("username").value.trim();
        const password = document.getElementById("password").value.trim();

        try {
            const response = await fetch("https://localhost:7055/api/Order/Login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ login, password })
            });

            if (response.ok) {
                const data = await response.json();
                const token = data.tokens.accessToken;
                const refreshToken = data.tokens.refreshToken;

                // Сохраняем токены в localStorage
                localStorage.setItem('authToken', token);
                localStorage.setItem('refreshToken', refreshToken);

                // Перенаправляем на страницу с музыкой
                window.location.href = 'index.html';
            } else {
                errorMessage.textContent = "Invalid username or password!";
                errorMessage.style.color = 'red';
            }
        } catch (error) {
            console.error("Error:", error);
            errorMessage.textContent = "An error occurred during login.";
            errorMessage.style.color = 'red';
        }
    });
});
