// StaffJob.js - Shared staff job CRUD and charge rate calculation.
// Requires ajax-form-validation.js to be loaded before this script.
// Each page must configure StaffJobConfig before this script runs its event bindings.

var _hoursPerDay = 8;

var StaffJobConfig = {
    getJobCode: function () { return ''; },
    requireJobCodeForAdd: false,
    onSaved: function () { window.location.reload(); },
    onUpdated: function () { window.location.reload(); },
    onDeleted: function () { window.location.reload(); }
};

// ---- Staff Job CRUD ----

function addStaffJob(btn) {
    if (StaffJobConfig.requireJobCodeForAdd && !StaffJobConfig.getJobCode()) {
        showAlertMessage('Please select a project first.', AlertType.INFO);
        return;
    }
    showLoader();
    $.ajax({
        url: '/FPS/StaffJob/Create',
        type: 'GET',
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            hideLoader();
        },
        error: function (xhr) {
            hideLoader();
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                showAlertMessage('An error occurred while opening the form.', AlertType.ERROR);
            }
        }
    });
}

function saveStaffJob() {
    var form = $('#formAddStaff');
    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }
    showLoader();
    var staffId = $('#StaffID').val();
    var staffName = $('#Name').val();
    var data = {
        StaffID: staffId,
        JobCode: StaffJobConfig.getJobCode(),
        Name: staffName,
        ChargeRate: parseFloat($('#ChargeRate').val()) || 0,
        PlannedHours: parseFloat($('#PlannedHours').val()) || 0,
        Days: parseFloat($('#Days').val()) || 0,
        StaffCost: parseFloat($('#StaffCost').val()) || 0
    };
    $.ajax({
        url: '/FPS/StaffJob/Create',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            hideLoader();
            if (result.success) {
                closeModal();
                showAlertMessage(result.message, AlertType.SUCCESS).then(function () {                    
                    StaffJobConfig.onSaved();
                });
            } else {
                displayServerValidationErrors(result.errors, result.message, '#modaPopupBody');
            }
        },
        error: function (xhr) {
            hideLoader();
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                showAlertMessage('An error occurred while saving.', AlertType.ERROR);
            }
        }
    });
}

function editStaffJob(btn) {
    showLoader();
    var staffJobId = $(btn).data('id');
    $.ajax({
        url: '/FPS/StaffJob/Edit',
        type: 'GET',
        data: { staffId: staffJobId, jobCode: StaffJobConfig.getJobCode() },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            hideLoader();
        },
        error: function (xhr) {
            hideLoader();
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                showAlertMessage('An error occurred while fetching the record.', AlertType.ERROR);
            }
        }
    });
}

function updateStaffJob() {
    var form = $('#formEditStaff');
    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }
    showLoader();
    var staffId = $('#StaffID').val();
    var jobCode = form.find('[name="JobCode"]').val();
    var staffName = $('#Name').val();
    var data = {
        StaffID: staffId,
        JobCode: jobCode,
        Name: staffName,
        ChargeRate: parseFloat($('#ChargeRate').val()) || 0,
        PlannedHours: parseFloat($('#PlannedHours').val()) || 0,
        Days: parseFloat($('#Days').val()) || 0,
        StaffCost: parseFloat($('#StaffCost').val()) || 0
    };
    $.ajax({
        url: '/FPS/StaffJob/Edit?staffId=' + staffId,
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            hideLoader();
            if (result.success) {
                closeModal();
                showAlertMessage(result.message, AlertType.SUCCESS).then(function () {                   
                    StaffJobConfig.onUpdated();
                });
            } else {
                displayServerValidationErrors(result.errors, result.message, '#modaPopupBody');
            }
        },
        error: function (xhr) {
            hideLoader();
            if (xhr.status === 400 && xhr.responseJSON) {
                displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message, '#modaPopupBody');
            } else {
                showAlertMessage('An error occurred while saving.', AlertType.ERROR);
            }
        }
    });
}

function deleteStaffJob(btn) {

    var staffJobId = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this record?').then(function (confirmed) {
        if (!confirmed) { return; }
        showLoader();
        $.ajax({
            url: '/FPS/StaffJob/Delete',
            type: 'DELETE',
            data: { staffId: staffJobId, jobCode: StaffJobConfig.getJobCode() },
            success: function (response) {
                hideLoader();
                if (response.success) {
                    showAlertMessage('Deleted successfully.', AlertType.SUCCESS).then(function () {
                        StaffJobConfig.onDeleted();
                    });
                } else {
                    showAlertMessage(response.message, AlertType.ERROR);
                }
            },
            error: function () {
                hideLoader();
                showAlertMessage('An error occurred while deleting.', AlertType.ERROR);
            }
        });
    });
}

function getStaffJobExtraFilters() {
    return { jobCode: StaffJobConfig.getJobCode() };
}

// ---- Charge rate calculation ----

function onStaffSelected(selectElement) {
    var staffId = $(selectElement).val();
    var staffName = $(selectElement).find('option:selected').data('name');
    $('#StaffID').val(staffId);
   
    if (staffId) {
        fetchChargeRate(staffId);
    }
}

function fetchChargeRate(staffId) {
    var jobCode = StaffJobConfig.getJobCode();
    if (!staffId || !jobCode) { return; }
    var chargeRateField = $('#ChargeRate');
    chargeRateField.prop('disabled', true).val('');
    $.ajax({
        url: '/FPS/StaffJob/GetChargeRate',
        type: 'GET',
        data: { staffId: staffId, jobCode: jobCode },
        success: function (result) {
            chargeRateField.prop('disabled', false);
            chargeRateField.val(result.success ? result.chargeRate.toFixed(2) : '0.00');
            calculateStaffCost();
        },
        error: function () {
            chargeRateField.prop('disabled', false).val('0.00');
        }
    });
}

function fetchHoursPerDay() {
    $.ajax({
        url: '/FPS/Setting/GetHoursPerDay',
        type: 'GET',
        success: function (result) {
            if (result.success && result.hoursPerDay) {
                _hoursPerDay = result.hoursPerDay;
            }
        }
    });
}

function calculateStaffCost() {
    var rate = parseFloat($('#ChargeRate').val()) || 0;
    var hours = parseFloat($('#PlannedHours').val()) || 0;
    $('#StaffCost').val((rate * hours).toFixed(2));
    $('#Days').val((hours / _hoursPerDay).toFixed(2));
}

function calculateHoursFromDays() {
    var days = parseFloat($('#Days').val()) || 0;
    var hours = days * _hoursPerDay;
    $('#PlannedHours').val(hours.toFixed(2));
    var rate = parseFloat($('#ChargeRate').val()) || 0;
    $('#StaffCost').val((rate * hours).toFixed(2));
}

$(document).on('change', '#PlannedHours, #ChargeRate', function () {
    calculateStaffCost();
});

$(document).on('change', '#Days', function () {
    calculateHoursFromDays();
});

// Prevent non-numeric input on Hrs and Days fields using keypress event
$(document).on('keypress', '#PlannedHours, #Days', function (e) {
    var char = String.fromCharCode(e.which || e.keyCode);
    // Allow only digits (0-9) and decimal point (.)
    if (!/[\d.]/.test(char)) {
        e.preventDefault();
        return false;
    }
});

// Allow special keys like backspace, delete, arrows, tab, enter, ctrl shortcuts
$(document).on('keydown', '#PlannedHours, #Days', function (e) {
    var allowedKeys = [8, 9, 27, 13, 35, 36, 37, 38, 39, 40, 46]; // Backspace, Tab, Escape, Enter, End, Home, Arrow keys, Delete
    if (allowedKeys.indexOf(e.keyCode) !== -1) {
        return true;
    }
    // Allow Ctrl+A, Ctrl+C, Ctrl+X, Ctrl+V (copy/paste)
    if ((e.keyCode === 65 || e.keyCode === 67 || e.keyCode === 86 || e.keyCode === 88) && (e.ctrlKey || e.metaKey)) {
        return true;
    }
});

// Additional cleanup on input event to handle paste and other edge cases
$(document).on('input', '#PlannedHours, #Days', function () {
    var value = $(this).val();
    var filtered = value.replace(/[^\d.]/g, '');
    // Prevent multiple decimal points - keep only the first one
    var parts = filtered.split('.');
    if (parts.length > 2) {
        filtered = parts[0] + '.' + parts.slice(1).join('');
    }
    if (value !== filtered) {
        $(this).val(filtered);
    }
});

$(document).ready(function () {
    fetchHoursPerDay();
});

// ---- Modal helpers ----

function closeModal() {
    clearValidationErrors('#modaPopupBody');
    $('#modaPopupBody').html('');
    $('#modalPopup').removeClass('show');
}
