window.onload = function () {
    "use strict";

    let uri = Common.trimEnd(Settings.Uri, "/");
    let lnkManager = document.getElementById("lnk-manager");
    let lnkDesigner = document.getElementById("lnk-designer");
    let lnkEditor = document.getElementById("lnk-editor");
    let lnkApproval = document.getElementById("lnk-approval");
    let lnkUsers = document.getElementById("lnk-users");
    let lnkProfiles = document.getElementById("lnk-profiles");
    let navigation = document.getElementById("navigation");
    let leftcard = document.getElementById("leftcard");
    let propwrap = document.getElementById("propwrap");
    let wfclose = document.getElementById("wfclose");
    let wfpropwrap = document.getElementById("wfpropwrap");
    let canvas = document.getElementById("canvas");
    let suser = getUser();
    let username = "";
    let password = "";
    let auth = "";

    if (suser === null || suser === "") {
        Common.redirectToLoginPage();
    } else {
        let user = JSON.parse(suser);

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

                        navigation.style.display = "block";
                        leftcard.style.display = "block";
                        propwrap.style.display = "block";
                        wfclose.style.display = "block";
                        wfpropwrap.style.display = "block";
                        canvas.style.display = "block";

                        let btnLogout = document.getElementById("btn-logout");
                        btnLogout.onclick = function () {
                            deleteUser();
                            Common.redirectToLoginPage();
                        };

                        btnLogout.innerHTML = "Logout (" + u.Username + ")";

                        load();
                    } else {
                        Common.redirectToLoginPage();
                    }

                }
            },
            function () { }, auth);
    }

    function load() {
        let searchtasks = document.getElementById("searchtasks");
        let leftcardHidden = true;
        let leftcardwidth = 361;
        let closecardimg = document.getElementById("closecardimg");
        let wfpropHidden = true;
        let wfpropwidth = 331;
        let closewfcardimg = document.getElementById("wfcloseimg");
        let wfclose = document.getElementById("wfclose");
        let code = document.getElementById("code-container");
        let rightcard = false;
        let tempblock;
        let tempblock2;
        let tasks = {};
        let editor = null;
        let checkId = true;
        let removeworkflow = document.getElementById("removeworkflow");

        flowy(canvas, drag, release, snapping);

        function loadTasks() {
            Common.get(uri + "/searchTaskNames?s=" + searchtasks.value,
                function (taskNames) {
                    let blockelements = "";
                    for (let i = 0; i < taskNames.length; i++) {
                        let taskName = taskNames[i];
                        blockelements += '<div class="blockelem create-flowy noselect"><input type="hidden" name="blockelemtype" class="blockelemtype" value="' + taskName.Name + '"><input type="hidden" name="blockelemdesc" class="blockelemdesc" value="' + taskName.Description + '"><div class="grabme"><img src="assets/grabme.svg"></div><div class="blockin"><div class="blockico"><span></span><img src="assets/action.svg"></div><div class="blocktext"><p class="blocktitle">' + taskName.Name + '</p><p class="blockdesc">' + taskName.Description + '</p></div></div></div>';
                    }
                    let blocklist = document.getElementById("blocklist");
                    blocklist.innerHTML = blockelements;
                },
                function () {
                    Common.toastError("An error occured while retrieving task names.");
                }, auth);
        }
        loadTasks();

        searchtasks.onkeyup = function (event) {
            event.preventDefault();
            if (event.keyCode === 13) { // Enter
                loadTasks();
            }
        };

        document.getElementById("newworkflow").onclick = function () {
            checkId = true;
            flowy.deleteBlocks();
            removeworkflow.style.display = "none";

            document.getElementById("leftcard").style.left = "0";
            leftcardHidden = false;
            canvas.style.left = leftcardwidth + "px";
            canvas.style.width = "calc(100% - " + leftcardwidth + "px)";
            closecardimg.src = "assets/closeleft.png";

            document.getElementById("wfpropwrap").style.right = "0";
            wfclose.style.right = "311px";
            wfpropHidden = false;
            closewfcardimg.src = "assets/openleft.png";

            if (rightcard === true) {
                rightcard = false;
                document.getElementById("properties").classList.remove("expanded");
                setTimeout(function () {
                    document.getElementById("propwrap").classList.remove("itson");
                }, 300);
                if (tempblock) {
                    tempblock.classList.remove("selectedblock");
                }
            }

            document.getElementById("wfid").value = "";
            document.getElementById("wfname").value = "";
            document.getElementById("wfdesc").value = "";
            document.getElementById("wflaunchtype").value = "";
            document.getElementById("wfperiod").onkeyup = "";
            document.getElementById("wfcronexp").value = "";
            document.getElementById("wfenabled").checked = true;
            document.getElementById("wfapproval").checked = false;
            document.getElementById("wfenablepj").checked = true;

            workflow = {
                "WorkflowInfo": {
                    "Id": document.getElementById("wfid").value,
                    "Name": document.getElementById("wfname").value,
                    "Description": document.getElementById("wfdesc").value,
                    "LaunchType": launchTypeReverse(document.getElementById("wflaunchtype").value),
                    "Period": document.getElementById("wfperiod").value,
                    "CronExpression": document.getElementById("wfcronexp").value,
                    "IsEnabled": document.getElementById("wfenabled").checked,
                    "IsApproval": document.getElementById("wfapproval").checked,
                    "EnableParallelJobs": document.getElementById("wfenablepj").checked,
                    "LocalVariables": []
                },
                "Tasks": []
            }
            tasks = {};

        };

        function addEventListenerMulti(type, listener, capture, selector) {
            let nodes = document.querySelectorAll(selector);
            for (let i = 0; i < nodes.length; i++) {
                nodes[i].addEventListener(type, listener, capture);
            }
        }

        function snapping(drag, first) {
            let grab = drag.querySelector(".grabme");
            grab.parentNode.removeChild(grab);
            let blockin = drag.querySelector(".blockin");
            blockin.parentNode.removeChild(blockin);

            let taskname = drag.querySelector(".blockelemtype").value;
            let taskdesc = drag.querySelector(".blockelemdesc").value;
            drag.innerHTML += "<div class='blockyleft'><img src='assets/actionorange.svg'><p class='blockyname'>" + taskname + "</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>" + taskdesc + "</div>";

            let index = parseInt(drag.querySelector(".blockid").value);
            if (!tasks[index]) {
                tasks[index] = {
                    "Id": 0,
                    "Name": taskname,
                    "Description": "",
                    "IsEnabled": true,
                    "Settings": []
                };
                updateTasks();
            }



            return true;
        }

        function drag(block) {
            block.classList.add("blockdisabled");
            tempblock2 = block;
        }

        function release() {
            if (tempblock2) {
                tempblock2.classList.remove("blockdisabled");
            }
        }

        function closeTaskSettings() {
            if (rightcard) {
                rightcard = false;
                document.getElementById("properties").classList.remove("expanded");
                setTimeout(function () {
                    document.getElementById("propwrap").classList.remove("itson");
                    wfclose.style.right = "0";
                }, 300);
                tempblock.classList.remove("selectedblock");
            }
        }

        document.getElementById("close").addEventListener("click", function () {
            closeTaskSettings();
        });

        document.getElementById("removeblock").addEventListener("click", function () {
            var confirmRes = confirm("Are you sure you want to delete all the tasks?");
            if (confirmRes === true) {
                flowy.deleteBlocks();
                closeTaskSettings();
            }
        });

        removeworkflow.addEventListener("click", function () {
            let workflowId = document.getElementById("wfid").value;
            var confirmRes = confirm("Are you sure you want to delete the workflow " + workflowId + "?");
            if (confirmRes === true) {
                Common.post(uri + "/delete?w=" + workflowId,
                    function (res) {
                        if (res === true) {
                            Common.toastSuccess("Workflow " + workflowId + " deleted with success.");

                            document.getElementById("wfid").value = "";
                            document.getElementById("wfname").value = "";
                            document.getElementById("wfdesc").value = "";
                            document.getElementById("wflaunchtype").value = "";
                            document.getElementById("wfperiod").onkeyup = "";
                            document.getElementById("wfcronexp").value = "";
                            document.getElementById("wfenabled").checked = true;
                            document.getElementById("wfapproval").checked = false;
                            document.getElementById("wfenablepj").checked = true;

                            document.getElementById("wfpropwrap").style.right = -wfpropwidth + "px";
                            wfpropHidden = true;
                            closewfcardimg.src = "assets/closeleft.png";
                            wfclose.style.right = "0";

                            document.getElementById("leftcard").style.left = -leftcardwidth + "px";
                            closecardimg.src = "assets/openleft.png";
                            leftcardHidden = true;

                            workflow = {
                                "WorkflowInfo": {
                                    "Id": document.getElementById("wfid").value,
                                    "Name": document.getElementById("wfname").value,
                                    "Description": document.getElementById("wfdesc").value,
                                    "LaunchType": launchTypeReverse(document.getElementById("wflaunchtype").value),
                                    "Period": document.getElementById("wfperiod").value,
                                    "CronExpression": document.getElementById("wfcronexp").value,
                                    "IsEnabled": document.getElementById("wfenabled").checked,
                                    "IsApproval": document.getElementById("wfapproval").checked,
                                    "EnableParallelJobs": document.getElementById("wfenablepj").checked,
                                    "LocalVariables": []
                                },
                                "Tasks": []
                            }

                            tasks = {};

                        } else {
                            Common.toastError("An error occured while deleting the workflow" + workflowId + ".");
                        }
                    }, function () {
                        Common.toastError("An error occured while deleting the workflow" + workflowId + ".");
                    }, "", auth);
            }
        });

        let aclick = false;
        let beginTouch = function (event) {
            aclick = true;
        }
        let checkTouch = function (event) {
            aclick = false;
        }

        let doneTouch = function (event) {
            if (event.type === "mouseup") {
                updateTasks();
            }

            if (event.type === "mouseup" && aclick && event.target.closest("#canvas")) {
                if (event.target.closest(".block")) {
                    let selectedBlocks = document.getElementsByClassName("selectedblock");
                    for (let i = 0; i < selectedBlocks.length; i++) {
                        selectedBlocks[i].classList.remove("selectedblock");
                    }
                    tempblock = event.target.closest(".block");
                    rightcard = true;
                    document.getElementById("properties").classList.add("expanded");
                    document.getElementById("propwrap").classList.add("itson");
                    tempblock.classList.add("selectedblock");

                    document.getElementById("wfpropwrap").style.right = -wfpropwidth + "px";
                    wfpropHidden = true;
                    closewfcardimg.src = "assets/closeleft.png";
                    wfclose.style.right = "-60px";

                    // task settings
                    let taskname = tempblock.getElementsByClassName("blockelemtype")[0].value;
                    let proplist = document.getElementById("proplist");

                    document.getElementById("header2").innerHTML = "Task Settings&nbsp;<a title='Task Documentation' href='https://github.com/aelassas/Wexflow/wiki/" + taskname + "' target='_blank'><img src='assets/doc.png'></a>";
                    proplist.innerHTML = '<p class="inputlabel">Id</p><input id="taskid" class="inputtext" type="text" /><p class="inputlabel">Description</p><input id="taskdescription" class="inputtext" type="text" /><p class="inputlabel">Enabled</p><input id="taskenabled" class="inputtext" type="checkbox" checked />';

                    let index = parseInt(event.target.closest(".block").childNodes[2].value);

                    if (!tasks[index]) {
                        tasks[index] = {
                            "Id": 0,
                            "Name": taskname,
                            "Description": "",
                            "IsEnabled": true,
                            "Settings": []
                        };
                    }

                    updateTasks();

                    Common.get(uri + "/settings/" + taskname,
                        function (settings) {
                            let tasksettings = "";
                            for (let i = 0; i < settings.length; i++) {
                                let settingName = settings[i];
                                tasksettings += '<p class="wf-setting-name">' + settingName + '</p><input class="wf-setting-index" type="hidden" value="' + i + '"><input class="wf-setting-value inputtext" value="' + (tasks[index].Settings[i] ? tasks[index].Settings[i].Value : "") + '" type="text" />';

                                if (!tasks[index].Settings[i]) {
                                    tasks[index].Settings.push({
                                        "Name": settingName,
                                        "Value": "",
                                        "Attributes": []
                                    });
                                }
                            }

                            proplist.innerHTML = proplist.innerHTML + tasksettings;

                            document.getElementById("taskid").value = tasks[index].Id;
                            document.getElementById("taskdescription").value = tasks[index].Description;
                            document.getElementById("taskenabled").checked = tasks[index].IsEnabled;

                            document.getElementById("taskid").onkeyup = function () {
                                tasks[index].Id = this.value;

                                updateTasks();
                            };

                            document.getElementById("taskdescription").onkeyup = function () {
                                tasks[index].Description = this.value;

                                updateTasks();
                            };

                            document.getElementById("taskenabled").onchange = function () {
                                tasks[index].IsEnabled = this.checked;

                                updateTasks();
                            };

                            let settingValues = document.getElementsByClassName("wf-setting-value");
                            for (let i = 0; i < settingValues.length; i++) {
                                let settingValue = settingValues[i];

                                settingValue.onkeyup = function () {
                                    let sindex = this.previousElementSibling.value;
                                    tasks[index].Settings[sindex].Value = this.value;

                                    updateTasks();
                                };
                            }

                        },
                        function () {
                            Common.toastError("An error occured while retrieving settings.");
                        }, auth);

                }
            }
        }
        addEventListener("mousedown", beginTouch, false);
        addEventListener("mousemove", checkTouch, false);
        addEventListener("mouseup", doneTouch, false);
        addEventListenerMulti("touchstart", beginTouch, false, ".block");

        document.getElementById("closecard").onclick = function () {
            let blockelems = canvas.getElementsByClassName("blockelem");
            let arrowblocks = canvas.getElementsByClassName("arrowblock");

            if (leftcardHidden === false) {

                document.getElementById("leftcard").style.left = -leftcardwidth + "px";
                closecardimg.src = "assets/openleft.png";
                leftcardHidden = true;
                canvas.style.left = "0";
                canvas.style.width = "100%";

                for (let i = 0; i < blockelems.length; i++) {
                    let blockelm = blockelems[i];
                    blockelm.style.left = (blockelm.offsetLeft + leftcardwidth) + "px";
                }

                for (let i = 0; i < arrowblocks.length; i++) {
                    let arrowblock = arrowblocks[i];
                    arrowblock.style.left = (arrowblock.offsetLeft + leftcardwidth) + "px";
                }

            } else {

                document.getElementById("leftcard").style.left = "0";
                closecardimg.src = "assets/closeleft.png";
                leftcardHidden = false;
                canvas.style.left = leftcardwidth + "px";
                canvas.style.width = "calc(100% - " + leftcardwidth + "px)";

                for (let i = 0; i < blockelems.length; i++) {
                    let blockelm = blockelems[i];
                    blockelm.style.left = (blockelm.offsetLeft - leftcardwidth) + "px";
                }

                for (let i = 0; i < arrowblocks.length; i++) {
                    let arrowblock = arrowblocks[i];
                    arrowblock.style.left = (arrowblock.offsetLeft - leftcardwidth) + "px";
                }

            }
        };


        wfclose.onclick = function () {
            if (wfpropHidden === false) {
                document.getElementById("wfpropwrap").style.right = -wfpropwidth + "px";
                wfclose.style.right = "0";
                wfpropHidden = true;
                closewfcardimg.src = "assets/closeleft.png";
            } else {
                document.getElementById("wfpropwrap").style.right = "0";
                wfclose.style.right = "311px";
                wfpropHidden = false;
                closewfcardimg.src = "assets/openleft.png";

            }
        };

        // CTRL+S
        let workflow = {
            "WorkflowInfo": {
                "Id": document.getElementById("wfid").value,
                "Name": document.getElementById("wfname").value,
                "Description": document.getElementById("wfdesc").value,
                "LaunchType": launchTypeReverse(document.getElementById("wflaunchtype").value),
                "Period": document.getElementById("wfperiod").value,
                "CronExpression": document.getElementById("wfcronexp").value,
                "IsEnabled": document.getElementById("wfenabled").checked,
                "IsApproval": document.getElementById("wfapproval").checked,
                "EnableParallelJobs": document.getElementById("wfenablepj").checked,
                "LocalVariables": []
            },
            "Tasks": []
        }
        let diag = true;
        let json = false;
        let xml = false;

        document.getElementById("wfid").onkeyup = function () {
            workflow.WorkflowInfo.Id = this.value;
        };
        document.getElementById("wfname").onkeyup = function () {
            workflow.WorkflowInfo.Name = this.value;
        };
        document.getElementById("wfdesc").onkeyup = function () {
            workflow.WorkflowInfo.Description = this.value;
        };
        document.getElementById("wflaunchtype").onchange = function () {
            workflow.WorkflowInfo.LaunchType = launchTypeReverse(this.value);
        };
        document.getElementById("wfperiod").onkeyup = function () {
            workflow.WorkflowInfo.Period = this.value;
        };
        document.getElementById("wfcronexp").onkeyup = function () {
            workflow.WorkflowInfo.CronExpression = this.value;
        };
        document.getElementById("wfenabled").onchange = function () {
            workflow.WorkflowInfo.IsEnabled = this.checked;
        };
        document.getElementById("wfapproval").onchange = function () {
            workflow.WorkflowInfo.IsApproval = this.checked;
        };
        document.getElementById("wfenablepj").onchange = function () {
            workflow.WorkflowInfo.EnableParallelJobs = this.checked;
        };

        function updateTasks() {
            let length = 0;
            while (tasks[length]) {
                length++;
            }
            workflow.Tasks = [];
            for (let i = 0; i < length; i++) {
                workflow.Tasks.push(tasks[i]);
            }
        }

        window.onkeydown = function (event) {
            if ((event.ctrlKey || event.metaKey || event.keyCode === 17 || event.keyCode === 224 || event.keyCode === 91 || event.keyCode === 93) && event.keyCode === 83) {
                event.preventDefault();
                let wfid = document.getElementById("wfid").value;

                if (diag === true) {
                    updateTasks();

                    let saveFunc = function () {
                        Common.post(uri + "/save", function (res) {
                            if (res === true) {
                                checkId = false;
                                removeworkflow.style.display = "block";
                                Common.toastSuccess("workflow " + wfid + " saved and loaded with success from diagram view.");
                            } else {
                                Common.toastError("An error occured while saving the workflow " + wfid + " from diagram view.");
                            }
                        }, function () {
                            Common.toastError("An error occured while saving the workflow " + wfid + " from diagram view.");
                        }, workflow, auth);
                    };

                    let wfIdStr = document.getElementById("wfid").value;
                    if (isInt(wfIdStr)) {
                        let workflowId = parseInt(wfIdStr);

                        if (checkId === true) {
                            Common.get(uri + "/isWorkflowIdValid/" + workflowId,
                                function (res) {
                                    if (res === true) {
                                        if (document.getElementById("wfname").value === "") {
                                            Common.toastInfo("Enter a name for this workflow.");
                                        } else {
                                            let lt = document.getElementById("wflaunchtype").value;
                                            if (lt === "") {
                                                Common.toastInfo("Select a launchType for this workflow.");
                                            } else {
                                                if (lt === "periodic" && document.getElementById("wfperiod").value === "") {
                                                    Common.toastInfo("Enter a period for this workflow.");
                                                } else {
                                                    if (lt === "cron" && document.getElementById("wfcronexp").value === "") {
                                                        Common.toastInfo("Enter a cron expression for this workflow.");
                                                    } else {

                                                        // Period validation
                                                        if (lt === "periodic" && document.getElementById("wfperiod").value !== "") {
                                                            let period = document.getElementById("wfperiod").value;
                                                            Common.get(uri + "/isPeriodValid/" + period,
                                                                function (res) {
                                                                    if (res === true) {
                                                                        saveFunc();
                                                                    } else {
                                                                        Common.toastInfo("The period format is not valid. The valid format is: dd.hh:mm:ss");
                                                                    }
                                                                },
                                                                function () { }, auth
                                                            );
                                                        } // Cron expression validation
                                                        else if (lt === "cron" && document.getElementById("wfcronexp").value !== "") {
                                                            let expression = document.getElementById("wfcronexp").value;
                                                            let expressionEncoded = encodeURIComponent(expression);

                                                            Common.get(uri + "/isCronExpressionValid?e=" + expressionEncoded,
                                                                function (res) {
                                                                    if (res === true) {
                                                                        saveFunc();
                                                                    } else {
                                                                        if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                                            openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                                        }
                                                                    }
                                                                },
                                                                function () { }, auth
                                                            );
                                                        } else {
                                                            saveFunc();
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    } else {
                                        Common.toastInfo("The workflow id is already in use. Enter another one.");
                                    }
                                },
                                function () { }, auth
                            );
                        } else {

                            if (document.getElementById("wfname").value === "") {
                                Common.toastInfo("Enter a name for this workflow.");
                            } else {
                                let lt = document.getElementById("wflaunchtype").value;
                                if (lt === "") {
                                    Common.toastInfo("Select a launchType for this workflow.");
                                } else {
                                    if (lt === "periodic" && document.getElementById("wfperiod").value === "") {
                                        Common.toastInfo("Enter a period for this workflow.");
                                    } else {
                                        if (lt === "cron" && document.getElementById("wfcronexp").value === "") {
                                            Common.toastInfo("Enter a cron expression for this workflow.");
                                        } else {

                                            // Period validation
                                            if (lt === "periodic" && document.getElementById("wfperiod").value !== "") {
                                                let period = document.getElementById("wfperiod").value;
                                                Common.get(uri + "/isPeriodValid/" + period,
                                                    function (res) {
                                                        if (res === true) {
                                                            saveFunc();
                                                        } else {
                                                            Common.toastInfo("The period format is not valid. The valid format is: dd.hh:mm:ss");
                                                        }
                                                    },
                                                    function () { }, auth
                                                );
                                            } // Cron expression validation
                                            else if (lt === "cron" && document.getElementById("wfcronexp").value !== "") {
                                                let expression = document.getElementById("wfcronexp").value;
                                                let expressionEncoded = encodeURIComponent(expression);

                                                Common.get(uri + "/isCronExpressionValid?e=" + expressionEncoded,
                                                    function (res) {
                                                        if (res === true) {
                                                            saveFunc();
                                                        } else {
                                                            if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                                openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                            }
                                                        }
                                                    },
                                                    function () { }, auth
                                                );
                                            } else {
                                                saveFunc();
                                            }

                                        }
                                    }
                                }
                            }

                        }

                    } else {
                        Common.toastInfo("Enter a valid workflow id.");
                    }

                } else if (json === true) {
                    let json = JSON.parse(editor.getValue());
                    Common.post(uri + "/save", function (res) {
                        if (res === true) {
                            removeworkflow.style.display = "block";
                            Common.toastSuccess("workflow " + wfid + " saved and loaded with success from JSON view.");
                        } else {
                            Common.toastError("An error occured while saving the workflow " + wfid + " from JSON view.");
                        }
                    }, function () {
                        Common.toastError("An error occured while saving the workflow " + wfid + " from JSON view.");
                    }, json, auth);
                } else if (xml === true) {
                    let json = {
                        workflowId: workflow.WorkflowInfo.Id,
                        xml: editor.getValue()
                    };
                    Common.post(uri + "/saveXml", function (res) {
                        if (res === true) {
                            removeworkflow.style.display = "block";
                            Common.toastSuccess("workflow " + wfid + " saved and loaded with success from XML view.");
                        } else {
                            Common.toastError("An error occured while saving the workflow " + wfid + " from XML view.");
                        }
                    }, function () {
                        Common.toastError("An error occured while saving the workflow " + wfid + " from XML view.");
                    }, json, auth);
                }

                return false;
            }
        };

        function launchTypeReverse(lt) {
            switch (lt) {
                case "startup":
                    return 0;
                case "trigger":
                    return 1;
                case "periodic":
                    return 2;
                case "cron":
                    return 3;
                default:
                    return -1;
            }
        }

        function launchType(lt) {
            switch (lt) {
                case 0:
                    return "startup";
                case 1:
                    return "trigger";
                case 2:
                    return "periodic";
                case 3:
                    return "cron";
                default:
                    return "";
            }
        }

        function isInt(str) {
            return /^\+?(0|[1-9]\d*)$/.test(str);
        }

        // diagram click
        document.getElementById("leftswitch").onclick = function () {
            diag = true;
            json = false;
            xml = false;

            leftcard.style.display = "block";
            propwrap.style.display = "block";
            wfclose.style.display = "block";
            wfpropwrap.style.display = "block";
            canvas.style.display = "block";
            code.style.display = "none";

            this.style.backgroundColor = "#F0F0F0";
            document.getElementById("middleswitch").style.backgroundColor = "transparent";
            document.getElementById("rightswitch").style.backgroundColor = "transparent"

        };

        // json click
        function openJsonView(jsonVal) {
            diag = false;
            json = true;
            xml = false;

            leftcard.style.display = "none";
            propwrap.style.display = "none";
            wfclose.style.display = "none";
            wfpropwrap.style.display = "none";
            canvas.style.display = "none";
            code.style.display = "block";

            document.getElementById("middleswitch").style.backgroundColor = "#F0F0F0";
            document.getElementById("leftswitch").style.backgroundColor = "transparent";
            document.getElementById("rightswitch").style.backgroundColor = "transparent";

            editor = ace.edit("code");
            editor.setOptions({
                maxLines: Infinity,
                autoScrollEditorIntoView: true
            });

            editor.setReadOnly(false);
            editor.setFontSize("100%");
            editor.setPrintMarginColumn(false);
            editor.getSession().setMode("ace/mode/json");

            editor.commands.addCommand({
                name: "showKeyboardShortcuts",
                bindKey: { win: "Ctrl-Alt-h", mac: "Command-Alt-h" },
                exec: function (editor) {
                    ace.config.loadModule("ace/ext/keybinding_menu", function (module) {
                        module.init(editor);
                        editor.showKeyboardShortcuts()
                    })
                }
            });

            editor.setValue(jsonVal, -1);
            editor.clearSelection();
            editor.resize(true);
            editor.focus();
        }

        document.getElementById("middleswitch").onclick = function () {

            let jsonVal = JSON.stringify(workflow, null, '\t');

            if (diag === true) {
                let wfIdStr = document.getElementById("wfid").value;
                if (isInt(wfIdStr)) {
                    let workflowId = parseInt(wfIdStr);

                    if (checkId === true) {
                        Common.get(uri + "/isWorkflowIdValid/" + workflowId,
                            function (res) {
                                if (res === true) {
                                    if (document.getElementById("wfname").value === "") {
                                        Common.toastInfo("Enter a name for this workflow.");
                                    } else {
                                        let lt = document.getElementById("wflaunchtype").value;
                                        if (lt === "") {
                                            Common.toastInfo("Select a launchType for this workflow.");
                                        } else {
                                            if (lt === "periodic" && document.getElementById("wfperiod").value === "") {
                                                Common.toastInfo("Enter a period for this workflow.");
                                            } else {
                                                if (lt === "cron" && document.getElementById("wfcronexp").value === "") {
                                                    Common.toastInfo("Enter a cron expression for this workflow.");
                                                } else {

                                                    // Period validation
                                                    if (lt === "periodic" && document.getElementById("wfperiod").value !== "") {
                                                        let period = document.getElementById("wfperiod").value;
                                                        Common.get(uri + "/isPeriodValid/" + period,
                                                            function (res) {
                                                                if (res === true) {
                                                                    openJsonView(jsonVal);
                                                                } else {
                                                                    Common.toastInfo("The period format is not valid. The valid format is: dd.hh:mm:ss");
                                                                }
                                                            },
                                                            function () { }, auth
                                                        );
                                                    } // Cron expression validation
                                                    else if (lt === "cron" && document.getElementById("wfcronexp").value !== "") {
                                                        let expression = document.getElementById("wfcronexp").value;
                                                        let expressionEncoded = encodeURIComponent(expression);

                                                        Common.get(uri + "/isCronExpressionValid?e=" + expressionEncoded,
                                                            function (res) {
                                                                if (res === true) {
                                                                    openJsonView(jsonVal);
                                                                } else {
                                                                    if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                                        openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                                    }
                                                                }
                                                            },
                                                            function () { }, auth
                                                        );
                                                    } else {
                                                        openJsonView(jsonVal);
                                                    }

                                                }
                                            }
                                        }
                                    }
                                } else {
                                    Common.toastInfo("The workflow id is already in use. Enter another one.");
                                }
                            },
                            function () { }, auth
                        );
                    } else {

                        if (document.getElementById("wfname").value === "") {
                            Common.toastInfo("Enter a name for this workflow.");
                        } else {
                            let lt = document.getElementById("wflaunchtype").value;
                            if (lt === "") {
                                Common.toastInfo("Select a launchType for this workflow.");
                            } else {
                                if (lt === "periodic" && document.getElementById("wfperiod").value === "") {
                                    Common.toastInfo("Enter a period for this workflow.");
                                } else {
                                    if (lt === "cron" && document.getElementById("wfcronexp").value === "") {
                                        Common.toastInfo("Enter a cron expression for this workflow.");
                                    } else {

                                        // Period validation
                                        if (lt === "periodic" && document.getElementById("wfperiod").value !== "") {
                                            let period = document.getElementById("wfperiod").value;
                                            Common.get(uri + "/isPeriodValid/" + period,
                                                function (res) {
                                                    if (res === true) {
                                                        openJsonView(jsonVal);
                                                    } else {
                                                        Common.toastInfo("The period format is not valid. The valid format is: dd.hh:mm:ss");
                                                    }
                                                },
                                                function () { }, auth
                                            );
                                        } // Cron expression validation
                                        else if (lt === "cron" && document.getElementById("wfcronexp").value !== "") {
                                            let expression = document.getElementById("wfcronexp").value;
                                            let expressionEncoded = encodeURIComponent(expression);

                                            Common.get(uri + "/isCronExpressionValid?e=" + expressionEncoded,
                                                function (res) {
                                                    if (res === true) {
                                                        openJsonView(jsonVal);
                                                    } else {
                                                        if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                            openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                        }
                                                    }
                                                },
                                                function () { }, auth
                                            );
                                        } else {
                                            openJsonView(jsonVal);
                                        }

                                    }
                                }
                            }
                        }

                    }

                } else {
                    Common.toastInfo("Enter a valid workflow id.");
                }
            }
            else {
                openJsonView(jsonVal);
            }
        };

        // xml click
        function openXmlView(xmlVal) {

            diag = false;
            json = false;
            xml = true;

            leftcard.style.display = "none";
            propwrap.style.display = "none";
            wfclose.style.display = "none";
            wfpropwrap.style.display = "none";
            canvas.style.display = "none";
            code.style.display = "block";

            document.getElementById("rightswitch").style.backgroundColor = "#F0F0F0";
            document.getElementById("leftswitch").style.backgroundColor = "transparent";
            document.getElementById("middleswitch").style.backgroundColor = "transparent";

            editor = ace.edit("code");
            editor.setOptions({
                maxLines: Infinity,
                autoScrollEditorIntoView: true
            });

            editor.setReadOnly(false);
            editor.setFontSize("100%");
            editor.setPrintMarginColumn(false);
            editor.getSession().setMode("ace/mode/xml");

            editor.commands.addCommand({
                name: "showKeyboardShortcuts",
                bindKey: { win: "Ctrl-Alt-h", mac: "Command-Alt-h" },
                exec: function (editor) {
                    ace.config.loadModule("ace/ext/keybinding_menu", function (module) {
                        module.init(editor);
                        editor.showKeyboardShortcuts()
                    })
                }
            });

            editor.setValue(xmlVal, -1);
            editor.clearSelection();
            editor.resize(true);
            editor.focus();
        };

        document.getElementById("rightswitch").onclick = function () {

            if (diag === true) {
                let wfIdStr = document.getElementById("wfid").value;
                if (isInt(wfIdStr)) {
                    let workflowId = parseInt(wfIdStr);

                    if (checkId === true) {
                        Common.get(uri + "/isWorkflowIdValid/" + workflowId,
                            function (res) {
                                if (res === true) {
                                    if (document.getElementById("wfname").value === "") {
                                        Common.toastInfo("Enter a name for this workflow.");
                                    } else {
                                        let lt = document.getElementById("wflaunchtype").value;
                                        if (lt === "") {
                                            Common.toastInfo("Select a launchType for this workflow.");
                                        } else {
                                            if (lt === "periodic" && document.getElementById("wfperiod").value === "") {
                                                Common.toastInfo("Enter a period for this workflow.");
                                            } else {
                                                if (lt === "cron" && document.getElementById("wfcronexp").value === "") {
                                                    Common.toastInfo("Enter a cron expression for this workflow.");
                                                } else {

                                                    // Period validation
                                                    if (lt === "periodic" && document.getElementById("wfperiod").value !== "") {
                                                        let period = document.getElementById("wfperiod").value;
                                                        Common.get(uri + "/isPeriodValid/" + period,
                                                            function (res) {
                                                                if (res === true) {
                                                                    openXmlView(getXml());
                                                                } else {
                                                                    Common.toastInfo("The period format is not valid. The valid format is: dd.hh:mm:ss");
                                                                }
                                                            },
                                                            function () { }, auth
                                                        );
                                                    } // Cron expression validation
                                                    else if (lt === "cron" && document.getElementById("wfcronexp").value !== "") {
                                                        let expression = document.getElementById("wfcronexp").value;
                                                        let expressionEncoded = encodeURIComponent(expression);

                                                        Common.get(uri + "/isCronExpressionValid?e=" + expressionEncoded,
                                                            function (res) {
                                                                if (res === true) {
                                                                    openXmlView(getXml());
                                                                } else {
                                                                    if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                                        openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                                    }
                                                                }
                                                            },
                                                            function () { }, auth
                                                        );
                                                    } else {
                                                        openXmlView(getXml());
                                                    }

                                                }
                                            }
                                        }
                                    }
                                } else {
                                    Common.toastInfo("The workflow id is already in use. Enter another one.");
                                }
                            },
                            function () { }, auth
                        );
                    } else {

                        if (document.getElementById("wfname").value === "") {
                            Common.toastInfo("Enter a name for this workflow.");
                        } else {
                            let lt = document.getElementById("wflaunchtype").value;
                            if (lt === "") {
                                Common.toastInfo("Select a launchType for this workflow.");
                            } else {
                                if (lt === "periodic" && document.getElementById("wfperiod").value === "") {
                                    Common.toastInfo("Enter a period for this workflow.");
                                } else {
                                    if (lt === "cron" && document.getElementById("wfcronexp").value === "") {
                                        Common.toastInfo("Enter a cron expression for this workflow.");
                                    } else {

                                        // Period validation
                                        if (lt === "periodic" && document.getElementById("wfperiod").value !== "") {
                                            let period = document.getElementById("wfperiod").value;
                                            Common.get(uri + "/isPeriodValid/" + period,
                                                function (res) {
                                                    if (res === true) {
                                                        openXmlView(getXml());
                                                    } else {
                                                        Common.toastInfo("The period format is not valid. The valid format is: dd.hh:mm:ss");
                                                    }
                                                },
                                                function () { }, auth
                                            );
                                        } // Cron expression validation
                                        else if (lt === "cron" && document.getElementById("wfcronexp").value !== "") {
                                            let expression = document.getElementById("wfcronexp").value;
                                            let expressionEncoded = encodeURIComponent(expression);

                                            Common.get(uri + "/isCronExpressionValid?e=" + expressionEncoded,
                                                function (res) {
                                                    if (res === true) {
                                                        openXmlView(getXml());
                                                    } else {
                                                        if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                            openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                        }
                                                    }
                                                },
                                                function () { }, auth
                                            );
                                        } else {
                                            openXmlView(getXml());
                                        }

                                    }
                                }
                            }
                        }

                    }

                } else {
                    Common.toastInfo("Enter a valid workflow id.");
                }
            }
            else {
                openXmlView(getXml());
            }
        };

        function getXml() {
            let xml = '<Workflow xmlns="urn:wexflow-schema" id="' + workflow.WorkflowInfo.Id + '" name="' + workflow.WorkflowInfo.Name + '" description="' + workflow.WorkflowInfo.Description + '">\r\n';
            xml += '\t<Settings>\r\n\t\t<Setting name="launchType" value="' + launchType(workflow.WorkflowInfo.LaunchType) + '" />' + (workflow.WorkflowInfo.Period !== '' && workflow.WorkflowInfo.Period !== '00:00:00' ? ('\r\n\t\t<Setting name="period" value="' + workflow.WorkflowInfo.Period + '" />') : '') + (workflow.WorkflowInfo.CronExpression !== '' && workflow.WorkflowInfo.CronExpression !== null ? ('\r\n\t\t<Setting name="cronExpression" value="' + workflow.WorkflowInfo.CronExpression + '" />') : '') + '\r\n\t\t<Setting name="enabled" value="' + workflow.WorkflowInfo.IsEnabled + '" />\r\n\t\t<Setting name="approval" value="' + workflow.WorkflowInfo.IsApproval + '" />\r\n\t\t<Setting name="enableParallelJobs" value="' + workflow.WorkflowInfo.EnableParallelJobs + '" />\r\n\t</Settings>\r\n';
            xml += '\t<LocalVariables />\r\n';
            xml += '\t<Tasks>\r\n';
            for (let i = 0; i < workflow.Tasks.length; i++) {
                let task = workflow.Tasks[i];
                xml += '\t\t<Task id="' + task.Id + '" name="' + task.Name + '" description="' + task.Description + '" enabled="' + task.IsEnabled + '">\r\n';
                for (let j = 0; j < task.Settings.length; j++) {
                    let setting = task.Settings[j];
                    if (setting.Value !== "") {
                        xml += '\t\t\t<Setting name="' + setting.Name + '" value="' + setting.Value + '" />\r\n';
                    }
                }
                xml += '\t\t</Task>\r\n';
            }
            xml += '\t</Tasks>\r\n';
            xml += '</Workflow>';
            return xml;
        }


        // Browse workflows
        let modal = null;
        let exportModal = null;
        document.getElementById("browse").onclick = function () {

            Common.get(uri + "/search?s=",
                function (workflows) {
                    let browser = document.getElementById("browser");

                    workflows.sort(compareById);

                    let workflowsToTable = function (wfs) {
                        let items = [];
                        for (let i = 0; i < wfs.length; i++) {
                            let val = wfs[i];

                            items.push("<tr>" +
                                //"<td><input class='wf-delete' type='checkbox'></td>" +
                                "<td class='wf-id' title='" + val.Id + "'>" + val.Id + "</td>" +
                                "<td class='wf-n' title='" + val.Name + "'>" + val.Name + "</td>" +
                                "<td class='wf-n' title='" + val.Description + "'>" + val.Description + "</td>" +
                                "</tr>");

                        }

                        let table = "<table id='wf-workflows-table' class='table'>" +
                            "<thead class='thead-dark'>" +
                            "<tr>" +
                            //"<th><input id='wf-delete-all' type='checkbox'></th>" +
                            "<th class='wf-id'>Id</th>" +
                            "<th class='wf-n'>Name</th>" +
                            "<th class='wf-d'>Description</th>" +
                            "</tr>" +
                            "</thead>" +
                            "<tbody>" +
                            items.join("") +
                            "</tbody>" +
                            "</table>";

                        return table;
                    };
                    let search = '<div id="searchworkflows"><img src="assets/search.svg"><input id="searchworkflowsinput" type="text" placeholder="Search workflows"></div>';
                    browser.innerHTML = workflowsToTable(workflows);
                    let footer = '<div id="openworkflow">Open</div>';

                    if (exportModal) {
                        exportModal.destroy();
                    }

                    if (modal) {
                        modal.destroy();
                    }

                    modal = new jBox('Modal', {
                        width: 800,
                        height: 420,
                        title: search,
                        content: browser.innerHTML,
                        footer: footer,
                        delayOpen: 0
                    });

                    modal.open();

                    let searchworkflows = document.getElementById("searchworkflowsinput");
                    searchworkflows.onkeyup = function (event) {
                        event.preventDefault();
                        if (event.keyCode === 13) { // Enter
                            let jbox = document.getElementsByClassName("jBox-content")[0];

                            Common.get(uri + "/search?s=" + searchworkflows.value,
                                function (wfs) {
                                    wfs.sort(compareById);

                                    jbox.innerHTML = workflowsToTable(wfs);

                                    // selection changed event
                                    let workflowsTable = jbox.childNodes[0];
                                    let rows = (workflowsTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
                                    for (let i = 0; i < rows.length; i++) {
                                        let row = rows[i];

                                        row.onclick = function () {
                                            var selected = document.getElementsByClassName("selected");
                                            if (selected.length > 0) {
                                                selected[0].className = selected[0].className.replace("selected", "");
                                            }
                                            row.className += "selected";
                                        };
                                    }

                                }, function () {
                                    Common.toastError("An error occured while retrieving workflows. Check that wexflow server is running correctly.");
                                }, auth);
                        }
                    };

                    // selection changed event
                    let workflowsTable = document.getElementsByClassName("jBox-content")[0].childNodes[0];
                    let rows = (workflowsTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
                    for (let i = 0; i < rows.length; i++) {
                        let row = rows[i];

                        row.onclick = function () {
                            var selected = document.getElementsByClassName("selected");
                            if (selected.length > 0) {
                                selected[0].className = selected[0].className.replace("selected", "");
                            }
                            row.className += "selected";
                        };
                    }

                    // open click
                    document.getElementById("openworkflow").onclick = function () {

                        var selected = document.getElementsByClassName("selected");
                        if (selected.length === 0) {
                            Common.toastInfo("Choose a workflow to open.");
                        } else {
                            let id = selected[0].getElementsByClassName("wf-id")[0].innerHTML;

                            if (json === true) {
                                Common.get(uri + "/json/" + id,
                                    function (val) {
                                        openJsonView(JSON.stringify(val, null, '\t'));
                                    }, function () { }, auth);
                            } else if (xml === true) {
                                Common.get(uri + "/xml/" + id,
                                    function (val) {
                                        openXmlView(val);
                                    }, function () { }, auth);
                            }

                            // load diagram
                            Common.get(uri + "/json/" + id,
                                function (val) {
                                    workflow = val;

                                    // fill workflow settings
                                    document.getElementById("wfid").value = workflow.WorkflowInfo.Id;
                                    document.getElementById("wfname").value = workflow.WorkflowInfo.Name;
                                    document.getElementById("wfdesc").value = workflow.WorkflowInfo.Description;
                                    document.getElementById("wflaunchtype").value = launchType(workflow.WorkflowInfo.LaunchType);
                                    document.getElementById("wfperiod").onkeyup = workflow.WorkflowInfo.Period;
                                    document.getElementById("wfcronexp").value = workflow.WorkflowInfo.CronExpression;
                                    document.getElementById("wfenabled").checked = workflow.WorkflowInfo.IsEnabled;
                                    document.getElementById("wfapproval").checked = workflow.WorkflowInfo.IsApproval;
                                    document.getElementById("wfenablepj").checked = workflow.WorkflowInfo.EnableParallelJobs;

                                    tasks = {};
                                    for (let i = 0; i < workflow.Tasks.length; i++) {
                                        let task = workflow.Tasks[i];
                                        tasks[i] = task;
                                    }

                                    // load flowy
                                    let flowyinput = {};
                                    canvas.style.width = "100%";
                                    canvas.style.left = "0";

                                    document.getElementById("leftcard").style.left = -leftcardwidth + "px";
                                    closecardimg.src = "assets/openleft.png";
                                    leftcardHidden = true;

                                    document.getElementById("wfpropwrap").style.right = -wfpropwidth + "px";
                                    wfclose.style.right = "0";
                                    wfpropHidden = true;
                                    closewfcardimg.src = "assets/closeleft.png";

                                    closeTaskSettings();

                                    // build canvashtml
                                    let canvashtml = "";
                                    let blockspace = 180;
                                    let arrowspace = 180;
                                    for (let i = 0; i < workflow.Tasks.length; i++) {
                                        let task = workflow.Tasks[i];
                                        canvashtml += "<div class='blockelem noselect block' style='left: 500px; top: " + (25 + blockspace * i) + "px;'><input type='hidden' name='blockelemtype' class='blockelemtype' value='" + task.Name + "'><input type='hidden' name='blockelemdesc' class='blockelemdesc' value='" + task.Description + "'><input type='hidden' name='blockid' class='blockid' value='" + i + "'><div class='blockyleft'><img src='assets/actionorange.svg'><p class='blockyname'>" + task.Name + "</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>" + task.Description + "</div><div class='indicator invisible' style='left: 154px; top: 100px;'></div></div>";
                                        if (i < workflow.Tasks.length - 1) {
                                            canvashtml += "<div class='arrowblock' style='left: 639px; top: " + (125 + arrowspace * i) + "px;'><input type='hidden' class='arrowid' value='" + (i + 1) + "'><svg preserveAspectRatio='none' fill='none' xmlns='http://www.w3.org/2000/svg'><path d='M20 0L20 40L20 40L20 80' stroke='#C5CCD0' stroke-width='2px'></path><path d='M15 75H25L20 80L15 75Z' fill='#C5CCD0'></path></svg></div>";
                                        }
                                    }

                                    // build blockarr
                                    let blockarr = [];
                                    for (let i = 0; i < workflow.Tasks.length; i++) {
                                        blockarr.push(
                                            {
                                                "parent": i - 1,
                                                "childwidth": (i < workflow.Tasks.length - 1 ? 318 : 0),
                                                "id": i,
                                                "x": 644,
                                                "y": 190 + blockspace * i,
                                                "width": 318,
                                                "height": 100
                                            });
                                    }

                                    flowyinput = {
                                        "html": canvashtml,
                                        "blockarr": blockarr
                                    };

                                    flowy.import(flowyinput);

                                    // disable checkId
                                    checkId = false;

                                    // show delete button
                                    removeworkflow.style.display = "block";

                                    // close jBox
                                    modal.close();
                                    modal.destroy();


                                },
                                function () { }, auth);

                        }
                    };

                },
                function () {
                    Common.toastError("An error occured while retrieving workflows. Check that wexflow server is running correctly.");
                }, auth);
        };

        function compareById(wf1, wf2) {
            if (wf1.Id < wf2.Id) {
                return -1;
            } else if (wf1.Id > wf2.Id) {
                return 1;
            }
            return 0;
        }

        document.getElementById("export").onclick = function () {
            // TODO json or xml popup
            // TODO validation
            //download('text content', 'workflow-id.txt', 'text/plain');
            if (exportModal) {
                exportModal.destroy();
            }

            if (modal) {
                modal.destroy();
            }

            let footer = '<div id="exportworkflow">Export</div>';

            exportModal = new jBox('Modal', {
                width: 800,
                height: 120,
                title: "Export",
                content: document.getElementById("exportmodal").innerHTML,
                footer: footer,
                delayOpen: 0
            });

            exportModal.open();

            document.getElementById("exportworkflow").onclick = function () {
                let exportType = document.getElementsByClassName("jBox-content")[0].childNodes[3].value;

                // export whith validation
                if (exportType === "json") {
                    let downloadJson = function () {
                        download(JSON.stringify(workflow, null, '\t'), 'workflow-' + document.getElementById("wfid").value + '.json', 'application/json');
                        exportModal.close();
                    };

                    let wfIdStr = document.getElementById("wfid").value;
                    if (isInt(wfIdStr)) {
                        let workflowId = parseInt(wfIdStr);

                        if (checkId === true) {
                            Common.get(uri + "/isWorkflowIdValid/" + workflowId,
                                function (res) {
                                    if (res === true) {
                                        if (document.getElementById("wfname").value === "") {
                                            Common.toastInfo("Enter a name for this workflow.");
                                            exportModal.close();
                                        } else {
                                            let lt = document.getElementById("wflaunchtype").value;
                                            if (lt === "") {
                                                Common.toastInfo("Select a launchType for this workflow.");
                                                exportModal.close();
                                            } else {
                                                if (lt === "periodic" && document.getElementById("wfperiod").value === "") {
                                                    Common.toastInfo("Enter a period for this workflow.");
                                                    exportModal.close();
                                                } else {
                                                    if (lt === "cron" && document.getElementById("wfcronexp").value === "") {
                                                        Common.toastInfo("Enter a cron expression for this workflow.");
                                                        exportModal.close();
                                                    } else {

                                                        // Period validation
                                                        if (lt === "periodic" && document.getElementById("wfperiod").value !== "") {
                                                            let period = document.getElementById("wfperiod").value;
                                                            Common.get(uri + "/isPeriodValid/" + period,
                                                                function (res) {
                                                                    if (res === true) {
                                                                        downloadJson();
                                                                    } else {
                                                                        Common.toastInfo("The period format is not valid. The valid format is: dd.hh:mm:ss");
                                                                        exportModal.close();
                                                                    }
                                                                },
                                                                function () { }, auth
                                                            );
                                                        } // Cron expression validation
                                                        else if (lt === "cron" && document.getElementById("wfcronexp").value !== "") {
                                                            let expression = document.getElementById("wfcronexp").value;
                                                            let expressionEncoded = encodeURIComponent(expression);

                                                            Common.get(uri + "/isCronExpressionValid?e=" + expressionEncoded,
                                                                function (res) {
                                                                    if (res === true) {
                                                                        downloadJson();
                                                                    } else {
                                                                        exportModal.close();
                                                                        if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                                            openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                                        }
                                                                    }
                                                                },
                                                                function () { }, auth
                                                            );
                                                        } else {
                                                            downloadJson();
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    } else {
                                        Common.toastInfo("The workflow id is already in use. Enter another one.");
                                    }
                                },
                                function () { }, auth);
                        } else {

                            if (document.getElementById("wfname").value === "") {
                                Common.toastInfo("Enter a name for this workflow.");
                            } else {
                                let lt = document.getElementById("wflaunchtype").value;
                                if (lt === "") {
                                    Common.toastInfo("Select a launchType for this workflow.");
                                    exportModal.close();
                                } else {
                                    if (lt === "periodic" && document.getElementById("wfperiod").value === "") {
                                        Common.toastInfo("Enter a period for this workflow.");
                                        exportModal.close();
                                    } else {
                                        if (lt === "cron" && document.getElementById("wfcronexp").value === "") {
                                            Common.toastInfo("Enter a cron expression for this workflow.");
                                            exportModal.close();
                                        } else {

                                            // Period validation
                                            if (lt === "periodic" && document.getElementById("wfperiod").value !== "") {
                                                let period = document.getElementById("wfperiod").value;
                                                Common.get(uri + "/isPeriodValid/" + period,
                                                    function (res) {
                                                        if (res === true) {
                                                            downloadJson();
                                                        } else {
                                                            Common.toastInfo("The period format is not valid. The valid format is: dd.hh:mm:ss");
                                                            exportModal.close();
                                                        }
                                                    },
                                                    function () { }, auth
                                                );
                                            } // Cron expression validation
                                            else if (lt === "cron" && document.getElementById("wfcronexp").value !== "") {
                                                let expression = document.getElementById("wfcronexp").value;
                                                let expressionEncoded = encodeURIComponent(expression);

                                                Common.get(uri + "/isCronExpressionValid?e=" + expressionEncoded,
                                                    function (res) {
                                                        if (res === true) {
                                                            downloadJson();
                                                        } else {
                                                            exportModal.close();
                                                            if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                                openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                            }
                                                        }
                                                    },
                                                    function () { }, auth
                                                );
                                            } else {
                                                downloadJson();
                                            }

                                        }
                                    }
                                }
                            }

                        }

                    } else {
                        Common.toastInfo("Enter a valid workflow id.");
                        exportModal.close();
                    }

                } else if (exportType === "xml") {
                    let downloadXml = function () {
                        download(getXml(), 'workflow-' + document.getElementById("wfid").value + '.xml', 'text/xml')
                        exportModal.close();
                    };

                    let wfIdStr = document.getElementById("wfid").value;
                    if (isInt(wfIdStr)) {
                        let workflowId = parseInt(wfIdStr);

                        if (checkId === true) {
                            Common.get(uri + "/isWorkflowIdValid/" + workflowId,
                                function (res) {
                                    if (res === true) {
                                        if (document.getElementById("wfname").value === "") {
                                            Common.toastInfo("Enter a name for this workflow.");
                                            exportModal.close();
                                        } else {
                                            let lt = document.getElementById("wflaunchtype").value;
                                            if (lt === "") {
                                                Common.toastInfo("Select a launchType for this workflow.");
                                                exportModal.close();
                                            } else {
                                                if (lt === "periodic" && document.getElementById("wfperiod").value === "") {
                                                    Common.toastInfo("Enter a period for this workflow.");
                                                    exportModal.close();
                                                } else {
                                                    if (lt === "cron" && document.getElementById("wfcronexp").value === "") {
                                                        Common.toastInfo("Enter a cron expression for this workflow.");
                                                        exportModal.close();
                                                    } else {

                                                        // Period validation
                                                        if (lt === "periodic" && document.getElementById("wfperiod").value !== "") {
                                                            let period = document.getElementById("wfperiod").value;
                                                            Common.get(uri + "/isPeriodValid/" + period,
                                                                function (res) {
                                                                    if (res === true) {
                                                                        downloadXml();
                                                                    } else {
                                                                        Common.toastInfo("The period format is not valid. The valid format is: dd.hh:mm:ss");
                                                                        exportModal.close();
                                                                    }
                                                                },
                                                                function () { }, auth
                                                            );
                                                        } // Cron expression validation
                                                        else if (lt === "cron" && document.getElementById("wfcronexp").value !== "") {
                                                            let expression = document.getElementById("wfcronexp").value;
                                                            let expressionEncoded = encodeURIComponent(expression);

                                                            Common.get(uri + "/isCronExpressionValid?e=" + expressionEncoded,
                                                                function (res) {
                                                                    if (res === true) {
                                                                        downloadXml();
                                                                    } else {
                                                                        exportModal.close();
                                                                        if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                                            openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                                        }
                                                                    }
                                                                },
                                                                function () { }, auth
                                                            );
                                                        } else {
                                                            downloadXml();
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    } else {
                                        Common.toastInfo("The workflow id is already in use. Enter another one.");
                                    }
                                },
                                function () { }, auth);
                        } else {

                            if (document.getElementById("wfname").value === "") {
                                Common.toastInfo("Enter a name for this workflow.");
                                exportModal.close();
                            } else {
                                let lt = document.getElementById("wflaunchtype").value;
                                if (lt === "") {
                                    Common.toastInfo("Select a launchType for this workflow.");
                                    exportModal.close();
                                } else {
                                    if (lt === "periodic" && document.getElementById("wfperiod").value === "") {
                                        Common.toastInfo("Enter a period for this workflow.");
                                        exportModal.close();
                                    } else {
                                        if (lt === "cron" && document.getElementById("wfcronexp").value === "") {
                                            Common.toastInfo("Enter a cron expression for this workflow.");
                                            exportModal.close();
                                        } else {

                                            // Period validation
                                            if (lt === "periodic" && document.getElementById("wfperiod").value !== "") {
                                                let period = document.getElementById("wfperiod").value;
                                                Common.get(uri + "/isPeriodValid/" + period,
                                                    function (res) {
                                                        if (res === true) {
                                                            downloadXml();
                                                        } else {
                                                            Common.toastInfo("The period format is not valid. The valid format is: dd.hh:mm:ss");
                                                            exportModal.close();
                                                        }
                                                    },
                                                    function () { }, auth
                                                );
                                            } // Cron expression validation
                                            else if (lt === "cron" && document.getElementById("wfcronexp").value !== "") {
                                                let expression = document.getElementById("wfcronexp").value;
                                                let expressionEncoded = encodeURIComponent(expression);

                                                Common.get(uri + "/isCronExpressionValid?e=" + expressionEncoded,
                                                    function (res) {
                                                        if (res === true) {
                                                            downloadXml();
                                                        } else {
                                                            exportModal.close();
                                                            if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                                openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                            }
                                                        }
                                                    },
                                                    function () { }, auth
                                                );
                                            } else {
                                                downloadXml();
                                            }

                                        }
                                    }
                                }
                            }

                        }

                    } else {
                        Common.toastInfo("Enter a valid workflow id.");
                        exportModal.close();
                    }
                }
            };
        };

        function download(text, name, type) {
            var a = document.createElement("a");
            var file = new Blob([text], { type: type });
            var url = URL.createObjectURL(file);
            a.href = url;
            a.download = name;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
        }

    }

};