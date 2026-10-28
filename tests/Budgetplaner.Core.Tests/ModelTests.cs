using Budgetplaner.Core.Models;

namespace Budgetplaner.Core.Tests;

public class ModelTests
{
    [Fact]
    public void NeueBuchungen_HabenVerschiedeneIds()
    {
        var a = new Buchung();
        var b = new Buchung();

        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void NeuesProfil_IstLeer()
    {
        var profil = new BudgetProfil();

        Assert.Empty(profil.Buchungen);
        Assert.Empty(profil.Sparziele);
    }
}