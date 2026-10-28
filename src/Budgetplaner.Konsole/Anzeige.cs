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
}
