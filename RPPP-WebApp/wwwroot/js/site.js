//Pri svakom kliku kontrole koja ima css klasu delete zatraži potvrdu
//za razliku od  //$(".delete").click ovo se odnosi i na elemente koji će se pojaviti u budućnosti 
//dinamičkim učitavanjem
$(function () {
    $(document).on('click', '.delete', function (event) {
        if (!confirm("Jeste li sigurni da želite izbrisati zapis iz baze podataka? Ovaj postupak je ireverzibilan.")) {
            event.preventDefault();
        }
    });
});