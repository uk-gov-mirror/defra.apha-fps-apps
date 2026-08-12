function getLiveGridManager() {
    return window['gridManager_' + monthlyTimeLiveGridId];
}

function getStagingGridManager() {
    return window['gridManager_' + monthlyTimeStagingGridId];
}

function getSelectedWorkGroup() {
    return $('#ddWorkGroup').val() || null;
}

function getMonthlyTimeLiveFilters() {
    return {
        workGroup: getSelectedWorkGroup(),
        timeCode: $('#ddTimeCode').val() || null,
        pactStaffId: $('#ddStaff').val() || null,
        parentProject: $('#ddParentProject').val() || null,
        month: $('#ddMonth').val() || null
    };
}

function getMonthlyTimeStagingFilters() {
    return {
        passed: window.monthlyTimePassedFilter ?? null
    };
}

function reloadLiveGrid() {
    const gm = getLiveGridManager();
    if (gm) {
        gm.reloadGrid({ page: 1, sortBy: 'WorkGroup', descending: false });
        scheduleAlignTotalHoursFields();
    }
}

function reloadStagingGrid() {
    const gm = getStagingGridManager();
    if (gm) {
        gm.reloadGrid({ page: 1, sortBy: 'Id', descending: false });
        scheduleAlignTotalHoursFields();
    }
}

function clearLiveSearch() {
    $('#ddWorkGroup').val('');
    $('#ddStaff').val('');
    $('#ddMonth').val('');

    if (window.monthlyTimeWorkGroupDropdown) {
        window.monthlyTimeWorkGroupDropdown.clear();
    }

    if (window.monthlyTimeStaffDropdown) {
        window.monthlyTimeStaffDropdown.clear();
    }

    resetTimeCodeOptions();
    resetParentProjectOptions();
}

function alignTotalHoursBox(gridId, rowContainerId, inputId, labelSelector) {
    const hoursTh = document.querySelector('#tbl_' + gridId + ' [data-column="Hours"]');
    const gridContainer = document.getElementById('gridContainer_' + gridId);
    const rowContainer = document.getElementById(rowContainerId);
    const input = document.getElementById(inputId);
    const label = document.querySelector(labelSelector);

    if (!hoursTh || !gridContainer || !rowContainer || !input || !label) return;

    const thRect = hoursTh.getBoundingClientRect();
    const containerLeft = gridContainer.getBoundingClientRect().left;
    const rightOffset = thRect.right - containerLeft;

    rowContainer.style.display = 'flex';
    rowContainer.style.alignItems = 'center';

    label.style.whiteSpace = 'nowrap';
    label.style.marginLeft = Math.max(0, rightOffset - label.offsetWidth - 8 - input.offsetWidth) + 'px';
    label.style.marginRight = '8px';

    input.style.flexShrink = '0';
}

function alignTotalHoursFields() {
    alignTotalHoursBox(monthlyTimeLiveGridId, 'divMakeliveTotalhours', 'txtMakeliveTotalhours', '#divMakeliveTotalhours .total-hours-label');
    alignTotalHoursBox(monthlyTimeStagingGridId, 'divStagingTotalhours', 'txtTotalhours', '#lbltotalhours');
}

function updateTotalHoursValues() {
    const liveTotal = parseFloat($('#gridContainer_' + monthlyTimeLiveGridId + ' .editable-grid-container').data('grid-total'));
    const stagingTotal = parseFloat($('#gridContainer_' + monthlyTimeStagingGridId + ' .editable-grid-container').data('grid-total'));

    $('#txtMakeliveTotalhours').val(Number.isFinite(liveTotal) ? liveTotal.toFixed(2) : '0.00');
    $('#txtTotalhours').val(Number.isFinite(stagingTotal) ? stagingTotal.toFixed(2) : '0.00');
}

function scheduleAlignTotalHoursFields() {
    window.requestAnimationFrame(function () {
        alignTotalHoursFields();
        updateTotalHoursValues();
        setTimeout(function () {
            alignTotalHoursFields();
            updateTotalHoursValues();
        }, 120);
        setTimeout(function () {
            alignTotalHoursFields();
            updateTotalHoursValues();
        }, 350);
    });
}

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

function resetTimeCodeOptions() {
    $('#ddTimeCode').val('');
    if (window.monthlyTimeTimeCodeDropdown) {
        window.monthlyTimeTimeCodeDropdown.clear();
        window.monthlyTimeTimeCodeDropdown.updateData([]);
    }
}

function resetParentProjectOptions() {
    $('#ddParentProject').val('');
    if (window.monthlyTimeParentProjectDropdown) {
        window.monthlyTimeParentProjectDropdown.clear();
        window.monthlyTimeParentProjectDropdown.updateData([]);
    }
}

function initWorkGroupDropdown() {
    const workGroups = readDropdownJsonData('#monthly-time-workgroups-data');

    window.monthlyTimeWorkGroupDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'monthlyTimeWorkGroup',
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
                loadStaffByWorkGroup(workGroup);
                loadTimeCodesByWorkGroup(workGroup);
            },
            onClear: function () {
                $('#ddWorkGroup').val('');
                if (window.monthlyTimeStaffDropdown) {
                    window.monthlyTimeStaffDropdown.clear();
                    window.monthlyTimeStaffDropdown.updateData([]);
                }
                $('#ddStaff').val('');
                resetTimeCodeOptions();
                resetParentProjectOptions();
            }
        }
    });
}

function initLiveFilterDropdowns() {
    const initialTimeCodes = readDropdownJsonData('#monthly-time-timecodes-data');
    const initialProjects = readDropdownJsonData('#monthly-time-projects-data');

    window.monthlyTimeTimeCodeDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'monthlyTimeTimeCode',
        containerSelector: '#timeCodeSelectDropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        columns: [
            { field: 'text', header: 'Timecode', width: '220px' }
        ],
        data: initialTimeCodes,
        displayField: function (row) { return row.text || ''; },
        valueField: function (row) { return row.value || ''; },
        enableSearch: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {
                const workGroup = getSelectedWorkGroup();
                const timeCode = selectedItem?.value || '';
                $('#ddTimeCode').val(timeCode);
                loadProjectsByWorkGroupAndTimeCode(workGroup, timeCode);
            },
            onClear: function () {
                $('#ddTimeCode').val('');
                resetParentProjectOptions();
            }
        }
    });

    window.monthlyTimeParentProjectDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'monthlyTimeParentProject',
        containerSelector: '#parentProjectSelectDropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        columns: [
            { field: 'text', header: 'Parent Project', width: '240px' }
        ],
        data: initialProjects,
        displayField: function (row) { return row.text || ''; },
        valueField: function (row) { return row.value || ''; },
        enableSearch: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {
                $('#ddParentProject').val(selectedItem?.value || '');
            },
            onClear: function () {
                $('#ddParentProject').val('');
            }
        }
    });
}

function initStaffDropdown() {
    window.monthlyTimeStaffDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'monthlyTimeStaff',
        containerSelector: '#staffSelectDropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: 'Staff',
        columns: [
            { field: 'name', header: 'Name', width: '180px' },
            { field: 'spNumber', header: 'SPNumber', width: '180px' },
            { field: 'pactId', header: 'PACTid', width: '90px' },
            { field: 'workGroupGrade', header: 'WG_Grade', width: '100px' }
        ],
        data: [],
        displayField: function (row) { return row.name || ''; },
        valueField: function (row) { return row.pactId || ''; },
        enableSearch: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {
                $('#ddStaff').val(selectedItem?.pactId || '');
            },
            onClear: function () {
                $('#ddStaff').val('');
            }
        }
    });
}

function loadStaffByWorkGroup(workGroup) {
    if (!window.monthlyTimeStaffDropdown) return;

    window.monthlyTimeStaffDropdown.clear();

    if (!workGroup) {
        window.monthlyTimeStaffDropdown.updateData([]);
        return;
    }

    $.ajax({
        url: '/PACT/MonthlyTime/GetStaffByWorkGroup',
        type: 'GET',
        data: { workGroup: workGroup },
        success: function (data) {
            window.monthlyTimeStaffDropdown.updateData(Array.isArray(data) ? data : []);
        },
        error: function () {
            window.monthlyTimeStaffDropdown.updateData([]);
            showAlertMessage('Failed to load staff options.', AlertType.ERROR);
        }
    });
}

function loadTimeCodesByWorkGroup(workGroup) {
    resetTimeCodeOptions();
    resetParentProjectOptions();

    if (!workGroup) {
        return;
    }

    $.ajax({
        url: '/PACT/MonthlyTime/GetTimeCodesByWorkGroup',
        type: 'GET',
        data: { workGroup: workGroup },
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            if (window.monthlyTimeTimeCodeDropdown) {
                window.monthlyTimeTimeCodeDropdown.updateData(items);
            }
        },
        error: function () {
            showAlertMessage('Failed to load timecode options.', AlertType.ERROR);
        }
    });
}

function loadProjectsByWorkGroupAndTimeCode(workGroup, timeCode, restoreParentProject) {
    resetParentProjectOptions();

    if (!workGroup || !timeCode) {
        return;
    }

    $.ajax({
        url: '/PACT/MonthlyTime/GetProjectsByWorkGroupAndTimeCode',
        type: 'GET',
        data: { workGroup: workGroup, timeCode: timeCode },
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            if (window.monthlyTimeParentProjectDropdown) {
                window.monthlyTimeParentProjectDropdown.updateData(items);
                if (restoreParentProject && items.some(function (item) { return item.value === restoreParentProject; })) {
                    window.monthlyTimeParentProjectDropdown.setValue(restoreParentProject);
                    $('#ddParentProject').val(restoreParentProject);
                }
            }
        },
        error: function () {
            showAlertMessage('Failed to load parent project options.', AlertType.ERROR);
        }
    });
}

function parseCompositeKey(key) {
    const parts = (key || '').split('|');
    return {
        pactStaffId: parts[0] || '',
        timeCode: parts[1] || '',
        month: parts[2] || '',
        parentProject: parts[3] || ''
    };
}

function editMonthlyTimeLive(btn) {
    const key = $(btn).data('id');
    const parsed = parseCompositeKey(key);
    $.ajax({
        url: '/PACT/MonthlyTime/GetLiveRecord',
        type: 'GET',
        data: parsed,
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            const workGroup = $('#LiveWorkGroup').val();
            const existingName = $('#LiveName').val();
            const existingPactId = $('#LivePactStaffId').val();
            initLiveModalDropdowns(workGroup, existingName, existingPactId);
            // Initialize form validation (unobtrusive + numeric)
            initializeFormValidation('#monthlyTimeLiveForm');
        },
        error: function () {
            showAlertMessage('Failed to load monthly time record.', AlertType.ERROR);
        }
    });
}

function initLiveModalDropdowns(existingWorkGroup, existingName, existingPactId) {
    window.liveWorkGroupDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'liveWorkGroup',
        containerSelector: '#live-modal-workgroup-dropdown-container',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: 'Work Group <span class="app-required" aria-hidden="true">*</span>',
        columns: [
            { field: 'text', header: 'Work Group', width: '200px' }
        ],
        data: readDropdownJsonData('#monthly-time-workgroups-data'),
        displayField: function (row) { return row.text || ''; },
        valueField: function (row) { return row.value || ''; },
        enableSearch: true,
        disabled: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {
                const selectedWorkGroup = selectedItem?.value || '';
                $('#LiveWorkGroup').val(selectedWorkGroup);

                // Clear validation error when a valid selection is made
                const workGroupInput = $('#LiveWorkGroup');
                const formGroup = workGroupInput.closest('.govuk-form-group');
                const errorMsg = formGroup.find('[data-valmsg-for="WorkGroup"]');

                formGroup.removeClass('govuk-form-group--error');
                workGroupInput.removeClass('govuk-input--error');
                errorMsg.hide().text('');

                loadLiveModalStaffByWorkGroup(selectedWorkGroup);
            },
            onClear: function () {
                $('#LiveWorkGroup').val('');
                if (window.liveNameDropdown) window.liveNameDropdown.updateData([]);
                $('#LivePactStaffId').val('');
                $('#LiveName').val('');
            }
        }
    });

    window.liveNameDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'liveName',
        containerSelector: '#live-modal-name-dropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: '',
        columns: [
            { field: 'name', header: 'Name', width: '180px' },
            { field: 'spNumber', header: 'SPNumber', width: '180px' },
            { field: 'pactId', header: 'PACTid', width: '90px' },
            { field: 'workGroupGrade', header: 'WG_Grade', width: '100px' }
        ],
        data: [],
        displayField: function (row) { return row.name || ''; },
        valueField: function (row) { return row.pactId || ''; },
        enableSearch: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {
                $('#LiveName').val(selectedItem?.name || '');
                $('#LivePactStaffId').val(selectedItem?.pactId || '');

                // Clear validation error when a valid selection is made
                const nameInput = $('#LiveName');
                const formGroup = nameInput.closest('.govuk-form-group');
                const errorMsg = formGroup.find('[data-valmsg-for="Name"]');

                formGroup.removeClass('govuk-form-group--error');
                nameInput.removeClass('govuk-input--error');
                errorMsg.hide().text('');
            },
            onClear: function () {
                $('#LiveName').val('');
                $('#LivePactStaffId').val('');
            }
        }
    });

    if (existingWorkGroup) {
        window.liveWorkGroupDropdown.setValue(existingWorkGroup);
        loadLiveModalStaffByWorkGroup(existingWorkGroup, existingName, existingPactId);
    }
}

function loadLiveModalStaffByWorkGroup(workGroup, restoreName, restorePactId) {
    if (!window.liveNameDropdown) return;

    window.liveNameDropdown.clear();
    $('#LiveName').val('');
    $('#LivePactStaffId').val('');

    $.ajax({
        url: '/PACT/MonthlyTime/GetStaffByWorkGroup',
        type: 'GET',
        data: { workGroup: workGroup },
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            window.liveNameDropdown.updateData(items);

            if (restoreName || restorePactId) {
                const match = items.find(function (x) {
                    return (restorePactId && x.pactId === restorePactId) || (restoreName && x.name === restoreName);
                });
                if (match) {
                    window.liveNameDropdown.setValue(match.pactId);
                    // Set hidden inputs explicitly in case setValue does not fire onSelect
                    $('#LiveName').val(match.name);
                    $('#LivePactStaffId').val(match.pactId);
                } else {
                    // Staff not in list (e.g. inactive) — show stored values directly
                    $('#LiveName').val(restoreName || '');
                    $('#LivePactStaffId').val(restorePactId || '');
                }
            }
        },
        error: function () {
            window.liveNameDropdown.updateData([]);
            showAlertMessage('Failed to load staff options.', AlertType.ERROR);
        }
    });
}

function saveMonthlyTimeLive() {
    const form = $('#monthlyTimeLiveForm');

    if (!isFormValid(form)) {
        displayClientValidationErrors(form, form);
        return;
    }

    // Clear validation errors only after validation passes
    clearValidationErrors(form);

    const data = {
        CompositeKey: $('#CompositeKey').val(),
        WorkGroup: $('#LiveWorkGroup').val(),
        PactStaffId: $('#LivePactStaffId').val(),
        Name: $('#LiveName').val(),
        TimeCode: $('#TimeCode').val(),
        ParentProject: $('#ParentProject').val(),
        Month: $('#LiveMonth').val(),
        Hours: $('#LiveHours').val()
    };

    $.ajax({
        url: '/PACT/MonthlyTime/SaveLiveRecord',
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

function addStagingMonthlyTime() {
    $.ajax({
        url: '/PACT/MonthlyTime/AddStagingRecord',
        type: 'GET',
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            initStagingModalDropdowns(null);
            // Initialize form validation (unobtrusive + numeric)
            initializeFormValidation('#monthlyTimeLiveForm');
        },
        error: function () {
            showAlertMessage('Failed to load add form.', AlertType.ERROR);
        }
    });
}

function editStagingMonthlyTime(btn) {
    const id = $(btn).data('id');
    $.ajax({
        url: '/PACT/MonthlyTime/GetStagingRecord',
        type: 'GET',
        data: { id: id },
        success: function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
            const workGroup = $('#StagingWorkGroup').val();
            const existingName = $('#StagingName').val();
            const existingPactId = $('#StagingPactStaffId').val();
            const existingTimeCode = $('#StagingTimeCode').val();
            const existingParentProject = $('#StagingParentProject').val();
            initStagingModalDropdowns(workGroup, existingName, existingPactId, existingTimeCode, existingParentProject);
            // Attach numeric validation to decimal fields
            if (typeof attachNumericValidation === 'function') {
                attachNumericValidation();
            }
        },
        error: function () {
            showAlertMessage('Failed to load staging record.', AlertType.ERROR);
        }
    });
}

function initStagingModalDropdowns(existingWorkGroup, existingName, existingPactId, existingTimeCode, existingParentProject) {
    const wgData = readDropdownJsonData('#staging-modal-workgroups-data');
    const initialTimeCodes = readDropdownJsonData('#staging-modal-timecodes-data');
    const initialProjects = readDropdownJsonData('#staging-modal-projects-data');

    window.stagingWorkGroupDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'stagingWorkGroup',
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
                $('#StagingPactId').val('');

                // Clear validation error when a valid selection is made
                const workGroupInput = $('#StagingWorkGroup');
                const formGroup = workGroupInput.closest('.govuk-form-group');
                const errorMsg = formGroup.find('[data-valmsg-for="WorkGroup"]');

                formGroup.removeClass('govuk-form-group--error');
                workGroupInput.removeClass('govuk-input--error');
                errorMsg.hide().text('');

                const isInitialRestore = existingWorkGroup && selectedWorkGroup === existingWorkGroup;
                if (isInitialRestore) {
                    return;
                }

                loadStagingModalStaffByWorkGroup(selectedWorkGroup);
                loadStagingModalTimeCodesByWorkGroup(selectedWorkGroup);
            },
            onClear: function () {
                $('#StagingWorkGroup').val('');
                $('#StagingPactId').val('');
                loadStagingModalStaffByWorkGroup('');
                loadAllStagingModalTimeCodes();
                loadAllStagingModalProjects();
            }
        }
    });

    window.stagingNameDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'stagingName',
        containerSelector: '#staging-modal-name-dropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: '',
        columns: [
            { field: 'name', header: 'Name', width: '180px' },
            { field: 'spNumber', header: 'SPNumber', width: '90px' },
            { field: 'pactId', header: 'PACTid', width: '90px' },
            { field: 'workGroupGrade', header: 'WG_Grade', width: '100px' }
        ],
        data: [],
        displayField: function (row) { return row.name || ''; },
        valueField: function (row) { return row.pactId || ''; },
        enableSearch: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {                
                const selectedPactId = selectedItem?.pactId || '';
                const selectedSpNumber = selectedItem?.spNumber || '';
                $('#StagingName').val(selectedItem?.name || '');
                $('#StagingPactStaffId').val(selectedSpNumber);
                $('#StagingPactId').val(selectedPactId);

                // Clear validation error when a valid selection is made
                const nameInput = $('#StagingName');
                const formGroup = nameInput.closest('.govuk-form-group');
                const errorMsg = formGroup.find('[data-valmsg-for="Name"]');

                formGroup.removeClass('govuk-form-group--error');
                nameInput.removeClass('govuk-input--error');
                errorMsg.hide().text('');
            },
            onClear: function () {
                $('#StagingName').val('');
                $('#StagingPactStaffId').val('');
                $('#StagingPactId').val('');
            }
        }
    });

    window.stagingTimeCodeDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'stagingTimeCode',
        containerSelector: '#staging-modal-timecode-dropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: '',
        columns: [
            { field: 'text', header: 'Time Code', width: '260px' }
        ],
        data: initialTimeCodes,
        displayField: function (row) { return row.text || ''; },
        valueField: function (row) { return row.value || ''; },
        enableSearch: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {
                if (window._stagingSkipTimeCodeOnSelect) return;
                const timeCode = selectedItem?.value || '';
                const workGroup = $('#StagingWorkGroup').val();
                $('#StagingTimeCode').val(timeCode);

                // Clear validation error when a valid selection is made
                const timeCodeInput = $('#StagingTimeCode');
                const formGroup = timeCodeInput.closest('.govuk-form-group');
                const errorMsg = formGroup.find('[data-valmsg-for="TimeCode"]');

                formGroup.removeClass('govuk-form-group--error');
                timeCodeInput.removeClass('govuk-input--error');
                errorMsg.hide().text('');

                if (workGroup) {
                    loadStagingModalParentProjectsByWorkGroupAndTimeCode(workGroup, timeCode);
                }
            },
            onClear: function () {
                const workGroup = $('#StagingWorkGroup').val();
                $('#StagingTimeCode').val('');

                if (workGroup) {
                    resetStagingModalParentProjectOptions();
                } else {
                    loadAllStagingModalProjects();
                }
            }
        }
    });

    window.stagingParentProjectDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'stagingParentProject',
        containerSelector: '#staging-modal-parentproject-dropdown',
        placeholder: '--select--',
        searchPlaceholder: 'Type to search',
        labelText: '',
        columns: [
            { field: 'text', header: 'Parent Project', width: '260px' }
        ],
        data: initialProjects,
        displayField: function (row) { return row.text || ''; },
        valueField: function (row) { return row.value || ''; },
        enableSearch: true,
        showSerialNumber: false,
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem) {
                $('#StagingParentProject').val(selectedItem?.value || '');

                // Clear validation error when a valid selection is made
                const parentProjectInput = $('#StagingParentProject');
                const formGroup = parentProjectInput.closest('.govuk-form-group');
                const errorMsg = formGroup.find('[data-valmsg-for="ParentProject"]');

                formGroup.removeClass('govuk-form-group--error');
                parentProjectInput.removeClass('govuk-input--error');
                errorMsg.hide().text('');
            },
            onClear: function () {
                $('#StagingParentProject').val('');
            }
        }
    });

    if (existingWorkGroup) {
        window.stagingWorkGroupDropdown.setValue(existingWorkGroup);
        loadStagingModalStaffByWorkGroup(existingWorkGroup, existingName, existingPactId);
        loadAllStagingModalTimeCodes(existingTimeCode, existingParentProject);
    } else {
        loadStagingModalStaffByWorkGroup($('#StagingWorkGroup').val());
        loadAllStagingModalTimeCodes();
        loadAllStagingModalProjects();
    }
}

function loadStagingModalStaffByWorkGroup(workGroup, restoreName, restorePactId) {
    if (!window.stagingNameDropdown) return;

    window.stagingNameDropdown.clear();
    $('#StagingName').val('');
    $('#StagingPactStaffId').val('');
    $('#StagingPactId').val('');

    if (!workGroup) {
        window.stagingNameDropdown.updateData([]);
        return;
    }

    $.ajax({
        url: '/PACT/MonthlyTime/GetStaffByWorkGroup',
        type: 'GET',
        data: { workGroup: workGroup },
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            window.stagingNameDropdown.updateData(items);

            if (restoreName || restorePactId) {
                const match = items.find(function (x) {
                    return (restorePactId && x.pactId === restorePactId) || (restoreName && x.name === restoreName);
                });
                if (match) {
                    window.stagingNameDropdown.setValue(match.pactId);
                    // Set hidden inputs explicitly in case setValue does not fire onSelect
                    $('#StagingName').val(match.name);
                    $('#StagingPactStaffId').val(match.spNumber);
                    $('#StagingPactId').val(match.pactId);
                } else {
                    // Employee not in list (e.g. inactive) — show stored text directly
                    $('#StagingName').val(restoreName || '');
                    $('#StagingPactStaffId').val(restorePactId || '');
                    $('#StagingPactId').val(restorePactId || '');
                }
            }
        },
        error: function () {
            window.stagingNameDropdown.updateData([]);
            showAlertMessage('Failed to load staff options.', AlertType.ERROR);
        }
    });
}

function loadStagingModalTimeCodesByWorkGroup(workGroup, restoreTimeCode, loadProjectsOnRestore = true) {
    resetStagingModalTimeCodeOptions();
    resetStagingModalParentProjectOptions();

    if (!workGroup) return;

    $.ajax({
        url: '/PACT/MonthlyTime/GetTimeCodesByWorkGroup',
        type: 'GET',
        data: { workGroup: workGroup },
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            if (window.stagingTimeCodeDropdown) {
                window.stagingTimeCodeDropdown.updateData(items);

                if (restoreTimeCode && items.some(function (item) { return item.value === restoreTimeCode; })) {
                    window.stagingTimeCodeDropdown.setValue(restoreTimeCode);
                    $('#StagingTimeCode').val(restoreTimeCode);
                    if (loadProjectsOnRestore) {
                        loadStagingModalParentProjectsByWorkGroupAndTimeCode(workGroup, restoreTimeCode);
                    }
                }
            }
        },
        error: function () {
            showAlertMessage('Failed to load timecode options.', AlertType.ERROR);
        }
    });
}

function loadStagingModalParentProjectsByWorkGroupAndTimeCode(workGroup, timeCode, restoreParentProject) {
    resetStagingModalParentProjectOptions();

    if (!workGroup || !timeCode) return;

    $.ajax({
        url: '/PACT/MonthlyTime/GetProjectsByWorkGroupAndTimeCode',
        type: 'GET',
        data: { workGroup: workGroup, timeCode: timeCode },
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            if (window.stagingParentProjectDropdown) {
                window.stagingParentProjectDropdown.updateData(items);

                if (restoreParentProject && items.some(function (item) { return item.value === restoreParentProject; })) {
                    window.stagingParentProjectDropdown.setValue(restoreParentProject);
                    $('#StagingParentProject').val(restoreParentProject);
                }
            }
        },
        error: function () {
            showAlertMessage('Failed to load parent project options.', AlertType.ERROR);
        }
    });
}

function resetStagingModalTimeCodeOptions() {
    $('#StagingTimeCode').val('');
    if (window.stagingTimeCodeDropdown) {
        window.stagingTimeCodeDropdown.clear();
        window.stagingTimeCodeDropdown.updateData([]);
    }
}

function resetStagingModalParentProjectOptions() {
    $('#StagingParentProject').val('');
    if (window.stagingParentProjectDropdown) {
        window.stagingParentProjectDropdown.clear();
        window.stagingParentProjectDropdown.updateData([]);
    }
}

function loadAllStagingModalStaff() {
    if (!window.stagingNameDropdown) return;
    $.ajax({
        url: '/PACT/MonthlyTime/GetStaffByWorkGroup',
        type: 'GET',
        success: function (data) {
            window.stagingNameDropdown.updateData(Array.isArray(data) ? data : []);
        },
        error: function () {
            showAlertMessage('Failed to load staff options.', AlertType.ERROR);
        }
    });
}

function loadAllStagingModalTimeCodes(restoreTimeCode, restoreParentProject) {
    resetStagingModalTimeCodeOptions();
    $.ajax({
        url: '/PACT/MonthlyTime/GetAllTimeCodes',
        type: 'GET',
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            if (window.stagingTimeCodeDropdown) {
                window.stagingTimeCodeDropdown.updateData(items);

                if (restoreTimeCode && items.some(function (item) { return item.value === restoreTimeCode; })) {
                    window._stagingSkipTimeCodeOnSelect = true;
                    window.stagingTimeCodeDropdown.setValue(restoreTimeCode);
                    window._stagingSkipTimeCodeOnSelect = false;
                    $('#StagingTimeCode').val(restoreTimeCode);

                    // Chain project restore after timecode is set, avoiding the race
                    // with a concurrent loadAllStagingModalProjects call
                    const workGroup = $('#StagingWorkGroup').val();
                    if (workGroup) {
                        loadStagingModalParentProjectsByWorkGroupAndTimeCode(workGroup, restoreTimeCode, restoreParentProject);
                    } else if (restoreParentProject !== undefined) {
                        loadAllStagingModalProjects(restoreParentProject);
                    }
                }
            }
        },
        error: function () {
            showAlertMessage('Failed to load timecode options.', AlertType.ERROR);
        }
    });
}

function loadAllStagingModalProjects(restoreParentProject) {
    resetStagingModalParentProjectOptions();
    $.ajax({
        url: '/PACT/MonthlyTime/GetAllProjects',
        type: 'GET',
        success: function (data) {
            const items = Array.isArray(data) ? data : [];
            if (window.stagingParentProjectDropdown) {
                window.stagingParentProjectDropdown.updateData(items);

                if (restoreParentProject && items.some(function (item) { return item.value === restoreParentProject; })) {
                    window.stagingParentProjectDropdown.setValue(restoreParentProject);
                    $('#StagingParentProject').val(restoreParentProject);
                }
            }
        },
        error: function () {
            showAlertMessage('Failed to load parent project options.', AlertType.ERROR);
        }
    });
}

function submitStagingMonthlyTime(data) {
    $.ajax({
        url: '/PACT/MonthlyTime/SaveStagingRecord',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                reloadStagingGrid();
                showAlertMessage(response.message, AlertType.SUCCESS);
            } else {
                showAlertMessage(response.message || 'Save failed.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred while saving.', AlertType.ERROR);
        }
    });
}

function saveStagingMonthlyTime() {
    const form = $('#stagingMonthlyTimeForm');

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

    const isNameUpdatingChecked = $('#chkNameupdating').is(':checked');

    const data = {
        Id: $('#Id').val(),
        WorkGroup: $('#StagingWorkGroup').val(),
        PactStaffId: $('#StagingPactStaffId').val(),
        PactId: $('#StagingPactId').val(),
        Name: $('#StagingName').val(),
        TimeCode: $('#StagingTimeCode').val(),
        ParentProject: $('#StagingParentProject').val(),
        Month: $('#StagingMonth').val(),
        Hours: $('#StagingHours').val(),
        NameUpdating: isNameUpdatingChecked
    };

    if (isNameUpdatingChecked) {
        const selectedSpNumber = ($('#StagingPactStaffId').val() || '').trim();
        const selectedWorkGroup = ($('#StagingWorkGroup').val() || '').trim();
        const strOriginalName = selectedSpNumber;

        const message = 'Do you want to update all other entries for "' + strOriginalName + '" in "' + selectedWorkGroup + '" ?';

        showGovukConfirm(message).then(function (confirmed) {
            data.NameUpdating = confirmed;
            submitStagingMonthlyTime(data);
        });

        return;
    }

    submitStagingMonthlyTime(data);
}

function deleteStagingMonthlyTime(btn) {
    const id = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this staging record').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/MonthlyTime/DeleteStagingRecord',
            type: 'DELETE',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    reloadStagingGrid();
                    showAlertMessage('Imported record deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Failed to delete imported record.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting.', AlertType.ERROR);
            }
        });
    });
}

function openImportTypeModal() {
    $('input[name="importType"]').prop('checked', false);
    $('#importTypeModal').addClass('show').css('display', 'flex');
}

function closeImportTypeModal() {
    $('#importTypeModal').removeClass('show').hide();
}

function triggerMonthlyTimeImportSelection(importType) {
    window.monthlyTimeImportType = importType;
    $('#csvInput').click();
}

function openImportExportedFilePicker() {
    triggerMonthlyTimeImportSelection('4');
}

function confirmImportType() {
    const selected = $('input[name="importType"]:checked').val();
    if (!selected) {
        showAlertMessage('Please select an import type.', AlertType.INFO);
        return;
    }

    closeImportTypeModal();
    triggerMonthlyTimeImportSelection(selected);
}

function importMonthlyTime(file) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('importType', window.monthlyTimeImportType || '2');
    showLoader();

    $.ajax({
        url: '/PACT/MonthlyTime/Import',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                window.monthlyTimePassedFilter = null;
                reloadStagingGrid();
                showAlertMessage(response.message || 'Import completed.', AlertType.SUCCESS);
            } else {
                showAlertMessage(response.message || 'Import failed.', AlertType.ERROR);
            }
        },
        error: function () {
            showAlertMessage('An error occurred while importing.', AlertType.ERROR);
        },
        complete: function () {
            hideLoader();
        }
    });
}

function validateMonthlyTime() {
    showLoader();
    $.ajax({
        url: '/PACT/MonthlyTime/Validate',
        type: 'POST',
        success: function (response) {
            if (response.success) {
                reloadStagingGrid();
                showAlertMessage(response.message, AlertType.SUCCESS);
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

function makeLiveMonthlyTime() {
    showLoader();
    $.ajax({
        url: '/PACT/MonthlyTime/MakeLive',
        type: 'POST',
        success: function (response) {
            if (response.success) {
                reloadLiveGrid();
                reloadStagingGrid();
                showAlertMessage(response.message, AlertType.SUCCESS);
            } else {
                showAlertMessage(response.message || 'Make live failed.', AlertType.ERROR);
            }
        },
        error: function (xhr) {
            showAlertMessage(xhr.responseJSON?.message || 'An error occurred during make live.', AlertType.ERROR);
        },
        complete: function () {
            hideLoader();
        }
    });
}

function deleteAllMonthlyTime() {
    showGovukConfirm('Delete all imported records for the current user?').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/MonthlyTime/DeleteAllStagingRecords',
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    reloadStagingGrid();
                    showAlertMessage('Imported records deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Delete all failed.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting imported records.', AlertType.ERROR);
            }
        });
    });
}

function deleteFailedMonthlyTime() {
    showGovukConfirm('Delete failed imported records for the current user?').then(function (confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/MonthlyTime/DeleteFailedStagingRecords',
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    reloadStagingGrid();
                    showAlertMessage('Failed imported records deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage(response.message || 'Delete failed records failed.', AlertType.ERROR);
                }
            },
            error: function () {
                showAlertMessage('An error occurred while deleting failed imported records.', AlertType.ERROR);
            }
        });
    });
}

function exportMonthlyTime() {
    const passed = window.monthlyTimePassedFilter;
    const url = passed === null || passed === undefined
        ? '/PACT/MonthlyTime/ExportStaging'
        : '/PACT/MonthlyTime/ExportStaging?passed=' + passed;
    window.location = url;
}

$(function () {
    window.monthlyTimePassedFilter = null;

    initWorkGroupDropdown();
    initStaffDropdown();
    initLiveFilterDropdowns();
    scheduleAlignTotalHoursFields();
    $(window).on('resize', scheduleAlignTotalHoursFields);

    document.addEventListener('gridReloaded', function (e) {
        if (e?.detail?.gridId === monthlyTimeLiveGridId || e?.detail?.gridId === monthlyTimeStagingGridId) {
            scheduleAlignTotalHoursFields();
        }
    });


    $('#btnSearchLive').on('click', reloadLiveGrid);
    $('#btnClearLiveSearch').on('click', function () {
        clearLiveSearch();
        reloadLiveGrid();
    });
    $('#importTypeBtn').on('click', openImportTypeModal);
    $('#csvInput').on('change', function () {
        const file = this.files && this.files[0];
        if (file) {
            importMonthlyTime(file);
        }
        this.value = '';
    });
    $('#validateBtn').on('click', validateMonthlyTime);
    $('#passedBtn').on('click', function () { window.monthlyTimePassedFilter = true; reloadStagingGrid(); });
    $('#failedBtn').on('click', function () { window.monthlyTimePassedFilter = false; reloadStagingGrid(); });
    $('#allBtn').on('click', function () { window.monthlyTimePassedFilter = null; reloadStagingGrid(); });
    $('#moveBtn').on('click', makeLiveMonthlyTime);
    $('#deleteAllWGBtn').on('click', deleteAllMonthlyTime);
    $('#deleteFailedWGBtn').on('click', deleteFailedMonthlyTime);
    $('#exportExcel').on('click', exportMonthlyTime);
});
