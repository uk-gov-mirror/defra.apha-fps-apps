/* ── Shared helpers (used by both modules below) ────────────────────────── */
function buildSelectOptions(select, items, valueFn, textFn) {
    const frag = document.createDocumentFragment();
    (items || []).forEach(item => {
        const opt = document.createElement('option');
        opt.value       = valueFn(item);
        opt.textContent = textFn(item);
        frag.appendChild(opt);
    });
    select.appendChild(frag);
}

function resetSelect(select, placeholder) {
    if (select) select.innerHTML = `<option value="">${placeholder}</option>`;
}

function ajaxGet(url, params) {
    return new Promise((resolve, reject) => $.get(url, params, resolve).fail(reject));
}

function ajaxPost(url, params) {
    return new Promise((resolve, reject) => $.post(url, params, resolve).fail(reject));
}


/* ── SSR cascade (workgroup / grade dropdowns) ──────────────────────────── */
(function () {
    'use strict';

    const el = id => document.getElementById(id);

    const WG_PLACEHOLDER    = '-- Select a Work Group --';
    const GRADE_PLACEHOLDER = '-- Select a WorkGroup Grade--';

    const WG_URL    = '/FPS/SetUpStaffResources/GetGroupsByResourceCentre';
    const GRADE_URL = '/FPS/SetUpStaffResources/GetGradesByGroups';

    function LoadGroupsByResourceCentre() {
        const centre = el('resourceCentreSelect')?.value || '';
        const nameEl = el('ssrSelectedCentreName');

        if (!centre) {
            if (nameEl) nameEl.textContent = '';
            const list = el('ssrGradeList');
            if (list) list.innerHTML = '';
            ssrClearAll();
            return;
        }

        if (nameEl) nameEl.textContent = centre;
        resetSelect(el('workGroupSelect'), WG_PLACEHOLDER);
        showLoader();

        $.get(WG_URL, { resourceCentre: centre })
            .done(r => {
                hideLoader();
                if (r?.success) {
                    buildSelectOptions(el('workGroupSelect'), r.data, wg => wg, wg => wg);
                } else {
                    showAlertMessage(`Could not load work groups for '${centre}': ${r?.message || 'Unknown error.'}`, AlertType.ERROR);
                }
            })
            .fail(() => {
                hideLoader();
                showAlertMessage(`Could not load work groups for '${centre}'. Please try selecting the Resource Centre again.`, AlertType.ERROR);
            });
    }

    function LoadGradeByGroup() {
        const selectedGroup = el('workGroupSelect')?.value || '';

        if (!selectedGroup) {
            resetSelect(el('workGroupGradeSelect'), GRADE_PLACEHOLDER);
            return;
        }

        showLoader();
        $.get(GRADE_URL, { workGroup: selectedGroup })
            .done(r => {
                hideLoader();
                if (r?.success) {
                    resetSelect(el('workGroupGradeSelect'), GRADE_PLACEHOLDER);
                    buildSelectOptions(el('workGroupGradeSelect'), r.data, wg => wg.wgGrade, wg => wg.wgGrade);
                } else {
                    showAlertMessage(`Could not load grades for work group '${selectedGroup}': ${r?.message || 'Unknown error.'}`, AlertType.ERROR);
                }
            })
            .fail(() => {
                hideLoader();
                showAlertMessage(`Could not load grades for work group '${selectedGroup}'. Please try selecting the work group again.`, AlertType.ERROR);
            });
    }

    window.LoadGroupsByResourceCentre = LoadGroupsByResourceCentre;
    window.LoadGradeByGroup           = LoadGradeByGroup;

}());


/* ── Resource Allocation (Stage 2) module ───────────────────────────────── */
(function (cfg) {
    'use strict';

    const { gradesUrl = '', staffGridUrl = '', jobsGridUrl = '', totalsUrl = '', ztCodeUrl = '' } = cfg;

    let currentGrade = '';
    let currentStaffId = '';

    const el = id => document.getElementById(id);
    const setText = (id, v) => { const e = el(id); if (e) e.textContent = v; };
    const setVal = (id, v) => { const e = el(id); if (e) e.value = v; };

    const GRADE_PLACEHOLDER = '-- Select a WorkGroup Grade--';
    const WG_URL = '/FPS/SetUpStaffResources/GetGroupsByResourceCentre';
    const GRADE_URL = '/FPS/SetUpStaffResources/GetGradesByGroups';

    // Rebuild the "Staff of this Grade" totals row after the grid markup is
    // replaced directly (grade change / state restore / clear). Grid reloads
    // triggered by paging or filtering are handled by the gridReloaded event.
    function refreshStaffTotalsRow() {
        if (typeof window.buildRaStaffTotalsRow === 'function') {
            window.buildRaStaffTotalsRow();
        }
    }

    /* ── Resource Centre change ─────────────────────────────────────────── */
    async function OnResourceCenterChange() {
        const centre = el('resourceCentreSelect').value;
        const gradeSelect = el('workGroupGradeSelect');

        resetSelect(gradeSelect, '-- Select a Workgroup Grade --');
        gradeSelect.disabled = true;
        clearGrids();

        if (!centre) return;

        try {
            const resp = await fetch(`${gradesUrl}?resourceCentre=${encodeURIComponent(centre)}`);
            const json = await resp.json();
            if (json.success && json.data?.length) {
                buildSelectOptions(gradeSelect, json.data, g => g.value, g => g.text);
                gradeSelect.disabled = false;
            }
        } catch {
            showAlertMessage('Error loading workgroup grades.', AlertType.ERROR);
        }
    }

    /* ── Grade change → load staff allocation grid ──────────────────────── */
    async function WorkgroupGradeChange() {
        const grade = el('workGroupGradeSelect').value;
        const group = el('workGroupSelect').value;

        setText('stage2SelectedWorkGroupGrade', grade);
        setText('stage2SelectedWorkGroup', group);
        clearJobsGrid();

        if (!grade) { clearStaffGrid(); return; }

        currentGrade = grade;
        currentStaffId = '';

        showLoader();
        try {
            const html = await ajaxPost(staffGridUrl, { workGroupGrade: grade, page: 1, pageSize: 10 });
            el('gridContainer_StaffAllocationGrid').innerHTML = html;
            refreshStaffTotalsRow();
            SelectFirstStaffRow();
            await loadStaffAllocationTotals(grade);
        } catch {
            hideLoader();
            showAlertMessage('Error loading staff allocation grid.', AlertType.ERROR);
        }
    }

    /* ── Load grade-level totals into the summary panel ─────────────────── */
    async function loadStaffAllocationTotals(grade) {
        if (!totalsUrl || !grade) { clearSummaryPanel(); return; }

        try {
            const data = await ajaxGet(totalsUrl, { workGroupGrade: grade });
            if (data?.success) {
                setVal('stage2HoursAvailInput', data.hrsAvail);
                setVal('stage2PlannedHrsInput', data.plannedHrs);
                setVal('stage2AllocationPctInput', data.allocationPct);
                setVal('stage2AssuredChargeInput', data.assuredChargeHrs);
                setVal('stage2AssuredUtilInput', data.assuredUtilPct);
                setVal('stage2TotalChargeInput', data.totalChargeHrs);
                setVal('stage2TotalUtilInput', data.totalUtilPct);
            } else {
                clearSummaryPanel();
            }
        } catch {
            clearSummaryPanel();
        } finally {
            hideLoader();
        }
    }

    function clearSummaryPanel() {
        ['stage2HoursAvailInput', 'stage2PlannedHrsInput', 'stage2AllocationPctInput',
            'stage2AssuredChargeInput', 'stage2AssuredUtilInput',
            'stage2TotalChargeInput', 'stage2TotalUtilInput'].forEach(id => setVal(id, ''));
    }

    /* ── Staff row selection → load jobs grid ───────────────────────────── */
    async function OnStaffRowSelect(rowData) {
        if (!rowData) return;

        const $row = $(rowData);
        const staffId = $row.data('id');
        const staffName = cellText($row, 'Name');
        const planHrs = cellText($row, 'PlannedHours');

        setText('stage2SelectedStaffName', staffName);
        setVal('stage2PersonSelectedInput', staffName);
        setVal('stage2SelectedStaffHoursInput', planHrs);

        if (!staffId) return;
        currentStaffId = staffId;

        showLoader();
        try {
            const html = await ajaxPost(jobsGridUrl, { staffId, page: 1, pageSize: 10 });
            el('gridContainer_StaffJobsGrid').innerHTML = html;
        } catch {
            showAlertMessage('Error loading staff jobs grid.', AlertType.ERROR);
        } finally {
            hideLoader();
        }
    }

    function cellText($row, property) {
        return $row
            .find(`td[data-property="${property}"] span, td[data-property="${property}"]`)
            .first().text().trim();
    }

    /* ── Auto-select first staff row after grid reload ──────────────────── */
    function SelectFirstStaffRow() {
        const $first = $('#gridContainer_StaffAllocationGrid table tbody tr.selectable-row:first');
        if ($first.length && $first.data('id')) {
            $('#gridContainer_StaffAllocationGrid table tbody tr').removeClass('selected-row');
            $first.addClass('selected-row');
            OnStaffRowSelect($first[0]);
        }
    }

    /* ── Grid clear helpers ─────────────────────────────────────────────── */
    function clearStaffGrid() {
        $.post(staffGridUrl, { workGroupGrade: '' }, html => {
            el('gridContainer_StaffAllocationGrid').innerHTML = html;
            refreshStaffTotalsRow();
        });
        clearSummaryPanel();
    }

    function clearJobsGrid() {
        $.post(jobsGridUrl, { staffId: '' }, html => {
            el('gridContainer_StaffJobsGrid').innerHTML = html;
        });
        setVal('stage2PersonSelectedInput', '');
        setText('stage2SelectedStaffName', '');
        setVal('stage2SelectedStaffHoursInput', '');
    }

    function clearGrids() {
        clearStaffGrid();
        clearJobsGrid();
    }

    /* ── ExtraFilter callbacks (used by DataGrid reloadGrid) ────────────── */
    const GetStaffAllocationExtraFilters = () => ({ workGroupGrade: currentGrade });
    const GetStaffJobsExtraFilters = () => ({ staffId: currentStaffId });

    /* ── Navigate to PlanStaffZTCode ────────────────────────────────────── */
    function ssrPlanPersonOntoZT() {
        if (!el('stage2PersonSelectedInput')?.value) {
            showAlertMessage('Please select a person first.', AlertType.INFO);
            return;
        }

        try {
            sessionStorage.setItem('raReturnState', JSON.stringify({
                centre: el('resourceCentreSelect')?.value || '',
                workGroup: el('workGroupSelect')?.value || '',
                grade: currentGrade,
                staffId: currentStaffId
            }));
        } catch { /* non-critical — page still navigates */ }

        const url = currentStaffId
            ? `${ztCodeUrl}?staffId=${encodeURIComponent(currentStaffId)}&source=ra`
            : ztCodeUrl;
        window.location.href = url;
    }

    /* ── Restore state saved before navigating to PlanStaffZTCode ────────── */
    async function restoreReturnState() {
        let saved;
        try {
            const raw = sessionStorage.getItem('raReturnState');
            if (!raw) return;
            sessionStorage.removeItem('raReturnState');
            saved = JSON.parse(raw);
        } catch { return; }

        if (!saved?.centre) return;

        const rcSelect = el('resourceCentreSelect');
        if (!rcSelect || !Array.from(rcSelect.options).some(o => o.value === saved.centre)) return;
        rcSelect.value = saved.centre;

        showLoader();
        try {
            // ── 1. Restore workgroup dropdown ─────────────────────────────
            const wgResp = await ajaxGet(WG_URL, { resourceCentre: saved.centre });
            if (!wgResp?.success) return;

            const wgSelect = el('workGroupSelect');
            if (!wgSelect) return;
            resetSelect(wgSelect, '-- Select a Work Group --');
            buildSelectOptions(wgSelect, wgResp.data, wg => wg, wg => wg);

            if (!saved.workGroup || !Array.from(wgSelect.options).some(o => o.value === saved.workGroup)) return;
            wgSelect.value = saved.workGroup;

            // ── 2. Restore grade dropdown ─────────────────────────────────
            const grResp = await ajaxGet(GRADE_URL, { workGroup: saved.workGroup });
            if (!grResp?.success) return;

            const gradeSelect = el('workGroupGradeSelect');
            if (!gradeSelect) return;
            resetSelect(gradeSelect, GRADE_PLACEHOLDER);
            buildSelectOptions(gradeSelect, grResp.data, wg => wg.wgGrade, wg => wg.wgGrade);
            gradeSelect.disabled = false;

            if (!saved.grade || !Array.from(gradeSelect.options).some(o => o.value === saved.grade)) return;
            gradeSelect.value = saved.grade;
            currentGrade = saved.grade;

            // ── 3. Restore heading labels ─────────────────────────────────
            setText('stage2SelectedWorkGroup', saved.workGroup);
            setText('stage2SelectedWorkGroupGrade', saved.grade);
            clearJobsGrid();

            // ── 4. Reload staff grid and re-select saved staff row ────────
            const html = await ajaxPost(staffGridUrl, { workGroupGrade: saved.grade, page: 1, pageSize: 10 });
            el('gridContainer_StaffAllocationGrid').innerHTML = html;
            refreshStaffTotalsRow();
            await loadStaffAllocationTotals(saved.grade);

            if (saved.staffId) {
                currentStaffId = saved.staffId;
                const $savedRow = $(`#gridContainer_StaffAllocationGrid table tbody tr[data-id="${saved.staffId}"]`);
                if ($savedRow.length) {
                    $('#gridContainer_StaffAllocationGrid table tbody tr').removeClass('selected-row');
                    $savedRow.addClass('selected-row');
                    await OnStaffRowSelect($savedRow[0]);
                    return;
                }
            }
            SelectFirstStaffRow();
        } catch {
            showAlertMessage('Could not restore selections. Please re-select the Resource Centre manually.', AlertType.INFO);
        } finally {
            hideLoader();
        }
    }

    // pageshow fires on both normal page load and bfcache restore
    window.addEventListener('pageshow', restoreReturnState);

    // Page load initialization: When the page loads with selected resource centre and workgroup
    // (from side nav query string), trigger grade dropdown population.
    function initializePageLoadState() {
        const rcSelect = el('resourceCentreSelect');
        const wgSelect = el('workGroupSelect');

        // If both resource centre and workgroup are selected on page load,
        // trigger the grade dropdown loading via LoadGradeByGroup
        if (rcSelect && rcSelect.value && wgSelect && wgSelect.value) {
            // Update the display elements
            setText('stage2SelectedWorkGroup', wgSelect.value);

            // Trigger grade dropdown population (from the SSR cascade module)
            // Use setTimeout to ensure the SSR cascade module has fully initialized
            setTimeout(function() {
                if (typeof window.LoadGradeByGroup === 'function') {
                    window.LoadGradeByGroup();
                } else {
                    console.error('window.LoadGradeByGroup is not defined');
                }
            }, 100);
        }
    }

    // Execute immediately if DOM is already loaded, otherwise wait for DOMContentLoaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializePageLoadState);
    } else {
        // DOM is already loaded, execute immediately
        initializePageLoadState();
    }

    /* ── Public API ─────────────────────────────────────────────────────── */
    Object.assign(window, {
        OnResourceCenterChange,
        WorkgroupGradeChange,
        GetStaffAllocationExtraFilters,
        GetStaffJobsExtraFilters,
        OnStaffRowSelect,
        SelectFirstStaffRow,
        clearJobsGrid,
        clearStaffGrid,
        clearGrids,
        ssrPlanPersonOntoZT
    });

}(window.raConfig || {}));
