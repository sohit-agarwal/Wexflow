function ForgotPassword() {
    "use strict";

    var uri = Common.trimEnd(Settings.Uri, "/");
    var txtUsername = document.getElementById("txt-username");
    var txtEmail = document.getElementById("txt-email");
    var btnSubmit = document.getElementById("btn-submit");

    btnSubmit.onclick = function () {
        sendEmail();
    };

    txtEmail.onkeyup = function(e) {
        e.preventDefault();
        if (e.keyCode === 13) {
            sendEmail();
        }
    };

    function sendEmail() {
        btnSubmit.disabled = true;

        var username = txtUsername.value;
        var email = txtEmail.value;

        if (username === "") {
            Common.toastInfo("Enter a username.");
            btnSubmit.disabled = false;
            return;
        }

        if (validateEmail(email) === false) {
            Common.toastInfo("Enter a valid email address.");
            btnSubmit.disabled = false;
            return;
        }

        Common.post(uri + "/resetPassword?username=" + encodeURIComponent(username) + "&email=" + encodeURIComponent(email), function (val) {
            if (val === true) {
                Common.toastSuccess("An email with a new password was sent to: " + email);
                setTimeout(function () {
                    Common.redirectToLoginPage();
                }, 5000);
            } else {
                Common.toastError("An error occured while sending the email.");
                btnSubmit.disabled = false;
            }
        });
    }

    function validateEmail(email) {
        var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(String(email).toLowerCase());
    }

}