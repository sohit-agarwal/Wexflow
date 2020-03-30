function WexflowApproval() {
    "use strict";

    var id = "wf-approval";
    var uri = Common.trimEnd(Settings.Uri, "/");
    var lnkManager = document.getElementById("lnk-manager");
    var lnkDesigner = document.getElementById("lnk-designer");
    var lnkDesigner = document.getElementById("lnk-designer");
    var lnkEditor = document.getElementById("lnk-editor");
    var lnkApproval = document.getElementById("lnk-approval");
    var lnkUsers = document.getElementById("lnk-users");
    var lnkProfiles = document.getElementById("lnk-profiles");
    var selectedId = -1;
    var instanceIds = [];
    var workflows = {};
    var timer = null;
    var timerInterval = 1000; // ms
    var username = "";
    var password = "";
    var auth = "";

    var html = "<div id='wf-container'>"
        + "<div id='wf-cmd'>"
        + "<button id='wf-start' type='button' class='btn btn-primary btn-xs'>Start</button>"
        + "<button id='wf-pause' type='button' class='btn btn-secondary btn-xs'>Suspend</button>"
        + "<button id='wf-resume' type='button' class='btn btn-secondary btn-xs'>Resume</button>"
        + "<button id='wf-stop' type='button' class='btn btn-danger btn-xs'>Stop</button>"
        + "<button id='wf-approve' type='button' class='btn btn-primary btn-xs'>Approve</button>"
        + "<button id='wf-disapprove' type='button' class='btn btn-danger btn-xs'>Reject</button>"
        + "</div>"
        + "<div id='wf-notifier'>"
        + "<input id='wf-notifier-text' type='text' name='fname' readonly>"
        + "</div>"
        + "<div id='wf-search'>"
        + "<div id='wf-search-text-container'>"
        + "<input id='wf-search-text' type='text' name='fname'>"
        + "</div>"
        + "<button id='wf-search-action' type='button' class='btn btn-primary btn-xs'>Search</button>"
        + "</div>"
        + "<div id='wf-workflows'>"
        + "</div>"
        + "</div>";

    document.getElementById(id).innerHTML = html;

    var startButton = document.getElementById("wf-start");
    var suspendButton = document.getElementById("wf-pause");
    var resumeButton = document.getElementById("wf-resume");
    var stopButton = document.getElementById("wf-stop");
    var approveButton = document.getElementById("wf-approve");
    var disapproveButton = document.getElementById("wf-disapprove");
    var searchButton = document.getElementById("wf-search-action");
    var searchText = document.getElementById("wf-search-text");
    var suser = getUser();

    if (suser === null || suser === "") {
        Common.redirectToLoginPage();
    } else {
        var user = JSON.parse(suser);

        username = user.Username;
        password = user.Password;
        auth = "Basic " + btoa(username + ":" + password);

        Common.get(uri + "/user?username=" + encodeURIComponent(user.Username),
            function (u) {
                if (user.Password !== u.Password) {
                    Common.redirectToLoginPage();
                } else {

                    if (u.UserProfile === 0 || u.UserProfile === 1) {
                        lnkManager.style.display = "inline";
                        lnkDesigner.style.display = "inline";
                        lnkEditor.style.display = "inline";
                        lnkApproval.style.display = "inline";
                        lnkUsers.style.display = "inline";

                        if (u.UserProfile === 0) {
                            lnkProfiles.style.display = "inline";
                        }

                        var btnLogout = document.getElementById("btn-logout");
                        var divWorkflows = document.getElementById("wf-approval");
                        divWorkflows.style.display = "block";

                        btnLogout.onclick = function () {
                            deleteUser();
                            Common.redirectToLoginPage();
                        };
                        btnLogout.innerHTML = "Logout (" + u.Username + ")";

                        Common.disableButton(startButton, true);
                        Common.disableButton(suspendButton, true);
                        Common.disableButton(resumeButton, true);
                        Common.disableButton(stopButton, true);
                        Common.disableButton(approveButton, true);
                        Common.disableButton(disapproveButton, true);

                        searchButton.onclick = function () {
                            loadWorkflows();
                            notify("");
                            Common.disableButton(startButton, true);
                            Common.disableButton(suspendButton, true);
                            Common.disableButton(resumeButton, true);
                            Common.disableButton(stopButton, true);
                            Common.disableButton(approveButton, true);
                            Common.disableButton(disapproveButton, true);
                        };

                        searchText.onkeyup = function (event) {
                            event.preventDefault();

                            if (event.keyCode === 13) { // Enter
                                loadWorkflows();
                                notify("");
                                Common.disableButton(startButton, true);
                                Common.disableButton(suspendButton, true);
                                Common.disableButton(resumeButton, true);
                                Common.disableButton(stopButton, true);
                                Common.disableButton(approveButton, true);
                                Common.disableButton(disapproveButton, true);
                            }
                        };

                        loadWorkflows();
                    } else {
                        Common.redirectToLoginPage();
                    }

                }
            },
            function () { }, auth);
    }



    function compareById(wf1, wf2) {
        if (wf1.Id < wf2.Id) {
            return -1;
        } else if (wf1.Id > wf2.Id) {
            return 1;
        }
        return 0;
    }

    function launchType(lt) {
        switch (lt) {
            case 0:
                return "Startup";
            case 1:
                return "Trigger";
            case 2:
                return "Periodic";
            case 3:
                return "Cron";
            default:
                return "";
        }
    }

    function loadWorkflows() {
        Common.get(uri + "/searchApprovalWorkflows?s=" + encodeURIComponent(searchText.value),
            function (data) {
                data.sort(compareById);
                var items = [];
                var i;
                for (i = 0; i < data.length; i++) {
                    var val = data[i];
                    workflows[val.Id] = val;
                    var lt = launchType(val.LaunchType);
                    items.push("<tr>"
                        + "<td class='wf-id' title='" + val.Id + "'>" + val.Id + "</td>"
                        + "<td class='wf-n' title='" + val.Name + "'>" + val.Name + "</td>"
                        + "<td class='wf-lt'>" + lt + "</td>"
                        + "<td class='wf-e'><input type='checkbox' readonly disabled " + (val.IsEnabled ? "checked" : "") + "></input></td>"
                        + "<td class='wf-d' title='" + val.Description + "'>" + val.Description + "</td>"
                        + "</tr>");

                }

                var table = "<table id='wf-workflows-table' class='table'>"
                    + "<thead class='thead-dark'>"
                    + "<tr>"
                    + "<th class='wf-id'>Id</th>"
                    + "<th class='wf-n'>Name</th>"
                    + "<th class='wf-lt'>LaunchType</th>"
                    + "<th class='wf-e'>Enabled</th>"
                    + "<th class='wf-d'>Description</th>"
                    + "</tr>"
                    + "</thead>"
                    + "<tbody>"
                    + items.join("")
                    + "</tbody>"
                    + "</table>";

                document.getElementById("wf-workflows").innerHTML = table;

                var workflowsTable = document.getElementById("wf-workflows-table");
                var descriptions = document.getElementsByClassName("wf-d");
                for (i = 0; i < descriptions.length; i++) {
                    descriptions[i].style.width = workflowsTable.offsetWidth - (45 + 200 + 100 + 75 + 16 * 5 + 17) + "px";
                }

                function getWorkflow(wid, func) {
                    Common.get(uri + "/workflow?w=" + wid, function (d) {
                        func(d);
                    },
                        function () { }, auth);
                }

                function updateButtons(wid, force) {
                    getWorkflow(wid, function (workflow) {
                        if (workflow.IsEnabled === false) {
                            notify("This workflow is disabled.");
                            Common.disableButton(startButton, true);
                            Common.disableButton(suspendButton, true);
                            Common.disableButton(resumeButton, true);
                            Common.disableButton(stopButton, true);
                            Common.disableButton(approveButton, true);
                            Common.disableButton(disapproveButton, true);
                        }
                        else {
                            if (force === false && workflowStatusChanged(workflow) === false) return;

                            Common.disableButton(startButton, workflow.IsRunning);
                            Common.disableButton(stopButton, !(workflow.IsRunning && !workflow.IsPaused));
                            Common.disableButton(suspendButton, !(workflow.IsRunning && !workflow.IsPaused));
                            Common.disableButton(resumeButton, !workflow.IsPaused);
                            Common.disableButton(approveButton, !(workflow.IsWaitingForApproval && workflow.IsApproval));
                            Common.disableButton(disapproveButton, !(workflow.IsWaitingForApproval && workflow.IsApproval));

                            if (workflow.IsApproval === true && workflow.IsWaitingForApproval === true && workflow.IsPaused === false) {
                                notify("This workflow is waiting for approval...");
                            } else {
                                if (workflow.IsRunning === true && workflow.IsPaused === false) {
                                    notify("This workflow is running...");
                                }
                                else if (workflow.IsPaused === true) {
                                    notify("This workflow is suspended.");
                                } else {
                                    notify("");
                                }
                            }

                        }
                    });
                }

                function workflowStatusChanged(workflow) {
                    var changed = workflows[workflow.Id].IsRunning !== workflow.IsRunning || workflows[workflow.Id].IsPaused !== workflow.IsPaused || workflows[workflow.Id].IsWaitingForApproval !== workflow.IsWaitingForApproval;
                    workflows[workflow.Id].IsRunning = workflow.IsRunning;
                    workflows[workflow.Id].IsPaused = workflow.IsPaused;
                    workflows[workflow.Id].IsWaitingForApproval = workflow.IsWaitingForApproval;
                    return changed;
                }

                var rows = (workflowsTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
                for (i = 0; i < rows.length; i++) {
                    rows[i].onclick = function () {
                        selectedId = parseInt(this.getElementsByClassName("wf-id")[0].innerHTML);

                        var selected = document.getElementsByClassName("selected");
                        if (selected.length > 0) {
                            selected[0].className = selected[0].className.replace("selected", "");
                        }

                        this.className += "selected";

                        clearInterval(timer);

                        if (workflows[selectedId].IsEnabled === true) {
                            timer = setInterval(function () {
                                updateButtons(selectedId, false);
                            }, timerInterval);

                            updateButtons(selectedId, true);
                        } else {
                            updateButtons(selectedId, true);
                        }

                        Common.get(uri + "/workflow?w=" + selectedId, function (w) {
                            if (w.IsRunning === true) {
                                instanceIds[selectedId] = w.InstanceId;
                            }

                        }, function () { }, auth);

                    };
                }

                startButton.onclick = function () {
                    var startUri = uri + "/start?w=" + selectedId;
                    Common.post(startUri, function (res) {
                        instanceIds[selectedId] = res;
                    }, function () { }, "", auth);
                };

                suspendButton.onclick = function () {
                    var suspendUri = uri + "/suspend?w=" + selectedId + "&i=" + instanceIds[selectedId];
                    Common.post(suspendUri, function (res) {
                        if (res === true) {
                            updateButtons(selectedId, true);
                        } else {
                            Common.toastInfo("This operation is not supported.");
                        }
                    }, function () { }, "", auth);
                };

                resumeButton.onclick = function () {
                    var resumeUri = uri + "/resume?w=" + selectedId + "&i=" + instanceIds[selectedId];
                    Common.post(resumeUri, function () { }, function () { }, "", auth);
                };

                stopButton.onclick = function () {
                    var stopUri = uri + "/stop?w=" + selectedId + "&i=" + instanceIds[selectedId];
                    Common.post(stopUri,
                        function (res) {
                            if (res === true) {
                                updateButtons(selectedId, true);
                            } else {
                                Common.toastInfo("This operation is not supported.");
                            }
                        },
                        function () { }, "", auth);
                };

                approveButton.onclick = function () {
                    Common.disableButton(approveButton, true);
                    Common.disableButton(stopButton, true);
                    var approveUri = uri + "/approve?w=" + selectedId + "&i=" + instanceIds[selectedId];
                    Common.post(approveUri,
                        function (res) {
                            if (res === true) {
                                updateButtons(selectedId, true);
                                Common.toastSuccess("The workflow " + selectedId + " was approved.");
                            } else {
                                Common.disableButton(approveButton, false);
                                Common.disableButton(stopButton, false);
                                Common.toastError("An error occured while approving the workflow " + selectedId + ".");
                            }
                        },
                        function () { }, "", auth);
                };

                disapproveButton.onclick = function () {
                    Common.disableButton(disapproveButton, true);
                    Common.disableButton(approveButton, true);
                    Common.disableButton(stopButton, true);
                    var disapproveUri = uri + "/reject?w=" + selectedId + "&i=" + instanceIds[selectedId];
                    Common.post(disapproveUri,
                        function (res) {
                            if (res === true) {
                                updateButtons(selectedId, true);
                                Common.toastSuccess("The workflow " + selectedId + " was rejected.");
                            } else {
                                Common.disableButton(disapproveButton, true);
                                Common.disableButton(approveButton, false);
                                Common.disableButton(stopButton, false);
                                Common.toastError("An error occured while rejecting the workflow " + selectedId + ".");
                            }
                        },
                        function () { }, "", auth);
                };

                // End of get workflows
            }, function () {
                Common.toastError("An error occured while retrieving workflows. Check that Wexflow server is running correctly.");
            }, auth);
    }

    function notify(msg) {
        document.getElementById("wf-notifier-text").value = msg;
    }
}