function Profiles() {
    "use strict";

    var uri = Common.trimEnd(Settings.Uri, "/");
    var lnkManager = document.getElementById("lnk-manager");
    var lnkDesigner = document.getElementById("lnk-designer");
    var lnkApproval = document.getElementById("lnk-approval");
    var lnkUsers = document.getElementById("lnk-users");
    var lnkProfiles = document.getElementById("lnk-profiles");
    var divProfiles = document.getElementById("profiles");
    var btnLogout = document.getElementById("btn-logout");
    var txtSearch = document.getElementById("users-search-text");
    var divUsersTable = document.getElementById("users-table");
    var btnSearch = document.getElementById("users-search-action");
    var divWorkflows = document.getElementById("workflows");
    var btnSave = document.getElementById("users-save-action");
    var suser = getUser();
    var uo = 0;
    var selectedUserId = "";
    var selectedUsername = "";
    var workflows = [];
    var userWorkflows = [];    // [{"UserId": 1, "WorkflowId": 6}, ...]
    var username = "";
    var password = "";
    var auth = "";

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
                } else if (u.UserProfile === 0) {
                    divProfiles.style.display = "block";
                    lnkManager.style.display = "inline";
                    lnkDesigner.style.display = "inline";
                    lnkApproval.style.display = "inline";
                    lnkUsers.style.display = "inline";
                    lnkProfiles.style.display = "inline";

                    btnLogout.innerHTML = "Logout (" + u.Username + ")";

                    btnLogout.onclick = function () {
                        deleteUser();
                        Common.redirectToLoginPage();
                    };

                    loadUsers();

                } else {
                    Common.redirectToLoginPage();
                }

            },
            function () { }, auth);
    }

    btnSearch.onclick = function () {
        loadUsers(selectedUsername, true);
    };

    txtSearch.onkeyup = function (e) {
        if (e.keyCode === 13) {
            loadUsers(selectedUsername, true);
        }
    }

    function loadUsers(usernameToSelect, scroll) {
        Common.get(uri + "/searchAdmins?keyword=" + encodeURIComponent(txtSearch.value) + "&uo=" + uo,
            function (data) {

                var items = [];
                for (var i = 0; i < data.length; i++) {
                    var val = data[i];
                    var tr;

                    if (usernameToSelect === val.Username) {
                        tr = "<tr class='selected'>";
                    } else {
                        tr = "<tr>";
                    }

                    items.push(
                        tr
                        + "<td class='userid'>" + val.Id + "</td>"
                        + "<td class='username'>" + val.Username + "</td>"
                        //+ "<td class='userprofile'>" + userProfileToText(val.UserProfile) + "</td>"
                        + "</tr>"
                    );
                }

                var table = "<table id='wf-users-table' class='table'>"
                    + "<thead class='thead-dark'>"
                    + "<tr>"
                    + "<th id='th-id'>Id</th>"
                    + "<th id='th-username'>Username&nbsp;&nbsp;🔺</th>"
                    //+ "<th>Profile</th>"
                    + "</tr>"
                    + "</thead>"
                    + "<tbody>"
                    + items.join("")
                    + "</tbody>"
                    + "</table>";

                divUsersTable.innerHTML = table;

                var thUsername = document.getElementById("th-username");
                thUsername.onclick = function () {
                    if (uo === 0) {
                        uo = 1;
                    } else {
                        uo = 0;
                    }
                    loadUsers(selectedUsername, true);
                };

                if (uo === 0) {
                    thUsername.innerHTML = "Username&nbsp;&nbsp;🔺";
                } else {
                    thUsername.innerHTML = "Username&nbsp;&nbsp;🔻";
                }

                var usersTable = document.getElementById("wf-users-table");

                var rows = (usersTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
                for (var j = 0; j < rows.length; j++) {

                    var row = rows[j];
                    if (scroll === true) {
                        var userIdTd = row.getElementsByClassName("userid")[0];

                        if (typeof userIdTd !== "undefined" && userIdTd !== null) {
                            var userId = userIdTd.innerHTML;
                            if (userId === selectedUserId) {
                                row.scrollIntoView(true);
                            }

                        }
                    }

                    row.onclick = function () {
                        var selected = document.getElementsByClassName("selected");
                        if (selected.length > 0) {
                            var selectedTr = selected[0];
                            selectedTr.className = selectedTr.className.replace("selected", "");
                        }

                        this.className += "selected";

                        selectedUserId = this.getElementsByClassName("userid")[0].innerHTML;
                        var selectedUsernameTd = this.getElementsByClassName("username")[0];
                        selectedUsername = selectedUsernameTd.innerHTML;

                        userWorkflows = [];
                        loadRightPanel(selectedUserId);
                    };
                }

            },
            function () { }, auth);
    }

    function loadRightPanel() {
        Common.get(uri + "/search?s=",
            function (data) {
                btnSave.style.display = "block";

                data.sort(compareById);
                var items = [];
                var i;
                for (i = 0; i < data.length; i++) {
                    var val = data[i];
                    workflows[val.Id] = val;
                    var lt = launchType(val.LaunchType);
                    items.push("<tr>"
                        + "<td class='wf-check'><input type='checkbox'></td>"
                        + "<td class='wf-id' title='" + val.Id + "'>" + val.Id + "</td>"
                        + "<td class='wf-n' title='" + val.Name + "'>" + val.Name + "</td>"
                        + "<td class='wf-lt'>" + lt + "</td>"
                        + "<td class='wf-e'><input type='checkbox' readonly disabled " + (val.IsEnabled ? "checked" : "") + "></input></td>"
                        + "<td class='wf-a'><input type='checkbox' readonly disabled " + (val.IsApproval ? "checked" : "") + "></input></td>"
                        + "<td class='wf-d' title='" + val.Description + "'>" + val.Description + "</td>"
                        + "</tr>");

                }

                var table = "<table id='wf-workflows-table' class='table'>"
                    + "<thead class='thead-dark'>"
                    + "<tr>"
                    + "<th><input id='check-all' type='checkbox'></th>"
                    + "<th class='wf-id'>Id</th>"
                    + "<th class='wf-n'>Name</th>"
                    + "<th class='wf-lt'>LaunchType</th>"
                    + "<th class='wf-e'>Enabled</th>"
                    + "<th class='wf-a'>Approval</th>"
                    + "<th class='wf-d'>Description</th>"
                    + "</tr>"
                    + "</thead>"
                    + "<tbody>"
                    + items.join("")
                    + "</tbody>"
                    + "</table>";

                divWorkflows.innerHTML = table;

                var workflowsTable = document.getElementById("wf-workflows-table");
                var descriptions = document.getElementsByClassName("wf-d");
                for (i = 0; i < descriptions.length; i++) {
                    descriptions[i].style.width = workflowsTable.offsetWidth - (45 + 200 + 100 + 75 + 16 * 5 + 17) + "px";
                }

                var rows = (workflowsTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
                for (i = 0; i < rows.length; i++) {
                    var row = rows[i];
                    var checkBox = row.getElementsByClassName("wf-check")[0].firstChild;

                    checkBox.onchange = function () {
                        var currentRow = this.parentElement.parentElement;
                        var workflowId = parseInt(currentRow.getElementsByClassName("wf-id")[0].innerHTML);
                        var workflowDbId = workflows[workflowId].DbId;

                        if (this.checked === true) {
                            userWorkflows.push({ "UserId": selectedUserId, "WorkflowId": workflowDbId });
                        } else {
                            //var index = -1;
                            for (var j = userWorkflows.length - 1; j > -1; j--) {
                                if (userWorkflows[j].WorkflowId === workflowDbId) {
                                    //index = j;
                                    //break;
                                    userWorkflows.splice(j, 1);
                                }
                            }

                            //if (index > -1) {
                            //    userWorkflows.splice(index, 1);
                            //}
                        }
                        //console.log(userWorkflows);
                    };
                }

                // Check the boxes from the relations in db
                Common.get(uri + "/userWorkflows?u=" + selectedUserId,
                    function (res) {

                        var workflowsTable = document.getElementById("wf-workflows-table");
                        var rows = (workflowsTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
                        for (var i = 0; i < rows.length; i++) {
                            var row = rows[i];
                            var checkBox = row.getElementsByClassName("wf-check")[0].firstChild;
                            var workflowId = parseInt(row.getElementsByClassName("wf-id")[0].innerHTML);
                            var workflowDbId = workflows[workflowId].DbId;

                            for (var j = 0; j < res.length; j++) {
                                if (workflowDbId === res[j].DbId) {
                                    checkBox.checked = true;
                                    //console.log(workflowDbId);
                                    userWorkflows.push({ "UserId": selectedUserId, "WorkflowId": workflowDbId });
                                    break;
                                }
                            }
                        }
                    }, function () {
                        Common.toastError("An error occured while retrieving user workflows.");
                    }, auth);

                document.getElementById("check-all").onchange = function () {
                    var workflowsTable = document.getElementById("wf-workflows-table");
                    var rows = (workflowsTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
                    for (var i = 0; i < rows.length; i++) {
                        var row = rows[i];
                        var checkBox = row.getElementsByClassName("wf-check")[0].firstChild;
                        var workflowId = parseInt(row.getElementsByClassName("wf-id")[0].innerHTML);
                        var workflowDbId = workflows[workflowId].DbId;

                        if (checkBox.checked === true) {
                            checkBox.checked = false;

                            //var index = -1;
                            for (var j = userWorkflows.length - 1; j > -1; j--) {
                                if (userWorkflows[j].WorkflowId === workflowDbId) {
                                    //index = j;
                                    //break;
                                    userWorkflows.splice(j, 1);
                                }
                            }

                            //if (index > -1) {
                            //    userWorkflows.splice(index, 1);
                            //}
                        } else {
                            checkBox.checked = true;
                            userWorkflows.push({ "UserId": selectedUserId, "WorkflowId": workflowDbId });
                        }

                    }

                    //console.log(userWorkflows);
                };

                // End of get workflows
            }, function () {
                Common.toastError("An error occured while retrieving workflows. Check that Wexflow server is running correctly.");
            }, auth);

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

    btnSave.onclick = function () {
        Common.post(uri + "/saveUserWorkflows", function (res) {
            if (res === true) {
                Common.toastSuccess("Workflow relations saved with success.");
            } else {
                Common.toastError("An error occured while saving workflow relations.");
            }
        }, function () {
            Common.toastError("An error occured while saving workflow relations.");
        }, { "UserId": selectedUserId, "UserWorkflows": userWorkflows }, auth);
    };

}