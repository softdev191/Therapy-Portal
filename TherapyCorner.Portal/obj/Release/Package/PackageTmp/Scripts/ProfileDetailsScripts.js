var diagAdd, diagRemove, diagRemExt;

$(document).ready(function () {
    diagRemove = $("#modalDelete").dialog({
        autoOpen: false,
        width: 365,
        modal: true,
        buttons: { "Yes": function () { ConfirmDel(); }, "No": function () { diagRemove.dialog("close"); } }
    });

    diagRemExt = $("#modalExternal").dialog({
        autoOpen: false,
        width: 365,
        modal: true,
        buttons: { "Yes": function () { ConfirmRemExt(); }, "No": function () { diagRemExt.dialog("close"); } }
    });

    diagAdd = $("#modalAdd").dialog({
        autoOpen: false,
        width: 365,
        modal: true,
        buttons: { "Add": function () { ConfirmAdd(); }, "Cancel": function () { diagAdd.dialog("close"); } }
    });

    $("#frmAdd").validate({errorClass:"inputinvalid"});
    //$("#InviteCode").rules("add",{
    //    required: true,
    //    messages : {required:"Required"}
        
    //});
});

 function   AddCompany()
    {
        $("#InviteCode").val("");
        diagAdd.dialog("open");
    }

   function RemoveCompany(id,name)
    {
        $("#Company").val(id);
        $("#delConfirm").text(name);
        diagRemove.dialog("open");
    }

    function ConfirmDel()
    {
        submitit();
        diagRemove.dialog("close");
        $("#frmDel").submit();
    }

    function ConfirmAdd()
    {
        submitit();
        var validator =  $("#frmAdd").validate();
        if(validator.form())
        {
            diagAdd.dialog("close");
            $("#frmAdd").submit();
        }
    }

    function RemoveExternal()
    {
        diagRemExt.dialog("open");
    }

    function ConfirmRemExt()
    {
        submitit();
        diagRemExt.dialog("close");
        $("#frmRemExt").submit();
    }