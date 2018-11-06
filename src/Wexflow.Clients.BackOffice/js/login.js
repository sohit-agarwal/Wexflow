function Login(){

    var uri = Common.trimEnd(Settings.Uri, "/");
	var loginBtn = document.getElementById("btn-login");
	var subscribeBtn = document.getElementById("btn-subscribe");
	var usernameTxt = document.getElementById("txt-username");
	var passwordTxt = document.getElementById("txt-password");
	
	loginBtn.onclick =  function(){
		login();
	};

    if (typeof subscribeBtn !== "undefined" && subscribeBtn !== null) {
        subscribeBtn.onclick = function () {
            if (typeof window.localStorage === "undefined") {
                alert("Your browser does not support localStorage.");
                return;
            }
            window.location.replace("subscribe.html");
        };
    }
	
    passwordTxt.onkeyup = function (event) {
        event.preventDefault();
        
        if (event.keyCode === 13) { 
			login();
		}
	};
	
    function login() {

        if (typeof window.localStorage === "undefined") {
            alert("Your browser does not support localStorage.");
            return;
        }

		var username = usernameTxt.value;
        var password = passwordTxt.value;
        var passwordHash = MD5(password);

        if (username === "" || password === "") {
            alert("Enter a valid username and password.");
        } else {
            Common.get(uri + "/user?username=" + encodeURIComponent(username), function (user) {
                if (typeof user === "undefined") {
                    alert("The user " + username + " does not exist. Subscribe to login.");
                } else {
                    if (passwordHash === user.Password) {
                        authorize(username, passwordHash);
                        window.location.replace("dashboard.html");
                    } else {
                        alert("The password is incorrect.");
                    }

                }
            });
        }
    }
    
}