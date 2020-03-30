window.onload = function () {
    "use strict";

    let uri = Common.trimEnd(Settings.Uri, "/");
    let lnkManager = document.getElementById("lnk-manager");
    let lnkDesigner = document.getElementById("lnk-designer");
    let lnkEditor = document.getElementById("lnk-editor");
    let lnkApproval = document.getElementById("lnk-approval");
    let lnkUsers = document.getElementById("lnk-users");
    let lnkProfiles = document.getElementById("lnk-profiles");
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

                        let navigation = document.getElementById("navigation");
                        let leftcard = document.getElementById("leftcard");
                        let propwrap = document.getElementById("propwrap");

                        navigation.style.display = "block";
                        leftcard.style.display = "block";
                        propwrap.style.display = "block";
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
        let canvas = document.getElementById("canvas");
        let leftcardHidden = true;
        let leftcardwidth = 361;
        let closecardimg = document.getElementById("closecardimg");
        let wfpropHidden = true;
        let wfpropwidth = 331;
        let closewfcardimg = document.getElementById("wfcloseimg");
        let wfclose = document.getElementById("wfclose")
        let rightcard = false;
        let tempblock;
        let tempblock2;

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
            let taskName = drag.querySelector(".blockelemtype").value;
            let taskDesc = drag.querySelector(".blockelemdesc").value;
            drag.innerHTML += "<div class='blockyleft'><img src='assets/actionorange.svg'><p class='blockyname'>" + taskName + "</p></div><div class='blockyright'><img src='assets/more.svg'></div><div class='blockydiv'></div><div class='blockyinfo'>" + taskDesc + "</div>";
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
        document.getElementById("close").addEventListener("click", function () {
            if (rightcard) {
                rightcard = false;
                document.getElementById("properties").classList.remove("expanded");
                setTimeout(function () {
                    document.getElementById("propwrap").classList.remove("itson");
                }, 300);
                tempblock.classList.remove("selectedblock");
            }
        });

        document.getElementById("removeblock").addEventListener("click", function () {
            flowy.deleteBlocks();
        });
        let aclick = false;
        let beginTouch = function (event) {
            aclick = true;
        }
        let checkTouch = function (event) {
            aclick = false;
        }
        let doneTouch = function (event) {
            if (event.type === "mouseup" && aclick) {
                if (!rightcard && event.target.closest(".block")) {
                    tempblock = event.target.closest(".block");
                    rightcard = true;
                    document.getElementById("properties").classList.add("expanded");
                    document.getElementById("propwrap").classList.add("itson");
                    tempblock.classList.add("selectedblock");
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

    }

};