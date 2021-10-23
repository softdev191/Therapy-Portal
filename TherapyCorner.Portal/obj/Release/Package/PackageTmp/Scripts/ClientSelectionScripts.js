var lstClient;
var clientPrefix;
var filteredClients;
var selFunction=null;
var haveSelFunction = false;
function InitializeClientWithEvent(controlId, prefix,selectionFunction) {
    InitializeClient(controlId, prefix);
    selFunction = selectionFunction;
    haveSelFunction = true;
}

function InitializeClient(controlId, prefix)
{
    clientPrefix = prefix;
    var addr = window.location.origin + "/client/clientlist";

    lstClient = $("#" + controlId);
    lstClient.autocomplete({
        source: function (request, response) {
            $.ajax({
            url: addr,
            dataType: "json",
            data: {
                name: request.term
            },
            success: function( data ) {
                CompleteClientSearch(data,  response );
            }
        } );
},
minLength: 3,
    change: function( event, ui ) {
        ClientSelected(event,ui);
    },
    select: function (event, ui) {
        ClientSelected(event, ui);
    }
    }
        );
}

function CompleteClientSearch(returndata, response)
{
    if (returndata.IsFailure) {
        var msgs = ProcessResponseBase(returndata);
        msgs.DisplayResponseResults();
        return;
    }
    filteredClients = returndata.PersonList;
    var data = [];
    if(filteredClients!=null && filteredClients.length>0)
    {
        $.each(filteredClients, function (index, item) {
            var nme = item.LastFirstMiddle;
              item.label = nme;
            item.value = nme;
            data.push(item);
        });
    }
    response(data);
}
function ClientSelected(event,ui)
{
    var selValue = ui.item;
   

    $("#" + clientPrefix + "Container").html("");
  if (selValue != null)
  {
      var fields = [];
    
      fields.push("<input type='hidden' name='" + clientPrefix + ".UniqueId' id='" + clientPrefix + "_UniqueId' value='" + selValue.UniqueId + "'/>");
      fields.push("<input type='hidden' name='" + clientPrefix + ".Context' value='Client'/>");
      fields.push('<input type="hidden" name="' + clientPrefix + '.FirstName" value="' + selValue.FirstName + '"/>');
      fields.push('<input type="hidden" name="' + clientPrefix + '.LastName" value="' + selValue.LastName + '"/>');
      fields.push('<input type="hidden" name="' + clientPrefix + '.MiddleName" value="' + (selValue.MiddleName || "") + '"/>');
      fields.push('<input type="hidden" name="' + clientPrefix + '.Suffix" value="' + (selValue.Suffix || "") + '"/>');

      $("#" + clientPrefix + "Container").html(fields.join(""));

  }
    else
  {
      lstClient.val("");
  }
  if (haveSelFunction) {
      selFunction();
  };
}