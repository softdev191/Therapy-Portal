var credentialsSource;
var credentialTable;
var imgView;
var viewSource;
var ignoreSource;
var watchSource;
var ignore;
var watch;
var showName;
var verifySource;
var msgSource
var verifyicon;
var msgicon;
var puVerifyCred;
var pendingCredentialId;
var puCredMsg;
var puIgnoreCred;
var puWatchCred;
function InitializeCredentials(listSource, docSource, magnify, ignoreUrl, watchUrl, ignoreText, watchText, includeName, verifyUrl, msgUrl, verifyImg, msgImg, editUrl, editImg, isAdmn )
{
    credentialsSource = listSource;
    credentialTable = $("#tbCredentials");
    imgView = magnify;
    viewSource = docSource;
    ignoreSource = ignoreUrl;
    watchSource = watchUrl;
    ignore = ignoreText;
    watch = watchText;
    showName = includeName;
    verifySource = verifyUrl;
    msgSource = msgUrl;
    verifyicon = verifyImg;
    msgicon = msgImg;
    editSource = editUrl;
    editIcon = editImg;
    isAdmin = isAdmn;
    FetchCredentials();

    puVerifyCred = $("#modalVerifyCred").dialog({
        autoOpen: false,
        width: 365,
        modal: true,
        buttons: { "Yes": function () { ConfirmVerify(); }, "No": function () { puVerifyCred.dialog("close"); } }
    });

    puIgnoreCred = $("#modalIgnoreCred").dialog({
        autoOpen: false,
        width: 365,
        modal: true,
        buttons: { "Yes": function () { ConfirmIgnore(); }, "No": function () { puIgnoreCred.dialog("close"); } }
    });

    puWatchCred = $("#modalWatchCred").dialog({
        autoOpen: false,
        width: 365,
        modal: true,
        buttons: { "Yes": function () { ConfirmWatch(); }, "No": function () { puWatchCred.dialog("close"); } }
    });

    puCredMsg = $("#modalCredMsg").dialog({
        autoOpen: false,
        width: 365,
        modal: true,
        buttons: { "Send": function () { ConfirmCredMsg(); }, "Cancel": function () { puCredMsg.dialog("close"); } }
    });
}

function FetchCredentials()
{
    TurnFreezePaneOn();
    $.ajax({
        url: credentialsSource,
        type: "GET",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (returndata) {
            FinishFetch(returndata);
        }
    });
}

function FinishFetch(returndata) {
    TurnFreezePaneOff();
    if (returndata.IsFailure) {
        var msgs = ProcessResponseBase(returndata);
        msgs.DisplayResponseResults();
    }

    var creds = [];

    if (returndata.Credentials != null && returndata.Credentials.length > 0) {
        var curDate = new Date();
        $.each(returndata.Credentials, function (index, itm) {
            var row = "<tr><td><div class='td2Button'>"
            if(itm.ImageExt!=null && itm.ImageExt!="")
            {
                row+= "<a target='_blank' href='"+ viewSource + "/" + itm.CredentialId +"'><img src='" + imgView + "'/></a>";
            }
            if (isAdmin) {
                row += "<a href ='" + editSource + "/" + itm.CredentialId + "'><img src='" + editIcon + "' /></a >";
            }
            row += "</div></td>";
            if (showName) {
                row+="<td><div class='NameText'>" + itm.User.FirstName + " " + itm.User.LastName + "</div></td>";
            }
                row += "<td><div class='NameText'>" + itm.Type.Name + "</div></td>"

                row += "<td  class='HideSmall'><div class='IdText'>" + itm.DocumentId || "" + "</div></td>"
            var dt = ReadJSONDate(itm.ValidFrom);
            row += "<td  class='HideMedium'><div class='DateText'>" + dt.toLocaleDateString() + "</div></td>"
            var todt = ReadJSONDate(itm.ValidTo);
            row += "<td  class='HideMobile'><div class='DateText'>" + todt.toLocaleDateString() + "</div></td>"
            var verification = itm.Validations[0];
            row += "<td class='HideMedium'><div class='IdText'>";
            var canVerify = todt > curDate;
            var fvars = "\"" + (itm.DocumentId || "").replace("'", "_") + "\"," + itm.CredentialId + ",\"" + todt.toLocaleDateString() + "\",\"" + itm.Type.Name.replace("'", "_") + "\", \"" + itm.User.FirstName.replace("'", "_") + " " + itm.User.LastName.replace("'", "_") + "\""

            if (verification.VerifiedBy)
            {
                row += verification.VerifiedBy.FirstName + " " + verification.VerifiedBy.LastName;
            }
            else if (verification.Ignored && canVerify)
            {
                row += "<a href='javascript:WatchCred(" + fvars + ")'>" + watch + "</a>"
            }
            else if (canVerify)
            {
                row += "<a href='javascript:IgnoreCred(" + fvars + ")'>" + ignore + "</a>"

            }
            row += "</div></td><td><div class='DateText'>"
            if (verification.VerifiedBy)
            {
                dt = ReadJSONUTCDate(verification.VerifiedAt);
                row += dt.toLocaleString() ;
            }
            else if (canVerify)
            {
                row += "<a  href='javascript:VerifyCredential("+ fvars +")'><img src='" + verifyicon + "'/></a>";
                row += "&nbsp;<a  href='javascript:MsgCredential(" + fvars +  ")'><img src='" + msgicon + "'/></a>";

            }
            creds.push(row);

        });
   
    }

    credentialTable.html(creds.join(''));

}

function ConfirmIgnore()
{
    puIgnoreCred.dialog("close");
    TurnFreezePaneOn();
    $.ajax({
        url: ignoreSource + "/" + pendingCredentialId,
        type: "GET",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (returndata) {
            FinishCredAction(returndata);
        }
    });
}

function ConfirmWatch() {
    puWatchCred.dialog("close");
    TurnFreezePaneOn();
    $.ajax({
        url: watchSource + "/" + pendingCredentialId,
        type: "GET",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (returndata) {
            FinishCredAction(returndata);
        }
    });

}
function FinishCredAction(returndata) {
    if (returndata.IsFailure) {
        var msgs = ProcessResponseBase(returndata);
        msgs.DisplayResponseResults();
    }
    FetchCredentials();
}

function VerifyCredential(docId, id,validTo,typeName,staffName)
{
    pendingCredentialId = id;
    $("#verifyCredType").text(typeName.replace("_", "'"));
    $("#verifyCredValid").text(validTo.replace("_", "'"));
    $("#verifyCredId").text(docId.replace("_", "'"));
    $("#verifyCredName").text(staffName.replace("_", "'"));
    puVerifyCred.dialog("open");
}

function WatchCred(docId, id, validTo, typeName, staffName) {
    pendingCredentialId = id;
    $("#watchCredType").text(typeName.replace("_", "'"));
    $("#watchCredValid").text(validTo.replace("_", "'"));
    $("#watchCredId").text(docId.replace("_", "'"));
    $("#watchCredName").text(staffName.replace("_", "'"));
    puWatchCred.dialog("open");
}

function IgnoreCred(docId, id, validTo, typeName, staffName) {
    pendingCredentialId = id;
    $("#ignoreCredType").text(typeName.replace("_", "'"));
    $("#ignoreCredValid").text(validTo.replace("_", "'"));
    $("#ignoreCredId").text(docId.replace("_", "'"));
    $("#ignoreCredName").text(staffName.replace("_", "'"));
    puIgnoreCred.dialog("open");
}

function ConfirmVerify() {
    TurnFreezePaneOn();
    puVerifyCred.dialog("close");
    $.ajax({
        url: verifySource + "/" + pendingCredentialId,
        type: "GET",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (returndata) {
            FinishCredAction(returndata);
        }
    });

}

function MsgCredential(docId, id, validTo, typeName, staffName) {
    pendingCredentialId = id;
    $("#msgCredType").text(typeName.replace("_", "'"));
    $("#msgCredValid").text(validTo.replace("_", "'"));
    $("#msgCredId").text(docId.replace("_", "'"));
    $("#msgCredName").text(staffName.replace("_", "'"));
    $("#CredentialMsg").val("");
    puCredMsg.dialog("open");
}

function ConfirmCredMsg()
{
    TurnFreezePaneOn();
    puCredMsg.dialog("close");

    var info = {
        "message": $("#CredentialMsg").val()
    };
    $.ajax({
        url: msgSource + "/" + pendingCredentialId ,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(info),
        success: function (returndata) {
            FinishSendCrdMsg(returndata);
        }
    });
}

function FinishSendCrdMsg(returndata) {
    TurnFreezePaneOff();


    if (returndata.IsFailure) {
               var msgs = ProcessResponseBase(returndata);
        msgs.DisplayResponseResults();
    }
    else
    {
        $("#SystemMessages").append(CreateSuccessMessage("Message sent"));
    }
}