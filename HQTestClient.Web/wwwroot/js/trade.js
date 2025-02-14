"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/tradeHub")
    .build();

document.getElementById("wsRequestButton").disabled = true;

connection.start().then(function () {
    document.getElementById("wsRequestButton").disabled = false;
}).catch(function (err) {
    console.error(err.toString());
});

document.getElementById("wsRequestButton")
    .addEventListener("click", function (event) {
        var pair = document.getElementById("wsPair").value;
        var count = parseInt(document.getElementById("wsMaxCount").value, 10);
        
        connection.invoke("TakeTrade", pair, count)
            .catch(function (err) {
                console.error(err.toString());
            });

        event.preventDefault();
    });
connection.on("ReceiveTrade", function (trade) {
    var table = document.getElementById("wsTradeTable");

    var row = table.insertRow(-1);
    row.insertCell(0).textContent = trade.pair;
    row.insertCell(1).textContent = trade.price;
    row.insertCell(2).textContent = trade.amount;
    row.insertCell(3).textContent = trade.side;
    row.insertCell(4).textContent = trade.time;
});

