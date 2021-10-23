    var curClient = 0;
    var lastComment = "";
var denialreasons = [];

function InsAmountChanged(id, client) {
    var tval = $("#Claims_" + id + "_Amount").val();
    if (tval != null && tval != "" && !isNaN(tval)) {
        newAmount = parseFloat(tval);
        if (newAmount < 0) {
            $("#Claims_" + id + "_Amount").addClass("alert-danger");

        }
        else {
            $("#Claims_" + id + "_Amount").removeClass("alert-danger");

        }
    }
    else {
        if (tval != null && tval != "") {
            $("#Claims_" + id + "_Amount").addClass("alert-danger");
            
        }
    }
    Amount(id, client);
}
function AmountChanged(id, client) {
    var newAmount = 0;
    var total = 0;
    var oldAmount = 0;
    var clientAmount = 0;

    var tval = $("#Claims_" + id + "_Amount").val();
    if (tval != null && tval != "" && !isNaN(tval)) {
        newAmount = parseFloat(tval);
        if (newAmount < 0) {
            $("#Claims_" + id + "_Amount").val("");
            $("#Claims_" + id + "_Amount").focus();
            return;
        }
        $("#Claims_" + id + "_Amount").val(newAmount.toFixed(2));
    }
    else {
        if (tval != null && tval != "") {
            $("#Claims_" + id + "_Amount").val("");
            $("#Claims_" + id + "_Amount").focus();
            return;
        }
    }
    tval = $("#OriginalAmount_" + id).val();
    if (tval != null && tval != "" && !isNaN(tval)) {
        oldAmount = parseFloat(tval);
    }
    tval = $("#Amount").val();
    if (tval != null && tval != "" && !isNaN(tval)) {
        total = parseFloat(tval);
    }

    tval = $("#ClientTotal_"+client).html();
    if (tval != null && tval != "" && !isNaN(tval)) {
        clientAmount = parseFloat(tval);
    }

    total = total - oldAmount + newAmount;
    clientAmount = clientAmount - oldAmount + newAmount;
    $("#Amount").val(total);
    $("#OriginalAmount_" + id).val(newAmount);
    $("#AmountReceived").html(total.toFixed(2));
    $("#ClientTotal_" + client).html(clientAmount.toFixed(2));
    if(newAmount>0)
    {
        $('#Denied_'+id).removeAttr('checked');
    }
}

function ToggleClient(id)
{
    $("#ClientDetails_" + id).toggle();
}

function Denied(id,client)
{
    if ($('#Denied_' + id).is(':checked'))
    {
        $("#Claims_" + id + "_Amount").val("");
        $("#Claims_" + id + "_Amount").prop("disabled",true);
        $("#Claims_" + id + "_Comment").focus();
        if (client==curClient)
        {
            $("#Claims_" + id + "_Comment").val(lastComment);
        }
        else
        {
            curClient = client;
            lastComment = "";
        }
    }
    else
    {
        $("#Claims_" + id + "_Amount").prop("disabled", false);
        $("#Claims_" + id + "_Comment").removeClass("alert-danger");
    }
    AmountChanged(id, client);
}

$(document).ready(function () {

    $(".DenyCheck:checked").each(function () {
        var idCheck = this.id;
        var nbr = idCheck.split("_")[1];
        $("#Claims_" + nbr + "_Comment").addClass("alert-danger");
    });
});

function ChangeAction(id) {

    var sv = $('#Action_' + id).val();
    var client = $('#Action_' + id).attr("tcclient");

    if (sv == "") {
        $("#Claims_" + id + "_Amount").val("");
        $("#Claims_" + id + "_Comment").val("");
        $("#Claims_" + id + "_Comment").prop("disabled", true);
        $("#Claims_" + id + "_Amount").hide();
        $("#Claims_" + id + "_DRSel").hide();
        $("#Claims_" + id + "_DRSel").val("");
        $("#Claims_" + id + "_Amount").prop("disabled", true);
        $("#Claims_" + id + "_DenialReason_UniqueId").val("N/A");
        $("#Claims_" + id + "_DenialReason_Name").val("");
        $("#Claims_" + id + "_Comment").removeClass("alert-danger");
        $("#Claims_" + id + "_Amount").removeClass("alert-danger");
        $("#Claims_" + id + "_DRSel").removeClass("alert-danger");

    }
    else if (sv == "Pay") {
        $("#Claims_" + id + "_Comment").prop("disabled", false);
        $("#Claims_" + id + "_Amount").show();
        $("#Claims_" + id + "_DRSel").hide();
        $("#Claims_" + id + "_Amount").prop("disabled", false);
        $("#Claims_" + id + "_DenialReason_UniqueId").val("N/A");
        $("#Claims_" + id + "_DenialReason_Name").val("");
        $("#Claims_" + id + "_Amount").focus();
        $("#Claims_" + id + "_Comment").removeClass("alert-danger");
        $("#Claims_" + id + "_DRSel").removeClass("alert-danger");

        $("#Claims_" + id + "_DRSel").val("");
    }
    else if (sv == "Deny") {
        $("#Claims_" + id + "_Amount").val("");
        $("#Claims_" + id + "_Comment").prop("disabled", false);
        $("#Claims_" + id + "_Amount").hide();
        $("#Claims_" + id + "_DRSel").show();
        $("#Claims_" + id + "_Amount").prop("disabled", true);
        $("#Claims_" + id + "_DRSel").focus();
        $("#Claims_" + id + "_Amount").removeClass("alert-danger");
        $("#Claims_" + id + "_DRSel").addClass("alert-danger");
     
    }
    else if (sv == "Issue") {
        $("#Claims_" + id + "_Amount").val("");
        $("#Claims_" + id + "_Comment").prop("disabled", false);
        $("#Claims_" + id + "_Amount").hide();
        $("#Claims_" + id + "_DRSel").hide();
        $("#Claims_" + id + "_Amount").prop("disabled", true);
        $("#Claims_" + id + "_DenialReason_UniqueId").val("N/A");
        $("#Claims_" + id + "_DenialReason_Name").val("");
        $("#Claims_" + id + "_Amount").removeClass("alert-danger");
        if (client == curClient) {
            $("#Claims_" + id + "_Comment").val(lastComment);
        }
        else {
            curClient = client;
            lastComment = "";
        }
        $("#Claims_" + id + "_Comment").focus();
        $("#Claims_" + id + "_DRSel").removeClass("alert-danger");

        $("#Claims_" + id + "_DRSel").val("");
    }

    AmountChanged(id, client);
}

function InitiateActions() {
    var addr = window.location.origin + "/claim/denialreasons";

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        success: function (data) {
            CompleteDRSetup(data);
        }
    });

    $(".SelAction").each(function () {
        var idCheck = this.id;
        var nbr = idCheck.split("_")[1];
        ChangeAction(nbr);

    });
}

function InsCommentFocusLost(id) {
    var sv = $('#Action_' + id).val();

    if (sv=="Deny" || sv=="Issue") {
        lastComment = $("#Claims_" + id + "_Comment").val();
        var cmt = $("#Claims_" + id + "_Comment").val();
        if (cmt == null || cmt == "" || cmt.match(/^ *$/) !== null) {
            try {
                if (sv == "Issue") {
                    $("#Claims_" + id + "_Comment").addClass("alert-danger");
                }
            }
            catch (err) {
                alert(err.message);
            }
        }
        else {
            $("#Claims_" + id + "_Comment").removeClass("alert-danger");
        }
    }
    else {

        $("#Claims_" + id + "_Comment").removeClass("alert-danger");
    }

}


function CompleteDRSetup(returndata) {
    if (returndata.IsFailure) {
        var msgs = ProcessResponseBase(returndata);
        msgs.DisplayResponseResults();
        return;
    }
    try {
        for (var x = 0; x < returndata.EntityList.length; x++) {
            var r = returndata.EntityList[x];
            denialreasons.push({
                value: r.UniqueId,
                label: r.UniqueId + ": " + r.Name
            });
        }
     
        $(".DRSelector").each(function () {
            var idCheck = this.id;
            var nbr = idCheck.split("_")[1];
            $("#"+idCheck).autocomplete({
                minlength: 0,
                source: denialreasons,
                select: function (event, ui) {
                    $("#"+idCheck).val(ui.item.label);
                    $("#Claims_" + nbr+ "_DenialReason_Name").val(ui.item.label);
                    $("#Claims_" + nbr + "_DenialReason_UniqueId").val(ui.item.value);
                    $("#" + idCheck).removeClass("alert-danger");


                    return false;
                },
                change: function (event, ui) {
                    if (ui.item == null) {
                        $("#" + idCheck).addClass("alert-danger");
                        $("#Claims_" + nbr + "_DenialReason_Name").val("");
                        $("#Claims_" + nbr + "_DenialReason_UniqueId").val("N/A");
                    }


                    return false;
                }
            });

        });
    }
    catch (ex) {
        var i = 1;
    }
}

function CommentFocusLost(id) {
    if ($('#Denied_' + id).is(':checked')) {
        lastComment = $("#Claims_" + id + "_Comment").val();
        var cmt = $("#Claims_" + id + "_Comment").val();
        if (cmt==null || cmt == "" || cmt.match(/^ *$/) !== null)
        {
            try
            {
                $("#Claims_" + id + "_Comment").addClass("alert-danger");
            }
            catch (err)
            {
                alert(err.message);
            }
        }
        else
        {
            $("#Claims_" + id + "_Comment").removeClass("alert-danger");
        }
    }
    else {

        $("#Claims_" + id + "_Comment").removeClass("alert-danger");
    }
 
}