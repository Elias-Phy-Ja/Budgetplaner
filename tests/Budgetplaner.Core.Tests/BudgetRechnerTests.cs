using Budgetplaner.Core.Models;
using Budgetplaner.Core.Services;

namespace Budgetplaner.Core.Tests;

public class BudgetRechnerTests
{
    // fixer Monat, damit die Tests nicht vom heutigen Datum abhängen
    private static readonly DateTime November = new(2026, 11, 1);

    private static Buchung Neu(string titel, decimal betrag, BuchungsTyp typ, Wiederholung wiederholung, DateTime datum)
        => new() { Titel = titel, Betrag = betrag, Typ = typ, Wiederholung = wiederholung, Datum = datum };

    // ---------- Umrechnung pro Monat ----------

    [Fact]
    public void ProMonat_Woechentlich_100_Gibt_433_33()
    {
        var b = Neu("Znüni", 100m, BuchungsTyp.Ausgabe, Wiederholung.Woechentlich, November);

        // 100 * 52 / 12 = 433.333...
        Assert.Equal(433.33m, Math.Round(BudgetRechner.ProMonat(b), 2));
    }

    [Fact]
    public void ProMonat_Monatlich_BleibtGleich()
    {
        var b = Neu("Handy", 30m, BuchungsTyp.Ausgabe, Wiederholung.Monatlich, November);

        Assert.Equal(30m, BudgetRechner.ProMonat(b));
    }

    [Fact]
    public void ProMonat_Jaehrlich_DurchZwoelf()
    {
        var b = Neu("Versicherung", 1200m, BuchungsTyp.Ausgabe, Wiederholung.Jaehrlich, November);

        Assert.Equal(100m, BudgetRechner.ProMonat(b));
    }

    [Fact]
    public void ProMonat_Einmalig_IstNull()
    {
        var b = Neu("Velo", 500m, BuchungsTyp.Ausgabe, Wiederholung.Einmalig, November);

        Assert.Equal(0m, BudgetRechner.ProMonat(b));
    }

    // ---------- Berechnung für einen Monat ----------

    [Fact]
    public void Berechne_BeispielProfil_StimmtMitHandrechnung()
    {
        var profil = new BudgetProfil();
        var januar = new DateTime(2026, 1, 1);

        profil.Buchungen.Add(Neu("Lohn", 1000m, BuchungsTyp.Einnahme, Wiederholung.Monatlich, januar));
        profil.Buchungen.Add(Neu("Handy", 30m, BuchungsTyp.Ausgabe, Wiederholung.Monatlich, januar));
        profil.Buchungen.Add(Neu("Versicherung", 600m, BuchungsTyp.Ausgabe, Wiederholung.Jaehrlich, januar)); // = 50 pro Monat
        profil.Buchungen.Add(Neu("Konzert", 120m, BuchungsTyp.Ausgabe, Wiederholung.Einmalig, new DateTime(2026, 11, 15)));
        profil.Buchungen.Add(Neu("Velo", 500m, BuchungsTyp.Ausgabe, Wiederholung.Einmalig, new DateTime(2026, 10, 15))); // anderer Monat!

        var u = BudgetRechner.Berechne(profil, November);

        // Von Hand: 1000 - 30 - 50 = 920 Überschuss, minus 120 Konzert = 800
        Assert.Equal(1000m, u.WiederkehrendeEinnahmen);
        Assert.Equal(80m, u.WiederkehrendeAusgaben);
        Assert.Equal(920m, u.Ueberschuss);
        Assert.Equal(120m, u.EinmaligeAusgaben);
        Assert.Equal(800m, u.AktuellesBudget);
        Assert.Equal(AmpelStatus.Gruen, u.Ampel);
    }

    [Fact]
    public void Berechne_WiederkehrendStartetSpaeter_ZaehltNochNicht()
    {
        var profil = new BudgetProfil();
        profil.Buchungen.Add(Neu("Neues Abo", 50m, BuchungsTyp.Ausgabe, Wiederholung.Monatlich, new DateTime(2026, 12, 1)));

        var u = BudgetRechner.Berechne(profil, November);

        Assert.Equal(0m, u.WiederkehrendeAusgaben);
    }

    [Fact]
    public void Berechne_LeeresProfil_AllesNullUndGelb()
    {
        var u = BudgetRechner.Berechne(new BudgetProfil(), November);

        Assert.Equal(0m, u.AktuellesBudget);
        Assert.Equal(AmpelStatus.Gelb, u.Ampel);
    }

    [Fact]
    public void Berechne_MehrAusgabenAlsEinnahmen_IstRot()
    {
        var profil = new BudgetProfil();
        profil.Buchungen.Add(Neu("Lohn", 500m, BuchungsTyp.Einnahme, Wiederholung.Monatlich, November));
        profil.Buchungen.Add(Neu("Miete", 800m, BuchungsTyp.Ausgabe, Wiederholung.Monatlich, November));

        var u = BudgetRechner.Berechne(profil, November);

        Assert.Equal(-300m, u.AktuellesBudget);
        Assert.Equal(AmpelStatus.Rot, u.Ampel);
    }

    // ---------- Ampel ----------

    [Theory]
    [InlineData(-1, 1000, AmpelStatus.Rot)]
    [InlineData(0, 1000, AmpelStatus.Gelb)]
    [InlineData(99.99, 1000, AmpelStatus.Gelb)]
    [InlineData(100, 1000, AmpelStatus.Gruen)]   // genau 10% -> grün
    [InlineData(0, 0, AmpelStatus.Gelb)]         // keine Einnahmen, keine Division durch 0
    public void BestimmeAmpel_Grenzwerte(double budget, double einnahmen, AmpelStatus erwartet)
    {
        var ampel = BudgetRechner.BestimmeAmpel((decimal)budget, (decimal)einnahmen);

        Assert.Equal(erwartet, ampel);
    }
}
