using System.Globalization;
using Spectre.Console;

namespace Budgetplaner.Konsole;

// Einlese-Helfer, die Erfassen und Bearbeiten beide brauchen
public static class Eingabe
{
    // fragt so lange, bis was drinsteht
    public static string LeseText(string frage)
    {
        while (true)
        {
            AnsiConsole.Markup($"[purple]{Markup.Escape(frage)}[/]");
            string? eingabe = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(eingabe))
                return eingabe;

            Anzeige.Fehler("Dieser Input ist leer!");
        }
    }

    // fragt bis eine Zahl kommt, ob > 0 checkt danach die Validierung
    public static decimal LeseBetrag(string frage)
    {
        while (true)
        {
            string eingabe = LeseText(frage);

            // 12,50 und 12.50 sollen beide gehen
            string normalisiert = eingabe.Trim().Replace(',', '.');

            if (decimal.TryParse(normalisiert, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal betrag))
                return betrag;

            Anzeige.Fehler("Das ist keine Zahl. Beispiel: 12.50");
        }
    }

    // leer = heute
    public static DateTime LeseDatum(string frage)
    {
        string[] formate = { "d.M.yyyy", "dd.MM.yyyy" };

        while (true)
        {
            AnsiConsole.Markup($"[purple]{Markup.Escape(frage)}[/]");
            string eingabe = (Console.ReadLine() ?? "").Trim();

            if (eingabe == "")
                return DateTime.Today;

            if (DateTime.TryParseExact(eingabe, formate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime datum))
                return datum;

            Anzeige.Fehler("Ungültiges Datum. Beispiel: 28.10.2026");
        }
    }

    // leer = standard (z.B. 0 bei "schon gespart")
    public static decimal LeseBetragOptional(string frage, decimal standard)
    {
        while (true)
        {
            AnsiConsole.Markup($"[purple]{Markup.Escape(frage)}[/]");
            string eingabe = (Console.ReadLine() ?? "").Trim();

            if (eingabe == "")
                return standard;

            if (decimal.TryParse(eingabe.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal betrag))
                return betrag;

            Anzeige.Fehler("Das ist keine Zahl. Beispiel: 12.50");
        }
    }

    // leer = kein Datum (null)
    public static DateTime? LeseDatumOptional(string frage)
    {
        string[] formate = { "d.M.yyyy", "dd.MM.yyyy" };

        while (true)
        {
            AnsiConsole.Markup($"[purple]{Markup.Escape(frage)}[/]");
            string eingabe = (Console.ReadLine() ?? "").Trim();

            if (eingabe == "")
                return null;

            if (DateTime.TryParseExact(eingabe, formate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime datum))
                return datum;

            Anzeige.Fehler("Ungültiges Datum. Beispiel: 15.03.2027");
        }
    }
}
