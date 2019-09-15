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
        btnSubmit.disabled = true;

        var username = txtUsername.value;

        if (username === "") {
            Common.toastInfo("Enter a username.");
            btnSubmit.disabled = false;
            return;
        }

        Common.post(uri + "/resetPassword?u=" + encodeURIComponent(username), function (val) {
            if (val === true) {
                Common.toastSuccess("An email with a new password was sent to: " + username);
                setTimeout(function () {
                    Common.redirectToLoginPage();
                }, 5000);
            } else {
                Common.toastError("An error occured while sending the email.");
                btnSubmit.disabled = false;
            }
        });
    }

}