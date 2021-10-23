$(document).ready(function () {
    var addr = window.location.origin + "/reference/diagnosis";

    lstClient = $("#NewDiagnoses");
    lstClient.autocomplete({
        source: function (request, response) {
            $.ajax({
                url: addr,
                dataType: "json",
                data: {
                    name: request.term
                },
                success: function (data) {
                    CompleteDiagnosisSearch(data, response);
                }
            });
        },
        minLength: 3,
        change: function (event, ui) {
            DiagnosisSelected(event, ui);
        }
    }
        );
});

var filteredDiagnosis;
var selDiagnosis = null;
var selDiagItem;
function RemoveDiagnosis(index)
{
    $("#DiagnosisEntry" + index).remove();

    if(index==$("DiagnosisCount").val())
    {
        $("DiagnosisCount").val(index-1);
    }
}

function AddDiagnosis()
{
    if (selDiagnosis == null) return;
    var curIndex = parseInt($("#DiagnosisCount").val());
    var contents = [];
    var addr = window.location.origin + "/images/delete.png";
    contents.push("<div id='DiagnosisEntry" + curIndex + "'><a href='javascript:RemoveDiagnosis(" + curIndex + ")'><img  class='DiagBtn'  src='" + addr + "'/></a>");
    contents.push(selDiagnosis.Name + " (" + selDiagnosis.UniqueId + ")");
    contents.push('<input type="hidden" name="Diagnosis.Index" value="' + curIndex + '"/>');
    contents.push('<input type="hidden" name="Diagnosis[' + curIndex + '].UniqueId" value="' + selDiagnosis.UniqueId + '"/>');
    contents.push('<input type="hidden" name="Diagnosis[' + curIndex + '].Context" value="Diagnosis"/>');
    contents.push('<input type="hidden" name="Diagnosis[' + curIndex + '].Name" value="' + selDiagnosis.Name + '"/></div>');

    $(".DiagnosisList").append(contents.join(""));
    curIndex++;
    $("#DiagnosisCount").val(curIndex)
}

function CompleteDiagnosisSearch(returndata, response) {
    if (returndata.IsFailure) {
        var msgs = ProcessResponseBase(returndata);
        msgs.DisplayResponseResults();
        return;
    }
    filteredDiagnosis = returndata.EntityList;
    var data = [];
    if (filteredDiagnosis != null && filteredDiagnosis.length > 0) {
        $.each(filteredDiagnosis, function (index, item) {
            var lbl = item.Name + " (" + item.UniqueId + ")";
            data.push({ label: lbl, value: lbl, uid:item.UniqueId });
        });
    }
    response(data);
}

function DiagnosisSelected(event, ui) {
 
    if (filteredDiagnosis != null && filteredDiagnosis.length > 0 && ui.item!=null) {
        $.each(filteredDiagnosis, function (index, item) {
            if (item.UniqueId == ui.item.uid) {
                selDiagnosis = item;
            }
        });
    }

    if (selDiagnosis == null) {
        
        $("#NewDiagnoses").val("");
    }
}