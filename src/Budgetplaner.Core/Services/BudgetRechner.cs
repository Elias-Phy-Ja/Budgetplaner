using Budgetplaner.Core.Models;

namespace Budgetplaner.Core.Services;

public static class BudgetRechner
{
    // Rechnet eine wiederkehrende Buchung auf einen Monat um
    public static decimal ProMonat(Buchung buchung)
    {
        return buchung.Wiederholung switch
        {
            Wiederholung.Woechentlich => buchung.Betrag * 52m / 12m,  // 52 Wochen auf 12 Monate verteilt
            Wiederholung.Monatlich => buchung.Betrag,
            Wiederholung.Jaehrlich => buchung.Betrag / 12m,
            _ => 0m  // Einmalig ist nicht wiederkehrend
        };
    }

    // Berechnet das Budget für den Monat, in dem "monat" liegt
    public static BudgetUebersicht Berechne(BudgetProfil profil, DateTime monat)
    {
        var ersterTag = new DateTime(monat.Year, monat.Month, 1);
        var letzterTag = ersterTag.AddMonths(1).AddDays(-1);

        decimal wiederkehrendeEinnahmen = 0;
        decimal wiederkehrendeAusgaben = 0;
        decimal einmaligeEinnahmen = 0;
        decimal einmaligeAusgaben = 0;

        foreach (var buchung in profil.Buchungen)
        {
            if (buchung.Wiederholung == Wiederholung.Einmalig)
            {
                // Einmalige Buchungen zählen nur in ihrem eigenen Monat
                if (buchung.Datum.Date < ersterTag || buchung.Datum.Date > letzterTag)
                    continue;

                if (buchung.Typ == BuchungsTyp.Einnahme)
                    einmaligeEinnahmen += buchung.Betrag;
                else
                    einmaligeAusgaben += buchung.Betrag;
            }
            else
            {
                // Wiederkehrende Buchung, die erst nach diesem Monat startet, zählt noch nicht
                if (buchung.Datum.Date > letzterTag)
                    continue;

                decimal betrag = ProMonat(buchung);

                if (buchung.Typ == BuchungsTyp.Einnahme)
                    wiederkehrendeEinnahmen += betrag;
                else
                    wiederkehrendeAusgaben += betrag;
            }
        }

        var uebersicht = new BudgetUebersicht
        {
            Monat = ersterTag,
            // erst am Schluss auf Rappen runden, sonst summieren sich Rundungsfehler
            WiederkehrendeEinnahmen = Math.Round(wiederkehrendeEinnahmen, 2),
            WiederkehrendeAusgaben = Math.Round(wiederkehrendeAusgaben, 2),
            EinmaligeEinnahmen = Math.Round(einmaligeEinnahmen, 2),
            EinmaligeAusgaben = Math.Round(einmaligeAusgaben, 2)
        };

        uebersicht.Ampel = BestimmeAmpel(uebersicht.AktuellesBudget, uebersicht.EinnahmenTotal);

        return uebersicht;
    }

    // Ampel-Regeln aus dem README
    public static AmpelStatus BestimmeAmpel(decimal budget, decimal einnahmen)
    {
        if (budget < 0)
            return AmpelStatus.Rot;

        // keine Einnahmen -> kann keine Quote rechnen (Division durch 0!)
        if (einnahmen <= 0)
            return AmpelStatus.Gelb;

        decimal quote = budget / einnahmen;

        return quote >= 0.10m ? AmpelStatus.Gruen : AmpelStatus.Gelb;
    }

    // wie viel pro Kategorie diesen Monat rausgeht, gleiche Regeln wie bei Berechne
    public static Dictionary<string, decimal> AusgabenProKategorie(BudgetProfil profil, DateTime monat)
    {
        var ersterTag = new DateTime(monat.Year, monat.Month, 1);
        var letzterTag = ersterTag.AddMonths(1).AddDays(-1);

        var summen = new Dictionary<string, decimal>();

        foreach (var buchung in profil.Buchungen)
        {
            if (buchung.Typ != BuchungsTyp.Ausgabe)
                continue;

            decimal betrag;

            if (buchung.Wiederholung == Wiederholung.Einmalig)
            {
                if (buchung.Datum.Date < ersterTag || buchung.Datum.Date > letzterTag)
                    continue;
                betrag = buchung.Betrag;
            }
            else
            {
                if (buchung.Datum.Date > letzterTag)
                    continue;
                betrag = ProMonat(buchung);
            }

            // GetValueOrDefault gibt 0 wenn die Kategorie noch nicht drin ist
            summen[buchung.Kategorie] = summen.GetValueOrDefault(buchung.Kategorie) + betrag;
        }

        return summen.ToDictionary(k => k.Key, k => Math.Round(k.Value, 2));
    }
}
