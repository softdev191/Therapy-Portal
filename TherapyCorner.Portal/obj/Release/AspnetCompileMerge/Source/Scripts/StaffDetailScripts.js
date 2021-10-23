var diagAdd, diagRemove, diagNone;

$(document).ready(function () {
    diagRemove = $("#modalDelete").dialog({
        autoOpen: false,
        width: 365,
        modal: true,
        buttons: { "Yes": function () { ConfirmDel(); }, "No": function () { diagRemove.dialog("close"); } }
    });

    diagNone = $("#modalCannotAdd").dialog({
        autoOpen: false,
        width: 365,
        modal: true,
        buttons: { "OK": function () { diagNone.dialog("close"); } }
    });

    diagAdd = $("#modalAdd").dialog({
        autoOpen: false,
        width: 365,
        modal: true,
        buttons: { "Add": function () { ConfirmAdd(); }, "Cancel": function () { diagAdd.dialog("close"); } }
    });

  
});

function AddService() {
    var addr = window.location.origin + "/staff/availableservices/" + $("#addId").val();
    //Fetch available services
    TurnFreezePaneOn();
    $.ajax({
        url: addr,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (returndata) {
            FinishServiceLookup(returndata);
        }
    });
}

function FinishServiceLookup(returndata)
{
    TurnFreezePaneOff();
    if (returndata.IsFailure) {
        var msgs = ProcessResponseBase(returndata);
        msgs.DisplayResponseResults();
    }

    if (returndata.EntityList == null || returndata.EntityList.length == 0)
    {
        diagNone.dialog("open");
    }
    else
    {
        var options = [];

        $.each(returndata.EntityList, function (index, itm)
        {
            options.push("<option value='" + itm.UniqueId + "'>" + itm.Name + "</option>");

        });
        $("#NewServiceId").html(options.join(''));
        diagAdd.dialog("open");
    }
}
function RemoveService(id, name) {
    $("#ServiceId").val(id);
    $("#delConfirm").text(name);
    diagRemove.dialog("open");
}

function ConfirmDel() {
    submitit();
    diagRemove.dialog("close");
    $("#frmDel").submit();
}

function ConfirmAdd() {
    submitit();

        diagAdd.dialog("close");
        $("#frmAdd").submit();
    
}

