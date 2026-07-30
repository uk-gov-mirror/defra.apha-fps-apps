// Project Maintenance Details - JavaScript Module
// Handles all client-side functionality for the Project Maintenance Details page

// Global variables
var parentProject = '';
var jobCodeGridId = '';
var timeCodeGridId = '';
var selectedJobCodeId = null;

// Multicolumn dropdown instances
var programDropdown = null;
var contractDropdown = null;

// Data for dropdowns
var programListData = [];
var contractListData = [];
var selectedProgramValue = '';
var selectedContractValue = '';

// Initialize the page
function initializeProjectMaintenanceDetails(config) {
    parentProject = config.parentProject;
    jobCodeGridId = config.jobCodeGridId;
    timeCodeGridId = config.timeCodeGridId;
    programListData = config.programListData;
    contractListData = config.contractListData;
    selectedProgramValue = config.selectedProgramValue;
    selectedContractValue = config.selectedContractValue;

    // Initialize dropdowns when page loads
    $(document).ready(function () {
        initializeProgramDropdown();
        initializeContractDropdown();
    });
}

// Toggle sidebar
function toggleSidebar() {
    document.querySelector('.sidenav').classList.toggle('collapsed');
}

// Grid manager accessors
function getJobCodeGridManager() { 
    return window['gridManager_' + jobCodeGridId]; 
}

function getTimeCodeGridManager() { 
    return window['gridManager_' + timeCodeGridId]; 
}

// Save project details
function saveProjectDetail() {
    var form = $('#projectDetailForm');
    var data = {};
    var decimalFields = ['BudgetCvl', 'TransferIncome', 'BudgetExt', 'PvsIncome', 'WipEoy', 'WipLimit', 'WipCurrent', 'FecCost'];
    var editableDecimalFields = ['TransferIncome', 'BudgetExt', 'PvsIncome', 'WipEoy', 'WipLimit', 'WipCurrent', 'FecCost'];
    var maxMoney = 92233720368547758.07;
    var validationError = null;

    form.serializeArray().forEach(function(item) {
        if (validationError) return; // Skip if already failed

        var key = item.name.startsWith('Project.') ? item.name.substring('Project.'.length) : item.name;
        if (decimalFields.indexOf(key) !== -1) {
            var parsed = parseFloat(item.value);

            // Only validate editable fields (skip BudgetCvl as it's readonly)
            if (editableDecimalFields.indexOf(key) !== -1 && !isNaN(parsed)) {
                if (parsed < 0 || parsed > maxMoney) {
                    validationError = key + '  value you enter is not valid for this fields. The entered value is larger than the fieldsize permit.';
                    return;
                }

                // Check decimal places
                var decimalPart = parsed.toString().split('.')[1];
                if (decimalPart && decimalPart.length > 2) {
                    validationError = key + '  value you enter is not valid for this fields. The entered value is larger than the fieldsize permit.';
                    return;
                }
            }

            data[key] = isNaN(parsed) ? 0 : parsed;
        } else {
            data[key] = item.value;
        }
    });

    // Stop if validation failed
    if (validationError) {
        showAlertMessage(validationError, AlertType.INFO);
        return;
    }

    // Add multicolumn dropdown values from hidden inputs
    data.Program = $('#Project_Program').val();
    data.Contract = $('#Project_Contract').val();

    data.IsDefraProject = $('#IsDefraProject').is(':checked') ? -1 : 0;
    data.Finished = $('#Finished').is(':checked') ? 1 : 0;

    clearValidationErrors('#projectDetailForm');

    $.ajax({
        url: '/PACT/ProjectMaintenance/Edit',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function(response) {
            if (response.success) {
                showAlertMessage('Project updated successfully.', AlertType.SUCCESS)
                .then(function () {
                    window.location.reload();
                });
            }
            else {
                displayServerValidationErrors(response.errors, response.message, '#projectDetailForm');
            }
        },
        error: function() { 
            showAlertMessage('An error occurred while saving.', AlertType.ERROR); 
        }
    });
}

// Job Code functions
function addJobCode() {
    $.get('/PACT/ProjectMaintenance/CreateJobCode', { parentProject: decodeURIComponent(parentProject) }, function(html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function editJobCode(btn) {
    var jobCodeId = $(btn).data('id');
    selectedJobCodeId = jobCodeId;
    $.get('/PACT/ProjectMaintenance/EditJobCode', { jobCodeId: jobCodeId }, function(html) {
        $('#modaPopupBody').html(html);
        $('#modalPopup').addClass('show');
    });
}

function deleteJobCode(btn) {
    var jobCodeId = $(btn).data('id');
    showGovukConfirm('Delete this job code?').then(function(confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/ProjectMaintenance/DeleteJobCode',
            type: 'DELETE',
            data: { jobCodeId: jobCodeId, parentProject: decodeURIComponent(parentProject) },
            success: function(response) {
                if (response.success) {
                    getJobCodeGridManager()?.reloadGrid({ page: 1 });
                    reloadTimeCodeGrid(null);
                    showAlertMessage('JobCode deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Error: ' + response.message, AlertType.ERROR);
                }
            },
            error: function() { 
                showAlertMessage('An error occurred while deleting.', AlertType.ERROR); 
            }
        });
    });
}

function copyJobCode(btn) {
    var jobCodeId = $(btn).data('id');
    selectedJobCodeId = jobCodeId;
    $.get('/PACT/ProjectMaintenance/CopyProjectJobCode',
        { jobCodeId: selectedJobCodeId },
        function(html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        });
}

function saveJobCode() {
    var form = $('#jobCodeForm');
    if (form.length && typeof form.valid === 'function' && !form.valid()) return;
    var data = form.serializeObject ? form.serializeObject() : Object.fromEntries(new FormData(form[0]));
    var successMsg = data.isEdit === 'true'
        ? 'JobCode edited successfully.'
        : 'JobCode saved successfully.';
    var url = data.isEdit === 'true'
        ? '/PACT/ProjectMaintenance/EditJobCode'
        : '/PACT/ProjectMaintenance/CreateJobCode';

    clearValidationErrors('#jobCodeForm');

    $.ajax({
        url: url, 
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function(response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                getJobCodeGridManager()?.reloadGrid({ page: 1 });
                showAlertMessage(successMsg, AlertType.SUCCESS);
            } else {
                displayServerValidationErrors(response.errors, response.message, '#jobCodeForm');
            }
        },
        error: function() { 
            showAlertMessage('An error occurred while saving.', AlertType.ERROR); 
        }
    });
}

function loadTimeCodesForJobCode(btn) {
    selectedJobCodeId = $(btn).data('id');
    $('#timeCodeGridSubtitle').text('— Job Code: ' + selectedJobCodeId);
    reloadTimeCodeGrid(selectedJobCodeId);
}

function selectJobCode(row) {
    var jobCodeId = $(row).data('id');
    selectedJobCodeId = jobCodeId;
    $('#timeCodeGridSubtitle').text('— Job Code: ' + jobCodeId);
    reloadTimeCodeGrid(jobCodeId);
}

function reloadTimeCodeGrid(jobCodeId) {
    selectedJobCodeId = jobCodeId;
    var params = { parentProject: decodeURIComponent(parentProject), jobCodeId: jobCodeId || '' };
    $.post('/PACT/ProjectMaintenance/LoadTimeCodeGrid', params, function(html) {
        $('#gridContainer_' + timeCodeGridId).html(html);
    });
}

function getTimeCodeExtraFilters() {
    return { jobCodeId: selectedJobCodeId || '' };
}

// Time Code functions
function addTimeCode() {
    if (!selectedJobCodeId) { 
        showAlertMessage('Please select a job code first.', AlertType.INFO); 
        return; 
    }
    $.get('/PACT/ProjectMaintenance/CreateTimeCode',
        { parentProject: decodeURIComponent(parentProject), jobCodeId: selectedJobCodeId },
        function(html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        });
}

function editTimeCode(btn) {
    var timeCode = $(btn).data('id');
    var workGroup = $(btn).closest('tr').find('[data-property="WorkGroup"] span').text();
    $.get('/PACT/ProjectMaintenance/EditTimeCode',
        { workGroup: workGroup, timeCode: timeCode, jobCodeId: selectedJobCodeId, parentProject: decodeURIComponent(parentProject) },
        function(html) {
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        });
}

function deleteTimeCode(btn) {
    var timeCode = $(btn).data('id');
    var workGroup = $(btn).closest('tr').find('[data-property="WorkGroup"] span').text();
    showGovukConfirm('Delete time code "' + timeCode + '"?').then(function(confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/ProjectMaintenance/DeleteTimeCode',
            type: 'DELETE',
            data: { workGroup: workGroup, timeCode: timeCode, parentProject: decodeURIComponent(parentProject) },
            success: function(response) {
                if (response.success) {
                    reloadTimeCodeGrid(selectedJobCodeId);
                    showAlertMessage('Time code deleted successfully.', AlertType.SUCCESS);
                } else {
                    showAlertMessage('Error: ' + response.message, AlertType.ERROR);
                }
            },
            error: function() { 
                showAlertMessage('An error occurred while deleting.', AlertType.ERROR); 
            }
        });
    });
}

function saveTimeCode() {
    var form = $('#timeCodeForm');
    if (form.length && typeof form.valid === 'function' && !form.valid()) return;
    var data = form.serializeObject ? form.serializeObject() : Object.fromEntries(new FormData(form[0]));
    data.Active = $('#Active').is(':checked');
    var successMsg = data.isEdit === 'true'
        ? 'TimeCode edited successfully.'
        : 'TimeCode saved successfully.';
    var url = data.isEdit === 'true'
        ? '/PACT/ProjectMaintenance/EditTimeCode'
        : '/PACT/ProjectMaintenance/CreateTimeCode';

    clearValidationErrors('#timeCodeForm');

    $.ajax({
        url: url, 
        type: 'POST', 
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function(response) {
            if (response.success) {
                $('#modalPopup').removeClass('show');
                reloadTimeCodeGrid(selectedJobCodeId);
                showAlertMessage(successMsg, AlertType.SUCCESS);
            } else {
                displayServerValidationErrors(response.errors, response.message, '#timeCodeForm');
            }
        },
        error: function() { 
            showAlertMessage('An error occurred while saving.', AlertType.ERROR); 
        }
    });
}

function executeCopyJobCode() {
    var $form = $('#formAddJobcode');
    clearValidationErrors($form);
    if (!isFormValid($form)) {
        displayClientValidationErrors($form, $form);
        return;
    }
    var jobCodeId = $('#JobCodeId').val();
    var data = {
        SourceJobCode: $('#sourceJobCode').val(),
        JobCodeId: jobCodeId,
        JobCodeName: $('#JobCodeName').val(),
        Type: $('#Type').val(),
        JobCodeWorkGroup: $('#JobCodeWorkGroup').val(),
        ParentProject: decodeURIComponent(parentProject),
        CopyWorkGroup: $('#chkcopywithworkgroup').is(':checked')
    };
    showGovukConfirm('Copy job code "' + data.SourceJobCode + '" to "' + jobCodeId + '"?').then(function(confirmed) {
        if (!confirmed) return;
        $.ajax({
            url: '/PACT/ProjectMaintenance/CopyProjectJobCode',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function(response) {
                if (response.success) {
                    $('#modalPopup').removeClass('show');
                    getJobCodeGridManager()?.reloadGrid({ page: 1 });
                    if (data.CopyWorkGroup) {
                        reloadTimeCodeGrid(jobCodeId);
                    }
                    showAlertMessage('JobCode copied successfully.', AlertType.SUCCESS);
                } else {
                    displayServerValidationErrors(response.errors, response.message, '#formAddJobcode');
                }
            },
            error: function() { 
                showAlertMessage('An error occurred while copying.', AlertType.ERROR); 
            }
        });
    });
}

// Bulk Copy Work Group functions
function copyBulkWorkGroup(selection) {
    if (!selectedJobCodeId) { 
        showAlertMessage('Please select a job code first.', AlertType.INFO); 
        return; 
    }
    if (!selection || !selection.ids || selection.ids.length === 0) {
        showAlertMessage('Please select at least one work group to copy.', AlertType.INFO); 
        return;
    }

    $.get('/PACT/ProjectMaintenance/CopyWorkGroupPartial',
        { parentProject: decodeURIComponent(parentProject), sourceJobCodeId: selectedJobCodeId },
        function(html) {
            $('#modaPopupBody').html(html);

            if (selection.isAll) {
                // All pages selected — backend will copy every work group
                $('#cwg_isAll').val('true');
                $('#cwg_workGroupsHidden').empty();
            } else {
                // Only the checked rows on the current page
                $('#cwg_isAll').val('false');
                var container = $('#cwg_workGroupsHidden').empty();
                $('#tbl_' + timeCodeGridId + ' tbody tr').each(function() {
                    if ($(this).find('.row-checkbox').is(':checked')) {
                        var wg = $(this).find('[data-property="WorkGroup"] span').text().trim();
                        if (wg) {
                            container.append(
                                '<input type="hidden" class="cwg-wg" value="' +
                                $('<div>').text(wg).html() + '" />'
                            );
                        }
                    }
                });

                if (container.find('.cwg-wg').length === 0) {
                    showAlertMessage('No work groups found in selected rows.', AlertType.INFO);
                    return;
                }
            }

            $('#modalPopup').addClass('show');
        });
}

function executeCopyBulkWorkGroup() {
    var targetJobCode = $('#dpTargetJobcode').val();
    var sourceJobCode = $('#cwg_sourceJobCode').val();
    var pp = $('#cwg_parentProject').val();
    var isAll = $('#cwg_isAll').val() === 'true';

    clearValidationErrors('#copyWorkGroupForm');

    if (!targetJobCode) {
        displayServerValidationErrors(
            [{ field: 'dpTargetJobcode', message: 'Please select a target job code.' }],
            'There is a problem',
            '#copyWorkGroupForm'
        );
        return;
    }

    if (isAll) {
        // Copy every work group across all pages using the simple copy endpoint
        showGovukConfirm('Copy all work groups from "' + sourceJobCode + '" to "' + targetJobCode + '"?').then(function(confirmed) {
            if (!confirmed) return;
            $.ajax({
                url: '/PACT/ProjectMaintenance/CopyAllJobCodeWorkGroups',
                type: 'POST',
                data: { parentProject: pp, sourceJobCodeId: sourceJobCode, targetJobCodeId: targetJobCode },
                success: function(response) {
                    if (response.success) {
                        $('#modalPopup').removeClass('show');
                        reloadTimeCodeGrid(selectedJobCodeId);
                        showAlertMessage('All work groups copied successfully.', AlertType.SUCCESS);
                    } else {
                        displayServerValidationErrors(response.errors, response.message, '#copyWorkGroupForm');
                    }
                },
                error: function() { 
                    showAlertMessage('An error occurred while copying.', AlertType.ERROR); 
                }
            });
        });
    } else {
        // Copy only the selected work groups
        var workGroups = [];
        $('#cwg_workGroupsHidden .cwg-wg').each(function() {
            workGroups.push($(this).val());
        });

        if (workGroups.length === 0) { 
            showAlertMessage('No work groups to copy.', AlertType.INFO); 
            return; 
        }

        showGovukConfirm('Copy ' + workGroups.length + ' work group(s) from "' + sourceJobCode + '" to "' + targetJobCode + '"?').then(function(confirmed) {
            if (!confirmed) return;
            $.ajax({
                url: '/PACT/ProjectMaintenance/CopyBulkWorkGroup',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    parentProject: pp,
                    sourceJobCodeId: sourceJobCode,
                    targetJobCodeId: targetJobCode,
                    workGroups: workGroups
                }),
                success: function(response) {
                    if (response.success) {
                        $('#modalPopup').removeClass('show');
                        reloadTimeCodeGrid(selectedJobCodeId);
                        showAlertMessage('Work group(s) copied successfully.', AlertType.SUCCESS);
                    } else {
                        displayServerValidationErrors(response.errors, response.message, '#copyWorkGroupForm');
                    }
                },
                error: function() { 
                    showAlertMessage('An error occurred while copying.', AlertType.ERROR); 
                }
            });
        });
    }
}

// Bulk Delete Time Codes
function deleteBulkTimeCode(selection) {
    if (!selectedJobCodeId) { 
        showAlertMessage('Please select a job code first.', AlertType.INFO); 
        return; 
    }
    if (!selection || !selection.ids || selection.ids.length === 0) {
        showAlertMessage('Please select at least one time code row to delete.', AlertType.INFO); 
        return;
    }

    if (selection.isAll) {
        // Select-all across pages — delete every time code for this job code
        showGovukConfirm('Delete ALL time codes for job code "' + selectedJobCodeId + '"?').then(function(confirmed) {
            if (!confirmed) return;
            $.ajax({
                url: '/PACT/ProjectMaintenance/DeleteAllJobCodeTimeCodes',
                type: 'POST',
                data: { parentProject: decodeURIComponent(parentProject), jobCodeId: selectedJobCodeId },
                success: function(response) {
                    if (response.success) {
                        reloadTimeCodeGrid(selectedJobCodeId);
                        showAlertMessage('All time codes deleted successfully.', AlertType.SUCCESS);
                    } else {
                        showAlertMessage('Error: ' + (response.message || 'Delete failed.'), AlertType.ERROR);
                    }
                },
                error: function() { 
                    showAlertMessage('An error occurred while deleting.', AlertType.ERROR); 
                }
            });
        });
    } else {
        // Delete only the checked rows on the current page
        var items = [];
        $('#tbl_' + timeCodeGridId + ' tbody tr').each(function() {
            if ($(this).find('.row-checkbox').is(':checked')) {
                var timeCode = String($(this).data('id'));
                var wg = $(this).find('[data-property="WorkGroup"] span').text().trim();
                if (wg && timeCode) items.push({ workGroup: wg, timeCode: timeCode });
            }
        });

        if (items.length === 0) { 
            showAlertMessage('No rows selected for deletion.', AlertType.INFO); 
            return; 
        }

        showGovukConfirm('Delete ' + items.length + ' selected time code(s)?').then(function(confirmed) {
            if (!confirmed) return;
            $.ajax({
                url: '/PACT/ProjectMaintenance/DeleteBulkTimeCode',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    parentProject: decodeURIComponent(parentProject),
                    items: items
                }),
                success: function(response) {
                    if (response.success) {
                        reloadTimeCodeGrid(selectedJobCodeId);
                        showAlertMessage(items.length + ' time code(s) deleted successfully.', AlertType.SUCCESS);
                    } else {
                        showAlertMessage('Error: ' + (response.message || 'Delete failed.'), AlertType.ERROR);
                    }
                },
                error: function() { 
                    showAlertMessage('An error occurred while deleting.', AlertType.ERROR); 
                }
            });
        });
    }
}

// Initialize multicolumn dropdowns
function initializeProgramDropdown() {
    programDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'programDropdown',
        containerSelector: '#programMultiDropdown',
        placeholder: 'Select a Programme',
        showSerialNumber: false,
        searchPlaceholder: 'Search by code or name',
        labelText: 'Programme',
        required: true,
        columns: [
            { field: 'Value', header: 'Code', width: '100px' },
            { field: 'Text', header: 'Programme Name', width: '250px' }
        ],
        data: programListData,
        displayField: 'Text',
        valueField: 'Value',
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem, dropdown) {
                $('#Project_Program').val(selectedItem.Value);
            },
            onClear: function (dropdown) {
                $('#Project_Program').val('');
                programDropdown.clear();
            }
        }
    });
    // Set initial value if exists
    if (selectedProgramValue && selectedProgramValue !== '') {
        programDropdown.setValue(selectedProgramValue);
    }
}

function initializeContractDropdown() {
    contractDropdown = new MultiColumnDropdownComponent({
        dropdownId: 'contractDropdown',
        containerSelector: '#contractMultiDropdown',
        placeholder: 'Select a Contract',
        showSerialNumber: false,
        searchPlaceholder: 'Search by code or name',
        labelText: 'Contract',
        required: true,
        columns: [
            { field: 'Value', header: 'Code', width: '100px' },
            { field: 'Text', header: 'Contract Name', width: '250px' }
        ],
        data: contractListData,
        displayField: 'Text',
        valueField: 'Value',
        clearButtonClearsSelection: true,
        callbacks: {
            onSelect: function (selectedItem, dropdown) {
                $('#Project_Contract').val(selectedItem.Value);
            },
            onClear: function (dropdown) {
                $('#Project_Contract').val('');
                contractDropdown.clear();
            }
        }
    });
   
    // Set initial value if exists
    if (selectedContractValue && selectedContractValue !== '') {
        contractDropdown.setValue(selectedContractValue);
    }
}

// Initialize WorkGroup Multicolumn Dropdown for Job Code Modal
function initializeJobCodeWorkGroupDropdown(config) {
    var workGroupData = config.workGroupData || [];
    var selectedWorkGroup = config.selectedWorkGroup || '';

            jobCodeWorkGroupDropdown = new MultiColumnDropdownComponent({
            dropdownId: 'workGroupDropdown',
            containerSelector: '#workGroupMultiDropdown',
            placeholder: 'Select a Work Group',
            showSerialNumber: false,
            searchPlaceholder: 'Search by work group',
            labelText: 'Work Group',
            required: false,
            columns: [
                { field: 'Value', header: 'Code', width: '100px' },
                { field: 'Text', header: 'Work Group', width: '250px' }
            ],
            data: workGroupData,
            displayField: 'Text',
            valueField: 'Value',
            clearButtonClearsSelection: true,
            callbacks: {
                onSelect: function (selectedItem, dropdown) {
                    $('#JobCodeWorkGroup').val(selectedItem.Value);
                },
                onClear: function (dropdown) {
                    $('#JobCodeWorkGroup').val('');
                    jobCodeWorkGroupDropdown.clear();
                }
            }
        });

        // Set initial value if exists
        if (selectedWorkGroup && selectedWorkGroup !== '') {
            jobCodeWorkGroupDropdown.setValue(selectedWorkGroup);
        }
}
