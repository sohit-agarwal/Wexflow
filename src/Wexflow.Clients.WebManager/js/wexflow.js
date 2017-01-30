function Wexflow(id, uri) {

    uri = trimEnd(uri, "/");
    var selectedId = -1;
    var workflows = {};
    var timer = null;
    var timerInterval = 500; // ms

    var html = "<div id='wf-container'>"
                    + "<div id='wf-cmd'>"
                        + "<button id='wf-start' type='button'>Start</button>"
                        + "<button id='wf-pause' type='button'>Suspend</button>"
                        + "<button id='wf-resume' type='button'>Resume</button>"
                        + "<button id='wf-stop' type='button'>Stop</button>"
                    + "</div>"
                    + "<div id='wf-notifier'>"
                        + "<input id='wf-notifier-text' type='text' name='fname' readonly>"
                    + "</div>"
                    + "<div id='wf-workflows'>"
                    + "</div>"
              + "</div>";

    document.getElementById(id).innerHTML = html;

    function trimEnd(string, charToRemove) {
        while (string.charAt(string.length - 1) == charToRemove) {
            string = string.substring(0, string.length - 1);
        }

        return string;
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
            default:
                return "";
        }
    }

    get(uri + "/workflows", function (data) {
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
                          + "<td class='wf-e'>" + val.IsEnabled + "</td>"
                          + "<td class='wf-d' title='" + val.Description + "'>" + val.Description + "</td>"
                         + "</tr>");

        }

        var table = "<table id='wf-workflows-table' class='table table-striped'>"
                        + "<thead>"
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
            get(uri + "/workflow/" + wid, function (d) {
                func(d);
            });
        }

        function notify(msg) {
            document.getElementById("wf-notifier-text").value = msg;
        }

        var startButton = document.getElementById("wf-start");
        var suspendButton = document.getElementById("wf-pause");
        var resumeButton = document.getElementById("wf-resume");
        var stopButton = document.getElementById("wf-stop");

        function updateButtons(wid, force) {
            getWorkflow(wid, function (workflow) {
                if (workflow.IsEnabled === false) {
                    notify("This workflow is disabled.");
                    disableButton(startButton, true);
                    disableButton(suspendButton, true);
                    disableButton(resumeButton, true);
                    disableButton(stopButton, true);
                }
                else {
                    if (force === false && workflowStatusChanged(workflow) === false) return;

                    disableButton(startButton, workflow.IsRunning);
                    disableButton(stopButton, !(workflow.IsRunning && !workflow.IsPaused));
                    disableButton(suspendButton, !(workflow.IsRunning && !workflow.IsPaused));
                    disableButton(resumeButton, !workflow.IsPaused);

                    if (workflow.IsRunning === true && workflow.IsPaused === false) {
                        notify("This workflow is running...");
                    }
                    else if (workflow.IsPaused === true) {
                        notify("This workflow is paused.");
                    }
                    else {
                        notify("");
                    }
                }
            });
        }

        function workflowStatusChanged(workflow) {
            var changed = workflows[workflow.Id].IsRunning !== workflow.IsRunning || workflows[workflow.Id].IsPaused !== workflow.IsPaused;
            workflows[workflow.Id].IsRunning = workflow.IsRunning;
            workflows[workflow.Id].IsPaused = workflow.IsPaused;

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

                if (workflows[selectedId].IsEnabled === true) {
                    clearInterval(timer);
                    timer = setInterval(function () {
                        updateButtons(selectedId, false);
                    }, timerInterval);

                    updateButtons(selectedId, true);
                } else {
                    updateButtons(selectedId, true);
                }
            };
        }

        function disableButton(button, disabled) {
            button.disabled = disabled;
        }

        disableButton(startButton, true);
        startButton.onclick = function () {
            var startUri = uri + "/start/" + selectedId;
            post(startUri);
        };

        disableButton(suspendButton, true);
        suspendButton.onclick = function () {
            var suspendUri = uri + "/suspend/" + selectedId;
            post(suspendUri, function () {
                updateButtons(selectedId, true);
            });
        };

        disableButton(resumeButton, true);
        resumeButton.onclick = function () {
            var resumeUri = uri + "/resume/" + selectedId;
            post(resumeUri, function () {
            });
        };

        disableButton(stopButton, true);
        stopButton.onclick = function () {
            var stopUri = uri + "/stop/" + selectedId;
            post(stopUri, function () {
                updateButtons(selectedId, true);
            });
        };

        // End of get workflows
    });

    function get(url, callback) {
        var xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                var data = JSON.parse(this.responseText);
                callback(data);
            }
        };
        xmlhttp.open("GET", url, true);
        xmlhttp.send();
    }

    function post(url, callback) {
        var xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                callback();
            }
        };
        xmlhttp.open("POST", url, true);
        xmlhttp.send();
    }

    // End of wexflow
}