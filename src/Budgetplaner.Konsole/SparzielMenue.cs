using Budgetplaner.Core.Models;
using Budgetplaner.Core.Services;
using Spectre.Console;

namespace Budgetplaner.Konsole;

public static class SparzielMenue
{
    // Platzhalter für "Abbrechen" in der Auswahl
    private static readonly Sparziel Abbrechen = new() { Name = "« Abbrechen" };

    // true = hinzugefügt, dann speichern
    public static bool Erfassen(BudgetProfil profil)
    {
        AnsiConsole.MarkupLine("[bold deepskyblue2]Neues Sparziel[/]");
        Console.WriteLine("");

        var ziel = new Sparziel
        {
            Name = Eingabe.LeseText("Wofür sparst du? (z.B. Neuer Laptop): ").Trim(),
            Zielbetrag = Eingabe.LeseBetrag("Zielbetrag in CHF: "),
            BereitsGespart = Eingabe.LeseBetragOptional("Schon gespart? (leer = 0): ", 0),
            Zieldatum = Eingabe.LeseDatumOptional("Bis wann? (TT.MM.JJJJ, leer = egal): ")
        };

        if (!ZeigeFehler(Validierung.PruefeSparziel(ziel)))
            return false;

        profil.Sparziele.Add(ziel);

        Console.WriteLine("");
        AnsiConsole.MarkupLine($"[green]Gespeichert ✔[/]  {Markup.Escape(ziel.Name)}");
        Console.WriteLine("");

        // gleich zeigen, wie es aussieht
        ZeigeZiel(ziel, profil);
        return true;
    }

    public static void Anzeigen(BudgetProfil profil)
    {
        AnsiConsole.MarkupLine("[bold deepskyblue2]Deine Sparziele[/]");
        Console.WriteLine("");

        if (profil.Sparziele.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Noch keine Sparziele. Wähle im Menü 6, um eins zu erfassen.[/]");
            return;
        }

        decimal ueberschuss = BudgetRechner.Berechne(profil, DateTime.Today).Ueberschuss;
        AnsiConsole.MarkupLine($"Monatlicher Überschuss: [bold]{Anzeige.Chf(ueberschuss)}[/]");
        Console.WriteLine("");

        foreach (var ziel in profil.Sparziele)
            ZeigeZiel(ziel, profil);
    }

    // true = eingezahlt, dann speichern
    public static bool Einzahlen(BudgetProfil profil)
    {
        AnsiConsole.MarkupLine("[bold deepskyblue2]Auf Sparziel einzahlen[/]");
        Console.WriteLine("");

        var ziel = WaehleZiel(profil, "Auf welches Sparziel?");
        if (ziel == null)
            return false;

        decimal betrag = Eingabe.LeseBetrag("Wie viel legst du zur Seite? CHF ");

        if (!ZeigeFehler(Validierung.PruefeEinzahlung(betrag)))
            return false;

        // vorher merken, damit der Motivator sieht ob ein Meilenstein geknackt wurde
        decimal vorher = ziel.BereitsGespart;

        ziel.BereitsGespart += betrag;

        Console.WriteLine("");
        AnsiConsole.MarkupLine($"[green]Eingezahlt ✔[/]  {Anzeige.Chf(betrag)}");
        Console.WriteLine("");

        // 25 / 50 / 75 / 100 % geknackt?
        var feier = Motivator.NachEinzahlung(ziel, vorher);
        if (feier != null)
        {
            Anzeige.ZeigeNachricht(feier);
            Console.WriteLine("");
        }

        ZeigeZiel(ziel, profil);
        return true;
    }

    // true = gelöscht, dann speichern
    public static bool Loeschen(BudgetProfil profil)
    {
        AnsiConsole.MarkupLine("[bold deepskyblue2]Sparziel löschen[/]");
        Console.WriteLine("");

        var ziel = WaehleZiel(profil, "Welches Sparziel willst du löschen?");
        if (ziel == null)
            return false;

        if (!AnsiConsole.Confirm($"[red]„{Markup.Escape(ziel.Name)}“ wirklich löschen?[/]", defaultValue: false))
        {
            AnsiConsole.MarkupLine("[grey]Nichts gelöscht.[/]");
            return false;
        }

        profil.Sparziele.Remove(ziel);
        AnsiConsole.MarkupLine("[green]Gelöscht ✔[/]");
        return true;
    }

    // ein Kasten pro Ziel mit Balken und Prognose
    private static void ZeigeZiel(Sparziel ziel, BudgetProfil profil)
    {
        var p = SparzielRechner.Berechne(ziel, profil, DateTime.Today);

        var zeilen = new List<string>
        {
            $"{Anzeige.Fortschrittsbalken(p.Fortschritt)}  {p.Fortschritt:P0}",
            $"{Anzeige.Chf(ziel.BereitsGespart)} von {Anzeige.Chf(ziel.Zielbetrag)}  [grey](fehlt noch {Anzeige.Chf(p.Restbetrag)})[/]",
            "",
            "Status: " + Anzeige.SparzielStatusText(p.Status)
        };

        // Frage 1: bis wann?
        if (p.Status != SparzielStatus.Erreicht && p.Prognosedatum.HasValue)
            zeilen.Add($"Prognose: erreicht ca. [bold]{Anzeige.Monat(p.Prognosedatum.Value)}[/] (in {p.MonateBisZiel} {(p.MonateBisZiel == 1 ? "Monat" : "Monaten")})");

        // Frage 2: wie viel pro Monat fürs Zieldatum?
        if (p.Status != SparzielStatus.Erreicht && ziel.Zieldatum.HasValue && p.NoetigProMonat.HasValue)
            zeilen.Add($"Für dein Zieldatum {Anzeige.Datum(ziel.Zieldatum.Value)} brauchst du [bold]{Anzeige.Chf(p.NoetigProMonat.Value)}[/] pro Monat");

        var panel = new Panel(new Markup(string.Join("\n", zeilen)))
            .Header($" {Markup.Escape(ziel.Name)} ")
            .Border(BoxBorder.Rounded)
            .Expand();

        AnsiConsole.Write(panel);
    }

    private static Sparziel? WaehleZiel(BudgetProfil profil, string frage)
    {
        if (profil.Sparziele.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Noch keine Sparziele erfasst.[/]");
            return null;
        }

        var gewaehlt = AnsiConsole.Prompt(
            new SelectionPrompt<Sparziel>()
                .Title(frage)
                .AddChoices(profil.Sparziele)
                .AddChoices(Abbrechen)
                .UseConverter(z => z == Abbrechen
                    ? "[grey]« Abbrechen[/]"
                    : $"{Markup.Escape(z.Name)} [grey]({Anzeige.Chf(z.BereitsGespart)} / {Anzeige.Chf(z.Zielbetrag)})[/]"));

        return gewaehlt == Abbrechen ? null : gewaehlt;
    }

    // true = keine Fehler
    private static bool ZeigeFehler(List<string> fehler)
    {
        if (fehler.Count == 0)
            return true;

        Console.WriteLine("");
        foreach (var f in fehler)
            Anzeige.Fehler("✘ " + f);

        return false;
    }
}
