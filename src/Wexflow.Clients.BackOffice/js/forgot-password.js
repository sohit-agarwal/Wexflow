function ForgotPassword() {
    "use strict";

    var uri = Common.trimEnd(Settings.Uri, "/");
    var txtUsername = document.getElementById("txt-username");
    var btnSubmit = document.getElementById("btn-submit");

    btnSubmit.onclick = function () {
        sendEmail();
    };

    txtUsername.onkeyup = function(e) {
        e.preventDefault();
        if (e.keyCode === 13) {
            sendEmail();
        }
    };

    function sendEmail() {
        Common.get(uri + "/user?username=" + encodeURIComponent(txtUsername.value), function (u) {

            if (typeof u === "undefined" || u === null) {
                alert("The user " + txtUsername.value + " does not exist.");
            } else {
                var email = u.Email;
                if (email === "" || email === null || typeof email === "undefined") {
                    alert("The user " + txtUsername.value + " does not have an email.");
                } else {
                    Common.post(uri + "/resetPassword?username=" + encodeURIComponent(u.Username) + "&email=" + encodeURIComponent(email), function (val) {
                        if (val === true) {
                            alert("An email with a new password was sent to: " + u.Email);
                            Common.redirectToLoginPage();
                        } else {
                            alert("An error occured while sending the email.");
                        }
                    });
                }
            }

        });
    }
}