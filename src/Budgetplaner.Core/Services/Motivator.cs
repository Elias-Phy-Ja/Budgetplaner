using System.Globalization;
using Budgetplaner.Core.Models;

namespace Budgetplaner.Core.Services;

// sucht anhand der echten Zahlen passende Sprüche raus (loben, warnen, feiern)
public static class Motivator
{
    private static readonly CultureInfo Schweiz = new("de-CH");
    private static readonly int[] Meilensteine = { 25, 50, 75, 100 };

    public static List<Nachricht> Nachrichten(BudgetProfil profil, DateTime heute)
    {
        var liste = new List<Nachricht>();

        // ohne Buchungen gibts nichts zu sagen
        if (profil.Buchungen.Count == 0)
        {
            liste.Add(new(NachrichtArt.Hinweis, "Erfasse zuerst deine Einnahmen und Fixkosten, dann sag ich dir, wie du dastehst."));
            return liste;
        }

        var u = BudgetRechner.Berechne(profil, heute);
        var ausgaben = BudgetRechner.AusgabenProKategorie(profil, heute);

        PruefeAmpel(u, ausgaben, liste);
        PruefeLimits(profil, ausgaben, liste);
        PruefeSparziele(profil, u.Ueberschuss, heute, liste);

        int streak = PlusStreak(profil, heute);
        if (streak >= 2)
            liste.Add(new(NachrichtArt.Lob, $"🔥 {streak} Monate in Folge im Plus!"));

        // wichtigstes zuerst (Reihenfolge vom Enum)
        return liste.OrderBy(n => n.Art).ToList();
    }

    // nach dem Einzahlen: wurde gerade 25/50/75/100% geknackt? null = nein
    public static Nachricht? NachEinzahlung(Sparziel ziel, decimal vorherGespart)
    {
        if (ziel.Zielbetrag <= 0)
            return null;

        decimal vorher = vorherGespart / ziel.Zielbetrag * 100;
        decimal jetzt = ziel.BereitsGespart / ziel.Zielbetrag * 100;

        // den höchsten Meilenstein nehmen, falls man mehrere auf einmal überspringt
        int? geknackt = null;
        foreach (int m in Meilensteine)
        {
            if (vorher < m && jetzt >= m)
                geknackt = m;
        }

        if (geknackt == null)
            return null;

        if (geknackt == 100)
            return new(NachrichtArt.Feier, $"🎉 Geschafft! „{ziel.Name}“ ist komplett gespart!");

        return new(NachrichtArt.Feier, $"🎉 {geknackt} % von „{ziel.Name}“ geschafft, weiter so!");
    }

    // wie viele Monate am Stück (bis heute) das Budget im Plus war
    public static int PlusStreak(BudgetProfil profil, DateTime heute)
    {
        if (profil.Buchungen.Count == 0)
            return 0;

        // weiter zurück als die erste Buchung macht keinen Sinn
        var erste = profil.Buchungen.Min(b => b.Datum);
        var startMonat = new DateTime(erste.Year, erste.Month, 1);
        var monat = new DateTime(heute.Year, heute.Month, 1);

        int streak = 0;

        // max. 10 Jahre zurück, sicher ist sicher
        while (monat >= startMonat && streak < 120)
        {
            if (BudgetRechner.Berechne(profil, monat).AktuellesBudget <= 0)
                break;

            streak++;
            monat = monat.AddMonths(-1);
        }

        return streak;
    }

    private static void PruefeAmpel(BudgetUebersicht u, Dictionary<string, decimal> ausgaben, List<Nachricht> liste)
    {
        string quote = u.Sparquote.ToString("P0", Schweiz);

        switch (u.Ampel)
        {
            case AmpelStatus.Rot:
                liste.Add(new(NachrichtArt.Warnung, $"Dein Budget ist diesen Monat {Chf(-u.AktuellesBudget)} im Minus."));

                // konkreter Tipp: wo geht am meisten Geld hin?
                if (ausgaben.Count > 0)
                {
                    var groesste = ausgaben.MaxBy(k => k.Value);
                    liste.Add(new(NachrichtArt.Hinweis, $"Am meisten gibst du für {groesste.Key} aus ({Chf(groesste.Value)}). Da lässt sich vielleicht was sparen."));
                }
                break;

            case AmpelStatus.Gelb:
                if (u.EinnahmenTotal <= 0)
                    liste.Add(new(NachrichtArt.Hinweis, "Du hast diesen Monat noch keine Einnahmen erfasst."));
                else
                    liste.Add(new(NachrichtArt.Hinweis, $"Es wird knapp: nur {quote} deiner Einnahmen bleiben übrig. Gut wären mind. 10 %."));
                break;

            case AmpelStatus.Gruen:
                if (u.Sparquote >= 0.30m)
                    liste.Add(new(NachrichtArt.Lob, $"Wow! Du sparst {quote} deiner Einnahmen. Weiter so!"));
                else
                    liste.Add(new(NachrichtArt.Lob, $"Stark! Diesen Monat bleiben {quote} deiner Einnahmen übrig."));
                break;
        }
    }

    private static void PruefeLimits(BudgetProfil profil, Dictionary<string, decimal> ausgaben, List<Nachricht> liste)
    {
        foreach (var (kategorie, limit) in profil.KategorieLimits)
        {
            if (limit <= 0)
                continue;

            decimal ausgegeben = ausgaben.GetValueOrDefault(kategorie);

            if (ausgegeben > limit)
                liste.Add(new(NachrichtArt.Warnung, $"{kategorie} liegt {Chf(ausgegeben - limit)} über deinem Limit von {Chf(limit)}."));
            else if (ausgegeben >= limit * 0.8m)
                liste.Add(new(NachrichtArt.Hinweis, $"{kategorie}: schon {(ausgegeben / limit).ToString("P0", Schweiz)} von deinem Limit verbraucht."));
        }
    }

    private static void PruefeSparziele(BudgetProfil profil, decimal ueberschuss, DateTime heute, List<Nachricht> liste)
    {
        foreach (var ziel in profil.Sparziele)
        {
            var p = SparzielRechner.Berechne(ziel, ueberschuss, heute);

            switch (p.Status)
            {
                case SparzielStatus.Erreicht:
                    liste.Add(new(NachrichtArt.Feier, $"Ziel „{ziel.Name}“ erreicht! 🎉"));
                    break;

                case SparzielStatus.NichtErreichbar:
                    liste.Add(new(NachrichtArt.Warnung, $"„{ziel.Name}“ ist mit deinem aktuellen Budget nicht erreichbar, dafür brauchst du einen Überschuss."));
                    break;

                case SparzielStatus.NichtRealistisch:
                    decimal fehlt = p.NoetigProMonat!.Value - ueberschuss;
                    liste.Add(new(NachrichtArt.Hinweis, $"Für „{ziel.Name}“ bis {ziel.Zieldatum:dd.MM.yyyy} fehlen dir {Chf(fehlt)} pro Monat. Spar mehr oder verschieb das Datum."));
                    break;

                case SparzielStatus.AufKurs:
                    if (p.Fortschritt >= 0.75m)
                        liste.Add(new(NachrichtArt.Lob, $"Fast geschafft: „{ziel.Name}“ ist zu {p.Fortschritt.ToString("P0", Schweiz)} voll!"));
                    break;
            }
        }
    }

    private static string Chf(decimal betrag) => "CHF " + betrag.ToString("N2", Schweiz);
}
