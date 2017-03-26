$(document).ready(function () {
    $("#add-user").click(function () {
        var selectedOption = $("#user-select").find(":selected");
        var id = selectedOption.val();
        var name = selectedOption.text();
        $('#hidden-ids').val($('#hidden-ids').val() + id + "|");
        $("#currently-added").append(name).append("<br/>");
    });

    $(".btn-details").html("<i class='fa fa-search'></i>");
    $(".btn-edit").html("<i class='fa fa-pencil'></i>");
    $(".btn-cancel").html("<i class='fa fa-times'></i>");
})