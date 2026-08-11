// project-dropdown.js
// Shared project multi-column dropdown logic for PlanActual pages.
// Call initProjectDropdown(lookupUrl, onSelectCallback) inside $(document).ready.
//
// Parameters:
//   lookupUrl        – URL for fetching project list JSON (GetProjectLookup endpoint).
//   onSelectCallback – function(projectCode, projectTitle) called when a row is picked.

function initProjectDropdown(lookupUrl, onSelectCallback) {
    var allProjects = [];
    var display = document.getElementById('projectNameInput');
    var panel   = document.getElementById('ProjectDropdownPanel');
    var search  = document.getElementById('ProjectSearchBox');
    var body    = document.getElementById('ProjectDropdownBody');

    if (!display || !panel || !search || !body) return;

    function positionPanel() {
        var r = display.getBoundingClientRect();
        panel.style.top  = r.bottom + 'px';
        panel.style.left = Math.max(0, r.right - 420) + 'px';
    }

    function renderRows(rows) {
        body.innerHTML = '';
        rows.forEach(function (item) {
            var tr = document.createElement('tr');
            tr.style.cursor = 'pointer';
            tr.innerHTML =
                '<td style="padding:6px 8px;border-bottom:1px solid #f3f2f1">' + item.parentProject + '</td>' +
                '<td style="padding:6px 8px;border-bottom:1px solid #f3f2f1">' + item.projectTitle  + '</td>';
            tr.addEventListener('click', function () {
                panel.style.display = 'none';
                onSelectCallback(item.parentProject, item.projectTitle);
            });
            tr.addEventListener('mouseenter', function () { this.style.backgroundColor = '#f3f2f1'; });
            tr.addEventListener('mouseleave', function () { this.style.backgroundColor = ''; });
            body.appendChild(tr);
        });
    }

    function openPanel() {
        if (allProjects.length === 0) {
            fetch(lookupUrl)
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    allProjects = data;
                    search.value = '';
                    renderRows(allProjects);
                    positionPanel();
                    panel.style.display = 'block';
                    search.focus();
                })
                .catch(function (err) { console.error('Project lookup failed', err); });
        } else {
            search.value = '';
            renderRows(allProjects);
            positionPanel();
            panel.style.display = 'block';
            search.focus();
        }
    }

    display.addEventListener('click', function (e) {
        e.stopPropagation();
        if (panel.style.display !== 'none') { panel.style.display = 'none'; return; }
        openPanel();
    });

    search.addEventListener('input', function () {
        var t = this.value.toLowerCase();
        renderRows(t
            ? allProjects.filter(function (p) {
                return p.parentProject.toLowerCase().includes(t) || p.projectTitle.toLowerCase().includes(t);
              })
            : allProjects);
    });

    document.addEventListener('click', function (e) {
        if (!display.contains(e.target) && !panel.contains(e.target)) {
            panel.style.display = 'none';
        }
    });
}
