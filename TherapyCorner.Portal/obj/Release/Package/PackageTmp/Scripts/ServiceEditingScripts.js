function DisciplineChanged() {
    TurnFreezePaneOn();
    var addr = window.location.origin + "/discipline/items/" + $("#Discipline_UniqueId").val();

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        success: function (data) {
            CompleteDisciplineChange(data);
        }
    });
}

function CompleteDisciplineChange(returndata) {
    TurnFreezePaneOff();
    var msgs = ProcessResponseBase(returndata);
    msgs.DisplayResponseResults();
    if (returndata.IsFailure) {

        return;
    }

    var durations = [];
    if (returndata.FreqDurations != null) {
        $.each(returndata.FreqDurations, function (index, itm) {
            var row = '<input type="checkbox" name="DurationIds" value="' + itm.UniqueId + '"/> ' + itm.Name + '</br>';
            durations.push(row);

        });
    }
    $("#DurationList").html(durations.join(''));

    var rates = [];
    if (returndata.PayRates != null) {
        $.each(returndata.PayRates, function (index, itm) {
            var row = '<input type="checkbox" name="RateIds" value="' + itm.UniqueId + '"/> ' + itm.Name + '</br>';
            rates.push(row);

        });
    }
    $("#RateList").html(rates.join(''));

    var providers = [];
    if (returndata.ProvidingStaff != null) {
        $.each(returndata.ProvidingStaff, function (index, itm) {
            var row = '<input type="checkbox" name="ProviderIds" value="' + itm.UniqueId + '"/> ' + itm.Name + '</br>';
            providers.push(row);

        });
    }
    $("#ProviderList").html(providers.join(''));

}

function FilterCPTs() {
    var term = $("#CPTFilter").val();

    if (term == '') {
        $(".CPTOption").show();
    }
    else {
        $(".CPTOption").hide();
        $('.CPTOption:contains("' + term + '")').show();
    }
}

function RemoveMe() {
    diagRemove.dialog("open");
}



function ConfirmDel() {
    submitit();
    diagRemove.dialog("close");
    $("#frmDel").submit();
}
function ActivateMe() {
    submitit();
    $("#frmActivate").submit();
}
function DeactivateMe() {
    submitit();
    $("#frmDeactivate").submit();
}