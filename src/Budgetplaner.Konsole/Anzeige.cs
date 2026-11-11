using System.Globalization;
using Budgetplaner.Core.Models;

namespace Budgetplaner.Konsole;

// Kleine Helfer, damit Beträge und Enums überall gleich angezeigt werden
public static class Anzeige
{
    private static readonly CultureInfo Schweiz = new("de-CH");

    // 1234.5 -> "CHF 1’234.50"
    public static string Chf(decimal betrag) => "CHF " + betrag.ToString("N2", Schweiz);

    public static string Monat(DateTime datum) => datum.ToString("MMMM yyyy", Schweiz);

    public static string Datum(DateTime datum) => datum.ToString("dd.MM.yyyy");

    public static string WiederholungText(Wiederholung w) => w switch
    {
        Wiederholung.Einmalig => "Einmalig",
        Wiederholung.Woechentlich => "Wöchentlich",
        Wiederholung.Monatlich => "Monatlich",
        Wiederholung.Jaehrlich => "Jährlich",
        _ => w.ToString()
    };

    public static void Fehler(string text) =>
        Spectre.Console.AnsiConsole.MarkupLine($"[red]{Spectre.Console.Markup.Escape(text)}[/]");

    // farbiger Text je nach Status
    public static string SparzielStatusText(SparzielStatus status) => status switch
    {
        SparzielStatus.Erreicht => "[bold green]🎉 Erreicht![/]",
        SparzielStatus.AufKurs => "[green]Auf Kurs[/]",
        SparzielStatus.NichtRealistisch => "[yellow]Zieldatum so nicht realistisch[/]",
        SparzielStatus.NichtErreichbar => "[red]Mit dem aktuellen Budget nicht erreichbar[/]",
        _ => status.ToString()
    };

    // [██████░░░░░░] als Text, breite = Anzahl Zeichen
    public static string Fortschrittsbalken(decimal anteil, int breite = 20)
    {
        int voll = (int)Math.Round(anteil * breite);
        voll = Math.Clamp(voll, 0, breite);

        return "[green]" + new string('█', voll) + "[/][grey]" + new string('░', breite - voll) + "[/]";
    }

    // Motivator-Nachricht in der passenden Farbe
    public static void ZeigeNachricht(Nachricht n)
    {
        string text = Spectre.Console.Markup.Escape(n.Text);

        string zeile = n.Art switch
        {
            NachrichtArt.Warnung => $"[red]⚠  {text}[/]",
            NachrichtArt.Hinweis => $"[yellow]💡 {text}[/]",
            NachrichtArt.Feier => $"[magenta]{text}[/]",
            NachrichtArt.Lob => $"[green]👍 {text}[/]",
            _ => text
        };

        Spectre.Console.AnsiConsole.MarkupLine(zeile);
    }
}
