
var diagSubmit=null;
var diagApprove, diagReject,diagShare, diagRemove, diagGoal,diagCancel;
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
    if ($("#modalCancel").length > 0) {
        diagCancel = $("#modalCancel").dialog({
            autoOpen: false,
            width: 365,
            modal: true,
            buttons: { "Yes": function () { ConfirmCancel(); }, "No": function () { diagCancel.dialog("close"); } }
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

function    DisableApprove()
{
    $("#btnApprove").prop("disabled", true);
}
function SaveMe()
{
    submitit();
    var addr = window.location.origin + "/report/save";
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
    var addr = window.location.origin + "/report/submit";
    $('#frmData').attr('action', addr).submit();
}

function PrintMe(id) {
    var addr = window.location.origin + "/report/pdf/"+id;
   window.open(addr);

}

function ShareMe()
{
    diagShare.dialog("open");
}

function CancelMe() {
    diagCancel.dialog("open");
}


function ConfirmShare()
{
    submitit();
    diagShare.dialog("close");
    $('#frmShare').submit();
}

function ConfirmCancel() {
    submitit();
    diagCancel.dialog("close");
    $('#frmCancel').submit();
}

function ApproveMe() {
    diagApprove.dialog("open");
}

function ConfirmApprove() {
    submitit();
    diagApprove.dialog("close");
    var addr = window.location.origin + "/report/approve";
    $('#frmData').attr('action', addr).submit();
}

function RejectMe() {
    diagReject.dialog("open");
}

function ConfirmReject() {
    submitit();
    diagReject.dialog("close");
    $("#RejectReason").val($("#Reason").val());
    var addr = window.location.origin + "/report/reject";

    $('#frmData').attr('action', addr).submit();

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
    var addr = window.location.origin + "/report/remattach/" + noteId + "?fileId="+remId;
    window.location=addr;

}

function ConfirmRemoveGoal() {
    submitit();
    diagGoal.dialog("close");
    var addr = window.location.origin + "/report/remgoal/" + noteId + "?goalId=" + goalId;
    window.location = addr;

}