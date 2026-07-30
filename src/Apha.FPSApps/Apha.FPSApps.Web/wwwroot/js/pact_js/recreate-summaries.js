/**
 * recreate-summaries.js
 *
 * Handles all client-side behaviour for the Recreate Summaries page:
 *   - Period dropdown initialisation (MultiColumnDropdownComponent)
 *   - Client-side validation using ajax-form-validation.js helpers
 *   - AJAX job trigger (POST /PACT/RecreateSummary/TriggerJob)
 *   - AJAX history-grid refresh (POST /PACT/RecreateSummary/LoadHistoryGrid)
 *   - Success / error banner display
 *
 * Depends on (must be loaded before this file):
 *   - jquery
 *   - multicolumn-dropdown.component.js
 *   - ajax-form-validation.js  (clearValidationErrors, displayServerValidationErrors)
 *
 * Configuration bridge (set inline by the Razor view before this script loads):
 *   window.RecreateSummariesConfig = { periodData: [...] }
 */

(function () {
    'use strict';

    // ── Constants ─────────────────────────────────────────────────────────────

    var SCOPE        = '#recreate-form-scope';
    var TRIGGER_URL  = '/PACT/RecreateSummary/TriggerJob';
    var GRID_URL     = '/PACT/RecreateSummary/LoadHistoryGrid';

    // ── Initialise period dropdown ────────────────────────────────────────────

    document.addEventListener('DOMContentLoaded', function () {

        var config     = window.RecreateSummariesConfig || {};
        var periodData = config.periodData || [];

        new MultiColumnDropdownComponent({
            dropdownId            : 'selectPeriodDropdown',
            containerSelector     : '#select-period-container',
            placeholder           : '--select--',
            searchPlaceholder     : 'Type to search and select period',
            showSerialNumber      : false,
            columns: [
                { field: 'period',    header: 'Period',     width: '80px'  },
                { field: 'monthName', header: 'Month Name', width: '160px' }
            ],
            data                  : periodData,
            displayField          : 'period',
            valueField            : 'value',
            clearButtonClearsSelection: true,
            callbacks: {
                onSelect: function (selectedItem) {
                    // Sync selected value to the named hidden input so the
                    // validation library can locate the field by name="month"
                    document.getElementById('month').value = selectedItem.value || '';

                    // Update the visible text input to show "period - monthName"
                    var input = document.getElementById('selectPeriodDropdown_input');
                    if (input) {
                        input.value = selectedItem.period + ' - ' + selectedItem.monthName;
                    }

                    // Clear any existing validation errors when the user selects a period
                    clearValidationErrors(SCOPE);
                    hideSuccessBanner();
                },
                onClear: function () {
                    document.getElementById('month').value = '';
                }
            }
        });
    });

    // ── Trigger job ───────────────────────────────────────────────────────────

    window.triggerCreateSummary = function () {
        clearValidationErrors(SCOPE);
        hideSuccessBanner();

        var monthVal = parseInt(document.getElementById('month').value, 10);

        if (!monthVal || isNaN(monthVal)) {
            // Field found by name="month" → validation library shows error
            // inline below the dropdown via [data-valmsg-for="month"]
            displayServerValidationErrors(
                [{ field: 'month', message: 'Please select a summary period.' }],
                'There is a problem',
                SCOPE
            );
            return;
        }

        var btn = document.getElementById('btnCreateSummary');
        btn.disabled = true;
        btn.setAttribute('aria-disabled', 'true');

        fetch(TRIGGER_URL, {
            method  : 'POST',
            headers : { 'Content-Type': 'application/x-www-form-urlencoded' },
            body    : new URLSearchParams({
                month                      : monthVal,
                __RequestVerificationToken : getAntiForgeryToken()
            })
        })
        .then(function (r) { return r.json(); })
        .then(function (data) {
            if (data.success) {
                // Show green success banner at the same position as the error summary
                showSuccessBanner('Job triggered successfully.');
                refreshHistoryGrid();
            } else {
                // Map ALL server errors — each gets a summary entry (field is empty
                // so the library routes every item to .govuk-error-summary)
                var errors = Array.isArray(data.errors) && data.errors.length
                    ? data.errors
                    : [{ field: '', message: data.message || 'Failed to trigger the job.' }];
                displayServerValidationErrors(errors, 'There is a problem', SCOPE);
                btn.disabled = false;
                btn.removeAttribute('aria-disabled');
            }
        })
        .catch(function () {
            displayServerValidationErrors(
                [{ field: '', message: 'An unexpected error occurred. Please try again.' }],
                'There is a problem',
                SCOPE
            );
            btn.disabled = false;
            btn.removeAttribute('aria-disabled');
        });
    };

    // ── Refresh history grid ──────────────────────────────────────────────────

    function refreshHistoryGrid() {
        var container = document.getElementById('gridContainer_summaryHistoryGrid');
        if (!container) return;

        fetch(GRID_URL, {
            method  : 'POST',
            headers : { 'Content-Type': 'application/x-www-form-urlencoded' },
            body    : new URLSearchParams({
                Filter                     : '{}',
                Page                       : 1,
                PageSize                   : 10,
                __RequestVerificationToken : getAntiForgeryToken()
            })
        })
        .then(function (r) { return r.text(); })
        .then(function (html) { container.innerHTML = html; });
    }

    // ── Success banner helpers ────────────────────────────────────────────────

    function showSuccessBanner(message) {
        var banner = document.getElementById('recreate-success-banner');
        if (!banner) return;
        document.getElementById('recreate-success-banner-message').textContent = message;
        banner.style.display = '';
        banner.focus();
    }

    function hideSuccessBanner() {
        var banner = document.getElementById('recreate-success-banner');
        if (banner) banner.style.display = 'none';
    }

    // ── Anti-forgery token helper ─────────────────────────────────────────────

    function getAntiForgeryToken() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

}());
