document.addEventListener("DOMContentLoaded", () => {
    const BASE_URL = "https://localhost:7296/api";
    const questionsContainer = document.getElementById("survey-questions-container");
    const surveyForm = document.getElementById("survey-form");
    const surveyMessage = document.getElementById("survey-message");
    const surveyTitle = document.getElementById("survey-main-title");
    const surveyDescription = document.getElementById("survey-description-text");

    const loadSurveyQuestions = async () => {
        try {
            const res = await fetch(`${BASE_URL}/Survey/active-template`);

            if (res.status === 404) {
                throw new Error("No active survey template found in the database.");
            }
            if (!res.ok) {
                throw new Error("Failed to load survey data due to a server error.");
            }

            const template = await res.json();
            const questions = template.questions || [];

            questionsContainer.innerHTML = "";
            surveyTitle.textContent = template.title || "Please rate your experience";
            surveyDescription.textContent = template.description || "";

            if (questions.length === 0) {
                questionsContainer.innerHTML = "<p style='color: var(--text-light); font-size: 1.2rem;'>No questions defined for the active template.</p>";
                return;
            }

            questions.forEach((q, index) => {
                const questionEl = document.createElement("div");
                questionEl.className = "form-group";
                const questionText = q.questionText || `Question ${index + 1} (Missing Text)`;

                const questionId = q.questionId;
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
            questionsContainer.innerHTML = `<p style='color: var(--delete-color); font-size: 1.2rem; font-weight: bold;'>Error loading survey: ${err.message}</p>`;
            surveyDescription.textContent = "";
        }
    };

    surveyForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        const formData = new FormData(surveyForm);
        const responses = [];
        const respondentName = formData.get("respondent-name") || "";

        const commentElement = document.getElementById("survey-comments");
        if (!commentElement) console.warn("Warning: Comment box with ID 'survey-comments' not found.");
        const commentText = commentElement ? commentElement.value : "";

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
            Comment: commentText,
            Responses: responses
        };

        surveyMessage.textContent = "Submitting...";
        surveyMessage.style.color = "var(--text-light)";

        try {
            const res = await fetch(`${BASE_URL}/Survey`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(submission)
            });

            if (!res.ok) {
                const errorBody = await res.json().catch(() => ({}));
                const errorMessage = errorBody.message || `HTTP Error ${res.status}: Submission failed.`;
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