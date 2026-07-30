// Invoice Recording Page JavaScript

// ── State ──────────────────────────────────────────────────────────
// Note: currentParentProject, currentMonth, and invoicesGridId 
// are initialized in the Razor view to avoid flicker
// DO NOT redeclare them here - they are set via inline script in Index.cshtml

function getInvoicesGridManager() {
    return window['gridManager_' + invoicesGridId];
}

// ── Project dropdown change ────────────────────────────────────────
function onProjectPickChange(value) {
    document.getElementById('monthPick').value = '';
    currentParentProject = value || null;
    currentMonth = null;
    reloadInvoicesGrid();
}

// ── Month dropdown change ──────────────────────────────────────────
function onMonthPickChange(value) {
    document.getElementById('projectPick').value = '';
    currentMonth = value || null;
    currentParentProject = null;
    reloadInvoicesGrid();
}

// ── Grid reload ────────────────────────────────────────────────────
function reloadInvoicesGrid() {
    $.ajax({
        url: '/PACT/Invoice/LoadInvoicesGrid',
        type: 'POST',
        data: {
            Page: 1,
            PageSize: 10,
            SortBy: 'Month',
            Descending: false,
            Filter: '{}',
            parentProject: currentParentProject || '',
            month: currentMonth || ''
        },
        success: function (html) {
            $('#gridContainer_invoicesGrid').html(html);
        },
        error: function () {
            console.error('Failed to load Invoices grid.');
        }
    });
}

// ── Extra filter method (passed to gridManager for pagination/sort) ─
function getInvoiceFilters() {
    return {
        parentProject: currentParentProject || '',
        month: currentMonth || ''
    };
}

// ── CRUD Functions ─────────────────────────────────────────────────
function addInvoice() {
    $.get('/PACT/Invoice/GetInvoice',
        { id: 0, parentProject: currentParentProject || '' },
        function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        })
        .fail(function(xhr, status, error) {
            showAlertMessage('Error loading form: ' + error, AlertType.ERROR);
        });
}

function editInvoice(btn) {
    var id = $(btn).data('id');
    $.get('/PACT/Invoice/GetInvoice', { id: id }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    })
    .fail(function(xhr, status, error) {
        showAlertMessage('Error loading form: ' + error, AlertType.ERROR);
    });
}

function deleteInvoice(btn) {
    var id = $(btn).data('id');
    showGovukConfirm('Delete this invoice?').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/Invoice/DeleteInvoice',
            type: 'DELETE',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    reloadInvoicesGrid();
                    showAlertMessage('Invoice deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Error: ' + response.message, AlertType.ERROR);
                }
            },
            error: function () { showAlertMessage('An error occurred while deleting.', AlertType.ERROR); }
        });
    });
}

function saveInvoice() {
    clearValidationErrors('#modaPopupBody');
    var form = $('#invoiceForm');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }
    var data = form.serializeObject ? form.serializeObject() : Object.fromEntries(new FormData(form[0]));

    ['Month', 'Amount', 'CostOfWork', 'Wip', 'ProfitLoss'].forEach(function (f) {
        if (data[f] === '' || data[f] === undefined) data[f] = null;
    });

    // Validate money fields against PostgreSQL money limits
    var moneyFields = ['Amount', 'CostOfWork', 'Wip', 'ProfitLoss'];
    var maxMoney = 92233720368547758.07;

    for (var i = 0; i < moneyFields.length; i++) {
        var fieldName = moneyFields[i];
        var fieldValue = data[fieldName];

        if (fieldValue !== null && fieldValue !== undefined) {
            var parsedValue = parseFloat(fieldValue);

            if (isNaN(parsedValue)) {
                showAlertMessage('The value you enter is not valid for this fields. The entered value is larger than the fieldsize permit.', AlertType.ERROR);
                return;
            }
            if (parsedValue < 0 || parsedValue > maxMoney) {
                showAlertMessage('The value you enter is not valid for this fields. The entered value is larger than the fieldsize permit.', AlertType.ERROR);
                return;
            }

            // Check decimal places
            var decimalPart = parsedValue.toString().split('.')[1];
            if (decimalPart && decimalPart.length > 2) {
                showAlertMessage(fieldName.replace(/([A-Z])/g, ' $1').trim() + ' must have at most 2 decimal places.', AlertType.INFO);
                return;
            }

            // Ensure we send a proper decimal, not scientific notation
            data[fieldName] = parsedValue;
        }
    }

    $.ajax({
        url: '/PACT/Invoice/SaveInvoice',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                showAlertMessage(response.message || 'Invoice saved successfully.', AlertType.SUCCESS);
                reloadInvoicesGrid();
            } else {
                displayServerValidationErrors(response.errors, response.message, '#modaPopupBody');
            }
        },
        error: function () { 
            showAlertMessage('An error occurred while saving.', AlertType.ERROR); 
        }
    });
}

// ── Search (if needed) ─────────────────────────────────────────────
function filterInvoicesGrid(input) {
    var gm = getInvoicesGridManager();
    if (gm) gm.reloadGrid({ page: 1, search: input.value });
}
