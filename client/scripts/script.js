// Initialization
function init() {
    // call these routines when the page has finished loading
    initializeEvents();
    initializeSocket();
}

// global for all the input elements for probe data
var dataElements = [];

// global variable for the WebSocket
var ws;

// player id
var playerId = 42;
var role;
var started = false;

function initializeSocket() {
    // Open the WebSocket and set up its handlers
    loc = "ws://" + location.host + "/ws";
    ws = new WebSocket(loc);
    ws.onopen = beginSocket;
    ws.onmessage = function (evt) { receiveMessage(evt.data) };
    ws.onclose = endSocket;
    ws.onerror = endSocket;
}

function receiveMessage(msg) {
    // receiveMessage is called when any message from the server arrives on the WebSocket
    console.log("recieved: " + msg);
    var r = JSON.parse(msg);
    processCommand(r);
}

function sendMessage(msg) {
    // simple send
    var m = JSON.stringify(msg);
    console.log("sending: " + m)
    ws.send(m);
}

function beginSocket() {
    // handler for socket open
}

function endSocket() {
    // ask the user to reload the page if the socket is lost
    // if (confirm("Lost connection to server. Reload page?")) {
    //   location.reload(true);
    // }
}

function setID(id) {
    playerId = id;
    if (playerId == 0) {
        var start_container = document.getElementById("start_container");
        start_container.className = "";
    }
}

function makeVisible(word) {
    var w = document.getElementById("_" + word + "_");
    w.className = "";
    w.classList.add("content");
    w.classList.add(w.name);
    w.classList.add("invis");
}

function setName(name) {
    var name_text = document.getElementById("name_text");
    name_text.innerText = name;
}

function initializeEvents() {
    // Set up the event handlers
    var name = document.getElementById("name");
    name.addEventListener("change", function () {
        setName(name.value);
    });
}

// Page init
window.addEventListener("load", initializeEvents, false);