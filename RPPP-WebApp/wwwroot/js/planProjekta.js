$(document).on('click', '.deleterow', function () {
    event.preventDefault();
    var tr = $(this).parents("tr");
    tr.remove();
});

$(function () {
    $("#etapa-dodaj").click(function () {
        event.preventDefault();
        dodajEtapu();
    });
});

var etapaIndex = 0;

function dodajEtapu() {
    var ime = $("#etapa-ime").val();
    var opis = $("#etapa-opis").val();

    var aktivnostId = $("#aktivnost-id").val();
    var aktivnostIme = $("#aktivnost-ime").val();

    var template = $('#template').html();

    template = template.replace(/--sifra--/g, etapaIndex)
        .replace(/--ime--/g, ime)
        .replace(/--opis--/g, opis)
        .replace(/--aktivnostId--/g, aktivnostId)
        .replace(/--aktivnostIme--/g, aktivnostIme)

    $(template).find('tr').insertBefore($("#table-etape tbody").find('tr').last());

    $("#etapa-ime").val('');
    $("#etapa-opis").val('');
    $("#aktivnost-id").val('');
    $("#aktivnost-ime").val('');

    etapaIndex++;
}