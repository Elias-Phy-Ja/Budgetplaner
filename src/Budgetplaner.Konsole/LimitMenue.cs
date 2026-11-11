using Budgetplaner.Core.Models;
using Budgetplaner.Core.Services;
using Spectre.Console;

namespace Budgetplaner.Konsole;

public static class LimitMenue
{
    private const string Fertig = "« Fertig";

    // true = mind. ein Limit geändert, dann speichern
    public static bool Festlegen(BudgetProfil profil)
    {
        bool geaendert = false;

        // mehrere Limits hintereinander setzen, bis man "Fertig" wählt
        while (true)
        {
            Console.Clear();
            AnsiConsole.MarkupLine("[bold deepskyblue2]Kategorie-Limits[/]");
            AnsiConsole.MarkupLine("[grey]Wie viel willst du pro Monat höchstens für eine Kategorie ausgeben?[/]");
            Console.WriteLine("");

            var auswahl = new List<string>(Kategorien.Ausgaben) { Fertig };

            string kategorie = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Für welche Kategorie?")
                    .AddChoices(auswahl)
                    .UseConverter(k => k == Fertig ? "[grey]« Fertig[/]" : KategorieText(profil, k)));

            if (kategorie == Fertig)
                return geaendert;

            decimal neuesLimit = Eingabe.LeseBetragOptional($"Limit für {kategorie} in CHF (leer = Limit entfernen): ", 0);

            // 0 bzw. leer = Limit weg
            if (neuesLimit == 0)
            {
                if (profil.KategorieLimits.Remove(kategorie))
                {
                    AnsiConsole.MarkupLine("[green]Limit entfernt ✔[/]");
                    geaendert = true;
                }
            }
            else
            {
                var fehler = Validierung.PruefeLimit(neuesLimit);
                if (fehler.Count > 0)
                {
                    foreach (var f in fehler)
                        Anzeige.Fehler("✘ " + f);
                }
                else
                {
                    profil.KategorieLimits[kategorie] = neuesLimit;
                    AnsiConsole.MarkupLine($"[green]Gespeichert ✔[/]  {Markup.Escape(kategorie)}: max. {Anzeige.Chf(neuesLimit)} pro Monat");
                    geaendert = true;
                }
            }

            AnsiConsole.MarkupLine("[grey]Weiter mit Enter …[/]");
            Console.ReadLine();
        }
    }

    // "Freizeit (Limit: CHF 150.00)" in der Auswahl
    private static string KategorieText(BudgetProfil profil, string kategorie)
    {
        string name = Markup.Escape(kategorie);

        return profil.KategorieLimits.TryGetValue(kategorie, out decimal limit)
            ? $"{name} [grey](Limit: {Anzeige.Chf(limit)})[/]"
            : $"{name} [grey](kein Limit)[/]";
    }
}
