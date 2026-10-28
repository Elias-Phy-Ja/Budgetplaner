using System.Globalization;
using Budgetplaner.Core.Models;
using Budgetplaner.Core.Services;
using Spectre.Console;

namespace Budgetplaner.Konsole;

public static class BuchungErfassen
{
    // Gibt true zurück, wenn eine Buchung hinzugefügt wurde (dann muss gespeichert werden)
    public static bool Ausfuehren(BudgetProfil profil)
    {
        AnsiConsole.MarkupLine("[bold deepskyblue2]Neue Buchung erfassen[/]");
        Console.WriteLine("");

        // Typ und Wiederholung mit Pfeiltasten auswählen, dann kann man sich nicht vertippen
        var typ = AnsiConsole.Prompt(
            new SelectionPrompt<BuchungsTyp>()
                .Title("Einnahme oder Ausgabe?")
                .AddChoices(BuchungsTyp.Einnahme, BuchungsTyp.Ausgabe));

        string titel = LeseText("Titel (z.B. Handy-Abo): ");
        decimal betrag = LeseBetrag("Betrag in CHF: ");

        // Je nach Typ andere Kategorien anbieten
        var kategorien = typ == BuchungsTyp.Einnahme ? Kategorien.Einnahmen : Kategorien.Ausgaben;
        string kategorie = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Kategorie?")
                .AddChoices(kategorien));

        var wiederholung = AnsiConsole.Prompt(
            new SelectionPrompt<Wiederholung>()
                .Title("Wie oft?")
                .AddChoices(Enum.GetValues<Wiederholung>())
                .UseConverter(Anzeige.WiederholungText));

        DateTime datum = LeseDatum(wiederholung == Wiederholung.Einmalig
            ? "Datum (TT.MM.JJJJ, leer = heute): "
            : "Ab wann? (TT.MM.JJJJ, leer = heute): ");

        var buchung = new Buchung
        {
            Titel = titel.Trim(),
            Betrag = betrag,
            Typ = typ,
            Kategorie = kategorie,
            Wiederholung = wiederholung,
            Datum = datum
        };

        // Prüfen macht die Validierung im Core, nicht die Konsole
        var fehler = Validierung.PruefeBuchung(buchung);

        if (fehler.Count > 0)
        {
            Console.WriteLine("");
            foreach (var f in fehler)
                Anzeige.Fehler("✘ " + f);
            return false;
        }

        profil.Buchungen.Add(buchung);

        Console.WriteLine("");
        AnsiConsole.MarkupLine($"[green]Gespeichert ✔[/]  {Markup.Escape(buchung.Titel)} | {Anzeige.Chf(buchung.Betrag)} | {Anzeige.WiederholungText(buchung.Wiederholung)}");
        return true;
    }

    // Fragt so lange, bis etwas eingegeben wurde
    private static string LeseText(string frage)
    {
        while (true)
        {
            AnsiConsole.Markup($"[purple]{Markup.Escape(frage)}[/]");
            string? eingabe = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(eingabe))
                return eingabe;

            Anzeige.Fehler("Dieser Input ist leer!");
        }
    }

    // Fragt so lange, bis eine gültige Zahl kommt. Ob > 0 prüft danach die Validierung.
    private static decimal LeseBetrag(string frage)
    {
        while (true)
        {
            string eingabe = LeseText(frage);

            // 12,50 und 12.50 sollen beide gehen -> Komma durch Punkt ersetzen
            // InvariantCulture = Punkt ist immer das Dezimalzeichen, egal wie Windows eingestellt ist
            string normalisiert = eingabe.Trim().Replace(',', '.');

            if (decimal.TryParse(normalisiert, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal betrag))
                return betrag;

            Anzeige.Fehler("Das ist keine Zahl. Beispiel: 12.50");
        }
    }

    private static DateTime LeseDatum(string frage)
    {
        string[] formate = { "d.M.yyyy", "dd.MM.yyyy" };

        while (true)
        {
            AnsiConsole.Markup($"[purple]{Markup.Escape(frage)}[/]");
            string eingabe = (Console.ReadLine() ?? "").Trim();

            if (eingabe == "")
                return DateTime.Today;

            if (DateTime.TryParseExact(eingabe, formate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime datum))
                return datum;

            Anzeige.Fehler("Ungültiges Datum. Beispiel: 28.10.2026");
        }
    }
}
