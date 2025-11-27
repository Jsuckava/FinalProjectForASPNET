let questionCounter = 0;

const createQuestionEditor = (text = '', maxRating = 5, questionId = null) => {
    if (questionId === null) {
        questionCounter++;
    }

    const dataId = questionId !== null ? `existing-${questionId}` : `new-${questionCounter}`;

    const placeholder = document.querySelector('#questions-edit-container p');
    if (placeholder) {
        placeholder.remove();
    }

    return `
        <div class="card question-editor" data-id="${dataId}" style="margin-bottom: 1rem; padding: 1rem;">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem;">
                <h4 style="font-size: 1rem; margin: 0;">Question ${questionId !== null ? '(Existing)' : '#' + questionCounter}</h4>
                <button type="button" class="delete btn-small" onclick="removeQuestion('${dataId}')" style="padding: 5px 10px; font-size: 0.8rem; margin: 0;">Remove</button>
            </div>
            
            <div class="form-group" style="border-bottom: none; padding-bottom: 0;">
                <label for="${dataId}-text">Question Text</label>
                <input type="text" id="${dataId}-text" class="question-text-input" name="questionText" value="${text || ''}" required>
            </div>
            
            <div class="form-group" style="border-bottom: none; padding-bottom: 0;">
                <label for="${dataId}-max-rating">Max Rating (Min: 1, Default: 5)</label>
                <input type="number" id="${dataId}-max-rating" class="question-rating-input" name="maxRating" value="${maxRating}" min="1" required>
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

window.toggleModal = (modal, show) => {
    if (show) {
        modal.classList.add('modal-visible');
    } else {
        modal.classList.remove('modal-visible');
    }
}

window.openShareModal = (surveyId) => {
    const shareModal = document.getElementById('share-survey-modal');
    const qrcodeCanvas = document.getElementById('qrcode-canvas');
    const surveyShareLink = document.getElementById('survey-share-link');

    const surveyBaseUrl = `${window.location.origin}/survey.html?id=`;
    const shareableLink = surveyBaseUrl + surveyId;

    surveyShareLink.textContent = shareableLink;
    surveyShareLink.href = shareableLink;

    qrcodeCanvas.innerHTML = '';

    try {
        const qr = new QRCode(qrcodeCanvas, {
            text: shareableLink,
            width: 128,
            height: 128,
            colorDark: "#1a1d24",
            colorLight: "#f0f0f0",
            correctLevel: QRCode.CorrectLevel.H
        });
        qrcodeCanvas.style.backgroundColor = 'white';
    } catch (error) {
        console.error('QR Code generation failed. Check if qrcode.min.js is loaded.', error);
        qrcodeCanvas.innerHTML = '<p style="color:red;">QR Code library not loaded.</p>';
    }

    toggleModal(shareModal, true);
};

window.copyShareLink = (btn) => {
    const linkText = document.getElementById('survey-share-link').href;
    navigator.clipboard.writeText(linkText)
        .then(() => {
            const originalText = btn.textContent;
            btn.textContent = 'Copied!';
            setTimeout(() => btn.textContent = originalText, 2000);
        })
        .catch(err => {
            console.error('Could not copy text: ', err);
            alert(`Failed to copy. Please copy manually: ${linkText}`);
        });
}

document.addEventListener("DOMContentLoaded", () => {
    const BASE_URL = "https://localhost:7296/api/SurveyTemplates";
    const tableBody = document.querySelector("#survey-list-table tbody");
    const modal = document.getElementById("survey-modal");
    const form = document.getElementById("survey-edit-form");
    const questionsContainer = document.getElementById("questions-edit-container");
    const shareModal = document.getElementById("share-survey-modal");

    const token = localStorage.getItem("authToken") || localStorage.getItem("jwtToken");

    if (!token) {
        window.location.href = "SurveyAdminManager.html";
        return;
    }

    const authHeader = { "Authorization": `Bearer ${token}` };

    const showMessage = (message, isError = false) => {
        alert(message);
    };

    const openBtn = document.getElementById("open-add-survey-modal");
    if (openBtn) {
        openBtn.addEventListener("click", () => {
            form.reset();
            document.querySelector("#survey-modal h2").textContent = "Create New Survey";
            document.getElementById("survey-id").value = "";
            questionsContainer.innerHTML = '<p style="color: #aaa; margin-bottom: 1rem;">No questions added yet.</p>';
            toggleModal(modal, true);
        });
    }

    const closeSurveyModalBtn = document.getElementById("close-survey-modal");
    if (closeSurveyModalBtn) {
        closeSurveyModalBtn.addEventListener("click", () => {
            toggleModal(modal, false);
        });
    }

    const closeShareModalBtn = document.getElementById("close-share-modal");
    if (closeShareModalBtn) {
        closeShareModalBtn.addEventListener("click", () => {
            toggleModal(shareModal, false);
        });
    }

    const cancelBtn = document.getElementById("cancel-edit-btn");
    if (cancelBtn) {
        cancelBtn.addEventListener("click", () => {
            toggleModal(modal, false);
        });
    }

    const copyBtn = document.querySelector('#share-survey-modal .copy-btn');
    if (copyBtn) {
        copyBtn.addEventListener('click', (e) => window.copyShareLink(e.target));
    }

    const addQBtn = document.getElementById("add-question-btn");
    if (addQBtn) {
        addQBtn.addEventListener("click", () => {
            questionsContainer.insertAdjacentHTML('beforeend', createQuestionEditor());
        });
    }

    const loadSurveys = async () => {
        if (!tableBody) return;

        tableBody.innerHTML = '<tr><td colspan="4" style="padding: 20px; text-align: center; color: var(--primary-color);">Loading...</td></tr>';

        try {
            const res = await fetch(BASE_URL, {
                method: 'GET',
                headers: { ...authHeader, 'Content-Type': 'application/json' }
            });

            if (res.status === 401) {
                window.location.href = "SurveyAdminManager.html";
                return;
            }
            if (!res.ok) throw new Error("Failed to load surveys.");

            const allSurveys = await res.json();

            const activeSurveys = allSurveys.filter(s => s.isActive === true);

            tableBody.innerHTML = "";

            if (activeSurveys.length === 0) {
                tableBody.innerHTML = '<tr><td colspan="4" style="padding: 20px; text-align: center; color: #aaa;">No active survey templates found.</td></tr>';
                return;
            }

            activeSurveys.forEach(survey => {
                const statusColor = 'var(--success-color)';
                const statusText = 'Active';

                const row = tableBody.insertRow();
                row.style.borderBottom = '1px dashed var(--border-color)';
                row.innerHTML = `
                    <td style="padding: 12px; font-weight: 500;">${survey.title}</td>
                    <td style="padding: 12px;">${survey.questionCount} Questions</td>
                    <td style="padding: 12px; color: ${statusColor}; font-weight: 600;">${statusText}</td>
                    <td style="padding: 12px; text-align: center;">
                        <button class="primary" data-id="${survey.surveyTemplateID}" onclick="window.openShareModal(this.dataset.id)" style="padding: 8px 12px; margin-right: 5px;">Share</button>
                        <button class="primary" data-id="${survey.surveyTemplateID}" onclick="editSurvey(this.dataset.id)" style="padding: 8px 12px; margin-right: 5px;">Edit</button>
                        <button class="delete" data-id="${survey.surveyTemplateID}" onclick="deleteSurvey(this.dataset.id)" style="padding: 8px 12px;">Archive</button>
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
            const dataId = editor.getAttribute('data-id');
            const qId = dataId && dataId.startsWith('existing-') ? parseInt(dataId.replace('existing-', '')) : null;

            return {
                questionId: qId,
                questionText: textInput,
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

            if (res.status === 401) { window.location.href = "SurveyAdminManager.html"; return; }
            if (res.status === 404) {
                throw new Error("Survey template not found.");
            }

            if (!res.ok && res.status !== 204) {
                const errorBody = await res.json();
                throw new Error(errorBody.message || `Failed to ${method === 'POST' ? 'create' : 'update'} survey.`);
            }

            toggleModal(modal, false);
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

            if (res.status === 401) { window.location.href = "SurveyAdminManager.html"; return; }
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
                        question.questionText || '',
                        question.maxRating,
                        question.questionId
                    );
                    questionsContainer.insertAdjacentHTML('beforeend', editorHtml);
                });
            } else {
                questionsContainer.innerHTML = '<p style="color: #aaa; margin-bottom: 1rem;">No questions added yet.</p>';
            }

            toggleModal(modal, true);

        } catch (error) {
            showMessage(`Error loading survey for editing: ${error.message}`, true);
        }
    };

    window.deleteSurvey = async (id) => {
        if (!confirm(`Are you sure you want to delete Survey Template ID ${id}? This action is irreversible!`)) return;

        try {
            const res = await fetch(`${BASE_URL}/${id}`, {
                method: 'DELETE',
                headers: { ...authHeader }
            });

            if (res.status === 401) { window.location.href = "SurveyAdminManager.html"; return; }

            if (res.status === 404) {
                loadSurveys();
                return;
            }

            if (!res.ok && res.status !== 204) throw new Error("Failed to delete survey.");
            loadSurveys();
        } catch (error) {
            showMessage(`Error deleting survey: ${error.message}`, true);
        }
    };

    loadSurveys();
});