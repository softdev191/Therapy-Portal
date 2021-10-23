$(document).ready(function () {
    TypeChanged();
});


function TypeChanged()
{
    var tpe = $("#Type").val();

    switch(tpe)
    {
        case "Integer":
            case "Decimal":
            $("#grpValidation").hide();
            $("#grpMin").show();
            $("#grpMax").show();
            $("#Validation").val("");
            break;
        default:
            $("#grpValidation").hide();
            $("#grpMin").hide();
            $("#grpMax").hide();
            $("#Validation").val("");
            $("#Min").val("");
            $("#Max").val("");
            break;

    }
}