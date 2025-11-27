document.addEventListener("DOMContentLoaded", () => {
    const BASE_URL = "https://localhost:7296/api";
    const questionsContainer = document.getElementById("survey-questions-container");
    const surveyForm = document.getElementById("survey-form");
    const surveyMessage = document.getElementById("survey-message");

    const loadSurveyQuestions = async () => {
        try {
            const res = await fetch(`${BASE_URL}/Survey/questions`);
            if (!res.ok) throw new Error("Could not load survey questions.");

            const questions = await res.json();
            questionsContainer.innerHTML = "";

            if (questions.length === 0) {
                questionsContainer.innerHTML = "<p style='color: var(--text-light); font-size: 1.2rem;'>No active questions found. Please check back later.</p>";
                return;
            }

            questions.forEach((q, index) => {
                const questionEl = document.createElement("div");
                questionEl.className = "form-group";

                const questionId = q.questionId;
                const questionText = q.questionText;
                const maxRating = q.maxRating || 5;
                let inputHtml = `<label for="q-${questionId}">${questionText}</label>`;

                inputHtml += `<div class="rating-group">`;
                for (let i = 1; i <= maxRating; i++) {
                    inputHtml += `
                        <input type="radio" id="q-${questionId}-r${i}" name="q-${questionId}" value="${i}" required>
                        <label class="rating-label" for="q-${questionId}-r${i}">${i}</label>
                    `;
                }
                inputHtml += `</div>`;

                questionEl.innerHTML = inputHtml;
                questionsContainer.appendChild(questionEl);
            });

        } catch (err) {
            console.error("Error loading survey:", err);
            questionsContainer.innerHTML = "<p style='color: var(--delete-color); font-size: 1.2rem; font-weight: bold;'>Error loading survey. Please ensure the API is running and an active template exists.</p>";
        }
    };

    surveyForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        surveyMessage.textContent = "Submitting...";
        surveyMessage.style.color = "var(--text-light)";

        const formData = new FormData(surveyForm);
        const responses = [];
        const respondentName = formData.get("respondent-name") || "";

        for (let [key, value] of formData.entries()) {
            if (key.startsWith("q-")) {
                responses.push({
                    QuestionId: parseInt(key.replace("q-", ""), 10),
                    Rating: parseInt(value, 10)
                });
            }
        }

        const submission = {
            RespondentName: respondentName,
            Responses: responses
        };

        try {
            const res = await fetch(`${BASE_URL}/Survey`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(submission)
            });

            if (!res.ok) {
                const errorText = await res.text();
                let errorMessage = "Submission failed with an unknown error.";
                try {
                    const errorData = JSON.parse(errorText);
                    errorMessage = errorData.message || errorMessage;
                } catch {
                    errorMessage = errorText;
                }
                throw new Error(errorMessage);
            }

            surveyForm.innerHTML = "<h3>Thank you for your feedback! Your analysis is complete!</h3><p>You may close this window now.</p>";
            surveyMessage.textContent = "";

        } catch (err) {
            console.error("Submission error:", err.message);
            surveyMessage.textContent = `An error occurred: ${err.message}`;
            surveyMessage.style.color = "var(--delete-color)";
        }
    });

    loadSurveyQuestions();
});