using System.Globalization;
using Budgetplaner.Core.Models;
using Budgetplaner.Core.Services;

namespace Budgetplaner.Core.Tests;

public class ValidierungTests
{
    // Hilfsmethoden: liefern ein gültiges Objekt, das im Test gezielt "kaputt" gemacht wird
    private static Buchung GueltigeBuchung() => new()
    {
        Titel = "Handy-Abo",
        Betrag = 29.90m,
        Typ = BuchungsTyp.Ausgabe,
        Kategorie = "Kommunikation",
        Wiederholung = Wiederholung.Monatlich
    };

    private static Sparziel GueltigesSparziel() => new()
    {
        Name = "Neuer Laptop",
        Zielbetrag = 1200m,
        BereitsGespart = 150m,
        Zieldatum = DateTime.Today.AddMonths(6)
    };

    // decimal geht nicht in InlineData → als string übergeben und umwandeln
    private static decimal Dez(string wert) => decimal.Parse(wert, CultureInfo.InvariantCulture);

    // ---------- Buchung ----------

    [Fact]
    public void GueltigeBuchung_HatKeineFehler()
    {
        var fehler = Validierung.PruefeBuchung(GueltigeBuchung());

        Assert.Empty(fehler);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Buchung_LeererTitel_GibtFehler(string titel)
    {
        var buchung = GueltigeBuchung();
        buchung.Titel = titel;

        var fehler = Validierung.PruefeBuchung(buchung);

        Assert.Single(fehler);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("12.505")]
    [InlineData("0.001")]
    public void Buchung_UngueltigerBetrag_GibtFehler(string betrag)
    {
        var buchung = GueltigeBuchung();
        buchung.Betrag = Dez(betrag);

        var fehler = Validierung.PruefeBuchung(buchung);

        Assert.Single(fehler);
    }

    [Theory]
    [InlineData("0.05")]
    [InlineData("12.5")]
    [InlineData("12.50")]
    [InlineData("5000")]
    public void Buchung_GueltigerBetrag_KeineFehler(string betrag)
    {
        var buchung = GueltigeBuchung();
        buchung.Betrag = Dez(betrag);

        var fehler = Validierung.PruefeBuchung(buchung);

        Assert.Empty(fehler);
    }

    [Fact]
    public void Buchung_MehrereFehler_WerdenAlleGemeldet()
    {
        var buchung = new Buchung { Titel = "", Betrag = 0 };

        var fehler = Validierung.PruefeBuchung(buchung);

        Assert.Equal(2, fehler.Count);
    }

    // ---------- Sparziel ----------

    [Fact]
    public void GueltigesSparziel_HatKeineFehler()
    {
        var fehler = Validierung.PruefeSparziel(GueltigesSparziel());

        Assert.Empty(fehler);
    }

    [Fact]
    public void Sparziel_OhneZieldatum_IstGueltig()
    {
        var ziel = GueltigesSparziel();
        ziel.Zieldatum = null;

        var fehler = Validierung.PruefeSparziel(ziel);

        Assert.Empty(fehler);
    }

    [Theory]
    [InlineData(-1)] // gestern
    [InlineData(0)]  // heute
    public void Sparziel_ZieldatumNichtInZukunft_GibtFehler(int tageAbHeute)
    {
        var ziel = GueltigesSparziel();
        ziel.Zieldatum = DateTime.Today.AddDays(tageAbHeute);

        var fehler = Validierung.PruefeSparziel(ziel);

        Assert.Single(fehler);
    }

    [Fact]
    public void Sparziel_LeererName_GibtFehler()
    {
        var ziel = GueltigesSparziel();
        ziel.Name = " ";

        Assert.Single(Validierung.PruefeSparziel(ziel));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-100")]
    public void Sparziel_UngueltigerZielbetrag_GibtFehler(string betrag)
    {
        var ziel = GueltigesSparziel();
        ziel.Zielbetrag = Dez(betrag);

        Assert.Single(Validierung.PruefeSparziel(ziel));
    }

    [Fact]
    public void Sparziel_NegativBereitsGespart_GibtFehler()
    {
        var ziel = GueltigesSparziel();
        ziel.BereitsGespart = -1m;

        Assert.Single(Validierung.PruefeSparziel(ziel));
    }
}
