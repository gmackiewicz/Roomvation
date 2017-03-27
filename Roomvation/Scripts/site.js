$(document).ready(function () {
    $("#add-user").click(function () {
        var selectedOption = $("#user-select").find(":selected");
        var id = selectedOption.val();
        var name = selectedOption.text();
        $('#hidden-ids').val($('#hidden-ids').val() + id + "|");
        $("#currently-added").append(name).append("<br/>");
    });
})