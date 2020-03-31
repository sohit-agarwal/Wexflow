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

            document.getElementById("leftcard").style.left = "0";
            leftcardHidden = false;
            canvas.style.left = leftcardwidth + "px";
            canvas.style.width = "calc(100% - " + leftcardwidth + "px)";
            closecardimg.src = "assets/closeleft.png";

            document.getElementById("wfpropwrap").style.right = "0";
            wfclose.style.right = "330px";
            wfpropHidden = false;
            closewfcardimg.src = "assets/openleft.png";

            if (rightcard) {
                rightcard = false;
                document.getElementById("properties").classList.remove("expanded");
                setTimeout(function () {
                    document.getElementById("propwrap").classList.remove("itson");
                }, 300);
                tempblock.classList.remove("selectedblock");
            }
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
            /*if (drag.querySelector(".blockelemtype").value == "1") {
                drag.innerHTML += "<div class='blockyleft'><img src='assets/eyeblue.svg'><p class='blockyname'>New visitor</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>When a <span>new visitor</span> goes to <span>Site 1</span></div>";
            } else if (drag.querySelector(".blockelemtype").value == "2") {
                drag.innerHTML += "<div class='blockyleft'><img src='assets/actionblue.svg'><p class='blockyname'>Action is performed</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>When <span>Action 1</span> is performed</div>";
            } else if (drag.querySelector(".blockelemtype").value == "3") {
                drag.innerHTML += "<div class='blockyleft'><img src='assets/timeblue.svg'><p class='blockyname'>Time has passed</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>When <span>10 seconds</span> have passed</div>";
            } else if (drag.querySelector(".blockelemtype").value == "4") {
                drag.innerHTML += "<div class='blockyleft'><img src='assets/errorblue.svg'><p class='blockyname'>Error prompt</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>When <span>Error 1</span> is triggered</div>";
            } else if (drag.querySelector(".blockelemtype").value == "5") {
                drag.innerHTML += "<div class='blockyleft'><img src='assets/databaseorange.svg'><p class='blockyname'>New database entry</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>Add <span>Data object</span> to <span>Database 1</span></div>";
            } else if (drag.querySelector(".blockelemtype").value == "6") {
                drag.innerHTML += "<div class='blockyleft'><img src='assets/databaseorange.svg'><p class='blockyname'>Update database</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>Update <span>Database 1</span></div>";
            } else if (drag.querySelector(".blockelemtype").value == "7") {
                drag.innerHTML += "<div class='blockyleft'><img src='assets/actionorange.svg'><p class='blockyname'>Perform an action</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>Perform <span>Action 1</span></div>";
            } else if (drag.querySelector(".blockelemtype").value == "8") {
                drag.innerHTML += "<div class='blockyleft'><img src='assets/twitterorange.svg'><p class='blockyname'>Make a tweet</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>Tweet <span>Query 1</span> with the account <span>@alyssaxuu</span></div>";
            } else if (drag.querySelector(".blockelemtype").value == "9") {
                drag.innerHTML += "<div class='blockyleft'><img src='assets/logred.svg'><p class='blockyname'>Add new log entry</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>Add new <span>success</span> log entry</div>";
            } else if (drag.querySelector(".blockelemtype").value == "10") {
                drag.innerHTML += "<div class='blockyleft'><img src='assets/logred.svg'><p class='blockyname'>Update logs</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>Edit <span>Log Entry 1</span></div>";
            } else if (drag.querySelector(".blockelemtype").value == "11") {
                drag.innerHTML += "<div class='blockyleft'><img src='assets/errorred.svg'><p class='blockyname'>Prompt an error</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>Trigger <span>Error 1</span></div>";
            }*/
            let taskname = drag.querySelector(".blockelemtype").value;
            let taskdesc = drag.querySelector(".blockelemdesc").value;
            drag.innerHTML += "<div class='blockyleft'><img src='assets/actionorange.svg'><p class='blockyname'>" + taskname + "</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>" + taskdesc + "</div>";

            if (!tasks[taskname]) {
                tasks[taskname] = {
                    "Id": 0,
                    "Name": taskname,
                    "Description": "",
                    "IsEnabled": true,
                    "Settings": []
                };
            }

            return true;
        }
        function drag(block) {
            block.classList.add("blockdisabled");
            tempblock2 = block;
        }
        function release() {
            tempblock2.classList.remove("blockdisabled");
        }
        /*let disabledClick = function(){
            document.querySelector(".navactive").classList.add("navdisabled");
            document.querySelector(".navactive").classList.remove("navactive");
            this.classList.add("navactive");
            this.classList.remove("navdisabled");
            if (this.getAttribute("id") == "triggers") {
                document.getElementById("blocklist").innerHTML = '<div class="blockelem create-flowy noselect"><input type="hidden" name="blockelemtype" class="blockelemtype" value="1"><div class="grabme"><img src="assets/grabme.svg"></div><div class="blockin">                  <div class="blockico"><span></span><img src="assets/eye.svg"></div><div class="blocktext">                        <p class="blocktitle">New visitor</p><p class="blockdesc">Triggers when somebody visits a specified page</p>        </div></div></div><div class="blockelem create-flowy noselect"><input type="hidden" name="blockelemtype" class="blockelemtype" value="2"><div class="grabme"><img src="assets/grabme.svg"></div><div class="blockin">                    <div class="blockico"><span></span><img src="assets/action.svg"></div><div class="blocktext">                        <p class="blocktitle">Action is performed</p><p class="blockdesc">Triggers when somebody performs a specified action</p></div></div></div><div class="blockelem create-flowy noselect"><input type="hidden" name="blockelemtype" class="blockelemtype" value="3"><div class="grabme"><img src="assets/grabme.svg"></div><div class="blockin">                    <div class="blockico"><span></span><img src="assets/time.svg"></div><div class="blocktext">                        <p class="blocktitle">Time has passed</p><p class="blockdesc">Triggers after a specified amount of time</p>          </div></div></div><div class="blockelem create-flowy noselect"><input type="hidden" name="blockelemtype" class="blockelemtype" value="4"><div class="grabme"><img src="assets/grabme.svg"></div><div class="blockin">                    <div class="blockico"><span></span><img src="assets/error.svg"></div><div class="blocktext">                        <p class="blocktitle">Error prompt</p><p class="blockdesc">Triggers when a specified error happens</p>              </div></div></div>';
            } else if (this.getAttribute("id") == "actions") {
                document.getElementById("blocklist").innerHTML = '<div class="blockelem create-flowy noselect"><input type="hidden" name="blockelemtype" class="blockelemtype" value="5"><div class="grabme"><img src="assets/grabme.svg"></div><div class="blockin">                  <div class="blockico"><span></span><img src="assets/database.svg"></div><div class="blocktext">                        <p class="blocktitle">New database entry</p><p class="blockdesc">Adds a new entry to a specified database</p>        </div></div></div><div class="blockelem create-flowy noselect"><input type="hidden" name="blockelemtype" class="blockelemtype" value="6"><div class="grabme"><img src="assets/grabme.svg"></div><div class="blockin">                  <div class="blockico"><span></span><img src="assets/database.svg"></div><div class="blocktext">                        <p class="blocktitle">Update database</p><p class="blockdesc">Edits and deletes database entries and properties</p>        </div></div></div><div class="blockelem create-flowy noselect"><input type="hidden" name="blockelemtype" class="blockelemtype" value="7"><div class="grabme"><img src="assets/grabme.svg"></div><div class="blockin">                  <div class="blockico"><span></span><img src="assets/action.svg"></div><div class="blocktext">                        <p class="blocktitle">Perform an action</p><p class="blockdesc">Performs or edits a specified action</p>        </div></div></div><div class="blockelem create-flowy noselect"><input type="hidden" name="blockelemtype" class="blockelemtype" value="8"><div class="grabme"><img src="assets/grabme.svg"></div><div class="blockin">                  <div class="blockico"><span></span><img src="assets/twitter.svg"></div><div class="blocktext">                        <p class="blocktitle">Make a tweet</p><p class="blockdesc">Makes a tweet with a specified query</p>        </div></div></div>';
            } else if (this.getAttribute("id") == "loggers") {
                document.getElementById("blocklist").innerHTML = '<div class="blockelem create-flowy noselect"><input type="hidden" name="blockelemtype" class="blockelemtype" value="9"><div class="grabme"><img src="assets/grabme.svg"></div><div class="blockin">                  <div class="blockico"><span></span><img src="assets/log.svg"></div><div class="blocktext">                        <p class="blocktitle">Add new log entry</p><p class="blockdesc">Adds a new log entry to this project</p>        </div></div></div><div class="blockelem create-flowy noselect"><input type="hidden" name="blockelemtype" class="blockelemtype" value="10"><div class="grabme"><img src="assets/grabme.svg"></div><div class="blockin">                  <div class="blockico"><span></span><img src="assets/log.svg"></div><div class="blocktext">                        <p class="blocktitle">Update logs</p><p class="blockdesc">Edits and deletes log entries in this project</p>        </div></div></div><div class="blockelem create-flowy noselect"><input type="hidden" name="blockelemtype" class="blockelemtype" value="11"><div class="grabme"><img src="assets/grabme.svg"></div><div class="blockin">                  <div class="blockico"><span></span><img src="assets/error.svg"></div><div class="blocktext">                        <p class="blocktitle">Prompt an error</p><p class="blockdesc">Triggers a specified error</p>        </div></div></div>';
            }
        }
        addEventListenerMulti("click", disabledClick, false, ".side");*/
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
            flowy.deleteBlocks();

            //document.getElementById("wfpropwrap").style.right = -wfpropwidth + "px";
            //wfclose.style.right = "0";
            //wfpropHidden = true;
            //closewfcardimg.src = "assets/closeleft.png";

            closeTaskSettings();
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

            if (event.type === "mouseup" && aclick) {
                //if (!rightcard && event.target.closest(".block")) {
                if (event.target.closest(".block")) {
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

                    if (!tasks[taskname]) {
                        tasks[taskname] = {
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
                                tasksettings += '<p class="wf-setting-name">' + settingName + '</p><input class="wf-setting-index" type="hidden" value="' + i + '"><input class="wf-setting-value inputtext" value="' + (tasks[taskname].Settings[i] ? tasks[taskname].Settings[i].Value : "") + '" type="text" />';

                                if (!tasks[taskname].Settings[i]) {
                                    tasks[taskname].Settings.push({
                                        "Name": settingName,
                                        "Value": "",
                                        "Attributes": []
                                    });
                                }
                            }

                            proplist.innerHTML = proplist.innerHTML + tasksettings;

                            document.getElementById("taskid").value = tasks[taskname].Id;
                            document.getElementById("taskdescription").value = tasks[taskname].Description;
                            document.getElementById("taskenabled").checked = tasks[taskname].IsEnabled;

                            document.getElementById("taskid").onkeyup = function () {
                                tasks[taskname].Id = this.value;

                                updateTasks();
                            };

                            document.getElementById("taskdescription").onkeyup = function () {
                                tasks[taskname].Description = this.value;

                                updateTasks();
                            };

                            document.getElementById("taskenabled").onchange = function () {
                                tasks[taskname].IsEnabled = this.checked;

                                updateTasks();
                            };

                            let settingValues = document.getElementsByClassName("wf-setting-value");
                            for (let i = 0; i < settingValues.length; i++) {
                                let settingValue = settingValues[i];

                                settingValue.onkeyup = function () {
                                    let index = this.previousElementSibling.value;
                                    tasks[taskname].Settings[index].Value = this.value;

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
                leftcardHidden = true;
                canvas.style.left = "0";
                canvas.style.width = "100%";
                closecardimg.src = "assets/openleft.png";

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
                leftcardHidden = false;
                canvas.style.left = leftcardwidth + "px";
                canvas.style.width = "calc(100% - " + leftcardwidth + "px)";
                closecardimg.src = "assets/closeleft.png";

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
                wfclose.style.right = "330px";
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
            let flowyoutput = flowy.output();
            //console.log(flowyoutput);
            let wftasks = [];
            if (flowyoutput && flowyoutput.blocks) {
                for (let i = 0; i < flowyoutput.blocks.length; i++) {
                    wftasks.push(tasks[flowyoutput.blocks[i].data[0].value]);
                }
            }
            workflow.Tasks = wftasks;
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
        document.getElementById("middleswitch").onclick = function () {

            // TODO diagram validation when switching from diagram view

            let openJsonView = function () {
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

                editor.setValue(JSON.stringify(workflow, null, '\t'), -1);
                editor.clearSelection();
                editor.resize(true);
                editor.focus();
            };

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
                                                                    openJsonView();
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
                                                                    openJsonView();
                                                                } else {
                                                                    if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                                        openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                                    }
                                                                }
                                                            },
                                                            function () { }, auth
                                                        );
                                                    } else {
                                                        openJsonView();
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
                                                        openJsonView();
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
                                                        openJsonView();
                                                    } else {
                                                        if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                            openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                        }
                                                    }
                                                },
                                                function () { }, auth
                                            );
                                        } else {
                                            openJsonView();
                                        }

                                    }
                                }
                            }
                        }

                    }

                } else {
                    Common.toastInfo("Enter a valid workflow id.");
                }
            } else {
                openJsonView();
            }
        };

        // xml click
        document.getElementById("rightswitch").onclick = function () {

            // TODO diagram validation when switching from diagram view

            let openXmlView = function () {

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

                editor.setValue(getXml(), -1);
                editor.clearSelection();
                editor.resize(true);
                editor.focus();
            };

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
                                                                    openXmlView();
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
                                                                    openXmlView();
                                                                } else {
                                                                    if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                                        openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                                    }
                                                                }
                                                            },
                                                            function () { }, auth
                                                        );
                                                    } else {
                                                        openXmlView();
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
                                                        openXmlView();
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
                                                        openXmlView();
                                                    } else {
                                                        if (confirm("The cron expression format is not valid.\nRead the documentation?")) {
                                                            openInNewTab("https://github.com/aelassas/Wexflow/wiki/Cron-scheduling");
                                                        }
                                                    }
                                                },
                                                function () { }, auth
                                            );
                                        } else {
                                            openXmlView();
                                        }

                                    }
                                }
                            }
                        }

                    }

                } else {
                    Common.toastInfo("Enter a valid workflow id.");
                }
            } else {
                openXmlView();
            }
        };

        function getXml() {
            let xml = '<Workflow xmlns="urn:wexflow-schema" id="' + workflow.WorkflowInfo.Id + '" name="' + workflow.WorkflowInfo.Name + '" description="' + workflow.WorkflowInfo.Description + '">\r\n';
            xml += '\t<Settings>\r\n\t\t<Setting name="launchType" value="' + launchType(workflow.WorkflowInfo.LaunchType) + '" />' + (workflow.WorkflowInfo.Period !== '' ? ('\r\n\t\t<Setting name="period" value="' + workflow.WorkflowInfo.Period + '" />') : '') + (workflow.WorkflowInfo.CronExpression !== '' ? ('\r\n\t\t<Setting name="cronExpression" value="' + workflow.WorkflowInfo.CronExpression + '" />') : '') + '\r\n\t\t<Setting name="enabled" value="' + workflow.WorkflowInfo.IsEnabled + '" />\r\n\t\t<Setting name="approval" value="' + workflow.WorkflowInfo.IsApproval + '" />\r\n\t\t<Setting name="enableParallelJobs" value="' + workflow.WorkflowInfo.EnableParallelJobs + '" />\r\n\t</Settings>\r\n';
            xml += '\t<LocalVariables />\r\n';
            xml += '\t<Tasks>\r\n';
            for (let i = 0; i < workflow.Tasks.length; i++) {
                let task = workflow.Tasks[i];
                xml += '\t\t<Task id="' + task.Id + '" name="' + task.Name + '" description="' + task.Description + '" enabled="' + task.IsEnabled + '">\r\n';
                for (let j = 0; j < task.Settings.length; j++) {
                    let setting = task.Settings[j];
                    xml += '\t\t\t<Setting name="' + setting.Name + '" value="' + setting.Value + '" />\r\n';
                }
                xml += '\t\t</Task>\r\n';
            }
            xml += '\t</Tasks>\r\n';
            xml += '</Workflow>';
            return xml;
        }


        // Browse workflows
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

                    new jBox('Modal', {
                        width: 800,
                        height: 420,
                        title: search,
                        content: browser.innerHTML,
                        footer: footer,
                        delayOpen: 0
                    }).open();

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
                            // TODO
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

    }

};