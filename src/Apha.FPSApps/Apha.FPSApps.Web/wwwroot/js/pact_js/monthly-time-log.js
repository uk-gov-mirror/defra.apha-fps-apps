// Monthly Time Log of Imports Page JavaScript

// -- Grid manager accessor --
function getGridManager() {
    return window['gridManager_' + mtLogGridId];
}

// -- Validation summary helpers --
function showMtLogError(message) {
    $('#mtLogErrorList').empty().append('<li>' + message + '</li>');
    $('#mtLogErrorSummary').show().focus();
}

function clearMtLogError() {
    $('#mtLogErrorSummary').hide();
    $('#mtLogErrorList').empty();
}

// -- Criteria check --
function hasMtLogCriteria() {
    return !!(
        $('#ddWorkGroup').val()    ||
        $('#hdnProject').val()     ||
        $('#txtMonth').val()       ||
        $('#hdnJobCode').val()     ||
        $('#ddTestCode').val()     ||
        $('#hdnStaffId').val()     ||
        $('#dtDateImported').val() ||
        $('#txtUserId').val()      ||
        $('#ddAction').val()
    );
}

// -- Extra filters for _DataGrid gridManager --
// Called by _DataGrid.cshtml gridManager on every reload (pagination, sort, page-size).
function getExtraFilters_mtLogGrid() {
    var jobCode  = $('#hdnJobCode').val() || null;
    var testCode = $('#ddTestCode').val() || null;
    var timeCode = jobCode || testCode || null;

    return {
        workGroup:     $('#ddWorkGroup').val()    || null,
        timeCode:      timeCode,
        parentProject: $('#hdnProject').val()     || null,
        pactStaffId:   $('#hdnStaffId').val()     || null,
        dateImported:  $('#dtDateImported').val() || null,
        month:         $('#txtMonth').val()       || null,
        userId:        $('#txtUserId').val()      || null,
        insertDelete:  $('#ddAction').val()       || null
    };
}

// -- Generic multicolumn dropdown wiring --
function initMultiColumnDropdown(wrapperId, inputId, hiddenId, panelId, bodyId, onSelect) {
    var $wrapper = $('#' + wrapperId);
    var $input   = $('#' + inputId);
    var $hidden  = $('#' + hiddenId);
    var $panel   = $('#' + panelId);
    var $rows    = $('#' + bodyId + ' tr');

    $input.on('click', function (e) {
        e.stopPropagation();
        $('.multicolumn-dropdown-panel').not($panel).hide();
        $panel.toggle();
    });

    $rows.on('click', function () {
        var value = $(this).data('value');
        var label = $(this).find('td').map(function () {
            return $(this).text().trim();
        }).get().filter(Boolean).join(' \u2014 ');
        $input.val(label);
        $hidden.val(value);
        $panel.hide();
        $panel.find('.search-box').val('');
        $rows.show();
        $panel.find('.clear-search-btn').hide();
        if (onSelect) onSelect(value);
    });

    $panel.find('.search-box').on('input', function () {
        var term = $(this).val().toLowerCase();
        $rows.each(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(term) !== -1);
        });
        $panel.find('.clear-search-btn').toggle($(this).val().length > 0);
    });

    $panel.find('.clear-search-btn').on('click', function (e) {
        e.stopPropagation();
        $panel.find('.search-box').val('').trigger('input');
    });

    $(document).on('click', function (e) {
        if (!$wrapper.is(e.target) && $wrapper.find(e.target).length === 0) {
            $panel.hide();
        }
    });
}

// -- Button handlers --
$(function () {

    // Initialize form validation (unobtrusive + numeric)
    initializeFormValidation('#invoiceForm');

    initMultiColumnDropdown(
        'projectPickerWrapper', 'txtProjectPick', 'hdnProject',
        'projectDropdownPanel', 'projectDropdownBody', null);

    initMultiColumnDropdown(
        'jobCodePickerWrapper', 'txtJobCodePick', 'hdnJobCode',
        'jobCodeDropdownPanel', 'jobCodeDropdownBody',
        function (value) {
            if (value) { $('#ddTestCode').val(''); }
        });

    initMultiColumnDropdown(
        'staffPickerWrapper', 'txtStaffPick', 'hdnStaffId',
        'staffDropdownPanel', 'staffDropdownBody', null);

    $('#ddTestCode').on('change', function () {
        if ($(this).val()) {
            $('#txtJobCodePick').val('');
            $('#hdnJobCode').val('');
        }
    });

    $('#btnSearch').on('click', function () {
        clearMtLogError();

        // Check for numeric validation errors before searching
        const hasNumericErrors = $('#txtMonth').hasClass('govuk-input--error');
        if (hasNumericErrors) {
            showMtLogError('Please correct the validation errors before searching.');
            return;
        }

        if (!hasMtLogCriteria()) {
            showMtLogError('Please enter some criteria before searching.');
            return;
        }
        var gm = getGridManager();
        if (gm) { gm.reloadGrid({ page: 1 }); }
    });

    $('#btnClearAll').on('click', function () {
        clearMtLogError();
        $('#ddWorkGroup').val('');
        $('#txtProjectPick').val('');
        $('#hdnProject').val('');
        $('#txtMonth').val('');
        $('#txtJobCodePick').val('');
        $('#hdnJobCode').val('');
        $('#ddTestCode').val('');
        $('#txtStaffPick').val('');
        $('#hdnStaffId').val('');
        $('#dtDateImported').val('');
        $('#txtUserId').val('');
        $('#ddAction').val('');

        // Clear any validation errors on the month field
        const monthInput = $('#txtMonth');
        const formGroup = monthInput.closest('.govuk-form-group');
        const errorMsg = $('#txtMonth-error');

        formGroup.removeClass('govuk-form-group--error');
        monthInput.removeClass('govuk-input--error');
        errorMsg.hide().text('');

        var gm = getGridManager();
        if (gm) { gm.reloadGrid({ page: 1 }); }
    });
});
