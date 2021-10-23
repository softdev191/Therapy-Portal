
var diagSubmit=null;
var diagApprove, diagReject,diagShare, diagRemove, diagGoal;
var remId = null;
var noteId = null;
var goalId = null;

$(document).ready(function () {
    if ($("#modalSubmit").length > 0) {
        diagSubmit = $("#modalSubmit").dialog({
            autoOpen: false,
            width: 365,
            modal: true,
            buttons: { "Yes": function () { ConfirmSubmit(); }, "No": function () { diagSubmit.dialog("close"); } }
        });
    }
    if ($("#modalReject").length > 0) {
        diagReject = $("#modalReject").dialog({
            autoOpen: false,
            width: 365,
            modal: true,
            buttons: { "Reject": function () { ConfirmReject(); }, "Cancel": function () { diagReject.dialog("close"); } }
        });
    }
    if ($("#modalApprove").length > 0) {
        diagApprove = $("#modalApprove").dialog({
            autoOpen: false,
            width: 365,
            modal: true,
            buttons: { "Approve": function () { ConfirmApprove(); }, "Cancel": function () { diagApprove.dialog("close"); } }
        });
    }
    if ($("#modalShare").length > 0) {
        diagShare = $("#modalShare").dialog({
            autoOpen: false,
            width: 365,
            modal: true,
            buttons: { "Share": function () { ConfirmShare(); }, "Cancel": function () { diagShare.dialog("close"); } }
        });
    }
    if ($("#modalRemove").length > 0) {
        diagRemove = $("#modalRemove").dialog({
            autoOpen: false,
            width: 365,
            modal: true,
            buttons: { "Yes": function () { ConfirmRemove(); }, "No": function () { diagRemove.dialog("close"); } }
        });
    }
    if ($("#modalRemoveGoal").length > 0) {
        diagGoal = $("#modalRemoveGoal").dialog({
            autoOpen: false,
            width: 365,
            modal: true,
            buttons: { "Yes": function () { ConfirmRemoveGoal(); }, "No": function () { diagGoal.dialog("close"); } }
        });
    }
});

function SaveMe()
{
    submitit();
    var addr = window.location.origin + "/notes/save";
    $('#frmData').attr('action', addr).submit();

}

function SaveTime() {
    submitit();
    var addr = window.location.origin + "/notes/savetime";
    $('#frmData').attr('action', addr).submit();

}

function SubmitMe() {
    if (diagSubmit == null) {
        ConfirmSubmit();
    }
    else
    {
        diagSubmit.dialog("open");
    }

}

function ConfirmSubmit()
{
    submitit();
    var addr = window.location.origin + "/notes/submit";
    $('#frmData').attr('action', addr).submit();
}

function PrintMe(id) {
    var addr = window.location.origin + "/notes/pdf/"+id;
   window.open(addr);

}

function ShareMe()
{
    diagShare.dialog("open");
}

function ConfirmShare()
{
    submitit();
    diagShare.dialog("close");
    $('#frmShare').submit();
}

function ApproveMe() {
    diagApprove.dialog("open");
}

function ConfirmApprove() {
    submitit();
    diagApprove.dialog("close");
    $("#ActionType").val($("#ServiceAction").val());
    var addr = window.location.origin + "/notes/approve";
    $('#frmData').attr('action', addr).submit();
}

function RejectMe() {
    diagReject.dialog("open");
}

function ConfirmReject() {
    submitit();
    $("#Reason").val($("#RejectReason").val());
    diagReject.dialog("close");
    var addr = window.location.origin + "/notes/reject";
    $('#frmData').attr('action', addr).submit();

}

function HighlightGoals(area)
{
    $(".GoalArea").removeClass("GoalHighlight");
    $(".GoalArea" + area).addClass("GoalHighlight");
}

function RemoveFile(id, name, note)
{
    remId = id;
    noteId = note;
    $("#rAttach").text(name);
    diagRemove.dialog("open");

}

function RemoveGoal(id, name, note) {
    goalId = id;
    noteId = note;
    $("#rGoal").text(name);
    diagGoal.dialog("open");

}


function ConfirmRemove() {
    submitit();
    diagRemove.dialog("close");
    var addr = window.location.origin + "/notes/remattach/" + noteId + "?fileId="+remId;
    window.location=addr;

}

function ConfirmRemoveGoal() {
    submitit();
    diagGoal.dialog("close");
    var addr = window.location.origin + "/notes/remgoal/" + noteId + "?goalId=" + goalId;
    window.location = addr;

}