function Users() {
    "use strict";

    var uri = Common.trimEnd(Settings.Uri, "/");
    var lnkManager = document.getElementById("lnk-manager");
    var lnkDesigner = document.getElementById("lnk-designer");
    var lnkUsers = document.getElementById("lnk-users");
    var divUsers = document.getElementById("users");
    var divUsersTable = document.getElementById("users-table");
    var divUserActions = document.getElementById("user-actions");
    var divUserProfile = document.getElementById("user-profile");
    var slctProfile = document.getElementById("slct-profile");
    var oldPasswordText = document.getElementById("old-password-text");
    var newPasswordText = document.getElementById("new-password-text");
    var confirmPasswordText = document.getElementById("confirm-password-text");
    var deleteAction = document.getElementById("delete-action");
    var saveAction = document.getElementById("save-action");
    var newUserAction = document.getElementById("new-user-action");
    var btnLogout = document.getElementById("btn-logout");
    var oldPasswordTr = document.getElementById("old-password-tr");
    var lblNewPassword = document.getElementById("lbl-new-password");
    var txtUsername = document.getElementById("username-text");
    var selectedUsername;
    var logedinUser;
    var newUser = false;

    var suser = getUser();

    if (suser === null || suser === "") {
        Common.redirectToLoginPage();
    } else {
        var user = JSON.parse(suser);
        Common.get(uri + "/user?username=" + encodeURIComponent(user.Username),
            function (u) {
                if (user.Password !== u.Password) {
                    Common.redirectToLoginPage();
                } else if (u.UserProfile === 0) {
                    logedinUser = u.Username;

                    divUsers.style.display = "block";
                    lnkManager.style.display = "inline";
                    lnkDesigner.style.display = "inline";
                    lnkUsers.style.display = "inline";

                    btnLogout.innerHTML = "Logout (" + u.Username + ")";

                    btnLogout.onclick = function() {
                        deleteUser();
                        Common.redirectToLoginPage();
                    };

                    Common.get(uri + "/users",
                        function(data) {

                            var items = [];
                            for (var i = 0; i < data.length; i++) {
                                var val = data[i];
                                items.push(
                                    "<tr>" + "<td class='username'>" + val.Username + "</td>" + "</tr>"
                                );
                            }

                            var table = "<table id='wf-users-table' class='table table-hover'>"
                                + "<thead class='thead-dark'>"
                                + "<tr>"
                                + "<th>Username</th>"
                                + "</tr>"
                                + "</thead>"
                                + "<tbody>"
                                + items.join("")
                                + "</tbody>"
                                + "</table>";

                            divUsersTable.innerHTML = table;

                            var usersTable = document.getElementById("wf-users-table");

                            var rows = (usersTable.getElementsByTagName("tbody")[0]).getElementsByTagName("tr");
                            for (var j = 0; j < rows.length; j++) {
                                rows[j].onclick = function() {
                                    var selected = document.getElementsByClassName("selected");
                                    if (selected.length > 0) {
                                        selected[0].className = selected[0].className.replace("selected", "");
                                    }

                                    this.className += "selected";

                                    selectedUsername = this.getElementsByClassName("username")[0].innerHTML;

                                    loadRightPanel();

                                    newUser = false;
                                };
                            }

                        });

                } else {
                    Common.redirectToLoginPage();
                }

            });
    }

    function loadRightPanel() {
        divUserActions.style.display = "block";
        divUserProfile.style.display = "block";

        if (selectedUsername === logedinUser) {
            deleteAction.style.display = "none";
            slctProfile.value = 0;
            slctProfile.disabled = true;
        } else {
            oldPasswordTr.style.display = "none";
        }

        Common.get(uri + "/user?username=" + encodeURIComponent(selectedUsername),
            function (u) {
                txtUsername.value = u.Username;
                slctProfile.value = u.UserProfile;

                if (u.UserProfile === 1) {
                    slctProfile.disabled = false;
                    deleteAction.style.display = "block";
                }

                if (u.Username === logedinUser || (u.Username !== logedinUser && u.UserProfile === 0)) {
                    oldPasswordTr.style.display = "";
                    slctProfile.disabled = true;
                    deleteAction.style.display = "none";
                }
                
            });

    }


    newUserAction.onclick = function () {
        divUserActions.style.display = "block";
        divUserProfile.style.display = "block";
        deleteAction.style.display = "none";

        oldPasswordTr.style.display = "none";
        lblNewPassword.innerHTML = "Password";
        txtUsername.value = "";
        slctProfile.disabled = false;
        slctProfile.value = -1;
        newPasswordText.value = "";
        oldPasswordText.value = "";

        newUser = true;
    };

    deleteAction.onclick = function() {
        if (selectedUsername !== logedinUser) {
            Common.post(uri + "/deleteUser?username=" + encodeURIComponent(selectedUsername),
                function (val) {
                    if (val === true) {
                        alert("The user " + selectedUsername + " was deleted with success.");
                    } else {
                        alert("An error occured while deleting the user " + selectedUsername + ".");
                    }
                });
        }
    };

    saveAction.onclick = function () {

        if (newUser === true) {

            var username = txtUsername.value;
            var up = parseInt(getSelectedProfile());
            var password = newPasswordText.value;
            var confirmedPassword = confirmPasswordText.value;

            if (username === "") {
                alert("Type a username.");
            } else {
                Common.get(uri + "/user?username=" + encodeURIComponent(username),
                    function (u) {
                        console.log(u);

                        if (typeof u === "undefined") {
                            if (up === -1) {
                                alert("Choose a user profile for this user.");
                            } else {
                                if (password === "" || confirmedPassword === "") {
                                    alert("Type a password.");
                                } else {
                                    if (password !== confirmedPassword) {
                                        alert("Passwords don't match.");
                                    } else {
                                        var hashedPass = MD5(password);
                                        Common.post(
                                            uri + "/insertUser?username=" + encodeURIComponent(username) + "&password=" + hashedPass + "&up=" + up,
                                            function (val) {
                                                if (val === true) {
                                                    alert("The user " + username + " was created with success.");
                                                } else {
                                                    alert("An error occured while creating the user " + username + ".");
                                                }
                                            });
                                    }
                                }
                                
                            }
                        } else {
                            alert("A user with this name already exists. Type another username.");
                        }

                    });
            }

        } else {

            Common.get(uri + "/user?username=" + encodeURIComponent(selectedUsername),
                function (u) {
                    var oldPassword = MD5(oldPasswordText.value);
                    if (u.UserProfile === 0 &&  u.Password !== oldPassword) {
                        alert("The old password is not valid.");
                    } else {
                        if (newPasswordText.value !== confirmPasswordText.value) {
                            alert("New passwords don't match.");
                        } else if (newPasswordText.value === "" || confirmPasswordText.value === "") {
                            alert("Enter a new password.");
                        } else {
                            var newPassword = MD5(newPasswordText.value);
                            var up = getSelectedProfile();
                            Common.post(uri +
                                "/updateUser?username=" + encodeURIComponent(selectedUsername) + "&password=" + encodeURIComponent(newPassword) + "&up=" + up,
                                function (val) {
                                    if (val === true) {
                                        alert("The password was changed with success.");
                                    } else {
                                        alert("An error occured while changing the password.");
                                    }
                                });
                        }
                    }

                });
        }
    };


    function getSelectedProfile() {
        return slctProfile.options[slctProfile.selectedIndex].value;
    }

    // TODO use id in hidden field to change username?
    // TODO search
    // TODO order by
}