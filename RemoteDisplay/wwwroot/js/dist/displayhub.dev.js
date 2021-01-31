"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("https://localhost:5001/display").build();
var charWidth = 16;
var charHeight = 16;
var width = 40;
var height = 25;
var SVG_NS = "http://www.w3.org/2000/svg";
connection.on("ReceiveMessage", function (user, message) {
  var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
  var encodedMsg = user + " says " + msg;
  var li = document.createElement("li");
  li.textContent = encodedMsg;
  document.getElementById("messagesList").appendChild(li);
});
connection.on("SetMode", function (w, h) {
  width = w;
  height = h;
  charWidth = 400 / width;
  charHeight = 250 / height;
});
connection.on("Write", function (offset, value) {
  debugger;
  var elId = 'el' + offset;
  var el = document.getElementById(elId);

  if (!el) {
    var x = offset % width * charWidth + 2;
    var y = Math.trunc(offset / width) * charHeight + 12;
    el = document.createElementNS(SVG_NS, 'text');
    el.setAttributeNS(null, 'id', elId);
    el.setAttributeNS(null, 'x', x);
    el.setAttributeNS(null, 'y', y);
    el.textContent = String.fromCharCode(value);
    var screen = document.getElementById('screen');
    screen.appendChild(el);
  } else {
    el.textContent = String.fromCharCode(value);
  }
});
connection.start().then(function () {//     document.getElementById("sendButton").disabled = false;
  // }).catch(function (err) {
  //     return console.error(err.toString());
}); // document.getElementById("sendButton").addEventListener("click", function (event) {
//     var user = document.getElementById("userInput").value;
//     var message = document.getElementById("messageInput").value;
//     connection.invoke("SendMessage", user, message).catch(function (err) {
//         return console.error(err.toString());
//     });
//     event.preventDefault();
// });