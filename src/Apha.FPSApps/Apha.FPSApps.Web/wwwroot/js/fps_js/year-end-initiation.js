/*year-end-initiation.js */

(function ($) {
    'use strict';

    var cfg = window.YearEndInitiationConfig || {};

    function openModalWithHtml(html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    }

    // Called by Cancel/X buttons inside the loaded partials
    window.closeYeiModal = function () {
        $('#modalPopup').removeClass('show');
        $('#modaPopupBody').html('');
    };

    // ── Grid reload helpers ───────────────────────────────────────────────────

    function reloadConfigGrid() {
        var gm = window['gridManager_yearEndConfigValuesGrid'];
        if (gm && typeof gm.reloadGrid === 'function') { gm.reloadGrid({ page: 1 }); }
    }

    function reloadMonthGrid() {
        var gm = window['gridManager_yearEndMonthHoursGrid'];
        if (gm && typeof gm.reloadGrid === 'function') { gm.reloadGrid({ page: 1 }); }
    }

    function reloadHistoryGrid() {
        var gm = window['gridManager_yearEndInitiationHistoryGrid'];
        if (gm && typeof gm.reloadGrid === 'function') { gm.reloadGrid({ page: 1 }); }
    }

    // ── Generic helpers ───────────────────────────────────────────────────────

    function getAntiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function postJson(url, data, onSuccess, onError) {
        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: { 'RequestVerificationToken': getAntiForgeryToken() },
            success: function (result) {
                if (result.success) {
                    onSuccess(result);
                } else {
                    var msgs = (result.errors || []).map(function (e) { return e.message || e; });
                    onError(msgs.length ? msgs : ['Request failed.']);
                }
            },
            error: function () { onError(['A network error occurred. Please try again.']); }
        });
    }

    function getCellValue(row, propertyName) {
        var cell = row.querySelector('td[data-property="' + propertyName + '"]');
        if (!cell) return '';
        var span = cell.querySelector('span[name="' + propertyName + '"]');
        return span ? span.textContent.trim() : cell.textContent.trim();
    }

    function showPageError(messages) {
        var summary = document.querySelector('.govuk-error-summary[aria-labelledby="yei-error-summary-title"]');
        var list    = document.getElementById('yei-error-list');
        if (summary && list) {
            list.innerHTML = '';
            (messages || ['An unexpected error occurred.']).forEach(function (m) {
                var li = document.createElement('li');
                li.textContent = m;
                list.appendChild(li);
            });
            summary.style.display = '';
        }
    }

    function hidePageError() {
        var summary = document.querySelector('.govuk-error-summary[aria-labelledby="yei-error-summary-title"]');
        if (summary) summary.style.display = 'none';
    }

    function showModalError(messages) {
        var summary = document.querySelector('#modaPopupBody .govuk-error-summary');
        var list    = document.querySelector('#modaPopupBody .govuk-error-summary__list');
        if (summary && list) {
            list.innerHTML = '';
            (messages || ['An unexpected error occurred.']).forEach(function (m) {
                var li = document.createElement('li');
                li.textContent = m;
                list.appendChild(li);
            });
            summary.style.display = '';
        }
    }

    // ── Config Value grid — Edit button ───────────────────────────────────────
    window.openConfigEditModal = function (btn) {
        hidePageError();

        var row         = $(btn).closest('tr')[0];
        var id          = getCellValue(row, 'Id');
        var fpsYearType = getCellValue(row, 'ExistsForPlannedYear');

        $.ajax({
            url: cfg.editConfigValueUrl,
            type: 'GET',
            data: { id: id },
            success: function (html) {
                openModalWithHtml(html);
            },
            error: function () {
                showAlertMessage('Failed to load the edit form.', AlertType.ERROR);
            }
        });
    };

    // ── Config Value grid — Delete/Confirm button ─────────────────────────────
    window.confirmConfigValue = function (btn) {
        hidePageError();

        var row         = $(btn).closest('tr')[0];
        var id          = getCellValue(row, 'Id');
        var value       = getCellValue(row, 'Value') || '';
        var fpsYearType = getCellValue(row, 'ExistsForPlannedYear');
        var fpsyear = getCellValue(row, 'FpsYear');
 
        showGovukConfirm('Are you sure you want to confirm the config value for "' + id + '"?')
            .then(function (confirmed) {
                if (!confirmed) return;
                postJson(cfg.saveSettingUrl, { id: id, setting: value, fpsyear: fpsyear },
                    function () {
                        showAlertMessage('Config value "' + id + '" confirmed successfully.', AlertType.SUCCESS);
                        reloadConfigGrid();
                    },
                    function (msgs) { showPageError(msgs); }
                );
            });
    };

    // Called by the Save button inside _EditConfigValue.cshtml partial
    window.saveConfigValue = function () {
        var id    = $('#modaPopupBody #configModalId').val();
        var fpsYear = $('#modaPopupBody #configModalFpsYear').val();

        // Server renders either a <select id="configModalSelect"> or <input id="configModalInput">
        var $select = $('#modaPopupBody #configModalSelect');
        var $input  = $('#modaPopupBody #configModalInput');
        var value   = $select.length ? $select.val() : $input.val();

        postJson(cfg.saveSettingUrl, { id: id, setting: value, fpsYear: fpsYear },
            function () {
                window.closeYeiModal();
                showAlertMessage('Config value "' + value + '" saved successfully.', AlertType.SUCCESS);
                reloadConfigGrid();
            },
            function (msgs) { showModalError(msgs); }
        );
    };

    // ── Month Working Hours grid — Edit button ────────────────────────────────
    // Loads _EditMonthHour partial for the row's Year+Month via $.get → shows in #modalPopup
    window.openMonthHourEditModal = function (btn) {
        hidePageError();

        var row         = $(btn).closest('tr')[0];
        var fpsYearType = getCellValue(row, 'ExistsForPlannedYear');
        var year        = getCellValue(row, 'Year');    
        var month       = getCellValue(row, 'Month');
        var fpsyear = getCellValue(row, 'FpsYear');
        var fmonth = getCellValue(row, 'Fmonth');

        $.ajax({
            url: cfg.editMonthHourUrl,
            type: 'GET',
            data: { year: year, month: month, fpsyear: fpsyear, fmonth: fmonth },
            success: function (html) {
                openModalWithHtml(html);
            },
            error: function () {
                showAlertMessage('Failed to load the edit form.', AlertType.ERROR);
            }
        });
    };

    // ── Month Working Hours grid — Delete/Confirm button ──────────────────────
    window.confirmMonthHour = function (btn) {
        hidePageError();

        var row         = $(btn).closest('tr')[0];
        var fpsYearType = getCellValue(row, 'ExistsForPlannedYear');
        var monthName   = getCellValue(row, 'MonthName');

        showGovukConfirm('Are you sure you want to confirm the working hours for ' + monthName + '?')
            .then(function (confirmed) {
                if (!confirmed) return;
                var dto = {
                    year:     parseInt(getCellValue(row, 'Year'), 10),
                    month:    parseInt(getCellValue(row, 'Month'), 10),
                    days:     parseFloat(getCellValue(row, 'Days'))    || null,
                    cvlHours: parseFloat(getCellValue(row, 'CvlHours')) || null,
                    vidHours: parseFloat(getCellValue(row, 'VidHours')) || null,
                    fmonth:   parseInt(getCellValue(row, 'Fmonth'), 10) || null,
                    fpsYear:  parseInt(getCellValue(row, 'FpsYear'), 10)
                };
                postJson(cfg.saveMonthHourUrl, dto,
                    function () {
                        showAlertMessage('Working hours for ' + monthName + ' saved successfully.', AlertType.SUCCESS);
                        reloadMonthGrid();
                    },
                    function (msgs) { showPageError(msgs); }
                );
            });
    };

    // Called by the Save button inside _EditMonthHour.cshtml partial
    window.saveMonthHour = function () {
        var dto = {
            year:     parseInt($('#modaPopupBody #monthModalYear').val(),    10),
            month:    parseInt($('#modaPopupBody #monthModalMonth').val(),   10),
            days:     parseFloat($('#modaPopupBody #monthModalDays').val())     || null,
            cvlHours: parseFloat($('#modaPopupBody #monthModalCvlHours').val()) || null,
            vidHours: parseFloat($('#modaPopupBody #monthModalVidHours').val()) || null,
            fmonth:   parseInt($('#modaPopupBody #monthModalFmonth').val(),  10) || null,
            fpsYear:  parseInt($('#modaPopupBody #monthModalFpsYear').val(), 10)
        };

        postJson(cfg.saveMonthHourUrl, dto,
            function () {
                window.closeYeiModal();
                showAlertMessage('Month working hours saved successfully.', AlertType.SUCCESS);
                reloadMonthGrid();
            },
            function (msgs) { showModalError(msgs); }
        );
    };

    // ── Row button state enforcement ──────────────────────────────────────────
    function applyConfigGridButtonStates() {
        $('#tbl_yearEndConfigValuesGrid tbody tr').each(function () {
            var fpsYearType = getCellValue(this, 'ExistsForPlannedYear').toLowerCase();
            var isPlanned = (fpsYearType === 'yes');

            $(this).find('.edit-row-btn').prop('disabled', !isPlanned);
            $(this).find('.delete-row-btn').prop('disabled', isPlanned);
        });
    }

    function applyMonthGridButtonStates() {
        $('#tbl_yearEndMonthHoursGrid tbody tr').each(function () {
            var fpsYearType = getCellValue(this, 'ExistsForPlannedYear').toLowerCase();
            var fmonth      = getCellValue(this, 'Fmonth').trim();
            var fmonthZero  = (fmonth === '0' || fmonth === '');

            if (fmonthZero) {
                $(this).find('.edit-row-btn').prop('disabled', true);
                $(this).find('.delete-row-btn').prop('disabled', true);
            } else {
                var Planned = (fpsYearType == 'yes');
                $(this).find('.edit-row-btn').prop('disabled', !Planned);
                $(this).find('.delete-row-btn').prop('disabled', Planned);
            }
        });
    }

    // Applies button states immediately and re-applies whenever the grid container
    // DOM changes (i.e. after every grid reload).
    function observeGridForButtonStates(containerId, applyFn) {
        var container = document.getElementById(containerId);
        if (!container) { return; }
        applyFn();
        new MutationObserver(function () { applyFn(); })
            .observe(container, { childList: true, subtree: true });
    }

    // ── Initiate DataSetup Request button ─────────────────────────────────────
    $(function () {
        observeGridForButtonStates('gridContainer_yearEndConfigValuesGrid', applyConfigGridButtonStates);
        observeGridForButtonStates('gridContainer_yearEndMonthHoursGrid',   applyMonthGridButtonStates);

        var btnInitiate = document.getElementById('btnInitiateDataSetupRequest');
        if (btnInitiate) {
            btnInitiate.addEventListener('click', function () {
                hidePageError();

                var plannedYearVal = parseInt(document.getElementById('yearEndProcessYear').value, 10);

                if (!plannedYearVal || isNaN(plannedYearVal)) {
                    displayServerValidationErrors(
                        [{ field: 'plannedYear', message: 'Please provide planned year.' }],
                        'There is a problem',
                        SCOPE
                    );
                    return;
                }

                showGovukConfirm('Are you sure you want to initiate the DataSetup Request for year ' + plannedYearVal + '?')
                    .then(function (confirmed) {
                        if (!confirmed) return;

                        btnInitiate.disabled = true;
                        postJson(cfg.triggerInitiateUrl + '?plannedYear=' + plannedYearVal, {},
                            function () {
                                showAlertMessage('Year End Initiation request submitted successfully.', AlertType.SUCCESS);
                                reloadHistoryGrid();
                                var btnapprove = document.getElementById('btnApproveDataSetupRequest');
                                btnapprove.disabled = false;
                            },
                            function (msgs) {
                                showPageError(msgs);
                                btnInitiate.disabled = false;
                            }
                        );
                    });
            });
        }

        // ── Approve DataSetup Request button ──────────────────────────────────
        var btnApprove = document.getElementById('btnApproveDataSetupRequest');
        if (btnApprove) {
            btnApprove.addEventListener('click', function () {
                hidePageError();

                var plannedYearVal = parseInt(document.getElementById('yearEndProcessYear').value, 10);

                if (!plannedYearVal || isNaN(plannedYearVal)) {
                    displayServerValidationErrors(
                        [{ field: 'plannedYear', message: 'Please provide planned year.' }],
                        'There is a problem',
                        SCOPE
                    );
                    return;
                }

                showGovukApproveReject('Are you sure you want to approve the DataSetup Request for year ' + plannedYearVal + '?')
                    .then(function (confirmed) {
                        if (confirmed) {
                            btnApprove.disabled = true;
                            postJson(cfg.triggerApproveUrl + '?plannedYear=' + plannedYearVal, {},
                                function () {
                                    showAlertMessage('Year End Approval request submitted successfully.', AlertType.SUCCESS);
                                    reloadHistoryGrid();
                                },
                                function (msgs) {
                                    showPageError(msgs);
                                    btnApprove.disabled = false;
                                }
                            );
                        } else {
                            btnApprove.disabled = true;
                            postJson(cfg.triggerRejectUrl + '?plannedYear=' + plannedYearVal, {},
                                function () {
                                    showAlertMessage('Year End datasetup request rejected successfully.', AlertType.SUCCESS);
                                    reloadHistoryGrid();
                                    btnInitiate.disabled = false;
                                },
                                function (msgs) {
                                    showPageError(msgs);
                                    btnApprove.disabled = false;
                                }
                            );
                        }
                    });
            });
        }
    });

}(jQuery));
