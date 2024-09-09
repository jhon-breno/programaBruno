function configInitForms() {
    $('input[type=text]').each(function () {
        $(this).attr('autocomplete', 'off');
    });

    //ADICIONA BORDA NOS TEXTFIELDS QUE TEM ICONES
    var inputs = document.querySelectorAll('.input-group input.text-field');

    inputs.forEach(function (elem) {
        elem.addEventListener("focus", function () {
            var container = elem.parentNode;
            var addon = container.querySelector('.input-group-addon');

            addon.style.border = '2px solid #000';
            addon.style.borderLeft = '0';
            addon.style.transition = 'all ease .3s';
        });

        elem.addEventListener("blur", function () {
            var container = elem.parentNode;
            var addon = container.querySelector('.input-group-addon');

            addon.style.border = '2px solid rgba(0, 0, 0, 0.15)';
            addon.style.borderLeft = '0';
        });
    });
}

if (typeof (Sys) !== 'undefined') {
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(configInitForms);
}

$(function () {
    configInitForms();
});



function SomenteLetras(e) {
    if (document.all) { var evt = event.keyCode; }
    else { var evt = e.charCode; }
    var chr = String.fromCharCode(evt);
    var re = /[A-Za-z\s-ÃÕÑÁÉÍÓÚÀÜÇãõñáéíóúàçü]/;
    return (re.test(chr) || evt < 20);
}

function SomenteNumeros(event) {
    var charCode = (event.which) ? event.which : event.keyCode;

    if (charCode > 31 && (charCode < 48 || charCode > 57))
        return false;
    return true;
}

function LimparForm() {
    $('input[type=text]').each(function () {
        $(this).val('');
    });

    $("select").each(function () {
        $(this).val($("#" + $(this).attr("id") + " option:first").val());
    });

    $("textarea").each(function () {
        $(this).val('');
    });

    $('input[type="checkbox"]').each(function () {
        this.checked = false;
    });

    $('input[type="radio"]').each(function () {
        this.checked = false;
    });
}
