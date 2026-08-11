(function () {
    if (window.showAlertMessage && window.showGovukConfirm) {
        return;
    }

    var AlertType = Object.freeze({
        ERROR: "E",
        INFO: "I",
        SUCCESS: "S",
        WARNING: "W"
    });

    window.AlertType = AlertType;

    var pending = Promise.resolve();

    function getFocusable(container) {
        return Array.prototype.slice.call(
            container.querySelectorAll(
                'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
            )
        ).filter(function (el) {
            return !el.hasAttribute("disabled") && el.getAttribute("aria-hidden") !== "true";
        });
    }

    function buildDialog(options) {
        var titleId = "govuk-dialog-title-" + Date.now() + "-" + Math.random().toString(16).slice(2);
        var descId = "govuk-dialog-desc-" + Date.now() + "-" + Math.random().toString(16).slice(2);
        var modalclassName = "";
        var modalTextHeader = "";

        switch (options.type) {
            case "confirm":
                modalclassName = "isInfo";
                modalTextHeader = "Please confirm";
                break;
            case AlertType.ERROR:
                modalclassName = "isDanger";
                modalTextHeader = "Error";
                break;
            case AlertType.INFO:
                modalclassName = "isInfo";
                modalTextHeader = "Information";
                break;
            case AlertType.SUCCESS:
                modalclassName = "isSuccess";
                modalTextHeader = "Message";
                break;
            case AlertType.WARNING:
                modalclassName = "isWarning";
                modalTextHeader = "Warning";
                break;
        }

        var backdrop = document.createElement("div");
        backdrop.setAttribute("data-govuk-modal", "backdrop");
        backdrop.style.position = "fixed";
        backdrop.style.inset = "0";
        backdrop.style.zIndex = "2000";
        backdrop.style.display = "flex";
        backdrop.style.alignItems = "center";
        backdrop.style.justifyContent = "center";
        backdrop.style.backgroundColor = "rgba(11, 12, 12, 0.6)";
        backdrop.style.padding = "20px";

        var dialog = document.createElement("div");
        dialog.setAttribute("role", "dialog");
        dialog.setAttribute("aria-modal", "true");
        dialog.setAttribute("aria-labelledby", titleId);
        dialog.setAttribute("aria-describedby", descId);
        dialog.setAttribute("tabindex", "-1");
        dialog.className = "govuk-body " + modalclassName + "";
        dialog.style.backgroundColor = "#ffffff";
        dialog.style.maxWidth = "620px";
        dialog.style.width = "100%";
        dialog.style.maxHeight = "calc(100vh - 40px)";
        dialog.style.overflowY = "auto";
        dialog.style.padding = "30px";
        dialog.style.boxShadow = "0 8px 24px rgba(11, 12, 12, 0.3)";
        dialog.style.border = "2px solid #0b0c0c";

        var heading = document.createElement("h2");
        heading.id = titleId;
        heading.className = "govuk-heading-m govuk-!-margin-bottom-3";
        heading.textContent = options.title || modalTextHeader;

        var message = document.createElement("p");
        message.id = descId;
        message.className = "govuk-body govuk-!-margin-bottom-5";
        message.textContent = String(options.message || "");

        var buttonGroup = document.createElement("div");
        buttonGroup.className = "govuk-button-group govuk-!-margin-bottom-0";
        buttonGroup.style.display = "flex";
        buttonGroup.style.justifyContent = "flex-end";
        buttonGroup.style.width = "100%";

        var okButton = document.createElement("button");
        okButton.type = "button";
        okButton.className = "govuk-button";
        okButton.setAttribute("data-module", "govuk-button");
        okButton.textContent = options.okText || "OK";

        buttonGroup.appendChild(okButton);

        var cancelButton = null;
        if (options.type === "confirm") {
            cancelButton = document.createElement("button");
            cancelButton.type = "button";
            cancelButton.className = "govuk-button govuk-button--secondary";
            cancelButton.setAttribute("data-module", "govuk-button");
            cancelButton.textContent = options.cancelText || "Cancel";
            buttonGroup.appendChild(cancelButton);
        }

        dialog.appendChild(heading);
        dialog.appendChild(message);
        dialog.appendChild(buttonGroup);
        backdrop.appendChild(dialog);

        return {
            backdrop: backdrop,
            dialog: dialog,
            okButton: okButton,
            cancelButton: cancelButton
        };
    }

    function openDialog(options) {
        return new Promise(function (resolve) {
            var previousActive = document.activeElement;
            var previousOverflow = document.body.style.overflow;
            var closed = false;
            var parts = buildDialog(options);

            function cleanup(result) {
                if (closed) {
                    return;
                }
                closed = true;

                document.removeEventListener("keydown", onKeyDown, true);
                if (parts.backdrop.parentNode) {
                    parts.backdrop.parentNode.removeChild(parts.backdrop);
                }

                document.body.style.overflow = previousOverflow;

                if (previousActive && typeof previousActive.focus === "function") {
                    previousActive.focus();
                }

                resolve(result);
            }

            function onKeyDown(event) {
                if (event.key === "Escape") {
                    event.preventDefault();
                    cleanup(options.type === "confirm" ? false : true);
                    return;
                }

                if (event.key === "Tab") {
                    var focusable = getFocusable(parts.dialog);
                    if (!focusable.length) {
                        event.preventDefault();
                        parts.dialog.focus();
                        return;
                    }

                    var first = focusable[0];
                    var last = focusable[focusable.length - 1];

                    if (event.shiftKey && document.activeElement === first) {
                        event.preventDefault();
                        last.focus();
                    } else if (!event.shiftKey && document.activeElement === last) {
                        event.preventDefault();
                        first.focus();
                    }
                }
            }

            parts.okButton.addEventListener("click", function () {
                cleanup(true);
            });

            if (parts.cancelButton) {
                parts.cancelButton.addEventListener("click", function () {
                    cleanup(false);
                });
            }

            document.body.appendChild(parts.backdrop);
            document.body.style.overflow = "hidden";

            document.addEventListener("keydown", onKeyDown, true);
            parts.okButton.focus();
        });
    }

    function stopPropagation(event) {
        event.stopPropagation();
    }

    function applySafeBootstrapConfig(modalElement) {
        if (!modalElement) {
            return;
        }

        modalElement.setAttribute("data-bs-backdrop", "static");
        modalElement.setAttribute("data-bs-keyboard", "false");

        if (window.bootstrap && window.bootstrap.Modal) {
            var instance = window.bootstrap.Modal.getInstance(modalElement);
            if (instance && instance._config) {
                instance._config.backdrop = "static";
                instance._config.keyboard = false;
            }
        }
    }

    function initializeSafeModal(modalOrId) {
        var modalElement = null;

        if (typeof modalOrId === "string") {
            var id = modalOrId.charAt(0) === "#" ? modalOrId.slice(1) : modalOrId;
            modalElement = document.getElementById(id);
        } else {
            modalElement = modalOrId;
        }

        if (!modalElement || !modalElement.classList || !modalElement.classList.contains("modal")) {
            return;
        }

        if (modalElement.dataset.safeModalInit === "true") {
            applySafeBootstrapConfig(modalElement);
            return;
        }

        modalElement.dataset.safeModalInit = "true";
        applySafeBootstrapConfig(modalElement);

        modalElement.addEventListener("show.bs.modal", function () {
            applySafeBootstrapConfig(modalElement);
        });

        var modalContent = modalElement.querySelector(".modal-content");
        if (modalContent) {
            ["click", "mousedown", "mouseup"].forEach(function (eventName) {
                modalContent.addEventListener(eventName, stopPropagation);
            });
        }

        var fields = modalElement.querySelectorAll("input, textarea, select, button");
        fields.forEach(function (field) {
            ["click", "keydown"].forEach(function (eventName) {
                field.addEventListener(eventName, stopPropagation);
            });
        });
    }

    function initializeAllSafeModals() {
        var modals = document.querySelectorAll(".modal");
        modals.forEach(function (modalElement) {
            initializeSafeModal(modalElement);
        });
    }

    function watchDynamicModals() {
        if (!window.MutationObserver || !document.body) {
            return;
        }

        var observer = new MutationObserver(function (mutations) {
            var hasChanges = mutations.some(function (mutation) {
                return mutation.type === "childList" && (mutation.addedNodes && mutation.addedNodes.length > 0);
            });

            if (hasChanges) {
                initializeAllSafeModals();
            }
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", function () {
            initializeAllSafeModals();
            watchDynamicModals();
        });
    } else {
        initializeAllSafeModals();
        watchDynamicModals();
    }

    window.initializeSafeModal = initializeSafeModal;

    window.showAlertMessage = function (message, type) {

        var validTypes = Object.values(AlertType);
        var resolvedType = type || AlertType.INFO;

        if (validTypes.indexOf(resolvedType) === -1) {
            resolvedType = AlertType.INFO;
        }
        pending = pending.then(function () {
            return openDialog({
                type: resolvedType,
                message: message,
                okText: "OK"
            });
        });

        return pending.then(function () {
            return undefined;
        });
    };

    window.showGovukConfirm = function (message) {
        pending = pending.then(function () {
            return openDialog({
                type: "confirm",
                message: message,
                okText: "OK",
                cancelText: "Cancel"
            });
        });

        return pending.then(function (result) {
            return result;
        });
    };

    window.showLoader = function () {
        $("#loader").show();
        // document.getElementById("loader").style.display = "block";
    }

    window.hideLoader = function () {
        $("#loader").hide();
        //document.getElementById("loader").style.display = "none";
    }

    // Downloads a file from the given URL, showing the global loader until the
    // download completes. Reusable for any Excel/PDF/CSV export endpoint.
    window.downloadFile = function (url, fileName) {
        showLoader();
        return fetch(url)
            .then(function (r) { return r.blob(); })
            .then(function (blob) {
                const link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = fileName || 'download';
                link.click();
                window.URL.revokeObjectURL(link.href);
            })
            .finally(hideLoader);
    }


    window.showGovukYesNo = function (message) {
        pending = pending.then(function () {
            return openDialog({
                type: "confirm",
                message: message,
                okText: "Yes",
                cancelText: "No"
            });
        });

        return pending.then(function (result) {
            return result;
        });
    };

    window.showGovukApproveReject = function (message) {
        pending = pending.then(function () {
            return openDialog({
                type: "confirm",
                message: message,
                okText: "Approve",
                cancelText: "Reject"
            });
        });

        return pending.then(function (result) {
            return result;
        });
    };
})();
