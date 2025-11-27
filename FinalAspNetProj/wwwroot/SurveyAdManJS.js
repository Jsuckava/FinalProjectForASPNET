let questionCounter = 0;

const createQuestionEditor = (text = '', maxRating = 5, isExisting = false) => {
    if (!isExisting) {
        questionCounter++;
    }
    const qId = `q-${isExisting ? 'existing-' + maxRating + Math.random().toString(36).substring(7) : questionCounter}`;

    const placeholder = document.querySelector('#questions-edit-container p');
    if (placeholder) {
        placeholder.remove();
    }

    return `
        <div class="card question-editor" data-id="${qId}" style="margin-bottom: 1rem; padding: 1rem;">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem;">
                <h4 style="font-size: 1rem; margin: 0;">Question ${isExisting ? '(Existing)' : '#' + questionCounter}</h4>
                <button type="button" class="delete btn-small" onclick="removeQuestion('${qId}')" style="padding: 5px 10px; font-size: 0.8rem; margin: 0;">Remove</button>
            </div>
            
            <div class="form-group" style="border-bottom: none; padding-bottom: 0;">
                <label for="${qId}-text">Question Text</label>
                <input type="text" id="${qId}-text" class="question-text-input" name="questionText" value="${text}" required>
            </div>
            
            <div class="form-group" style="border-bottom: none; padding-bottom: 0;">
                <label for="${qId}-max-rating">Max Rating (Min: 1, Default: 5)</label>
                <input type="number" id="${qId}-max-rating" class="question-rating-input" name="maxRating" value="${maxRating}" min="1" required>
            </div>
        </div>
    `;
};

window.removeQuestion = (id) => {
    const questionElement = document.querySelector(`.question-editor[data-id="${id}"]`);
    if (questionElement) {
        questionElement.remove();
    }

    const questionsContainer = document.getElementById("questions-edit-container");
    if (questionsContainer.children.length === 0) {
        questionsContainer.innerHTML = '<p style="color: #aaa; margin-bottom: 1rem;">No questions added yet.</p>';
    }
};

document.addEventListener("DOMContentLoaded", () => {
    const BASE_URL = "https://localhost:7296/api/SurveyTemplates";
    const tableBody = document.querySelector("#survey-list-table tbody");
    const modal = document.getElementById("survey-modal");
    const form = document.getElementById("survey-edit-form");
    const questionsContainer = document.getElementById("questions-edit-container");

    const token = localStorage.getItem("authToken");
    if (!token) {
        window.location.href = "login.html";
        return;
    }

    const authHeader = { "Authorization": `Bearer ${token}` };

    const showMessage = (message, isError = false) => {
        alert(message);
    };

    document.getElementById("open-add-survey-modal").addEventListener("click", () => {
        form.reset();
        document.querySelector("#survey-modal h2").textContent = "Create New Survey";
        document.getElementById("survey-id").value = "";
        questionsContainer.innerHTML = '<p style="color: #aaa; margin-bottom: 1rem;">No questions added yet.</p>';
        modal.classList.add("modal-visible");
    });

    document.getElementById("close-survey-modal").addEventListener("click", () => {
        modal.classList.remove("modal-visible");
    });

    document.getElementById("cancel-edit-btn").addEventListener("click", () => {
        modal.classList.remove("modal-visible");
    });

    document.getElementById("add-question-btn").addEventListener("click", () => {
        questionsContainer.insertAdjacentHTML('beforeend', createQuestionEditor());
    });


    const loadSurveys = async () => {
        tableBody.innerHTML = '<tr><td colspan="4" style="padding: 20px; text-align: center; color: var(--primary-color);">Loading...</td></tr>';

        try {
            const res = await fetch(BASE_URL, {
                method: 'GET',
                headers: { ...authHeader, 'Content-Type': 'application/json' }
            });

            if (res.status === 401) { 
                window.location.href = "login.html";
                return;
            }
            if (!res.ok) throw new Error("Failed to load surveys.");

            const surveys = await res.json();
            tableBody.innerHTML = "";

            if (surveys.length === 0) {
                tableBody.innerHTML = '<tr><td colspan="4" style="padding: 20px; text-align: center; color: #aaa;">No survey templates found.</td></tr>';
                return;
            }

            surveys.forEach(survey => {
                const statusColor = survey.isActive ? 'var(--success-color)' : 'var(--delete-color)';
                const statusText = survey.isActive ? 'Active' : 'Draft';

                const row = tableBody.insertRow();
                row.style.borderBottom = '1px dashed var(--border-color)';
                row.innerHTML = `
                    <td style="padding: 12px; font-weight: 500;">${survey.title}</td>
                    <td style="padding: 12px;">${survey.questionCount} Questions</td>
                    <td style="padding: 12px; color: ${statusColor}; font-weight: 600;">${statusText}</td>
                    <td style="padding: 12px; text-align: center;">
                        <button class="primary" data-id="${survey.surveyTemplateID}" onclick="editSurvey(this.dataset.id)" style="padding: 8px 12px;">Edit</button>
                        <button class="delete" data-id="${survey.surveyTemplateID}" onclick="deleteSurvey(this.dataset.id)" style="padding: 8px 12px;">Delete</button>
                    </td>
                `;
            });

        } catch (error) {
            console.error("Error loading surveys:", error);
            tableBody.innerHTML = '<tr><td colspan="4" style="padding: 20px; text-align: center; color: var(--delete-color);">Error connecting to API. Check console for details.</td></tr>';
        }
    };

    form.addEventListener("submit", async (e) => {
        e.preventDefault();
        const id = document.getElementById("survey-id").value;
        const method = id ? 'PUT' : 'POST';
        const url = id ? `${BASE_URL}/${id}` : BASE_URL;

        const questionEditors = questionsContainer.querySelectorAll('.question-editor');
        const questions = Array.from(questionEditors).map(editor => {
            const textInput = editor.querySelector('.question-text-input').value;
            const ratingInput = parseInt(editor.querySelector('.question-rating-input').value, 10);
            return {
                text: textInput,
                maxRating: ratingInput
            };
        });

        if (questions.length === 0) {
            showMessage("Please add at least one question to the survey.");
            return;
        }

        const surveyData = {
            title: document.getElementById("survey-title").value,
            description: document.getElementById("survey-description").value,
            isActive: document.getElementById("survey-is-active").value === 'true',
            questions: questions
        };

        try {
            const res = await fetch(url, {
                method: method,
                headers: { ...authHeader, 'Content-Type': 'application/json' },
                body: JSON.stringify(surveyData)
            });

            if (res.status === 401) { window.location.href = "login.html"; return; }
            if (res.status === 404) {
                throw new Error("Survey template not found.");
            }

            if (!res.ok && res.status !== 204) {
                const errorBody = await res.json();
                throw new Error(errorBody.message || `Failed to ${method === 'POST' ? 'create' : 'update'} survey.`);
            }

            modal.classList.remove("modal-visible");
            loadSurveys();
        } catch (error) {
            showMessage(`Error: ${error.message}`, true);
        }
    });

    window.editSurvey = async (id) => {
        try {
            const res = await fetch(`${BASE_URL}/${id}`, {
                method: 'GET',
                headers: { ...authHeader, 'Content-Type': 'application/json' }
            });

            if (res.status === 401) { window.location.href = "login.html"; return; }
            if (!res.ok) throw new Error("Failed to fetch survey details.");

            const template = await res.json();

            document.querySelector("#survey-modal h2").textContent = `Edit Survey: ${template.title}`;
            document.getElementById("survey-id").value = template.surveyTemplateID;
            document.getElementById("survey-title").value = template.title;
            document.getElementById("survey-description").value = template.description || "";
            document.getElementById("survey-is-active").value = template.isActive ? 'true' : 'false';

            questionsContainer.innerHTML = '';

            if (template.questions && template.questions.length > 0) {
                template.questions.forEach(question => {
                    const editorHtml = createQuestionEditor(
                        question.text,
                        question.maxRating,
                        true
                    );
                    questionsContainer.insertAdjacentHTML('beforeend', editorHtml);
                });
            } else {
                questionsContainer.innerHTML = '<p style="color: #aaa; margin-bottom: 1rem;">No questions added yet.</p>';
            }

            modal.classList.add("modal-visible");

        } catch (error) {
            showMessage(`Error loading survey for editing: ${error.message}`, true);
        }
    };

    window.deleteSurvey = async (id) => {
        if (!confirm(`Are you sure you want to delete Survey Template ID ${id}? This action is irreversible!`)) return;

        try {
            // 6. Add authHeader to fetch
            const res = await fetch(`${BASE_URL}/${id}`, {
                method: 'DELETE',
                headers: { ...authHeader }
            });

            if (res.status === 401) { window.location.href = "login.html"; return; }
            if (res.status === 404) {
                throw new Error("Survey template not found.");
            }

            if (!res.ok) throw new Error("Failed to delete survey.");

            loadSurveys();
        } catch (error) {
            showMessage(`Error deleting survey: ${error.message}`, true);
        }
    };

    loadSurveys();
});