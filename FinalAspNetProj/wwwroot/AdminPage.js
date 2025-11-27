document.addEventListener('DOMContentLoaded', function () {
    async function fetchSummaryData() {
        await new Promise(resolve => setTimeout(resolve, 500));

        const data = {
            totalResponses: '1,482',
            averageScore: '4.2',
            completionRate: '89%',
            responsesToday: '51'
        };

        document.getElementById('total-responses').textContent = data.totalResponses;
        document.getElementById('average-score').textContent = data.averageScore;
        document.getElementById('completion-rate').textContent = data.completionRate + '%';
        document.getElementById('responses-today').textContent = data.responsesToday;
    }
    let analyticsChartInstance;

    function generateChartData(days) {
        let labels = [];
        let data = [];
        const today = new Date();

        const formatDate = (date) => `${date.getMonth() + 1}/${date.getDate()}`;
        for (let i = days - 1; i >= 0; i--) {
            const date = new Date(today);
            date.setDate(today.getDate() - i);
            labels.push(formatDate(date));
            data.push(Math.floor(Math.random() * 50) + 10);
        }

        return { labels, data };
    }

    function renderAnalyticsChart(days) {
        const ctx = document.getElementById('analyticsChart').getContext('2d');
        const daysInt = parseInt(days);
        const { labels, data } = generateChartData(daysInt);
        if (analyticsChartInstance) {
            analyticsChartInstance.destroy();
        }

        analyticsChartInstance = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Responses Over Time',
                    data: data,
                    borderColor: '#3498db',
                    backgroundColor: 'rgba(52, 152, 219, 0.1)',
                    fill: true,
                    tension: 0.3 
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Number of Responses'
                        }
                    }
                }
            }
        });
    }
    const chartToggle = document.getElementById('chart-days-toggle');
    if (chartToggle) {
        chartToggle.addEventListener('change', function (e) {
            const days = e.target.value === '30' ? 30 : parseInt(e.target.value);
            renderAnalyticsChart(days);
        });
    }

    const exportForm = document.getElementById('export-form');
    if (exportForm) {
        exportForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const format = document.getElementById('export-format').value;
            alert(`Simulating API call to download report in ${format.toUpperCase()} format...`);
        });
    }
    fetchSummaryData();
    renderAnalyticsChart(30);

});