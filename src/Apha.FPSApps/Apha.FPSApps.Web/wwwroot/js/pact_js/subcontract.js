// SubContract Page JavaScript

// ── State ──────────────────────────────────────────────────────────
// Note: currentParentProject, currentMonth, and subContractsGridId 
// are initialized in the Razor view to avoid flicker
// DO NOT redeclare them here - they are set via inline script in Index.cshtml

function getSubContractsGridManager() {
    return window['gridManager_' + subContractsGridId];
}

// ── Project dropdown change ────────────────────────────────────────
function onProjectPickChange(value) {
    document.getElementById('monthPick').value = '';
    currentParentProject = value || null;
    currentMonth = null;
    reloadSubContractsGrid();
}

// ── Month dropdown change ──────────────────────────────────────────
function onMonthPickChange(value) {
    document.getElementById('projectPick').value = '';
    currentMonth = value ? parseInt(value) : null;
    currentParentProject = null;
    reloadSubContractsGrid();
}

// ── Grid reload ────────────────────────────────────────────────────
function reloadSubContractsGrid() {
    var postData = {
        Page: 1,
        PageSize: 10,
        SortBy: 'Month',
        Descending: false,
        Filter: '{}',
        parentProject: currentParentProject || ''
    };

    // Only add month if it has a value
    if (currentMonth) {
        postData.month = currentMonth;
    }

    $.ajax({
        url: '/PACT/SubContract/LoadSubContractsGrid',
        type: 'POST',
        data: postData,
        success: function (html) {
            $('#gridContainer_subContractsGrid').html(html);
        },
        error: function () {
            showAlertMessage('Failed to load SubContracts grid.', AlertType.ERROR);
        }
    });
}

// ── Extra filter method (passed to gridManager for pagination/sort) ─
function getSubContractFilters() {
    return {
        parentProject: currentParentProject || '',
        month: currentMonth || ''
    };
}

// ── CRUD Functions ─────────────────────────────────────────────────
function addSubContract() {
    $.get('/PACT/SubContract/GetSubContract',
        { id: 0, parentProject: currentParentProject || '' },
        function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        })
        .fail(function(xhr, status, error) {
            showAlertMessage('Error loading form: ' + error, AlertType.ERROR);
        });
}

function editSubContract(btn) {
    var id = $(btn).data('id');
    $.get('/PACT/SubContract/GetSubContract', { id: id }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    })
    .fail(function(xhr, status, error) {
        showAlertMessage('Error loading form: ' + error, AlertType.ERROR);
    });
}

function deleteSubContract(btn) {
    var id = $(btn).data('id');
    showGovukConfirm('Delete this subcontract?').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/SubContract/DeleteSubContract',
            type: 'DELETE',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    reloadSubContractsGrid();
                    showAlertMessage('SubContract deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Error: ' + response.message, AlertType.ERROR);
                }
            },
            error: function () { showAlertMessage('An error occurred while deleting.', AlertType.ERROR); }
        });
    });
}

function saveSubContract() {
    clearValidationErrors('#modaPopupBody');
    var form = $('#subContractForm');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, '#modaPopupBody');
        return;
    }
    var data = form.serializeObject ? form.serializeObject() : Object.fromEntries(new FormData(form[0]));

    ['Month', 'Amount', 'SupplierNumber'].forEach(function (f) {
        if (data[f] === '' || data[f] === undefined) data[f] = null;
    });

    // Validate Amount field against PostgreSQL money limits
    if (data.Amount !== null && data.Amount !== undefined) {
        var amount = parseFloat(data.Amount);
        var maxMoney = 92233720368547758.07;

        if (isNaN(amount)) {
            showAlertMessage('The value you enter is not valid for this fields. The entered value is larger than the fieldsize permit.', AlertType.ERROR);
            return;
        }
        if (amount < 0 || amount > maxMoney) {
            showAlertMessage('The value you enter is not valid for this fields. The entered value is larger than the fieldsize permit.', AlertType.ERROR);
            return;
        }

        // Check decimal places
        var decimalPart = amount.toString().split('.')[1];
        if (decimalPart && decimalPart.length > 2) {
            showAlertMessage('Amount must have at most 2 decimal places.', AlertType.INFO);
            return;
        }

        // Ensure we send a proper decimal, not scientific notation
        data.Amount = amount;
    }

    $.ajax({
        url: '/PACT/SubContract/SaveSubContract',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                showAlertMessage(response.message || 'SubContract saved successfully.', AlertType.SUCCESS);
                reloadSubContractsGrid();
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
function filterSubContractsGrid(input) {
    var gm = getSubContractsGridManager();
    if (gm) gm.reloadGrid({ page: 1, search: input.value });
}

// ========================================
// Multi-Column Dropdown for SubContract Modal
// ========================================

function initializeSubContractProjectDropdown(config) {
    var isClearing = false;

     setTimeout(function () {
        var projectDropdown = new MultiColumnDropdownComponent({
            dropdownId: 'projectDropdown',
            containerSelector: '#projectMultiDropdown',
            placeholder: 'Select Project',
            showSerialNumber: false,
            searchPlaceholder: 'Search by code or title',
            labelText: '',
            required: true,
            columns: [
                { field: 'Text', header: 'Project Code', width: '120px' },
                { field: 'Value', header: 'Project Title', width: '300px' }
            ],
            data: config.projectsData || [],
            displayField: 'Text',
            valueField: 'Value',
            clearButtonClearsSelection: true,
            callbacks: {
                onSelect: function (selectedItem, dropdown) {
                    if (!isClearing) {
                        $('#Project').val(selectedItem.Value).trigger('change');
                        // Explicitly close dropdown if needed
                        setTimeout(function() {
                            if (dropdown && typeof dropdown.closeDropdown === 'function') {
                                dropdown.closeDropdown();
                            }
                        }, 50);
                    }
                },
                onClear: function (dropdown) {
                    if (!isClearing) {
                        isClearing = true;
                        $('#Project').val('').trigger('change');
                        $('#Project').val('');
                        setTimeout(function () {
                            isClearing = false;
                        }, 50);
                    }
                }

            }
        });
        const initialProject = $('#Project').val();
        if (initialProject) {
            projectDropdown.setValue(initialProject);
        }
     }, 100);
}
