using Budgetplaner.Core.Models;
using Budgetplaner.Core.Services;
using Spectre.Console;

namespace Budgetplaner.Konsole;

// Einrichtungsassistent: führt in 4 Schritten durch die wichtigsten Eckwerte
public static class Assistent
{
    private enum Navigation { Weiter, Zurueck, Abbrechen }

    // alles wird zuerst hier gesammelt und kommt erst beim Speichern ins Profil
    private class Entwurf
    {
        public List<Buchung> Einnahmen { get; } = new();
        public List<Buchung> Fixkosten { get; } = new();
        public Sparziel? Sparziel { get; set; }
    }

    private static readonly string[] SchrittNamen = { "Einnahmen", "Fixkosten", "Sparziel", "Zusammenfassung" };

    private const string OptWeiter = "Weiter →";
    private const string OptZurueck = "← Zurück";
    private const string OptEntfernen = "✘ Eintrag entfernen";
    private const string OptAndere = "+ Andere …";
    private const string OptAbbrechen = "Assistent abbrechen";

    // Vorschläge, damit man nicht alles selber tippen muss (Titel, Kategorie)
    private static readonly (string Titel, string Kategorie)[] EinnahmenVorschlaege =
    {
        ("Lohn", "Lohn"),
        ("Taschengeld", "Taschengeld"),
        ("Stipendium", "Sonstiges")
    };

    private static readonly (string Titel, string Kategorie)[] FixkostenVorschlaege =
    {
        ("Handy-Abo", "Kommunikation"),
        ("ÖV-Abo", "Mobilität"),
        ("Krankenkasse", "Versicherung"),
        ("Miete / Kostgeld", "Wohnen"),
        ("Streaming", "Freizeit"),
        ("Fitness / Verein", "Freizeit")
    };

    // true = es wurde etwas gespeichert
    public static bool Starten(BudgetProfil profil, bool ersterStart)
    {
        if (!Begruessung(ersterStart))
            return false;

        var entwurf = new Entwurf();
        int schritt = 0;

        // Schritte vor und zurück, bis gespeichert oder abgebrochen wird
        while (true)
        {
            Navigation nav = schritt switch
            {
                0 => SchrittBuchungen(profil, entwurf, schritt, BuchungsTyp.Einnahme, entwurf.Einnahmen, EinnahmenVorschlaege,
                        "Welche regelmässigen Einnahmen hast du?",
                        "Lohn, Taschengeld … alles, was regelmässig reinkommt."),
                1 => SchrittBuchungen(profil, entwurf, schritt, BuchungsTyp.Ausgabe, entwurf.Fixkosten, FixkostenVorschlaege,
                        "Welche Fixkosten hast du?",
                        "Alles, was jeden Monat (oder jedes Jahr) sicher rausgeht."),
                2 => SchrittSparziel(profil, entwurf, schritt),
                _ => SchrittZusammenfassung(profil, entwurf, schritt)
            };

            if (nav == Navigation.Abbrechen)
                return false;

            if (nav == Navigation.Zurueck)
            {
                schritt = Math.Max(0, schritt - 1);
                continue;
            }

            // Weiter auf der Zusammenfassung = speichern
            if (schritt == SchrittNamen.Length - 1)
            {
                profil.Buchungen.AddRange(entwurf.Einnahmen);
                profil.Buchungen.AddRange(entwurf.Fixkosten);
                if (entwurf.Sparziel != null)
                    profil.Sparziele.Add(entwurf.Sparziel);

                Console.Clear();
                AnsiConsole.MarkupLine("[bold green]✔ Alles gespeichert![/]");
                AnsiConsole.MarkupLine("[grey]Im Hauptmenü kannst du jederzeit Buchungen ergänzen oder ändern.[/]");
                return true;
            }

            schritt++;
        }
    }

    // false = übersprungen
    private static bool Begruessung(bool ersterStart)
    {
        Console.Clear();

        if (ersterStart)
        {
            AnsiConsole.Write(new FigletText("Budgetplaner").Color(Color.DeepSkyBlue2));
            AnsiConsole.MarkupLine("[bold]Hoi! Schön, dass du da bist.[/]");
            Console.WriteLine("");
            AnsiConsole.MarkupLine("In [bold]4 kurzen Schritten[/] richten wir dein Budget ein:");
        }
        else
        {
            AnsiConsole.MarkupLine("[bold deepskyblue2]Einrichtungsassistent[/]");
            Console.WriteLine("");
            AnsiConsole.MarkupLine("[grey]Was du hier erfasst, kommt zu deinen bestehenden Einträgen dazu.[/]");
        }

        Console.WriteLine("");
        AnsiConsole.MarkupLine("  1. Einnahmen      [grey]was regelmässig reinkommt[/]");
        AnsiConsole.MarkupLine("  2. Fixkosten      [grey]was regelmässig rausgeht[/]");
        AnsiConsole.MarkupLine("  3. Sparziel       [grey]optional[/]");
        AnsiConsole.MarkupLine("  4. Zusammenfassung mit Ampel");
        Console.WriteLine("");
        AnsiConsole.MarkupLine("[grey]Du kannst jederzeit zurück, gespeichert wird erst ganz am Schluss.[/]");
        Console.WriteLine("");

        string wahl = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .AddChoices("Los geht's →", ersterStart ? "Überspringen (geht später im Menü)" : "Zurück zum Menü"));

        return wahl == "Los geht's →";
    }

    // Kopfzeile mit Fortschritt und Live-Vorschau vom Budget
    private static void Kopf(BudgetProfil profil, Entwurf entwurf, int schritt)
    {
        Console.Clear();
        AnsiConsole.Write(new Rule($"[bold deepskyblue2]Einrichtung · Schritt {schritt + 1} von {SchrittNamen.Length}[/]") { Justification = Justify.Left });

        // ● erledigt  ◉ aktuell  ○ kommt noch
        var punkte = SchrittNamen.Select((name, i) =>
            i < schritt ? $"[green]● {name}[/]" :
            i == schritt ? $"[bold deepskyblue2]◉ {name}[/]" :
            $"[grey]○ {name}[/]");

        AnsiConsole.MarkupLine(string.Join("  [grey]›[/]  ", punkte));
        Console.WriteLine("");

        // ab Schritt 2 sieht man live, wie das Budget aussieht
        if (schritt > 0)
        {
            var u = BudgetRechner.Berechne(Vorschau(profil, entwurf), DateTime.Today);
            AnsiConsole.MarkupLine(
                $"[grey]Pro Monat:[/] rein {Anzeige.Chf(u.WiederkehrendeEinnahmen)}  [grey]·[/]  raus {Anzeige.Chf(u.WiederkehrendeAusgaben)}" +
                $"  [grey]·[/]  übrig [bold]{Anzeige.Chf(u.Ueberschuss)}[/]  {Anzeige.AmpelText(u.Ampel)}");
            Console.WriteLine("");
        }
    }

    // bestehendes Profil + Entwurf zusammen, nur zum Rechnen (wird nicht gespeichert)
    private static BudgetProfil Vorschau(BudgetProfil profil, Entwurf entwurf)
    {
        var vorschau = new BudgetProfil
        {
            Buchungen = profil.Buchungen.Concat(entwurf.Einnahmen).Concat(entwurf.Fixkosten).ToList(),
            Sparziele = profil.Sparziele.ToList(),
            KategorieLimits = profil.KategorieLimits
        };

        if (entwurf.Sparziel != null)
            vorschau.Sparziele.Add(entwurf.Sparziel);

        return vorschau;
    }

    // Schritt 1 und 2 funktionieren gleich, nur mit anderen Vorschlägen
    private static Navigation SchrittBuchungen(BudgetProfil profil, Entwurf entwurf, int schritt, BuchungsTyp typ,
        List<Buchung> liste, (string Titel, string Kategorie)[] vorschlaege, string frage, string hinweis)
    {
        while (true)
        {
            Kopf(profil, entwurf, schritt);

            AnsiConsole.MarkupLine($"[bold]{frage}[/]");
            AnsiConsole.MarkupLine($"[grey]{hinweis}[/]");
            Console.WriteLine("");

            ZeigeListe(liste);

            // Vorschläge, die schon drin sind, nicht nochmal anbieten
            var choices = vorschlaege
                .Where(v => !liste.Any(b => b.Titel == v.Titel))
                .Select(v => v.Titel)
                .ToList();

            choices.Add(OptAndere);
            if (liste.Count > 0)
                choices.Add(OptEntfernen);
            choices.Add(OptWeiter);
            if (schritt > 0)
                choices.Add(OptZurueck);
            choices.Add(OptAbbrechen);

            string wahl = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(liste.Count == 0 ? "Wähl etwas aus oder geh weiter:" : "Noch etwas dazu?")
                    .PageSize(12)
                    .MoreChoicesText("[grey](mit ↑↓ mehr anzeigen)[/]")
                    .AddChoices(choices));

            switch (wahl)
            {
                case OptWeiter:
                    // ohne Einnahmen kann man kein Budget rechnen, lieber nachfragen
                    if (typ == BuchungsTyp.Einnahme && liste.Count == 0 &&
                        !Eingabe.JaNein("Ohne Einnahmen kann ich kein Budget rechnen. Trotzdem weiter?"))
                        continue;
                    return Navigation.Weiter;

                case OptZurueck:
                    return Navigation.Zurueck;

                case OptAbbrechen:
                    if (Eingabe.JaNein("Wirklich abbrechen? Deine Eingaben gehen verloren."))
                        return Navigation.Abbrechen;
                    continue;

                case OptEntfernen:
                    Entfernen(liste);
                    continue;

                case OptAndere:
                    string titel = Eingabe.LeseText("Wie heisst es? ").Trim();
                    string kategorie = BuchungErfassen.WaehleKategorie(typ);
                    Hinzufuegen(liste, typ, titel, kategorie);
                    continue;

                default:
                    // ein Vorschlag wurde gewählt
                    var vorschlag = vorschlaege.First(v => v.Titel == wahl);
                    Hinzufuegen(liste, typ, vorschlag.Titel, vorschlag.Kategorie);
                    continue;
            }
        }
    }

    // fragt Betrag + wie oft, prüft und fügt hinzu
    private static void Hinzufuegen(List<Buchung> liste, BuchungsTyp typ, string titel, string kategorie)
    {
        AnsiConsole.MarkupLine($"[bold]{Markup.Escape(titel)}[/]");
        decimal betrag = Eingabe.LeseBetrag("Betrag in CHF: ");

        // im Assistenten nur wiederkehrend, Monatlich zuerst weils am häufigsten ist
        var wiederholung = AnsiConsole.Prompt(
            new SelectionPrompt<Wiederholung>()
                .Title("Wie oft?")
                .AddChoices(Wiederholung.Monatlich, Wiederholung.Woechentlich, Wiederholung.Jaehrlich)
                .UseConverter(Anzeige.WiederholungText));

        var buchung = new Buchung
        {
            Titel = titel,
            Betrag = betrag,
            Typ = typ,
            Kategorie = kategorie,
            Wiederholung = wiederholung,
            Datum = DateTime.Today
        };

        if (!BuchungErfassen.IstGueltig(buchung))
        {
            AnsiConsole.MarkupLine("[grey]Nicht hinzugefügt. Weiter mit Enter …[/]");
            Console.ReadLine();
            return;
        }

        liste.Add(buchung);
    }

    private static void Entfernen(List<Buchung> liste)
    {
        var abbrechen = new Buchung { Titel = "« Doch nicht" };

        var wahl = AnsiConsole.Prompt(
            new SelectionPrompt<Buchung>()
                .Title("Welchen Eintrag entfernen?")
                .AddChoices(liste)
                .AddChoices(abbrechen)
                .UseConverter(b => b == abbrechen
                    ? "[grey]« Doch nicht[/]"
                    : $"{Markup.Escape(b.Titel)} [grey]({Anzeige.Chf(b.Betrag)}, {Anzeige.WiederholungText(b.Wiederholung)})[/]"));

        if (wahl != abbrechen)
            liste.Remove(wahl);
    }

    // kleine Tabelle mit dem, was schon erfasst ist
    private static void ZeigeListe(List<Buchung> liste)
    {
        if (liste.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]  (noch nichts erfasst)[/]");
            Console.WriteLine("");
            return;
        }

        var tabelle = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Titel")
            .AddColumn("Kategorie")
            .AddColumn("Wie oft")
            .AddColumn(new TableColumn("Betrag").RightAligned())
            .AddColumn(new TableColumn("pro Monat").RightAligned());

        foreach (var b in liste)
        {
            tabelle.AddRow(
                Markup.Escape(b.Titel),
                Markup.Escape(b.Kategorie),
                Anzeige.WiederholungText(b.Wiederholung),
                Anzeige.Chf(b.Betrag),
                $"[bold]{Anzeige.Chf(Math.Round(BudgetRechner.ProMonat(b), 2))}[/]");
        }

        AnsiConsole.Write(tabelle);
        Console.WriteLine("");
    }

    private static Navigation SchrittSparziel(BudgetProfil profil, Entwurf entwurf, int schritt)
    {
        const string OptFestlegen = "Sparziel festlegen";
        const string OptAendern = "Sparziel ändern";
        const string OptOhne = "Ohne Sparziel weiter →";
        const string OptLoeschen = "✘ Sparziel entfernen";

        while (true)
        {
            Kopf(profil, entwurf, schritt);

            AnsiConsole.MarkupLine("[bold]Willst du auf etwas sparen?[/]");
            AnsiConsole.MarkupLine("[grey]Optional – z.B. neuer Laptop, Töffli oder Ferien. Geht auch später noch.[/]");
            Console.WriteLine("");

            var choices = new List<string>();

            if (entwurf.Sparziel == null)
            {
                choices.Add(OptFestlegen);
                choices.Add(OptOhne);
            }
            else
            {
                var z = entwurf.Sparziel;
                AnsiConsole.MarkupLine($"  [green]●[/] [bold]{Markup.Escape(z.Name)}[/]: {Anzeige.Chf(z.Zielbetrag)}" +
                    (z.BereitsGespart > 0 ? $" [grey](schon {Anzeige.Chf(z.BereitsGespart)} gespart)[/]" : "") +
                    (z.Zieldatum.HasValue ? $" [grey]bis {Anzeige.Datum(z.Zieldatum.Value)}[/]" : ""));
                Console.WriteLine("");

                choices.Add(OptWeiter);
                choices.Add(OptAendern);
                choices.Add(OptLoeschen);
            }

            choices.Add(OptZurueck);
            choices.Add(OptAbbrechen);

            string wahl = AnsiConsole.Prompt(new SelectionPrompt<string>().AddChoices(choices));

            switch (wahl)
            {
                case OptFestlegen:
                case OptAendern:
                    var ziel = new Sparziel
                    {
                        Name = Eingabe.LeseText("Wofür sparst du? ").Trim(),
                        Zielbetrag = Eingabe.LeseBetrag("Wie viel brauchst du? CHF "),
                        BereitsGespart = Eingabe.LeseBetragOptional("Schon gespart? (leer = 0) CHF ", 0),
                        Zieldatum = Eingabe.LeseDatumOptional("Bis wann? (TT.MM.JJJJ, leer = egal): ")
                    };

                    var fehler = Validierung.PruefeSparziel(ziel);
                    if (fehler.Count > 0)
                    {
                        foreach (var f in fehler)
                            Anzeige.Fehler("✘ " + f);
                        AnsiConsole.MarkupLine("[grey]Weiter mit Enter …[/]");
                        Console.ReadLine();
                    }
                    else
                    {
                        entwurf.Sparziel = ziel;
                    }
                    continue;

                case OptLoeschen:
                    entwurf.Sparziel = null;
                    continue;

                case OptOhne:
                case OptWeiter:
                    return Navigation.Weiter;

                case OptZurueck:
                    return Navigation.Zurueck;

                default:
                    if (Eingabe.JaNein("Wirklich abbrechen? Deine Eingaben gehen verloren."))
                        return Navigation.Abbrechen;
                    continue;
            }
        }
    }

    private static Navigation SchrittZusammenfassung(BudgetProfil profil, Entwurf entwurf, int schritt)
    {
        const string OptSpeichern = "✔ Speichern und fertig";

        while (true)
        {
            Kopf(profil, entwurf, schritt);

            var vorschau = Vorschau(profil, entwurf);
            var u = BudgetRechner.Berechne(vorschau, DateTime.Today);

            AnsiConsole.MarkupLine("[bold]Einnahmen[/]");
            ZeigeListe(entwurf.Einnahmen);
            AnsiConsole.MarkupLine("[bold]Fixkosten[/]");
            ZeigeListe(entwurf.Fixkosten);

            // das Wichtigste gross
            var grid = new Grid().AddColumn().AddColumn(new GridColumn().RightAligned());
            grid.AddRow("Einnahmen pro Monat", Anzeige.Chf(u.WiederkehrendeEinnahmen));
            grid.AddRow("Fixkosten pro Monat", "- " + Anzeige.Chf(u.WiederkehrendeAusgaben));
            grid.AddRow("[bold]Bleibt dir pro Monat[/]", $"[bold]{Anzeige.Chf(u.Ueberschuss)}[/]");
            grid.AddRow("Ampel", Anzeige.AmpelText(u.Ampel));

            AnsiConsole.Write(new Panel(grid).Header(" Dein Budget ").Border(BoxBorder.Rounded));

            if (entwurf.Sparziel != null)
            {
                var p = SparzielRechner.Berechne(entwurf.Sparziel, u.Ueberschuss, DateTime.Today);
                string prognose = p.Prognosedatum.HasValue && p.Status != SparzielStatus.Erreicht
                    ? $"erreicht ca. [bold]{Anzeige.Monat(p.Prognosedatum.Value)}[/]"
                    : Anzeige.SparzielStatusText(p.Status);

                AnsiConsole.MarkupLine($"Sparziel [bold]{Markup.Escape(entwurf.Sparziel.Name)}[/]: {prognose}");
            }

            // die 3 wichtigsten Sprüche vom Motivator
            Console.WriteLine("");
            foreach (var n in Motivator.Nachrichten(vorschau, DateTime.Today).Take(3))
                Anzeige.ZeigeNachricht(n);
            Console.WriteLine("");

            string wahl = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Passt alles?")
                    .AddChoices(OptSpeichern, OptZurueck, OptAbbrechen));

            switch (wahl)
            {
                case OptSpeichern:
                    return Navigation.Weiter;

                case OptZurueck:
                    return Navigation.Zurueck;

                default:
                    if (Eingabe.JaNein("Wirklich abbrechen? Deine Eingaben gehen verloren."))
                        return Navigation.Abbrechen;
                    continue;
            }
        }
    }
}
