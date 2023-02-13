$(function () {
    $.ajaxSetup({ cache: false });

    $("a[data-modal]").on("click", function (e) {
        // hide dropdown if any (this is used wehen invoking modal from link in bootstrap dropdown )
        //$(e.target).closest('.btn-group').children('.dropdown-toggle').dropdown('toggle');

        $('#myModalContent').load(this.href, function () {
            $('#myModal').modal({
                /*backdrop: 'static',*/
                keyboard: true
            }, 'show');
            bindForm(this);
        });
        return false;
    });
});

function bindForm(dialog) {
    $('form', dialog).submit(function () {
        //console.log("BBBaaaBBBBB");
        $('#progress').show();
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            //success: function(){
            //    $('#myModal').modal('hide');
            //    $('#progress').hide();
            //    location.reload();
            //}
            success: function (result) {
                //console.log("qwdqwdwq");
                if (result.success) {
                    $('#myModal').modal('hide');
                    $('#progress').hide();
                    window.location = result.url;
                    //location.reload(result.urlurlSession);
                } else {
                    $('#progress').hide();
                    $('#myModalContent').html(result);
                    bindForm();
                }
            }
        });
        return false;
    });
}