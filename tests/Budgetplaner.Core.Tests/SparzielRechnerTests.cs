using Budgetplaner.Core.Models;
using Budgetplaner.Core.Services;

namespace Budgetplaner.Core.Tests;

public class SparzielRechnerTests
{
    // fixes "heute", damit die Tests morgen noch gleich laufen
    private static readonly DateTime Heute = new(2026, 11, 11);

    private static Sparziel Ziel(decimal zielbetrag, decimal gespart, DateTime? zieldatum = null)
        => new() { Name = "Laptop", Zielbetrag = zielbetrag, BereitsGespart = gespart, Zieldatum = zieldatum };

    [Fact]
    public void Prognose_1200_Ziel_200_Gespart_300_Ueberschuss_Gibt4Monate()
    {
        // von Hand: fehlt 1000, 1000 / 300 = 3.33 -> aufrunden = 4 Monate
        var p = SparzielRechner.Berechne(Ziel(1200m, 200m), 300m, Heute);

        Assert.Equal(1000m, p.Restbetrag);
        Assert.Equal(4, p.MonateBisZiel);
        Assert.Equal(new DateTime(2027, 3, 11), p.Prognosedatum);
        Assert.Equal(SparzielStatus.AufKurs, p.Status);
        Assert.Null(p.NoetigProMonat); // kein Zieldatum -> nichts zu rechnen
    }

    [Fact]
    public void Prognose_GehtGenauAuf_KeinAufrunden()
    {
        // 900 / 300 = genau 3
        var p = SparzielRechner.Berechne(Ziel(900m, 0m), 300m, Heute);

        Assert.Equal(3, p.MonateBisZiel);
    }

    [Fact]
    public void Fortschritt_Viertel()
    {
        var p = SparzielRechner.Berechne(Ziel(1200m, 300m), 100m, Heute);

        Assert.Equal(0.25m, p.Fortschritt);
    }

    [Theory]
    [InlineData(1200)]
    [InlineData(1500)] // mehr gespart als nötig
    public void SchonGenugGespart_IstErreicht(double gespart)
    {
        var p = SparzielRechner.Berechne(Ziel(1200m, (decimal)gespart), 300m, Heute);

        Assert.Equal(SparzielStatus.Erreicht, p.Status);
        Assert.Equal(0m, p.Restbetrag);
        Assert.Equal(0, p.MonateBisZiel);
        Assert.Equal(1m, p.Fortschritt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-200)]
    public void KeinUeberschuss_NichtErreichbar_OhneAbsturz(double ueberschuss)
    {
        // wichtig: hier darf nicht durch 0 geteilt werden
        var p = SparzielRechner.Berechne(Ziel(1200m, 0m), (decimal)ueberschuss, Heute);

        Assert.Equal(SparzielStatus.NichtErreichbar, p.Status);
        Assert.Null(p.MonateBisZiel);
        Assert.Null(p.Prognosedatum);
    }

    [Fact]
    public void WinzigerUeberschuss_UeberHundertJahre_NichtErreichbar()
    {
        // 1'000'000 / 0.01 = 100 Mio. Monate, da würde AddMonths crashen
        var p = SparzielRechner.Berechne(Ziel(1_000_000m, 0m), 0.01m, Heute);

        Assert.Equal(SparzielStatus.NichtErreichbar, p.Status);
    }

    [Fact]
    public void MitZieldatum_UeberschussReicht_AufKurs()
    {
        // 11.11.2026 -> 15.03.2027 = 4 Monate, 1000 / 4 = 250 pro Monat
        var p = SparzielRechner.Berechne(Ziel(1200m, 200m, new DateTime(2027, 3, 15)), 300m, Heute);

        Assert.Equal(250m, p.NoetigProMonat);
        Assert.Equal(SparzielStatus.AufKurs, p.Status);
    }

    [Fact]
    public void MitZieldatum_UeberschussZuKlein_NichtRealistisch()
    {
        // braucht 250 pro Monat, hat aber nur 200
        var p = SparzielRechner.Berechne(Ziel(1200m, 200m, new DateTime(2027, 3, 15)), 200m, Heute);

        Assert.Equal(250m, p.NoetigProMonat);
        Assert.Equal(SparzielStatus.NichtRealistisch, p.Status);
        Assert.Equal(5, p.MonateBisZiel); // 1000 / 200 = 5, Prognose gibts trotzdem
    }

    [Fact]
    public void NoetigProMonat_WirdAufDenRappenAufgerundet()
    {
        // 100 / 3 = 33.333.. -> 33.34, sonst fehlt am Schluss 1 Rappen
        var p = SparzielRechner.Berechne(Ziel(100m, 0m, new DateTime(2027, 2, 11)), 500m, Heute);

        Assert.Equal(33.34m, p.NoetigProMonat);
    }

    [Fact]
    public void ZieldatumSchonVorbei_GanzerRestAufEinmal()
    {
        // mind. 1 Monat, also muss alles jetzt rein
        var p = SparzielRechner.Berechne(Ziel(1000m, 400m, new DateTime(2026, 10, 1)), 300m, Heute);

        Assert.Equal(600m, p.NoetigProMonat);
        Assert.Equal(SparzielStatus.NichtRealistisch, p.Status);
    }

    [Fact]
    public void MitProfil_HoltUeberschussVomBudgetRechner()
    {
        var profil = new BudgetProfil();
        profil.Buchungen.Add(new Buchung { Titel = "Lohn", Betrag = 800m, Typ = BuchungsTyp.Einnahme, Wiederholung = Wiederholung.Monatlich, Datum = new DateTime(2026, 1, 1) });
        profil.Buchungen.Add(new Buchung { Titel = "Handy", Betrag = 300m, Typ = BuchungsTyp.Ausgabe, Wiederholung = Wiederholung.Monatlich, Datum = new DateTime(2026, 1, 1) });

        // Überschuss 500, fehlt 1000 -> 2 Monate
        var p = SparzielRechner.Berechne(Ziel(1000m, 0m), profil, Heute);

        Assert.Equal(2, p.MonateBisZiel);
    }

    [Theory]
    [InlineData("2026-11-11", "2027-03-15", 4)]
    [InlineData("2026-11-11", "2026-11-30", 0)] // gleicher Monat
    [InlineData("2026-12-31", "2027-01-01", 1)] // über Neujahr
    public void MonateZwischen_ZaehltKalendermonate(string von, string bis, int erwartet)
    {
        Assert.Equal(erwartet, SparzielRechner.MonateZwischen(DateTime.Parse(von), DateTime.Parse(bis)));
    }
}
