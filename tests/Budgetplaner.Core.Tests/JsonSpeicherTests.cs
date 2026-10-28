using Budgetplaner.Core.Models;
using Budgetplaner.Core.Storage;

namespace Budgetplaner.Core.Tests;

// IDisposable: Dispose() läuft nach jedem Test und räumt die Testdatei wieder auf
public class JsonSpeicherTests : IDisposable
{
    private readonly string _pfad = Path.Combine(Path.GetTempPath(), $"budget_test_{Guid.NewGuid()}.json");

    public void Dispose()
    {
        File.Delete(_pfad);
        File.Delete(_pfad + ".defekt");
    }

    [Fact]
    public void SpeichernUndLaden_GibtGleicheDaten()
    {
        var profil = new BudgetProfil();
        profil.Buchungen.Add(new Buchung
        {
            Titel = "Lohn",
            Betrag = 1234.50m,
            Typ = BuchungsTyp.Einnahme,
            Kategorie = "Lohn",
            Wiederholung = Wiederholung.Monatlich
        });
        profil.Sparziele.Add(new Sparziel { Name = "Laptop", Zielbetrag = 1200m });
        profil.KategorieLimits["Freizeit"] = 150m;

        JsonSpeicher.Speichern(profil, _pfad);
        var geladen = JsonSpeicher.Laden(_pfad);

        var buchung = Assert.Single(geladen.Buchungen);
        Assert.Equal("Lohn", buchung.Titel);
        Assert.Equal(1234.50m, buchung.Betrag);
        Assert.Equal(BuchungsTyp.Einnahme, buchung.Typ);
        Assert.Equal(Wiederholung.Monatlich, buchung.Wiederholung);
        Assert.Equal(profil.Buchungen[0].Id, buchung.Id);

        Assert.Equal("Laptop", Assert.Single(geladen.Sparziele).Name);
        Assert.Equal(150m, geladen.KategorieLimits["Freizeit"]);
    }

    [Fact]
    public void Laden_DateiFehlt_GibtLeeresProfil()
    {
        var profil = JsonSpeicher.Laden(_pfad);

        Assert.Empty(profil.Buchungen);
        Assert.Empty(profil.Sparziele);
    }

    [Fact]
    public void Laden_KaputteDatei_GibtLeeresProfilUndMachtKopie()
    {
        File.WriteAllText(_pfad, "{ das ist kein json");

        var profil = JsonSpeicher.Laden(_pfad);

        Assert.Empty(profil.Buchungen);
        Assert.True(File.Exists(_pfad + ".defekt"));
    }

    [Fact]
    public void Speichern_EnumsAlsText()
    {
        var profil = new BudgetProfil();
        profil.Buchungen.Add(new Buchung { Titel = "Test", Betrag = 1m, Wiederholung = Wiederholung.Jaehrlich });

        JsonSpeicher.Speichern(profil, _pfad);

        // damit man die Datei auch von Hand lesen kann
        Assert.Contains("\"Jaehrlich\"", File.ReadAllText(_pfad));
    }
}
