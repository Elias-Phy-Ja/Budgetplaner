using Budgetplaner.Core.Models;
using Spectre.Console;

namespace Budgetplaner.Konsole;

public static class BuchungBearbeiten
{
    // Platzhalter für "Abbrechen" in der Auswahl, wird nur per Referenz verglichen
    private static readonly Buchung Abbrechen = new() { Titel = "« Abbrechen" };

    // true = es wurde was geändert, dann speichern
    public static bool Bearbeiten(BudgetProfil profil)
    {
        AnsiConsole.MarkupLine("[bold deepskyblue2]Buchung bearbeiten[/]");
        Console.WriteLine("");

        var original = WaehleBuchung(profil, "Welche Buchung willst du bearbeiten?");
        if (original == null)
            return false;

        // auf einer Kopie arbeiten, damit bei ungültiger Eingabe das Original nicht kaputt ist
        var kopie = new Buchung
        {
            Id = original.Id,
            Titel = original.Titel,
            Betrag = original.Betrag,
            Typ = original.Typ,
            Kategorie = original.Kategorie,
            Wiederholung = original.Wiederholung,
            Datum = original.Datum
        };

        string feld = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Was willst du ändern?")
                .AddChoices("Titel", "Betrag", "Kategorie", "Wiederholung", "Datum", "Einnahme/Ausgabe", "« Abbrechen"));

        switch (feld)
        {
            case "Titel":
                kopie.Titel = Eingabe.LeseText($"Neuer Titel (bisher: {original.Titel}): ").Trim();
                break;

            case "Betrag":
                kopie.Betrag = Eingabe.LeseBetrag($"Neuer Betrag (bisher: {original.Betrag}): ");
                break;

            case "Kategorie":
                kopie.Kategorie = BuchungErfassen.WaehleKategorie(kopie.Typ);
                break;

            case "Wiederholung":
                kopie.Wiederholung = BuchungErfassen.WaehleWiederholung();
                break;

            case "Datum":
                kopie.Datum = Eingabe.LeseDatum($"Neues Datum (bisher: {Anzeige.Datum(original.Datum)}, leer = heute): ");
                break;

            case "Einnahme/Ausgabe":
                // Typ umdrehen, dann passt die alte Kategorie nicht mehr -> neu wählen
                kopie.Typ = kopie.Typ == BuchungsTyp.Einnahme ? BuchungsTyp.Ausgabe : BuchungsTyp.Einnahme;
                AnsiConsole.MarkupLine($"Ist jetzt: [bold]{kopie.Typ}[/]");
                kopie.Kategorie = BuchungErfassen.WaehleKategorie(kopie.Typ);
                break;

            default:
                return false;
        }

        if (!BuchungErfassen.IstGueltig(kopie))
            return false;

        // Original in der Liste durch die Kopie ersetzen
        int index = profil.Buchungen.IndexOf(original);
        profil.Buchungen[index] = kopie;

        Console.WriteLine("");
        AnsiConsole.MarkupLine($"[green]Geändert ✔[/]  {BuchungText(kopie)}");
        return true;
    }

    // true = gelöscht, dann speichern
    public static bool Loeschen(BudgetProfil profil)
    {
        AnsiConsole.MarkupLine("[bold deepskyblue2]Buchung löschen[/]");
        Console.WriteLine("");

        var buchung = WaehleBuchung(profil, "Welche Buchung willst du löschen?");
        if (buchung == null)
            return false;

        AnsiConsole.MarkupLine(BuchungText(buchung));

        // lieber nochmal fragen, löschen kann man nicht rückgängig machen
        if (!AnsiConsole.Confirm("[red]Wirklich löschen?[/]", defaultValue: false))
        {
            AnsiConsole.MarkupLine("[grey]Nichts gelöscht.[/]");
            return false;
        }

        profil.Buchungen.Remove(buchung);
        AnsiConsole.MarkupLine("[green]Gelöscht ✔[/]");
        return true;
    }

    // Auswahlliste mit allen Buchungen, null wenn abgebrochen oder keine da
    private static Buchung? WaehleBuchung(BudgetProfil profil, string frage)
    {
        if (profil.Buchungen.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Noch keine Buchungen erfasst.[/]");
            return null;
        }

        var auswahl = new SelectionPrompt<Buchung>()
            .Title(frage)
            .PageSize(10)
            .AddChoices(profil.Buchungen.OrderByDescending(x => x.Datum))
            .AddChoices(Abbrechen)
            .UseConverter(b => b == Abbrechen ? "[grey]« Abbrechen[/]" : BuchungText(b));

        var gewaehlt = AnsiConsole.Prompt(auswahl);

        return gewaehlt == Abbrechen ? null : gewaehlt;
    }

    // eine Zeile pro Buchung, Titel escapen wegen [ ]
    private static string BuchungText(Buchung b)
    {
        string betrag = b.Typ == BuchungsTyp.Einnahme
            ? $"[green]+ {Anzeige.Chf(b.Betrag)}[/]"
            : $"[red]- {Anzeige.Chf(b.Betrag)}[/]";

        return $"{Anzeige.Datum(b.Datum)} | {Markup.Escape(b.Titel)} | {betrag} | {Anzeige.WiederholungText(b.Wiederholung)}";
    }
}
