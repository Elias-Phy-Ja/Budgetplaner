using System.Text;
using Budgetplaner.Core.Models;
using Budgetplaner.Core.Services;
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

    // wichtigste Nachricht vom Motivator gleich oben zeigen
    var tipp = Motivator.Nachrichten(profil, DateTime.Today).FirstOrDefault();
    if (tipp != null)
    {
        Anzeige.ZeigeNachricht(tipp);
        Console.WriteLine("");
    }

    Console.WriteLine("Was möchten Sie machen?");
    Console.WriteLine("1) Buchung erfassen");
    Console.WriteLine("2) Alle Buchungen anzeigen");
    Console.WriteLine("3) Budget-Übersicht");
    Console.WriteLine("4) Buchung bearbeiten");
    Console.WriteLine("5) Buchung löschen");
    Console.WriteLine("");
    Console.WriteLine("6) Sparziel erfassen");
    Console.WriteLine("7) Sparziele anzeigen");
    Console.WriteLine("8) Auf Sparziel einzahlen");
    Console.WriteLine("9) Sparziel löschen");
    Console.WriteLine("10) Kategorie-Limits festlegen");
    Console.WriteLine("");
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

        case "4":
            if (BuchungBearbeiten.Bearbeiten(profil))
                JsonSpeicher.Speichern(profil, pfad);
            break;

        case "5":
            if (BuchungBearbeiten.Loeschen(profil))
                JsonSpeicher.Speichern(profil, pfad);
            break;

        case "6":
            if (SparzielMenue.Erfassen(profil))
                JsonSpeicher.Speichern(profil, pfad);
            break;

        case "7":
            SparzielMenue.Anzeigen(profil);
            break;

        case "8":
            if (SparzielMenue.Einzahlen(profil))
                JsonSpeicher.Speichern(profil, pfad);
            break;

        case "9":
            if (SparzielMenue.Loeschen(profil))
                JsonSpeicher.Speichern(profil, pfad);
            break;

        case "10":
            if (LimitMenue.Festlegen(profil))
                JsonSpeicher.Speichern(profil, pfad);
            break;

        case "/exit":
            AnsiConsole.MarkupLine("[deepskyblue2]Tschüss![/]");
            AnsiConsole.MarkupLine("[red]Programm wird beendet…[/]");
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
