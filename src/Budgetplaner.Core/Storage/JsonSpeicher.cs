using System.Text.Json;
using System.Text.Json.Serialization;
using Budgetplaner.Core.Models;

namespace Budgetplaner.Core.Storage;

public static class JsonSpeicher
{
    // Standard-Speicherort: C:\Users\<Name>\AppData\Roaming\Budgetplaner\budget.json
    public static string StandardPfad =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Budgetplaner",
            "budget.json");

    // WriteIndented = schön formatiert, Enums als Text ("Monatlich" statt 2)
    private static readonly JsonSerializerOptions Optionen = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static void Speichern(BudgetProfil profil, string pfad)
    {
        // Ordner erstellen, falls es ihn noch nicht gibt
        string? ordner = Path.GetDirectoryName(Path.GetFullPath(pfad));
        if (!string.IsNullOrEmpty(ordner))
            Directory.CreateDirectory(ordner);

        string json = JsonSerializer.Serialize(profil, Optionen);
        File.WriteAllText(pfad, json);
    }

    public static BudgetProfil Laden(string pfad)
    {
        // Erster Start: noch keine Datei -> leeres Profil
        if (!File.Exists(pfad))
            return new BudgetProfil();

        try
        {
            string json = File.ReadAllText(pfad);
            return JsonSerializer.Deserialize<BudgetProfil>(json, Optionen) ?? new BudgetProfil();
        }
        catch (JsonException)
        {
            // Datei kaputt -> Kopie behalten, damit beim nächsten Speichern nicht alles weg ist
            File.Copy(pfad, pfad + ".defekt", overwrite: true);
            return new BudgetProfil();
        }
    }
}
