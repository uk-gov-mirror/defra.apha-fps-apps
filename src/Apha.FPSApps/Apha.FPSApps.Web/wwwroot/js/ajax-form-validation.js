/**
 * ajax-form-validation.js
 *
 * Generic helpers for showing and clearing client-side and server-side
 * validation errors inside AJAX-driven forms (modal or inline).
 *
 * All functions accept an optional `container` argument (a CSS selector string
 * or jQuery object) that scopes every DOM query.  When omitted the scope
 * defaults to the whole document.
 *
 * Supported error formats for displayServerValidationErrors:
 *   - Array  : [{ field: 'FieldName', message: 'Error text' }, ...]
 *   - Object : { 'FieldName': 'Error text', ... }  (plain-object / dictionary)
 *
 * Public API
 * ----------
 *   isFormValid(form)
 *   clearValidationErrors([container])
 *   displayClientValidationErrors(form [, container])
 *   displayServerValidationErrors(errors, summaryMessage [, container])
 */
(function ($) {
    'use strict';

    // ── maxlength suppression for fields that also carry [Range] validation ──
    //
    // jQuery Validate reads the `maxlength` HTML attribute as a live validation
    // rule at runtime (via attributeRules). When a field also has `data-val-range`
    // (emitted by the ASP.NET [Range] data annotation), the maxlength rule fires
    // first and shows a generic "no more than N characters" message, hiding the
    // meaningful Range error message from the model.
    //
    // This patch wraps $.validator.methods.maxlength once, at script-load time,
    // so it applies globally to every form on every page. For any field that has
    // both a maxlength attribute AND a data-val-range attribute the maxlength
    // check is skipped (returns true), allowing [Range] to be the sole validator.
    // All other fields continue to use the original maxlength behaviour.
    if (typeof $.validator !== 'undefined') {
        var _origMaxlength = $.validator.methods.maxlength;
        $.validator.methods.maxlength = function (value, element, param) {
            if (element.maxLength > 0 && $(element).data('val-range') !== undefined) {
                return true; // defer to [Range] — suppress the maxlength message
            }
            return _origMaxlength.call(this, value, element, param);
        };
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    /** Resolve the optional container argument into a jQuery object. */
    function resolveContainer(container) {
        if (container == null) return $(document);
        return container instanceof $ ? container : $(container);
    }

    /**
     * Wires up a one-shot input/change listener on `$field` that clears its
     * inline error styling as soon as the user starts editing.
     *
     * @param {jQuery} $field     - The input/select element.
     * @param {string} fieldName  - The field's `name` attribute value.
     * @param {jQuery} $c         - The scoped container.
     */
    function clearFieldErrorOnInput($field, fieldName, $c) {
        $field.off('input.valclear change.valclear')
            .on('input.valclear change.valclear', function () {
                var $fg = $field.closest('.govuk-form-group');
                $fg.removeClass('govuk-form-group--error');
                $field.removeClass('govuk-input--error');
                $fg.find('[data-valmsg-for="' + fieldName + '"]')
                    .text('')
                    .hide()
                    .removeClass('field-validation-error')
                    .addClass('field-validation-valid');
                $field.off('input.valclear change.valclear');
            });
    }

    /**
     * Normalise errors into a uniform array: [{ field, message }].
     * Accepts either an array or a plain-object dictionary.
     */
    function normaliseErrors(errors) {
        if (!errors) return [];
        if (Array.isArray(errors)) return errors;
        return Object.keys(errors).map(function (key) {
            return { field: key, message: errors[key] };
        });
    }

    /**
     * Attaches an input/change listener on $field so the inline error is cleared
     * as soon as the user provides a non-empty value.  Uses the '.valclear'
     * namespace so handlers can be removed cleanly by clearValidationErrors.
     *
     * @param {jQuery} $field    - The field element.
     * @param {string} fieldName - The name attribute used to locate the valmsg span.
     * @param {jQuery} $c        - The scoped container.
     */
    function clearFieldErrorOnInput($field, fieldName, $c) {
        $field.off('input.valclear change.valclear')
            .on('input.valclear change.valclear', function () {
                if (!$(this).val() || $(this).val().trim() === '') return;
                var $fg = $(this).closest('.govuk-form-group');
                $fg.removeClass('govuk-form-group--error');
                $(this).removeClass('govuk-input--error');
                $fg.find('[data-valmsg-for="' + fieldName + '"]')
                    .text('')
                    .hide()
                    .removeClass('field-validation-error')
                    .addClass('field-validation-valid');
            });
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /**
     * Returns true when every [required] field inside `form` has a non-blank value.
     * @param {jQuery} form - The form element to validate.
     * @returns {boolean}
     */
    window.isFormValid = function (form) {
        var valid = true;
        form.find('[required]').each(function () {
            var v = $(this).val();
            if (!v || v.trim() === '') {
                valid = false;
                return false; // break $.each
            }
        });
        return valid;
    };

    /**
     * Clears all validation errors inside `container`:
     *   - Hides and empties the govuk-error-summary.
     *   - Removes govuk-form-group--error / govuk-input--error classes.
     *   - Empties and hides every validation-message span (data-valmsg-for).
     *
     * @param {string|jQuery} [container] - Scope element (defaults to document).
     */
    window.clearValidationErrors = function (container) {
        var $c = resolveContainer(container);

        $c.find('.govuk-error-summary').hide();
        $c.find('.govuk-error-summary__list').empty();

        $c.find('.govuk-form-group--error').removeClass('govuk-form-group--error');
        $c.find('.govuk-input--error').removeClass('govuk-input--error');

        $c.find('[data-valmsg-for]').each(function () {
            var fieldName = $(this).attr('data-valmsg-for');
            $c.find('[name="' + fieldName + '"]').off('input.valclear change.valclear');
            $(this)
                .text('')
                .hide()
                .removeClass('field-validation-error')
                .addClass('field-validation-valid');
        });
    };

    /**
     * Validates required fields client-side and displays errors.
     * Highlights each invalid field inline and populates the govuk-error-summary.
     *
     * @param {jQuery}        form        - The <form> element to validate.
     * @param {string|jQuery} [container] - Scope element (defaults to `form`).
     */
    window.displayClientValidationErrors = function (form, container) {
        var $c = resolveContainer(container != null ? container : form);

        // Collect [Range] and other jQuery Unobtrusive Validation errors BEFORE
        // clearValidationErrors wipes the spans that $form.valid() just populated.
        var jqueryValErrors = [];
        form.find('[data-valmsg-for]').each(function () {
            var $span = $(this);
            if ($span.hasClass('field-validation-error') && $span.text().trim() !== '') {
                jqueryValErrors.push({
                    field: $span.attr('data-valmsg-for'),
                    message: $span.text().trim()
                });
            }
        });

        clearValidationErrors($c);

        var errors = [];
        form.find('[required]').each(function () {
            var $field = $(this);
            if (!$field.val() || $field.val().trim() === '') {
                var name = $field.attr('name') || '';
                var label = $('label[for="' + name + '"]', $c).clone().children().remove().end().text().trim().replace(/:\s*$/, '') || name;
                var requiredMessage = $field.attr('data-val-required') || $field.attr('data-msg-required');
                errors.push({ field: name, message: requiredMessage || (label + ' is required') });
            }
        });

        // Merge jQuery Unobtrusive Validation errors (e.g. [Range]) — avoid duplicates
        jqueryValErrors.forEach(function (jqErr) {
            var alreadyAdded = errors.some(function (e) { return e.field === jqErr.field; });
            if (!alreadyAdded) {
                errors.push(jqErr);
            }
        });

        if (!errors.length) return;

        var $summary = $c.find('.govuk-error-summary');
        $summary.find('.govuk-error-summary__title').text('There is a problem');
        var $list = $summary.find('.govuk-error-summary__list').empty();

        var hasSummaryErrors = false;

        errors.forEach(function (error) {
            var $field = $('[name="' + error.field + '"]', $c);

            if ($field.length) {
                // Field found in the form — highlight inline only
                var $fg = $field.closest('.govuk-form-group').addClass('govuk-form-group--error');
                $field.addClass('govuk-input--error');
                $fg.find('[data-valmsg-for="' + error.field + '"]')
                    .text(error.message)
                    .show()
                    .removeClass('field-validation-valid')
                    .addClass('field-validation-error');
                clearFieldErrorOnInput($field, error.field, $c);
            } else {
                // No matching field — show in summary only
                $list.append(
                    '<li><a href="#' + error.field + '">' + error.message + '</a></li>'
                );
                hasSummaryErrors = true;
            }
        });

        if (hasSummaryErrors) {
            $summary.show().focus();
        } else if ($summary.length) {
            $summary.hide();
        }
    };

    /**
     * Displays server-side validation errors.
     *
     * - When a matching named field is found in `container`:
     *   highlights it inline only (not added to the summary).
     * - When no matching field is found:
     *   adds the message to the govuk-error-summary.
     *
     * @param {Array|Object}   errors           - Array or dictionary of errors.
     * @param {string}        [summaryMessage]  - Heading text for the error summary.
     * @param {string|jQuery} [container]       - Scope element (defaults to document).
     */
    window.displayServerValidationErrors = function (errors, summaryMessage, container) {
        var $c = resolveContainer(container);
        var $summary = $c.find('.govuk-error-summary');
        var $list = $summary.find('.govuk-error-summary__list').empty();
        $summary.find('.govuk-error-summary__title').text('There is a problem');

        var items = normaliseErrors(errors);
        var hasSummaryErrors = false;

        items.forEach(function (error) {
            var fieldName = error.field || '';
            var message = error.message || 'Validation error';
            var $field = $('[name="' + fieldName + '"]', $c);

            if ($field.length) {
                // Field found in the form — highlight inline only
                var $fg = $field.closest('.govuk-form-group').addClass('govuk-form-group--error');
                $field.addClass('govuk-input--error');
                $fg.find('[data-valmsg-for="' + fieldName + '"]')
                    .text(message)
                    .show()
                    .removeClass('field-validation-valid')
                    .addClass('field-validation-error');
                clearFieldErrorOnInput($field, fieldName, $c);
            } else {
                // No matching field — show in summary only
                $list.append(
                    '<li><a href="#' + fieldName + '">' + message + '</a></li>'
                );
                hasSummaryErrors = true;
            }
        });

        if (hasSummaryErrors) {
            $summary.show().focus();
        } else if ($summary.length) {
            $summary.hide();
        }
    };

}(jQuery));