namespace Budgetplaner.Core.Models;

public class Buchung
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Titel { get; set; } = "";
    public decimal Betrag { get; set; }              // immer positiv
    public BuchungsTyp Typ { get; set; }
    public string Kategorie { get; set; } = "Sonstiges";
    public DateTime Datum { get; set; } = DateTime.Today;
    public Wiederholung Wiederholung { get; set; } = Wiederholung.Monatlich;
}