var diagDeletes;
var diagRecips;
$(document).ready(function()
{
    if ($("#modalConfirmDeletes").length > 0) {
        diagDeletes = $("#modalConfirmDeletes").dialog({
            autoOpen: false,
            width: 365,
            modal: true,
            buttons: { "Yes": function () { ConfirmDeletes(); }, "No": function () { diagDeletes.dialog("close"); } }
        });
    }
    if ($("#modalConfirmDelete").length > 0) {
        diagDeletes = $("#modalConfirmDelete").dialog({
            autoOpen: false,
            width: 365,
            modal: true,
            buttons: { "Yes": function () { DeleteMe(); }, "No": function () { diagDeletes.dialog("close"); } }
        });
    }
    if ($("#modalSelStaff").length > 0) {
        diagRecips = $("#modalSelStaff").dialog({
            autoOpen: false,
            width: 365,
            modal: true,
            buttons: { "OK": function () { SetRecips(); }, "Cancel": function () { diagRecips.dialog("close"); } }
        });
    }
});


function ToggleDelete()
{
    var btn = $("#btnDelete");
    if($("input:checkbox:checked").length>0)
    {
        btn.prop("disabled", false);

    }
    else
    {
        btn.prop("disabled", true);

    }
}

function DeleteMessages()
{
    diagDeletes.dialog("open");
}

function ConfirmDeletes()
{
    diagDeletes.dialog("close");
    var ids = [];
    $('input:checkbox:checked').each(function () {
        ids.push($(this).val());
    });
    $('#DeleteIds').val(ids.join(','));

    submitit();
    $("#frmDelete").submit();

}


function DeleteMe() {
    diagDeletes.dialog("close");
  

    submitit();
    $("#frmDelete").submit();

}

function SetContents() {
    $("#MessageBody").val($('#EditContents').trumbowyg('html'));
}

function SelectAllRecips()
{
    $('input:checkbox').each(function () {
        $(this).prop('checked', true);
    })
}

function SelectNoRecips() {
    $('input:checkbox').each(function () {
        $(this).prop('checked', false);
    })
}

function SetRecipients()
{
    diagRecips.dialog("open");
}

function SetRecips()
{
    diagRecips.dialog("close");
    var ids = [];
    var names = [];
    $('input:checkbox:checked').each(function () {
        ids.push($(this).val());
        names.push($("#StaffName" + $(this).val()).text());
    });

    $('#RecipientIds').val(ids.join(','));
    $('#RecipNames').text(names.join('; '));

    if(ids.length==0)
    {
        $("#btnSend").prop("disabled", true);
    }
    else
    {
        $("#btnSend").prop("disabled", false);
    }

}