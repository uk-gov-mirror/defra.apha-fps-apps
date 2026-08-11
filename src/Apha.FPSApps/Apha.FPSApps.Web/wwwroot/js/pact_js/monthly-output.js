// ── Grid accessors ────────────────────────────────────────────────────────────

function getLiveGridManager() {
    return window['gridManager_' + monthlyOutputLiveGridId];
}

function getStagingGridManager() {
    return window['gridManager_' + monthlyOutputStagingGridId];
}

// ── Filter helpers ────────────────────────────────────────────────────────────

function getSelectedWorkGroup() {
    return $('#ddWorkGroup').val() || null;
}

function getMonthlyOutputLiveFilters() {
    return {
        workGroup: getSelectedWorkGroup(),
        testCode:  $('#ddTestCode').val() || null,
        buyer:     $('#ddBuyer').val()    || null,
        month:     $('#ddMonth').val()    || null
    };
}

function getMonthlyOutputStagingFilters() {
    return {
        passed: window.monthlyOutputPassedFilter ?? null
    };
}

// ── Grid reload ───────────────────────────────────────────────────────────────

function reloadLiveGrid() {
    const gm = getLiveGridManager();
    if (gm) {
        gm.reloadGrid({ page: 1, sortBy: 'WorkGroup', descending: false });
        scheduleAlignTotalVolumeFields();
    }
}

function reloadStagingGrid() {
    const gm = getStagingGridManager();
    if (gm) {
        gm.reloadGrid({ page: 1, sortBy: 'Id', descending: false });
        scheduleAlignTotalVolumeFields();
    }
}

function alignTotalVolumeBox(gridId, rowContainerId, inputId, labelSelector) {
    const volumeTh = document.querySelector('#tbl_' + gridId + ' [data-column="Volume"]');
    const gridContainer = document.getElementById('gridContainer_' + gridId);
    const rowContainer = document.getElementById(rowContainerId);
    const input = document.getElementById(inputId);
    const label = document.querySelector(labelSelector);

    if (!volumeTh || !gridContainer || !rowContainer || !input || !label) return;

    const thRect = volumeTh.getBoundingClientRect();
    const containerLeft = gridContainer.getBoundingClientRect().left;
    const rightOffset = thRect.right - containerLeft;

    rowContainer.style.display = 'flex';
    rowContainer.style.alignItems = 'center';

    label.style.whiteSpace = 'nowrap';
    label.style.marginLeft = Math.max(0, rightOffset - label.offsetWidth - 8 - input.offsetWidth) + 'px';
    label.style.marginRight = '8px';

    input.style.flexShrink = '0';
}

function alignTotalVolumeFields() {
    alignTotalVolumeBox(monthlyOutputLiveGridId, 'divMakeliveTotalvolume', 'txtMakeliveTotalvolume', '#divMakeliveTotalvolume .total-volume-label');
    alignTotalVolumeBox(monthlyOutputStagingGridId, 'divStagingTotalvolume', 'txtTotalvolume', '#lbltotalvolume');
}

function updateTotalVolumeValues() {
    const liveTotal = parseFloat($('#gridContainer_' + monthlyOutputLiveGridId + ' .editable-grid-container').data('grid-total'));
    const stagingTotal = parseFloat($('#gridContainer_' + monthlyOutputStagingGridId + ' .editable-grid-container').data('grid-total'));

    $('#txtMakeliveTotalvolume').val(Number.isFinite(liveTotal) ? liveTotal.toFixed(2) : '0.00');
    $('#txtTotalvolume').val(Number.isFinite(stagingTotal) ? stagingTotal.toFixed(2) : '0.00');
}

function scheduleAlignTotalVolumeFields() {
    window.requestAnimationFrame(function () {
        alignTotalVolumeFields();
        updateTotalVolumeValues();
        setTimeout(function () {
            alignTotalVolumeFields();
            updateTotalVolumeValues();
        }, 120);
        setTimeout(function () {
            alignTotalVolumeFields();
            updateTotalVolumeValues();
        }, 350);
    });
}

function clearLiveSearch() {
    $('#ddWorkGroup').val('');
    $('#ddTestCode').val('');
    $('#ddBuyer').val('');
    $('#ddMonth').val('');

    if (window.monthlyOutputWorkGroupDropdown) window.monthlyOutputWorkGroupDropdown.clear();
    resetTestCodeOptions();
    resetBuyerOptions();
}

// ── Utility ───────────────────────────────────────────────────────────────────

function readDropdownJsonData(selector) {
    const $json = $(selector);
    if (!$json.length) return [];
    try {
        const parsed = JSON.parse($json.text());
        return Array.isArray(parsed) ? parsed : [];
    } catch {
        return [];
    }
}

// ── Work Group dropdown (filter panel) ───────────────────────────────────────

function initWorkGroupDropdown() {
    const workGroups = readDropdownJsonData('#monthly-output-workgroups-data');

    window.monthlyOutputWorkGroupDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'monthlyOutputWorkGroup',
        containerSelector: '#workGroupSelectDropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        columns: [
            { field: 'text', header: 'Work Group', width: '240px' }
        ],
        data: workGroups,
        displayField: function (row) { return row.text || ''; },
        valueField: function (row) { return row.value || ''; },
        enableSearch: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {
                const workGroup = selectedItem?.value || '';
                $('#ddWorkGroup').val(workGroup);
                loadTestCodesByWorkGroup(workGroup);
                resetBuyerOptions();
            },
            onClear: function () {
                $('#ddWorkGroup').val('');
                resetTestCodeOptions();
                resetBuyerOptions();
            }
        }
    });
}

function resetTestCodeOptions() {
    $('#ddTestCode').val('');
    if (window.monthlyOutputTestCodeDropdown) {
        window.monthlyOutputTestCodeDropdown.clear();
        window.monthlyOutputTestCodeDropdown.updateData([]);
    }
}

function resetBuyerOptions() {
    $('#ddBuyer').val('');
    if (window.monthlyOutputBuyerDropdown) {
        window.monthlyOutputBuyerDropdown.clear();
        window.monthlyOutputBuyerDropdown.updateData([]);
    }
}

function initTestCodeDropdown() {
    window.monthlyOutputTestCodeDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'monthlyOutputTestCode',
        containerSelector: '#testCodeSelectDropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        columns: [
            { field: 'text', header: 'Test Code', width: '220px' }
        ],
        data: [],
        displayField: function (row) { return row.text || ''; },
        valueField: function (row) { return row.value || ''; },
        enableSearch: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {
                const testCode = selectedItem?.value || '';
                $('#ddTestCode').val(testCode);
                const workGroup = getSelectedWorkGroup();
                loadBuyersByTestCode(workGroup, testCode);
            },
            onClear: function () {
                $('#ddTestCode').val('');
                resetBuyerOptions();
            }
        }
    });
}

function initBuyerDropdown() {
    window.monthlyOutputBuyerDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'monthlyOutputBuyer',
        containerSelector: '#buyerSelectDropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        columns: [
            { field: 'text',     header: 'Buyer',     width: '200px' },
            { field: 'testCode', header: 'Test Code', width: '120px' }
        ],
        data: [],
        displayField: function (row) { return row.text || ''; },
        valueField: function (row) { return row.value || ''; },
        enableSearch: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {
                $('#ddBuyer').val(selectedItem?.value || '');
            },
            onClear: function () {
                $('#ddBuyer').val('');
            }
        }
    });
}

function loadTestCodesByWorkGroup(workGroup) {
    resetTestCodeOptions();
    if (!workGroup) return;

    $.ajax({
        url: '/PACT/MonthlyOutput/GetTestCodesByWorkGroup',
        type: 'GET',
        data: { workGroup: workGroup },
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            if (window.monthlyOutputTestCodeDropdown)
                window.monthlyOutputTestCodeDropdown.updateData(items);
        },
        error: function () {
            showAlertMessage('Failed to load test code options.', AlertType.ERROR);
        }
    });
}

function loadBuyersByTestCode(workGroup, testCode) {
    resetBuyerOptions();
    if (!workGroup || !testCode) return;

    $.ajax({
        url: '/PACT/MonthlyOutput/GetBuyersByTestCode',
        type: 'GET',
        data: { workGroup: workGroup, testCode: testCode },
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            if (window.monthlyOutputBuyerDropdown)
                window.monthlyOutputBuyerDropdown.updateData(items);
        },
        error: function () {
            showAlertMessage('Failed to load buyer options.', AlertType.ERROR);
        }
    });
}

// ── Live grid edit ────────────────────────────────────────────────────────────

function parseOutputCompositeKey(key) {
    const parts = (key || '').split('|');
    return {
        testCode:  parts[0] || '',
        buyer:     parts[1] || '',
        month:     parts[2] || '',
        workGroup: parts[3] || ''
    };
}

function editMonthlyOutputLive(btn) {
    const key = $(btn).data('id');
    const parsed = parseOutputCompositeKey(key);
    $.ajax({
        url: '/PACT/MonthlyOutput/GetLiveRecord',
        type: 'GET',
        data: parsed,
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            // Initialize form validation (unobtrusive + numeric)
            initializeFormValidation('#stagingMonthlyOutputForm');
            initLiveModalWorkGroupDropdown();
        },
        error: function () {
            showAlertMessage('Failed to load monthly output record.', AlertType.ERROR);
        }
    });
}

function initLiveModalWorkGroupDropdown() {
    const workGroups = readDropdownJsonData('#live-modal-workgroups-data');
    const existingWorkGroup = $('#LiveWorkGroup').val();

    window.liveOutputWorkGroupDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'liveOutputWorkGroup',
        containerSelector: '#live-modal-workgroup-dropdown-container',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: 'Work Group <span class="app-required" aria-hidden="true">*</span>',
        columns: [
            { field: 'text', header: 'Work Group', width: '200px' }
        ],
        data: workGroups,
        displayField: function (row) { return row.text || ''; },
        valueField: function (row) { return row.value || ''; },
        enableSearch: true,
        disabled: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {
                $('#LiveWorkGroup').val(selectedItem?.value || '');
            },
            onClear: function () {
                $('#LiveWorkGroup').val('');
            }
        }
    });

    if (existingWorkGroup) {
        window.liveOutputWorkGroupDropdown.setValue(existingWorkGroup);
    }
}

function saveMonthlyOutputLive() {
    var form = $('#monthlyOutputLiveForm');

    // Validate all numeric fields before checking isFormValid
    form.find('.decfmt-input').each(function() {
        validateRangeOnInput(this);
    });

    // Check for numeric validation errors
    if (hasNumericValidationErrors(form)) {
        // Ensure validation messages are visible
        if (typeof ensureValidationMessagesVisible === 'function') {
            ensureValidationMessagesVisible(form);
        }
        displayClientValidationErrors(form, form);
        return;
    }

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, form);
        return;
    }

    // Clear validation errors only after validation passes
    clearValidationErrors(form);

    const data = {
        CompositeKey: $('#CompositeKey').val(),
        WorkGroup:    $('#LiveWorkGroup').val(),
        TestCode:     $('#LiveTestCode').val(),
        Buyer:        $('#LiveBuyer').val(),
        Month:        $('#LiveMonth').val(),
        Volume:       $('#LiveVolume').val()
    };

    $.ajax({
        url: '/PACT/MonthlyOutput/SaveLiveRecord',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                reloadLiveGrid();
                showAlertMessage(response.message, AlertType.SUCCESS);
            } else if (response.errors) {
                displayServerValidationErrors(response.errors, response.message || 'Validation failed.', form);
            } else {
                showAlertMessage(response.message || 'Update failed.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred while saving.', AlertType.ERROR);
        }
    });
}

function deleteLiveOutputRecord(btn) {
    const key = $(btn).data('id');
    const parsed = parseOutputCompositeKey(key);

    showGovukConfirm('Are you sure you want to delete this live record?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/MonthlyOutput/DeleteLiveRecord',
            type: 'DELETE',
            data: parsed,
            success: function (response) {
                if (response.success) {
                    reloadLiveGrid();
                    showAlertMessage('Record deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Failed to delete record.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting the record.', AlertType.ERROR);
            }
        });
    });
}

// ── Staging grid edit ─────────────────────────────────────────────────────────

function addStagingMonthlyOutput() {
    $.ajax({
        url: '/PACT/MonthlyOutput/AddStagingRecord',
        type: 'GET',
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            // Initialize form validation (unobtrusive + numeric)
            initializeFormValidation('#stagingMonthlyOutputForm');
            initStagingModalDropdowns(null, null, null);
        },
        error: function () {
            showAlertMessage('Failed to load add form.', AlertType.ERROR);
        }
    });
}

function editStagingMonthlyOutput(btn) {
    const id = $(btn).data('id');
    $.ajax({
        url: '/PACT/MonthlyOutput/GetStagingRecord',
        type: 'GET',
        data: { id: id },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            $('#modalPopup').addClass('show');
            // Initialize form validation (unobtrusive + numeric)
            initializeFormValidation('#stagingMonthlyOutputForm');
            const workGroup  = $('#StagingWorkGroup').val();
            const existingTestCode = $('#StagingTestCode').val();
            const existingBuyer    = $('#StagingBuyer').val();
            initStagingModalDropdowns(workGroup, existingTestCode, existingBuyer);
        },
        error: function () {
            showAlertMessage('Failed to load staging record.', AlertType.ERROR);
        }
    });
}

function initStagingModalDropdowns(existingWorkGroup, existingTestCode, existingBuyer) {
    const wgData       = readDropdownJsonData('#staging-modal-workgroups-data');
    const isEditMode   = !!(existingTestCode || existingBuyer || existingWorkGroup);
    const allTestCodes = isEditMode ? readDropdownJsonData('#staging-modal-testcodes-data') : [];
    const allBuyers    = isEditMode ? readDropdownJsonData('#staging-modal-buyers-data')    : [];

    window.stagingOutputWorkGroupDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'stagingOutputWorkGroup',
        containerSelector: '#staging-modal-workgroup-dropdown-container',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: 'Work Group <span class="app-required" aria-hidden="true">*</span>',
        columns: [
            { field: 'text', header: 'Work Group', width: '200px' }
        ],
        data: wgData,
        displayField: function (row) { return row.text || ''; },
        valueField: function (row) { return row.value || ''; },
        enableSearch: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {
                const selectedWorkGroup = selectedItem?.value || '';
                $('#StagingWorkGroup').val(selectedWorkGroup);

                const isInitialRestore = existingWorkGroup && selectedWorkGroup === existingWorkGroup;
                if (isInitialRestore) return;

                // User changed WorkGroup — filter TestCodes via AJAX, preserving existing selections in edit mode
                loadStagingModalTestCodesByWorkGroup(
                    selectedWorkGroup,
                    isEditMode ? existingTestCode : null,
                    isEditMode ? existingBuyer    : null
                );
            },
            onClear: function () {
                $('#StagingWorkGroup').val('');
                if (isEditMode) {
                    // Edit mode: restore full unfiltered lists from embedded data
                    seedStagingModalTestCodesFromData(allTestCodes, null, allBuyers, null);
                } else {
                    // Add mode: clear both dropdowns
                    resetStagingModalTestCodeOptions();
                    resetStagingModalBuyerOptions();
                }
            }
        }
    });

    window.stagingOutputTestCodeDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'stagingOutputTestCode',
        containerSelector: '#staging-modal-testcode-dropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: 'Test Code <span class="app-required" aria-hidden="true">*</span>',
        columns: [
            { field: 'text', header: 'Test Code', width: '260px' }
        ],
        data: isEditMode ? allTestCodes : [],
        displayField: function (row) { return row.text || ''; },
        valueField: function (row) { return row.value || ''; },
        enableSearch: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {
                if (window._stagingSkipTestCodeOnSelect) return;
                const testCode  = selectedItem?.value || '';
                const workGroup = $('#StagingWorkGroup').val() || null;
                $('#StagingTestCode').val(testCode);
                // User changed TestCode — filter Buyers via AJAX, preserving existing buyer in edit mode
                loadStagingModalBuyersByTestCode(
                    workGroup,
                    testCode,
                    isEditMode ? existingBuyer : null
                );
            },
            onClear: function () {
                $('#StagingTestCode').val('');
                const workGroup = $('#StagingWorkGroup').val() || null;
                if (workGroup) {
                    // Reload buyers filtered to workGroup via AJAX
                    loadStagingModalBuyersByTestCode(workGroup, null);
                } else if (isEditMode) {
                    // Edit mode, no workGroup: restore full buyer list from embedded data
                    seedStagingModalBuyersFromData(allBuyers, null);
                } else {
                    // Add mode: clear buyers
                    resetStagingModalBuyerOptions();
                }
            }
        }
    });

    window.stagingOutputBuyerDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'stagingOutputBuyer',
        containerSelector: '#staging-modal-buyer-dropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: 'Buyer <span class="app-required" aria-hidden="true">*</span>',
        columns: [
            { field: 'text', header: 'Buyer', width: '260px' }
        ],
        data: isEditMode ? allBuyers : [],
        displayField: function (row) { return row.text || ''; },
        valueField: function (row) { return row.value || ''; },
        enableSearch: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {
                $('#StagingBuyer').val(selectedItem?.value || '');
            },
            onClear: function () {
                $('#StagingBuyer').val('');
            }
        }
    });

    if (existingWorkGroup) {
        window.stagingOutputWorkGroupDropdown.setValue(existingWorkGroup);
    }

    if (isEditMode) {
        // Edit mode: restore TestCode and Buyer synchronously from embedded data — no AJAX needed
        if (existingTestCode) {
            window._stagingSkipTestCodeOnSelect = true;
            window.stagingOutputTestCodeDropdown.setValue(existingTestCode);
            window._stagingSkipTestCodeOnSelect = false;
            $('#StagingTestCode').val(existingTestCode);
        }
        if (existingBuyer) {
            window.stagingOutputBuyerDropdown.setValue(existingBuyer);
            $('#StagingBuyer').val(existingBuyer);
        }
    }
}


function resetStagingModalTestCodeOptions() {
    $('#StagingTestCode').val('');
    if (window.stagingOutputTestCodeDropdown) {
        window.stagingOutputTestCodeDropdown.clear();
        window.stagingOutputTestCodeDropdown.updateData([]);
    }
}

function resetStagingModalBuyerOptions() {
    $('#StagingBuyer').val('');
    if (window.stagingOutputBuyerDropdown) {
        window.stagingOutputBuyerDropdown.clear();
        window.stagingOutputBuyerDropdown.updateData([]);
    }
}

function seedStagingModalTestCodesFromData(testCodes, restoreTestCode, buyers, restoreBuyer) {
    if (window.stagingOutputTestCodeDropdown) {
        window.stagingOutputTestCodeDropdown.updateData(testCodes);
        if (restoreTestCode && testCodes.some(function (item) { return item.value === restoreTestCode; })) {
            window._stagingSkipTestCodeOnSelect = true;
            window.stagingOutputTestCodeDropdown.setValue(restoreTestCode);
            window._stagingSkipTestCodeOnSelect = false;
            $('#StagingTestCode').val(restoreTestCode);
        }
    }
    seedStagingModalBuyersFromData(buyers, restoreBuyer);
}

function seedStagingModalBuyersFromData(buyers, restoreBuyer) {
    if (window.stagingOutputBuyerDropdown) {
        window.stagingOutputBuyerDropdown.updateData(buyers);
        if (restoreBuyer && buyers.some(function (item) { return item.value === restoreBuyer; })) {
            window.stagingOutputBuyerDropdown.setValue(restoreBuyer);
            $('#StagingBuyer').val(restoreBuyer);
        }
    }
}

function loadStagingModalTestCodesByWorkGroup(workGroup, restoreTestCode, restoreBuyer) {
    resetStagingModalTestCodeOptions();
    resetStagingModalBuyerOptions();

    $.ajax({
        url: '/PACT/MonthlyOutput/GetTestCodesByWorkGroup',
        type: 'GET',
        data: workGroup ? { workGroup: workGroup } : {},
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            if (window.stagingOutputTestCodeDropdown) {
                window.stagingOutputTestCodeDropdown.updateData(items);

                if (restoreTestCode && items.some(function (item) { return item.value === restoreTestCode; })) {
                    window._stagingSkipTestCodeOnSelect = true;
                    window.stagingOutputTestCodeDropdown.setValue(restoreTestCode);
                    window._stagingSkipTestCodeOnSelect = false;
                    $('#StagingTestCode').val(restoreTestCode);
                    loadStagingModalBuyersByTestCode(workGroup, restoreTestCode, restoreBuyer);
                } else {
                    loadStagingModalBuyersByTestCode(workGroup, null);
                }
            }
        },
        error: function () {
            showAlertMessage('Failed to load test code options.', AlertType.ERROR);
        }
    });
}

function loadStagingModalBuyersByTestCode(workGroup, testCode, restoreBuyer) {
    resetStagingModalBuyerOptions();

    const params = {};
    if (workGroup) params.workGroup = workGroup;
    if (testCode)  params.testCode  = testCode;

    $.ajax({
        url: '/PACT/MonthlyOutput/GetBuyersByTestCode',
        type: 'GET',
        data: params,
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            if (window.stagingOutputBuyerDropdown) {
                window.stagingOutputBuyerDropdown.updateData(items);

                if (restoreBuyer && items.some(function (item) { return item.value === restoreBuyer; })) {
                    window.stagingOutputBuyerDropdown.setValue(restoreBuyer);
                    $('#StagingBuyer').val(restoreBuyer);
                }
            }
        },
        error: function () {
            showAlertMessage('Failed to load buyer options.', AlertType.ERROR);
        }
    });
}

function saveStagingMonthlyOutput() {
    var form = $('#stagingMonthlyOutputForm');

    // Validate all numeric fields before checking isFormValid
    form.find('.decfmt-input').each(function() {
        validateRangeOnInput(this);
    });

    // Check for numeric validation errors
    if (hasNumericValidationErrors(form)) {
        // Ensure validation messages are visible
        if (typeof ensureValidationMessagesVisible === 'function') {
            ensureValidationMessagesVisible(form);
        }
        displayClientValidationErrors(form, form);
        return;
    }

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, form);
        return;
    }

    // Clear validation errors only after validation passes
    clearValidationErrors(form);

    const id = parseInt($('#Id').val()) || 0;
    submitStagingRecord(id);
}

function submitStagingRecord(id) {
    const data = {
        Id:        id,
        WorkGroup: $('#StagingWorkGroup').val(),
        TestCode:  $('#StagingTestCode').val(),
        Buyer:     $('#StagingBuyer').val(),
        Month:     $('#StagingMonth').val(),
        Volume:    $('#StagingVolume').val()
    };

    $.ajax({
        url: '/PACT/MonthlyOutput/SaveStagingRecord',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                reloadStagingGrid();
                showAlertMessage(response.message, AlertType.SUCCESS);
            } else if (response.errors) {
                displayServerValidationErrors(response.errors, response.message || 'Validation failed.', $('#stagingMonthlyOutputForm'));
            } else {
                showAlertMessage(response.message || 'Save failed.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred while saving.', AlertType.ERROR);
        }
    });
}

function deleteStagingMonthlyOutput(btn) {
    const id = $(btn).data('id');

    showGovukConfirm('Are you sure you want to delete this staging record?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/MonthlyOutput/DeleteStagingRecord',
            type: 'DELETE',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    reloadStagingGrid();
                    showAlertMessage('Staging record deleted.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Failed to delete staging record.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting the record.', AlertType.ERROR);
            }
        });
    });
}

// ── Import (direct file browse — PACT flat file .xls, ImportOption 1) ─────────

function triggerMonthlyOutputImportSelection(importType) {
    window.monthlyOutputImportType = importType;
    openImportFilePicker();
}

function openImportExportedFilePicker() {
    triggerMonthlyOutputImportSelection('4');
}

function openImportFilePicker() {
    $('#csvInput').val('');
    $('#csvInput').trigger('click');
}

function importMonthlyOutput(file) {
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);
    formData.append('importType', window.monthlyOutputImportType || '1');

    showLoader();

    $.ajax({
        url: '/PACT/MonthlyOutput/Import',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                window.monthlyOutputPassedFilter = null;
                reloadStagingGrid();
                const msg = response.message ||
                    ('Imported: ' + response.importedCount +
                     ' | Passed: ' + response.passedCount +
                     ' | Failed: ' + response.failedCount);
                showAlertMessage(msg, AlertType.SUCCESS);
            } else {
                showAlertMessage(response.message || 'Import failed.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred during import.', AlertType.ERROR);
        },
        complete: function () {
            hideLoader();
        }
    });
}

// ── Validate ──────────────────────────────────────────────────────────────────

function validateMonthlyOutput() {
    showLoader();

    $.ajax({
        url: '/PACT/MonthlyOutput/Validate',
        type: 'POST',
        success: function (response) {
            if (response.success) {
                reloadStagingGrid();
                const msg = response.message ||
                    ('Passed: ' + response.passedCount + ' | Failed: ' + response.failedCount);
                showAlertMessage(msg, AlertType.SUCCESS);
                document.getElementById('failedmsg').style.display = 'none';
            } else {
                showAlertMessage(response.message || 'Validation failed.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred during validation.', AlertType.ERROR);
        },
        complete: function () {
            hideLoader();
        }
    });
}

// ── Make Live ─────────────────────────────────────────────────────────────────

function makeLiveMonthlyOutput() {
    showGovukConfirm('Are you sure you want to make all passed records live?').then(function (confirmed) {
        if (!confirmed) return;

        showLoader();

        $.ajax({
            url: '/PACT/MonthlyOutput/MakeLive',
            type: 'POST',
            success: function (response) {
                if (response.success) {
                    reloadLiveGrid();
                    reloadStagingGrid();
                    document.getElementById('failedmsg').style.display = 'none';
                    const msg = response.message ||
                        ('Processed: ' + response.processedCount +
                         ' | Imported: ' + response.importedCount +
                         ' | Failed: ' + response.failedCount);
                    showAlertMessage(msg, AlertType.SUCCESS);
                } else {
                    showAlertMessage(response.message || 'Make Live failed.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred during Make Live.', AlertType.ERROR);
            },
            complete: function () {
                hideLoader();
            }
        });
    });
}

// ── Delete All / Delete Failed ────────────────────────────────────────────────

function deleteAllStagingRecords() {
    showGovukConfirm('Are you sure you want to delete all staging records?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/MonthlyOutput/DeleteAllStagingRecords',
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    reloadStagingGrid();
                    document.getElementById('failedmsg').style.display = 'none';
                    showAlertMessage('All staging records deleted.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Failed to delete all staging records.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting all staging records.', AlertType.ERROR);
            }
        });
    });
}

function deleteFailedStagingRecords() {
    showGovukConfirm('Are you sure you want to delete all failed staging records?').then(function (confirmed) {
        if (!confirmed) return;

        $.ajax({
            url: '/PACT/MonthlyOutput/DeleteFailedStagingRecords',
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    reloadStagingGrid();
                    document.getElementById('failedmsg').style.display = 'none';
                    showAlertMessage('Failed staging records deleted.', AlertType.SUCCESS);
                } else {
                    showAlertMessage(response.message || 'Failed to delete failed staging records.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting failed staging records.', AlertType.ERROR);
            }
        });
    });
}

// ── Passed / Failed / All filter buttons ──────────────────────────────────────

function filterStagingPassed() {
    window.monthlyOutputPassedFilter = true;
    reloadStagingGrid();
}

function filterStagingFailed() {
    window.monthlyOutputPassedFilter = false;
    reloadStagingGrid();
}

function filterStagingAll() {
    window.monthlyOutputPassedFilter = null;
    reloadStagingGrid();
}

// ── Page init ─────────────────────────────────────────────────────────────────

$(document).ready(function () {
    initWorkGroupDropdown();
    initTestCodeDropdown();
    initBuyerDropdown();
    scheduleAlignTotalVolumeFields();
    $(window).on('resize', scheduleAlignTotalVolumeFields);

    document.addEventListener('gridReloaded', function (e) {
        if (e?.detail?.gridId === monthlyOutputLiveGridId || e?.detail?.gridId === monthlyOutputStagingGridId) {
            scheduleAlignTotalVolumeFields();
        }
    });

    // Search
    $('#btnSearchLive').on('click', function () {
        reloadLiveGrid();
    });

    // Clear search
    $('#btnClearLiveSearch').on('click', function () {
        clearLiveSearch();
        reloadLiveGrid();
    });

    // Import — direct file browse, PACT flat file default
    $('#importBtn').on('click', function () {
        triggerMonthlyOutputImportSelection('1');
    });

    $('#csvInput').on('change', function () {
        const file = this.files[0];
        if (file) importMonthlyOutput(file);
        this.value = '';
    });

    // Validate
    $('#validateBtn').on('click', function () {
        validateMonthlyOutput();
    });

    // Passed / Failed / All staging filter
    $('#passedBtn').on('click', function () {
        filterStagingPassed();
    });

    $('#failedBtn').on('click', function () {
        filterStagingFailed();
    });

    $('#allBtn').on('click', function () {
        filterStagingAll();
    });

    // Make Live
    $('#moveBtn').on('click', function () {
        makeLiveMonthlyOutput();
    });

    // Delete All
    $('#deleteAllWGBtn').on('click', function () {
        deleteAllStagingRecords();
    });

    // Delete Failed
    $('#deleteFailedWGBtn').on('click', function () {
        deleteFailedStagingRecords();
    });

    // Export Excel
    $('#exportExcel').on('click', exportMonthlyOutput);
});

// ── Export ────────────────────────────────────────────────────────────────────

function exportMonthlyOutput() {
    const passed = window.monthlyOutputPassedFilter;
    const url = (passed === null || passed === undefined)
        ? '/PACT/MonthlyOutput/ExportStaging'
        : '/PACT/MonthlyOutput/ExportStaging?passed=' + passed;
    window.location = url;
}
