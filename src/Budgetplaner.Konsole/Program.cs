using System.Text;
using Budgetplaner.Core.Models;
using Budgetplaner.Core.Storage;
using Budgetplaner.Konsole;
using Spectre.Console;

// Damit Umlaute und Sonderzeichen in der Konsole korrekt angezeigt werden
Console.OutputEncoding = Encoding.UTF8;

// Beim Start die gespeicherten Daten laden (gibt leeres Profil, wenn noch nichts da ist)
string pfad = JsonSpeicher.StandardPfad;
BudgetProfil profil = JsonSpeicher.Laden(pfad);

string input = "";

// Menü läuft so lange, bis /exit eingegeben wird
while (input != "/exit")
{
    Console.Clear();

    AnsiConsole.MarkupLine("[bold deepskyblue2]Willkommen beim Budgetplaner[/]");
    Console.WriteLine("");
    Console.WriteLine("Was möchten Sie machen?");
    Console.WriteLine("1) Buchung erfassen");
    Console.WriteLine("2) Alle Buchungen anzeigen");
    Console.WriteLine("3) Budget-Übersicht");
    Console.WriteLine("/exit um das Programm zu beenden.");
    Console.WriteLine("");
    AnsiConsole.Markup("[purple]> [/]");

    string? rawInput = Console.ReadLine();
    input = (rawInput ?? "").Trim().ToLower();

    Console.Clear();

    switch (input)
    {
        case "1":
            // nur speichern, wenn wirklich etwas dazugekommen ist
            if (BuchungErfassen.Ausfuehren(profil))
                JsonSpeicher.Speichern(profil, pfad);
            break;

        case "2":
            BuchungenAnzeigen.Ausfuehren(profil);
            break;

        case "3":
            BudgetAnzeigen.Ausfuehren(profil);
            break;

        case "/exit":
            AnsiConsole.MarkupLine("[deepskyblue2]Tschüss! 👋[/]");
            break;

        case "":
            AnsiConsole.MarkupLine("[red]Dieser Input ist Leer![/]");
            break;

        default:
            AnsiConsole.MarkupLine($"[red]Unbekannte Eingabe:[/] {Markup.Escape(input)}");
            break;
    }

    // Warten, damit man das Ergebnis lesen kann, bevor das Menü wieder kommt
    if (input != "/exit")
    {
        Console.WriteLine("");
        AnsiConsole.MarkupLine("[grey]Weiter mit Enter …[/]");
        Console.ReadLine();
    }
}
