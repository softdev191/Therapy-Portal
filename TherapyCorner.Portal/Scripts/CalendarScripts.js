
var sourceCalendar;

function InitializeCalendar(cw, source, listType)
{
    sourceCalendar = source;
    $("#calendar").fullCalendar({
        header: {
            left: 'prev,next today',
            center: 'title',
            right: 'month,agendaWeek,' + listType
        },
        defaultView: cw,
        eventLimit: true,
        eventLimitClick: "agendaDay",
        editable: false,
        events: source,
        eventDrop: function (event, delta, revertFunc)
        {
            EditEvent(event, delta, revertFunc);
        },
        allDaySlot: false,
        timezone: 'UTC'
    });
}

function ToggleCancelled()
{

    $("#calendar").fullCalendar('removeEventSources');
    if ($('#chkCancelled:checkbox:checked').length > 0) {
        $("#calendar").fullCalendar('addEventSource', sourceCalendar + "?includeCancelled=true");
    }
    else
    {
        $("#calendar").fullCalendar('addEventSource', sourceCalendar);

    }

}
function InitializeHomeCalendar(source) {
    $("#calendar").fullCalendar({
        header: {
            left: '',
            center: 'title',
            right: ''
        },
        defaultView: 'agendaWeek',
        eventLimit: true,
        eventLimitClick: "agendaDay",
        editable: false,
        events: source,
        eventDrop: function (event, delta, revertFunc) {
            EditEvent(event, delta, revertFunc);
        },
        allDaySlot: false,
        timezone: 'UTC',
        scrollTime: '08:00:00',
        contentHeight:500
    });
}

function EditEvent(event, delta, revertFunc)
{
    var minutes = delta / 1000;
    submitit();
    if (event.isAppointment)
    {
        window.location = window.location.origin + "/appointment/dragmove/" + event.id + "?minutes=" + minutes;

    }
    else
    {
        
        window.location = window.location.origin + "/appointment/editunavailable/" + event.id + "?minutes=" + minutes + "&dy=" + event.start.add(-1*minutes,"s").format();
    }
}

