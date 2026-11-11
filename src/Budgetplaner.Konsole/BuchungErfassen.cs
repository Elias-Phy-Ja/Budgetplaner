using Budgetplaner.Core.Models;
using Budgetplaner.Core.Services;
using Spectre.Console;

namespace Budgetplaner.Konsole;

public static class BuchungErfassen
{
    // true = Buchung wurde hinzugefügt, dann muss gespeichert werden
    public static bool Ausfuehren(BudgetProfil profil)
    {
        AnsiConsole.MarkupLine("[bold deepskyblue2]Neue Buchung erfassen[/]");
        Console.WriteLine("");

        // mit Pfeiltasten wählen, so kann man sich nicht vertippen
        var typ = AnsiConsole.Prompt(
            new SelectionPrompt<BuchungsTyp>()
                .Title("Einnahme oder Ausgabe?")
                .AddChoices(BuchungsTyp.Einnahme, BuchungsTyp.Ausgabe));

        string titel = Eingabe.LeseText("Titel (z.B. Handy-Abo): ");
        decimal betrag = Eingabe.LeseBetrag("Betrag in CHF: ");
        string kategorie = WaehleKategorie(typ);
        var wiederholung = WaehleWiederholung();

        DateTime datum = Eingabe.LeseDatum(wiederholung == Wiederholung.Einmalig
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

        // prüfen macht Core, nicht die Konsole
        if (!IstGueltig(buchung))
            return false;

        profil.Buchungen.Add(buchung);

        Console.WriteLine("");
        AnsiConsole.MarkupLine($"[green]Gespeichert ✔[/]  {Markup.Escape(buchung.Titel)} | {Anzeige.Chf(buchung.Betrag)} | {Anzeige.WiederholungText(buchung.Wiederholung)}");
        return true;
    }

    // je nach Typ andere Kategorien
    public static string WaehleKategorie(BuchungsTyp typ)
    {
        var kategorien = typ == BuchungsTyp.Einnahme ? Kategorien.Einnahmen : Kategorien.Ausgaben;

        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Kategorie?")
                .AddChoices(kategorien));
    }

    public static Wiederholung WaehleWiederholung()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<Wiederholung>()
                .Title("Wie oft?")
                .AddChoices(Enum.GetValues<Wiederholung>())
                .UseConverter(Anzeige.WiederholungText));
    }

    // zeigt alle Fehler an, true wenn keine da sind
    public static bool IstGueltig(Buchung buchung)
    {
        var fehler = Validierung.PruefeBuchung(buchung);

        if (fehler.Count == 0)
            return true;

        Console.WriteLine("");
        foreach (var f in fehler)
            Anzeige.Fehler("✘ " + f);

        return false;
    }
}
