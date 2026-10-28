using Budgetplaner.Core.Models;

namespace Budgetplaner.Core.Services;

// Prüft Buchungen und Sparziele, bevor sie gespeichert werden.
// Gibt eine Liste mit Fehlermeldungen zurück. Leere Liste = alles gültig.

public static class Validierung
{
    public static List<string> PruefeBuchung(Buchung buchung)
    {
        var fehler = new List<string>();

        if (string.IsNullOrWhiteSpace(buchung.Titel))
            fehler.Add("Der Titel darf nicht leer sein.");

        if (buchung.Betrag <= 0)
            fehler.Add("Der Betrag muss grösser als 0 sein.");
        else if (!HatHoechstensZweiNachkommastellen(buchung.Betrag))
            fehler.Add("Der Betrag darf höchstens zwei Nachkommastellen haben.");

        return fehler;
    }

    public static List<string> PruefeSparziel(Sparziel sparziel)
    {
        var fehler = new List<string>();

        if (string.IsNullOrWhiteSpace(sparziel.Name))
            fehler.Add("Der Name des Sparziels darf nicht leer sein.");

        if (sparziel.Zielbetrag <= 0)
            fehler.Add("Der Zielbetrag muss grösser als 0 sein.");
        else if (!HatHoechstensZweiNachkommastellen(sparziel.Zielbetrag))
            fehler.Add("Der Zielbetrag darf höchstens zwei Nachkommastellen haben.");

        if (sparziel.BereitsGespart < 0)
            fehler.Add("Der bereits gesparte Betrag darf nicht negativ sein.");

        // Zieldatum ist optional, wenn gesetzt muss es nach heute liegen
        if (sparziel.Zieldatum.HasValue && sparziel.Zieldatum.Value.Date <= DateTime.Today)
            fehler.Add("Das Zieldatum muss in der Zukunft liegen.");

        return fehler;
    }

    // 12.50 bleibt nach dem Runden 12.50 → ok; 12.505 wird zu 12.50 → nicht ok
    private static bool HatHoechstensZweiNachkommastellen(decimal betrag)
        => Math.Round(betrag, 2) == betrag;
}
