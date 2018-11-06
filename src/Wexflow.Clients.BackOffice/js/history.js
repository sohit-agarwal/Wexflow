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
    var page = 1;
    var numberOfPages = 0;

    var suser = getUser();

    if (suser === null || suser === "") {
        Common.redirectToLoginPage();
    } else {
        var user = JSON.parse(suser);
        Common.get(uri + "/user?username=" + encodeURIComponent(user.Username), function (u) {
            if (user.Password !== u.Password) {
                Common.redirectToLoginPage();
            } else {
                divEntries.style.display = "block";
                divEntriesAction.style.display = "block";

                btnLogout.onclick = function () {
                    deleteUser();
                    Common.redirectToLoginPage();
                };

                btnLogout.innerHTML = "Logout (" + u.Username + ")";

                Common.get(uri + "/historyEntriesCount?s=" + encodeURIComponent(txtSearch.value), function (count) {

                    updatePagerControls(count);

                    btnNextPage.onclick = function () {
                        page++;
                        if (page > 1) {
                            disableButton(btnPreviousPage, false);
                        }

                        if (page >= numberOfPages) {
                            disableButton(btnNextPage, true);
                        } else {
                            disableButton(btnNextPage, false);
                        }

                        lblPages.innerHTML = page + " / " + numberOfPages;
                        loadEntries();
                    };

                    disableButton(btnPreviousPage, true);

                    btnPreviousPage.onclick = function () {
                        page--;
                        if (page === 1) {
                            disableButton(btnPreviousPage, true);
                        }

                        if (page < numberOfPages) {
                            disableButton(btnNextPage, false);
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

                    function updatePager() {

                        Common.get(uri + "/historyEntriesCount?s=" + encodeURIComponent(txtSearch.value), function (count) {
                            updatePagerControls(count);
                        });
                    }

                    function updatePagerControls(count) {
                        lblEntriesCount.innerHTML = "Total entries: " + count;

                        numberOfPages = count / getEntriesCount();
                        var numberOfPagesInt = parseInt(numberOfPages);
                        if (numberOfPages > numberOfPagesInt) {
                            numberOfPages = numberOfPagesInt + 1;
                        } else {
                            numberOfPages = numberOfPagesInt;
                        }

                        lblPages.innerHTML = page + " / " + numberOfPages;

                        if (page >= numberOfPages) {
                            disableButton(btnNextPage, true);
                        } else {
                            disableButton(btnNextPage, false);
                        }

                        if (page === 1) {
                            disableButton(btnPreviousPage, true);
                        }
                    }

                });

                loadEntries();
            }
        });
    }

    function loadEntries() {
        var entriesCount = getEntriesCount();

        Common.get(uri + "/searchHistoryEntriesByPage?s=" + encodeURIComponent(txtSearch.value) + "&page=" + page + "&entriesCount=" + entriesCount, function (data) {
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
                    + "<td class='date'>" + formatDate(new Date(val.StatusDate)) + "</td>"
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
                + "<th class='date'>Date</th>"
                + "</tr>"
                + "</thead>"
                + "<tbody>"
                + items.join("")
                + "</tbody>"
                + "</table>";

            document.getElementById("entries").innerHTML = table;
        }, function () {
            //alert("An error occured while retrieving entries. Check Wexflow Web Service Uri and check that Wexflow Windows Service is running correctly.");
        });
    }

    function formatDate(d) {
        return ("0" + d.getDate()).slice(-2) + "-" + ("0" + (d.getMonth() + 1)).slice(-2) + "-" +
            d.getFullYear() + " " + ("0" + d.getHours()).slice(-2) + ":" + ("0" + d.getMinutes()).slice(-2) + ":" + ("0" + d.getSeconds()).slice(-2);

    }

    function getEntriesCount() {
        if (slctEntriesCount.selectedIndex === -1) {
            return 10;
        }

        return slctEntriesCount.options[slctEntriesCount.selectedIndex].value;
    }

    function disableButton(button, disabled) {
        button.disabled = disabled;
    }
}