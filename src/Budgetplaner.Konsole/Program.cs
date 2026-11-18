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

// Menüpunkte als Konstanten, so heissen Menü und switch sicher gleich
const string BuchungErfassenText = "Buchung erfassen";
const string BuchungenAnzeigenText = "Alle Buchungen anzeigen";
const string BuchungBearbeitenText = "Buchung bearbeiten";
const string BuchungLoeschenText = "Buchung löschen";
const string UebersichtText = "Budget-Übersicht";
const string LimitsText = "Kategorie-Limits festlegen";
const string SparzielErfassenText = "Sparziel erfassen";
const string SparzieleAnzeigenText = "Sparziele anzeigen";
const string EinzahlenText = "Auf Sparziel einzahlen";
const string SparzielLoeschenText = "Sparziel löschen";
const string AssistentText = "Einrichtungsassistent starten";
const string BeendenText = "Beenden";

// erster Start (noch gar nichts drin) gleich mit dem Assistenten loslegen
if (profil.Buchungen.Count == 0 && profil.Sparziele.Count == 0)
{
    if (Assistent.Starten(profil, ersterStart: true))
    {
        JsonSpeicher.Speichern(profil, pfad);
        WarteAufEnter();
    }
}

bool laeuft = true;

while (laeuft)
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

    // Menü mit Pfeiltasten statt Nummern eintippen, Gruppen nur als Überschrift
    string auswahl = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Was möchtest du machen?")
            .PageSize(20)
            .Mode(SelectionMode.Leaf)
            .AddChoiceGroup("Buchungen", BuchungErfassenText, BuchungenAnzeigenText, BuchungBearbeitenText, BuchungLoeschenText)
            .AddChoiceGroup("Budget", UebersichtText, LimitsText)
            .AddChoiceGroup("Sparziele", SparzielErfassenText, SparzieleAnzeigenText, EinzahlenText, SparzielLoeschenText)
            .AddChoiceGroup("Sonstiges", AssistentText, BeendenText));

    Console.Clear();

    // bei allem was etwas ändert: nur speichern, wenn wirklich was geändert wurde
    switch (auswahl)
    {
        case BuchungErfassenText:
            if (BuchungErfassen.Ausfuehren(profil))
                JsonSpeicher.Speichern(profil, pfad);
            break;

        case BuchungenAnzeigenText:
            BuchungenAnzeigen.Ausfuehren(profil);
            break;

        case BuchungBearbeitenText:
            if (BuchungBearbeiten.Bearbeiten(profil))
                JsonSpeicher.Speichern(profil, pfad);
            break;

        case BuchungLoeschenText:
            if (BuchungBearbeiten.Loeschen(profil))
                JsonSpeicher.Speichern(profil, pfad);
            break;

        case UebersichtText:
            BudgetAnzeigen.Ausfuehren(profil);
            break;

        case LimitsText:
            if (LimitMenue.Festlegen(profil))
                JsonSpeicher.Speichern(profil, pfad);
            break;

        case SparzielErfassenText:
            if (SparzielMenue.Erfassen(profil))
                JsonSpeicher.Speichern(profil, pfad);
            break;

        case SparzieleAnzeigenText:
            SparzielMenue.Anzeigen(profil);
            break;

        case EinzahlenText:
            if (SparzielMenue.Einzahlen(profil))
                JsonSpeicher.Speichern(profil, pfad);
            break;

        case SparzielLoeschenText:
            if (SparzielMenue.Loeschen(profil))
                JsonSpeicher.Speichern(profil, pfad);
            break;

        case AssistentText:
            if (Assistent.Starten(profil, ersterStart: false))
                JsonSpeicher.Speichern(profil, pfad);
            else
                continue; // abgebrochen direkt zurück ins Menü
            break;

        case BeendenText:
            AnsiConsole.MarkupLine("[deepskyblue2]Tschüss![/]");
            AnsiConsole.MarkupLine("[red]Programm wird beendet…[/]");
            laeuft = false;
            break;
    }

    // Warten, damit man das Ergebnis lesen kann, bevor das Menü wieder kommt
    if (laeuft)
        WarteAufEnter();
}

static void WarteAufEnter()
{
    Console.WriteLine("");
    AnsiConsole.MarkupLine("[grey]Weiter mit Enter …[/]");
    Console.ReadLine();
}
