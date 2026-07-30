// ── State ───────────────────────────────────────────────────────────────────
var selectedTestCode = '';
var selectedGeneralProfitCentre = '';

// ── Grid reload helpers ──────────────────────────────────────────────────────

function reloadTestListVlaGrid() {
    var gm = window['gridManager_testListVlaGrid'];
    if (gm) { gm.reloadGrid({ page: 1 }); }
}

function reloadTestRequirementsGrid() {
    var gm = window['gridManager_testRequirementsGrid'];
    if (gm) { gm.reloadGrid({ page: 1 }); }
}

function reloadComponentChargesGeneralGrid() {
    var gm = window['gridManager_componentChargesGeneralGrid'];
    if (gm) { gm.reloadGrid({ page: 1 }); }
}

function reloadComponentChargesProjectGrid() {
    var gm = window['gridManager_componentChargesProjectGrid'];
    if (gm) { gm.reloadGrid({ page: 1 }); }
}

function reloadSuppliersGrid() {
    var gm = window['gridManager_suppliersGrid'];
    if (gm) { gm.reloadGrid({ page: 1 }); }
}

function reloadAllTabGrids() {
    reloadTestRequirementsGrid();
    reloadComponentChargesGeneralGrid();
    reloadComponentChargesProjectGrid();
    reloadSuppliersGrid();
}

// ── Extra-filter methods (called automatically by _DataGrid on every reload) ─

function getTestListVlaExtraFilters() {
    return {};
}

function getTestRequirementExtraFilters() {
    return { testCode: selectedTestCode };
}

function getComponentChargesExtraFilters() {
    return { testCode: selectedTestCode };
}

function getComponentChargesProjectExtraFilters() {
    return {
        testCode: selectedTestCode,
        profitCentre: selectedGeneralProfitCentre
    };
}

function getSuppliersExtraFilters() {
    return { testCode: selectedTestCode };
}

// ── Parent row selection ──────────────────────────────────────────────────────

function selectTestListVlaRow(btn) {
    var $row = $(btn).closest('tr');
    var itemCode = $row.data('id') || $(btn).data('id');
    selectedTestCode = itemCode || '';
    selectedGeneralProfitCentre = '';

    // Highlight selected row
    $('#tbl_testListVlaGrid tbody tr').removeClass('selected-row');
    if ($row.length) {
        $row.addClass('selected-row');
    }

    // Show VLA Unit Price from selected row
    var unitPriceText = $row.find('td[data-property="UnitPriceVla"] span').text().trim();
    $('#stage2-vla-unit-price').val(unitPriceText);

    // Clear total while component charges grid reloads
    $('#stage2-component-total').val('');

    reloadAllTabGrids();
}

function updateComponentTotalPrice() {
    var total = 0;
    $('#tbl_componentChargesGeneralGrid tbody tr').each(function () {
        var priceText = $(this).find('td[data-property="Price"] span').text().trim();
        var parsed = parseFloat(priceText.replace(/[£,]/g, ''));
        if (!isNaN(parsed)) { total += parsed; }
    });
    var formatted = total === 0 ? ''
        : '\u00A3' + total.toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    $('#stage2-component-total').val(formatted);
}

function selectFirstTestListRowIfAvailable() {
    var $firstRow = $('#tbl_testListVlaGrid tbody tr').filter(function () {
        return $(this).data('id') !== undefined && $(this).data('id') !== null && $(this).data('id') !== '';
    }).first();

    if ($firstRow.length === 0) {
        selectedTestCode = '';
        selectedGeneralProfitCentre = '';
        $('#stage2-vla-unit-price').val('');
        $('#stage2-component-total').val('');
        reloadAllTabGrids();
        return;
    }

    selectTestListVlaRow($firstRow[0]);
}

function selectComponentChargeGeneralRow(btn) {
    var $row = $(btn).closest('tr');
    var profitCentre = $row.data('id') || $(btn).data('id');
    selectedGeneralProfitCentre = profitCentre || '';

    $('#tbl_componentChargesGeneralGrid tbody tr').removeClass('selected-row');
    if ($row.length) {
        $row.addClass('selected-row');
    }

    reloadComponentChargesProjectGrid();
}

function selectFirstComponentChargeGeneralRowIfAvailable() {
    var $firstRow = $('#tbl_componentChargesGeneralGrid tbody tr').filter(function () {
        return $(this).data('id') !== undefined && $(this).data('id') !== null && $(this).data('id') !== '';
    }).first();

    if ($firstRow.length === 0) {
        selectedGeneralProfitCentre = '';
        reloadComponentChargesProjectGrid();
        return;
    }

    selectComponentChargeGeneralRow($firstRow[0]);
}

// ── Grid-reloaded event handler ───────────────────────────────────────────────

document.addEventListener('gridReloaded', function (event) {
    if (!event.detail) {
        return;
    }

    if (event.detail.gridId === 'testListVlaGrid') {
        selectFirstTestListRowIfAvailable();
    }

    if (event.detail.gridId === 'componentChargesGeneralGrid') {
        selectFirstComponentChargeGeneralRowIfAvailable();
        updateComponentTotalPrice();
    }
});

// ── Initialise ────────────────────────────────────────────────────────────────

$(function () {
    reloadTestListVlaGrid();
});
