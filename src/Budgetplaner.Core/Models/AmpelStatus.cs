namespace Budgetplaner.Core.Models;

// Ampel für die Budget-Übersicht
public enum AmpelStatus
{
    Gruen,  // Überschuss >= 10% der Einnahmen
    Gelb,   // knapp: zwischen 0 und 10%
    Rot     // Budget im Minus
}
