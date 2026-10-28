namespace Budgetplaner.Core.Models;

public class BudgetProfil
{
    public List<Buchung> Buchungen { get; set; } = new();
    public List<Sparziel> Sparziele { get; set; } = new();
    public Dictionary<string, decimal> KategorieLimits { get; set; } = new();
}