// Project Budget Profile page script

function goBack() {
    window.history.back();
}

function refreshGraph() {
    const project = document.getElementById('ParentProject').value;
    if (!project) return;
    loadProfileData(project);
    loadCumulativeData(project);
}

// ── Cost Profile Grid ─────────────────────────────────────────────────────

function loadCostProfileGrid(parentProject) {
    let url = '/PACT/ProjectProfile/LoadCostProfileGrid';
    if (parentProject) {
        url += '?parentProject=' + encodeURIComponent(parentProject);
    }

    $.ajax({
        url: url,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ filter: '{}' }),
        success: function (html) {
            const container = document.getElementById('gridContainer_costProfileGrid');
            container.innerHTML = '';
            container.appendChild(document.createRange().createContextualFragment(html));
            updateTotalCostProfile(parentProject);
        },
        error: function () {
            console.error('Failed to load cost profile grid.');
        }
    });
}

function updateTotalCostProfile(parentProject) {
    const input = document.getElementById('TotalCostProfile');
    if (!parentProject) {
        input.value = '';
        return;
    }

    $.ajax({
        url: '/PACT/ProjectProfile/GetTotalCostProfile?parentProject=' + encodeURIComponent(parentProject),
        method: 'GET',
        success: function (res) {
            if (res.success) {
                input.value = parseFloat(res.data).toFixed(2);
            }
        }
    });
}

// ── Graph Data ────────────────────────────────────────────────────────────

let nonCumulativeChart = null;
let cumulativeChart = null;

function loadProfileData(parentProject) {
    if (!parentProject) return;

    $.ajax({
        url: '/PACT/ProjectProfile/GetProfileData?parentProject=' + encodeURIComponent(parentProject),
        method: 'GET',
        success: function (res) {
            if (!res.success || !res.data) return;

            const labels = res.data.map(d => 'Month ' + d.monthNo);
            const profileData = res.data.map(d => d.profile || 0);
            const costData = res.data.map(d => d.totalCost || 0);

            if (nonCumulativeChart) {
                nonCumulativeChart.destroy();
            }

            const ctx = document.getElementById('nonCumulativeChart').getContext('2d');
            nonCumulativeChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels,
                    datasets: [
                        {
                            label: 'Profile',
                            data: profileData,
                            borderColor: 'rgba(0, 0, 255, 1)',
                            backgroundColor: 'rgba(0, 0, 255, 1)',
                            fill: false,
                            tension: 0,
                            borderWidth: 1,
                            pointStyle: 'rectRot',
                            pointRadius: 5,
                            pointHoverRadius: 7
                        },
                        {
                            label: 'Cost',
                            data: costData,
                            borderColor: 'rgba(255, 0, 255, 1)',
                            backgroundColor: 'rgba(255, 0, 255, 1)',
                            fill: false,
                            tension: 0,
                            borderWidth: 1,
                            pointStyle: 'rect',
                            pointRadius: 5,
                            pointHoverRadius: 7
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'top',
                            labels: {
                                color: "Black"
                            }
                        }
                    },
                    scales: {
                        x: {
                            ticks: {
                                maxTicksLimit: 12,
                                color: '#0b0c0c'
                            }
                        },
                        y: {
                            beginAtZero: true,
                            ticks: {
                                maxTicksLimit: 7,
                                color: '#0b0c0c',
                                callback: value => value.toLocaleString()
                            }
                        }
                    }
                }
            });
        }
    });
}

function loadCumulativeData(parentProject) {
    if (!parentProject) return;

    $.ajax({
        url: '/PACT/ProjectProfile/GetCumulativeData?parentProject=' + encodeURIComponent(parentProject),
        method: 'GET',
        success: function (res) {
            if (!res.success || !res.data) return;

            const labels = res.data.map(d => 'Month ' + d.monthNo);
            const cumProfileData = res.data.map(d => d.cumulativeProfile || 0);
            const cumCostData = res.data.map(d => d.cumulativeCost || 0);

            if (cumulativeChart) {
                cumulativeChart.destroy();
            }

            const ctx = document.getElementById('cumulativeChart').getContext('2d');
            cumulativeChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels,
                    datasets: [
                        {
                            label: 'Profile',
                            data: cumProfileData,
                            borderColor: 'rgba(0, 0, 255, 1)',
                            backgroundColor: 'rgba(0, 0, 255, 1)',
                            fill: false,
                            tension: 0,
                            borderWidth: 1,
                            pointStyle: 'rectRot',
                            pointRadius: 5,
                            pointHoverRadius: 7
                        },
                        {
                            label: 'Cost',
                            data: cumCostData,
                            borderColor: 'rgba(255, 0, 255, 1)',
                            backgroundColor: 'rgba(255, 0, 255, 1)',
                            fill: false,
                            tension: 0,
                            borderWidth: 1,
                            pointStyle: 'rect',
                            pointRadius: 5,
                            pointHoverRadius: 7
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'top',
                            labels: {
                                color: '#000000'
                            }
                        }
                    },
                    scales: {
                        x: {
                            ticks: {
                                maxTicksLimit: 12,
                                color: '#000000'
                            }
                        },
                        y: {
                            beginAtZero: true,
                            ticks: {
                                maxTicksLimit: 6,
                                color: '#000000',
                                callback: value => value.toLocaleString()
                            }
                        }
                    }
                }
            });
        }
    });
}

// ── CRUD helpers

// Store modal state outside of jQuery .data()
let _modalProject = '';

function addProjectMonth() {
    const project = document.getElementById('ParentProject').value;
    if (!project) { showAlertMessage('Please select a project first.', AlertType.INFO); return; }
    openCostProfileModal(project, 0);
}

function editProjectMonth(btn) {
    const monthNo = parseInt(btn.getAttribute('data-id')) || 0;
    const project = document.getElementById('ParentProject').value;
    openCostProfileModal(project, monthNo);
}

function openCostProfileModal(project, monthNo) {
    _modalProject = project;

    const url = '/PACT/ProjectProfile/GetProjectMonth?project=' + encodeURIComponent(project) + '&monthNo=' + monthNo;

    $.ajax({
        url: url,
        method: 'GET',
        success: function (html) {
            const content = document.getElementById('costProfileModalContent');
            content.innerHTML = '';
            content.appendChild(document.createRange().createContextualFragment(html));
            document.getElementById('costProfileModal').classList.add('show');
            // Initialize form validation (unobtrusive + numeric)
            initializeFormValidation('#projectMonthForm');
        },
        error: function (xhr) {
            showAlertMessage('Failed to load cost profile form: HTTP ' + xhr.status + ' – ' + url, AlertType.ERROR);
        }
    });
}

function saveProjectMonth() {
    const form = document.getElementById('projectMonthForm');
    if (!form) return;

    const costProfileInput = form.querySelector('[name="CostProfile"]')?.value || '';
    const costProfileValue = costProfileInput.trim() === '' ? null : parseFloat(costProfileInput);

    const payload = {
        project:     form.querySelector('[name="Project"]')?.value,
        monthNo:     parseInt(form.querySelector('[name="MonthNo"]')?.value) || 0,
        costProfile: costProfileValue
    };

    if (!payload.monthNo) { showAlertMessage('Please enter a month number.', AlertType.INFO); return; }

    $.ajax({
        url: '/PACT/ProjectProfile/SaveProjectMonth',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.success) {
                document.getElementById('costProfileModal').classList.remove('show');
                loadCostProfileGrid(_modalProject);
                loadProfileData(_modalProject);
                loadCumulativeData(_modalProject);
            } else {
                showAlertMessage(res.message || 'Failed to save.', AlertType.ERROR);
            }
        }
    });
}

function deleteProjectMonth(btn) {
    const monthNo = parseInt(btn.getAttribute('data-id')) || 0;
    const project = document.getElementById('ParentProject').value;
    showGovukConfirm('Delete month ' + monthNo + '?').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/ProjectProfile/DeleteProjectMonth?project=' + encodeURIComponent(project) + '&monthNo=' + monthNo,
            method: 'DELETE',
            success: function (res) {
                if (res.success) {
                    loadCostProfileGrid(project);
                    showAlertMessage('Month deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage(res.message || 'Failed to delete.', AlertType.ERROR);
                }
            }
        });
    });
}

// ── Document Ready ────────────────────────────────────────────────────────

function loadProjectDetails(project) {
    const titleInput  = document.getElementById('ProjectTitle');
    const budgetInput = document.getElementById('BudgetCvl');

    if (!project) {
        if (titleInput)  titleInput.value  = '';
        if (budgetInput) budgetInput.value = '';
        return;
    }

    $.ajax({
        url: '/PACT/ProjectProfile/GetProjectDetailsAsync?parentProject=' + encodeURIComponent(project),
        method: 'GET',
        success: function (res) {
            if (res.success) {
                if (titleInput)  titleInput.value  = res.projectTitle  ?? '';
                if (budgetInput) budgetInput.value = res.budgetCvl     ?? '';
            }
        },
        error: function (err) {
            showAlertMessage('Failed to load project details.', AlertType.ERROR);
        }
    });
}

document.addEventListener('DOMContentLoaded', () => {

    // Bind grid on dropdown change
    document.getElementById('ParentProject').addEventListener('change', function () {
        const project = this.value;
        loadProjectDetails(project);
        loadCostProfileGrid(project);
        loadProfileData(project);
        loadCumulativeData(project);
    });

    // Bind grid on page load if a project is already selected
    const initialProject = document.getElementById('ParentProject').value;
    if (initialProject) {
        loadProjectDetails(initialProject);
        loadCostProfileGrid(initialProject);
        loadProfileData(initialProject);
        loadCumulativeData(initialProject);
    }


    // Close modal when clicking outside the dialog
    document.getElementById('costProfileModal').addEventListener('click', function (e) {
        if (e.target === this) closeCostProfileModal();
    });
});

function closeCostProfileModal() {
    document.getElementById('costProfileModal').classList.remove('show');
}
