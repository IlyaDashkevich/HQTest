"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/candleHub")
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
        var periodInSec = parseInt(document.getElementById("wsPeriod").value, 10);

        var from = document.getElementById("wsBeginTime").value;
        var to = document.getElementById("wsEndTime").value;

        var count = parseInt(document.getElementById("wsCount").value, 10);

        connection.invoke("TakeCandle", pair, periodInSec, from, to, count)
            .catch(function (err) {
                console.error(err.toString());
            });

        event.preventDefault();
    });
connection.on("ReceiveCandle", function (candle) {
    var table = document.getElementById("wsCandleTable");

    var row = table.insertRow(-1);
    row.insertCell(0).textContent = candle.pair;
    row.insertCell(1).textContent = candle.openPrice;
    row.insertCell(2).textContent = candle.highPrice;
    row.insertCell(3).textContent = candle.lowPrice;
    row.insertCell(4).textContent = candle.closePrice;
    row.insertCell(5).textContent = candle.totalPrice;
    row.insertCell(6).textContent = candle.totalVolume;
    row.insertCell(7).textContent = new Date(candle.openTime).toLocaleString();
});