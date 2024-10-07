$(document).on('click', '.deleterow', function (event) {
    event.preventDefault();
    var tr = $(this).parents("tr");
    tr.remove();
});

$(function () {
    $(".form-control").bind('keydown', function (event) {
        if (event.which === 13) {
            event.preventDefault();
        }
    });

    $("#artikl-kolicina, #artikl-rabat").bind('keydown', function (event) {
        if (event.which === 13) {
            event.preventDefault();
            dodajArtikl();
        }
    });


    $("#posao-dodaj").click(function (event) {
        event.preventDefault();
        dodajPosao();
    });
});

var index = 1000;

function dodajPosao() {
    // var sifra = $("#posao").val();
    var sifra = index++;
    if (sifra != '') {
        if ($("[name='Stavke[" + sifra + "].SifArtikla'").length > 0) {
            alert('Artikl je ve? u dokumentu');
            return;
        }

        var opis = $("#posao-opis").val();
        var posaopocetak = $("#posao-pocetak").val();
        var template = $('#template').html();
        var posaokraj= $("#posao-kraj").val();
        var posaovrsta = $("#posao-vrsta").val();
        var posaovrstaid = $("#posao-vrstaid").val();
        var posaozadatakid = $("#posao-zadatakid").val();
        var posaozadatakime = $("#posao-zadatak").val();
        console.log(posaozadatakid);
        console.log(posaozadatakime);
        console.log(posaovrsta);

        template = template.replace(/--sifra--/g, sifra)
            .replace(/--vrijemepocetak--/g, posaopocetak)
            .replace(/--opis--/g, opis)
            .replace(/--vrijemekraja--/g, posaokraj)
            .replace(/--zadatakid--/g, posaozadatakid)
            .replace(/--zadatakime--/g, posaozadatakime)
            .replace(/--vrstaime--/g, posaovrsta)
            .replace(/--vrstaid--/g, posaovrstaid);

        $(template).find('tr').insertBefore($("#table-posao").find('tr').last());

        $("#posao-opis").val('');
        $("#posao-pocetak").val('');
        $("#posao-kraj").val('');
        $("#posao-vrsta").val('');
        $("#posao-vrstaid").val('');
        $("#posao-zadatakid").val('');
        $("#posao-zadatak").val('');
    }
}