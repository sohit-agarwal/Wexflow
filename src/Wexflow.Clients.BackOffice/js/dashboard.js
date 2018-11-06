function Dashboard () {
    "use strict";

    var uri = Common.trimEnd(Settings.Uri, "/");
    var suser = getUser();

    if (suser === null || suser === "" ) {
        Common.redirectToLoginPage();
    } else {
        var user = JSON.parse(suser);
        Common.get(uri + "/user?username=" + encodeURIComponent(user.Username), function (u) {
            if (user.Password !== u.Password) {
                Common.redirectToLoginPage();
            } else {
                var refreshTimeout = 1000;
                var statusPending = document.getElementById("status-pending-value");
                var statusRunning = document.getElementById("status-running-value");
                var statusDone = document.getElementById("status-done-value");
                var statusFailed = document.getElementById("status-failed-value");
                var statusWarning = document.getElementById("status-warning-value");
                var statusDisabled = document.getElementById("status-disabled-value");
                var statusStopped = document.getElementById("status-stopped-value");
                var btnLogout = document.getElementById("btn-logout");
                var divStatus = document.getElementById("status");
                var divEntries = document.getElementById("entries");

                divStatus.style.display = "block";
                divEntries.style.display = "block";

                btnLogout.onclick = function () {
                    deleteUser();
                    Common.redirectToLoginPage();
                };

                btnLogout.innerHTML = "Logout (" + u.Username + ")";

                setInterval(function () {

                    Common.get(uri + "/statusCount", function (data) {
                        statusPending.innerHTML = data.PendingCount;
                        statusRunning.innerHTML = data.RunningCount;
                        statusDone.innerHTML = data.DoneCount;
                        statusFailed.innerHTML = data.FailedCount;
                        statusWarning.innerHTML = data.WarningCount;
                        statusDisabled.innerHTML = data.DisabledCount;
                        statusStopped.innerHTML = data.StoppedCount;
                    }, function () {
                        //alert("An error occured while retrieving workflows. Check Wexflow Web Service Uri and check that Wexflow Windows Service is running correctly.");
                    });

                    Common.get(uri + "/entries", function (data) {
                        var items = [];
                        for (var i = 0; i < data.length; i++) {
                            var val = data[i];
                            var lt = Common.launchType(val.LaunchType);
                            var estatus = Common.status(val.Status);
                            items.push("<tr>"
                                + "<td class='status'>" + estatus + "</td>"
                                + "<td class='id' title='" + val.WorkflowId + "'>" + val.WorkflowId + "</td>"
                                + "<td class='name'>" + val.Name + "</td>"
                                + "<td class='lt'>" + lt + "</td>"
                                + "<td class='desc' title='" + val.Description + "'>" + val.Description + "</td>"
                                + "</tr>");
                        }

                        var table = "<table id='entries-table' class='table table-hover'>"
                            + "<thead class='thead-dark'>"
                            + "<tr>"
                            + "<th class='status'>Status</th>"
                            + "<th class='id'>Id</th>"
                            + "<th class='name'>Name</th>"
                            + "<th class='lt'>LaunchType</th>"
                            + "<th class='desc'>Description</th>"
                            + "</tr>"
                            + "</thead>"
                            + "<tbody>"
                            + items.join("")
                            + "</tbody>"
                            + "</table>";

                        document.getElementById("entries").innerHTML = table;
                    }, function () {
                        //alert("An error occured while retrieving workflows. Check Wexflow Web Service Uri and check that Wexflow Windows Service is running correctly.");
                    });

                }, refreshTimeout);

            }
        });
    }

}