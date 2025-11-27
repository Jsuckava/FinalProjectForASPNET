document.addEventListener("DOMContentLoaded", () => {
    const BASE_URL = "https://localhost:7296/api/Survey";
    const token = localStorage.getItem("authToken");
    if (!token) {
        window.location.href = "login.html";
        return;
    }
    const authHeader = { "Authorization": `Bearer ${token}` };

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

        try {
            const res = await fetch(downloadUrl, {
                method: 'GET',
                headers: { ...authHeader }
            });

            if (res.status === 401) { window.location.href = "login.html"; return; }
            if (!res.ok) { throw new Error('Download failed'); }

            const blob = await res.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.style.display = 'none';
            a.href = url;
            a.download = `SurveyResponses_${new Date().toISOString().split('T')[0]}.${format}`;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
        } catch (err) {
            alert('Failed to download report. Check console.');
        }
    });


    async function fetchData(endpoint) {
        try {
            const res = await fetch(`${BASE_URL}${endpoint}`, {
                method: 'GET',
                headers: { ...authHeader }
            });

            if (res.status === 401) {
                window.location.href = "login.html";
                return null;
            }
            if (!res.ok) {
                console.error(`Failed to fetch ${endpoint}: ${res.status}`);
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

        if (!newData?.labels?.length || !newData?.datasets?.length) {
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

        state.analyticsChart = new Chart(ctx, {
            type: "line",
            data: {
                labels: newData.labels,
                datasets: [
                    {
                        label: 'Responses',
                        data: newData.datasets[0].data,
                        fill: false,
                        borderColor: 'rgb(106, 141, 207)',
                        tension: 0.1
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

    async function updateDashboard() {
        const endpoints = [
            "/Stats/Total",
            "/Stats/AverageScore",
            "/Stats/CompletionRate",
            "/Stats/Today",
            `/Stats/Trends?days=${state.days}`
        ];

        ids.forEach(id => updateElement(id, "Loading..."));

        const [total, avg, rate, today, chart] = await Promise.all(
            endpoints.map(fetchData)
        );

        updateElement("total-responses", total?.count ?? "N/A");
        updateElement("average-score", avg?.score?.toFixed(1) ?? "N/A");
        updateElement("completion-rate", rate ? `${rate.percentage}%` : "N/A");
        updateElement("responses-today", today?.count ?? "N/A");
        updateChart(chart);
    }

    updateDashboard();
    setInterval(updateDashboard, 60000);
});