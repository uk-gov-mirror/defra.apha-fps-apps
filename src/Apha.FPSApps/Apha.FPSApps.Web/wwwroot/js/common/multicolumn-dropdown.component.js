/**
 * Reusable MultiColumnDropdown Component
 * Version: 1.0
 * 
 * Usage:
 * var dropdown = new MultiColumnDropdownComponent({
 *     dropdownId: 'myDropdown',
 *     containerSelector: '#dropdownContainer',
 *     placeholder: 'Select an option',
 *     searchPlaceholder: 'Type to search',
 *     columns: [
 *         { field: 'id', header: 'ID', width: '80px' },
 *         { field: 'name', header: 'Name', width: '200px' }
 *     ],
 *     data: [...],
 *     displayField: 'name',
 *     valueField: 'id',
 *     enableSearch: true,
 *     callbacks: {
 *         onSelect: function(selectedItem) { ... }
 *     }
 * });
 */

(function(window) {
    'use strict';

    // Global z-index counter for managing dropdown stacking
    var globalZIndex = 10000;

    /**
     * MultiColumnDropdownComponent Constructor
     * @param {Object} config - Configuration object
     */
    function MultiColumnDropdownComponent(config) {
        // Default configuration
        this.config = Object.assign({
            dropdownId: 'multiColumnDropdown',
            containerSelector: '#dropdownContainer',
            placeholder: 'Select an option',
            searchPlaceholder: 'Type to search',
            columns: [],
            data: [],
            displayField: 'name',
            valueField: 'id',
            enableSearch: true,
            showSerialNumber: true,
            labelText: '',
            required: false,
            disabled: false,
            callbacks: {
                onSelect: null,
                onChange: null,
                onClear: null
            }
        }, config);

        this.originalData = [...this.config.data];
        this.filteredData = [...this.config.data];
        this.selectedValue = null;
        this.selectedItem = null;
        this.isOpen = false;
        this.focusedRowIndex = -1; // Track keyboard navigation

        this.init();
    }

    /**
     * Initialize the component
     */
    MultiColumnDropdownComponent.prototype.init = function() {
        this.render();
        this.attachEventHandlers();
    };

    /**
     * Render the entire dropdown
     */
    MultiColumnDropdownComponent.prototype.render = function() {
        var container = document.querySelector(this.config.containerSelector);
        if (!container) {
            console.error('Container not found:', this.config.containerSelector);
            return;
        }

        container.innerHTML = this.getDropdownHTML();
        this.renderTableBody();
    };

    /**
     * Generate the main dropdown HTML structure
     */
    MultiColumnDropdownComponent.prototype.getDropdownHTML = function() {
        var config = this.config;
        var dropdownId = config.dropdownId;
        var requiredMark = config.required ? '<span class="sup_color_red">*</span>' : '';
        
        var html = `
            <div class="tableselectdropdown input-group searchfiels" data-dropdown-id="${dropdownId}">
                ${config.labelText ? `
                    <label for="${dropdownId}_input" class="govuk-label govuk-!-font-weight-bold">
                        ${config.labelText} ${requiredMark}
                    </label>
                ` : ''}
                <input 
                    type="text" 
                    id="${dropdownId}_input" 
                    placeholder="${config.placeholder}" 
                    class="dropdown-input down-arrow-img govuk-input govuk-!-font-size-16" 
                    ${config.disabled ? 'disabled' : ''}
                    ${config.required ? 'required' : ''}
                    readonly
                />
                <input type="hidden" id="${dropdownId}_value" />
                
                <div class="multicolumn-dropdown-panel" id="${dropdownId}_panel">
                    ${config.enableSearch ? `
                        <div class="search-box-wrapper">
                            <input 
                                type="text" 
                                class="select-search-box" 
                                id="${dropdownId}_search"
                                placeholder="${config.searchPlaceholder}" 
                            />
                            <button 
                                type="button" 
                                class="govuk-button govuk-button--secondary clear-search-btn" 
                                id="${dropdownId}_clearSearch"
                                aria-label="Clear search"
                            >
                                <span class="sup_error_text_color govuk-!-font-size-19">&times;</span>
                            </button>
                        </div>
                    ` : ''}
                    
                    <div class="dropdown-table-wrapper">
                        <table>
                            <thead>
                                <tr>
                                    ${this.getTableHeaderHTML()}
                                </tr>
                            </thead>
                            <tbody id="${dropdownId}_tbody">
                                <!-- Table rows will be rendered here -->
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        `;

        return html;
    };

    /**
     * Generate table header HTML
     */
    MultiColumnDropdownComponent.prototype.getTableHeaderHTML = function() {
        var html = '';
        var config = this.config;

        // Serial number column
        if (config.showSerialNumber) {
            html += '<th style="width: 60px;">Sr. No</th>';
        }

        // Data columns
        config.columns.forEach(function(column) {
            var style = column.width ? `style="width: ${column.width};"` : '';
            html += `<th ${style}>${column.header || column.field}</th>`;
        });

        return html;
    };

    /**
     * Render table body with data
     */
    MultiColumnDropdownComponent.prototype.renderTableBody = function() {
        var tbody = document.getElementById(this.config.dropdownId + '_tbody');
        if (!tbody) return;

        var html = '';

        if (this.filteredData.length === 0) {
            var colspan = this.config.columns.length + (this.config.showSerialNumber ? 1 : 0);
            html = `<tr><td colspan="${colspan}" style="text-align: center; padding: 10px;">No data available</td></tr>`;
        } else {
            this.filteredData.forEach(function(row, index) {
                html += this.getTableRowHTML(row, index);
            }, this);
        }

        tbody.innerHTML = html;
    };

    /**
     * Generate table row HTML
     */
    MultiColumnDropdownComponent.prototype.getTableRowHTML = function(row, rowIndex) {
        var config = this.config;
        var rowValue = this.getFieldValue(row, config.valueField);
        var isSelected = this.selectedValue === rowValue;
        var isFocused = this.focusedRowIndex === rowIndex;
        
        var html = `<tr class="dropdown-row ${isSelected ? 'selected' : ''} ${isFocused ? 'focused' : ''}" 
                        data-value="${this.escapeHtml(rowValue)}" 
                        data-row-index="${rowIndex}"
                        tabindex="0">`;

        // Serial number
        if (config.showSerialNumber) {
            html += `<td>${rowIndex + 1}</td>`;
        }

        // Data columns
        config.columns.forEach(function(column) {
            var value = this.getFieldValue(row, column.field);
            var cellHtml = column.render ? column.render(value, row, rowIndex) : this.escapeHtml(value);
            html += `<td>${cellHtml}</td>`;
        }, this);

        html += '</tr>';
        return html;
    };

    /**
     * Get field value from row data
     */
    MultiColumnDropdownComponent.prototype.getFieldValue = function(row, field) {
        if (typeof field === 'function') {
            return field(row);
        }
        
        // Support nested properties like 'address.city'
        if (typeof field === 'string' && field.indexOf('.') > -1) {
            var parts = field.split('.');
            var value = row;
            for (var i = 0; i < parts.length; i++) {
                if (value === null || value === undefined) return '';
                value = value[parts[i]];
            }
            return value !== undefined ? value : '';
        }
        
        return row[field] !== undefined ? row[field] : '';
    };

    /**
     * Escape HTML to prevent XSS
     */
    MultiColumnDropdownComponent.prototype.escapeHtml = function(text) {
        if (text === null || text === undefined) return '';
        var div = document.createElement('div');
        div.textContent = String(text);
        return div.innerHTML;
    };

    /**
     * Attach event handlers
     */
    MultiColumnDropdownComponent.prototype.attachEventHandlers = function() {
        var self = this;
        var dropdownId = this.config.dropdownId;
        var input = document.getElementById(dropdownId + '_input');
        var panel = document.getElementById(dropdownId + '_panel');
        var searchBox = document.getElementById(dropdownId + '_search');
        var clearSearchBtn = document.getElementById(dropdownId + '_clearSearch');
        var tbody = document.getElementById(dropdownId + '_tbody');

        if (!input || !panel) return;

        // Toggle dropdown on input click
        input.addEventListener('click', function(e) {
            e.stopPropagation();
            if (!self.config.disabled) {
                self.toggleDropdown();
            }
        });

        // Search functionality
        if (searchBox && this.config.enableSearch) {
            searchBox.addEventListener('input', function() {
                self.filterData(this.value);
                self.focusedRowIndex = -1; // Reset focus when filtering
            });

            searchBox.addEventListener('click', function(e) {
                e.stopPropagation();
            });

            // Keyboard navigation from search box
            searchBox.addEventListener('keydown', function(e) {
                if (e.key === 'Tab' && !e.shiftKey) {
                    // Tab: Move focus to first row
                    if (self.filteredData.length > 0) {
                        e.preventDefault();
                        self.focusRow(0);
                    }
                } else if (e.key === 'ArrowDown') {
                    // Arrow Down: Move to first row
                    e.preventDefault();
                    if (self.filteredData.length > 0) {
                        self.focusRow(0);
                    }
                } else if (e.key === 'Escape') {
                    e.preventDefault();
                    self.closeDropdown();
                } else if (e.key === 'Enter') {
                    e.preventDefault();
                    // If there's exactly one filtered result, select it
                    if (self.filteredData.length === 1) {
                        self.selectItem(self.filteredData[0]);
                        self.closeDropdown();
                    }
                }
            });
        }

        // Clear search button
        if (clearSearchBtn && this.config.enableSearch) {
            clearSearchBtn.addEventListener('click', function(e) {
                e.stopPropagation();
                if (searchBox) {
                    searchBox.value = '';
                    self.filterData('');
                    searchBox.focus();
                }
            });
        }

        // Row selection
        if (tbody) {
            tbody.addEventListener('click', function(e) {
                var row = e.target.closest('.dropdown-row');
                if (row) {
                    var rowIndex = parseInt(row.getAttribute('data-row-index'));
                    var selectedData = self.filteredData[rowIndex];
                    self.selectItem(selectedData);
                    self.closeDropdown();
                }
            });

            // Keyboard navigation for rows
            tbody.addEventListener('keydown', function(e) {
                var row = e.target.closest('.dropdown-row');
                if (!row) return;

                var rowIndex = parseInt(row.getAttribute('data-row-index'));

                if (e.key === 'ArrowDown') {
                    e.preventDefault();
                    self.focusRow(rowIndex + 1);
                } else if (e.key === 'ArrowUp') {
                    e.preventDefault();
                    if (rowIndex > 0) {
                        self.focusRow(rowIndex - 1);
                    } else {
                        // Go back to search box
                        var searchBox = document.getElementById(self.config.dropdownId + '_search');
                        if (searchBox) {
                            searchBox.focus();
                            self.focusedRowIndex = -1;
                        }
                    }
                } else if (e.key === 'Enter') {
                    e.preventDefault();
                    var selectedData = self.filteredData[rowIndex];
                    self.selectItem(selectedData);
                    self.closeDropdown();
                } else if (e.key === 'Escape') {
                    e.preventDefault();
                    self.closeDropdown();
                } else if (e.key === 'Tab') {
                    e.preventDefault();
                    if (e.shiftKey) {
                        // Shift+Tab: Go back to search or previous row
                        if (rowIndex > 0) {
                            self.focusRow(rowIndex - 1);
                        } else {
                            var searchBox = document.getElementById(self.config.dropdownId + '_search');
                            if (searchBox) {
                                searchBox.focus();
                                self.focusedRowIndex = -1;
                            }
                        }
                    } else {
                        // Tab: Move to next row
                        self.focusRow(rowIndex + 1);
                    }
                }
            });
        }

        // Close dropdown when clicking outside
        document.addEventListener('click', function(e) {
            var container = document.querySelector('[data-dropdown-id="' + dropdownId + '"]');
            if (container && !container.contains(e.target)) {
                self.closeDropdown();
            }
        });

        // Prevent panel clicks from closing dropdown
        panel.addEventListener('click', function(e) {
            e.stopPropagation();
        });
    };

    /**
     * Toggle dropdown open/close
     */
    MultiColumnDropdownComponent.prototype.toggleDropdown = function() {
        if (this.isOpen) {
            this.closeDropdown();
        } else {
            this.openDropdown();
        }
    };

    /**
     * Open dropdown
     */
    MultiColumnDropdownComponent.prototype.openDropdown = function() {
        var panel = document.getElementById(this.config.dropdownId + '_panel');
        var input = document.getElementById(this.config.dropdownId + '_input');
        var container = document.querySelector(this.config.containerSelector);
        
        if (panel && input) {
            // Increase z-index to ensure this dropdown appears above others
            globalZIndex++;
            panel.style.zIndex = globalZIndex;
            if (container) {
                container.style.position = 'relative';
                container.style.zIndex = globalZIndex;
            }
            
            panel.style.display = 'block';
            this.isOpen = true;
            input.classList.add('dropdown-open');
            
            // Focus search box if enabled
            if (this.config.enableSearch) {
                var searchBox = document.getElementById(this.config.dropdownId + '_search');
                if (searchBox) {
                    setTimeout(function() {
                        searchBox.focus();
                    }, 100);
                }
            }
        }
    };

    /**
     * Close dropdown
     */
    MultiColumnDropdownComponent.prototype.closeDropdown = function() {
        var panel = document.getElementById(this.config.dropdownId + '_panel');
        var input = document.getElementById(this.config.dropdownId + '_input');
        var searchBox = document.getElementById(this.config.dropdownId + '_search');
        var container = document.querySelector(this.config.containerSelector);
        
        if (panel && input) {
            panel.style.display = 'none';
            this.isOpen = false;
            input.classList.remove('dropdown-open');
            this.focusedRowIndex = -1; // Reset focus
            
            // Reset z-index
            if (container) {
                container.style.zIndex = '';
            }
            
            // Clear search
            if (searchBox && this.config.enableSearch) {
                searchBox.value = '';
                this.filterData('');
            }
        }
    };

    /**
     * Focus a specific row for keyboard navigation
     */
    MultiColumnDropdownComponent.prototype.focusRow = function(rowIndex) {
        if (rowIndex < 0 || rowIndex >= this.filteredData.length) {
            return;
        }

        this.focusedRowIndex = rowIndex;
        var tbody = document.getElementById(this.config.dropdownId + '_tbody');
        if (!tbody) return;

        var row = tbody.querySelector('.dropdown-row[data-row-index="' + rowIndex + '"]');
        if (row) {
            row.focus();
            // Scroll into view if needed
            row.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
        }
    };

    /**
     * Filter data based on search term
     */
    MultiColumnDropdownComponent.prototype.filterData = function(searchTerm) {
        var self = this;
        searchTerm = String(searchTerm).toLowerCase().trim();

        if (!searchTerm) {
            this.filteredData = [...this.originalData];
        } else {
            this.filteredData = this.originalData.filter(function(row) {
                // Search across all columns
                for (var i = 0; i < self.config.columns.length; i++) {
                    var column = self.config.columns[i];
                    var value = String(self.getFieldValue(row, column.field)).toLowerCase();
                    if (value.indexOf(searchTerm) > -1) {
                        return true;
                    }
                }
                return false;
            });
        }

        this.renderTableBody();
    };

    /**
     * Select an item
     */
    MultiColumnDropdownComponent.prototype.selectItem = function(item) {
        var input = document.getElementById(this.config.dropdownId + '_input');
        var hiddenInput = document.getElementById(this.config.dropdownId + '_value');
        
        if (input && hiddenInput && item) {
            var displayValue = this.getFieldValue(item, this.config.displayField);
            var value = this.getFieldValue(item, this.config.valueField);
            
            input.value = displayValue;
            hiddenInput.value = value;
            
            this.selectedValue = value;
            this.selectedItem = item;
            
            // Update row selection styling
            this.updateRowSelection();
            
            // Trigger callback
            if (this.config.callbacks.onSelect) {
                this.config.callbacks.onSelect(item, this);
            }
            
            if (this.config.callbacks.onChange) {
                this.config.callbacks.onChange(item, this);
            }
        }
    };

    /**
     * Update row selection styling
     */
    MultiColumnDropdownComponent.prototype.updateRowSelection = function() {
        var tbody = document.getElementById(this.config.dropdownId + '_tbody');
        if (!tbody) return;

        var rows = tbody.querySelectorAll('.dropdown-row');
        rows.forEach(function(row) {
            var rowValue = row.getAttribute('data-value');
            if (String(rowValue) === String(this.selectedValue)) {
                row.classList.add('selected');
            } else {
                row.classList.remove('selected');
            }
        }, this);
    };

    /**
     * Set selected value programmatically
     */
    MultiColumnDropdownComponent.prototype.setValue = function(value) {
        var item = this.originalData.find(function(row) {
            return String(this.getFieldValue(row, this.config.valueField)) === String(value);
        }, this);

        if (item) {
            this.selectItem(item);
        }
    };

    /**
     * Get selected value
     */
    MultiColumnDropdownComponent.prototype.getValue = function() {
        return this.selectedValue;
    };

    /**
     * Get selected item
     */
    MultiColumnDropdownComponent.prototype.getSelectedItem = function() {
        return this.selectedItem;
    };

    /**
     * Clear selection
     */
    MultiColumnDropdownComponent.prototype.clear = function() {
        var input = document.getElementById(this.config.dropdownId + '_input');
        var hiddenInput = document.getElementById(this.config.dropdownId + '_value');
        
        if (input) input.value = '';
        if (hiddenInput) hiddenInput.value = '';
        
        this.selectedValue = null;
        this.selectedItem = null;
        
        this.updateRowSelection();
        
        if (this.config.callbacks.onClear) {
            this.config.callbacks.onClear(this);
        }
    };

    /**
     * Update dropdown data
     */
    MultiColumnDropdownComponent.prototype.updateData = function(newData) {
        this.originalData = [...newData];
        this.filteredData = [...newData];
        this.renderTableBody();
    };

    /**
     * Enable dropdown
     */
    MultiColumnDropdownComponent.prototype.enable = function() {
        var input = document.getElementById(this.config.dropdownId + '_input');
        if (input) {
            input.disabled = false;
            this.config.disabled = false;
        }
    };

    /**
     * Disable dropdown
     */
    MultiColumnDropdownComponent.prototype.disable = function() {
        var input = document.getElementById(this.config.dropdownId + '_input');
        if (input) {
            input.disabled = true;
            this.config.disabled = true;
        }
        this.closeDropdown();
    };

    /**
     * Refresh/reload the dropdown
     */
    MultiColumnDropdownComponent.prototype.refresh = function() {
        this.renderTableBody();
    };

    /**
     * Destroy the dropdown and clean up
     */
    MultiColumnDropdownComponent.prototype.destroy = function() {
        var container = document.querySelector(this.config.containerSelector);
        if (container) {
            container.innerHTML = '';
        }
        
        // Clear references
        this.originalData = [];
        this.filteredData = [];
        this.selectedValue = null;
        this.selectedItem = null;
    };

    // Expose to global scope
    window.MultiColumnDropdownComponent = MultiColumnDropdownComponent;

})(window);
