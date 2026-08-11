// Number Validation - Shared JavaScript Module

// Numeric input validation - allows positive/negative numbers with decimal point
function validateNumericInput(event) {
    var input = event.target;
    var value = input.value;
    var key = event.key;
    var cursorPosition = input.selectionStart;

    // Allow control keys
    if (['Backspace', 'Delete', 'Tab', 'Escape', 'Enter', 'ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'].includes(key)) {
        return true;
    }

    // Allow Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X
    if (event.ctrlKey || event.metaKey) {
        return true;
    }

    // Allow digits
    if (/^\d$/.test(key)) {
        return true;
    }

    // Allow minus sign only at the beginning and only if there isn't one already
    if (key === '-') {
        if (cursorPosition === 0 && !value.includes('-')) {
            return true;
        }
        event.preventDefault();
        return false;
    }

    // Allow decimal point only if there isn't one already
    if (key === '.' || key === ',') {
        if (!value.includes('.') && !value.includes(',')) {
            return true;
        }
        event.preventDefault();
        return false;
    }

    // Block all other keys
    event.preventDefault();
    return false;
}

// Format and validate numeric input on paste
function handleNumericPaste(event) {
    // Get the original event if this is a jQuery event
    var originalEvent = event.originalEvent || event;
    originalEvent.preventDefault();

    var pastedData = (originalEvent.clipboardData || window.clipboardData).getData('text');

    // Check if pasted data contains any invalid characters (anything other than digits, minus, decimal, and whitespace)
    // This includes alphabetic characters AND special characters
    if (/[^\d.\-\s]/.test(pastedData)) {
        showAlertMessage('You may have enter text in a numeric field or a number that is larger than the FieldSize Permits.', AlertType.ERROR);
        return;
    }

    // Remove any non-numeric characters except minus and decimal point (including spaces)
    var cleaned = pastedData.replace(/[^\d.-]/g, '');

    // If after cleaning, nothing remains, show error
    if (!cleaned) {
        showAlertMessage('You may have enter text in a numeric field or a number that is larger than the FieldSize Permits.', AlertType.ERROR);
        return;
    }

    // Ensure only one minus sign at the beginning
    if (cleaned.indexOf('-') > 0) {
        cleaned = cleaned.replace(/-/g, '');
    } else if ((cleaned.match(/-/g) || []).length > 1) {
        cleaned = '-' + cleaned.replace(/-/g, '');
    }

    // Ensure only one decimal point
    var parts = cleaned.split('.');
    if (parts.length > 2) {
        cleaned = parts[0] + '.' + parts.slice(1).join('');
    }

    // Enforce maxlength of 20 characters
    if (cleaned.length > 20) {
        showAlertMessage('Value exceeds maximum length of 20 characters.', AlertType.ERROR);
        return;
    }

    // Validate range: -999999999999999.9999 to 999999999999999.9999
    var parsedValue = parseFloat(cleaned);
    var min = -999999999999999.9999;
    var max = 999999999999999.9999;

    if (!isNaN(parsedValue) && (parsedValue < min || parsedValue > max)) {
        showAlertMessage('Value must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999', AlertType.ERROR);
        return;
    }

    // Get the input element (handle both jQuery events and native events)
    var input = event.target || event.currentTarget;
    var start = input.selectionStart;
    var end = input.selectionEnd;
    var currentValue = input.value;

    // Replace the selected portion (or insert at cursor if nothing selected)
    input.value = currentValue.substring(0, start) + cleaned + currentValue.substring(end);

    // Set cursor position after the pasted content
    var newCursorPos = start + cleaned.length;
    input.selectionStart = input.selectionEnd = newCursorPos;

    // Trigger input event for any validation listeners
    $(input).trigger('input');
}

// Validate numeric input range and provide visual feedback
function validateRangeOnInput(input) {
    var $input = $(input); // Use jQuery for consistency
    var value = $input.val().trim();
    var fieldName = $input.attr('name') || $input.attr('id');

    // Only allow minus at the beginning, remove any other minus signs
    if (value.length > 0) {
        var sanitized = value;
        var firstChar = value.charAt(0);
        var isNegative = firstChar === '-';

        if (isNegative) {
            // Keep first minus, remove all others
            sanitized = '-' + value.substring(1).replace(/-/g, '');
        } else {
            // Remove all minus signs if not at the beginning
            sanitized = value.replace(/-/g, '');
        }

        // Ensure only one decimal point
        var parts = sanitized.split('.');
        if (parts.length > 2) {
            sanitized = parts[0] + '.' + parts.slice(1).join('');
        }

        // Update the field value if it was sanitized
        if (sanitized !== value) {
            var cursorPos = input.selectionStart;
            $input.val(sanitized);
            // Restore cursor position
            input.selectionStart = input.selectionEnd = Math.min(cursorPos, sanitized.length);
            value = sanitized;
        }
    }

    // Find the parent form-group and validation message span
    var $formGroup = $input.closest('.govuk-form-group');
    var $validationSpan = $formGroup.find('span[data-valmsg-for="' + fieldName + '"]');

    // If not found, try with asp-validation-for
    if ($validationSpan.length === 0) {
        $validationSpan = $formGroup.find('span[asp-validation-for="' + fieldName + '"]');
    }

    // If still not found by name, try finding by class
    if ($validationSpan.length === 0) {
        $validationSpan = $formGroup.find('.govuk-error-message, .field-validation-error, .validation-message');
    }

    // Skip validation if field is empty
    if (value === '' || value === '-') {
        $input.removeClass('govuk-input--error');
        $formGroup.removeClass('govuk-form-group--error');
        $input.removeAttr('title');
        if ($validationSpan.length > 0) {
            $validationSpan.text('').hide();
        }
        return;
    }

    var parsedValue = parseFloat(value);
    var min = -999999999999999.9999;
    var max = 999999999999999.9999;

    // Check if value is a valid number
    if (isNaN(parsedValue)) {
        $input.addClass('govuk-input--error');
        $formGroup.addClass('govuk-form-group--error');
        $input.attr('title', 'Please enter a valid number');
        if ($validationSpan.length > 0) {
            $validationSpan.removeClass('field-validation-valid')
                          .addClass('field-validation-error')
                          .text('Please enter a valid number')
                          .show()
                          .css('display', 'block');
        }
        return;
    }

    // Check if value is within range
    if (parsedValue < min || parsedValue > max) {
        var errorMessage = 'Value must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999';
        $input.addClass('govuk-input--error');
        $formGroup.addClass('govuk-form-group--error');
        $input.attr('title', errorMessage);
        if ($validationSpan.length > 0) {
            $validationSpan.removeClass('field-validation-valid')
                          .addClass('field-validation-error')
                          .text(errorMessage)
                          .show()
                          .css('display', 'block');
        }
    } else {
        $input.removeClass('govuk-input--error');
        $formGroup.removeClass('govuk-form-group--error');
        $input.removeAttr('title');
        if ($validationSpan.length > 0) {
            $validationSpan.removeClass('field-validation-error')
                          .addClass('field-validation-valid')
                          .text('')
                          .hide();
        }
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// Numeric Validation Initialization
// ══════════════════════════════════════════════════════════════════════════════

/**
 * Attach numeric validation to all input fields with the 'decfmt-input' class.
 * This function binds keydown, paste, and input event handlers to enable
 * real-time numeric validation and range checking.
 * Also sets maxlength="20" on fields that don't already have it set.
 */
function attachNumericValidation() {
    // Find all input fields with decfmt-input class
    $('.decfmt-input').each(function () {
        var $input = $(this);

        // Set maxlength="20" for all decfmt-input fields if not already set
        if (!$input.attr('maxlength')) {
            $input.attr('maxlength', '20');
        }

        // Remove any existing handlers to prevent duplicate bindings
        $input.off('keydown.numericValidation');
        $input.off('paste.numericValidation');
        $input.off('input.numericValidation');
        $input.off('blur.numericValidation');

        // Attach keydown event for character-by-character validation
        $input.on('keydown.numericValidation', validateNumericInput);

        // Attach paste event for clipboard data validation
        $input.on('paste.numericValidation', handleNumericPaste);

        // Attach input event for range validation and real-time feedback
        $input.on('input.numericValidation', function () {
            validateRangeOnInput(this);
        });

        // Attach blur event for final validation
        $input.on('blur.numericValidation', function() {
            validateRangeOnInput(this);
        });
    });
}

/**
 * Check if a form has any numeric validation errors.
 * This function checks for fields with the .govuk-input--error class within the given form.
 * 
 * @param {jQuery} form - The form element to check for numeric errors.
 * @returns {boolean} - Returns true if there are numeric validation errors, false otherwise.
 * 
 * Usage example:
 *   if (hasNumericValidationErrors(form)) {
 *       // Handle validation errors
 *   }
 */
function hasNumericValidationErrors(form) {
    return form.find('.govuk-input--error').length > 0;
}

/**
 * Ensure validation messages are visible for all fields with errors.
 * This function makes sure that validation message spans are properly shown
 * and marked with the correct validation classes.
 * 
 * @param {jQuery} form - The form element to process.
 * 
 * Usage example:
 *   ensureValidationMessagesVisible(form);
 */
function ensureValidationMessagesVisible(form) {
    form.find('.govuk-input--error').each(function() {
        var $input = $(this);
        var fieldName = $input.attr('name') || $input.attr('id');
        var $formGroup = $input.closest('.govuk-form-group');
        var $validationSpan = $formGroup.find('span[data-valmsg-for="' + fieldName + '"]');

        // If not found, try with asp-validation-for
        if ($validationSpan.length === 0) {
            $validationSpan = $formGroup.find('span[asp-validation-for="' + fieldName + '"]');
        }

        // If still not found by name, try finding by class
        if ($validationSpan.length === 0) {
            $validationSpan = $formGroup.find('.govuk-error-message, .field-validation-error, .validation-message');
        }

        // Ensure the validation span is visible and has proper classes
        if ($validationSpan.length > 0 && $validationSpan.text().trim() !== '') {
            $validationSpan.removeClass('field-validation-valid')
                          .addClass('field-validation-error')
                          .show()
                          .css('display', 'block');
        }
    });
}

/**
 * Initialize form validation with both jQuery Unobtrusive Validation and numeric validation.
 * This is a convenience wrapper that combines unobtrusive validation parsing with numeric validation attachment.
 * 
 * @param {string} formSelector - jQuery selector for the form (e.g., '#invoiceForm', '#monthlyOutputLiveForm')
 * 
 * Usage example:
 *   initializeFormValidation('#invoiceForm');
 */
function initializeFormValidation(formSelector) {
    // Initialize jQuery Unobtrusive Validation
    if (typeof $.validator !== 'undefined' && $.validator.unobtrusive) {
        $.validator.unobtrusive.parse(formSelector);
    }

    // Attach numeric validation to all decfmt-input fields
    attachNumericValidation();
}