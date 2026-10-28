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
    }
}
