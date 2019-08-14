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
        Common.get(uri + "/user?username=" + encodeURIComponent(txtUsername.value), function (u) {

            if (typeof u === "undefined" || u === null) {
                Common.toastInfo("The user " + txtUsername.value + " does not exist.");
                btnSubmit.disabled = false;
            } else {
                var email = u.Email;
                if (email === "" || email === null || typeof email === "undefined") {
                    Common.toastInfo("The user " + txtUsername.value + " does not have an email.");
                    btnSubmit.disabled = false;
                } else {
                    Common.post(uri + "/resetPassword?username=" + encodeURIComponent(u.Username) + "&email=" + encodeURIComponent(email), function (val) {
                        if (val === true) {
                            Common.toastSuccess("An email with a new password was sent to: " + u.Email);
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

        });
    }

    $(".field-wrapper .field-placeholder").on("click", function () {
        $(this).closest(".field-wrapper").find("input").focus();
    });
    $(".field-wrapper input").on("keyup", function () {
        var value = $.trim($(this).val());
        if (value) {
            $(this).closest(".field-wrapper").addClass("hasValue");
        } else {
            $(this).closest(".field-wrapper").removeClass("hasValue");
        }
    });

}