namespace Budgetplaner.Core.Models;

public class Sparziel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public decimal Zielbetrag { get; set; }
    public decimal BereitsGespart { get; set; }
    public DateTime? Zieldatum { get; set; }         // ? = optional
}