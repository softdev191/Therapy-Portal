var lockOnSubmit = true;
var lockOnUnload = false;
var submitTimeout = null;
var diagMsgBox;
var diagSessionExp;
var loginAddr;
var tcUnloadingWindow = false;

//****************************
//*** Processing Display   ***
//****************************
function setSubmitLocking() {

    lockOnSubmit = true;
}

function clearSubmitLocking() {
    lockOnSubmit = false;
}

function submitit() {
    if (lockOnSubmit) {
        lockOnUnload = true;
        submitTimeout = window.setTimeout("lockOnUnload=false;", 100);

    }
}

function TurnFreezePaneOn() {
    var paneProcessing = $("#ProcessingPane");
    if (paneProcessing.length > 0) {
        paneProcessing[0].className = 'FreezePaneOn';
        var prc = document.getElementById("ProcessingMsg");
        if (prc) {
            prc.style.display = "";
        }
        if (document.documentElement.scrollHeight > document.documentElement.clientHeight) {
            paneProcessing[0].style.height = document.documentElement.scrollHeight;
        }
        else {
            paneProcessing[0].style.height = document.documentElement.clientHeight;
        }
    }
}

function TurnFreezePaneOff() {
    var paneProcessing = $("#ProcessingPane");
    if (paneProcessing.length > 0) {
        paneProcessing[0].className = 'FreezePaneOff';
    }
}

function CompleteSubmitProcessing() {
    if (lockOnUnload) {
        TurnFreezePaneOn();
        window.clearTimeout(submitTimeout);
    }
}
//Session Expiration Pop-up
function SetExpireTime(tm,txtLogin, destLogin) {
    var expTime = new Date(tm);
    var curTime = new Date();
    diagSessionExp = $("#modalSessionExpired").dialog({
        autoOpen: false,
        width: 365,
        modal: true,
        buttons: { "Sign Back In": function () { LogBackIn(); } }
    });
    loginAddr = destLogin;
    window.setTimeout(DisplayExpiration, expTime - curTime);

}

function DisplayExpiration() {
    $(".ui-dialog-content").dialog("close");
    diagSessionExp.dialog("open");
}

function LogBackIn() {
    diagSessionExp.dialog("open");
    submitit();
    window.location = loginAddr;
}
//Set each window to 
$(document).ready(function () {
    setSubmitLocking();
    $(window).bind("beforeunload", function () { tcUnloadingWindow = true });
    $(".DateText:input").datepicker({
        changeMonth: true,
        changeYear: true,
        yearRange:"c-20:c+20",
        dateFormat: "m/d/yy"
    });
    ResizedWindow();
    $(window).resize(function () { ResizedWindow(); });
    $(".sortable").tablesorter();
    $("#tabs").tabs();

   LocalizeDateTimes();
    var dp = $(".DateText:input");
    if (dp!=null && dp.length>0)
    {
        for (var x=0;x<dp.length;x++)
        {
            var dpv = dp[x].value;
            if (dpv!="")
            {
                dp[x].value = dpv.split(" ")[0];
            }
        }

    }

    

    $(".TimeEntry:input").timepicker({
        timeFormat: 'h:mm p',
        interval: 15,
        dynamic: true,
        dropdown: true,
        scrollbar: true

    });
});

function LocalizeDateTimes()
{
    var instances = $(".LocalDT");
    if (instances.length>0)
    {
        for(var x=0;x<instances.length;x++)
        {
            var inner = instances[x].innerHTML;
            if (inner.indexOf("<a ")==-1)
            {
                var orig = instances[x].innerText;
                var dt = new Date(orig);
                instances[x].innerText = dt.toLocaleString();
            }
            
        }
    }
}
function ResizedWindow()
{
    if ($(".UserMenu").length > 0) {
        $(".UserMenu").css({ left: $(".headercontext").offset().left + 15 });
    }
}
function ToggleUserMenu() {
    if ($(".UserName").hasClass("UsrNameOn")) {
        $(".UserName").removeClass("UsrNameOn");
        $(".UserMenu").hide();
    }
    else {
        $(".UserName").addClass("UsrNameOn");
        $(".UserMenu").show();
    }

}

//****************************
//** Button Click Functions **
//****************************
function doEnterClick(fnctn, e) {
    //the purpose of this function is to allow the enter key to 
    //trigger a function call.
    var key;
    if (window.event)
        key = window.event.keyCode;     //IE
    else
        key = e.which;     //firefox
    if (key == 13) {
        fnctn();
        event.keyCode = 0

    }
}

//************************
//* AJAX Processing 
//************************
//Translate json date into a date object
function ReadJSONDate(value) {
    var result = new Date(parseInt(value.substr(6)));

    return result;
}
function ReadJSONUTCDate(value) {
    var result = new Date(parseInt(value.substr(6)));

    return result;
}
//Used to process basic response message
function ProcessResponseBase(rsp) {

    var msgs = new ResponseMessages();
    msgs.GenerateResponseMessages(rsp);
    return msgs;
}



//standardized ajax service call error handling
$(document).ready(function () {
    $.ajaxSetup({
        cache: false,
        error: function (x, e) {
            if (tcUnloadingWindow) return;
            if (x.status == 0) {
                alert('You are offline!!\n Please Check Your Network.');
            } else if (x.status == 404) {
                alert('Requested URL not found.');
            } else if (x.status == 500) {
                alert('Internal Server Error.');
            } else if (e == 'parsererror') {
                alert('Error.\nParsing JSON Request failed.');
            } else if (e == 'timeout') {
                alert('Request Time out.');
            } else {
                alert('Unknown Error.\n' + x.responseText);
            }
            TurnFreezePaneOff();
        }
    });
});



//****************************
//***    MISC Functions    ***
//****************************
function parseDate(str) {
    var mdy = str.split('/')
    return new Date(mdy[2], mdy[0] - 1, mdy[1]);
}

function daydiff(first, second) {
    return (second - first) / (1000 * 60 * 60 * 24)
}

//String.Format function
if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
              ? args[number]
              : match
            ;
        });
    };
}

/* Message Functions */
function ResponseMessages() {
    this.Errors = [];
    this.Warnings = [];
    this.GenerateResponseMessages = function (rsp) {
        var msgs = "";

        if (rsp.ErrorMessages != null) {
            var errs = [];
            $.each(rsp.ErrorMessages, function (ky, msg) { errs.push(msg); });
            this.Errors = errs;
        }
        if (rsp.WarningMessages != null) {
            var warns = [];
            $.each(rsp.WarningMessages, function (ky, msg) { warns.push(msg); });
            this.w = warns;
        }
    };

    this.DisplayResponseResults = function () {
        var results = "";
        $.each(this.Errors, function (indx, msg) {
            results += CreateErrorMessage(msg);
        });
        $.each(this.Warnings, function (indx, msg) {
            results += CreateWarningMessage(msg);
        });
        $("#SystemMessages").append(results);
    }
}

function CreateErrorMessage(msg) {
    return "<div class='alert alert-danger'><a href='#' class='close' data-dismiss='alert' aria-label='close'>&times;</a><span class='glyphicon glyphicon-exclamation-sign'></span>" + msg + "</div>";
}
function CreateWarningMessage(msg) {
    return "<div class='alert alert-warning'><a href='#' class='close' data-dismiss='alert' aria-label='close'>&times;</a><span class='glyphicon glyphicon-warning-sign'></span>"+msg+"</div>";
}
function CreateSuccessMessage(msg) {
    return "<div class='alert alert-success'><a href='#' class='close' data-dismiss='alert' aria-label='close'>&times;</a><span class='glyphicon glyphicon-check'></span>"+msg+"</div>";
}
function CreateInfoMessage(msg) {
    return "<div class='alert alert-info'><a href='#' class='close' data-dismiss='alert' aria-label='close'>&times;</a><span class='glyphicon glyphicon-info-sign'></span>"+msg+"</div>";
}