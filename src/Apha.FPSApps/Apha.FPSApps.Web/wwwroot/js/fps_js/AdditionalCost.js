var AdditionalCostConfig = {
    getJobCode: function () { return ''; },
    requireJobCodeForAdd: false,
    onSaved: function () { },
    onUpdated: function () { },
    onDeleted: function () { }
};

function addAdditionalCost() {
    var jobCode = AdditionalCostConfig.getJobCode();
    if (AdditionalCostConfig.requireJobCodeForAdd && !jobCode) {
        showAlertMessage('Please select a project first.', AlertType.INFO);
        return;
    }
    showLoader();
    $.ajax({
        url: '/FPS/AdditionalCostJob/Create?jobCode=' + encodeURIComponent(jobCode),
        type: 'GET',
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            hideLoader();
        },
        error: function () {
            hideLoader();
            showAlertMessage('An error occurred while loading the form.', AlertType.ERROR);
        }
    });
}

function saveAdditionalCost() {
    var jobCode = AdditionalCostConfig.getJobCode();    
    var data = {
        JobCode: jobCode,
        Description: $('#Description').val(),
        Account: $('#Account').val(),
        ItemCost: parseFloat($('#ItemCost').val()) || 0,
        Freq: $('#Freq').val(),
        Supplier: $('#Supplier').val()
    };
    showLoader();
    $.ajax({
        url: '/FPS/AdditionalCostJob/Create',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            hideLoader();
            if (result.success) {
                showAlertMessage(result.message, AlertType.SUCCESS).then(function () {
                    closeModal();
                    AdditionalCostConfig.onSaved();
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

function editAdditionalCost(btn) {
    showLoader();
    var description = $(btn).data('id');
    var row = $(btn).closest('tr');
    var jobCode = AdditionalCostConfig.getJobCode();
    var account = row.find('td[data-property="Account"] span').text().trim();

    $.ajax({
        url: '/FPS/AdditionalCostJob/Edit',
        type: 'GET',
        data: { jobCode: jobCode, account: account, description: description },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            hideLoader();
        },
        error: function () {
            hideLoader();
            showAlertMessage('An error occurred while loading the form.', AlertType.ERROR);
        }
    });
}

function updateAdditionalCost() {
    var jobCode = AdditionalCostConfig.getJobCode();    
    var data = {
        JobCode: jobCode,
        Description: $('#Description').val(),
        OriginalDescription: $('#OriginalDescription').val(),
        Account: $('#Account').val(),
        OriginalAccount: $('#OriginalAccount').val(),
        ItemCost: parseFloat($('#ItemCost').val()) || 0,
        Freq: $('#Freq').val(),
        Supplier: $('#Supplier').val()
    };
    showLoader();
    $.ajax({
        url: '/FPS/AdditionalCostJob/Edit',
        type: 'POST',
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            hideLoader();
            if (result.success) {
                showAlertMessage(result.message, AlertType.SUCCESS).then(function () {
                    closeModal();
                    AdditionalCostConfig.onUpdated();
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
                showAlertMessage('An error occurred while updating.', AlertType.ERROR);
            }
        }
    });
}

function deleteAdditionalCost(btn) {
    var description = $(btn).data('id');
    var row = $(btn).closest('tr');
    var jobCode = AdditionalCostConfig.getJobCode();
    var account = row.find('td[data-property="Account"] span').text().trim();

    showGovukConfirm('Are you sure you want to delete this record?').then(function (confirmed) {
        if (!confirmed) { return; }
        showLoader();
        $.ajax({
            url: '/FPS/AdditionalCostJob/Delete',
            type: 'DELETE',
            data: { jobCode: jobCode, account: account, description: description },
            success: function (response) {
                hideLoader();
                if (response.success) {
                    showAlertMessage('Deleted successfully.', AlertType.SUCCESS).then(function () {
                        AdditionalCostConfig.onDeleted();
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

function getAdditionalCostExtraFilters() {
    return { jobCode: AdditionalCostConfig.getJobCode() };
}

function closeModal() {
    clearValidationErrors('#modaPopupBody');
    $('#modaPopupBody').html('');
    $('#modalPopup').removeClass('show');
}

// ---- Account multi-column dropdown ----

function toggleAccountPanel() {
    var panel = document.getElementById('AccountDropdownPanel');
    if (!panel) return;
    var isOpen = panel.style.display !== 'none';
    panel.style.display = isOpen ? 'none' : 'block';
    if (!isOpen) {
        var searchBox = document.getElementById('AccountSearchBox');
        if (searchBox) { searchBox.value = ''; filterAccountPanel(''); searchBox.focus(); }
    }
}

function filterAccountPanel(query) {
    var rows = document.querySelectorAll('#AccountDropdownBody tr');
    var q = (query || '').toLowerCase();
    rows.forEach(function (row) {
        row.style.display = (!q || row.textContent.toLowerCase().indexOf(q) !== -1) ? '' : 'none';
    });
}

function selectAccount(value, displayName, rowEl) {
    var display = document.getElementById('AccountDisplay');
    if (display) display.value = displayName;

    var select = document.getElementById('Account');
    if (select) {
        select.value = value;
        $(select).trigger('change');
    }

    var panel = document.getElementById('AccountDropdownPanel');
    if (panel) panel.style.display = 'none';
}

// Close panel when clicking outside
$(document).on('click', function (e) {
    if (!$(e.target).closest('#AccountDropdownPanel, #AccountDisplay').length) {
        var panel = document.getElementById('AccountDropdownPanel');
        if (panel) panel.style.display = 'none';
    }
});
