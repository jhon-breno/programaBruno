function ShowToast(mensagem, tipoAlerta) {
    var cssico;

    switch (tipoAlerta) {
        case 'Sucesso':
            cssico = 'fa-check-circle';
            break;
        case 'Erro':
            cssico = 'fa-times';
            break;
        case 'Alerta':
            cssico = 'fa fa-exclamation-circle';
            break;
        case 'Informacao':
            cssico = 'fa fa-exclamation-circle';
            break;
        case 'RedirectToHome':
            cssico = 'fa fa-exclamation-circle';
            setTimeout(function () { window.location.replace("/"); }, 2500);
            break;
        case 'Bug':
            cssico = 'fa fa-bug';
            break;
        default:
            cssico = '';
    }

    $(".toast-alert").empty();
    $(".toast-alert").append('<div class="row row-margin-top">' +
        '<div class="col-xs-12">' +
        '<div class="alert alert-oh" role="alert">' +
        '<button type="button" class="close" data-dismiss="alert" aria-label="Close">' +
        '<span aria-hidden="true">&times;</span>' +
        '</button>' +
        '<i class="fa ' + cssico + '"></i>' +
        '<div id="pnaviso" name="toast">' + mensagem + '</div>' +
        '</div>' +
        '</div>' +
        '</div>');

    setTimeout(function () { scrollToToast(); }, 1);
    //setTimeout(function () { $(".toast-alert").empty(); }, 60000);
}

function scrollToToast() {
    var aTag = $("div[name='toast']");
    $('html,body').animate({ scrollTop: aTag.offset().top }, 'slow');
}

function toastEmpty() {
    $(".toast-alert").empty();
}