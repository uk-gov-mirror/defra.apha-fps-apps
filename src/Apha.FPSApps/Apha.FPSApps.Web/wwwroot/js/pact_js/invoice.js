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
            // Initialize form validation (unobtrusive + numeric)
            initializeFormValidation('#invoiceForm');
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
        // Initialize form validation (unobtrusive + numeric)
        initializeFormValidation('#invoiceForm');
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

    // Convert empty strings to null for numeric fields, but parse valid numbers
    ['Amount', 'CostOfWork', 'Wip', 'ProfitLoss'].forEach(function (f) {
        if (data[f] === '' || data[f] === undefined) {
            data[f] = null;
        } else if (typeof data[f] === 'string') {
            var parsed = parseFloat(data[f]);
            data[f] = isNaN(parsed) ? null : parsed;
        }
    });

    // Parse Month as integer
    if (data['Month'] === '' || data['Month'] === undefined) {
        data['Month'] = null;
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
