var sessionExp;
var diagSaveReminder;

function InitiateReminder(exp, fSave)
{
    sessionExp = new Date(exp);
    var diffdate = sessionExp - new Date();

    window.setTimeout(RemindToSave, diffdate-100000);
    window.setInterval(RemindToSave, 300000);
    diagSaveReminder = $("#modalReminder").dialog({
        autoOpen: false,
        width: 365,
        modal: true,
        buttons: { "Yes": function () { diagSaveReminder.dialog("close"); fSave();}, "No": function () { diagSaveReminder.dialog("close"); } }
    });
}

function RemindToSave()
{
    var fcsd = $(":focus");
    if (diagSaveReminder.dialog("isOpen"))
    {
        return;
    }
    diagSaveReminder.dialog("open");
    if (fcsd.length>0)
    {
        $("#ReturnField").val(fcsd[0].id);
    }
}