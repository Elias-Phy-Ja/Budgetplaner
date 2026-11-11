namespace Budgetplaner.Core.Models;

// Reihenfolge = Wichtigkeit, Warnungen kommen zuerst
public enum NachrichtArt
{
    Warnung,
    Hinweis,
    Feier,
    Lob
}

// record = kleine Datenklasse, Konstruktor und Properties gibts automatisch
public record Nachricht(NachrichtArt Art, string Text);
