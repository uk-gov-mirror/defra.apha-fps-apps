// ── Stored original Time Tab values for Cancel ────────────────────────────────
var _timeOrigWorkingHours = document.getElementById('timeWorkingHours')?.value || '';
var _timeOrigWorkingDays  = document.getElementById('timeWorkingDays')?.value  || '';

// ── Current tab context ────────────────────────────────────────────────────────
var _currentManagerName = '';
var _currentReportId = null;
var _programManagerLinkDropdown = null;
var _profitCentreManagerLinkDropdown = null;

function initializeTimeTabState() {
    var hwEl = document.getElementById('timeWorkingHours');
    var dwEl = document.getElementById('timeWorkingDays');
    if (hwEl) {
        _timeOrigWorkingHours = hwEl.value;
        // Add change listener to clear errors when user starts typing
        $(hwEl).off('input.timevalidation').on('input.timevalidation', function() {
            validateTimeInput(this, 'timeWorkingHoursError', 'timeWorkingHoursErrorText', 'Working Hours in a Day');
        });
    }
    if (dwEl) {
        _timeOrigWorkingDays = dwEl.value;
        // Add change listener to clear errors when user starts typing
        $(dwEl).off('input.timevalidation').on('input.timevalidation', function() {
            validateTimeInput(this, 'timeWorkingDaysError', 'timeWorkingDaysErrorText', 'Working Days in a Year');
        });
    }
}

function initializeOtherTabState() {
    var $selectedItem = $('.other-list-item.selected').first();
    if ($selectedItem.length === 0) {
        $selectedItem = $('.other-list-item[data-listdesc="Frequency"]').first();
    }
    if ($selectedItem.length === 0) {
        $selectedItem = $('.other-list-item').first();
    }

    if ($selectedItem.length > 0) {
        $('.other-list-item').removeClass('selected');
        $selectedItem.addClass('selected');
        loadOtherValuesGrid($selectedItem.data('listdesc'));
    }
}

function loadGridIntoContainer(url, containerSelector, extraData) {
    $(containerSelector).html('<div class="govuk-body">Loading...</div>');
    return $.post(url, $.extend({ page: 1, pageSize: 10, filter: '{}' }, extraData || {}))
        .done(function (html) {
            $(containerSelector).html(html);
        })
        .fail(function () {
            $(containerSelector).html('<div class="govuk-error-message">Unable to load data.</div>');
        });
}

function autoSelectFirstGridRow(containerSelector) {
    window.setTimeout(function () {
        var $container = $(containerSelector);
        if ($container.find('tbody tr.selected-row').length > 0) {
            return;
        }

        var $firstRow = $container.find('tbody tr.selectable-row').first();
        if ($firstRow.length > 0) {
            $firstRow.trigger('click');
        }
    }, 0);
}

function parseBool(value) {
    return value === true || value === 'true' || value === 'True';
}

function getProgrammeGridFilterJson() {
    var filterModel = {};
    $('#gridContainer_pimsRadTrackProgTable .grid-filter').each(function () {
        var prop = $(this).data('filter');
        var val = $(this).val();
        if (val !== undefined && val !== null && val !== '') {
            filterModel[prop] = val;
        }
    });
    return JSON.stringify(filterModel);
}

function getProgrammeGridStateFromDom() {
    var state = {
        sortBy: '',
        descending: false,
        pageSize: $('#pimsRadTrackProgTable_pageSize').val() || 10
    };

    var $sortedHeader = $('#gridContainer_pimsRadTrackProgTable .sortable-header').filter(function () {
        return ($(this).find('.sort-icon').text() || '').trim().length > 0;
    }).first();

    if ($sortedHeader.length > 0) {
        state.sortBy = $sortedHeader.data('column') || '';
        state.descending = !parseBool($sortedHeader.data('sortdir'));
    }

    return state;
}

function wireProgrammeGridHandlers() {
    var containerSelector = '#gridContainer_pimsRadTrackProgTable';
    var $container = $(containerSelector);

    $container.off('click', '.sortable-header');
    $container.off('click', '.pagination a, .pagination span');
    $container.off('change', '.grid-filter');
    $('#pimsRadTrackProgTable_pageSize').off('change');

    $container.on('click', '.sortable-header', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();

        loadProgrammeGrid({
            page: 1,
            sortBy: $(this).data('column') || '',
            descending: parseBool($(this).data('sortdir'))
        });

        return false;
    });

    $container.on('click', '.pagination a, .pagination span', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();

        if ($(this).hasClass('pagination--disabled') || $(this).attr('aria-disabled') === 'true') return false;
        var pageNo = $(this).data('pageno');
        if (!pageNo || isNaN(pageNo) || pageNo < 1) return false;

        loadProgrammeGrid({ page: pageNo });
        return false;
    });

    $container.on('change', '.grid-filter', function () {
        loadProgrammeGrid({ page: 1 });
    });

    $('#pimsRadTrackProgTable_pageSize').on('change', function () {
        loadProgrammeGrid({
            page: 1,
            pageSize: $(this).val()
        });
    });
}

function loadProgrammeGrid(customParams) {
    var state = getProgrammeGridStateFromDom();
    var params = {
        page: (customParams && customParams.page != null) ? customParams.page : 1,
        pageSize: (customParams && customParams.pageSize != null && customParams.pageSize !== '')
            ? customParams.pageSize
            : state.pageSize,
        sortBy: (customParams && customParams.sortBy != null && customParams.sortBy !== '')
            ? customParams.sortBy
            : state.sortBy,
        descending: (customParams && customParams.descending != null)
            ? customParams.descending
            : state.descending,
        filter: (customParams && customParams.filter != null)
            ? customParams.filter
            : getProgrammeGridFilterJson()
    };

    return $.post('/PIMS/Maintenance/LoadRadTrackProgsGrid', params)
        .done(function (html) {
            $('#gridContainer_pimsRadTrackProgTable').html(html);
            wireProgrammeGridHandlers();
        })
        .fail(function () {
            $('#gridContainer_pimsRadTrackProgTable').html('<div class="govuk-error-message">Unable to load data.</div>');
        });
}

function loadMaintenanceTab(tabKey) {
    var panel = document.querySelector('.govuk-tabs__panel[data-tab-key="' + tabKey + '"]');
    if (!panel || panel.getAttribute('data-tab-loaded') === 'true') {
        return;
    }

    if (tabKey === 'programme') {
        loadProgrammeGrid({ page: 1, pageSize: 10, filter: '{}' })
            .done(function () {
                panel.setAttribute('data-tab-loaded', 'true');
            });
        return;
    }

    if (tabKey === 'manager') {
        loadGridIntoContainer('/PIMS/Maintenance/LoadProjectManagersGrid', '#gridContainer_mgrTable')
            .done(function () {
                panel.setAttribute('data-tab-loaded', 'true');
                autoSelectFirstGridRow('#gridContainer_mgrTable');
            });
        return;
    }

    if (tabKey === 'time') {
        $.get('/PIMS/Maintenance/LoadTimeTabSettings')
            .done(function (data) {
                console.log('LoadTimeTabSettings response:', data);

                // Populate the input fields with the retrieved values
                var hoursValue = (data && data.workingHours) ? data.workingHours : '';
                var daysValue = (data && data.workingDays) ? data.workingDays : '';

                console.log('Hours value:', hoursValue, 'Days value:', daysValue);

                $('#timeWorkingHours').val(hoursValue);
                $('#timeWorkingDays').val(daysValue);

                // Clear any validation errors if values were loaded successfully
                if (hoursValue) {
                    $('#timeWorkingHoursGroup').removeClass('govuk-form-group--error');
                    $('#timeWorkingHours').removeClass('govuk-input--error');
                    $('#timeWorkingHoursError').addClass('ra-hidden');
                }
                if (daysValue) {
                    $('#timeWorkingDaysGroup').removeClass('govuk-form-group--error');
                    $('#timeWorkingDays').removeClass('govuk-input--error');
                    $('#timeWorkingDaysError').addClass('ra-hidden');
                }

                initializeTimeTabState();
                panel.setAttribute('data-tab-loaded', 'true');
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                console.error('LoadTimeTabSettings failed:', textStatus, errorThrown);
                $('#timeDbErrorText').text('Unable to load time settings.');
                $('#timeDbError').removeClass('ra-hidden');
            });
        return;
    }

    if (tabKey === 'admin-maintenance') {
        $.when(
            loadGridIntoContainer('/PIMS/Maintenance/LoadAccessUsersGrid', '#gridContainer_adminUsersTable'),
            loadGridIntoContainer('/PIMS/Maintenance/LoadAccessUserLevelsGrid', '#gridContainer_adminAccessTable'))
            .done(function () {
                panel.setAttribute('data-tab-loaded', 'true');
            });
        return;
    }

    if (tabKey === 'other') {
        $.getJSON('/PIMS/Maintenance/GetOtherListDescriptions')
            .done(function (descriptions) {
                var $tbody = $('#otherListBody');
                $tbody.empty();
                $.each(descriptions, function (i, item) {
                    $tbody.append('<tr><td id="' + item.key + '" class="other-list-item" data-listdesc="' + item.key + '">' + item.value + '</td></tr>');
                });
                initializeOtherTabState();
                panel.setAttribute('data-tab-loaded', 'true');
            })
            .fail(function () {
                $('#otherListBody').html('<tr><td class="govuk-error-message">Unable to load list.</td></tr>');
            });
    }
}

// ════════════════════════════════════════════════════════════════════════════
//  GRID RELOAD HELPERS
//  ALWAYS use gm.reloadGrid({ page: 1 }) — NOT gm.reload()
// ════════════════════════════════════════════════════════════════════════════

function reloadReportsGrid() {
    var gm = window['gridManager_reportsGrid'];
    if (gm) { gm.reloadGrid({ page: 1 }); }
}

function reloadReportGroupsGrid() {
    var gm = window['gridManager_reportGroupsGrid'];
    if (gm) { gm.reloadGrid({ page: 1 }); }
}

function reloadRadTrackProgsGrid() {
    loadProgrammeGrid({ page: 1 });
}

function reloadProjectManagersGrid() {
    var gm = window['gridManager_mgrTable'];
    if (gm) { gm.reloadGrid({ page: 1 }); }
}

function reloadProgramManagerLinksGrid() {
    var gm = window['gridManager_mgrProgramTable'];
    if (gm) {
        gm.reloadGrid({ page: 1 });
        return;
    }

    if (_currentManagerName) {
        $.post('/PIMS/Maintenance/LoadProgramManagerLinksGrid',
            { manager: _currentManagerName, page: 1, pageSize: 10, filter: '{}' })
            .done(function (html) { $('#gridContainer_mgrProgramTable').html(html); });
    }
}

function reloadProfitCentreManagerLinksGrid() {
    var gm = window['gridManager_mgrResourceTable'];
    if (gm) {
        gm.reloadGrid({ page: 1 });
        return;
    }

    if (_currentManagerName) {
        $.post('/PIMS/Maintenance/LoadProfitCentreManagerLinksGrid',
            { manager: _currentManagerName, page: 1, pageSize: 10, filter: '{}' })
            .done(function (html) { $('#gridContainer_mgrResourceTable').html(html); });
    }
}

function reloadAccessUsersGrid() {
    var gm = window['gridManager_adminUsersTable'];
    if (gm) { gm.reloadGrid({ page: 1 }); }
}

function reloadAccessUserLevelsGrid() {
    var gm = window['gridManager_adminAccessTable'];
    if (gm) { gm.reloadGrid({ page: 1 }); }
}

function reloadOtherValuesGrid() {
    var gm = window['gridManager_otherValuesTable'];
    if (gm) { gm.reloadGrid({ page: 1 }); }
}

function reloadFrequenciesGrid() {
    reloadOtherValuesGrid();
}

function reloadReviewItemsGrid() {
    reloadOtherValuesGrid();
}

function reloadRisksGrid() {
    reloadOtherValuesGrid();
}

function reloadPublicationTypesGrid() {
    reloadOtherValuesGrid();
}

// ════════════════════════════════════════════════════════════════════════════
//  EXTRA FILTER METHODS (called automatically by _DataGrid)
// ════════════════════════════════════════════════════════════════════════════

function getReportsExtraFilters()              { return {}; }
function getReportGroupsExtraFilters()         { return _currentReportId !== null ? { reportid: _currentReportId } : {}; }
function getRadTrackProgsExtraFilters()        { return {}; }
function getProjectManagersExtraFilters()      { return {}; }
function getProgramManagerLinksExtraFilters()  { return { manager: _currentManagerName }; }
function getProfitCentreManagerLinksExtraFilters() { return { manager: _currentManagerName }; }
function getAccessUsersExtraFilters()          { return {}; }
function getAccessUserLevelsExtraFilters()     { return {}; }
function getFrequenciesExtraFilters()          { return {}; }
function getReviewItemsExtraFilters()          { return {}; }
function getRisksExtraFilters()                { return {}; }
function getPublicationTypesExtraFilters()     { return {}; }
function getOtherValuesExtraFilters()          { return {}; }

// ════════════════════════════════════════════════════════════════════════════
//  MODAL HELPERS
// ════════════════════════════════════════════════════════════════════════════

function closeModal() {
    $('#modaPopupBody').html('');
    $('#modalPopup').removeClass('show');
}

// ════════════════════════════════════════════════════════════════════════════
//  REPORTS TAB — Add / Edit / Delete
// ════════════════════════════════════════════════════════════════════════════

function saveReport() {
    var $form       = $('#formReport');
    var $banner     = $('#reportDbError');
    var $bannerText = $('#reportDbErrorText');

    displayClientValidationErrors($form, $form);
    if (!isFormValid($form)) return;

    $banner.hide();

    $.post({
        url: '/PIMS/Maintenance/SaveReport',
        data: $form.serialize(),
        success: function (data) {
            if (data.success) {
                showAlertMessage('Report saved successfully.', AlertType.SUCCESS)
                    .then(function () {
                        closeModal();
                        reloadReportsGrid();
                    });
            } else {
                if (data.errors) {
                    displayServerValidationErrors(data.errors, data.message, $form);
                }
                $bannerText.text(data.message || 'Save failed.');
                $banner.show();
            }
        },
        error: function () {
            $bannerText.text('An error occurred while saving.');
            $banner.show();
        }
    });
}

function addReport() {
    $.get('/PIMS/Maintenance/GetAddEditReportPartial', function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function editReport(id) {
    var id = $(id).data('id');
    $.get('/PIMS/Maintenance/GetAddEditReportPartial', { id: id }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function deleteReport(btn) {
    var id = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this report?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/PIMS/Maintenance/DeleteReport?id=' + encodeURIComponent(id),
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    showAlertMessage('Report deleted successfully.', AlertType.SUCCESS)
                        .then(function () { reloadReportsGrid(); });
                } else {
                    showAlertMessage(response.message || 'Delete failed.', AlertType.ERROR);
                }
            },
            error: function () { showAlertMessage('An error occurred while deleting the report.', AlertType.ERROR); }
        });
    });
}

function onReportRowSelect(row) {
    var id = $(row).data('id');
    _currentReportId = id;

    var gm = window['gridManager_reportGroupsGrid'];
    if (gm) {
        gm.reloadGrid({ page: 1 });
    } else {
        $.post('/PIMS/Maintenance/LoadReportGroupsGrid',
            { reportid: id, page: 1, pageSize: 10, filter: '{}' })
            .done(function (html) {
                $('#gridContainer_reportGroupsGrid').html(html);
            })
            .fail(function (xhr) {
                console.error('Failed to load Report Groups grid:', xhr.responseText);
            });
    }
}

function onProjectManagerRowSelect(row) {
    _currentManagerName = $(row).data('id') || '';
    if (!_currentManagerName) return;

    document.getElementById('mgrSubGrids')?.classList.remove('ra-subgrids-hidden');

    reloadProgramManagerLinksGrid();
    reloadProfitCentreManagerLinksGrid();
}

// ════════════════════════════════════════════════════════════════════════════
//  REPORTS TAB — Report Groups
// ════════════════════════════════════════════════════════════════════════════

function addReportGroup() {
    var url = '/PIMS/Maintenance/GetAddEditReportGroupPartial';
    if (_currentReportId !== null) {
        url += '?reportid=' + encodeURIComponent(_currentReportId);
    }
    $.get(url, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function saveReportGroup() {
    const $form = $('#formReportGroup');
    const $banner = $('#groupDbError');
    const $bannerText = $('#groupDbErrorText');

    displayClientValidationErrors($form, $form);

    if (!isFormValid($form)) return;

    $banner.hide();

    $.post('/PIMS/Maintenance/SaveReportGroup', $form.serialize())
        .done(function (data) {
            if (data.success) {
                showAlertMessage('Report group saved successfully.', AlertType.SUCCESS)
                    .then(function () {
                        closeModal();
                        reloadReportGroupsGrid();
                    });
                return;
            }

            if (data.errors) {
                displayServerValidationErrors(data.errors, data.message, $form);
            }

            const serverMessage = data.message
                || (Array.isArray(data.errors) && data.errors.length > 0 && data.errors[0].message)
                || 'Save failed.';

            $bannerText.text(serverMessage);
            $banner.show();
        })
        .fail(function (xhr) {
            const response = xhr && xhr.responseJSON ? xhr.responseJSON : null;
            const serverMessage = (response && (response.message
                || (Array.isArray(response.errors) && response.errors.length > 0 && response.errors[0].message)))
                || xhr.responseText
                || 'An error occurred while saving.';

            $bannerText.text(serverMessage);
            $banner.show();
        });
}

function editReportGroup(btn) {
    var groupid = $(btn).data('id');
    $.get('/PIMS/Maintenance/GetAddEditReportGroupPartial', { groupid: groupid }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function deleteReportGroup(btn) {
    var groupid = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this report group?').then(function (result) {
        if (!result) return;
        var url = '/PIMS/Maintenance/DeleteReportGroup?groupid=' + encodeURIComponent(groupid);
        if (_currentReportId !== null) {
            url += '&reportid=' + encodeURIComponent(_currentReportId);
        }

        $.ajax({
            url: url,
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    showAlertMessage('Report group deleted successfully.', AlertType.SUCCESS)
                        .then(function () { reloadReportGroupsGrid(); });
                } else {
                    showAlertMessage(response.message || 'Delete failed.', AlertType.ERROR);
                }
            },
            error: function () { showAlertMessage('An error occurred while deleting the report group.', AlertType.ERROR); }
        });
    });
}

// ════════════════════════════════════════════════════════════════════════════
//  PROGRAMME TAB
// ════════════════════════════════════════════════════════════════════════════

function getPimsAntiForgeryToken() {
    return $('#pimsMaintenanceAntiForgeryForm input[name="__RequestVerificationToken"]').val()
        || $('input[name="__RequestVerificationToken"]').first().val()
        || '';
}

function saveRadTrackProg() {
    const $form = $('#formRadTrackProg');
    const $banner = $('#progProgDbError');
    const $bannerText = $('#progProgDbErrorText');

    displayClientValidationErrors($form, $form);

    if (!isFormValid($form)) return;

    $banner.addClass('ra-hidden');

    $.post('/PIMS/Maintenance/SaveRadTrackProg', $form.serialize())
        .done(function (data) {
            if (data.success) {
                showAlertMessage(data.message || 'Programme saved successfully.', AlertType.SUCCESS)
                    .then(function () {
                        closeModal();
                        reloadRadTrackProgsGrid();
                    });
                return;
            }

            if (data.errors) {
                displayServerValidationErrors(data.errors, data.message, $form);
            }

            const serverMessage = data.message
                || (Array.isArray(data.errors) && data.errors.length > 0 && data.errors[0].message)
                || 'Save failed.';

            $bannerText.text(serverMessage);
            $banner.removeClass('ra-hidden');
        })
        .fail(function (xhr) {
            const response = xhr && xhr.responseJSON ? xhr.responseJSON : null;
            const serverMessage = (response && (response.message
                || (Array.isArray(response.errors) && response.errors.length > 0 && response.errors[0].message)))
                || xhr.responseText
                || 'An error occurred while saving.';

            $bannerText.text(serverMessage);
            $banner.removeClass('ra-hidden');
        });
}

$(document).off('change.radtrackprog', '#progProgName').on('change.radtrackprog', '#progProgName', function () {
    clearValidationErrors('#formRadTrackProg');
    $('#progProgDbError').addClass('ra-hidden');
    $('#progProgDbErrorText').text('');
});

function addRadTrackProg() {
    $.get('/PIMS/Maintenance/GetAddEditRadTrackProgPartial', function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function editRadTrackProg(btn) {
    var program = $(btn).data('id');
    $.get('/PIMS/Maintenance/GetAddEditRadTrackProgPartial', { program: program }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function deleteRadTrackProg(btn) {
    var program = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this programme?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/PIMS/Maintenance/DeleteRadTrackProg',
            type: 'POST',
            data: { program: program },
            headers: { 'RequestVerificationToken': getPimsAntiForgeryToken() },
            success: function (response) {
                if (response.success) {
                    showAlertMessage(response.message || 'Programme deleted successfully.', AlertType.SUCCESS)
                        .then(function () { reloadRadTrackProgsGrid(); });
                } else {
                    showAlertMessage(response.message || 'Delete failed.', AlertType.ERROR);
                }
            },
            error: function () { showAlertMessage('An error occurred while deleting the programme.', AlertType.ERROR); }
        });
    });
}

// ════════════════════════════════════════════════════════════════════════════
//  MANAGER TAB — Project Managers
// ════════════════════════════════════════════════════════════════════════════

function addProjectManager() {
    $.get('/PIMS/Maintenance/GetAddEditProjectManagerPartial', function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function editProjectManager(btn) {
    var projectmanager = $(btn).data('id');
    $.get('/PIMS/Maintenance/GetAddEditProjectManagerPartial', { projectmanager: projectmanager }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function deleteProjectManager(btn) {
    var projectmanager = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this manager?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/PIMS/Maintenance/DeleteProjectManager?projectmanager=' + encodeURIComponent(projectmanager),
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    showAlertMessage('Manager deleted successfully.', AlertType.SUCCESS)
                        .then(function () { reloadProjectManagersGrid(); });
                } else {
                    showAlertMessage(response.message || 'Delete failed.', AlertType.ERROR);
                }
            },
            error: function () { showAlertMessage('An error occurred while deleting the manager.', AlertType.ERROR); }
        });
    });
}

function saveProjectManager() {
    const $form = $('#formProjectManager');
    const $banner = $('#mgrEditDbError');
    const $bannerText = $('#mgrEditDbErrorText');

    displayClientValidationErrors($form, $form);

    if (!isFormValid($form)) return;

    $banner.addClass('ra-hidden');

    $.post('/PIMS/Maintenance/SaveProjectManager', $form.serialize())
        .done(function (data) {
            if (data.success) {
                showAlertMessage(data.message || 'Manager saved successfully.', AlertType.SUCCESS)
                    .then(function () {
                        closeModal();
                        reloadProjectManagersGrid();
                    });
                return;
            }

            if (data.errors && data.errors.length > 0) {
                $bannerText.text('');
                $banner.addClass('ra-hidden');
                displayServerValidationErrors(data.errors, data.message || 'Save failed.', $form);
                return;
            }

            const serverMessage = data.message
                || (Array.isArray(data.errors) && data.errors.length > 0 && data.errors[0].message)
                || 'Save failed.';

            $bannerText.text(serverMessage);
            $banner.removeClass('ra-hidden');
        })
        .fail(function (xhr) {
            const response = xhr && xhr.responseJSON ? xhr.responseJSON : null;
            const serverMessage = (response && (response.message
                || (Array.isArray(response.errors) && response.errors.length > 0 && response.errors[0].message)))
                || xhr.responseText
                || 'An error occurred while saving.';

            $bannerText.text(serverMessage);
            $banner.removeClass('ra-hidden');
        });
}

// ════════════════════════════════════════════════════════════════════════════
//  MANAGER TAB — Program Manager Links (sub-grid)
// ════════════════════════════════════════════════════════════════════════════

function initializeProgramManagerLinkDropdown() {
    var container = document.getElementById('mgrAssignProgramDropdownContainer');
    var dataElement = document.getElementById('mgrAssignProgramOptionsData');
    if (!container || !dataElement || typeof MultiColumnDropdownComponent === 'undefined') {
        return;
    }

    var dropdownData = [];
    try {
        dropdownData = JSON.parse(dataElement.textContent || '[]');
    } catch (error) {
        console.error('Failed to parse manager program dropdown data.', error);
    }

    if (_programManagerLinkDropdown && typeof _programManagerLinkDropdown.destroy === 'function') {
        _programManagerLinkDropdown.destroy();
    }

    _programManagerLinkDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'mgrAssignProgramDropdown',
        containerSelector: '#mgrAssignProgramDropdownContainer',
        placeholder: 'Select a Program',
        searchPlaceholder: 'Search by program or year',
        ariaLabelledBy: 'mgrAssignLabel',
        showSerialNumber: false,
        required: true,
        clearButtonClearsSelection: true,
        columns: [
            { field: 'ProgramNo', header: 'Program No', width: '180px' },
            { field: 'LatestYear', header: 'Latest Year', width: '120px' }
        ],
        data: dropdownData,
        displayField: function (item) { return item && item.ProgramNo ? item.ProgramNo : ''; },
        valueField: 'ProgramNo',
        callbacks: {
            onSelect: function (selectedItem) {
                $('#mgrAssignValue').val(selectedItem && selectedItem.ProgramNo ? selectedItem.ProgramNo : '');
                $('#mgrAssignValueGroup').removeClass('govuk-form-group--error');
                $('#mgrAssignProgramDropdown_input').removeClass('govuk-input--error');
                $('#mgrAssignValueError').text('').hide()
                    .removeClass('field-validation-error')
                    .addClass('field-validation-valid');
            },
            onClear: function () {
                $('#mgrAssignValue').val('');
            }
        }
    });

    var $displayInput = $('#mgrAssignProgramDropdown_input');
    $displayInput.attr({
        name: 'Program',
        required: 'required',
        'data-val-required': 'Program is required',
        'aria-describedby': 'mgrAssignValueError'
    });

    var selectedProgram = $('#mgrAssignValue').val() || $('#hdnOriginalProgram').val() || '';
    if (selectedProgram) {
        _programManagerLinkDropdown.setValue(selectedProgram);
    }
}

function addProgramManagerLink() {
    if (!_currentManagerName) {
        showAlertMessage('Please select a manager first.', AlertType.ERROR);
        return;
    }

    $.get('/PIMS/Maintenance/GetAddEditProgramManagerLinkPartial',
        { manager: _currentManagerName }, function (html) {
            $('#modaPopupBody').html(html);
            initializeProgramManagerLinkDropdown();
            $('#modalPopup').addClass('show');
        });
}

function editProgramManagerLink(btn) {
    var program = $(btn).data('id');
    if (!_currentManagerName) {
        showAlertMessage('Please select a manager first.', AlertType.ERROR);
        return;
    }

    $.get('/PIMS/Maintenance/GetAddEditProgramManagerLinkPartial',
        { manager: _currentManagerName, program: program }, function (html) {
            $('#modaPopupBody').html(html);
            initializeProgramManagerLinkDropdown();
            $('#modalPopup').addClass('show');
        });
}

function deleteProgramManagerLink(btn) {
    var program = $(btn).data('id');
    if (!_currentManagerName) {
        showAlertMessage('Please select a manager first.', AlertType.ERROR);
        return;
    }

    showGovukConfirm('Are you sure you want to remove this programme assignment?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/PIMS/Maintenance/DeleteProgramManagerLink?program=' + encodeURIComponent(program)
                + '&manager=' + encodeURIComponent(_currentManagerName),
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    showAlertMessage(response.message || 'Programme assignment deleted successfully.', AlertType.SUCCESS)
                        .then(function () { reloadProgramManagerLinksGrid(); });
                } else {
                    showAlertMessage(response.message || 'Delete failed.', AlertType.ERROR);
                }
            },
            error: function () { showAlertMessage('An error occurred while deleting the programme assignment.', AlertType.ERROR); }
        });
    });
}

function saveProgramManagerLink() {
    var $form = $('#formProgramManagerLink');
    var $banner = $('#mgrAssignDbError');
    var $bannerText = $('#mgrAssignDbErrorText');

    displayClientValidationErrors($form, $form);

    if (!isFormValid($form)) return;

    var program = document.getElementById('mgrAssignValue')?.value || '';
    var manager = document.getElementById('hdnLinkManager')?.value || _currentManagerName;

    if (!manager) {
        $bannerText.text('Please select a manager first.');
        $banner.removeClass('ra-hidden');
        return;
    }

    $banner.addClass('ra-hidden');

    var isEditMode = (document.getElementById('hdnIsProgramEditMode')?.value || '').toLowerCase() === 'true';
    var originalProgram = document.getElementById('hdnOriginalProgram')?.value || '';
    var url = isEditMode ? '/PIMS/Maintenance/UpdateProgramManagerLink' : '/PIMS/Maintenance/SaveProgramManagerLink';

    var payload = isEditMode
        ? { originalProgram: originalProgram, originalManager: manager, program: program, manager: manager }
        : { program: program, manager: manager };

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(payload),
        headers: { 'RequestVerificationToken': $('#formProgramManagerLink input[name="__RequestVerificationToken"]').val() },
        success: function (data) {
            if (data.success) {
                showAlertMessage(data.message || (isEditMode ? 'Programme assignment updated successfully.' : 'Programme assignment added successfully.'), AlertType.SUCCESS)
                    .then(function () {
                        closeModal();
                        reloadProgramManagerLinksGrid();
                    });
                return;
            }

            if (data.errors) {
                $bannerText.text('');
                displayServerValidationErrors(data.errors, data.message, $form);
                return;
            }

            var msg = data.message || 'Save failed.';
            $bannerText.text(msg);
            $banner.removeClass('ra-hidden');
        },
        error: function (xhr) {
            var response = xhr && xhr.responseJSON ? xhr.responseJSON : null;
            var msg = (response && (response.message || (Array.isArray(response.errors) && response.errors.length > 0 && response.errors[0].message)))
                || 'An error occurred while saving.';
            $bannerText.text(msg);
            $banner.removeClass('ra-hidden');
        }
    });
}

// ════════════════════════════════════════════════════════════════════════════
//  MANAGER TAB — Profit Centre Manager Links (sub-grid)
// ════════════════════════════════════════════════════════════════════════════

function initializeProfitCentreManagerLinkDropdown() {
    var container = document.getElementById('pcAssignProfitCentreDropdownContainer');
    var dataElement = document.getElementById('pcAssignProfitCentreOptionsData');
    if (!container || !dataElement || typeof MultiColumnDropdownComponent === 'undefined') {
        return;
    }

    var dropdownData = [];
    try {
        dropdownData = JSON.parse(dataElement.textContent || '[]');
    } catch (error) {
        console.error('Failed to parse manager resource centre dropdown data.', error);
    }

    if (_profitCentreManagerLinkDropdown && typeof _profitCentreManagerLinkDropdown.destroy === 'function') {
        _profitCentreManagerLinkDropdown.destroy();
    }

    _profitCentreManagerLinkDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'pcAssignProfitCentreDropdown',
        containerSelector: '#pcAssignProfitCentreDropdownContainer',
        placeholder: 'Select resource centre',
        searchPlaceholder: 'Search by resource centre or year',
        ariaLabelledBy: 'pcAssignLabel',
        showSerialNumber: false,
        required: true,
        clearButtonClearsSelection: true,
        columns: [
            { field: 'ProfitCentre', header: 'Profit Centre', width: '180px' },
            { field: 'LatestYear', header: 'Latest Year', width: '120px' }
        ],
        data: dropdownData,
        displayField: function (item) { return item && item.ProfitCentre ? item.ProfitCentre : ''; },
        valueField: 'ProfitCentre',
        callbacks: {
            onSelect: function (selectedItem) {
                $('#pcAssignValue').val(selectedItem && selectedItem.ProfitCentre ? selectedItem.ProfitCentre : '');
                $('#pcAssignValueGroup').removeClass('govuk-form-group--error');
                $('#pcAssignProfitCentreDropdown_input').removeClass('govuk-input--error');
                $('#pcAssignValueError').text('').hide()
                    .removeClass('field-validation-error')
                    .addClass('field-validation-valid');
            },
            onClear: function () {
                $('#pcAssignValue').val('');
            }
        }
    });

    var $displayInput = $('#pcAssignProfitCentreDropdown_input');
    $displayInput.attr({
        name: 'ProfitCentre',
        required: 'required',
        'data-val-required': 'Resource Centre is required',
        'aria-describedby': 'pcAssignValueError'
    });

    var selectedProfitCentre = $('#pcAssignValue').val() || $('#hdnOriginalProfitCentre').val() || '';
    if (selectedProfitCentre) {
        _profitCentreManagerLinkDropdown.setValue(selectedProfitCentre);
    }
}

function addProfitCentreManagerLink() {
    if (!_currentManagerName) {
        showAlertMessage('Please select a manager first.', AlertType.ERROR);
        return;
    }

    $.get('/PIMS/Maintenance/GetAddEditProfitCentreManagerLinkPartial',
        { manager: _currentManagerName }, function (html) {
            $('#modaPopupBody').html(html);
            initializeProfitCentreManagerLinkDropdown();
            $('#modalPopup').addClass('show');
        });
}

function editProfitCentreManagerLink(btn) {
    var profitcentre = $(btn).data('id');
    if (!_currentManagerName) {
        showAlertMessage('Please select a manager first.', AlertType.ERROR);
        return;
    }

    $.get('/PIMS/Maintenance/GetAddEditProfitCentreManagerLinkPartial',
        { manager: _currentManagerName, profitcentre: profitcentre }, function (html) {
            $('#modaPopupBody').html(html);
            initializeProfitCentreManagerLinkDropdown();
            $('#modalPopup').addClass('show');
        });
}

function deleteProfitCentreManagerLink(btn) {
    var profitcentre = $(btn).data('id');
    if (!_currentManagerName) {
        showAlertMessage('Please select a manager first.', AlertType.ERROR);
        return;
    }

    showGovukConfirm('Are you sure you want to remove this resource centre assignment?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/PIMS/Maintenance/DeleteProfitCentreManagerLink?profitcentre=' + encodeURIComponent(profitcentre)
                + '&manager=' + encodeURIComponent(_currentManagerName),
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    showAlertMessage(response.message || 'Resource Centre assignment deleted successfully.', AlertType.SUCCESS)
                        .then(function () { reloadProfitCentreManagerLinksGrid(); });
                } else {
                    showAlertMessage(response.message || 'Delete failed.', AlertType.ERROR);
                }
            },
            error: function () { showAlertMessage('An error occurred while deleting the resource centre assignment.', AlertType.ERROR); }
        });
    });
}

function saveProfitCentreManagerLink() {
    var $form = $('#formProfitCentreManagerLink');
    var $banner = $('#pcAssignDbError');
    var $bannerText = $('#pcAssignDbErrorText');

    displayClientValidationErrors($form, $form);

    if (!isFormValid($form)) return;

    var profitcentre = document.getElementById('pcAssignValue')?.value?.trim() || '';
    var manager = document.getElementById('hdnPcLinkManager')?.value || _currentManagerName;

    if (!manager) {
        $bannerText.text('Please select a manager first.');
        $banner.removeClass('ra-hidden');
        return;
    }

    $banner.addClass('ra-hidden');

    var isEditMode = (document.getElementById('hdnIsProfitCentreEditMode')?.value || '').toLowerCase() === 'true';
    var originalProfitCentre = document.getElementById('hdnOriginalProfitCentre')?.value || '';
    var url = isEditMode ? '/PIMS/Maintenance/UpdateProfitCentreManagerLink' : '/PIMS/Maintenance/SaveProfitCentreManagerLink';

    var payload = isEditMode
        ? { originalProfitcentre: originalProfitCentre, originalManager: manager, profitcentre: profitcentre, manager: manager }
        : { profitcentre: profitcentre, manager: manager };

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(payload),
        headers: { 'RequestVerificationToken': $('#formProfitCentreManagerLink input[name="__RequestVerificationToken"]').val() },
        success: function (data) {
            if (data.success) {
                showAlertMessage(data.message || (isEditMode ? 'Resource Centre assignment updated successfully.' : 'Resource Centre assignment added successfully.'), AlertType.SUCCESS)
                    .then(function () {
                        closeModal();
                        reloadProfitCentreManagerLinksGrid();
                    });
                return;
            }

            if (data.errors) {
                $bannerText.text('');
                displayServerValidationErrors(data.errors, data.message, $form);
                return;
            }

            var msg = data.message || 'Save failed.';
            $bannerText.text(msg);
            $banner.removeClass('ra-hidden');
        },
        error: function (xhr) {
            var response = xhr && xhr.responseJSON ? xhr.responseJSON : null;
            var msg = (response && (response.message || (Array.isArray(response.errors) && response.errors.length > 0 && response.errors[0].message)))
                || 'An error occurred while saving.';
            $bannerText.text(msg);
            $banner.removeClass('ra-hidden');
        }
    });
}

// ════════════════════════════════════════════════════════════════════════════
//  TIME TAB
// ════════════════════════════════════════════════════════════════════════════

// ── Time Tab Input Validation ────────────────────────────────────────────────
// Align with Costbook validation: required, positive decimal only, max 999.99.
function validateTimeInput(inputElement, errorElementId, errorTextElementId, fieldName) {
    if (!inputElement) return true;

    var value = inputElement.value;
    var trimmed = (value || '').trim();
    var errorElement = document.getElementById(errorElementId);
    var errorTextElement = document.getElementById(errorTextElementId);
    var formGroup = inputElement.closest('.govuk-form-group');

    if (errorElement) errorElement.classList.add('ra-hidden');
    if (formGroup) formGroup.classList.remove('govuk-form-group--error');
    inputElement.classList.remove('govuk-input--error');

    if (trimmed === '') {
        if (errorTextElement) errorTextElement.textContent = fieldName + ' is required';
        if (errorElement) errorElement.classList.remove('ra-hidden');
        if (formGroup) formGroup.classList.add('govuk-form-group--error');
        inputElement.classList.add('govuk-input--error');
        return false;
    }

    if (!/^\d+(\.\d+)?$/.test(trimmed)) {
        if (errorTextElement) errorTextElement.textContent = fieldName + ' must be a positive number (digits and decimal point only).';
        if (errorElement) errorElement.classList.remove('ra-hidden');
        if (formGroup) formGroup.classList.add('govuk-form-group--error');
        inputElement.classList.add('govuk-input--error');
        return false;
    }

    var numValue = parseFloat(trimmed);
    if (numValue <= 0) {
        if (errorTextElement) errorTextElement.textContent = fieldName + ' must be greater than zero';
        if (errorElement) errorElement.classList.remove('ra-hidden');
        if (formGroup) formGroup.classList.add('govuk-form-group--error');
        inputElement.classList.add('govuk-input--error');
        return false;
    }

    if (numValue > 999.99) {
        if (errorTextElement) errorTextElement.textContent = fieldName + ' must not be greater than 999.99.';
        if (errorElement) errorElement.classList.remove('ra-hidden');
        if (formGroup) formGroup.classList.add('govuk-form-group--error');
        inputElement.classList.add('govuk-input--error');
        return false;
    }

    return true;
}

function saveTimeTab() {
    var hoursEl = document.getElementById('timeWorkingHours');
    var daysEl = document.getElementById('timeWorkingDays');

    var hoursVal = hoursEl?.value || '';
    var daysVal = daysEl?.value || '';

    // IDs are injected by the Razor view into window._pimsHoursId / window._pimsDaysId
    var hoursId = window._pimsHoursId || 'WorkingHours';
    var daysId = window._pimsDaysId || 'WorkingDays';

    var dbError = document.getElementById('timeDbError');
    var dbErrorText = document.getElementById('timeDbErrorText');

    // Hide all errors initially
    if (dbError) dbError.classList.add('ra-hidden');

    // Validate both inputs using the validateTimeInput function
    var hoursValid = validateTimeInput(hoursEl, 'timeWorkingHoursError', 'timeWorkingHoursErrorText', 'Working Hours in a Day');
    var daysValid = validateTimeInput(daysEl, 'timeWorkingDaysError', 'timeWorkingDaysErrorText', 'Working Days in a Year');

    if (!hoursValid || !daysValid) {
        return;
    }

    // Disable save button during save
    var btnSave = document.getElementById('btnSaveTime');
    if (btnSave) {
        btnSave.disabled = true;
        btnSave.textContent = 'Saving...';
    }

    $.ajax({
        url: '/PIMS/Maintenance/SaveSetting',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify({ id: hoursId, settingValue: hoursVal }),
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').first().val() },
        success: function (data) {
            if (!data.success) {
                if (dbErrorText) dbErrorText.textContent = data.message || 'Failed to save working hours.';
                if (dbError) dbError.classList.remove('ra-hidden');
                if (btnSave) {
                    btnSave.disabled = false;
                    btnSave.textContent = 'Save';
                }
                return;
            }
            // Save working days
            $.ajax({
                url: '/PIMS/Maintenance/SaveSetting',
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({ id: daysId, settingValue: daysVal }),
                headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').first().val() },
                success: function (d2) {
                    if (btnSave) {
                        btnSave.disabled = false;
                        btnSave.textContent = 'Save';
                    }
                    if (d2.success) {
                        _timeOrigWorkingHours = hoursVal;
                        _timeOrigWorkingDays = daysVal;
                        showAlertMessage('Settings saved successfully.', AlertType.SUCCESS);
                    } else {
                        if (dbErrorText) dbErrorText.textContent = d2.message || 'Failed to save working days.';
                        if (dbError) dbError.classList.remove('ra-hidden');
                    }
                },
                error: function () {
                    if (btnSave) {
                        btnSave.disabled = false;
                        btnSave.textContent = 'Save';
                    }
                    if (dbErrorText) dbErrorText.textContent = 'An error occurred while saving.';
                    if (dbError) dbError.classList.remove('ra-hidden');
                }
            });
        },
        error: function () {
            if (btnSave) {
                btnSave.disabled = false;
                btnSave.textContent = 'Save';
            }
            if (dbErrorText) dbErrorText.textContent = 'An error occurred while saving.';
            if (dbError) dbError.classList.remove('ra-hidden');
        }
    });
}


function cancelTimeTab() {
    document.getElementById('timeWorkingHours').value = _timeOrigWorkingHours;
    document.getElementById('timeWorkingDays').value  = _timeOrigWorkingDays;
    var hoursError = document.getElementById('timeWorkingHoursError');
    var daysError  = document.getElementById('timeWorkingDaysError');
    var dbError    = document.getElementById('timeDbError');
    if (hoursError) hoursError.classList.add('ra-hidden');
    if (daysError)  daysError.classList.add('ra-hidden');
    if (dbError)    dbError.classList.add('ra-hidden');
}

// ════════════════════════════════════════════════════════════════════════════
//  ADMIN MAINTENANCE TAB — Access Users
// ════════════════════════════════════════════════════════════════════════════

function addAccessUser() {
    $.get('/PIMS/Maintenance/GetAddEditAccessUserPartial', function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function editAccessUser(btn) {
    var compositeKey = $(btn).data('id') || '';
    var parts = compositeKey.split('|');
    var ntlogin = parts[0] || '';
    var systemid = parseInt(parts[1], 10) || 0;

    $.get('/PIMS/Maintenance/GetAddEditAccessUserPartial',
        { systemid: systemid, ntlogin: ntlogin },
        function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        });
}

function deleteAccessUser(btn) {
    var compositeKey = $(btn).data('id') || '';
    var parts = compositeKey.split('|');
    var ntlogin = parts[0] || '';
    var systemid = parseInt(parts[1], 10) || 0;

    showGovukConfirm('Are you sure you want to delete this user?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/PIMS/Maintenance/DeleteAccessUser?systemid=' + encodeURIComponent(systemid)
                + '&ntlogin=' + encodeURIComponent(ntlogin),
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    showAlertMessage('User deleted successfully.', AlertType.SUCCESS)
                        .then(function () { reloadAccessUsersGrid(); });
                } else {
                    showAlertMessage(response.message || 'Delete failed.', AlertType.ERROR);
                }
            },
            error: function (xhr) {
                var msg = (xhr.responseJSON && xhr.responseJSON.message)
                    ? xhr.responseJSON.message
                    : 'An error occurred while deleting the user.';
                showAlertMessage(msg, AlertType.ERROR);
            }
        });
    });
}

function saveAccessUser() {
    var dbError = document.getElementById('adminUserDbError');
    var dbErrorText = document.getElementById('adminUserDbErrorText');
    if (dbError) dbError.classList.add('ra-hidden');

    var $form = $('#formAccessUser');
    displayClientValidationErrors($form, $form);
    if (!isFormValid($form)) {
        return;
    }

    $.post({
        url: '/PIMS/Maintenance/SaveAccessUser',
        data: $form.serialize(),
        success: function (data) {
            if (data.success) {
                showAlertMessage(data.message || 'User saved successfully.', AlertType.SUCCESS)
                    .then(function () {
                        closeModal();
                        reloadAccessUsersGrid();
                    });
            } else {
                if (data.errors && data.errors.length > 0) {
                    if (dbErrorText) dbErrorText.textContent = '';
                    if (dbError) dbError.classList.add('ra-hidden');
                    displayServerValidationErrors(data.errors, data.message || 'Save failed.', $form);
                    return;
                }

                if (dbErrorText) dbErrorText.textContent = data.message || 'Save failed.';
                if (dbError) dbError.classList.remove('ra-hidden');
            }
        },
        error: function (xhr) {
            var msg = (xhr.responseJSON && xhr.responseJSON.message) ? xhr.responseJSON.message : 'An error occurred while saving.';
            if (dbErrorText) dbErrorText.textContent = msg;
            if (dbError) dbError.classList.remove('ra-hidden');
        }
    });
}

// ════════════════════════════════════════════════════════════════════════════
//  ADMIN MAINTENANCE TAB — Access User Levels
// ════════════════════════════════════════════════════════════════════════════

function saveAccessUserLevel() {
    var $form       = $('#formAccessUserLevel');
    var $banner     = $('#adminAccessDbError');
    var $bannerText = $('#adminAccessDbErrorText');

    var $levelSelect = $form.find('#adminAccessLevel');
    var levelVal     = $levelSelect.val();
    if (levelVal === '0' || levelVal === '') { $levelSelect.val(''); }

    displayClientValidationErrors($form, $form);

    if (!isFormValid($form)) {
        if (levelVal === '0' || levelVal === '') { $levelSelect.val('0'); }
        return;
    }
    if (levelVal === '0' || levelVal === '') { $levelSelect.val('0'); }

    $banner.addClass('ra-hidden');

    var systemid      = parseInt($('#hdnAccessSystemid').val() || '0', 10);
    var ntlogin       = $('#adminAccessUser').val() || '';
    var accesslevelid = parseInt($('#adminAccessLevel').val() || '0', 10);

    var isEditMode = ($('#hdnAccessIsEdit').val() || '').toLowerCase() === 'true';
    var originalSystemid = parseInt($('#hdnAccessOriginalSystemid').val() || '0', 10);
    var originalNtlogin = $('#hdnAccessOriginalNtlogin').val() || '';
    var originalAccesslevelid = parseInt($('#hdnAccessOriginalAccesslevelid').val() || '0', 10);

    $.ajax({
        url:         '/PIMS/Maintenance/SaveAccessUserLevel',
        type:        'POST',
        contentType: 'application/json; charset=utf-8',
        data:        JSON.stringify({
            systemId: systemid,
            ntLogin: ntlogin,
            accessLevelId: accesslevelid,
            isEditMode: isEditMode,
            originalSystemId: originalSystemid,
            originalNtLogin: originalNtlogin,
            originalAccessLevelId: originalAccesslevelid
        }),
        headers:     { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').first().val() },
        success: function (data) {
            if (data.success) {
                showAlertMessage(data.message, AlertType.SUCCESS)
                    .then(function () {
                        closeModal();
                        reloadAccessUserLevelsGrid();
                    });
                return;
            }

            if (data.errors && data.errors.length > 0) {
                displayServerValidationErrors(data.errors, data.message, $form);
                $banner.removeClass('ra-hidden');
            } else {
                var msg = data.message || 'Save failed.';
                $bannerText.text(msg);
                $banner.removeClass('ra-hidden');
            }
        },
        error: function (xhr) {
            var msg = (xhr.responseJSON && xhr.responseJSON.message) ? xhr.responseJSON.message : 'An error occurred while saving.';
            $bannerText.text(msg);
            $banner.removeClass('ra-hidden');
        }
    });
}

function addAccessUserLevel() {
    $.get('/PIMS/Maintenance/GetAddEditAccessUserLevelPartial', function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function editAccessUserLevel(btn) {
    // CompositeKey format: "ntlogin|accesslevelid|systemid"
    var compositeKey = $(btn).data('id') || '';
    var parts = compositeKey.split('|');
    var ntlogin       = parts[0] || '';
    var accesslevelid = parseInt(parts[1], 10) || 0;
    var systemid      = parseInt(parts[2], 10) || 0;
    $.get('/PIMS/Maintenance/GetAddEditAccessUserLevelPartial',
        { systemid: systemid, ntlogin: ntlogin, accesslevelid: accesslevelid },
        function (html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        });
}

function deleteAccessUserLevel(btn) {
    // CompositeKey format: "ntlogin|accesslevelid|systemid"
    var compositeKey = $(btn).data('id') || '';
    var parts = compositeKey.split('|');
    var ntlogin       = parts[0] || '';
    var accesslevelid = parseInt(parts[1], 10) || 0;
    var systemid      = parseInt(parts[2], 10) || 0;
    showGovukConfirm('Are you sure you want to delete this user access?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/PIMS/Maintenance/DeleteAccessUserLevel?systemid=' + encodeURIComponent(systemid)
                + '&ntlogin=' + encodeURIComponent(ntlogin)
                + '&accesslevelid=' + encodeURIComponent(accesslevelid),
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    showAlertMessage('User access deleted successfully.', AlertType.SUCCESS)
                        .then(function () { reloadAccessUserLevelsGrid(); });
                } else {
                    showAlertMessage(response.message || 'Delete failed.', AlertType.ERROR);
                }
            },
            error: function () { showAlertMessage('An error occurred while deleting user access.', AlertType.ERROR); }
        });
    });
}

// ════════════════════════════════════════════════════════════════════════════
//  OTHER TAB — Dynamic list selection
// ════════════════════════════════════════════════════════════════════════════

function loadOtherValuesGrid(description) {
    var urlMap = {
        'Frequency':         '/PIMS/Maintenance/GetFrequenciesGrid?gridId=otherValuesTable',
        'ReportGroups':      '/PIMS/Maintenance/GetReportGroupsGrid?gridId=otherValuesTable',
        'ReviewItems':       '/PIMS/Maintenance/GetReviewItemsGrid?gridId=otherValuesTable',
        'Risk':              '/PIMS/Maintenance/GetRisksGrid?gridId=otherValuesTable',
        'PublicationTypes':  '/PIMS/Maintenance/GetPublicationTypesGrid?gridId=otherValuesTable'
    };

    var url = urlMap[description];
    if (!url) {
        $('#gridContainer_otherValuesTable').html('<div class="govuk-body">No grid configured for ' + description + '.</div>');
        return;
    }

    $.get(url, function (html) {
        $('#gridContainer_otherValuesTable').html(html);
    });
}

$(document).on('click', '.other-list-item', function () {
    var description = $(this).data('listdesc');
    $('.other-list-item').removeClass('selected');
    $(this).addClass('selected');
    loadOtherValuesGrid(description);
});

function getPimsOtherValidationLabel($field, $container) {
    var fieldId = $field.attr('id') || '';
    var fieldName = $field.attr('name') || '';
    return $container.find('label[for="' + fieldId + '"], label[for="' + fieldName + '"]')
        .first()
        .clone()
        .children()
        .remove()
        .end()
        .text()
        .trim()
        .replace(/:\s*$/, '') || fieldName;
}

function getPimsOtherValidationMessage($field, $container) {
    var field = $field[0];
    var label = getPimsOtherValidationLabel($field, $container);
    var minValue = $field.attr('min');
    var stepValue = $field.attr('step');
    var isPositiveWholeNumberField = $field.attr('type') === 'number'
        && (minValue === '0' || minValue === '1')
        && stepValue === '1';

    if (!field || !field.validity) {
        return label + ' is invalid';
    }

    if (field.validity.valueMissing) {
        return $field.attr('data-val-required') || (label + ' is required');
    }

    if (isPositiveWholeNumberField && (field.validity.badInput
        || field.validity.typeMismatch
        || field.validity.rangeUnderflow
        || field.validity.stepMismatch)) {
        if (minValue === '0') {
            return 'Only zero and positive numbers are acceptable.';
        } else {
            return 'Only positive numbers and should be greater than 0.';
        }
    }

    if (field.validity.badInput || field.validity.typeMismatch) {
        return $field.attr('data-val-type') || (label + ' must be a valid value');
    }

    if (field.validity.rangeUnderflow) {
        if (minValue === '0') {
            return label + ' must be 0 or greater';
        } else {
            return label + ' must be 1 or greater';
        }
    }

    return field.validationMessage || (label + ' is invalid');
}

function clearPimsOtherFieldErrorOnInput($field, fieldName, $container) {
    $field.off('input.pimsother change.pimsother')
        .on('input.pimsother change.pimsother', function () {
            if (this.willValidate && !this.checkValidity()) {
                return;
            }

            var $formGroup = $(this).closest('.govuk-form-group');
            $formGroup.removeClass('govuk-form-group--error');
            $(this).removeClass('govuk-input--error');
            $container.find('[data-valmsg-for="' + fieldName + '"]')
                .text('')
                .hide()
                .removeClass('field-validation-error')
                .addClass('field-validation-valid');
        });
}

function isPimsOtherFormValid($form) {
    var valid = true;

    $form.find(':input').each(function () {
        if (this.disabled || !this.willValidate) {
            return;
        }

        if (!this.checkValidity()) {
            valid = false;
            return false;
        }
    });

    return valid;
}

function displayPimsOtherClientValidationErrors($form) {
    clearValidationErrors($form);

    var errors = [];
    $form.find(':input').each(function () {
        if (this.disabled || !this.willValidate || this.checkValidity()) {
            return;
        }

        var $field = $(this);
        errors.push({
            field: $field.attr('name') || '',
            message: getPimsOtherValidationMessage($field, $form)
        });
    });

    errors.forEach(function (error) {
        var $field = $form.find('[name="' + error.field + '"]');
        if (!$field.length) {
            return;
        }

        var $formGroup = $field.closest('.govuk-form-group').addClass('govuk-form-group--error');
        $field.addClass('govuk-input--error');
        $formGroup.find('[data-valmsg-for="' + error.field + '"]')
            .text(error.message)
            .show()
            .removeClass('field-validation-valid')
            .addClass('field-validation-error');

        clearPimsOtherFieldErrorOnInput($field, error.field, $form);
    });
}

function saveFrequency() {
    const $form = $('#formFrequency');
    const $banner = $('#reportDbError');
    const $bannerText = $('#frequencyDbErrorText');
    const isEdit = ($form.data('is-edit') || '').toString().toLowerCase() === 'true';

    displayPimsOtherClientValidationErrors($form);

    if (!isPimsOtherFormValid($form)) return;

    $banner.hide();

    const formData = $form.serializeArray();
    formData.push({ name: 'isEdit', value: isEdit });

    $.post('/PIMS/Maintenance/SaveFrequency', $.param(formData))
        .done(function (data) {
            if (data.success) {
                showAlertMessage('Frequency saved successfully.', AlertType.SUCCESS)
                    .then(() => {
                        closeModal();
                        reloadFrequenciesGrid();
                    });
                return;
            }

            if (data.errors) {
                $bannerText.text('');
                displayServerValidationErrors(data.errors, data.message, $form);
                return;
            }

            var msg = data.message || 'Save failed.';
            $bannerText.text(msg);
            $banner.show();
        })
        .fail(function () {
            $bannerText.text('An error occurred while saving.');
            $banner.show();
        });
}

function addFrequency() {
    $.get('/PIMS/Maintenance/GetAddEditFrequencyPartial', function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function editFrequency(btn) {
    var frequencyid = $(btn).data('id');
    $.get('/PIMS/Maintenance/GetAddEditFrequencyPartial', { frequencyid: frequencyid }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function deleteFrequency(btn) {
    var frequencyid = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this frequency?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/PIMS/Maintenance/DeleteFrequency?frequencyid=' + encodeURIComponent(frequencyid),
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    showAlertMessage('Frequency deleted successfully.', AlertType.SUCCESS)
                        .then(function () { reloadFrequenciesGrid(); });
                } else {
                    showAlertMessage(response.message || 'Delete failed.', AlertType.ERROR);
                }
            },
            error: function () { showAlertMessage('An error occurred while deleting the frequency.', AlertType.ERROR); }
        });
    });
}

// ════════════════════════════════════════════════════════════════════════════
//  OTHER TAB — Review Items
// ════════════════════════════════════════════════════════════════════════════

function saveReviewItem() {
    const $form = $('#formReviewItem');
    const $banner = $('#reviewItemDbError');
    const $bannerText = $('#reviewItemDbErrorText');
    const isEdit = ($form.data('is-edit') || '').toString().toLowerCase() === 'true';

    displayPimsOtherClientValidationErrors($form);

    if (!isPimsOtherFormValid($form)) return;

    $banner.hide();

    const formData = $form.serializeArray();
    formData.push({ name: 'isEdit', value: isEdit });

    $.post('/PIMS/Maintenance/SaveReviewItem', $.param(formData))
        .done(function (data) {
            if (data.success) {
                showAlertMessage('Review item saved successfully.', AlertType.SUCCESS)
                    .then(() => {
                        closeModal();
                        reloadReviewItemsGrid();
                    });
                return;
            }

            if (data.errors) {
                $bannerText.text('');
                displayServerValidationErrors(data.errors, data.message, $form);
                return;
            }

            var msg = data.message || 'Save failed.';
            $bannerText.text(msg);
            $banner.show();
        })
        .fail(function () {
            $bannerText.text('An error occurred while saving.');
            $banner.show();
        });
}

function addReviewItem() {
    $.get('/PIMS/Maintenance/GetAddEditReviewItemPartial', function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function editReviewItem(btn) {
    var itemid = $(btn).data('id');
    $.get('/PIMS/Maintenance/GetAddEditReviewItemPartial', { itemid: itemid }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function deleteReviewItem(btn) {
    var itemid = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this review item?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/PIMS/Maintenance/DeleteReviewItem?itemid=' + encodeURIComponent(itemid),
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    showAlertMessage('Review item deleted successfully.', AlertType.SUCCESS)
                        .then(function () { reloadReviewItemsGrid(); });
                } else {
                    showAlertMessage(response.message || 'Delete failed.', AlertType.ERROR);
                }
            },
            error: function () { showAlertMessage('An error occurred while deleting the review item.', AlertType.ERROR); }
        });
    });
}

// ════════════════════════════════════════════════════════════════════════════
//  OTHER TAB — Risk Ratings
// ════════════════════════════════════════════════════════════════════════════

function saveRisk() {
    const $form = $('#formRisk');
    const $banner = $('#riskDbError');
    const $bannerText = $('#riskDbErrorText');
    const isEdit = ($form.data('is-edit') || '').toString().toLowerCase() === 'true';

    displayPimsOtherClientValidationErrors($form);

    if (!isPimsOtherFormValid($form)) return;

    $banner.hide();

    const formData = $form.serializeArray();
    formData.push({ name: 'isEdit', value: isEdit });

    $.post('/PIMS/Maintenance/SaveRisk', $.param(formData))
        .done(function (data) {
            if (data.success) {
                showAlertMessage('Risk rating saved successfully.', AlertType.SUCCESS)
                    .then(() => {
                        closeModal();
                        reloadRisksGrid();
                    });
                return;
            }

            if (data.errors) {
                $bannerText.text('');
                displayServerValidationErrors(data.errors, data.message, $form);
                return;
            }

            var msg = data.message || 'Save failed.';
            $bannerText.text(msg);
            $banner.show();
        })
        .fail(function () {
            $bannerText.text('An error occurred while saving.');
            $banner.show();
        });
}

function addRisk() {
    $.get('/PIMS/Maintenance/GetAddEditRiskPartial', function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function editRisk(btn) {
    var riskid = $(btn).data('id');
    $.get('/PIMS/Maintenance/GetAddEditRiskPartial', { riskid: riskid }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function deleteRisk(btn) {
    var riskid = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this risk rating?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/PIMS/Maintenance/DeleteRisk?riskid=' + encodeURIComponent(riskid),
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    showAlertMessage('Risk rating deleted successfully.', AlertType.SUCCESS)
                        .then(function () { reloadRisksGrid(); });
                } else {
                    showAlertMessage(response.message || 'Delete failed.', AlertType.ERROR);
                }
            },
            error: function () { showAlertMessage('An error occurred while deleting the risk rating.', AlertType.ERROR); }
        });
    });
}

// ════════════════════════════════════════════════════════════════════════════
//  OTHER TAB — Publication Types
// ════════════════════════════════════════════════════════════════════════════

function savePublicationType() {
    const $form = $('#formPublicationType');
    const isEdit = ($form.data('is-edit') || '').toString().toLowerCase() === 'true';

    displayPimsOtherClientValidationErrors($form);

    if (!isPimsOtherFormValid($form)) return;

    const formData = $form.serializeArray();
    formData.push({ name: 'isEdit', value: isEdit });

    $.post('/PIMS/Maintenance/SavePublicationType', $.param(formData))
        .done(function (data) {
            if (data.success) {
                showAlertMessage('Publication type saved successfully.', AlertType.SUCCESS)
                    .then(() => {
                        closeModal();
                        reloadPublicationTypesGrid();
                    });
                return;
            }

            if (data.errors) {
                displayServerValidationErrors(data.errors, data.message, $form);
            }
        })
        .fail(function () {
            showAlertMessage('An error occurred while saving.', AlertType.ERROR);
        });
}

function addPublicationType() {
    $.get('/PIMS/Maintenance/GetAddEditPublicationTypePartial', function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function editPublicationType(btn) {
    var type = $(btn).data('id');
    $.get('/PIMS/Maintenance/GetAddEditPublicationTypePartial', { type: type }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function deletePublicationType(btn) {
    var type = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this publication type?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/PIMS/Maintenance/DeletePublicationType?type=' + encodeURIComponent(type),
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    showAlertMessage('Publication type deleted successfully.', AlertType.SUCCESS)
                        .then(function () { reloadPublicationTypesGrid(); });
                } else {
                    const errorMessage = response.message
                        || (Array.isArray(response.errors) && response.errors.length > 0 && response.errors[0].message)
                        || 'Delete failed.';
                    showAlertMessage(errorMessage, AlertType.ERROR);
                }
            },
            error: function (xhr) {
                const response = xhr && xhr.responseJSON ? xhr.responseJSON : null;
                const errorMessage = (response && (response.message
                    || (Array.isArray(response.errors) && response.errors.length > 0 && response.errors[0].message)))
                    || xhr.responseText
                    || 'An error occurred while deleting the publication type.';
                showAlertMessage(errorMessage, AlertType.ERROR);
            }
        });
    });
}

// ── OTHER TAB — Report Groups master CRUD

function addOtherReportGroup() {
    $.get('/PIMS/Maintenance/GetAddEditOtherReportGroupPartial', function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function editOtherReportGroup(btn) {
    var groupid = $(btn).data('id');
    $.get('/PIMS/Maintenance/GetAddEditOtherReportGroupPartial', { groupid: groupid }, function (html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function saveOtherReportGroup() {
    const $form = $('#formOtherReportGroup');
    const $banner = $('#otherReportGroupDbError');
    const $bannerText = $('#otherReportGroupDbErrorText');
    const isEdit = ($form.data('is-edit') || '').toString().toLowerCase() === 'true';

    displayPimsOtherClientValidationErrors($form);

    if (!isPimsOtherFormValid($form)) return;

    $banner.hide();

    const formData = $form.serializeArray();
    formData.push({ name: 'isEdit', value: isEdit });

    $.post('/PIMS/Maintenance/SaveOtherReportGroup', $.param(formData))
        .done(function (data) {
            if (data.success) {
                showAlertMessage('Report group saved successfully.', AlertType.SUCCESS)
                    .then(function () {
                        closeModal();
                        reloadOtherValuesGrid();
                    });
                return;
            }

            if (data.errors) {
                $bannerText.text('');
                displayServerValidationErrors(data.errors, data.message, $form);
                return;
            }

            $bannerText.text(data.message || 'Save failed.');
            $banner.show();
        })
        .fail(function () {
            $bannerText.text('An error occurred while saving.');
            $banner.show();
        });
}

function deleteOtherReportGroup(btn) {
    var groupid = $(btn).data('id');
    showGovukConfirm('Are you sure you want to delete this report group?').then(function (result) {
        if (!result) return;
        $.ajax({
            url: '/PIMS/Maintenance/DeleteOtherReportGroup?groupid=' + encodeURIComponent(groupid),
            type: 'DELETE',
            success: function (response) {
                if (response.success) {
                    showAlertMessage('Report group deleted successfully.', AlertType.SUCCESS)
                        .then(function () { reloadOtherValuesGrid(); });
                } else {
                    showAlertMessage(response.message || 'Delete failed.', AlertType.ERROR);
                }
            },
            error: function () { showAlertMessage('An error occurred while deleting the report group.', AlertType.ERROR); }
        });
    });
}

// ════════════════════════════════════════════════════════════════════════════
//  TAB INITIALISATION
// ════════════════════════════════════════════════════════════════════════════

document.addEventListener('DOMContentLoaded', function () {
    initializeTimeTabState();
    autoSelectFirstGridRow('#gridContainer_reportsGrid');
    wireProgrammeGridHandlers();

    document.querySelectorAll('#mainTabs .govuk-tabs__tab').forEach(function (tab) {
        tab.addEventListener('click', function () {
            var href = tab.getAttribute('href') || '';
            var panel = href ? document.querySelector(href) : null;
            var tabKey = panel ? panel.getAttribute('data-tab-key') : '';
            if (tabKey && tabKey !== 'reports') {
                loadMaintenanceTab(tabKey);
            }
        });
    });
});
