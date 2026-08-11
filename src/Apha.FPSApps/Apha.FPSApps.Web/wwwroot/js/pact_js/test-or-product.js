/**
 * test-or-product.js
 *
 * Client-side logic for the Test/Product Maintenance page.
 * Depends on: jQuery, Bootstrap modal, ajax-form-validation.js
 *
 * The view must supply endpoint URLs via data attributes on a
 * configuration element:
 *
 *   <div id="testOrProductConfig"
 *        data-url-get-owners="..."
 *        data-url-get="..."
 *        data-url-create="..."
 *        data-url-update="..."
 *        data-url-delete="..."></div>
 */
(function ($) {
    'use strict';

    var owners = [];
    var urls = {};

    // ── URL helpers ──────────────────────────────────────────────────────────

    function loadUrls() {
        var $cfg = $('#testOrProductConfig');
        urls = {
            getOwners: $cfg.data('url-get-owners'),
            get:       $cfg.data('url-get'),
            create:    $cfg.data('url-create'),
            update:    $cfg.data('url-update'),
            delete:    $cfg.data('url-delete')
        };
    }

    // ── Owner dropdown ───────────────────────────────────────────────────────

    function loadOwners() {
        $.ajax({
            url: urls.getOwners,
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    owners = response.data;
                    populateOwnerDropdown();
                }
            },
            error: function () {
                // Error loading owners
            }
        });
    }

    function populateOwnerDropdown() {
        var ownerSelect = $('#owner');
        ownerSelect.empty();
        ownerSelect.append('<option value="">Select Owner</option>');

        owners.forEach(function (owner) {
            ownerSelect.append($('<option></option>').val(owner).text(owner));
        });
    }

    // ── Required-field helpers ───────────────────────────────────────────────

    function setRequiredFields() {
        $('#itemCode').prop('required', true);
        $('#owner').prop('required', true);
        $('#defraUnitPrice').prop('required', true);
    }

    // ── Add / Edit / Save / Delete ───────────────────────────────────────────

    function addTestOrProduct() {
        $('#testModalLabel').text('Add Test or Product');
        $('#isEdit').val('false');
        $('#originalItemCode').val('');
        $('#testForm')[0].reset();
        $('#itemCode').prop('disabled', false);

        setRequiredFields();
        populateOwnerDropdown();
        clearValidationErrors('#testModal');
        $('#testModal').modal('show');

        // Initialize form validation after modal is shown
        setTimeout(function() {
            initializeFormValidation('#testForm');
        }, 100);
    }

    function editTestOrProduct(btn) {
        var itemCode = $(btn).data('id');

        if (!itemCode) {
            showAlertMessage('Item code not found', AlertType.ERROR);
            return;
        }

        $('#testForm')[0].reset();
        clearValidationErrors('#testModal');

        // Temporarily disable required validation during data load
        $('#testForm [required]').prop('required', false);

        $.ajax({
            url: urls.get,
            type: 'GET',
            data: { itemCode: itemCode },
            success: function (response) {
                if (response.success) {
                    var data = response.data;

                    $('#testModalLabel').text('Edit Test or Product');
                    $('#isEdit').val('true');
                    $('#originalItemCode').val(itemCode);

                    // Populate form fields — support both PascalCase and camelCase
                    $('#itemCode').val(data.ItemCode || data.itemCode || '').prop('disabled', true);
                    $('#shortDescription').val(data.ShortDescription || data.shortDescription || '');
                    $('#itemDescription').val(data.ItemDescription || data.itemDescription || '');
                    $('#testManager').val(data.TestManager || data.testManager || '');
                    $('#owner').val(data.Owner || data.owner || '');
                    $('#jobStatus').val(data.JobStatus || data.jobStatus || '');
                    $('#chargeMethod').val(data.ChargeMethod || data.chargeMethod || '');

                    var defraPrice = data.DefraUnitPrice !== undefined ? data.DefraUnitPrice : data.defraUnitPrice;
                    if (defraPrice !== null && defraPrice !== undefined) {
                        $('#defraUnitPrice').val(defraPrice);
                    }

                    var unitPriceVla = data.UnitPriceVla !== undefined ? data.UnitPriceVla : data.unitPriceVla;
                    if (unitPriceVla !== null && unitPriceVla !== undefined) {
                        $('#unitPriceVla').val(unitPriceVla);
                    }

                    var priceAhvg = data.PriceAhvg !== undefined ? data.PriceAhvg : data.priceAhvg;
                    if (priceAhvg !== null && priceAhvg !== undefined) {
                        $('#priceAhvg').val(priceAhvg);
                    }

                    setRequiredFields();
                    clearValidationErrors('#testModal');

                    setTimeout(function () {
                        clearValidationErrors('#testModal');
                        $('#testModal .govuk-error-summary').hide();
                        $('#testModal .govuk-form-group').removeClass('govuk-form-group--error');
                        $('#testModal .govuk-input').removeClass('govuk-input--error');
                        $('#testModal').modal('show');

                        // Initialize form validation (unobtrusive + numeric)
                        initializeFormValidation('#testForm');
                    }, 50);
                } else {
                    setRequiredFields();
                    showAlertMessage('Error: '+ response.message, AlertType.ERROR);
                }
            },
            error: function () {
                setRequiredFields();
                showAlertMessage('Error: ' + 'Failed to load test/product details', AlertType.ERROR);

            }
        });
    }

    function closeTestModal() {
        $('#testForm')[0].reset();
        clearValidationErrors('#testModal');
        $('#isEdit').val('false');
        $('#originalItemCode').val('');
        $('#itemCode').prop('readonly', false);
        $('#testModal').modal('hide');
        $('.modal-backdrop').remove();
        $('body').removeClass('modal-open');
        $('body').css({ 'padding-right': '', 'overflow': '' });
    }

    function saveTestOrProduct() {
        var form = $('#testForm');

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
            displayClientValidationErrors(form, '#testModal');
            return;
        }

        if (!isFormValid(form)) {
            // Display validation errors without clearing them first
            displayClientValidationErrors(form, '#testModal');
            return;
        }

        // Clear validation errors only after validation passes
        clearValidationErrors('#testModal');

        var isEdit = $('#isEdit').val() === 'true';
        var itemCode = $('#originalItemCode').val() || $('#itemCode').val();

        var formData = {
            ItemCode: $('#itemCode').val(),
            ShortDescription: $('#shortDescription').val(),
            ItemDescription: $('#itemDescription').val(),
            TestManager: $('#testManager').val(),
            Owner: $('#owner').val(),
            JobStatus: $('#jobStatus').val(),
            ChargeMethod: $('#chargeMethod').val(),
            DefraUnitPrice: parseDefraUnitPrice(),
            UnitPriceVla: parseOptionalDecimal($('#unitPriceVla').val()),
            PriceAhvg: parseOptionalDecimal($('#priceAhvg').val())
        };

        var url = isEdit
            ? urls.update + '?itemCode=' + encodeURIComponent(itemCode)
            : urls.create;

        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.success) {
                    closeTestModal();
                    reloadGrid('testGrid');
                    showAlertMessage(response.message, AlertType.SUCCESS);
                } else {
                    if (response.errors) {
                        displayServerValidationErrors(response.errors, response.message, '#testModal');
                    } else {
                        showAlertMessage('Error: ' + response.message, AlertType.ERROR);
                    }
                }
            },
            error: function (xhr) {
                if (xhr.status === 400 && xhr.responseJSON) {
                    displayServerValidationErrors(xhr.responseJSON.errors, xhr.responseJSON.message || 'There is a problem', '#testModal');
                } else {
                    var message = xhr.responseJSON ? xhr.responseJSON.message : 'An error occurred while saving';
                    showAlertMessage('Error: ' + message || 'An error occurred while saving', AlertType.ERROR);
                }
            }
        });
    }

    function deleteTestOrProduct(btn) {
        var itemCode = $(btn).data('id');

        if (!itemCode) {
            showAlertMessage('Error: ' + 'Item code not found', AlertType.ERROR);
            return;
        }
        showGovukConfirm('Are you sure you want to delete test/product ' + itemCode + '?').then(function (confirmed) {
            if (!confirmed) return;

            $.ajax({
                url: urls.delete,
                type: 'POST',
                data: { itemCode: itemCode },
                success: function (response) {
                    if (response.success) {
                        reloadGrid('testGrid');
                        showAlertMessage(response.message, AlertType.SUCCESS);
                    } else {
                        showAlertMessage('Error: ' + response.message, AlertType.ERROR);
                    }
                },
                error: function (xhr) {
                    var message = xhr.responseJSON ? xhr.responseJSON.message : 'An error occurred while deleting';
                    showAlertMessage('Error: ' + message || 'An error occurred while deleting', AlertType.ERROR);
                }
            });
        });
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    function parseOptionalDecimal(value) {
        if (value === '' || value === null || value === undefined) {
            return null;
        }
        var parsed = parseFloat(value);
        return isNaN(parsed) ? null : parsed;
    }

    function parseDefraUnitPrice() {
        var value = $('#defraUnitPrice').val();
        if (value === '' || value === null || value === undefined) {
            return 0;
        }
        var parsed = parseFloat(value);
        return isNaN(parsed) ? 0 : parsed;
    }

    function reloadGrid(gridId) {
        var gridManager = window['gridManager_' + gridId];
        if (gridManager && typeof gridManager.reloadGrid === 'function') {
            gridManager.reloadGrid({ page: 1 });
        } else if (typeof bindGrid === 'function') {
            bindGrid(gridId, 1);
        } else {
            location.reload();
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    window.addTestOrProduct = addTestOrProduct;
    window.editTestOrProduct = editTestOrProduct;
    window.closeTestModal = closeTestModal;
    window.saveTestOrProduct = saveTestOrProduct;
    window.deleteTestOrProduct = deleteTestOrProduct;
    window.reloadGrid = reloadGrid;

    // ── Initialisation ────────────────────────────────────────────────────────

    $(document).ready(function () {
        loadUrls();
        loadOwners();

        // Initialize numeric validation after a short delay to ensure DOM is ready
        setTimeout(function() {
            if (typeof initializeNumericInputValidation === 'function') {
                initializeNumericInputValidation();
            }
        }, 100);

        // Disable jQuery Unobtrusive Validation for this form
        if ($.validator && $.validator.unobtrusive) {
            var form = $('#testForm');
            if (form.data('validator')) {
                form.removeData('validator');
            }
            if (form.data('unobtrusiveValidation')) {
                form.removeData('unobtrusiveValidation');
            }
        }

        $('#testForm').on('submit', function (e) {
            e.preventDefault();
            saveTestOrProduct();
        });

        $('#testModal').on('show.bs.modal', function () {
            clearValidationErrors('#testModal');
        });

        $('#testModal').on('shown.bs.modal', function () {
            clearValidationErrors('#testModal');
            $('#testModal .govuk-error-summary').hide().css('display', 'none');
            $('#testModal .govuk-error-summary__list').empty();
            $('#testModal .govuk-form-group').removeClass('govuk-form-group--error');
            $('#testModal .govuk-input').removeClass('govuk-input--error');
            $('#testModal .govuk-textarea').removeClass('govuk-textarea--error');
            $('#testModal .govuk-error-message').hide().css('display', 'none');

            // Re-initialize numeric validation when modal is shown
            if (typeof initializeNumericInputValidation === 'function') {
                initializeNumericInputValidation();
            }
        });

        $('#testModal').on('hidden.bs.modal', function () {
            closeTestModal();
        });

        $(document).on('click', '.govuk-button--secondary[data-dismiss="modal"]', function () {
            closeTestModal();
        });
    });

}(jQuery));
