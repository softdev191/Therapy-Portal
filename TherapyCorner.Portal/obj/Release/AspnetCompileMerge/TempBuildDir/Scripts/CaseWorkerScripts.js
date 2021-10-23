var caseworkerFields = [];
var caseWorkers = [];
var diagAddCaseWorker;
var addingCWField;

$(document).ready(function()
{
    diagAddCaseWorker = $("#modalAddCaseWorker").dialog({
        autoOpen: false,
        width: 365,
        modal: true,
        buttons: { "Create": function () { ConfirmAddCaseworker(); }, "Cancel": function () { diagAddCaseWorker.dialog("close"); } }
    });
    $("#frmAddCaseWorker").validate({ errorClass: "inputinvalid" });

});
function InitializeCaseWorker(id, field)
{
    if (caseWorkers.length==0) LoadCaseWorkers();
    caseworkerFields.push({ fieldId: id, fieldName: field.replace(".","_").replace("[","_").replace("]","_").replace('"','') });

    $("#CaseWorker" + id).autocomplete({
        source: caseWorkers,
        minLength: 3,
        change: function( event, ui ) {
            CaseWorkerSelected(event,ui, id);
        },
        selected: function (event, ui) {
            CaseWorkerSelected(event, ui, id);
        }
    }
        );


}

function CaseWorkerSelected(event, ui, id)
{
    var field=null;
    for (var x = 0; x < caseworkerFields.length; x++)
    {
        if (caseworkerFields[x].fieldId == id)
        {
            field = caseworkerFields[x].fieldName;
        }
    }
    if (field != null) {
        if (ui.item != null) {
            $("#" + field).val(ui.item.id);
        }
        else {
            $("#" + field).val("");
            $("#CaseWorker"+id).val("");
        }
    }
}

function AddCaseWorker(id)
{
    addingCWField = id;
    $("#CaseWorkerName").val("");
    $("#CaseWorkerEmail").val("");
    diagAddCaseWorker.dialog("open");
}

function ConfirmAddCaseworker()
{
    var validator = $("#frmAddCaseWorker").validate();
    if(validator.form())
    {
        diagAddCaseWorker.dialog("close");
        TurnFreezePaneOn();
        var addr = window.location.origin + "/client/addcaseworker";

        $.ajax({
            url: addr,
            type: "POST",
            dataType: "json",
            data: {
                name: $("#CaseWorkerName").val(),
                email: $("#CaseWorkerEmail").val()
            },
            success: function (data) {
                CompleteAddCaseWorker(data);
            }
        });
    }
}

function CompleteAddCaseWorker(returndata)
{
    TurnFreezePaneOff();
    if (returndata.IsFailure) {
        var msgs = ProcessResponseBase(returndata);
        msgs.DisplayResponseResults();
        return;
    }
    var fullName = $("#CaseWorkerName").val().replace('"','') + " (" + $("#CaseWorkerEmail").val() + ")";
    caseWorkers.push({ label: fullName, value: fullName, id: returndata.ObjectId });

    var field=null;
    for (var x = 0; x < caseworkerFields.length; x++)
    {
        if (caseworkerFields[x].fieldId == addingCWField)
        {
            field = caseworkerFields[x].fieldName;
        }
    }
    if (field != null) {
        $("#CaseWorker" + addingCWField).val(fullName);
        $("#" + field).val(returndata.ObjectId);
    }
}