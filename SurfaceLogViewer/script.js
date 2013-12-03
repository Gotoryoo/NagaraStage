/**
* @filename script.js
* @author Hirokazu Yokoyama
*/

var SurfaceIndex = new Object();
SurfaceIndex.LowBottom = 0;
SurfaceIndex.LowTop = 1;
SurfaceIndex.UpBottom = 2;
SurfaceIndex.UpTop = 3;

$(document).ready(function(){
    $("#displayingButton").bind("click", function(){
        var files = document.getElementById("fileInput").files;
        for(var i = 0; i < files.length; ++i) {
            var fileData = files[i];
            if(fileData.type != "text/xml") {
                alert("File type error.");
                return false;
            }

            var reader = new FileReader();
            reader.onload = function(env){
                var resultObj = parseLogXml(env.target.result);
                writeResult(resultObj);
            };
            reader.readAsText(fileData, "utf-8");
        }
    });
});

var parseLogXml = function(xml) {
    var sample = new Object();
    sample.PlateNo = $(xml).find("PlateNo").text();
    sample.ModuleNo = $(xml).find("ModuleNo").text();
    sample.MotorSpeed = $(xml).find("MotorSpeed").text();
    sample.Time = $(xml).find("Time").text();
    sample.ZValueByPc = new Array(0, 0, 0, 0);
    sample.ZValueByEye = new Array(0, 0, 0, 0);
    var i = 0; var j = 0;
    var events = $(xml).find("EventList").children();
    $(events).each(function(){
        var isBoundary = $(this).attr("IsBoundary");
        if(isBoundary.indexOf("t", 0) >= 0) {
            sample.ZValueByPc[i] = parseFloat($(this).attr("Distance")) * 1000;
            ++i;
        }
        if($(this).attr("Note").indexOf("境界", 0) >= 0) {
            sample.ZValueByEye[j] = $(this).attr("Distance") * 1000;
            ++j;
        }
    });
    sample.DiffLowBottom = sample.ZValueByEye[0] - sample.ZValueByPc[0];
    sample.DiffLowTop = sample.ZValueByPc[1] - sample.ZValueByEye[1];
    sample.DiffUpBottom = sample.ZValueByEye[2] - sample.ZValueByPc[2];
    sample.DiffUpTop =  sample.ZValueByPc[3] - sample.ZValueByEye[3] ;
    return sample;
};


var writeResult = function(resultObj){
    var row = document.createElement("tr");
    var speedCell = document.createElement("td");
    speedCell.innerHTML = parseFloat(resultObj.MotorSpeed).toFixed(2);
    $(row).append(speedCell);
    var timeCell = document.createElement("td");
    timeCell.innerHTML = parseFloat(resultObj.Time).toFixed(3);
    $(row).append(timeCell);
    var diffUpTopCell = document.createElement("td");
    diffUpTopCell.innerHTML = resultObj.DiffUpTop.toFixed(4);
    $(row).append(diffUpTopCell);
    var diffUpBottomCell = document.createElement("td");
    diffUpBottomCell.innerHTML = resultObj.DiffUpBottom.toFixed(4);
    $(row).append(diffUpBottomCell);
    var diffLowTopCell = document.createElement("td");
    diffLowTopCell.innerHTML = resultObj.DiffLowTop.toFixed(4);
    $(row).append(diffLowTopCell);
    var diffLowBottomCell = document.createElement("td");
    diffLowBottomCell.innerHTML = resultObj.DiffLowBottom.toFixed(4);
    $(row).append(diffLowBottomCell);
    $("#resultTable").append(row);
};
