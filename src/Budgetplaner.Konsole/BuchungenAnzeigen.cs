using Budgetplaner.Core.Models;
using Spectre.Console;

namespace Budgetplaner.Konsole;

public static class BuchungenAnzeigen
{
    public static void Ausfuehren(BudgetProfil profil)
    {
        AnsiConsole.MarkupLine("[bold deepskyblue2]Alle Buchungen[/]");
        Console.WriteLine("");

        if (profil.Buchungen.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Noch keine Buchungen erfasst. Wähle im Menü 1, um eine zu erfassen.[/]");
            return;
        }

        var tabelle = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Datum")
            .AddColumn("Titel")
            .AddColumn("Kategorie")
            .AddColumn("Wiederholung")
            .AddColumn(new TableColumn("Betrag").RightAligned());

        // Neueste zuerst
        foreach (var b in profil.Buchungen.OrderByDescending(x => x.Datum))
        {
            // Einnahmen grün mit +, Ausgaben rot mit -
            string betrag = b.Typ == BuchungsTyp.Einnahme
                ? $"[green]+ {Anzeige.Chf(b.Betrag)}[/]"
                : $"[red]- {Anzeige.Chf(b.Betrag)}[/]";

            tabelle.AddRow(
                Anzeige.Datum(b.Datum),
                Markup.Escape(b.Titel),
                Markup.Escape(b.Kategorie),
                Anzeige.WiederholungText(b.Wiederholung),
                betrag);
        }

        AnsiConsole.Write(tabelle);
    }
}
