namespace Budgetplaner.Core.Models;

// Ergebnis vom BudgetRechner für einen Monat (wird nicht gespeichert, nur berechnet)
public class BudgetUebersicht
{
    public DateTime Monat { get; set; }

    public decimal WiederkehrendeEinnahmen { get; set; }
    public decimal WiederkehrendeAusgaben { get; set; }
    public decimal EinmaligeEinnahmen { get; set; }
    public decimal EinmaligeAusgaben { get; set; }

    public AmpelStatus Ampel { get; set; }

    // Was jeden Monat regelmässig übrig bleibt
    public decimal Ueberschuss => WiederkehrendeEinnahmen - WiederkehrendeAusgaben;

    // Überschuss + alles Einmalige von diesem Monat
    public decimal AktuellesBudget => Ueberschuss + EinmaligeEinnahmen - EinmaligeAusgaben;

    public decimal EinnahmenTotal => WiederkehrendeEinnahmen + EinmaligeEinnahmen;

    // Anteil der Einnahmen, der übrig bleibt (0.18 = 18%), brauche ich später für den Motivator
    public decimal Sparquote => EinnahmenTotal > 0 ? AktuellesBudget / EinnahmenTotal : 0;
}
