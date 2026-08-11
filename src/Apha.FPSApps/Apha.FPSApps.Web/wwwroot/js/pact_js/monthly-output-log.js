// Monthly Output Log Page JavaScript

// ── Grid manager accessor ──────────────────────────────────────────
function getGridManager() {
    return window['gridManager_' + moLogGridId];
}

// ── Validation summary helpers ─────────────────────────────────────
function showMoLogError(message) {
    $('#moLogErrorList').empty().append('<li>' + message + '</li>');
    $('#moLogErrorSummary').show().focus();
}

function clearMoLogError() {
    $('#moLogErrorSummary').hide();
    $('#moLogErrorList').empty();
}

// ── Criteria check ─────────────────────────────────────────────────
function hasMoLogCriteria() {
    return !!(
        $('#ddWorkGroup').val()     ||
        $('#ddTestCode').val()      ||
        $('#ddBuyingProject').val() ||
        $('#txtBuyingTest').val()   ||
        $('#dtDateImported').val()  ||
        $('#txtMonth').val()        ||
        $('#txtUserId').val()       ||
        $('#ddAction').val()
    );
}

// ── Extra filters for _DataGrid gridManager ────────────────────────
// Called by _DataGrid.cshtml gridManager on every reload
// (pagination, sort, page-size). Returns ONLY search-panel values —
// page/sort/pageSize are owned by the grid manager itself.
function getExtraFilters_moLogGrid() {
    return {
        workGroup:    $('#ddWorkGroup').val()     || null,
        testCode:     $('#ddTestCode').val()      || null,
        buyer:        $('#ddBuyingProject').val() || null,
        buyingTest:   $('#txtBuyingTest').val()   || null,
        dateImported: $('#dtDateImported').val()  || null,
        month:        $('#txtMonth').val()        || null,
        userId:       $('#txtUserId').val()       || null,
        insertDelete: $('#ddAction').val()        || null
    };
}

// ── Button handlers ────────────────────────────────────────────────
$(function () {
    // Attach numeric validation to all decfmt-input fields
    // Note: This page doesn't have a form element, just individual filter inputs
    attachNumericValidation();

    $('#btnSearch').on('click', function () {
        clearMoLogError();

        if (!hasMoLogCriteria()) {
            showMoLogError('Please enter some criteria before searching.');
            return;
        }
        var gm = getGridManager();
        if (gm) {
            gm.reloadGrid({ page: 1 });
        }
    });

    $('#btnClearAll').on('click', function () {
        clearMoLogError();
        $('#ddWorkGroup').val('');
        $('#ddTestCode').val('');
        $('#ddBuyingProject').val('');
        $('#txtBuyingTest').val('');
        $('#txtMonth').val('');
        $('#dtDateImported').val('');
        $('#txtUserId').val('');
        $('#ddAction').val('');

        // Clear validation errors using shared function
        clearValidationErrors(document);

        // Reload the grid with empty criteria to show empty grid (no records)
        var gm = getGridManager();
        if (gm) {
            gm.reloadGrid({ page: 1 });
        }
    });
});
