document.addEventListener('DOMContentLoaded', () => {
    initCounters();
    initHeroChart();
    initDashboardCharts();
});

function initCounters() {
    document.querySelectorAll('[data-counter]').forEach(el => {
        const target = parseFloat(el.dataset.counter);
        const prefix = el.dataset.prefix || '';
        const suffix = el.dataset.suffix || '';
        const duration = 2000;
        const start = performance.now();

        function update(now) {
            const progress = Math.min((now - start) / duration, 1);
            const eased = 1 - Math.pow(1 - progress, 3);
            const current = target * eased;
            el.textContent = prefix + (Number.isInteger(target) ? Math.floor(current).toLocaleString() : current.toFixed(1)) + suffix;
            if (progress < 1) requestAnimationFrame(update);
        }
        requestAnimationFrame(update);
    });
}

function initHeroChart() {
    const canvas = document.getElementById('heroChart');
    if (!canvas || typeof Chart === 'undefined') return;

    new Chart(canvas, {
        type: 'line',
        data: {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            datasets: [{
                data: [12, 19, 15, 25, 22, 30, 28, 35, 32, 40, 38, 45],
                borderColor: '#d4af37',
                backgroundColor: 'rgba(212, 175, 55, 0.1)',
                fill: true,
                tension: 0.4,
                pointRadius: 0,
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: {
                x: { display: false },
                y: { display: false }
            }
        }
    });
}

function initDashboardCharts() {
    const growthCanvas = document.getElementById('growthChart');
    if (!growthCanvas || typeof Chart === 'undefined') return;

    const labels = JSON.parse(growthCanvas.dataset.labels || '[]');
    const values = JSON.parse(growthCanvas.dataset.values || '[]');

    new Chart(growthCanvas, {
        type: 'line',
        data: {
            labels,
            datasets: [{
                label: 'Portfolio Growth',
                data: values,
                borderColor: '#d4af37',
                backgroundColor: 'rgba(212, 175, 55, 0.08)',
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            plugins: { legend: { display: false } },
            scales: {
                x: { ticks: { color: '#9a9a9a' }, grid: { color: 'rgba(212,175,55,0.1)' } },
                y: { ticks: { color: '#9a9a9a' }, grid: { color: 'rgba(212,175,55,0.1)' } }
            }
        }
    });
}
