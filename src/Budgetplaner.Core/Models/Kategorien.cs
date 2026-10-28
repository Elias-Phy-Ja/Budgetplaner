namespace Budgetplaner.Core.Models;

// Standard-Kategorien, damit Konsole und später GUI die gleichen Listen haben
public static class Kategorien
{
    public static readonly List<string> Einnahmen = new()
    {
        "Lohn", "Taschengeld", "Geschenk", "Sonstiges"
    };

    public static readonly List<string> Ausgaben = new()
    {
        "Wohnen", "Essen", "Mobilität", "Kommunikation", "Versicherung", "Freizeit", "Kleidung", "Sonstiges"
    };
}
