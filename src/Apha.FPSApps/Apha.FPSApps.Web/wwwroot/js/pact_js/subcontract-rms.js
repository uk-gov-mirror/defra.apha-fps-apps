let currentRmsMonth = initialRmsMonth ? parseInt(initialRmsMonth, 10) : null;

function getRmsSubContractsGridManager() {
    return window['gridManager_' + rmsSubContractsGridId];
}

function getRmsFailedSubContractsGridManager() {
    return window['gridManager_' + rmsFailedSubContractsGridId];
}

function getRmsSubContractFilters() {
    return {
        month: currentRmsMonth || ''
    };
}

function updateSelectedMonthText() {
    document.getElementById('txtSelectedMonth').value = document.getElementById('dpSelectmonth').value;
}

function reloadRmsGrid() {
    const gridManager = getRmsSubContractsGridManager();
    if (gridManager) {
        gridManager.reloadGrid({
            page: 1,
            sortBy: 'Project',
            descending: false
        });
    }
}

function reloadFailedGrid() {
    const gridManager = getRmsFailedSubContractsGridManager();
    if (gridManager) {
        gridManager.reloadGrid({
            page: 1,
            sortBy: 'Id',
            descending: false
        });
    }
}

function addSubContractRms() {
    if (!currentRmsMonth) {
        showAlertMessage('Please select a period first.', AlertType.INFO);
        return;
    }

    $.ajax({
        url: '/PACT/SubContractRms/GetSubContractRms',
        type: 'GET',
        data: { id: 0, month: currentRmsMonth },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            // Initialize form validation (unobtrusive + numeric)
            initializeFormValidation('#formAddProjectCost');
        },
        error: function () {
            showAlertMessage('Error loading form.', AlertType.ERROR);
        }
    });
}

function editSubContractRms(btn) {
    const id = $(btn).data('id');

    $.ajax({
        url: '/PACT/SubContractRms/GetSubContractRms',
        type: 'GET',
        data: { id: id, month: currentRmsMonth },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            // Initialize form validation (unobtrusive + numeric)
            initializeFormValidation('#formAddProjectCost');
        },
        error: function () {
            showAlertMessage('Error loading form.', AlertType.ERROR);
        }
    });
}

function deleteSubContractRms(btn) {
    const id = $(btn).data('id');
    showGovukConfirm('Delete this subcontract?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/SubContractRms/DeleteSubContractRms',
            type: 'DELETE',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    reloadRmsGrid();
                    showAlertMessage('SubContract deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Error: ' + response.message, AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting.', AlertType.ERROR);
            }
        });
    });
}

function saveProjectCost() {
    clearValidationErrors('#modaPopupBody');
    const form = $('#formAddProjectCost');

    // Check basic form validity (required fields)
    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    const data = form.serializeObject ? form.serializeObject() : Object.fromEntries(new FormData(form[0]));

    // Convert empty strings to null for decimal fields, but parse valid numbers
    ['Amount', 'DailyRate'].forEach(function (field) {
        if (data[field] === '' || data[field] === undefined) {
            data[field] = null;
        } else if (typeof data[field] === 'string') {
            var parsed = parseFloat(data[field]);
            data[field] = isNaN(parsed) ? null : parsed;
        }
    });

    // Convert empty strings to null for integer fields, but parse valid integers
    ['Month', 'SupplierNumber', 'AnimalDays'].forEach(function (field) {
        if (data[field] === '' || data[field] === undefined) {
            data[field] = null;
        } else if (typeof data[field] === 'string') {
            var parsed = parseInt(data[field], 10);
            data[field] = isNaN(parsed) ? null : parsed;
        }
    });

    $.ajax({
        url: '/PACT/SubContractRms/SaveSubContractRms',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                showAlertMessage(response.message || 'SubContract saved successfully.', AlertType.SUCCESS);
                reloadRmsGrid();
            } else {
                displayServerValidationErrors(response.errors, response.message, '#modaPopupBody');
            }
        },
        error: function () {
            showAlertMessage('An error occurred while saving.', AlertType.ERROR);
        }
    });
}

function downloadSubContractRmsTemplate() {
    const downloadUrl = '/PACT/SubContractRms/DownloadTemplate';

    $.ajax({
        url: downloadUrl,
        type: 'GET',
        xhrFields: {
            responseType: 'blob'
        },
        success: function (blob, status, xhr) {
            const disposition = xhr.getResponseHeader('Content-Disposition') || '';
            const fileNameMatch = disposition.match(/filename\*=UTF-8''([^;]+)|filename="?([^";]+)"?/i);
            const fileName = decodeURIComponent(fileNameMatch?.[1] || fileNameMatch?.[2] || 'SubContractRMS-Template.xlsx');

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);

            showAlertMessage('Template downloaded successfully. Please use the template to import Sub-Contract RMS data.', AlertType.INFO);
        },
        error: function () {
            showAlertMessage('Template download failed. Please try again.', AlertType.ERROR);
        }
    });
}

function importSubContractRms(file) {
    if (!file) {
        showAlertMessage('Please select an Excel file to import.', AlertType.INFO);
        return;
    }

    const formData = new FormData();
    formData.append('file', file);

    $.ajax({
        url: '/PACT/SubContractRms/Import',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                const msg = response.message || ('Import completed successfully. ' + (response.passedCount || 0) + 'records successfully validated and is now live');
                showAlertMessage(msg, AlertType.SUCCESS);
                reloadRmsGrid();
                reloadFailedGrid();
            } else {
                showAlertMessage(response.message || 'Import failed.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred while importing file.', AlertType.ERROR);
        }
    });
}

function deleteAllFailedSubContractRms() {
    showGovukConfirm('Delete all failed records?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/SubContractRms/DeleteAllFailedSubContractRms',
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    reloadFailedGrid();
                    showAlertMessage('Failed records deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage(response.message || 'Failed to delete failed records.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting failed records.', AlertType.ERROR);
            }
        });
    });
}

function editFailedSubContractRms(btn) {
    const id = $(btn).data('id');

    $.ajax({
        url: '/PACT/SubContractRms/GetFailedSubContractRms',
        type: 'GET',
        data: { id: id },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            // Initialize form validation after modal is shown
            setTimeout(function() {
                initializeFormValidation('#formEditFailedSubContractRms');
            }, 50);
        },
        error: function () {
            showAlertMessage('Error loading form.', AlertType.ERROR);
        }
    });
}

function deleteFailedSubContractRms(btn) {
    const id = $(btn).data('id');
    showGovukConfirm('Delete this failed record?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/SubContractRms/DeleteFailedSubContractRms',
            type: 'DELETE',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    reloadFailedGrid();
                    showAlertMessage('Failed record deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Error: ' + response.message, AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting.', AlertType.ERROR);
            }
        });
    });
}

function saveFailedSubContractRms() {
    clearValidationErrors('#modaPopupBody');
    const form = $('#formEditFailedSubContractRms');

    // Check basic form validity (required fields)
    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }

    const data = form.serializeObject ? form.serializeObject() : Object.fromEntries(new FormData(form[0]));

    // Note: SubContractRmsFailedItem model uses string? for all fields
    // Do NOT parse to numbers - keep as strings and let server handle validation
    // Only convert empty strings to null for optional fields
    ['SupplierNumber', 'DailyRate', 'AnimalDays'].forEach(function (field) {
        if (data[field] === '' || data[field] === undefined) {
            data[field] = null;
        }
    });

    $.ajax({
        url: '/PACT/SubContractRms/SaveFailedSubContractRms',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                showAlertMessage(response.message || 'Failed record saved successfully.', AlertType.SUCCESS);
                reloadFailedGrid();
                if (response.movedToSubContract) {
                    reloadRmsGrid();
                }
            } else {
                displayServerValidationErrors(response.errors, response.message, '#modaPopupBody');
            }
        },
        error: function () {
            showAlertMessage('An error occurred while saving.', AlertType.ERROR);
        }
    });
}

function exportFailedSubContractRms() {
    const gridManager = getRmsFailedSubContractsGridManager();
    const exportUrl = '/PACT/SubContractRms/ExportFailedSubContractRms';

    if (!gridManager) {
        showAlertMessage('Failed to export.', AlertType.ERROR);
        return;
    }

    $.ajax({
        url: exportUrl,
        type: 'GET',
        data: {
            filter: JSON.stringify(gridManager.getFilterModel())
        },
        xhrFields: {
            responseType: 'blob'
        },
        success: function (blob, status, xhr) {
            const disposition = xhr.getResponseHeader('Content-Disposition') || '';
            const fileNameMatch = disposition.match(/filename\*=UTF-8''([^;]+)|filename="?([^";]+)"?/i);
            const fileName = decodeURIComponent(fileNameMatch?.[1] || fileNameMatch?.[2] || 'SubContractRMS_failed.xlsx');

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
        },
        error: function () {
            showAlertMessage('Failed to export failed records.', AlertType.ERROR);
        }
    });
}

$(document).ready(function () {
    updateSelectedMonthText();

    $('#dpSelectmonth').on('change', function () {
        const value = this.value;
        currentRmsMonth = value ? parseInt(value, 10) : null;
        updateSelectedMonthText();
        reloadRmsGrid();
    });

    $('#templateExcel').on('click', function (e) {
        e.preventDefault();
        downloadSubContractRmsTemplate();
    });

    $('#csvInput').on('change', function () {
        const file = this.files && this.files[0];
        if (!file) return;

        importSubContractRms(file);
        $(this).val('');
    });

    $('#exportFailedBtn').on('click', function (e) {
        e.preventDefault();
        exportFailedSubContractRms();
    });

    $('#deleteAllFailedBtn').on('click', function (e) {
        e.preventDefault();
        deleteAllFailedSubContractRms();
    });
});

window.getRmsSubContractFilters = getRmsSubContractFilters;
window.addSubContractRms = addSubContractRms;
window.editSubContractRms = editSubContractRms;
window.deleteSubContractRms = deleteSubContractRms;
window.saveProjectCost = saveProjectCost;
window.downloadSubContractRmsTemplate = downloadSubContractRmsTemplate;
window.importSubContractRms = importSubContractRms;
window.deleteAllFailedSubContractRms = deleteAllFailedSubContractRms;
window.editFailedSubContractRms = editFailedSubContractRms;
window.deleteFailedSubContractRms = deleteFailedSubContractRms;
window.saveFailedSubContractRms = saveFailedSubContractRms;
window.exportFailedSubContractRms = exportFailedSubContractRms;
