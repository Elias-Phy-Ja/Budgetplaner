# 💰 Budgetplaner – Vorausschauende Budgetplanung

Programmierprojekt IMS 2026, Alte Kantonsschule Aarau
Ausschreibung 2: *Vorausschauende Budgetplanung* (ausgeschrieben von KliLi)

Ein Budget-Assistent für die persönliche Finanzplanung: Einnahmen und Ausgaben (wiederkehrend und einmalig) erfassen, das aktuelle Budget mit einer Ampel darstellen, Sparziele planen und zum Sparen motivieren.

---

## 📋 Anforderungen laut Ausschreibung

- **Basisstufe:** Ein Assistent erfasst die wichtigsten Eckwerte der finanziellen Situation (v.a. wiederkehrende Einnahmen und Ausgaben) und speichert sie in einer Datei.
- Unerwartete Einnahmen und Ausgaben können zusätzlich eingetragen werden → Übersicht über das aktuelle Budget.
- **Erweiterung:** Sparziele festlegen, Prognosen erstellen (bis wann ist das Ziel erreicht?) und berechnen, wie viel pro Zeiteinheit gespart werden muss.
- Der Budgetplaner kann loben bzw. zum Sparen animieren.

### Meilensteine laut Ausschreibung

1. Eingaben von Benutzer validieren und speichern
2. Berechnung des Budgets mit regelmässigen und ausserordentlichen Ein- und Ausgaben (Stufe 1)
3. Ansprechend gestaltetes GUI, ggf. mit Ampeldarstellung (Stufe 2)
4. Berechnung von Sparzielen (Stufe 3)
5. Motivieren zum Sparen (Stufe 4)

---

## ✅ Geklärte Rahmenbedingungen

| Frage | Antwort |
| --- | --- |
| Externe Bibliotheken erlaubt? | **Ja** |

---

## 🏗️ Architektur

Grundprinzip: **Logik und Oberfläche strikt trennen.** Alle Berechnungen liegen in eigenen Klassen, nicht in Click-Handlern. So kann die Logik zuerst in einer Konsolen-App getestet und die GUI später darübergelegt werden.

```
Budgetplaner/
├── Models/        → Buchung, Sparziel, BudgetProfil (reine Datenklassen)
├── Services/      → BudgetRechner, SparzielRechner, Motivator, Validierung
├── Storage/       → JsonSpeicher (Laden/Speichern in eine Datei)
└── UI/            → Fenster, Einrichtungsassistent, Dashboard
```

### Technologie

| Bereich | Wahl | Begründung |
| --- | --- | --- |
| Sprache | C# / .NET | |
| GUI | **WPF** (Alternative: Avalonia für plattformübergreifend) | Moderne, ansprechende Oberflächen mit XAML einfacher als mit WinForms |
| Diagramme | **LiveCharts2** (NuGet) | Kostenlos, schöne Donut- und Liniendiagramme; externe Bibliotheken sind erlaubt |
| Speicherung | JSON via `System.Text.Json` | Eingebaut, lesbar, einfach |

---

## 🧱 Datenmodell

### Buchung
| Feld | Typ | Beschreibung |
| --- | --- | --- |
| Id | `Guid` | Eindeutige ID |
| Titel | `string` | z.B. „Handy-Abo“ |
| Betrag | `decimal` | Immer positiv, Typ bestimmt Vorzeichen |
| Typ | `enum` | Einnahme / Ausgabe |
| Kategorie | `string` / `enum` | Wohnen, Essen, Freizeit, Mobilität, Lohn … |
| Datum | `DateTime` | Datum bzw. Startdatum |
| Wiederholung | `enum` | Einmalig, Wöchentlich, Monatlich, Jährlich |

Wiederkehrende und ausserordentliche Buchungen werden mit **einer** Klasse abgebildet.

### Sparziel
| Feld | Typ | Beschreibung |
| --- | --- | --- |
| Name | `string` | z.B. „Neuer Laptop“ |
| Zielbetrag | `decimal` | |
| BereitsGespart | `decimal` | |
| Zieldatum | `DateTime?` | Optional |

### BudgetProfil
Enthält die Listen aller Buchungen und Sparziele (plus optional Kategorie-Limits) und wird als Ganzes in eine JSON-Datei gespeichert.

> 💡 Für Geldbeträge immer `decimal` verwenden, nicht `double` sonst entstehen Rundungsfehler (0.1 + 0.2 = 0.30000000000000004).

---

## 🚀 Meilensteine im Detail

### Meilenstein 1 Validieren und Speichern
- Validierungsklasse mit klaren Fehlermeldungen:
  - Betrag ist eine Zahl > 0, höchstens zwei Nachkommastellen
  - Titel darf nicht leer sein
  - Sparziel-Datum liegt in der Zukunft
- Laden/Speichern als JSON
- Fehlende oder beschädigte Datei abfangen → App startet mit leerem Profil

### Meilenstein 2 Budgetberechnung (Stufe 1)
Alles wird auf **einen Monat** umgerechnet:

| Wiederholung | Umrechnung pro Monat |
| --- | --- |
| Wöchentlich | × 52 / 12 |
| Monatlich | × 1 |
| Jährlich | / 12 |
| Einmalig | nur im jeweiligen Monat |

- **Monatlicher Überschuss** = wiederkehrende Einnahmen − wiederkehrende Ausgaben
- **Aktuelles Budget** = Überschuss + einmalige Buchungen des aktuellen Monats
- Testfälle mit von Hand nachgerechneten Zahlen schreiben

### Meilenstein 3 GUI mit Ampel (Stufe 2)
Die Ampellogik liegt im Service, nicht im UI:

| Ampel | Bedingung |
| --- | --- |
| 🟢 Grün | Überschuss ≥ 10 % der Einnahmen |
| 🟡 Gelb | Überschuss zwischen 0 und 10 % |
| 🔴 Rot | Budget im Minus |

Zusätzlich: Limits pro Kategorie, die bei Überschreitung rot markiert werden.

### Meilenstein 4 Sparziele (Stufe 3)
**Bis wann ist das Ziel erreicht?**
```
Monate = aufrunden( (Zielbetrag − BereitsGespart) / monatlicher Überschuss )
Prognosedatum = heute + Monate
```

**Wie viel muss pro Monat gespart werden?**
```
nötig pro Monat = (Zielbetrag − BereitsGespart) / Monate bis Zieldatum
```
- Ist der nötige Betrag grösser als der Überschuss → Hinweis „so nicht realistisch“
- Sonderfall: Überschuss ≤ 0 → „Ziel mit aktuellem Budget nicht erreichbar“ (keine Division durch 0!)

### Meilenstein 5 Motivation (Stufe 4)
Eine `Motivator`-Klasse wählt Nachrichten anhand der echten Daten:
- 🟢 Lob: „Du sparst diesen Monat 18 % deiner Einnahmen, stark!“
- 🟡 Sanfte Warnung bei knappem Budget
- 🔴 Konkrete Hinweise: „Freizeit liegt 40 Franken über deinem Limit“
- 🎉 Meilenstein-Feiern bei 25 / 50 / 75 / 100 % eines Sparziels
- 🔥 Streak: Anzahl Monate in Folge im Plus

---

## 🗓️ Grober Zeitplan

| Phase | Dauer |
| --- | --- |
| Setup und Datenmodell | 1 Woche |
| Validierung, Speichern, Budgetrechnung (zuerst in der Konsole) | 1–2 Wochen |
| GUI inkl. Ampel | 2 Wochen |
| Sparziele | 1 Woche |
| Motivation | 1 Woche |
| Puffer, Testing, Dokumentation | Rest |

> ⚠️ Puffer einplanen die GUI braucht erfahrungsgemäss mehr Zeit als gedacht.

---

## 🎨 GUI-Konzept

**Stil:** Ruhig und vertrauenswürdig viel Weissraum, gut lesbare Schrift, sanfte Grundfarbe (z.B. tiefes Blau oder Petrol). So bleiben die Ampelfarben die einzigen lauten Farben und springen sofort ins Auge.


### Einrichtungsassistent (erster Start)
1. Regelmässige Einnahmen (Lohn, Taschengeld, Stipendium)
2. Fixkosten (Handy-Abo, ÖV-Abo, Versicherung)
3. Optional: erstes Sparziel
4. Zusammenfassung mit erster Ampel

Mit Weiter/Zurück-Buttons und Fortschrittsanzeige oben.

### Weitere Seiten
- **Buchungen:** Tabelle mit Filter nach Monat und Kategorie; Symbol für einmalig vs. wiederkehrend
- **Sparziele:** Karte pro Ziel mit Fortschrittsbalken, Prognosedatum und Live-Rechner („Ich will es bis ___ schaffen“ → nötiger Betrag pro Monat)
- **Profil:** Kategorien und Limits verwalten, Datei laden/speichern

---


## 🛠️ Nächste Schritte

- [ ] Offene Fragen mit KliLi klären
- [ ] Projekt anlegen (Solution mit Models / Services / Storage / UI)
- [ ] Model-Klassen und JSON-Speicher umsetzen
- [ ] `BudgetRechner` mit Monatsumrechnung + Testfälle