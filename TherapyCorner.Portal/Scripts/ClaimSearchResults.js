var forInsurance = true;

var diagSO;

$(document).ready(function () {


    diagSO = $("#modalSO").dialog({
        autoOpen: false,
        width: 365,
        modal: true,
        buttons: { "Yes": function () { ConfrimSO(); }, "No": function () { diagSO.dialog("close"); } }
    });

});


var checkme = true;
function SelectAll() {
    var c = $("#sac");
    $('input:checkbox').not(c).not("[disabled]").prop('checked', checkme);
    checkme = !checkme;
}

function GetSelected() {
    var c = $("#sac");
    var c2 = $(".HeaderSelector");
    var lst = $('input:checkbox:checked').not(c).not(c2).map(function () { return this.value; }).get().join(',');
    var addr = window.location.origin + "/claim/quickfilter";
    TurnFreezePaneOn();

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        data: {
            ids: lst,
            forInsurance: forInsurance
        },
        success: function (data) {
            CompleteQuickFilter(data);
        }
    });
}

function CompleteQuickFilter(returndata) {
    TurnFreezePaneOff();

    if (returndata.IsFailure) {
        var msgs = ProcessResponseBase(returndata);
        msgs.DisplayResponseResults();
        return;
    }
    $("#FileIds").val(returndata.ObjectId);
    if (forInsurance) {
        frmFile.action = window.location.origin + "/claim/hcfa";

    }
    else {
        frmFile.action = "/claim/govtsub";

    }
    frmFile.submit();
    $("#Id").val(returndata.ObjectId);
    diagSO.dialog("open");
}

function ToggleArea(id) {
    $("#Area" + id).toggle();
}

function HCFA() {
    forInsurance = true;
    GetSelected();

}

function GovtProcessing() {
    forInsurance = false;
    GetSelected();

}
function ConfrimSO() {
    submitit();
    diagSO.dialog("close");
    $("#frmSO").submit();
}

function SelectClaims(id) {
    var c = $("#sc" + id);
    var scheck = c[0].checked;
    $('.checkclaims' + id).not(c).not("[disabled]").prop('checked', scheck);

}

function ResolveClaims() {
    var c = $("#sac");
    var c2 = $(".HeaderSelector");

  var lst = $('input:checkbox:checked').not(c).not(c2).map(function () { return this.value; }).get().join(',');
    var addr = window.location.origin + "/claim/resolve?ids=" + lst;
    submitit();
    window.location = addr;

}