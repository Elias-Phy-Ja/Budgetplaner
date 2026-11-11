namespace Budgetplaner.Core.Models;

public enum SparzielStatus
{
    Erreicht,          // schon genug gespart
    AufKurs,           // klappt mit dem aktuellen Überschuss
    NichtRealistisch,  // Zieldatum ist mit dem Überschuss nicht zu schaffen
    NichtErreichbar    // kein Überschuss, so wird das nie was
}
