var questionList;
var questions;


$(document).ready(function () {
    var addr = window.location.origin + "/question/list";

    $.ajax({
        url: addr,
        dataType: "json",
        success: function (data) {
            CompleteListPull(data);
        }
    });
    }
        );

function CompleteListPull(returndata) {
    if (returndata.IsFailure) {
        var msgs = ProcessResponseBase(returndata);
        msgs.DisplayResponseResults();
        return;
    }
    var eList = $("#ExistingQuestions").val();
    var existing = [];
    if (eList != null && eList != "")
    {
        existing = eList.split(",");
    }
    questionList = [];
    questions = returndata.Questions;
    if (questions != null && questions.length > 0) {
        $.each(questions, function (index, item) {
            var a = existing.indexOf(item.QuestionId);
            if (a == -1) {
                questionList.push({ value: item.Label, QuestionId: item.QuestionId });
            }
        });
    }
    
    lstClient = $("#NewQuestion");
    lstClient.autocomplete({
        source: questionList,
        minLength: 0,
        select: function (event, ui) {
            QuestionSelected(event, ui);
        },
        change: function(event,ui)
        {
            $("#NewQuestion").val("");
        }

    });


}

function QuestionSelected(event, ui) {
    if (ui.item != null) {
        var selectedId=ui.item.QuestionId;
        var i = FindQuestion(questionList, selectedId);
        if (i!=-1)
        {
            questionList.splice(i,1);
        }
        i = FindQuestion(questions, selectedId);
        if (i!=-1)
        {
            var q = questions[i];
            var prefix = $("#QuestionPrefix").val();

            var curIndex = parseInt($("#NextQuestionIndex").val());
            var nbr = curIndex + 1;
            var contents = [];
            contents.push("<tr><td>" + q.Label);
            contents.push('<input type="hidden" name="' + prefix + '.Index" value="' + curIndex + '" />');
            contents.push('<input type="hidden" name="' + prefix + '[' + curIndex + ']' + '.QuestionId" value="' + q.QuestionId + '" />');
            contents.push('<input type="hidden" name="' + prefix + '[' + curIndex + ']' + '.Label" value="' + q.Label + '" />');
            contents.push('<input type="hidden" name="' + prefix + '[' + curIndex + ']' + '.Type" value="' + q.Type + '" />');
            contents.push("</td>");
            contents.push('<td class="IdText"><input type="number" class="form-control OrderNumberEntry" min="1" max="1000" name="' + prefix + '[' + curIndex + ']' + '.Order" value="' + nbr + '"   /></td>');
            contents.push('<td class="BoolText"><input type="checkbox" name="' + prefix + '[' + curIndex + ']' + '.Required" value="true"  /></td>');
            contents.push('<td class="BoolText"><input type="checkbox" name="' + prefix + '[' + curIndex + ']' + '.Prepopulate"  value="true"  /></td>');
            contents.push("</tr>");

            $("#QuestionList").append(contents.join(""));
 
            $("#NextQuestionIndex").val(nbr);

          
        }

    }
            return false;
}

function FindQuestion(q, id)
{
    var i = -1;

    for (x = 0; x < q.length;x++)
    {
        if (q[x].QuestionId==id)
        {
            i = x;
            break;
        }
    }

    return i;
}