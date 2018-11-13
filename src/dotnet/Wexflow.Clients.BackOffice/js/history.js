function History() {
    "use strict";

    var uri = Common.trimEnd(Settings.Uri, "/");
    var divEntries = document.getElementById("entries");
    var divEntriesAction = document.getElementById("entries-action");
    var btnLogout = document.getElementById("btn-logout");
    var btnSearch = document.getElementById("btn-search");
    var txtSearch = document.getElementById("txt-search");
    var slctEntriesCount = document.getElementById("slct-entries-count");
    var btnNextPage = document.getElementById("btn-next-page");
    var btnPreviousPage = document.getElementById("btn-previous-page");
    var lblPages = document.getElementById("lbl-pages");
    var lblEntriesCount = document.getElementById("lbl-entries-count");
    var txtFrom = document.getElementById("txt-from");
    var txtTo = document.getElementById("txt-to");
    var lnkManager = document.getElementById("lnk-manager");
    var lnkDesigner = document.getElementById("lnk-designer");
    var lnkUsers = document.getElementById("lnk-users");

    var page = 1;
    var numberOfPages = 0;
    var heo = 1;
    var suser = getUser();
    var from = null;
    var to = null;

    if (suser === null || suser === "") {
        Common.redirectToLoginPage();
    } else {
        var user = JSON.parse(suser);
        Common.get(uri + "/user?username=" + encodeURIComponent(user.Username), function (u) {
            if (user.Password !== u.Password) {
                Common.redirectToLoginPage();
            } else {

                if (u.UserProfile === 0) {
                    lnkManager.style.display = "inline";
                    lnkDesigner.style.display = "inline";
                    lnkUsers.style.display = "inline";
                }

                divEntries.style.display = "block";
                divEntriesAction.style.display = "block";

                btnLogout.onclick = function () {
                    deleteUser();
                    Common.redirectToLoginPage();
                };

                btnLogout.innerHTML = "Logout (" + u.Username + ")";
                Common.get(uri + "/historyEntryStatusDateMin",
                    function(dateMin) {
                        Common.get(uri + "/historyEntryStatusDateMax",
                            function (dateMax) {

                                from = new Date(dateMin);
                                to = new Date(dateMax);

                                if (from.getDay() === to.getDay() &&
                                    from.getMonth() === to.getMonth() &&
                                    from.getYear() === to.getYear()) {
                                    to.setDate(to.getDate() + 1);
                                }

                                Common.get(uri + "/historyEntriesCountByDate?s=" + encodeURIComponent(txtSearch.value) + "&from=" + from.getTime() + "&to=" + to.getTime(), function (count) {

                                    $(txtFrom).datepicker({
                                        changeMonth: true,
                                        changeYear: true,
                                        dateFormat: "dd-mm-yy",
                                        onSelect: function() {
                                            from = $(this).datepicker("getDate");
                                        }
                                    });

                                    $(txtFrom).datepicker("setDate", from);

                                    $(txtTo).datepicker({
                                        changeMonth: true,
                                        changeYear: true,
                                        dateFormat: "dd-mm-yy",
                                        onSelect: function () {
                                            to = $(this).datepicker("getDate");
                                        }
                                    });

                                    $(txtTo).datepicker("setDate", to);

                                    updatePagerControls(count);

                                    btnNextPage.onclick = function () {
                                        page++;
                                        if (page > 1) {
                                            Common.disableButton(btnPreviousPage, false);
                                        }

                                        if (page >= numberOfPages) {
                                            Common.disableButton(btnNextPage, true);
                                        } else {
                                            Common.disableButton(btnNextPage, false);
                                        }

                                        lblPages.innerHTML = page + " / " + numberOfPages;
                                        loadEntries();
                                    };

                                    Common.disableButton(btnPreviousPage, true);

                                    btnPreviousPage.onclick = function () {
                                        page--;
                                        if (page === 1) {
                                            Common.disableButton(btnPreviousPage, true);
                                        }

                                        if (page < numberOfPages) {
                                            Common.disableButton(btnNextPage, false);
                                        }

                                        lblPages.innerHTML = page + " / " + numberOfPages;
                                        loadEntries();
                                    };

                                    btnSearch.onclick = function () {
                                        page = 1;
                                        updatePager();
                                        loadEntries();
                                    };

                                    txtSearch.onkeyup = function (e) {
                                        e.preventDefault();

                                        if (e.keyCode === 13) {
                                            page = 1;
                                            updatePager();
                                            loadEntries();
                                        }
                                    };

                                    slctEntriesCount.onchange = function () {
                                        page = 1;
                                        updatePagerControls(count);
                                        loadEntries();
                                    };

                                    loadEntries();

                            });

                    });

                
            });

            }
        });
    }

    function updatePager() {

        Common.get(uri + "/historyEntriesCountByDate?s=" + encodeURIComponent(txtSearch.value) + "&from=" + from.getTime() + "&to=" + to.getTime(), function (count) {
            updatePagerControls(count);
        });
    }

    function updatePagerControls(count) {
        lblEntriesCount.innerHTML = "Total entries: " + count;

        numberOfPages = count / getEntriesCount();
        var numberOfPagesInt = parseInt(numberOfPages);
        if (numberOfPages > numberOfPagesInt) {
            numberOfPages = numberOfPagesInt + 1;
        } else if (numberOfPagesInt === 0) {
            numberOfPages = 1;
        } else {
            numberOfPages = numberOfPagesInt;
        }

        lblPages.innerHTML = page + " / " + numberOfPages;

        if (page >= numberOfPages) {
            Common.disableButton(btnNextPage, true);
        } else {
            Common.disableButton(btnNextPage, false);
        }

        if (page === 1) {
            Common.disableButton(btnPreviousPage, true);
        }
    }

    function getEntriesCount() {
        if (slctEntriesCount.selectedIndex === -1) {
            return 10;
        }

        return slctEntriesCount.options[slctEntriesCount.selectedIndex].value;
    }

    function loadEntries() {
        var entriesCount = getEntriesCount();

        Common.get(uri + "/searchHistoryEntriesByPageOrderBy?s=" + encodeURIComponent(txtSearch.value) +"&from="+ from.getTime() + "&to=" + to.getTime() + "&page=" + page + "&entriesCount=" + entriesCount + "&heo=" + heo, function (data) {
            var items = [];
            var i;
            for (i = 0; i < data.length; i++) {
                var val = data[i];
                var lt = Common.launchType(val.LaunchType);
                var entryStatus = Common.status(val.Status);
                items.push("<tr>"
                    + "<td class='status'>" + entryStatus + "</td>"
                    + "<td class='date'>" + Common.formatDate(new Date(val.StatusDate)) + "</td>"
                    + "<td class='id' title='" + val.WorkflowId + "'>" + val.WorkflowId + "</td>"
                    + "<td class='name'>" + val.Name + "</td>"
                    + "<td class='lt'>" + lt + "</td>"
                    + "<td class='desc' title='" + val.Description + "'>" + val.Description + "</td>"
                    + "</tr>");
            }

            var table = "<table id='entries-table' class='table table-hover'>"
                + "<thead class='thead-dark'>"
                + "<tr>"
                + "<th id='th-status' class='status'>Status</th>"
                + "<th id='th-date' class='date'>Date 🔻</th>"
                + "<th id='th-id' class='id'>Id</th>"
                + "<th id='th-name' class='name'>Name</th>"
                + "<th id='th-lt' class='lt'>LaunchType</th>"
                + "<th id='th-desc' class='desc'>Description</th>"
                + "</tr>"
                + "</thead>"
                + "<tbody>"
                + items.join("")
                + "</tbody>"
                + "</table>";

            document.getElementById("entries").innerHTML = table;

            var entriesTable = document.getElementById("entries-table");
            var rows = (entriesTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
            for (i = 0; i < rows.length; i++) {
                rows[i].onclick = function () {
                    var selected = document.getElementsByClassName("selected");
                    if (selected.length > 0) {
                        selected[0].className = selected[0].className.replace("selected", "");
                    }
                    this.className += "selected";
                };
            }

            var thDate = document.getElementById("th-date");
            var thId = document.getElementById("th-id");
            var thName = document.getElementById("th-name");
            var thLt = document.getElementById("th-lt");
            var thDesc = document.getElementById("th-desc");
            var thStatus = document.getElementById("th-status");

            if (heo === 0) { // By Date ascending
                thDate.innerHTML = "Date&nbsp;&nbsp;🔺";
                thId.innerHTML = "Id";
                thName.innerHTML = "Name";
                thLt.innerHTML = "LaunchType";
                thDesc.innerHTML = "Description";
                thStatus.innerHTML = "Status";
            } else if (heo === 1) { // By Date descending
                thDate.innerHTML = "Date&nbsp;&nbsp;🔻";
                thId.innerHTML = "Id";
                thName.innerHTML = "Name";
                thLt.innerHTML = "LaunchType";
                thDesc.innerHTML = "Description";
                thStatus.innerHTML = "Status";
            } else if (heo === 2) { // By Id ascending
                thId.innerHTML = "Id&nbsp;&nbsp;🔺";
                thDate.innerHTML = "Date";
                thName.innerHTML = "Name";
                thLt.innerHTML = "LaunchType";
                thDesc.innerHTML = "Description";
            } else if (heo === 3) { // By Id descending
                thId.innerHTML = "Id&nbsp;&nbsp;🔻";
                thDate.innerHTML = "Date";
                thName.innerHTML = "Name";
                thLt.innerHTML = "LaunchType";
                thDesc.innerHTML = "Description";
                thStatus.innerHTML = "Status";
            } else if (heo === 4) { // By Name ascending
                thName.innerHTML = "Name&nbsp;&nbsp;🔺";
                thDate.innerHTML = "Date";
                thId.innerHTML = "Id";
                thLt.innerHTML = "LaunchType";
                thDesc.innerHTML = "Description";
                thStatus.innerHTML = "Status";
            } else if (heo === 5) { // By Name descending
                thName.innerHTML = "Name&nbsp;&nbsp;🔻";
                thDate.innerHTML = "Date";
                thId.innerHTML = "Id";
                thLt.innerHTML = "LaunchType";
                thDesc.innerHTML = "Description";
                thStatus.innerHTML = "Status";
            } else if (heo === 6) { // By LaunchType ascending
                thLt.innerHTML = "LaunchType&nbsp;&nbsp;🔺";
                thDate.innerHTML = "Date";
                thId.innerHTML = "Id";
                thName.innerHTML = "Name";
                thDesc.innerHTML = "Description";
                thStatus.innerHTML = "Status";
            } else if (heo === 7) { // By LaunchType descending
                thLt.innerHTML = "LaunchType&nbsp;&nbsp;🔻";
                thDate.innerHTML = "Date";
                thId.innerHTML = "Id";
                thName.innerHTML = "Name";
                thDesc.innerHTML = "Description";
                thStatus.innerHTML = "Status";
            } else if (heo === 8) { // By Description ascending
                thDesc.innerHTML = "Description&nbsp;&nbsp;🔺";
                thDate.innerHTML = "Date";
                thId.innerHTML = "Id";
                thName.innerHTML = "Name";
                thLt.innerHTML = "LaunchType";
                thStatus.innerHTML = "Status";
            } else if (heo === 9) { // By Description descending
                thDesc.innerHTML = "Description&nbsp;&nbsp;🔻";
                thDate.innerHTML = "Date";
                thId.innerHTML = "Id";
                thName.innerHTML = "Name";
                thLt.innerHTML = "LaunchType";
                thStatus.innerHTML = "Status";
            } else if (heo === 10) { // By Status ascending
                thStatus.innerHTML = "Status&nbsp;&nbsp;🔺";
                thDate.innerHTML = "Date";
                thId.innerHTML = "Id";
                thName.innerHTML = "Name";
                thLt.innerHTML = "LaunchType";
                thDesc.innerHTML = "Description";
            } else if (heo === 11) { // By Status descending
                thStatus.innerHTML = "Status&nbsp;&nbsp;🔻";
                thDate.innerHTML = "Date";
                thId.innerHTML = "Id";
                thName.innerHTML = "Name";
                thLt.innerHTML = "LaunchType";
                thDesc.innerHTML = "Description";
            } 

            thDate.onclick = function () {
                if (heo === 1) {
                    heo = 0;
                    loadEntries();
                } else {
                    heo = 1;
                    loadEntries();
                }
            };

            thId.onclick = function () {
                if (heo === 2) {
                    heo = 3;
                    loadEntries();
                } else {
                    heo = 2;
                    loadEntries();
                }
            };

            thName.onclick = function () {
                if (heo === 4) {
                    heo = 5;
                    loadEntries();
                } else {
                    heo = 4;
                    loadEntries();
                }
            };

            thLt.onclick = function () {
                if (heo === 6) {
                    heo = 7;
                    loadEntries();
                } else {
                    heo = 6;
                    loadEntries();
                }
            };

            thDesc.onclick = function () {
                if (heo === 8) {
                    heo = 9;
                    loadEntries();
                } else {
                    heo = 8;
                    loadEntries();
                }
            };

            thStatus.onclick = function () {
                if (heo === 10) {
                    heo = 11;
                    loadEntries();
                } else {
                    heo = 10;
                    loadEntries();
                }
            };

        }, function () {
            //alert("An error occured while retrieving entries. Check Wexflow Web Service Uri and check that Wexflow Windows Service is running correctly.");
        });
    }

}