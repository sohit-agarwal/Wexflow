function Users() {
    "use strict";

    var uri = Common.trimEnd(Settings.Uri, "/");
    var lnkManager = document.getElementById("lnk-manager");
    var lnkDesigner = document.getElementById("lnk-designer");
    var lnkApproval = document.getElementById("lnk-approval");
    var lnkUsers = document.getElementById("lnk-users");
    var lnkProfiles = document.getElementById("lnk-profiles");
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
    var newPasswordTr = document.getElementById("new-password-tr");
    var confirmPasswordTr = document.getElementById("confirm-password-tr");
    var lblNewPassword = document.getElementById("lbl-new-password");
    var txtUsername = document.getElementById("username-text");
    var changePass = document.getElementById("change-password");
    var emailText = document.getElementById("email-text");
    var txtId = document.getElementById("txt-id");
    var txtCreatedOn = document.getElementById("txt-createdOn");
    var txtModifiedOn = document.getElementById("txt-modifiedOn");
    var trId = document.getElementById("tr-id");
    var trCreatedOn = document.getElementById("tr-createdOn");
    var trModifiedOn = document.getElementById("tr-modifiedOn");
    var txtSearch = document.getElementById("users-search-text");
    var btnSearch = document.getElementById("users-search-action");
    var selectedUsername;
    var selectedUsernameTd;
    var selectedUserId;
    var selectedUserProfile;
    var selectedUserProfileTd;
    var logedinUser;
    var logedinUserProfile;
    var newUser = false;
    var changePassword = false;
    var selectedTr;
    var uo = 0;
    var thUsername;
    var qusername = "";
    var qpassword = "";
    var auth = "";

    var suser = getUser();

    if (suser === null || suser === "") {
        Common.redirectToLoginPage();
    } else {
        var user = JSON.parse(suser);

        qusername = user.Username;
        qpassword = user.Password;
        auth = "Basic " + btoa(qusername + ":" + qpassword);

        Common.get(uri + "/user?username=" + encodeURIComponent(user.Username),
            function (u) {
                if (user.Password !== u.Password) {
                    Common.redirectToLoginPage();
                } else if (u.UserProfile === 0 || u.UserProfile === 1) {
                    logedinUser = u.Username;
                    logedinUserProfile = u.UserProfile;

                    divUsers.style.display = "block";
                    lnkManager.style.display = "inline";
                    lnkDesigner.style.display = "inline";
                    lnkApproval.style.display = "inline";
                    lnkUsers.style.display = "inline";

                    if (u.UserProfile === 0) {
                        lnkProfiles.style.display = "inline";
                    }

                    btnLogout.innerHTML = "Logout (" + u.Username + ")";

                    btnLogout.onclick = function () {
                        deleteUser();
                        Common.redirectToLoginPage();
                    };

                    if (u.UserProfile === 1) {
                        newUserAction.style.display = "none";
                    }

                    loadUsers();

                } else {
                    Common.redirectToLoginPage();
                }

            },
            function () { }, auth);
    }

    btnSearch.onclick = function () {
        loadUsers();
    };

    txtSearch.onkeyup = function (e) {
        e.preventDefault();
        if (e.keyCode === 13) {
            loadUsers();
        }
    };

    function loadUsers(usernameToSelect, scroll) {
        Common.get(uri + "/searchUsers?keyword=" + encodeURIComponent(txtSearch.value) + "&uo=" + uo,
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
                        + "<td class='userprofile'>" + userProfileToText(val.UserProfile) + "</td>"
                        + "</tr>"
                    );
                }

                var table = "<table id='wf-users-table' class='table'>"
                    + "<thead class='thead-dark'>"
                    + "<tr>"
                    + "<th id='th-id'>Id</th>"
                    + "<th id='th-username'>Username&nbsp;&nbsp;🔺</th>"
                    + "<th>Profile</th>"
                    + "</tr>"
                    + "</thead>"
                    + "<tbody>"
                    + items.join("")
                    + "</tbody>"
                    + "</table>";

                divUsersTable.innerHTML = table;

                thUsername = document.getElementById("th-username");
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

                // set selectedUsernameTd
                var selected = document.getElementsByClassName("selected");
                if (selected.length > 0) {
                    selectedTr = selected[0];
                    selectedUsernameTd = selectedTr.getElementsByClassName("username")[0];
                    selectedUserProfileTd = selectedTr.getElementsByClassName("userprofile")[0];
                }

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
                            selectedTr = selected[0];
                            selectedTr.className = selectedTr.className.replace("selected", "");
                        }

                        this.className += "selected";
                        selectedTr = this;

                        selectedUsernameTd = this.getElementsByClassName("username")[0];
                        selectedUserProfileTd = this.getElementsByClassName("userprofile")[0];
                        selectedUsername = selectedUsernameTd.innerHTML;
                        selectedUserId = this.getElementsByClassName("userid")[0].innerHTML;
                        selectedUserProfile = userProfileToInt(this.getElementsByClassName("userprofile")[0].innerHTML);

                        loadRightPanel();

                        oldPasswordTr.style.display = "none";
                        newPasswordTr.style.display = "none";
                        confirmPasswordTr.style.display = "none";
                        changePass.style.display = "block";

                        //if (selectedUsername !== logedinUser && selectedUserProfile === 0) {
                        if ((selectedUsername !== logedinUser && selectedUserProfile === 0) || (logedinUserProfile === 1 && selectedUsername !== logedinUser && selectedUserProfile === 1)) {
                            //newPasswordTr.style.display = "table-row";
                            //lblNewPassword.innerHTML = "Password";
                            changePass.style.display = "none";
                        } else {
                            lblNewPassword.innerHTML = "New password";
                        }

                        newUser = false;
                        changePassword = false;
                    };
                }

            },
            function () { }, auth);
    }

    function userProfileToText(userProfile) {
        switch (userProfile) {
            case 0:
                return "SuperAdministrator";
            case 1:
                return "Administrator";
            case 2:
                return "Restricted";
            default:
                return "Unknown";
        }
    }

    function userProfileToInt(userProfile) {
        switch (userProfile) {
            case "SuperAdministrator":
                return 0;
            case "Administrator":
                return 1;
            case "Restricted":
                return 2;
            default:
                return -1;
        }
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
                txtId.value = u.Id;
                //txtCreatedOn.value = Common.formatDate(new Date(u.CreatedOn));
                txtCreatedOn.value = u.CreatedOn;

                //if (u.ModifiedOn === -62135596800000) {
                if (u.ModifiedOn.indexOf("0001") > -1) {
                    txtModifiedOn.value = "-";
                } else {
                    //txtModifiedOn.value = Common.formatDate(new Date(u.ModifiedOn));
                    txtModifiedOn.value = u.ModifiedOn;
                }

                txtUsername.value = u.Username;
                slctProfile.value = u.UserProfile;
                emailText.value = u.Email;
                oldPasswordText.value = "";
                newPasswordText.value = "";
                confirmPasswordText.value = "";
                trId.style.display = "table-row";
                trCreatedOn.style.display = "table-row";
                trModifiedOn.style.display = "table-row";

                if (u.UserProfile === 2) {
                    slctProfile.disabled = false;
                    deleteAction.style.display = "block";
                    oldPasswordTr.style.display = "none";
                }

                if (u.Username === logedinUser || (u.Username !== logedinUser && u.UserProfile === 0)) {
                    oldPasswordTr.style.display = "";
                    slctProfile.disabled = true;
                    deleteAction.style.display = "none";
                }

                if (u.Username !== logedinUser && u.UserProfile === 0) {
                    txtUsername.disabled = true;
                    emailText.disabled = true;
                    saveAction.style.display = "none";
                } if (u.Username === logedinUser && u.UserProfile === 0) {
                    saveAction.style.display = "block";
                } else if (logedinUserProfile === 0 && u.Username !== logedinUser && (u.UserProfile === 1 || u.UserProfile === 2)) {
                    txtUsername.disabled = false;
                    emailText.disabled = false;
                    slctProfile.disabled = false;
                    saveAction.style.display = "block";
                    deleteAction.style.display = "block";
                } else if (u.Username === logedinUser && u.UserProfile === 1) {
                    txtUsername.disabled = false;
                    emailText.disabled = false;
                    saveAction.style.display = "block";
                    deleteAction.style.display = "none";

                } else if (u.UserProfile === 2) {
                    txtUsername.disabled = false;
                    emailText.disabled = false;
                    slctProfile.disabled = true;
                    saveAction.style.display = "block";
                    deleteAction.style.display = "block";
                } else {
                    txtUsername.disabled = true;
                    emailText.disabled = true;
                    slctProfile.disabled = true;
                    saveAction.style.display = "none";
                    deleteAction.style.display = "none";
                }

            },
            function () { }, auth);
    }

    newUserAction.onclick = function () {
        divUserActions.style.display = "block";
        divUserProfile.style.display = "block";
        deleteAction.style.display = "none";

        txtId.value = "";
        txtCreatedOn.value = "";
        txtModifiedOn.value = "";
        trId.style.display = "none";
        trCreatedOn.style.display = "none";
        trModifiedOn.style.display = "none";

        txtUsername.disabled = false;
        oldPasswordTr.style.display = "none";
        lblNewPassword.innerHTML = "Password";
        txtUsername.value = "";
        slctProfile.disabled = false;
        slctProfile.value = -1;
        newPasswordText.value = "";
        oldPasswordText.value = "";
        confirmPasswordText.value = "";
        emailText.disabled = false;
        emailText.value = "";

        newPasswordTr.style.display = "table-row";
        confirmPasswordTr.style.display = "table-row";
        changePass.style.display = "none";

        saveAction.style.display = "block";

        if (typeof selectedTr !== "undefined") {
            selectedTr.className = selectedTr.className.replace("selected", "");
        }

        newUser = true;
    };

    changePass.onclick = function () {
        if (selectedUserProfile === 0) {
            oldPasswordTr.style.display = "table-row";
        }
        newPasswordTr.style.display = "table-row";
        confirmPasswordTr.style.display = "table-row";

        changePass.style.display = "none";
        changePassword = true;
    };

    deleteAction.onclick = function () {
        var r = confirm("Are you sure you want to delete this user?");

        if (r === true) {
            if (selectedUsername !== logedinUser) {

                Common.get(uri + "/user?username=" + encodeURIComponent(selectedUsername),
                    function (u) {
                        Common.post(uri + "/deleteUser?username=" + encodeURIComponent(selectedUsername) + "&password=" + encodeURIComponent(u.Password),
                            function (val) {
                                if (val === true) {
                                    Common.toastSuccess("The user " + selectedUsername + " was deleted with success.");
                                    loadUsers();
                                    divUserActions.style.display = "none";
                                    divUserProfile.style.display = "none";
                                } else {
                                    Common.toastError("An error occured while deleting the user " + selectedUsername + ".");
                                }
                            },
                            function () { }, "", auth);
                    },
                    function () { }, auth);
            }
        }

    };

    //confirmPasswordText.onkeyup = function(e) {
    //    e.preventDefault();

    //    if (e.keyCode === 13) {
    //        save();
    //    }

    //};

    saveAction.onclick = function () {
        save();
    };


    function save() {
        if (newUser === true) {

            var username = txtUsername.value;
            var up = parseInt(getSelectedProfile());
            var password = newPasswordText.value;
            var confirmedPassword = confirmPasswordText.value;

            if (username === "") {
                Common.toastInfo("Type a username.");
            } else {
                Common.get(uri + "/user?username=" + encodeURIComponent(username),
                    function (u) {
                        if (typeof u === "undefined") {
                            if (up === -1) {
                                Common.toastInfo("Choose a user profile for this user.");
                            } else {
                                if (password === "" || confirmedPassword === "") {
                                    Common.toastInfo("Type a password.");
                                } else {
                                    if (password !== confirmedPassword) {
                                        Common.toastInfo("Passwords don't match.");
                                    } else if (emailText.value === "" || validateEmail(emailText.value) === false) {
                                        Common.toastInfo("Enter a valid email address.");
                                    } else {
                                        var hashedPass = MD5(password);
                                        Common.post(
                                            uri + "/insertUser?username=" + encodeURIComponent(username) + "&password=" + hashedPass + "&up=" + up + "&email=" + encodeURIComponent(emailText.value),
                                            function (val) {
                                                if (val === true) {
                                                    Common.toastSuccess("The user " + username + " was created with success.");

                                                    Common.get(uri + "/user?username=" + encodeURIComponent(username),
                                                        function (user) {

                                                            selectedUserId = user.Id;
                                                            selectedUsername = user.Username;
                                                            newUser = false;

                                                            loadUsers(username, true);

                                                            if (user.UserProfile === 1 || user.UserProfile == 2) {
                                                                deleteAction.style.display = "block";

                                                            } else if (user.UserProfile === 0) {
                                                                slctProfile.disabled = true;
                                                                saveAction.style.display = "none";
                                                                txtUsername.disabled = true;
                                                                emailText.disabled = true;
                                                                changePass.style.display = "none";
                                                                newPasswordTr.style.display = "none";
                                                                confirmPasswordTr.style.display = "none";
                                                            }

                                                            trId.style.display = "table-row";
                                                            trCreatedOn.style.display = "table-row";
                                                            trModifiedOn.style.display = "table-row";
                                                            txtId.value = user.Id;
                                                            //txtCreatedOn.value = Common.formatDate(new Date(user.CreatedOn));
                                                            txtCreatedOn.value = user.CreatedOn;
                                                            txtModifiedOn.value = "-";

                                                        },
                                                        function () { }, auth);

                                                } else {
                                                    Common.toastError("An error occured while creating the user " + username + ".");
                                                }
                                            },
                                            function () { }, "", auth);
                                    }
                                }

                            }
                        } else {
                            Common.toastInfo("A user with this name already exists. Type another username.");
                        }

                    },
                    function () { }, auth);
            }

        } else {
            var up2 = parseInt(getSelectedProfile());
            if (changePassword === false) {
                if (txtUsername.value === "") {
                    Common.toastInfo("Enter a username.");
                } else if (emailText.value === "" || validateEmail(emailText.value) === false) {
                    Common.toastInfo("Enter a valid email address.");
                } else if (up2 === -1) {
                    Common.toastInfo("Choose a user profile for this user.");
                } else {
                    Common.get(uri + "/user?username=" + encodeURIComponent(txtUsername.value),
                        function (u) {
                            if (typeof u !== "undefined" && u !== null && u.Username !== selectedUsername) {
                                Common.toastInfo("The user " + txtUsername.value + " already exists. Choose another username.");
                            } else {

                                if (selectedUsername !== logedinUser && selectedUserProfile === 0) {

                                    if (newPasswordText.value === "") {
                                        Common.toastInfo("Type the password of this user.");
                                    } else {
                                        var pass = MD5(newPasswordText.value);

                                        if (pass !== u.Password) {
                                            Common.toastInfo("The password is incorrect.");
                                        } else {
                                            updateUsernameAndPassword();
                                        }
                                    }

                                } else {
                                    updateUsernameAndPassword();
                                }

                            }

                        },
                        function () { }, auth);
                }


            } else {
                Common.get(uri + "/user?username=" + encodeURIComponent(selectedUsername),
                    function (u) {
                        var oldPassword = MD5(oldPasswordText.value);
                        if (u.UserProfile === 0 && u.Password !== oldPassword) {
                            Common.toastInfo("The old password is not valid.");
                        } else {

                            if (newPasswordText.value !== confirmPasswordText.value) {
                                Common.toastInfo("New passwords don't match.");
                            } else if (newPasswordText.value === "" || confirmPasswordText.value === "") {
                                Common.toastInfo("Enter a new password.");
                            } else {
                                var newPassword = MD5(newPasswordText.value);
                                var up = getSelectedProfile();

                                if (txtUsername.value === "") {
                                    Common.toastInfo("Enter a username.");
                                } else if (emailText.value === "" || validateEmail(emailText.value) === false) {
                                    Common.toastInfo("Enter a valid email address.");
                                } else if (up === -1) {
                                    Common.toastInfo("Choose a user profile for this user.");
                                } else {
                                    Common.get(uri + "/user?username=" + encodeURIComponent(txtUsername.value),
                                        function (u) {
                                            if (typeof u !== "undefined" &&
                                                u !== null &&
                                                u.Username !== selectedUsername) {
                                                Common.toastInfo("The user " + txtUsername.value + " already exists. Choose another username.");
                                            } else {

                                                Common.post(uri + "/updateUser?userId=" + selectedUserId + "&username=" + encodeURIComponent(txtUsername.value) + "&password=" + encodeURIComponent(newPassword) + "&up=" + up + "&email=" + encodeURIComponent(emailText.value),
                                                    function (val) {
                                                        if (val === true) {
                                                            auth = "Basic " + btoa(qusername + ":" + (selectedUsername === logedinUser ? newPassword : qpassword));
                                                            Common.get(uri + "/user?username=" + encodeURIComponent(txtUsername.value),
                                                                function (user) {
                                                                    if (selectedUsername === logedinUser) {
                                                                        qpassword = user.Password;
                                                                        auth = "Basic " + btoa(qusername + ":" + qpassword);
                                                                        deleteUser();
                                                                        authorize(txtUsername.value, user.Password, user.UserProfile);

                                                                        btnLogout.innerHTML = "Logout (" + txtUsername.value + ")";
                                                                    }

                                                                    selectedUsernameTd.innerHTML = txtUsername.value;

                                                                    if (logedinUser === selectedUsername) {
                                                                        logedinUser = txtUsername.value;
                                                                    }

                                                                    selectedUsername = txtUsername.value;

                                                                    trId.style.display = "table-row";
                                                                    trCreatedOn.style.display = "table-row";
                                                                    trModifiedOn.style.display = "table-row";
                                                                    txtId.value = user.Id;
                                                                    //txtCreatedOn.value = Common.formatDate(new Date(user.CreatedOn));
                                                                    txtCreatedOn.value = user.CreatedOn;
                                                                    //txtModifiedOn.value = Common.formatDate(new Date(user.ModifiedOn));
                                                                    txtModifiedOn.value = user.ModifiedOn;

                                                                    if (logedinUser !== selectedUsername && user.UserProfile === 0) {
                                                                        slctProfile.disabled = true;
                                                                        saveAction.style.display = "none";
                                                                        txtUsername.disabled = true;
                                                                        emailText.disabled = true;
                                                                        changePass.style.display = "none";
                                                                    }

                                                                    Common.toastSuccess("The user " + txtUsername.value + " was updated with success.");
                                                                },
                                                                function () { }, auth);
                                                        } else {
                                                            Common.toastError("An error occured while updating the user " + txtUsername.value + ".");
                                                        }
                                                    },
                                                    function () { }, "", auth);
                                            }
                                        },
                                        function () { }, auth
                                    );
                                }

                            }
                        }

                    },
                    function () { }, auth);
            }

        }
    }

    function updateUsernameAndPassword() {
        var up = parseInt(getSelectedProfile());

        Common.post(uri + "/updateUsernameAndEmailAndUserProfile?userId=" + selectedUserId + "&username=" + encodeURIComponent(txtUsername.value) + "&email=" + encodeURIComponent(emailText.value) + "&up=" + up,
            function (val) {
                if (val === true) {
                    Common.get(uri + "/user?username=" + encodeURIComponent(txtUsername.value),
                        function (user) {
                            if (logedinUser === selectedUsername) {
                                qpassword = user.Password;
                                btnLogout.innerHTML = "Logout (" + txtUsername.value + ")";

                                deleteUser();
                                authorize(txtUsername.value, user.Password, user.UserProfile);

                            }
                            Common.toastSuccess("The user " + txtUsername.value + " was updated with success.");
                            selectedUsernameTd.innerHTML = txtUsername.value;
                            selectedUserProfileTd.innerHTML = userProfileToText(user.UserProfile);

                            if (logedinUser === selectedUsername) {
                                logedinUser = txtUsername.value;
                            }

                            selectedUsername = txtUsername.value;

                            trId.style.display = "table-row";
                            trCreatedOn.style.display = "table-row";
                            trModifiedOn.style.display = "table-row";
                            txtId.value = user.Id;
                            //txtCreatedOn.value = Common.formatDate(new Date(user.CreatedOn));
                            txtCreatedOn.value = user.CreatedOn;
                            //txtModifiedOn.value = Common.formatDate(new Date(user.ModifiedOn));
                            txtModifiedOn.value = user.ModifiedOn;

                            if (logedinUser !== selectedUsername && user.UserProfile === 0) {
                                slctProfile.disabled = true;
                                saveAction.style.display = "none";
                                txtUsername.disabled = true;
                                emailText.disabled = true;
                                changePass.style.display = "none";
                                deleteAction.style.display = "none";
                                newPasswordTr.style.display = "none";
                                confirmPasswordTr.style.display = "none";
                            }

                        },
                        function () { }, auth);
                } else {
                    Common.toastError("An error occured while updating the user " + txtUsername.value + ".");
                }
            },
            function () { }, "", auth);
    }

    function getSelectedProfile() {
        return slctProfile.options[slctProfile.selectedIndex].value;
    }

    function validateEmail(email) {
        var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(String(email).toLowerCase());
    }

}