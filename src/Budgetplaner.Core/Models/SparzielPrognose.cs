namespace Budgetplaner.Core.Models;

// Ergebnis vom SparzielRechner, wird nicht gespeichert
public class SparzielPrognose
{
    public SparzielStatus Status { get; set; }

    // was noch fehlt bis zum Ziel
    public decimal Restbetrag { get; set; }

    // 0.25 = 25% geschafft (max. 1)
    public decimal Fortschritt { get; set; }

    // null = nicht erreichbar
    public int? MonateBisZiel { get; set; }
    public DateTime? Prognosedatum { get; set; }

    // nur wenn ein Zieldatum gesetzt ist
    public decimal? NoetigProMonat { get; set; }
}
