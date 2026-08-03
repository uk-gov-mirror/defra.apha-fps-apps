// fec_testmanagement.js
// FEC Bulk Rates Update — Phase 4 MVC/UI layer.
// Covers US-UI-01 (create), US-UI-02/03 (upload + validation), US-UI-04 (release),
// US-UI-05 (approve), US-UI-06 (reject modal), US-UI-07 (detail), US-UI-08 (cancel).

/* jshint esversion: 6 */
/* global $, showLoader, hideLoader, showAlertMessage, showGovukConfirm, AlertType */

var BulkRates = (function () {
    'use strict';

    // ── Helpers ────────────────────────────────────────────────────────────

    function getAntiForgeryToken() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function showActionError(msg) {
        var banner = document.getElementById('actionErrorBanner');
        var text   = document.getElementById('actionErrorText');
        if (banner && text) {
            text.textContent = msg;
            banner.style.display = '';
            banner.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        } else {
            alert(msg);
        }
    }

    function hideActionError() {
        var banner = document.getElementById('actionErrorBanner');
        if (banner) { banner.style.display = 'none'; }
    }

    function ajaxPost(url, data, successCallback, errorCallback) {
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            headers: { 'RequestVerificationToken': getAntiForgeryToken() },
            success: function (result) {
                if (result && result.success) {
                    successCallback(result);
                } else {
                    var msg = (result && result.message) ? result.message : 'An unexpected error occurred.';
                    if (errorCallback) {
                        errorCallback(msg);
                    } else {
                        showActionError(msg);
                    }
                }
            },
            error: function (xhr) {
                var msg = 'An unexpected error occurred. Please try again.';
                if (xhr.responseJSON && xhr.responseJSON.message) { msg = xhr.responseJSON.message; }
                if (errorCallback) {
                    errorCallback(msg);
                } else {
                    showActionError(msg);
                }
            }
        });
    }

    // ── US-UI-01: Create Request ────────────────────────────────────────────

    function submitCreate() {
        var jobName = document.getElementById('jobName');
        var fpsYear = document.getElementById('fpsYear');
        var errorSummary = document.getElementById('createErrorSummary');
        var errorText    = document.getElementById('createErrorText');

        if (!jobName || !fpsYear) { return; }

        var yearVal = parseInt(fpsYear.value, 10);
        if (!yearVal || yearVal < 2000 || yearVal > 2100) {
            if (errorText)  { errorText.textContent  = 'Enter a valid FPS year (2000–2100).'; }
            if (errorSummary) { errorSummary.style.display = ''; }
            return;
        }
        if (errorSummary) { errorSummary.style.display = 'none'; }

        var btn = document.getElementById('btnCreateRequest');
        if (btn) { btn.disabled = true; }

        ajaxPost(
            '/FPS/BulkRates/Create',
            { jobName: jobName.value, fpsYear: yearVal },
            function (result) {
                window.fpsNavigateTo('/FPS/BulkRates/Detail/' + result.id);
            },
            function (msg) {
                if (errorText)    { errorText.textContent    = msg; }
                if (errorSummary) { errorSummary.style.display = ''; }
                if (btn) { btn.disabled = false; }
            }
        );
    }

    // ── Download Test Data (Excel) ───────────────────────────────────────────

    function downloadTestData() {
        var btn = document.getElementById('btnDownloadTestData');
        var jobName = btn ? btn.getAttribute('data-job-name') : null;
        var yearSelector = document.getElementById('yearSelector');
        var fpsYear = (yearSelector && yearSelector.value) ? parseInt(yearSelector.value, 10) : new Date().getFullYear();
        if (!fpsYear || fpsYear <= 0) {
            alert('Please select a year before downloading.');
            return;
        }
        var endpoint = jobName === 'BulkStaffRatesUpdate' ? '/FPS/BulkRates/DownloadStaffTestData'
            : jobName === 'BulkAnimalRatesUpdate' ? '/FPS/BulkRates/DownloadAnimalTestData'
            : '/FPS/BulkRates/DownloadTestData';
        window.location.href = endpoint + '?fpsYear=' + fpsYear;
    }

    // ── US-UI-02/03: Upload Excel file ──────────────────────────────────────
    function uploadFile(requestId) {
        var fileInput = document.getElementById('ratesFile');
        if (!fileInput || !fileInput.files || fileInput.files.length === 0) {
            showActionError('Please select a file before uploading.');
            return;
        }

        var file = fileInput.files[0];
        var formData = new FormData();
        formData.append('id', requestId);
        formData.append('file', file);

        var btn      = document.getElementById('btnUpload');
        var progress = document.getElementById('uploadProgress');
        if (btn)      { btn.disabled = true; }
        if (progress) { progress.style.display = ''; }

        $.ajax({
            url: '/FPS/BulkRates/Upload',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            headers: { 'RequestVerificationToken': getAntiForgeryToken() },
            success: function (result) {
                if (btn)      { btn.disabled = false; }
                if (progress) { progress.style.display = 'none'; }
                if (result && result.success) {
                    // Reload the detail page so server-rendered validation results are refreshed.
                    window.location.reload();
                } else {
                    showActionError((result && result.message) ? result.message : 'Upload failed.');
                }
            },
            error: function (xhr) {
                if (btn)      { btn.disabled = false; }
                if (progress) { progress.style.display = 'none'; }
                var msg = 'Upload failed. Please try again.';
                if (xhr.responseJSON && xhr.responseJSON.message) { msg = xhr.responseJSON.message; }
                showActionError(msg);
            }
        });
    }

    // ── Upload Updated Data (tracker button on Index) ──────────────────────

    var _trackerUploadRequestId = null;

    function showUploadTrackerModal() {
        var btn = document.getElementById('btnUploadUpdatedData');
        var requestId = btn ? btn.getAttribute('data-request-id') : null;

        if (!requestId) {
            showActionError('No open request is available to upload against. Create a new request first.');
            return;
        }

        _trackerUploadRequestId = requestId;

        var overlay  = document.getElementById('uploadTrackerModalOverlay');
        var fileEl   = document.getElementById('uploadTrackerFile');
        var errEl    = document.getElementById('uploadTrackerFileError');
        var group    = document.getElementById('uploadTrackerFileGroup');
        if (!overlay) { return; }
        if (fileEl) { fileEl.value = ''; }
        if (errEl)  { errEl.style.display = 'none'; }
        if (group)  { group.classList.remove('govuk-form-group--error'); }
        overlay.style.display = '';
    }

    function closeUploadTrackerModal() {
        var overlay = document.getElementById('uploadTrackerModalOverlay');
        if (overlay) { overlay.style.display = 'none'; }
        _trackerUploadRequestId = null;
    }

    function confirmTrackerUpload() {
        var fileEl = document.getElementById('uploadTrackerFile');
        var errEl  = document.getElementById('uploadTrackerFileError');
        var group  = document.getElementById('uploadTrackerFileGroup');

        if (!fileEl || !fileEl.files || fileEl.files.length === 0) {
            if (errEl)  { errEl.style.display = ''; }
            if (group)  { group.classList.add('govuk-form-group--error'); }
            return;
        }
        if (errEl) { errEl.style.display = 'none'; }
        if (group) { group.classList.remove('govuk-form-group--error'); }

        var formData = new FormData();
        formData.append('id', _trackerUploadRequestId);
        formData.append('file', fileEl.files[0]);

        var btn      = document.getElementById('btnConfirmTrackerUpload');
        var progress = document.getElementById('uploadTrackerProgress');
        if (btn)      { btn.disabled = true; }
        if (progress) { progress.style.display = ''; }

        $.ajax({
            url: '/FPS/BulkRates/Upload',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            headers: { 'RequestVerificationToken': getAntiForgeryToken() },
            success: function (result) {
                if (result && result.success) {
                    window.fpsNavigateTo('/FPS/BulkRates/Detail/' + _trackerUploadRequestId);
                } else {
                    if (btn)      { btn.disabled = false; }
                    if (progress) { progress.style.display = 'none'; }
                    showActionError((result && result.message) ? result.message : 'Upload failed.');
                }
            },
            error: function (xhr) {
                if (btn)      { btn.disabled = false; }
                if (progress) { progress.style.display = 'none'; }
                var msg = 'Upload failed. Please try again.';
                if (xhr.responseJSON && xhr.responseJSON.message) { msg = xhr.responseJSON.message; }
                showActionError(msg);
            }
        });
    }

    // ── FEC Data (Staging) — Download Staging Data ──────────────────────────

    function downloadStagingData(requestId) {
        window.location.href = '/FPS/BulkRates/DownloadStagingData/' + requestId;
    }

    // ── US-UI-05: Bulk Rates queue grid ──────────────────────────────────────
    // getBulkRatesExtraFilters / viewBulkRatesRequest are looked up by _DataGrid.cshtml's
    // JS via window[functionName] (ExtraFilterMethod / ViewFunction) — they must be true
    // globals, not members of the BulkRates module object like the rest of this file.

    function getBulkRatesExtraFilters() {
        var jobNameEl = document.getElementById('jobNameFilter');
        var statusEl  = document.getElementById('statusFilter');
        return {
            jobName: jobNameEl ? jobNameEl.value : '',
            status:  statusEl  ? statusEl.value  : ''
        };
    }
    window.getBulkRatesExtraFilters = getBulkRatesExtraFilters;

    function viewBulkRatesRequest(btn) {
        var id = $(btn).data('id');
        window.fpsNavigateTo('/FPS/BulkRates/Detail/' + id);
    }
    window.viewBulkRatesRequest = viewBulkRatesRequest;

    function filterGrid() {
        var gm = window['gridManager_bulkRatesGrid'];
        if (gm) { gm.reloadGrid({ page: 1 }); }
    }

    // ── US-UI-04: Release for Approval ─────────────────────────────────────

    function release(requestId) {
        showGovukConfirm('Release this request for approval? This action cannot be undone.').then(function (confirmed) {
            if (!confirmed) { return; }
            hideActionError();
            var btn = document.getElementById('btnRelease');
            if (btn) { btn.disabled = true; }

            ajaxPost(
                '/FPS/BulkRates/Release',
                { id: requestId },
                function () { window.location.reload(); },
                function (msg) {
                    showActionError(msg);
                    if (btn) { btn.disabled = false; }
                }
            );
        });
    }

    // ── US-UI-05: Approve ───────────────────────────────────────────────────

    function approve(requestId) {
        showGovukConfirm('Approve this request? The batch job will be triggered.').then(function (confirmed) {
            if (!confirmed) { return; }
            hideActionError();
            var btn = document.getElementById('btnApprove');
            if (btn) { btn.disabled = true; }
            showLoader();

            var returnUrl = btn ? (btn.getAttribute('data-return-url') || '/FPS/BulkRates') : '/FPS/BulkRates';
            ajaxPost(
                '/FPS/BulkRates/Approve',
                { id: requestId },
                function () { window.fpsNavigateTo(returnUrl); },
                function (msg) {
                    hideLoader();
                    showActionError(msg);
                    if (btn) { btn.disabled = false; }
                }
            );
        });
    }

    // ── US-UI-06: Reject modal ──────────────────────────────────────────────

    var _pendingRejectId = null;

    function showRejectModal(requestId) {
        _pendingRejectId = requestId;
        var overlay = document.getElementById('rejectModalOverlay');
        var reason  = document.getElementById('rejectReason');
        var errEl   = document.getElementById('rejectReasonError');
        var group   = document.getElementById('rejectReasonGroup');
        if (!overlay) { return; }
        if (reason)  { reason.value = ''; }
        if (errEl)   { errEl.style.display = 'none'; }
        if (group)   { group.classList.remove('govuk-form-group--error'); }
        overlay.style.display = '';
        if (reason)  { reason.focus(); }
    }

    function closeRejectModal() {
        var overlay = document.getElementById('rejectModalOverlay');
        if (overlay) { overlay.style.display = 'none'; }
        _pendingRejectId = null;
    }

    function confirmReject() {
        var reason  = document.getElementById('rejectReason');
        var errEl   = document.getElementById('rejectReasonError');
        var group   = document.getElementById('rejectReasonGroup');
        var reasonVal = reason ? reason.value.trim() : '';

        if (!reasonVal) {
            if (errEl)  { errEl.style.display = ''; }
            if (group)  { group.classList.add('govuk-form-group--error'); }
            if (reason) { reason.focus(); }
            return;
        }
        if (errEl)  { errEl.style.display = 'none'; }
        if (group)  { group.classList.remove('govuk-form-group--error'); }

        var btn = document.getElementById('btnConfirmReject');
        if (btn) { btn.disabled = true; }

        var returnBtn = document.getElementById('btnReject');
        var returnUrl = returnBtn ? (returnBtn.getAttribute('data-return-url') || '/FPS/BulkRates') : '/FPS/BulkRates';
        ajaxPost(
            '/FPS/BulkRates/Reject',
            { id: _pendingRejectId, reason: reasonVal },
            function () {
                closeRejectModal();
                window.fpsNavigateTo(returnUrl);
            },
            function (msg) {
                if (btn) { btn.disabled = false; }
                showActionError(msg);
            }
        );
    }

    // ── US-UI-08: Cancel modal ──────────────────────────────────────────────

    var _pendingCancelId = null;

    function showCancelModal(requestId) {
        _pendingCancelId = requestId;
        var overlay = document.getElementById('cancelModalOverlay');
        var reason  = document.getElementById('cancelReason');
        if (!overlay) { return; }
        if (reason)  { reason.value = ''; }
        overlay.style.display = '';
        if (reason)  { reason.focus(); }
    }

    function closeCancelModal() {
        var overlay = document.getElementById('cancelModalOverlay');
        if (overlay) { overlay.style.display = 'none'; }
        _pendingCancelId = null;
    }

    function confirmCancel() {
        var reason    = document.getElementById('cancelReason');
        var reasonVal = reason ? reason.value.trim() : '';

        var btn = document.getElementById('btnConfirmCancel');
        if (btn) { btn.disabled = true; }

        ajaxPost(
            '/FPS/BulkRates/Cancel',
            { id: _pendingCancelId, reason: reasonVal },
            function () { window.fpsNavigateTo('/FPS/BulkRates/Index'); },
            function (msg) {
                if (btn) { btn.disabled = false; }
                alert(msg);
            }
        );
    }

    // ── Keyboard: close modals on Escape ─────────────────────────────────────
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeRejectModal();
            closeCancelModal();
            closeUploadTrackerModal();
        }
    });

    // ── Button bindings ─────────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', function () {
        var btnDownload = document.getElementById('btnDownloadTestData');
        if (btnDownload) {
            btnDownload.addEventListener('click', downloadTestData);
        }
        var btnUploadTracker = document.getElementById('btnUploadUpdatedData');
        if (btnUploadTracker) {
            btnUploadTracker.addEventListener('click', showUploadTrackerModal);
        }
    });

    // ── Public API ──────────────────────────────────────────────────────────
    return {
        submitCreate:           submitCreate,
        uploadFile:             uploadFile,
        release:                release,
        approve:                approve,
        showRejectModal:        showRejectModal,
        closeRejectModal:       closeRejectModal,
        confirmReject:          confirmReject,
        showCancelModal:        showCancelModal,
        closeCancelModal:       closeCancelModal,
        confirmCancel:          confirmCancel,
        filterGrid:             filterGrid,
        downloadTestData:       downloadTestData,
        downloadStagingData:    downloadStagingData,
        showUploadTrackerModal: showUploadTrackerModal,
        closeUploadTrackerModal:closeUploadTrackerModal,
        confirmTrackerUpload:   confirmTrackerUpload
    };
}());

