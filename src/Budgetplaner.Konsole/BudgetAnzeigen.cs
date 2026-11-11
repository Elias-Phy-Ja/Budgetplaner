using Budgetplaner.Core.Models;
using Budgetplaner.Core.Services;
using Spectre.Console;

namespace Budgetplaner.Konsole;

public static class BudgetAnzeigen
{
    public static void Ausfuehren(BudgetProfil profil)
    {
        // Rechnen macht der BudgetRechner, hier wird nur angezeigt
        var u = BudgetRechner.Berechne(profil, DateTime.Today);

        AnsiConsole.MarkupLine($"[bold deepskyblue2]Budget {Anzeige.Monat(u.Monat)}[/]");
        Console.WriteLine("");

        var grid = new Grid()
            .AddColumn()
            .AddColumn(new GridColumn().RightAligned());

        grid.AddRow("[grey]Wiederkehrende Einnahmen[/]", Anzeige.Chf(u.WiederkehrendeEinnahmen));
        grid.AddRow("[grey]Wiederkehrende Ausgaben[/]", "- " + Anzeige.Chf(u.WiederkehrendeAusgaben));
        grid.AddRow("[bold]Monatlicher Überschuss[/]", "[bold]" + Anzeige.Chf(u.Ueberschuss) + "[/]");
        grid.AddEmptyRow();
        grid.AddRow("[grey]Einmalige Einnahmen[/]", Anzeige.Chf(u.EinmaligeEinnahmen));
        grid.AddRow("[grey]Einmalige Ausgaben[/]", "- " + Anzeige.Chf(u.EinmaligeAusgaben));
        grid.AddEmptyRow();
        grid.AddRow("[bold]Aktuelles Budget[/]", "[bold]" + Anzeige.Chf(u.AktuellesBudget) + "[/]");

        AnsiConsole.Write(grid);
        Console.WriteLine("");

        // Ampel
        string ampel = u.Ampel switch
        {
            AmpelStatus.Gruen => "[bold green]● Grün[/]  Du hast genug Reserve.",
            AmpelStatus.Gelb => "[bold yellow]● Gelb[/]  Es wird knapp.",
            AmpelStatus.Rot => "[bold red]● Rot[/]  Dein Budget ist im Minus!",
            _ => ""
        };

        AnsiConsole.MarkupLine("Ampel: " + ampel);
        Console.WriteLine("");

        ZeigeKategorien(profil);

        // was der Motivator dazu sagt
        Console.WriteLine("");
        AnsiConsole.MarkupLine("[bold]Dein Budget-Coach:[/]");
        foreach (var n in Motivator.Nachrichten(profil, DateTime.Today))
            Anzeige.ZeigeNachricht(n);
    }

    // Ausgaben pro Kategorie, rot wenn übers Limit
    private static void ZeigeKategorien(BudgetProfil profil)
    {
        var ausgaben = BudgetRechner.AusgabenProKategorie(profil, DateTime.Today);

        if (ausgaben.Count == 0)
            return;

        var tabelle = new Table()
            .Border(TableBorder.Rounded)
            .Title("Ausgaben pro Kategorie")
            .AddColumn("Kategorie")
            .AddColumn(new TableColumn("Ausgegeben").RightAligned())
            .AddColumn(new TableColumn("Limit").RightAligned());

        foreach (var (kategorie, betrag) in ausgaben.OrderByDescending(k => k.Value))
        {
            string limitText = "[grey]–[/]";
            string betragText = Anzeige.Chf(betrag);

            if (profil.KategorieLimits.TryGetValue(kategorie, out decimal limit))
            {
                limitText = Anzeige.Chf(limit);

                if (betrag > limit)
                    betragText = $"[bold red]{betragText}[/]";
                else if (betrag >= limit * 0.8m)
                    betragText = $"[yellow]{betragText}[/]";
            }

            tabelle.AddRow(Markup.Escape(kategorie), betragText, limitText);
        }

        AnsiConsole.Write(tabelle);
    }
}
