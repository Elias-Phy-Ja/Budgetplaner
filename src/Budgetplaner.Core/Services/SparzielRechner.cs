using Budgetplaner.Core.Models;

namespace Budgetplaner.Core.Services;

public static class SparzielRechner
{
    // mehr als 100 Jahre zählt als "nie", sonst crasht AddMonths
    private const int MaxMonate = 1200;

    // Überschuss direkt aus dem Profil holen
    public static SparzielPrognose Berechne(Sparziel ziel, BudgetProfil profil, DateTime heute)
    {
        decimal ueberschuss = BudgetRechner.Berechne(profil, heute).Ueberschuss;
        return Berechne(ziel, ueberschuss, heute);
    }

    // Überschuss als Parameter, so kann man es gut testen
    public static SparzielPrognose Berechne(Sparziel ziel, decimal ueberschussProMonat, DateTime heute)
    {
        heute = heute.Date;

        var prognose = new SparzielPrognose
        {
            Restbetrag = Math.Max(0, ziel.Zielbetrag - ziel.BereitsGespart),
            Fortschritt = ziel.Zielbetrag > 0 ? Math.Min(1, ziel.BereitsGespart / ziel.Zielbetrag) : 0
        };

        // schon geschafft
        if (prognose.Restbetrag == 0)
        {
            prognose.Status = SparzielStatus.Erreicht;
            prognose.MonateBisZiel = 0;
            prognose.Prognosedatum = heute;
            return prognose;
        }

        // Frage 2: wie viel muss ich pro Monat sparen, um das Zieldatum zu schaffen?
        if (ziel.Zieldatum.HasValue)
        {
            // mind. 1 Monat, auch wenn das Datum diesen Monat oder schon vorbei ist
            int monate = Math.Max(1, MonateZwischen(heute, ziel.Zieldatum.Value));
            prognose.NoetigProMonat = AufRappenAufrunden(prognose.Restbetrag / monate);
        }

        // kein Überschuss = nicht durch 0 teilen, Ziel geht so einfach nicht
        if (ueberschussProMonat <= 0)
        {
            prognose.Status = SparzielStatus.NichtErreichbar;
            return prognose;
        }

        // Frage 1: bis wann ist das Ziel erreicht?
        decimal monateGenau = prognose.Restbetrag / ueberschussProMonat;

        if (monateGenau > MaxMonate)
        {
            prognose.Status = SparzielStatus.NichtErreichbar;
            return prognose;
        }

        // aufrunden: 3.2 Monate heisst, im 4. Monat ist es geschafft
        int monateBisZiel = (int)Math.Ceiling(monateGenau);
        prognose.MonateBisZiel = monateBisZiel;
        prognose.Prognosedatum = heute.AddMonths(monateBisZiel);

        // reicht der Überschuss für das Zieldatum?
        if (prognose.NoetigProMonat.HasValue && prognose.NoetigProMonat.Value > ueberschussProMonat)
            prognose.Status = SparzielStatus.NichtRealistisch;
        else
            prognose.Status = SparzielStatus.AufKurs;

        return prognose;
    }

    // Kalendermonate dazwischen, z.B. 11.11.2026 -> 15.03.2027 = 4
    public static int MonateZwischen(DateTime von, DateTime bis)
    {
        return (bis.Year - von.Year) * 12 + (bis.Month - von.Month);
    }

    // 33.333 -> 33.34, sonst fehlt am Schluss ein Rappen
    private static decimal AufRappenAufrunden(decimal betrag)
    {
        return Math.Ceiling(betrag * 100) / 100;
    }
}
