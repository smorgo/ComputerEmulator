"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("https://localhost:5001/display").build();
var charWidth = 16;
var charHeight = 16;
var width = 40;
var height = 25;
const SVG_NS = "http://www.w3.org/2000/svg";
var keyId = 0;

connection.on("ReceiveMessage", function (user, message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = user + " says " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

connection.on("SetMode", function (w,h) {
    width = w;
    height = h;
    var screen = document.getElementById('screen');
    var screenHeight = h * charHeight + 4;
    var screenWidth = w * charWidth + 4;
    var vb = "0 0 " + screenWidth + " " + screenHeight;
    screen.setAttributeNS(null, 'viewBox', vb);
});

connection.on("Clear", function () {
    var screen = document.getElementById('screen');
    node.querySelectorAll('text').forEach(n => n.remove());

    var encodedMsg = "Clear screen";
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);

});

connection.on("Write", function (offset, value) {
    var elId = 'el'+offset;
    var el = document.getElementById(elId);
    if(!el)
    {
        var x = (offset % width) * charWidth + 2;
        var y = Math.trunc(offset / width) * charHeight + 12;
        el = document.createElementNS(SVG_NS, 'text');
        el.setAttributeNS(null,'id', elId);
        el.setAttributeNS(null,'x', x);
        el.setAttributeNS(null,'y', y);
        el.textContent = String.fromCharCode(value);
        var screen = document.getElementById('screen');
        screen.appendChild(el);
    }
    else
    {
        el.textContent = String.fromCharCode(value);
    }
});

connection.start().then(function () {
    document.onkeydown = function(evt) {
        var key = evt.key;
        connection.invoke("KeyDown", key, keyId);
        keyId++; 
        }
    document.onkeyup = function(evt) {
        var key = evt.key;
        connection.invoke("KeyUp", key, keyId);
        keyId++; 
        }
    connection.invoke("RequestControl");
});

// document.getElementById("sendButton").addEventListener("click", function (event) {
//     var user = document.getElementById("userInput").value;
//     var message = document.getElementById("messageInput").value;
//     connection.invoke("SendMessage", user, message).catch(function (err) {
//         return console.error(err.toString());
//     });
//     event.preventDefault();
// });