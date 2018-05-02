// Write your JavaScript code.
$(function () {
    $('#CreateNewUser').on('click', function (event) {
        event.preventDefault();
        event.stopPropagation();

        $.ajax({
            url: '/User/CreateUser',
            method: 'GET',
            success: function (data) {
                $(data).modal('show');
            }
        });
    });

    $('#CreateNewGroup').on('click', function (event) {
        event.preventDefault();
        event.stopPropagation();

        $.ajax({
            url: '/Group/CreateGroup',
            method: 'GET',
            success: function (data) {
                $(data).modal('show');
            }
        });
    });

    $('#AddNewMember, #AddNewOwner').on('click', function (event) {
        event.preventDefault();
        event.stopPropagation();

        $.ajax({
            url: $(event.currentTarget).attr('href'),
            method: 'GET',
            success: function (data) {
                $(data).modal('show');
            }
        });
    });
})