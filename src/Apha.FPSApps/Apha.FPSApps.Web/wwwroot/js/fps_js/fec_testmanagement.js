// fec_testmanagement.js
// FEC Bulk Rates Update — Phase 4 MVC/UI layer.
// Covers US-UI-01 (create), US-UI-02/03 (upload + validation), US-UI-04 (release),
// US-UI-05 (approve), US-UI-06 (reject modal), US-UI-07 (detail), US-UI-08 (cancel).

/* jshint esversion: 6 */
/* global $, showLoader, hideLoader, showAlertMessage, AlertType */

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
                window.location.href = '/FPS/BulkRates/Detail/' + result.id;
            },
            function (msg) {
                if (errorText)    { errorText.textContent    = msg; }
                if (errorSummary) { errorSummary.style.display = ''; }
                if (btn) { btn.disabled = false; }
            }
        );
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

    // ── US-UI-04: Release for Approval ─────────────────────────────────────

    function release(requestId) {
        if (!confirm('Release this request for approval? This action cannot be undone.')) { return; }
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
    }

    // ── US-UI-05: Approve ───────────────────────────────────────────────────

    function approve(requestId) {
        if (!confirm('Approve this request? The batch job will be triggered.')) { return; }
        hideActionError();
        var btn = document.getElementById('btnApprove');
        if (btn) { btn.disabled = true; }

        ajaxPost(
            '/FPS/BulkRates/Approve',
            { id: requestId },
            function () { window.location.reload(); },
            function (msg) {
                showActionError(msg);
                if (btn) { btn.disabled = false; }
            }
        );
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

        ajaxPost(
            '/FPS/BulkRates/Reject',
            { id: _pendingRejectId, reason: reasonVal },
            function () {
                closeRejectModal();
                window.location.reload();
            },
            function (msg) {
                if (btn) { btn.disabled = false; }
                alert(msg);
            }
        );
    }

    // ── US-UI-08: Cancel ────────────────────────────────────────────────────

    function cancel(requestId) {
        var reason = prompt('Optional: enter a cancellation reason (or leave blank).');
        if (reason === null) { return; } // user pressed Cancel on the prompt
        hideActionError();
        var btn = document.getElementById('btnCancel');
        if (btn) { btn.disabled = true; }

        ajaxPost(
            '/FPS/BulkRates/Cancel',
            { id: requestId, reason: reason || '' },
            function () { window.location.href = '/FPS/BulkRates/Index'; },
            function (msg) {
                showActionError(msg);
                if (btn) { btn.disabled = false; }
            }
        );
    }

    // ── Keyboard: close reject modal on Escape ──────────────────────────────
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') { closeRejectModal(); }
    });

    // ── Public API ──────────────────────────────────────────────────────────
    return {
        submitCreate:     submitCreate,
        uploadFile:       uploadFile,
        release:          release,
        approve:          approve,
        showRejectModal:  showRejectModal,
        closeRejectModal: closeRejectModal,
        confirmReject:    confirmReject,
        cancel:           cancel
    };
}());

