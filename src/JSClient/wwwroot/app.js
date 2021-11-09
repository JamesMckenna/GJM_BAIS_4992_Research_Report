﻿function log() {
    document.getElementById('results').innerText = '';

    Array.prototype.forEach.call(arguments, function (msg) {
        if (msg instanceof Error) {
            msg = "Error: " + msg.message;
        }
        else if (typeof msg !== 'string') {
            msg = JSON.stringify(msg, null, 2);
        }
        document.getElementById('results').innerHTML += msg + '\r\n';
    });
}

function login() {
    mgr.signinRedirect();
}

function api() {
    mgr.getUser().then(function (user) {
        var url = "https://localhost:6001/developer";

        var xhr = new XMLHttpRequest();
        xhr.open("GET", url);
        xhr.onload = function () {
            log(xhr.status, JSON.parse(xhr.responseText));
        }
        xhr.setRequestHeader("Authorization", "Bearer " + user.access_token);
        xhr.send();
    });
}

function logout() {
    mgr.signoutRedirect();
}

function home() {
    mgr.getUser().then(function () {
        window.location.href = "https://localhost:5003/index.html";
    });
}

function mvcClient() {
    mgr.getUser().then(function (user) {
        window.location.href = "https://localhost:5002"
    });
}

function manage() {
    //mgr.getUser().then(function (user) {
        //var url = "https://localhost:6001/identity/account/manage";

        //var xhr = new XMLHttpRequest();
        //xhr.open("GET", url);
        //xhr.onload = function () {
        //    log(xhr.status, JSON.parse(xhr.responseText));
        //}
        //xhr.setRequestHeader("Authorization", "Bearer " + user.access_token);
        //xhr.send();
        //window.location.href = "https://localhost:6001"
    //});
    window.location.href = "https://localhost:6001"
}

var config = {
    authority: "https://localhost:5001",
    client_id: "JSClient",
    redirect_uri: "https://localhost:5003/callback.html",
    response_type: "code",
    scope: "openid profile AnAPI",
    post_logout_redirect_uri: "https://localhost:5003/index.html",
};
var mgr = new Oidc.UserManager(config);

mgr.getUser().then(function (user) {
    if (user) {
        log("User logged in", user.profile);
        let body = document.getElementById("nav");

        var apibtn = document.createElement("button");
        apibtn.setAttribute("id", "api");
        var apiValue = document.createTextNode("Call API");
        apibtn.appendChild(apiValue);
        body.appendChild(apibtn);
        apibtn.setAttribute("onclick", api);


        var logoutbtn = document.createElement("button");
        var logoutValue = document.createTextNode("Logout");
        logoutbtn.setAttribute("id", "logout");
        logoutbtn.appendChild(logoutValue);
        body.appendChild(logoutbtn);
        logoutbtn.setAttribute("onclick", logout);

        var mvcbtn = document.createElement("button");
        var mvcValue = document.createTextNode("SSO MvcClient");
        mvcbtn.setAttribute("id", "mvc");
        mvcbtn.appendChild(mvcValue);
        body.appendChild(mvcbtn);
        mvcbtn.setAttribute("onclick", mvc);

        var managebtn = document.createElement("a");
        var manageValue = document.createTextNode("Manage Account");
        managebtn.setAttribute("id", "manage");
        managebtn.setAttribute("type", "button");
        managebtn.setAttribute("class", "button");
        managebtn.appendChild(manageValue);
        body.appendChild(managebtn);
        managebtn.setAttribute("href", "https://localhost:6001/identity/account/manage");
       
        
        document.getElementById("api").addEventListener("click", api, false);
        document.getElementById("logout").addEventListener("click", logout, false);
        document.getElementById("mvc").addEventListener("click", mvcClient, false);
    }
    else {
        log(`You are not logged in on the JS Client app.\nClick Login to be redirected to the Secure Token Server to login.\nIf you logged into the MvcClient app, click login to see Single Sign-On in action!`);
    }
});

document.getElementById("home").addEventListener("click", home, false);
document.getElementById("login").addEventListener("click", login, false);