(function (cfg) {
    'use strict';

    /* ── Module-level state ─────────────────────────────────────────── */
    let currentCentre    = cfg.currentCentre    || '';
    let currentWorkGroup = cfg.currentWorkGroup || '';
    let currentGrade     = cfg.currentGrade     || '';

    /* ── Utility helpers ────────────────────────────────────────────── */
    function el(id) { return document.getElementById(id); }

    function setVal(id, value) {
        const input = el(id);
        if (input) input.value = value;
    }

    /* ── Resource-centre cascade ────────────────────────────────────── */
    function LoadGroupsByResourceCentre() {
        const sel = el('resourceCentreSelect');
        const nameEl = el('ssrSelectedCentreName');
        currentCentre = sel ? sel.value : '';

        if (!currentCentre) {
            if (nameEl) nameEl.textContent = '';
            currentGrade = '';
            const list = el('ssrGradeList');
            if (list) list.innerHTML = '';
            ssrClearAll();
            return;
        }
        
        if (nameEl) nameEl.textContent = currentCentre;
        currentWorkGroup = '';
        currentGrade = '';
        bindGroupDropdownList([]);
        showLoader();
        $.get('/FPS/SetUpStaffResources/GetGroupsByResourceCentre',
            { resourceCentre: currentCentre },
            function (response) {
                hideLoader();
                if (response && response.success) {
                    bindGroupDropdownList(response.data);
                } else {
                    showAlertMessage('Could not load work groups for \'' + currentCentre + '\': ' + (response && response.message || 'Unknown error.'), AlertType.INFO);
                }
            }
        ).fail(function () {
            hideLoader();
            showAlertMessage('Could not load work groups for \'' + currentCentre + '\'. Please try selecting the Resource Centre again.', AlertType.INFO);
        });
    }

    function bindGroupDropdownList(workgroups) {
        const select = el('workGroupSelect');
        if (!select) return;

        select.innerHTML = '<option value="">-- Select a Work Group --</option>';
        (workgroups || []).forEach(function (wg) {
            const opt = document.createElement('option');
            opt.value = wg;
            opt.textContent = wg;
            select.appendChild(opt);
        });
    }

    /* ── WorkGroup → Grade cascade ──────────────────────────────────── */
    function LoadGradeByGroup(restoreGrade) {
        const sel = el('workGroupSelect');
        const selectedGroup = sel ? sel.value : '';
        currentWorkGroup = selectedGroup;

        if (!selectedGroup) {
            bindGradeList([], null);
            return;
        }
        showLoader();
        $.get('/FPS/SetUpStaffResources/GetGradesByGroups',
            { workGroup: selectedGroup },
            function (response) {
                hideLoader();
                if (response && response.success) {
                    bindGradeList(response.data, restoreGrade || null);
                } else {
                    showAlertMessage('Could not load grades for work group \'' + selectedGroup + '\': ' + (response && response.message || 'Unknown error.'), AlertType.INFO);
                }
            }
        ).fail(function () {
            hideLoader();
            showAlertMessage('Could not load grades for work group \'' + selectedGroup + '\'. Please try selecting the work group again.', AlertType.INFO);
        });
    }

    /**
     * Populate the grade listbox.
     * @param {Array<{wgGrade:string, gradeCode:string}>} grades
     * @param {string|null} restoreGrade  When non-null, activate this grade instead of the first.
     */
    function bindGradeList(grades, restoreGrade) {
        const list = el('ssrGradeList');
        if (!list) return;

        list.innerHTML = '';

        (grades || []).forEach(function (item) {
            const wg = (typeof item === 'object') ? (item.wgGrade || '') : item;
            const gradeCode = (typeof item === 'object') ? (item.gradeCode || '') : '';

            const li = document.createElement('li');
            li.className = 'ssr-grade-item';
            li.textContent = wg;
            li.setAttribute('role', 'option');
            li.setAttribute('aria-selected', 'false');
            li.setAttribute('data-grade-code', gradeCode);
            li.addEventListener('click', function () { ssrSelectWorkGroup(wg); });
            list.appendChild(li);
        });

        if (grades && grades.length > 0) {
            const allGrades = grades.map(function (g) { return typeof g === 'object' ? g.wgGrade : g; });
            const target    = (restoreGrade && allGrades.includes(restoreGrade))
                ? restoreGrade
                : allGrades[0];
            ssrSelectWorkGroup(target);
        } else {
            currentGrade = '';
            ssrClearAll();
            reloadStaffGrid();
        }
    }

    /* ── Grade selection ────────────────────────────────────────────── */
    function ssrSelectWorkGroup(wg) {
        currentGrade = wg;

        // Highlight active grade item
        document.querySelectorAll('#ssrGradeList .ssr-grade-item').forEach(function (li) {
            const active = li.textContent.trim() === wg;
            li.classList.toggle('ssr-grade-item--active', active);
            li.setAttribute('aria-selected', active ? 'true' : 'false');
        });

        // Clear person selection; keep grade/workhrs until the AJAX result arrives
        ssrClearPersonSelection();

        // Fetch GradeCode + total AtWork for the selected grade
        if (wg) {
            refreshGradeStats(wg);
        } else {
            setVal('ssrSummaryGrade', '');
            setVal('ssrWorkHrs', '0');
        }

        reloadStaffGrid();
    }

    /**
     * Fetch GradeCode and total AtWork hours for a WgGrade and populate the summary inputs.
     * Also called after a successful save to keep the total in sync.
     */
    function refreshGradeStats(wg) {
        if (!wg) return;

        showLoader();
        $.get('/FPS/SetUpStaffResources/GetGradeStats', { wgGrade: wg }, function (data) {
            hideLoader();
            if (data && data.success) {
                setVal('ssrSummaryGrade', data.gradeCode || '');
                setVal('ssrWorkHrs',      data.totalAtWork != null ? data.totalAtWork : '0');
            }
        }).fail(function () {
            hideLoader();
            showAlertMessage('Could not load grade summary for: ' + wg + '. Please try selecting the grade again.', AlertType.INFO);
        });
    }

    /* ── Staff grid ─────────────────────────────────────────────────── */
    function reloadStaffGrid() {
        const gm = window['gridManager_ssrStaffGrid'];
        if (gm) gm.reloadGrid({ page: 1 });
    }

    /** Called by the DataGrid component as an extra-filter source. */
    function ssrGetStaffExtraFilters() {
        return { wgGrade: currentGrade || '' };
    }

    /* ── Staff row selection ────────────────────────────────────────── */
    /**
     * Called when a staff grid row is clicked.
     * Updates the Person Selected box only — does NOT override the Summary Grade,
     * which is driven solely by grade-list selection.
     */
    function ssrOnStaffRowSelect(row) {
        const id = $(row).data('id') || '';
        const name = $(row).find('td[data-property="Name"] span').text().trim();

        setVal('ssrPersonSelected', name);
        setVal('ssrSelectedPersonId', id);
    }

    function ssrSelectFirstStaffRow() {
        const $first = $('#gridContainer_ssrStaffGrid table tbody tr.selectable-row:first');
        if ($first.length && $first.data('id')) {
            $('#gridContainer_ssrStaffGrid table tbody tr').removeClass('selected-row');
            $first.addClass('selected-row');
            ssrOnStaffRowSelect($first[0]);
        }
    }

    /* ── Edit modal ─────────────────────────────────────────────────── */
    function editSsrStaff(btn) {
        const id = $(btn).data('id');
        showLoader();
        $.get('/FPS/SetUpStaffResources/Edit', { pactId: id }, function (html) {
            hideLoader();
            $('#modaPopupBody').html(html);
            $('#modalPopup').addClass('show');
        }).fail(function () {
            hideLoader();
            showAlertMessage('Failed to load edit form. Please try again.', AlertType.ERROR);
        });
    }

    function saveSetUpStaffResources() {
        const pactId = el('hdnPactId')?.value || '';
        const name = el('ssrEditName')?.value || '';
        const hrsPaid = parseFloat(el('ssrEditHrsPaid')?.value) || 0;
        const leave = parseFloat(el('ssrEditLeave')?.value) || 0;
        const sickSp = parseFloat(el('ssrEditSickSp')?.value) || 0;
        const planable = el('ssrEditPlanable')?.checked ? 1 : 0;

        if (!pactId) {
            showAlertMessage('Cannot save: staff record ID is missing.', AlertType.INFO);
            return;
        }

        const aftInput = document.querySelector('#ssrEditForm input[name="__RequestVerificationToken"]');
        const aft = aftInput ? aftInput.value : '';

        if (!aft) {
            showAlertMessage('Security token missing. Please refresh the page and try again.', AlertType.INFO);
            return;
        }

        // Send only the editable subset — controller fetches and patches the full record server-side.
        const dto = {
            PactId: pactId,
            Name: name,
            HrsPaid: hrsPaid,
            Leave: leave,
            SickSpecial: sickSp,
            HrsAvail: parseFloat((hrsPaid - leave - sickSp).toFixed(2)),
            MakeAvailable: planable
        };
        showLoader();
        $.ajax({
            url: '/FPS/SetUpStaffResources/Edit',
            type: 'POST',
            data: JSON.stringify(dto),
            contentType: 'application/json; charset=utf-8',
            headers: { 'RequestVerificationToken': aft },
            success: function (data) {
                hideLoader();
                if (data.success) {
                    showAlertMessage('Staff resource saved successfully.', AlertType.SUCCESS);
                    closeModal();
                    reloadStaffGrid();
                    refreshGradeStats(currentGrade);
                } else {
                    showAlertMessage('Save failed: ' + (data.message || 'Unknown error.'), AlertType.ERROR);
                }
            },
            error: function (xhr) {
                hideLoader();
                if (xhr.status === 400) {
                    showAlertMessage('Validation error. Please check the form and try again.', AlertType.ERROR);
                } else {
                    showAlertMessage('An error occurred while saving. Please try again.', AlertType.ERROR);
                }
            }
        });
    }

    function closeModal() {
        $('#modaPopupBody').html('');
        $('#modalPopup').removeClass('show');
    }

    /* ── Navigate to ZT codes ───────────────────────────────────────── */
    function ssrPlanPersonOntoZT() {
        const person = el('ssrPersonSelected');
        const idEl = el('ssrSelectedPersonId');

        if (!person || !person.value) {
            showAlertMessage('Please select a person first.', AlertType.INFO);
            return;
        }

        // Persist current selections so they are restored when the user clicks Back
        try {
            sessionStorage.setItem('ssrReturnState', JSON.stringify({
                centre:    currentCentre,
                workGroup: currentWorkGroup,
                grade:     currentGrade
            }));
        } catch (e) {
            showAlertMessage('Error, The page will still open but your previous selection may not be restored on return.', AlertType.INFO);
        }

        let url = cfg.ztCodeUrl;
        const id = idEl ? idEl.value : '';
        if (id) url += '?staffId=' + encodeURIComponent(id) + '&source=ssr';
        window.location.href = url;
    }

    /* ── Clear helpers ──────────────────────────────────────────────── */
    function ssrClearPersonSelection() {
        setVal('ssrPersonSelected', '');
        setVal('ssrSelectedPersonId', '');
    }

    function ssrClearAll() {
        setVal('ssrSummaryGrade', '');
        setVal('ssrWorkHrs', '0');
        ssrClearPersonSelection();
    }

    /* ── Back-navigation state restore ─────────────────────────────── */
    /**
     * Restore Resource Centre → WorkGroup → Grade cascade from sessionStorage.
     * Triggered on pageshow so it also fires after a bfcache restore.
     */
    function restoreSelectionState() {
        let saved;
        try {
            const raw = sessionStorage.getItem('ssrReturnState');
            if (!raw) return;
            saved = JSON.parse(raw);
            sessionStorage.removeItem('ssrReturnState'); // consume immediately
        } catch (e) {
            showAlertMessage('Error, Please re-select a Resource Centre and Grade.', AlertType.INFO);
            return;
        }

        if (!saved || !saved.centre) return;

        // ── 1. Set Resource Centre dropdown ─────────────────────────────
        const rcSelect = el('resourceCentreSelect');
        if (!rcSelect) return;

        // If the <option> for this centre does not exist the user lost access — bail
        const targetOption = Array.from(rcSelect.options).find(function (o) { return o.value === saved.centre; });
        if (!targetOption) return;

        rcSelect.value = saved.centre;
        currentCentre = saved.centre;

        const nameEl = el('ssrSelectedCentreName');
        if (nameEl) nameEl.textContent = saved.centre;

        showLoader();
        // ── 2. Load workgroups for this centre, then restore workgroup + grade ─
        $.get('/FPS/SetUpStaffResources/GetGroupsByResourceCentre',
            { resourceCentre: saved.centre },
            function (response) {
                
                if (!response || !response.success) return;

                bindGroupDropdownList(response.data);

                if (!saved.workGroup) return;

                const wgSelect = el('workGroupSelect');
                if (!wgSelect) return;

                // Find the matching workgroup option
                const wgOption = Array.from(wgSelect.options).find(function (o) { return o.value === saved.workGroup; });
                if (!wgOption) return;

                wgSelect.value = saved.workGroup;
                currentWorkGroup = saved.workGroup;

                // ── 3. Load grades for the restored workgroup, activate saved grade ─
                $.get('/FPS/SetUpStaffResources/GetGradesByGroups',
                    { workGroup: saved.workGroup },
                    function (gradeResponse) {
                        hideLoader();
                        if (gradeResponse && gradeResponse.success) {
                            bindGradeList(gradeResponse.data, saved.grade || null);
                        }
                    }
                ).fail(function () {
                    hideLoader();
                    showAlertMessage('Could not restore grade list for work group \'' + saved.workGroup + '\'. Please re-select manually.', AlertType.INFO);
                });
            }
        ).fail(function () {
            hideLoader();
            showAlertMessage('Could not restore work groups for \'' + saved.centre + '\'. Please re-select the Resource Centre manually.', AlertType.INFO);
        });
    }

    // pageshow fires on both normal page load and bfcache restore (persisted === true)
    window.addEventListener('pageshow', function () {
        restoreSelectionState();
    });

    /* ── Page load initialization ──────────────────────────────────────────── */
    // When the page loads with a selected workgroup (from side nav query string),
    // trigger grade list loading.
    function initializePageLoadState() {
        const wgSelect = el('workGroupSelect');
        const rcSelect = el('resourceCentreSelect');
         // If both resource centre and workgroup are selected on page load (from query string),
        // the grades are already server-rendered in the HTML
        if (rcSelect && rcSelect.value && wgSelect && wgSelect.value) {
            currentCentre = rcSelect.value;
            currentWorkGroup = wgSelect.value;

            // Update the centre name display
            const nameEl = el('ssrSelectedCentreName');
            if (nameEl) nameEl.textContent = currentCentre;

            // Check if grades are already rendered in the HTML (server-side)
            const gradeList = el('ssrGradeList');
            if (gradeList && gradeList.children.length > 0) {
                // Grades are server-rendered, trigger selection of the first grade
                const firstGradeItem = gradeList.children[0];
                if (firstGradeItem) {
                    const firstGrade = firstGradeItem.textContent.trim();
                    ssrSelectWorkGroup(firstGrade);
                }
            } else {
                // No grades rendered, load them via AJAX
                LoadGradeByGroup();
            }
        }
    }

    // Execute immediately if DOM is already loaded, otherwise wait for DOMContentLoaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializePageLoadState);
    } else {
        // DOM is already loaded, execute immediately
        initializePageLoadState();
    }

    /* ── Grid-reloaded event ────────────────────────────────────────── */
    document.addEventListener('gridReloaded', function (e) {
        if (e.detail && e.detail.gridId === 'ssrStaffGrid') {
            ssrSelectFirstStaffRow();
        }
    });

    /* ── Expose functions required by Razor-rendered HTML handlers ─── */
    window.LoadGroupsByResourceCentre = LoadGroupsByResourceCentre;
    window.LoadGradeByGroup = LoadGradeByGroup;
    window.ssrSelectWorkGroup = ssrSelectWorkGroup;
    window.ssrGetStaffExtraFilters = ssrGetStaffExtraFilters;
    window.ssrOnStaffRowSelect = ssrOnStaffRowSelect;
    window.editSsrStaff = editSsrStaff;
    window.saveSetUpStaffResources = saveSetUpStaffResources;
    window.closeModal = closeModal;
    window.ssrPlanPersonOntoZT = ssrPlanPersonOntoZT;

}(window.ssrConfig || {}));
