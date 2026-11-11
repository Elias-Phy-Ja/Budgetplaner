using Budgetplaner.Core.Models;
using Budgetplaner.Core.Services;

namespace Budgetplaner.Core.Tests;

public class MotivatorTests
{
    private static readonly DateTime Heute = new(2026, 11, 11);
    private static readonly DateTime Januar = new(2026, 1, 1);

    private static Buchung Monatlich(string titel, decimal betrag, BuchungsTyp typ, string kategorie = "Sonstiges", DateTime? ab = null)
        => new() { Titel = titel, Betrag = betrag, Typ = typ, Kategorie = kategorie, Wiederholung = Wiederholung.Monatlich, Datum = ab ?? Januar };

    private static Buchung Einmalig(string titel, decimal betrag, BuchungsTyp typ, DateTime datum, string kategorie = "Sonstiges")
        => new() { Titel = titel, Betrag = betrag, Typ = typ, Kategorie = kategorie, Wiederholung = Wiederholung.Einmalig, Datum = datum };

    [Fact]
    public void LeeresProfil_EinHinweisZumStarten()
    {
        var n = Motivator.Nachrichten(new BudgetProfil(), Heute);

        var einzige = Assert.Single(n);
        Assert.Equal(NachrichtArt.Hinweis, einzige.Art);
    }

    [Fact]
    public void GuteSparquote_GibtLob()
    {
        var profil = new BudgetProfil();
        profil.Buchungen.Add(Monatlich("Lohn", 1000m, BuchungsTyp.Einnahme));
        profil.Buchungen.Add(Monatlich("Handy", 200m, BuchungsTyp.Ausgabe));

        var n = Motivator.Nachrichten(profil, Heute);

        // 80% übrig -> "Wow" Lob
        Assert.Contains(n, x => x.Art == NachrichtArt.Lob && x.Text.Contains("80"));
    }

    [Fact]
    public void ImMinus_GibtWarnungUndTippZurGroesstenKategorie()
    {
        var profil = new BudgetProfil();
        profil.Buchungen.Add(Monatlich("Lohn", 500m, BuchungsTyp.Einnahme));
        profil.Buchungen.Add(Monatlich("Ausgang", 400m, BuchungsTyp.Ausgabe, "Freizeit"));
        profil.Buchungen.Add(Monatlich("Handy", 200m, BuchungsTyp.Ausgabe, "Kommunikation"));

        var n = Motivator.Nachrichten(profil, Heute);

        Assert.Equal(NachrichtArt.Warnung, n[0].Art); // Warnung steht zuoberst
        Assert.Contains(n, x => x.Text.Contains("Freizeit"));
    }

    [Fact]
    public void LimitUeberschritten_GibtWarnungMitDifferenz()
    {
        var profil = new BudgetProfil();
        profil.Buchungen.Add(Monatlich("Lohn", 2000m, BuchungsTyp.Einnahme));
        profil.Buchungen.Add(Monatlich("Ausgang", 150m, BuchungsTyp.Ausgabe, "Freizeit"));
        profil.Buchungen.Add(Einmalig("Konzert", 40m, BuchungsTyp.Ausgabe, new DateTime(2026, 11, 5), "Freizeit"));
        profil.KategorieLimits["Freizeit"] = 150m;

        var n = Motivator.Nachrichten(profil, Heute);

        // 150 + 40 = 190, Limit 150 -> 40 drüber
        Assert.Contains(n, x => x.Art == NachrichtArt.Warnung && x.Text.Contains("Freizeit") && x.Text.Contains("40.00"));
    }

    [Fact]
    public void LimitFastVoll_GibtHinweis()
    {
        var profil = new BudgetProfil();
        profil.Buchungen.Add(Monatlich("Lohn", 2000m, BuchungsTyp.Einnahme));
        profil.Buchungen.Add(Monatlich("Ausgang", 90m, BuchungsTyp.Ausgabe, "Freizeit"));
        profil.KategorieLimits["Freizeit"] = 100m;

        var n = Motivator.Nachrichten(profil, Heute);

        Assert.Contains(n, x => x.Art == NachrichtArt.Hinweis && x.Text.Contains("Freizeit"));
    }

    [Fact]
    public void SparzielErreicht_GibtFeier()
    {
        var profil = new BudgetProfil();
        profil.Buchungen.Add(Monatlich("Lohn", 1000m, BuchungsTyp.Einnahme));
        profil.Sparziele.Add(new Sparziel { Name = "Velo", Zielbetrag = 500m, BereitsGespart = 500m });

        var n = Motivator.Nachrichten(profil, Heute);

        Assert.Contains(n, x => x.Art == NachrichtArt.Feier && x.Text.Contains("Velo"));
    }

    [Fact]
    public void Streak_AlleMonateImPlus()
    {
        var profil = new BudgetProfil();
        profil.Buchungen.Add(Monatlich("Lohn", 1000m, BuchungsTyp.Einnahme, ab: new DateTime(2026, 8, 1)));
        profil.Buchungen.Add(Monatlich("Miete", 500m, BuchungsTyp.Ausgabe, ab: new DateTime(2026, 8, 1)));

        // Aug, Sep, Okt, Nov = 4
        Assert.Equal(4, Motivator.PlusStreak(profil, Heute));
    }

    [Fact]
    public void Streak_HoertBeiMinusMonatAuf()
    {
        var profil = new BudgetProfil();
        profil.Buchungen.Add(Monatlich("Lohn", 1000m, BuchungsTyp.Einnahme, ab: new DateTime(2026, 8, 1)));
        profil.Buchungen.Add(Einmalig("Laptop", 2000m, BuchungsTyp.Ausgabe, new DateTime(2026, 9, 10)));

        // Nov + Okt im Plus, Sep wegen Laptop im Minus -> 2
        Assert.Equal(2, Motivator.PlusStreak(profil, Heute));
    }

    [Theory]
    [InlineData(200, 650, "50 %")]    // 20% -> 65%
    [InlineData(0, 300, "25 %")]      // 0% -> 30%
    [InlineData(100, 900, "75 %")]    // mehrere übersprungen -> nimmt den höchsten
    public void NachEinzahlung_ErkenntMeilenstein(double vorher, double nachher, string erwartet)
    {
        var ziel = new Sparziel { Name = "Laptop", Zielbetrag = 1000m, BereitsGespart = (decimal)nachher };

        var n = Motivator.NachEinzahlung(ziel, (decimal)vorher);

        Assert.NotNull(n);
        Assert.Equal(NachrichtArt.Feier, n.Art);
        Assert.Contains(erwartet, n.Text);
    }

    [Fact]
    public void NachEinzahlung_Komplett_GibtGeschafft()
    {
        var ziel = new Sparziel { Name = "Laptop", Zielbetrag = 1000m, BereitsGespart = 1000m };

        var n = Motivator.NachEinzahlung(ziel, 900m);

        Assert.NotNull(n);
        Assert.Contains("komplett", n.Text);
    }

    [Fact]
    public void NachEinzahlung_KeinMeilenstein_GibtNull()
    {
        var ziel = new Sparziel { Name = "Laptop", Zielbetrag = 1000m, BereitsGespart = 300m };

        // 26% -> 30%, nichts geknackt
        Assert.Null(Motivator.NachEinzahlung(ziel, 260m));
    }

    [Fact]
    public void AusgabenProKategorie_ZaehltWiederkehrendUndEinmaligDiesesMonats()
    {
        var profil = new BudgetProfil();
        profil.Buchungen.Add(Monatlich("Ausgang", 100m, BuchungsTyp.Ausgabe, "Freizeit"));
        profil.Buchungen.Add(Einmalig("Kino", 20m, BuchungsTyp.Ausgabe, new DateTime(2026, 11, 3), "Freizeit"));
        profil.Buchungen.Add(Einmalig("Kino alt", 99m, BuchungsTyp.Ausgabe, new DateTime(2026, 10, 3), "Freizeit")); // Oktober zählt nicht
        profil.Buchungen.Add(Monatlich("Lohn", 1000m, BuchungsTyp.Einnahme, "Lohn")); // Einnahme zählt nicht

        var ausgaben = BudgetRechner.AusgabenProKategorie(profil, Heute);

        Assert.Equal(120m, ausgaben["Freizeit"]);
        Assert.False(ausgaben.ContainsKey("Lohn"));
    }
}
