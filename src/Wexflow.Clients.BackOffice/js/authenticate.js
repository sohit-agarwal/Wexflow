function authorize(username, password) {
    set("authorize", '{"Username": "' + username + '", "Password":"' + password + '"}', 365);
    console.log(window.localStorage);
}

function getUser() {
    return get("authorize");
}

function deleteUser() {
    remove("authorize");
}

function set(key, value) {
    window.localStorage.setItem(key, value);
}

function get(key) {
    return window.localStorage.getItem(key);
}

function remove(key) {
    return window.localStorage.removeItem(key);
}