// AnimalJob.js - Shared animal plan CRUD and rate calculation.
// Requires ajax-form-validation.js to be loaded before this script.
// Each page must configure AnimalJobConfig before this script runs its event bindings.

var AnimalJobConfig = {
    getJobCode: function () { return ''; },
    requireJobCodeForAdd: false,
    onSaved: function () { window.location.reload(); },
    onUpdated: function () { window.location.reload(); },
    onDeleted: function () { window.location.reload(); }
};

// ---- Animal Plan CRUD ----

function addAnimalPlan(btn) {
    if (AnimalJobConfig.requireJobCodeForAdd && !AnimalJobConfig.getJobCode()) {
        showAlertMessage('Please select a project first.', AlertType.INFO);
        return;
    }
    showLoader();
    $.ajax({
        url: '/FPS/AnimalJob/Create',
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

function saveAnimalPlan() {
    var form = $('#formAddAnimalPlan');
    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }
    showLoader();
    var data = {
        IndCounter: 0,
        JobCode: AnimalJobConfig.getJobCode(),
        AnimalType: $('#AnimalType').val(),
        NumberOfDays: parseFloat($('#NumberOfDays').val()) || 0,
        NumberOfAnimals: parseFloat($('#NumberOfAnimals').val()) || 0,
        DailyRate: parseFloat($('#DailyRate').val()) || 0,
        AnimalCost: parseFloat($('#AnimalCost').val()) || 0
    };
    $.ajax({
        url: '/FPS/AnimalJob/Create',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            hideLoader();
            if (result.success) {
                closeModal();
                showAlertMessage(result.message, AlertType.SUCCESS).then(function () {                   
                    AnimalJobConfig.onSaved();
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

function editAnimalPlan(btn) {
    showLoader();
    var indCounter = $(btn).data('id');
    $.ajax({
        url: '/FPS/AnimalJob/Edit',
        type: 'GET',
        data: { indCounter: indCounter, jobCode: AnimalJobConfig.getJobCode() },
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

function updateAnimalPlan() {
    var form = $('#formEditAnimalPlan');
    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }
    showLoader();
    var indCounter = $('#IndCounter').val();
    var jobCode = form.find('[name="JobCode"]').val();
    var data = {
        IndCounter: parseInt(indCounter) || 0,
        JobCode: jobCode,
        AnimalType: $('#AnimalType').val(),
        NumberOfDays: parseFloat($('#NumberOfDays').val()) || 0,
        NumberOfAnimals: parseFloat($('#NumberOfAnimals').val()) || 0,
        DailyRate: parseFloat($('#DailyRate').val()) || 0,
        AnimalCost: parseFloat($('#AnimalCost').val()) || 0
    };
    $.ajax({
        url: '/FPS/AnimalJob/Edit?indCounter=' + indCounter,
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            hideLoader();
            if (result.success) {
                closeModal();
                showAlertMessage(result.message, AlertType.SUCCESS).then(function () {                    
                    AnimalJobConfig.onUpdated();
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

function deleteAnimalPlan(btn) {
    var indCounter = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this animal cost entry?').then(function (confirmed) {
        if (!confirmed) { return; }
        showLoader();
        $.ajax({
            url: '/FPS/AnimalJob/Delete',
            type: 'DELETE',
            data: { indCounter: indCounter },
            success: function (response) {
                hideLoader();
                if (response.success) {
                    showAlertMessage('Deleted successfully.', AlertType.SUCCESS).then(function () {
                        AnimalJobConfig.onDeleted();
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

function getAnimalPlanExtraFilters() {
    return { jobCode: AnimalJobConfig.getJobCode() };
}

// ---- Rate calculation ----

function onAnimalTypeSelected(selectElement) {
    var animalType = $(selectElement).val();
    if (!animalType) {
        $('#DailyRate').val('');
        $('#AnimalCost').val('');
        return;
    }
    var rateField = $('#DailyRate');
    rateField.prop('disabled', true).val('');
    $.ajax({
        url: '/FPS/AnimalJob/GetAnimalRate',
        type: 'GET',
        data: { animalType: animalType, jobCode: AnimalJobConfig.getJobCode() },
        success: function (result) {
            rateField.prop('disabled', false);
            rateField.val(result.success ? result.dailyRate.toFixed(2) : '0.00');
            calculateAnimalCost();
        },
        error: function () {
            rateField.prop('disabled', false).val('0.00');
        }
    });
}

function calculateAnimalCost() {
    var days = parseFloat($('#NumberOfDays').val()) || 0;
    var animals = parseFloat($('#NumberOfAnimals').val()) || 0;
    var rate = parseFloat($('#DailyRate').val()) || 0;
    $('#AnimalCost').val(((days * animals)* rate).toFixed(4));
}

$(document).on('change', '#NumberOfDays, #NumberOfAnimals', function () {
    calculateAnimalCost();
});

// ---- Modal helpers ----

function closeModal() {
    clearValidationErrors('#modaPopupBody');
    $('#modaPopupBody').html('');
    $('#modalPopup').removeClass('show');
}
