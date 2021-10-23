var services = null;
var selectedService = null;
var staffId = 0;
var updatingAvail = false;
var muStaffId = 0;
var muClientId = 0;
var muDuration = 0;
var isSeries = false;

$(document).ready(function () {
    window.setTimeout(function () { EnableTimeDropdown() }, 250);
    $("#availcalendar").fullCalendar({
        header: false,
        defaultView: 'agendaWeek',
        eventLimit: false,
        editable: false,
        allDaySlot: false,
        timezone: 'UTC',
        scrollTime: '08:00:00',
        contentHeight: 300
    });
 
});

function InitializeSeries(duration)
{
    isSeries = true;
    muDuration = duration;
    SetMakeupEndTime();
}


function EnableTimeDropdown()
{
    if (isSeries) {
        $("#StartDT").timepicker({
            timeFormat: 'h:mm p',
            interval: 15,
            dynamic: true,
            dropdown: true,
            scrollbar: true,
            change: function (time) {
                SetMakeupEndTime();
            }
        });
    }
    else
    {
        $("#StartDT").timepicker({
            timeFormat: 'h:mm p',
            interval: 15,
            dynamic: true,
            dropdown: true,
            scrollbar: true,
            change: function (time) {
                SetEndTime();
            }
        });
    }
   
    $("#MeetingCount").on("spin", function (event, ui) { UpdateAvailability(); });
}
function ClientSelected()
{
    var clientId = $("#ClientId").val();

    $("#btnSave")[0].disabled = true;
    $("#ClientServiceId").html('');

    if(clientId ==null || clientId=="")
    {
        return;
    }

    FetchServices();

}

function FetchServices()
{
    var clientId = $("#ClientId").val();

    TurnFreezePaneOn();
    var addr = window.location.origin + "/clientservice/NeedsScheduling/" + clientId;

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        success: function (data) {
            CompleteFetchServices(data);
        }
    });
}

function InitializeMakeup(staff,client,duration)
{
    isSeries = true;
    muStaffId = staff;
    muClientId = client;
    muDuration = duration;
    UpdateMakeupAvailability();
    SetMakeupEndTime();
}



function CompleteFetchServices(returndata) {
    TurnFreezePaneOff();
    if (returndata.IsFailure) {
        var msgs = ProcessResponseBase(returndata);
        msgs.DisplayResponseResults();
        $("#btnSave")[0].disabled = true;
        $("#ClientServiceId").html('');
        SetServiceInfo(null);
        return;
    }
    var options = [];
    var svcId = $("#ServiceId").val();
    services = returndata.Services;
    var selectedService = null;
    if (returndata.Services == null || returndata.Services.length == 0)
    {
        $("#btnSave")[0].disabled = true;
        SetServiceInfo(null);
    }
    else
    {
        if (svcId==null || svcId =="") 
        {
            svcId = services[0].Id;
        }
        $.each(services, function (index, item) {
            var optsel ="";
            if  (svcId==item.Id)
            {
                optsel = "selected";
                selectedService = item;
            }
            options.push("<option value='" + item.Id + "' " + optsel + ">"+ item.Service.Name + "</option>");
        });
        $("#ClientServiceId").html(options.join(''));
        $("#btnSave")[0].disabled = false;
        if (selectedService==null)
        {
            selectedService = services[0];
        }
    }
   
    SetServiceInfo(selectedService);
}

function SetServiceInfo(svc)
{
    if (svc==null)
    {
        selectedService = null;
        $("#spFreqDur").text("");
        $("#spLocation").text("");
        $("#spProvider").text("");
        $("#spAllowed").text("");
        $("#spScheduled").text("");
    }
    else
    {
        selectedService = svc;
        var remaining = svc.AllowedCount - svc.ScheduledCount;
        if (remaining > 52) remaining = 52;

        $("#MeetingCount").val(remaining)
        $("#MeetingCount").spinner({
            min: 1,
            max: remaining,
            step: 1
        });

        SetEndTime();
        SetLastDate();
        $("#spFreqDur").text(svc.Duration.Name);
        $("#spLocation").text(LocationName(svc.Location));
        $("#spProvider").text(svc.Provider.Name);
        $("#spAllowed").text(svc.AllowedCount);

        $("#spScheduled").text(svc.ScheduledCount);
        if (svc.LastAppointment != null)
        {
            var dt = ReadJSONDate(svc.LastAppointment);
            $("#spLastDate").text(dt.toLocaleDateString());
        }
        else
        {
            $("#spLastDate").text("N/A");
        }

        var mindt = ReadJSONDate(svc.Start);
        $("#InitialDate").datepicker("option", "minDate",mindt);
        UpdateAvailability();
    }
}


function ServiceSelected()
{
    var id = $("#ClientServiceId").val();
    $.each(services, function (index, item) {
        if (item.Id == id)
        {
            SetServiceInfo(item);
        }
    });
}

function UpdateMakeupAvailability() {
    if (updatingAvail) return;
    updatingAvail = true;
   
    var dt = moment($("#InitialDate").val());
    if (dt == null || !dt._isValid) {
        dt = moment();
        $("#InitialDate").val(dt.toDate().toLocaleDateString())
    }

    TurnFreezePaneOn();
    var addr = window.location.origin + "/appointment/availability/" + muClientId;

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        data: {
            staff: muStaffId,
            count: 1,
            start: dt.toISOString()
        },
        success: function (data) {
            CompleteAvailability(data);
        }
    });
    $('#availcalendar').fullCalendar('gotoDate', dt);

}

function UpdateAvailability()
{
    if (updatingAvail) return;
    updatingAvail = true;
    var clientId = $("#ClientId").val();

    if (selectedService == null) return;
    var count = parseInt($("#MeetingCount").val());
    if (isNaN(count) || count<1) {
        $("#MeetingCount").val("1");
        count = 1;
    }
    var dt = moment($("#InitialDate").val());
    if (dt == null || !dt._isValid) {
        dt = moment();
        $("#InitialDate").val(dt.toDate().toLocaleDateString())
    }

    TurnFreezePaneOn();
    var addr = window.location.origin + "/appointment/availability/" + clientId;

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        data: {
            staff: selectedService.Provider.UniqueId,
            count: count,
            start: dt.toISOString()
        },
        success: function (data) {
            CompleteAvailability(data);
        }
    });
    $('#availcalendar').fullCalendar('gotoDate', dt);

}

function SetAvailability(clientId, staff, count, minDate)
{


    if (count < 1) count = 1;

    var dt = moment(minDate);
    
    TurnFreezePaneOn();
    var addr = window.location.origin + "/appointment/availability/" + clientId;

    $.ajax({
        url: addr,
        type: "POST",
        dataType: "json",
        data: {
            staff: staff,
            count: count,
            start: dt.toISOString()
        },
        success: function (data) {
            CompleteAvailability(data);
        }
    });
    $('#availcalendar').fullCalendar('gotoDate', dt);
}
function SetLastDate()
{
    if (selectedService==null) return;

    var dt = moment($("#InitialDate").val());
    if (dt == null || !dt._isValid)
    {
        dt = moment();
        $("#InitialDate").val(dt.toDate().toLocaleDateString())
    }
    var count = parseInt($("#MeetingCount").val());
    if (isNaN(count) || count<1) {
        $("#MeetingCount").val("1");
        count = 1;
    }
    if (count > selectedService.AllowedCount - selectedService.ScheduledCount)
    {
        count = selectedService.AllowedCount - selectedService.ScheduledCount;
        $("#MeetingCount").val(selectedService.AllowedCount - selectedService.ScheduledCount);
    }
    var end = dt.add(count-1, "w");
    $("#spThroughText").text(dt.format("dddd") + "s through " + end.toDate().toLocaleDateString());
    UpdateAvailability();

    }
function SetEndTime()
{
    var dt = moment($("#StartDT").val(), "hh:mm a");
    if (dt==null || !dt._isValid)
    {
        dt = moment("8:00 am", "hh:mm a");
        $("#StartDT").val(dt.toDate().toLocaleTimeString())

    }
    var end = dt.add(selectedService.DurationTime, "m");
    $("#spEndTime").text(end.toDate().toLocaleTimeString())
}

function SetMakeupEndTime() {
    var dt = moment($("#StartDT").val(), "hh:mm a");
    if (dt == null || !dt._isValid) {
        dt = moment("8:00 am", "hh:mm a");
        $("#StartDT").val(dt.toDate().toLocaleTimeString())

    }
    var end = dt.add(muDuration, "m");
    $("#spEndTime").text(end.toDate().toLocaleTimeString())
}

function LocationName(id)
{
    if (id == 0) return "Client Home";
    if (id == 1) return "Provider Home";
    if (id==2) return "Clinic";
}

function CompleteAvailability(returndata) {
    TurnFreezePaneOff();
    updatingAvail = false;
    if (returndata==null || returndata.IsFailure) {
        var msgs = ProcessResponseBase(returndata);
        msgs.DisplayResponseResults();
        return;
    }
   
    $("#availcalendar").fullCalendar('removeEvents', function (event) {
            return true;
    });

    
    var dt = moment($("#InitialDate").val());
    if (dt == null || !dt._isValid)
    {
        dt = moment();
        $("#InitialDate").val(dt.toDate().toLocaleDateString())
    }

    $('#availcalendar').fullCalendar('renderEvents', returndata);
}
