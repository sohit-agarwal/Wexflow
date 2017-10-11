function WexflowDesigner(id, uri) {
    "use strict";

    hljs.initHighlightingOnLoad();

    uri = trimEnd(uri, "/");
    var selectedId = -1;
    var workflows = {};
    var timer = null;
    var timerInterval = 500; // ms

    var html = "<div id='wf-container'>"
        + "<div id='wf-workflows'></div>"

        +  "<div id='wf-designer-right-panel' style='display: none;'>"
        + "<h3>Workflow <button id= 'wf-xml-button' type= 'button'>XML</button></h3>"
        + "<pre><code id='wf-xml-container' class='xml'></pre>"
        + "<table class='wf-designer-table'>"
        + "<tbody>"
        + "        <tr><td class='wf-title'>Id</td><td class='wf-value'><input id='wf-id' type='text' readonly /></td></tr>"
        + "        <tr><td class='wf-title'>Name</td><td class='wf-value'><input id='wf-name' type='text' readonly/></td></tr>"
        + "        <tr><td class='wf-title'>LaunchType</td><td class='wf-value'><select id='wf-launchType' disabled><option value='startup'>Startup</option><option value='trigger'>Trigger</option><option value='periodic'>Periodic</option></select></td></tr>"
        + "        <tr><td class='wf-title'>Period</td><td class='wf-value'><input id='wf-period' type='text' readonly/></td></tr>"
        + "        <tr><td class='wf-title'>Enabled</td><td class='wf-value'><input id='wf-enabled' type='checkbox' readonly disabled /></td></tr>"
        + "        <tr><td class='wf-title'>Description</td><td class='wf-value'><input id='wf-desc' type='text' readonly/></td></tr>"
        + "        <tr><td class='wf-title'>Status</td><td id='wf-status' class='wf-value'></td></tr>"
        + "</tbody>"
        + "</table>"

        + "        <h3>Tasks</h3>"
        + "        <div id='wf-tasks'>"
        
        + "        </div>"
        + "    </div>"


        + "</div>";

    document.getElementById(id).innerHTML = html;

    /*function disableButton(button, disabled) {
        button.disabled = disabled;
    }*/
    
    function trimEnd(string, charToRemove) {
        while (string.charAt(string.length - 1) === charToRemove) {
            string = string.substring(0, string.length - 1);
        }

        return string;
    }

    function escapeXml(xml) {
        return xml.replace(/[<>&'"]/g, function (c) {
            switch (c) {
            case '<': return '&lt;';
            case '>': return '&gt;';
            case '&': return '&amp;';
            case '\'': return '&apos;';
            case '"': return '&quot;';
            }
            return c;
        });
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
                return "startup";
            case 1:
                return "trigger";
            case 2:
                return "periodic";
            default:
                return "";
        }
    }

    function setSelectedIndex(s, v) {
        for (var i = 0; i < s.options.length; i++) {
            if (s.options[i].value === v) {
                s.options[i].selected = true;
                return;
            }
        }
    }

    get(uri + "/workflows", function (data) {
        data.sort(compareById);
        var items = [];
        var i;
        for (i = 0; i < data.length; i++) {
            var val = data[i];
            workflows[val.Id] = val;
            
            items.push("<tr>"
                          + "<td class='wf-id' title='" + val.Id + "'>" + val.Id + "</td>"
                          + "<td class='wf-n' title='" + val.Name + "'>" + val.Name + "</td>"
                         + "</tr>");

        }

        var table = "<table id='wf-workflows-table' class='table table-striped'>"
                        + "<thead>"
                        + "<tr>"
                         + "<th class='wf-id'>Id</th>"
                         + "<th class='wf-n'>Name</th>"
                        + "</tr>"
                        + "</thead>"
                        + "<tbody>"
                        + items.join("")
                        + "</tbody>"
                       + "</table>";

        document.getElementById("wf-workflows").innerHTML = table;

        var workflowsTable = document.getElementById("wf-workflows-table");

        function getWorkflow(wid, func) {
            get(uri + "/workflow/" + wid, function (w) {
                func(w);
            });
        }

        function getTasks(wid, func) {
            get(uri + "/tasks/" + wid, function (tasks) {
                func(tasks);
            });
        }

        function getXml(wid, func) {
            get(uri + "/xml/" + wid, function (tasks) {
                func(tasks);
            });
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

                document.getElementById('wf-designer-right-panel').style.display = 'block';

                var xmlContainer = document.getElementById("wf-xml-container");

                xmlContainer.innerHTML = '';
                document.getElementById("wf-xml-button").onclick = function () {
                    getXml(selectedId, function (xml) {
                        
                        xmlContainer.innerHTML = escapeXml(xml);
                        hljs.highlightBlock(xmlContainer);
                    });
                };

                getWorkflow(selectedId,
                    function (workflow) {
                        document.getElementById("wf-id").value = workflow.Id;
                        document.getElementById("wf-name").value = workflow.Name;
                        var lt = launchType(workflow.LaunchType);
                        setSelectedIndex(document.getElementById("wf-launchType"), lt);
                        document.getElementById("wf-period").value = workflow.Period;
                        document.getElementById("wf-enabled").checked = workflow.IsEnabled;
                        document.getElementById("wf-desc").value = workflow.Description;

                        // Status
                        clearInterval(timer);

                        if (workflow.IsEnabled === true) {
                            timer = setInterval(function () {
                                updateStatus(selectedId, false);
                            }, timerInterval);

                            updateStatus(selectedId, true);
                        } else {
                            updateStatus(selectedId, true);
                        }

                        // Tasks
                        getTasks(selectedId,
                            function(tasks) {

                                var tasksHtml = "";

                                for (var i = 0; i < tasks.length; i++) {
                                    var task = tasks[i];

                                    tasksHtml +=
                                        "<div id='wf-task'>" +
                                        "<h5 class='wf-task-title'>Task " + task.Id + "</h5>" +
                                        "<table class='wf-designer-table'>" +
                                        "<tbody>" +
                                        "        <tr><td class='wf-title'>Id</td><td class='wf-value'><input id='wf-task-id' type='text' value='" + task.Id +"' readonly /></td></tr>" +
                                        "        <tr><td class='wf-title'>Name</td><td class='wf-value'><input id='wf-task-name' type='text' value='" + task.Name + "' readonly/></td></tr>" +
                                        "        <tr><td class='wf-title'>Description</td><td class='wf-value'><input id='wf-task-desc' type='text' value='"+ task.Description+"' readonly/></td></tr>" +
                                        "        <tr><td class='wf-title'>Enabled</td><td class='wf-value'><input id='wf-task-enabled' type='checkbox' readonly disabled " + (task.IsEnabled ? "checked" : "") + " /></td></tr>" +
                                        "</tbody>" +
                                        "</table>" +
                                        "<h6>Settings</h6>" +
                                        "<table class='wf-designer-table'>" +
                                        "<tbody>";

                                         for (var j = 0; j < task.Settings.length; j++) {
                                             var setting = task.Settings[j];
                                             tasksHtml +=
                                                 "<tr><td class='wf-title'>" + setting.Name + "</td><td class='wf-setting-value'>";

                                             if (setting.Value !== '') {
                                                 tasksHtml += "<input type='text' value='" + setting.Value + "' readonly />";
                                             }

                                             if (setting.Attributes.length > 0) {
                                                 tasksHtml += "<table>";

                                                 for (var k = 0; k < setting.Attributes.length; k++) {
                                                     var attr = setting.Attributes[k];
                                                     tasksHtml += "<tr>" +
                                                         "<td><input type='text' value='" + attr.Name + "' readonly />" +
                                                         "<td><input type='text' value='" + attr.Value + "' readonly />" +
                                                         "</tr>";
                                                 }

                                                 tasksHtml += "</table>";
                                             }
                                             
                                             tasksHtml +="</td></tr>";
                                         }

                                        tasksHtml += "</tbody>" +
                                        "</table>" +
                                        "</div > ";
                                }

                                document.getElementById("wf-tasks").innerHTML = tasksHtml;
                            });
                    });

            };
        }

        function workflowStatusChanged(workflow) {
            var changed = workflows[workflow.Id].IsRunning !== workflow.IsRunning || workflows[workflow.Id].IsPaused !== workflow.IsPaused;
            workflows[workflow.Id].IsRunning = workflow.IsRunning;
            workflows[workflow.Id].IsPaused = workflow.IsPaused;
            return changed;
        }

        function updateStatus(workflowId, force) {
            getWorkflow(workflowId, function (workflow) {
                if (workflow.IsEnabled === false) {
                    notify("This workflow is disabled.");
                }
                else {
                    if (force === false && workflowStatusChanged(workflow) === false) return;

                    if (workflow.IsRunning === true && workflow.IsPaused === false) {
                        notify("This workflow is running...");
                    }
                    else if (workflow.IsPaused === true) {
                        notify("This workflow is suspended.");
                    }
                    else {
                        notify("This workflow is not running.");
                    }
                }
            });
        }

        function notify(status) {
            document.getElementById("wf-status").innerHTML = status;
        }

        // End of get workflows
    }, function () {
        alert("An error occured while retrieving workflows. Check Wexflow Web Service Uri and check that Wexflow Windows Service is running correctly.");
    });

    function get(url, callback, errorCallback) {
        var xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function () {
            if (this.readyState === 4 && this.status === 200 && callback) {
                var data = JSON.parse(this.responseText);
                callback(data);
            }
        };
        xmlhttp.onerror = function () {
            if (errorCallback) errorCallback();
        };
        xmlhttp.open("GET", url, true);
        xmlhttp.send();
    }

    /*function post(url, callback) {
        var xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function () {
            if (this.readyState === 4 && this.status === 200 && callback) {
                callback();
            }
        };
        xmlhttp.open("POST", url, true);
        xmlhttp.send();
    }*/

    // End of wexflow Designer
}