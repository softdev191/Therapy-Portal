function FetchClaims() {
    var addr = window.location.origin + "/claim/StatusCounts";

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        data: {
            statuses: "9,10,11"
        },
        success: function (data) {
            CompleteSetNumber(data, "Claims");
        }
    });
}

function FetchMakeups() {
    var addr = window.location.origin + "/clientservice/MakeupCount";

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        success: function (data) {
            CompleteSetNumber(data, "Makeups");
        }
    });
}


function FetchNeedRx() {
    var addr = window.location.origin + "/clientservice/NeedRxCount";

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        success: function (data) {
            CompleteSetNumber(data, "NeedRx");
        }
    });
}




function FetchPendingServices() {
    var addr = window.location.origin + "/clientservice/PendingCount";

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        success: function (data) {
            CompleteSetNumber(data, "PendingServices");
        }
    });
}

function FetchScheduling() {
    var addr = window.location.origin + "/clientservice/SchedulingCount";

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        success: function (data) {
            CompleteSetNumber(data, "Scheduling");
        }
    });
}

function FetchPendingNotes() {
    var addr = window.location.origin + "/notes/PendingCount";

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        success: function (data) {
            CompleteSetNumber(data, "PendingNotes");
        }
    });
}

function FetchPendingReports() {
    var addr = window.location.origin + "/report/PendingCount";

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        success: function (data) {
            CompleteSetNumber(data, "PendingReports");
        }
    });
}

function FetchReportReviews() {
    var addr = window.location.origin + "/report/ReviewCount";

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        success: function (data) {
            CompleteSetNumber(data, "ReportReviews");
        }
    });
}

function FetchNoteReviews() {
    var addr = window.location.origin + "/notes/ReviewCount";

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        success: function (data) {
            CompleteSetNumber(data, "NoteReviews");
        }
    });
}
function CompleteSetNumber(returndata, field) {
    if (returndata.IsFailure) {
        var msgs = ProcessResponseBase(returndata);
        msgs.DisplayResponseResults();
        return;
    }
    $("#spin" + field).hide();
    $("#sp" + field).text(returndata.ObjectId);
}