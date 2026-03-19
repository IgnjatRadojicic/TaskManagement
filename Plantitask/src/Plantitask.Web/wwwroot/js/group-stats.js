window.groupStats = {

    _charts: {},

    /* ---- Animated circular progress wheel (pure canvas, no Chart.js) ---- */
    renderWheel: function (canvasId, percent, activeColor, trackColor) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        const size = canvas.width;
        const center = size / 2;
        const lineWidth = 12;
        const radius = (size - lineWidth) / 2 - 4;

        let current = 0;
        const target = Math.min(100, Math.max(0, percent));
        const step = target / 40;

        const draw = () => {
            ctx.clearRect(0, 0, size, size);

            // Track
            ctx.beginPath();
            ctx.arc(center, center, radius, 0, Math.PI * 2);
            ctx.strokeStyle = trackColor;
            ctx.lineWidth = lineWidth;
            ctx.lineCap = 'round';
            ctx.stroke();

            // Active arc
            if (current > 0) {
                const startAngle = -Math.PI / 2;
                const endAngle = startAngle + (Math.PI * 2 * current / 100);
                ctx.beginPath();
                ctx.arc(center, center, radius, startAngle, endAngle);
                ctx.strokeStyle = activeColor;
                ctx.lineWidth = lineWidth;
                ctx.lineCap = 'round';
                ctx.stroke();
            }

            if (current < target) {
                current = Math.min(target, current + step);
                requestAnimationFrame(draw);
            }
        };

        draw();
    },


    /* ---- Donut chart (Chart.js) ---- */
    renderDonut: function (canvasId, labels, data, colors) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        if (this._charts[canvasId]) {
            this._charts[canvasId].destroy();
        }

        this._charts[canvasId] = new Chart(canvas.getContext('2d'), {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: colors,
                    borderColor: '#ffffff',
                    borderWidth: 3,
                    hoverBorderWidth: 0,
                    hoverOffset: 6,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '62%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 16,
                            usePointStyle: true,
                            pointStyle: 'circle',
                            font: {
                                family: 'DM Sans',
                                size: 12,
                                weight: '600',
                            },
                            color: '#757575',
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(255,255,255,0.95)',
                        titleColor: '#424242',
                        bodyColor: '#616161',
                        borderColor: '#F0F0F0',
                        borderWidth: 1,
                        padding: 12,
                        titleFont: { family: 'DM Sans', weight: '700' },
                        bodyFont: { family: 'DM Sans' },
                        callbacks: {
                            label: function (ctx) {
                                const total = ctx.dataset.data.reduce((a, b) => a + b, 0);
                                const pct = total > 0 ? ((ctx.parsed / total) * 100).toFixed(0) : 0;
                                return ` ${ctx.label}: ${ctx.parsed} (${pct}%)`;
                            }
                        }
                    }
                }
            }
        });
    },


    /* ---- Line trend chart (Chart.js) ---- */
    renderTrend: function (canvasId, labels, data) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        if (this._charts[canvasId]) {
            this._charts[canvasId].destroy();
        }

        this._charts[canvasId] = new Chart(canvas.getContext('2d'), {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Completed',
                    data: data,
                    borderColor: '#9CC103',
                    backgroundColor: function (context) {
                        const chart = context.chart;
                        const { ctx, chartArea } = chart;
                        if (!chartArea) return 'rgba(156,193,3,0.08)';
                        const gradient = ctx.createLinearGradient(0, chartArea.top, 0, chartArea.bottom);
                        gradient.addColorStop(0, 'rgba(156,193,3,0.18)');
                        gradient.addColorStop(1, 'rgba(156,193,3,0.02)');
                        return gradient;
                    },
                    borderWidth: 2.5,
                    pointBackgroundColor: '#9CC103',
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 2,
                    pointRadius: 4,
                    pointHoverRadius: 6,
                    fill: true,
                    tension: 0.35,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(255,255,255,0.95)',
                        titleColor: '#424242',
                        bodyColor: '#9CC103',
                        borderColor: '#F0F0F0',
                        borderWidth: 1,
                        padding: 12,
                        titleFont: { family: 'DM Sans', weight: '700' },
                        bodyFont: { family: 'DM Sans', weight: '600' },
                        callbacks: {
                            label: function (ctx) {
                                return ` ${ctx.parsed.y} completed`;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: {
                            color: '#BDBDBD',
                            font: { family: 'DM Sans', size: 11 },
                            maxRotation: 0,
                        }
                    },
                    y: {
                        beginAtZero: true,
                        grid: { color: '#F5F5F5' },
                        ticks: {
                            color: '#BDBDBD',
                            font: { family: 'DM Sans', size: 11 },
                            precision: 0,
                        }
                    }
                }
            }
        });
    },


    /* ---- Cleanup ---- */
    destroyAll: function () {
        for (const key in this._charts) {
            if (this._charts[key]) {
                this._charts[key].destroy();
            }
        }
        this._charts = {};
    }
};