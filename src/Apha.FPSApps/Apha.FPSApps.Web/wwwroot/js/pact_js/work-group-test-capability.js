// ══════════════════════════════════════════════════════════════════════════════
// WorkGroup Test Capability - Client-side functionality
// ══════════════════════════════════════════════════════════════════════════════

(function () {
    'use strict';

    // ── State Management ──────────────────────────────────────────────────
    var currentWorkGroup = null;
    var testCapabilityGridId = null;

    // ── Initialization ────────────────────────────────────────────────────
    function initialize(gridId, serverWorkGroup) {
        testCapabilityGridId = gridId;

        // Wire up the searchable dropdown
        initWorkGroupDropdown();

        // Use server-rendered value; the Razor Page already reads the query string server-side
        var workgroupParam = serverWorkGroup || '';

        if (workgroupParam) {
            preselectWorkGroup(workgroupParam);
            currentWorkGroup = workgroupParam;
            // Load the grid with the workgroup
            reloadTestCapabilityGrid(workgroupParam);
        } else {
            // Clear grid on load if no workgroup specified
            clearTestCapabilityGrid();
        }
    }

    // ── Searchable WorkGroup Dropdown ─────────────────────────────────────
    function initWorkGroupDropdown() {
        var $wgInput  = $('#workGroupSelect');
        var $wgPanel  = $('#workGroupDropdownPanel');
        var $wgSearch = $('#workGroupSearchBox');
        var $wgRows   = $('#workGroupDropdownBody tr');

        $wgInput.on('click', function (e) {
            e.stopPropagation();
            $wgPanel.toggle();
            if ($wgPanel.is(':visible')) {
                $wgSearch.val('').focus();
                $wgRows.show();
            }
        });

        $wgSearch.on('click', function (e) { e.stopPropagation(); });

        $wgSearch.on('input', function () {
            var term = $(this).val().toLowerCase();
            $wgRows.each(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(term) > -1);
            });
        });

        $(document).on('click', '#workGroupDropdownBody tr', function () {
            var value = $(this).data('value');
            var text  = $(this).find('td:first').text().trim();
            $wgInput.val(text);
            $wgPanel.hide();
            onWorkGroupChange(value);
        });

        $(document).on('click', function (e) {
            if (!$(e.target).closest('#workGroupSelect, #workGroupDropdownPanel').length) {
                $wgPanel.hide();
            }
        });
    }

    function preselectWorkGroup(workGroup) {
        var $matchRow = $('#workGroupDropdownBody tr[data-value="' + workGroup + '"]');
        if ($matchRow.length) {
            $('#workGroupSelect').val($matchRow.find('td:first').text().trim());
        }
    }

    // ── Grid Manager Helper ───────────────────────────────────────────────
    function getCapabilityGridManager() {
        return window['gridManager_' + testCapabilityGridId];
    }

    // ── WorkGroup Selection ───────────────────────────────────────────────
    function onWorkGroupChange(value) {
        currentWorkGroup = value || null;
        if (currentWorkGroup) {
            reloadTestCapabilityGrid(currentWorkGroup);
        } else {
            clearTestCapabilityGrid();
        }
    }

    // ── Grid Reload ───────────────────────────────────────────────────────
    function reloadTestCapabilityGrid(workGroup) {
        $.ajax({
            url: '/PACT/WorkGroupTestCapability/LoadTestCapabilityGrid',
            type: 'POST',
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            data: {
                Page: 1,
                PageSize: 10,
                Filter: '{}',
                workGroup: workGroup || ''
            },
            success: function (html) {
                $('#gridContainer_testCapabilitiesWGGrid').html(html);
                // Auto-select the first row after grid loads
                var $dataRows = $('#tbl_' + testCapabilityGridId + ' tbody tr').filter(function () {
                    return $(this).find('[data-property]').length > 0;
                });

                if ($dataRows.length > 0) {
                    selectFirstTestCapabilityRow();
                }
                else {
                    onTestCapabilityRowSelect('');
                }
            },
            error: function () {
                console.error('Failed to load Test Capability grid.');
                showAlertMessage('Failed to load test capabilities. Please try again.', AlertType.ERROR);
            }
        });
    }

    // ── Auto-select first row in Test Capability grid ─────────────────────
    function selectFirstTestCapabilityRow() {
        setTimeout(function() {
            var $firstRow = $('#tbl_' + testCapabilityGridId + ' tbody tr:first');
            if ($firstRow.length > 0 && $firstRow.find('td').length > 1) {
                // Check if it's not an empty/message row
                var hasData = $firstRow.find('[data-property]').length > 0;
                if (hasData) {
                    // Trigger click on the first row to select it and populate portfolio
                    $firstRow.click();
                }
            }
        }, 100);
    }

    function clearTestCapabilityGrid() {
        $('#tbl_' + testCapabilityGridId + ' tbody').html(
            '<tr><td colspan="100" class="govuk-table__cell" ' +
            'style="text-align:center;color:#505a5f;font-style:italic;padding:16px;">' +
            'Please select a WorkGroup to view test capabilities.</td></tr>'
        );
    }

    // ── Navigation ────────────────────────────────────────────────────────
    function navigateToTestCapability() {
        var workgroup = currentWorkGroup || $('#workGroupSelect').val();
        if (workgroup) {
            window.fpsNavigateTo('/PACT/TestCapability?workgroup=' + encodeURIComponent(workgroup));
        } else {
            window.fpsNavigateTo('/PACT/TestCapability');
        }
    }

    // ── Extra Filter Method (for pagination/sorting) ──────────────────────
    function getTestCapabilityExtraFilters() {
        return {
            workGroup: currentWorkGroup || ''
        };
    }

    // ── Row Selection Handler ─────────────────────────────────────────────
    function onTestCapabilityRowSelect(rowData) {
        // Extract portfolio value from the selected row
        var portfolio = $(rowData).closest('tr').find('[data-property="PlanPortfolio"]').text().trim();

        // Update the portfolio input field
        if (portfolio) {
            $('#selectedPortfolio').val(portfolio);
            // Enable the Project Administration button
            $('#btnShowProjectAdministration').prop('disabled', false);
        } else {
            // Disable the button if no portfolio selected
            $('#selectedPortfolio').val('');
            $('#btnShowProjectAdministration').prop('disabled', true);
        }
    }

    // ── Navigate to Portfolio Maintenance ─────────────────────────────────
    function navigateToPortfolioMaintenance() {
        var portfolio = $('#selectedPortfolio').val();
        if (!portfolio) {
            showAlertMessage('Please select a test capability row first.', AlertType.INFO);
            return;
        }

        // Get the current workgroup to pass as context for back navigation
        var workgroup = currentWorkGroup || $('#workGroupSelect').val();

        // Navigate to Portfolio Maintenance with selected portfolio and workgroup context
        var url = '/PACT/PortfolioMaintenance?portfolio=' + encodeURIComponent(portfolio);
        if (workgroup) {
            url += '&workgroup=' + encodeURIComponent(workgroup);
        }
        window.fpsNavigateTo(url);
    }

    // ── Public API ────────────────────────────────────────────────────────
    window.WorkGroupTestCapability = {
        initialize: initialize,
        onWorkGroupChange: onWorkGroupChange,
        reloadTestCapabilityGrid: reloadTestCapabilityGrid,
        clearTestCapabilityGrid: clearTestCapabilityGrid,
        navigateToTestCapability: navigateToTestCapability,
        navigateToPortfolioMaintenance: navigateToPortfolioMaintenance,
        getTestCapabilityExtraFilters: getTestCapabilityExtraFilters,
        onTestCapabilityRowSelect: onTestCapabilityRowSelect,
        getCapabilityGridManager: getCapabilityGridManager
    };

    // Expose individual functions to global scope for backward compatibility
    window.onWorkGroupChange = onWorkGroupChange;
    window.reloadTestCapabilityGrid = reloadTestCapabilityGrid;
    window.clearTestCapabilityGrid = clearTestCapabilityGrid;
    window.navigateToTestCapability = navigateToTestCapability;
    window.navigateToPortfolioMaintenance = navigateToPortfolioMaintenance;
    window.getTestCapabilityExtraFilters = getTestCapabilityExtraFilters;
    window.onTestCapabilityRowSelect = onTestCapabilityRowSelect;
    window.getCapabilityGridManager = getCapabilityGridManager;

})();
