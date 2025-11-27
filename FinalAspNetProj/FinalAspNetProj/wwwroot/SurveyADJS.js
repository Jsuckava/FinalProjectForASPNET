document.addEventListener("DOMContentLoaded", () => {
    const BASE_URL = "https://localhost:7296/api/Survey";


    const ids = ["total-responses", "average-score", "completion-rate", "responses-today"];
    const elements = Object.fromEntries(ids.map(id => [id, document.getElementById(id)]));

    const state = { chartData: null, analyticsChart: null, days: 30 };

    const chartToggle = document.getElementById("chart-days-toggle");
    if (chartToggle) {
        chartToggle.addEventListener("change", async e => {
            state.days = Number(e.target.value);
            await updateDashboard();
        });
    }

    document.getElementById("export-form")?.addEventListener("submit", async (e) => {
        e.preventDefault();
        const format = document.getElementById("export-format").value;
        const downloadUrl = `${BASE_URL}/download?format=${format}`;
        const btn = e.target.querySelector("button");
        const originalText = btn.textContent;

        btn.textContent = "Generating...";
        btn.disabled = true;

        try {
            const res = await fetch(downloadUrl, {
                method: 'GET'
            });

            if (!res.ok) { throw new Error('Download failed'); }

            const blob = await res.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.style.display = 'none';
            a.href = url;
            const extension = format === 'excel' ? 'csv' : 'pdf';
            a.download = `SurveyResponses_${new Date().toISOString().split('T')[0]}.${extension}`;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            a.remove();
        } catch (err) {
            alert('Failed to download report. Check console.');
        } finally {
            btn.textContent = originalText;
            btn.disabled = false;
        }
    });

    async function fetchData(endpoint) {
        try {
            const res = await fetch(`${BASE_URL}${endpoint}`, {
                method: 'GET'
            });


            if (!res.ok) {
                console.error(`Failed to fetch ${endpoint}: ${res.status} ${res.statusText}`);
                return null;
            }
            return await res.json();
        } catch (err) {
            console.error("Fetch error:", err);
            return null;
        }
    }

    function updateElement(id, text) {
        const el = elements[id];
        if (el && el.textContent !== text)
            requestAnimationFrame(() => el.textContent = text);
    }

    function updateChart(newData) {
        const ctx = document.getElementById("analyticsChart")?.getContext("2d");
        if (!ctx) return;
        Chart.defaults.color = '#ccc';
        Chart.defaults.borderColor = 'rgba(255, 255, 255, 0.1)';

        const datasets = newData?.datasets || newData?.Datasets;
        const labels = newData?.labels || newData?.Labels;

        if (!labels?.length || !datasets?.length) {
            ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
            ctx.fillStyle = "#6c757d";
            ctx.textAlign = "center";
            ctx.font = "16px Arial";
            ctx.fillText("Chart data unavailable", ctx.canvas.width / 2, ctx.canvas.height / 2);
            return;
        }

        if (JSON.stringify(state.chartData) === JSON.stringify(newData)) return;
        state.chartData = newData;

        if (state.analyticsChart) state.analyticsChart.destroy();

        const gradient = ctx.createLinearGradient(0, 0, 0, 400);
        gradient.addColorStop(0, 'rgba(106, 141, 207, 0.5)');
        gradient.addColorStop(1, 'rgba(106, 141, 207, 0)');

        const chartDataPoints = datasets[0].data || datasets[0].Data;

        state.analyticsChart = new Chart(ctx, {
            type: "line",
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Responses',
                        data: chartDataPoints,
                        fill: true,
                        backgroundColor: gradient,
                        borderColor: '#6a8dcf',
                        borderWidth: 2,
                        tension: 0.4,
                        pointBackgroundColor: '#fff',
                        pointBorderColor: '#6a8dcf'
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    title: {
                        display: false,
                    }
                },
                scales: {
                    x: {
                        ticks: { color: "#999" },
                        grid: { display: false }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            precision: 0,
                            color: "#999"
                        },
                        grid: {
                            color: "rgba(255, 255, 255, 0.1)"
                        }
                    }
                },
                animation: false
            }
        });
    }

    async function updateComments() {
        const container = document.getElementById("comments-list");
        if (!container) return;

        const comments = await fetchData("/comments");

        if (!comments) {
            container.innerHTML = '<p style="color: var(--delete-color); text-align: center; padding: 1rem;">Error loading comments. Ensure server is running.</p>';
            return;
        }

        console.log("Raw Comments Data Received:", comments);
        container.innerHTML = "";

        if (comments.length === 0) {
            container.innerHTML = '<p style="color: #aaa; text-align: center; padding: 1rem;">No recent comments found.</p>';
            return;
        }

        comments.forEach(c => {
            const name = c.name || c.Name || 'Anonymous';
            const text = c.comment || c.Comment || c.text || c.Text || '';
            const dateVal = c.date || c.Date;

            const dateObj = new Date(dateVal);
            const dateStr = dateObj.toLocaleDateString() + ' ' + dateObj.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

            const item = document.createElement("div");
            item.style.padding = "15px";
            item.style.borderBottom = "1px solid var(--border-color)";
            item.innerHTML = `
                <div style="display: flex; justify-content: space-between; margin-bottom: 5px;">
                    <span style="font-weight: 600; color: var(--primary-color);">${name}</span>
                    <span style="font-size: 0.85rem; color: #888;">${dateStr}</span>
                </div>
                <p style="color: var(--text-light); font-size: 0.95rem; margin: 0;">${text}</p>
            `;
            container.appendChild(item);
        });
    }

    async function updateDashboard() {
        const endpoints = [
            "/Stats/Total",
            "/Stats/AverageScore",
            "/Stats/CompletionRate",
            "/Stats/Today",
            `/Stats/Trends?days=${state.days}`
        ];

        if (document.getElementById("total-responses").textContent === "0") {
            ids.forEach(id => updateElement(id, "Loading..."));
        }

        const [total, avg, rate, today, chart] = await Promise.all(
            endpoints.map(fetchData)
        );

        const totalCount = total ? (total.count ?? total.Count) : "N/A";
        const avgScore = avg ? (avg.score ?? avg.Score) : null;
        const compRate = rate ? (rate.percentage ?? rate.Percentage) : null;
        const todayCount = today ? (today.count ?? today.Count) : "N/A";

        updateElement("total-responses", typeof totalCount === 'number' ? totalCount.toLocaleString() : "N/A");
        updateElement("average-score", avgScore !== null ? avgScore.toFixed(1) + "%" : "N/A");
        updateElement("completion-rate", compRate !== null ? compRate + "%" : "N/A");
        updateElement("responses-today", typeof todayCount === 'number' ? todayCount.toLocaleString() : "N/A");

        if (chart) updateChart(chart);

        await updateComments();
    }

    updateDashboard();
    setInterval(updateDashboard, 60000);
});