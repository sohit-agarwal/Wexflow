function Subscribe(){

    var uri = Common.trimEnd(Settings.Uri, "/");
	var submiBtn = document.getElementById("btn-submit");
	var usernameTxt = document.getElementById("txt-username");
	var passwordTxt = document.getElementById("txt-password");
	var confirmPasswordTxt = document.getElementById("txt-confirm-password");
	
	submiBtn.onclick = function(){
		subscribe();
	};
	
    confirmPasswordTxt.onkeyup = function (event){
		event.preventDefault();
        
        if (event.keyCode === 13) { 
			subscribe();
		}
	};
	
    function subscribe() {

		var username = usernameTxt.value;
		var password = passwordTxt.value;
		var confirmPassword = confirmPasswordTxt.value;

        if (password === "" || confirmPassword === "") {
            alert("Enter a valid password.");
        } else if (password !== confirmPassword) {
            alert("Passwords don't match.");
        } else {
            var passwordHash = MD5(password);
            Common.get(uri + "/user?username=" + username, function (user) {
                if (typeof user === "undefined") {
                    Common.post(uri + "/insertUser?username=" + encodeURIComponent(username) + "&password=" + encodeURIComponent(passwordHash), function () {
                        authorize(username, passwordHash);
                        window.location.replace("dashboard.html");
                    });
                } else {
                    alert("The user " + username + " already exists. Try whith another username.");
                }
            });
        }

    }
}