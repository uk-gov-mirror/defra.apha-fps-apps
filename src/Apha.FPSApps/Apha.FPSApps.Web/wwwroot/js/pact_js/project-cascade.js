// Project Cascade - JavaScript Module
// Manages Job Code, Time Code Valid, and Monthly Time grids with CRUD operations

// ── State ─────────────────────────────────────────────────────────────────────
let selectedProject = '';
let selectedJobCode = '';
let selectedTimeCode = '';
let selectedWorkGroup = '';

// ── Initialisation ────────────────────────────────────────────────────────────
function initProjectCascade(initialProject, projectsData) {
    selectedProject = initialProject || '';

    new MultiColumnDropdownComponent({
        dropdownId: 'projectDropdown',
        containerSelector: '#projectSelectDropdown',
        placeholder: 'Select Project',
        showSerialNumber: false,
        searchPlaceholder: 'Search by project name',
        labelText: '',
        columns: [
            { field: 'code', header: 'Project Code', width: '80px' },
            { field: 'title', header: 'Project Name', width: '120px' }
        ],
        data: projectsData || [],
        displayField: 'code',
        valueField: 'code',
        callbacks: {
            onSelect: function (selectedItem) {
                selectedProject = selectedItem.code;
                document.getElementById('txtSelectedProjectcode').value = selectedItem.code;
                document.getElementById('txtSelectedProjectTitle').value = selectedItem.title;
                resetPanel2();
                resetPanel3();
                loadJobCodeGrid(selectedItem.code);
            }
        }
    });

    if (selectedProject) {
        loadJobCodeGrid(selectedProject);
    }
}

// ── Grid loaders ──────────────────────────────────────────────────────────────
function loadJobCodeGrid(project) {
    fetchGrid('/PACT/ProjectCascade/LoadJobCodeGrid', { parentProject: project },
        'gridContainer_JobcodeBelongsToProjectGrid');
}

function loadTimeCodeGrid(project, jobCode) {
    fetchGrid('/PACT/ProjectCascade/LoadTimeCodeGrid', { parentProject: project, jobCodeId: jobCode },
        'gridContainer_TimeCodeValidOptionGrid');
}

function loadMonthlyTimeGrid(project, timeCode, workGroup) {
    fetchGrid('/PACT/ProjectCascade/LoadMonthlyTimeGrid',
        { parentProject: project, timeCode: timeCode, workGroup: workGroup },
        'gridContainer_TimeRecordsGrid');
}

function fetchGrid(url, data, containerId) {
    const payload = Object.assign({ filter: '{}', pageNumber: 1, pageSize: 10 }, data);
    $.post(url, payload, function (html) {
        document.getElementById(containerId).innerHTML = html;
    });
}

// ── Row select callbacks ──────────────────────────────────────────────────────
function selectJobcode(row) {
    selectedJobCode = row.jobCodeId ?? row.JobCodeId;
    document.getElementById('txtSelectedJobcode').value = selectedJobCode;
    resetPanel3();
    loadTimeCodeGrid(selectedProject, selectedJobCode);
}

function selectTimecode(row) {
    selectedTimeCode = row.timeCode ?? row.TimeCode;
    selectedWorkGroup = row.workGroup ?? row.WorkGroup;
    document.getElementById('txtSelectedProjectcodeTwo').value = selectedProject;
    document.getElementById('txtSelectedWorkGroup').value = selectedWorkGroup;
    document.getElementById('txtSelectedTimeCode').value = selectedTimeCode;
    loadMonthlyTimeGrid(selectedProject, selectedTimeCode, selectedWorkGroup);
}

function resetPanel2() {
    selectedJobCode = '';
    document.getElementById('txtSelectedJobcode').value = '';
    resetPanel3();
    loadTimeCodeGrid('', '');
}

function resetPanel3() {
    selectedTimeCode = '';
    selectedWorkGroup = '';
    document.getElementById('txtSelectedProjectcodeTwo').value = '';
    document.getElementById('txtSelectedWorkGroup').value = '';
    document.getElementById('txtSelectedTimeCode').value = '';
    loadMonthlyTimeGrid('', '', '');
}

// ── Job Code CRUD ─────────────────────────────────────────────────────────────
function addJobcode() {
    if (!selectedProject) {
        showGovukAlert('Please select a project first.');
        return;
    }
    $.get('/PACT/ProjectCascade/CreateJobCode', { parentProject: selectedProject }, function (html) {
        document.getElementById('jobcodeModalContent').innerHTML = html;
        document.getElementById('jobcodeModal').classList.add('show');
    });
}

function editJobcode(row) {
    $.get('/PACT/ProjectCascade/EditJobCode', { jobCodeId: row.jobCodeId ?? row.JobCodeId }, function (html) {
        document.getElementById('jobcodeModalContent').innerHTML = html;
        document.getElementById('jobcodeModal').classList.add('show');
    });
}

function deleteJobcode(row) {
    const jobCodeId = row.jobCodeId ?? row.JobCodeId;
    showGovukConfirm(`Are you sure you want to delete job code "${jobCodeId}"?`).then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: `/PACT/ProjectCascade/DeleteJobCode?jobCodeId=${encodeURIComponent(jobCodeId)}`,
            method: 'DELETE',
            success: function (r) {
                if (r.success) { resetPanel2(); loadJobCodeGrid(selectedProject); }
                else showGovukAlert(r.message || 'Delete failed.');
            }
        });
    });
}

function closeJobcodeModal() {
    document.getElementById('jobcodeModal').classList.remove('show');
}

function saveJobcode() {
    const form = document.getElementById('formJobcode');
    const isEdit = form.querySelector('[name=isEdit]').value === 'true';
    const url = isEdit ? '/PACT/ProjectCascade/EditJobCode' : '/PACT/ProjectCascade/CreateJobCode';
    const data = formToJson(form);

    $.ajax({
        url, method: 'POST', contentType: 'application/json', data: JSON.stringify(data),
        success: function (r) {
            if (r.success) { closeJobcodeModal(); loadJobCodeGrid(selectedProject); }
            else showFormErrors('formJobcode-db-error', 'formJobcode-db-error-msg', r);
        }
    });
}

// ── Time Code CRUD ────────────────────────────────────────────────────────────
function addTimecode() {
    if (!selectedJobCode) {
        showGovukAlert('Please select a job code first.');
        return;
    }
    $.get('/PACT/ProjectCascade/CreateTimeCode',
        { parentProject: selectedProject, jobCodeId: selectedJobCode }, function (html) {
            document.getElementById('timecodeModalContent').innerHTML = html;
            document.getElementById('timecodeModal').classList.add('show');
        });
}

function editTimecode(row) {
    $.get('/PACT/ProjectCascade/EditTimeCode',
        { timeCode: row.timeCode ?? row.TimeCode, workGroup: row.workGroup ?? row.WorkGroup, parentProject: selectedProject },
        function (html) {
            document.getElementById('timecodeModalContent').innerHTML = html;
            document.getElementById('timecodeModal').classList.add('show');
        });
}

function deleteTimecode(row) {
    const timeCode = row.timeCode ?? row.TimeCode;
    const workGroup = row.workGroup ?? row.WorkGroup;
    showGovukConfirm(`Are you sure you want to delete time code "${timeCode}"?`).then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: `/PACT/ProjectCascade/DeleteTimeCode?timeCode=${encodeURIComponent(timeCode)}&workGroup=${encodeURIComponent(workGroup)}&parentProject=${encodeURIComponent(selectedProject)}`,
            method: 'DELETE',
            success: function (r) {
                if (r.success) { resetPanel3(); loadTimeCodeGrid(selectedProject, selectedJobCode); }
                else showGovukAlert(r.message || 'Delete failed.');
            }
        });
    });
}

function closeTimecodeModal() {
    document.getElementById('timecodeModal').classList.remove('show');
}

function saveTimecode() {
    const form = document.getElementById('formTimecode');
    const isEdit = form.querySelector('[name=isEdit]').value === 'true';
    const url = isEdit ? '/PACT/ProjectCascade/EditTimeCode' : '/PACT/ProjectCascade/CreateTimeCode';
    const data = formToJson(form);

    $.ajax({
        url, method: 'POST', contentType: 'application/json', data: JSON.stringify(data),
        success: function (r) {
            if (r.success) { closeTimecodeModal(); loadTimeCodeGrid(selectedProject, selectedJobCode); }
            else showFormErrors('formTimecode-db-error', 'formTimecode-db-error-msg', r);
        }
    });
}

// ── Monthly Time CRUD ─────────────────────────────────────────────────────────
function addTimeentry() {
    if (!selectedTimeCode) {
        showGovukAlert('Please select a time code first.');
        return;
    }
    $.get('/PACT/ProjectCascade/CreateMonthlyTime',
        { parentProject: selectedProject, timeCode: selectedTimeCode, workGroup: selectedWorkGroup },
        function (html) {
            document.getElementById('timeentryModalContent').innerHTML = html;
            document.getElementById('timeentryModal').classList.add('show');
        });
}

function editTimeentry(row) {
    $.get('/PACT/ProjectCascade/EditMonthlyTime', {
        pactStaffId: row.pactStaffId ?? row.PactStaffId,
        timeCode: row.timeCode ?? row.TimeCode,
        month: row.month ?? row.Month,
        parentProject: selectedProject
    }, function (html) {
        document.getElementById('timeentryModalContent').innerHTML = html;
        document.getElementById('timeentryModal').classList.add('show');
    });
}

function deleteTimeentry(row) {
    const pactStaffId = row.pactStaffId ?? row.PactStaffId;
    const timeCode = row.timeCode ?? row.TimeCode;
    const month = row.month ?? row.Month;
    showGovukConfirm(`Are you sure you want to delete this time entry for staff "${pactStaffId}"?`).then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: `/PACT/ProjectCascade/DeleteMonthlyTime?pactStaffId=${encodeURIComponent(pactStaffId)}&timeCode=${encodeURIComponent(timeCode)}&month=${month}&parentProject=${encodeURIComponent(selectedProject)}`,
            method: 'DELETE',
            success: function (r) {
                if (r.success) { loadMonthlyTimeGrid(selectedProject, selectedTimeCode, selectedWorkGroup); }
                else showGovukAlert(r.message || 'Delete failed.');
            }
        });
    });
}

function closeTimeentryModal() {
    document.getElementById('timeentryModal').classList.remove('show');
}

function saveTimeentry() {
    const form = document.getElementById('formTimeentry');
    const isEdit = form.querySelector('[name=isEdit]').value === 'true';
    const url = isEdit ? '/PACT/ProjectCascade/EditMonthlyTime' : '/PACT/ProjectCascade/CreateMonthlyTime';
    const data = formToJson(form);

    $.ajax({
        url, method: 'POST', contentType: 'application/json', data: JSON.stringify(data),
        success: function (r) {
            if (r.success) { closeTimeentryModal(); loadMonthlyTimeGrid(selectedProject, selectedTimeCode, selectedWorkGroup); }
            else showFormErrors('formTimeentry-db-error', 'formTimeentry-db-error-msg', r);
        }
    });
}

// ── Helpers ───────────────────────────────────────────────────────────────────
function formToJson(form) {
    const fd = new FormData(form);
    const obj = {};
    fd.forEach((v, k) => { obj[k] = v; });
    return obj;
}

function showFormErrors(errorDivId, errorMsgId, response) {
    const msgEl = document.getElementById(errorMsgId);
    const divEl = document.getElementById(errorDivId);
    if (msgEl && divEl) {
        msgEl.textContent = response.message || 'An error occurred.';
        divEl.style.display = '';
    }
}

