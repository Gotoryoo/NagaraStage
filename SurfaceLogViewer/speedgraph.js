/**
* @author Hirokazu Yokoyama
*/

$(document).ready(function(){
    $("#openButton").bind("click", function(){
        var fileData = document.getElementById("fileInput").files[0];
        if(fileData.type != "text/xml") {
            return false;
        }

        var numOfloadedFiles = 0;
        var reader = new FileReader();
        reader.onload = function(env) {
            var speeds = getSpeedsFrom(env.target.result);
            draw(speeds);
        };
        reader.readAsText(fileData, "utf-8");
    });

    $("#saveButton").bind("click", function(){
//        var blobBuilder = new
    });
});


var getSpeedsFrom = function(xmlDocument) {
    var events = $(xmlDocument).find("EventList").children();
    var speeds = new Array();
    var i = 0;
    var distance, time;
    $("#speedParam").html($(xmlDocument).find("MotorSpeed").text());
    $(events).each(function(){
        distance = parseFloat($(this).attr("Distance")) * 1000;
        time = parseFloat($(this).attr("Second"));
        speeds[i] = (time != 0 ?  distance / time : -1);
        addTable(time, distance, speeds[i]);
        ++i;
    });
    return speeds;
};

var addTable = function(time, distance, speed) {
    var cell = document.createElement("td");
    $(cell).html(String(time.toFixed(3)));
    $("#secRow").append(cell);
    cell = document.createElement("td");
    $(cell).html(String(distance.toFixed(1)));
    $("#distanceRow").append(cell);
    cell = document.createElement("td");
    $(cell).html(String(speed.toFixed(3)));
    $("#speedRow").append(cell);
};

var draw = function(speeds) {
    var canvas = document.getElementById("Canvas");
    var width = 600;
    var height = 200;
    $(canvas).css("width", width);
    $(canvas).css("height", height);
    var ctx = canvas.getContext('2d');
    ctx.beginPath();
    ctx.moveTo(1,20);
    for(var i = 1; i < speeds.length; ++i) {
        height = (height < speeds[i] ? speeds[i] : height);
        ctx.lineTo(i , speeds[i]);
    }
    ctx.stroke();
    //$(canvas).css("height", height);
};
