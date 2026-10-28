# 📁 Projektstruktur Budgetplaner

Überblick über alle Ordner und Dateien im Repository und wofür sie da sind.

```
Budgetplaner/
├── .editorconfig
├── .gitignore
├── Budgetplaner.slnx
├── README.md
├── docs/
│   ├── Ausschreibung_KliLi.docx
│   └── STRUKTUR.md
├── src/
│   ├── Budgetplaner.Core/        ← Logik (ohne Oberfläche)
│   │   ├── Models/
│   │   ├── Services/
│   │   └── Storage/
│   └── Budgetplaner.App/         ← WPF-Oberfläche
│       ├── Converters/
│       ├── Resources/
│       ├── ViewModels/
│       └── Views/
└── tests/
    └── Budgetplaner.Core.Tests/  ← Unit-Tests für die Logik
```

**Grundidee:** `Core` rechnet, `App` zeigt an, `Tests` prüfen die Rechnungen.
Die App kennt Core, aber Core kennt die App nicht. Dadurch lässt sich die ganze Logik ohne Fenster testen.

---

## Root (Hauptordner)

| Name | Typ | Erklärung |
| --- | --- | --- |
| `.editorconfig` | Datei | Formatierungsregeln für C# (Einrückung, Leerzeichen, Namenskonventionen). Visual Studio und VS Code halten sich automatisch daran. |
| `.gitignore` | Datei | Sagt Git, welche Dateien **nicht** hochgeladen werden: `bin/`, `obj/`, `.vs/` und Benutzereinstellungen. |
| `Budgetplaner.slnx` | Datei | Die **Solution**: fasst alle drei Projekte zusammen. Diese Datei öffnest du in Visual Studio. |
| `README.md` | Datei | Projektbeschreibung, Anforderungen, Architektur und Zeitplan. Wird auf GitHub als Startseite angezeigt. |
| `docs/` | Ordner | Dokumentation, die nicht zum Code gehört. |
| `src/` | Ordner | „Source“: der eigentliche Programmcode. |
| `tests/` | Ordner | Automatische Tests. |

## docs/

| Name | Typ | Erklärung |
| --- | --- | --- |
| `Ausschreibung_KliLi.docx` | Datei | Original-Ausschreibung „Vorausschauende Budgetplanung“. |
| `STRUKTUR.md` | Datei | Diese Datei. |

---

## src/Budgetplaner.Core/ – die Logik

Klassenbibliothek (`classlib`, `net10.0`). Hier gibt es **keine** Fenster, Buttons oder WPF, nur reines C#.

| Name | Typ | Erklärung |
| --- | --- | --- |
| `Budgetplaner.Core.csproj` | Datei | Projektdatei: Zielframework `net10.0`, Einstellungen, Pakete. |
| `Models/` | Ordner | **Datenklassen**, die nur Werte halten und nichts berechnen. Beispiele: `Buchung`, `Sparziel`, `BudgetProfil`, Enums wie `Wiederholung`, `BuchungsTyp`. |
| `Services/` | Ordner | **Berechnungen und Regeln.** Beispiele: `BudgetRechner` (Monatsbudget, Ampel), `SparzielRechner` (Prognose, Betrag pro Monat), `Validierung` (Eingaben prüfen), `Motivator` (Lob und Warnungen). |
| `Storage/` | Ordner | **Speichern und Laden.** Beispiel: `JsonSpeicher`, der das `BudgetProfil` als JSON-Datei schreibt und liest. |

## src/Budgetplaner.App/ – die Oberfläche

WPF-Projekt (`net10.0-windows`). Hier ist alles, was man sieht. Gerechnet wird hier nicht, das macht Core.

| Name | Typ | Erklärung |
| --- | --- | --- |
| `Budgetplaner.App.csproj` | Datei | Projektdatei: WPF aktiviert, Verweis auf Core, Pakete (CommunityToolkit.Mvvm, LiveCharts). |
| `App.xaml` | Datei | Einstiegspunkt der App: legt fest, welches Fenster beim Start aufgeht, und hält globale Styles und Ressourcen. |
| `App.xaml.cs` | Datei | Code zu `App.xaml`, z.B. für Code, der beim Programmstart laufen soll. |
| `AssemblyInfo.cs` | Datei | Technische Infos für WPF (wo Themes liegen). Musst du nicht anfassen. |
| `MainWindow.xaml` | Datei | Das Hauptfenster: Layout in XAML (wie HTML für WPF). |
| `MainWindow.xaml.cs` | Datei | „Code-behind“ des Hauptfensters. Bei MVVM bleibt diese Datei möglichst leer. |
| `Views/` | Ordner | Weitere **Seiten und Fenster** in XAML: `DashboardView`, `BuchungenView`, `SparzieleView`, `AssistentView`. |
| `ViewModels/` | Ordner | **Verbindung** zwischen View und Core: hält die angezeigten Daten und Befehle (z.B. „Speichern“-Button) und ruft die Services auf. Ein ViewModel pro View. |
| `Converters/` | Ordner | Kleine Umwandler für XAML, z.B. `AmpelZuFarbeConverter` (Ampelstatus → Grün, Gelb oder Rot) oder `BetragZuTextConverter` (→ „CHF 1'234.50“). |
| `Resources/` | Ordner | Farben, Styles, Schriften, Icons, also alles fürs Design, an einem Ort. |

### Wie MVVM zusammenspielt

```
View (XAML)  ⇄  ViewModel  →  Services / Storage (Core)  →  Models
 anzeigen       vermitteln        rechnen / speichern          Daten
```

---

## ✅ tests/Budgetplaner.Core.Tests/ – die Tests

xUnit-Testprojekt (`net10.0`). Testet **nur Core**, nicht die Oberfläche.

| Name | Typ | Erklärung |
| --- | --- | --- |
| `Budgetplaner.Core.Tests.csproj` | Datei | Projektdatei mit xUnit-Paketen und Verweis auf Core. |
| *(später)* `BudgetRechnerTests.cs` | Datei | Prüft z.B.: wöchentlich 100 CHF → 433.33 CHF pro Monat. |
| *(später)* `SparzielRechnerTests.cs` | Datei | Prüft Prognosedatum und den Sonderfall „Überschuss ≤ 0“. |
| *(später)* `ValidierungTests.cs` | Datei | Prüft, ob ungültige Eingaben abgelehnt werden. |

Tests ausführen: `dotnet test`

---

## Automatisch erzeugt (nicht auf GitHub)

| Name | Erklärung |
| --- | --- |
| `bin/` | Kompiliertes Programm (`.dll`, `.exe`). Entsteht bei `dotnet build`. |
| `obj/` | Zwischendateien vom Compiler. |
| `.vs/` | Lokale Einstellungen von Visual Studio. |

Alle drei sind in der `.gitignore` eingetragen und können jederzeit gelöscht werden. Sie entstehen beim nächsten Build neu.

---

## ⌨️ Wichtige Befehle

| Befehl | Was er macht |
| --- | --- |
| `dotnet build` | Alles kompilieren |
| `dotnet test` | Alle Tests ausführen |
| `dotnet run --project src/Budgetplaner.App` | App starten |
