using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace TMDBPosterViewer.Models;

public partial class AppConfig : ObservableObject
{
    private static readonly string ConfigFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TMDBPosterViewer");

    private static readonly string ConfigPath = Path.Combine(ConfigFolder, "config.json");

    [ObservableProperty]
    private string tmdbApiKey = string.Empty;

    [ObservableProperty]
    private string posterSavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        "TMDBPosterViewer");

    public AppConfig()
    {
        if (!Directory.Exists(ConfigFolder))
        {
            Directory.CreateDirectory(ConfigFolder);
        }

        if (!Directory.Exists(PosterSavePath))
        {
            Directory.CreateDirectory(PosterSavePath);
        }
    }

    public void Save()
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(ConfigPath, json);
    }

    public static AppConfig Load()
    {
        if (File.Exists(ConfigPath))
        {
            var json = File.ReadAllText(ConfigPath);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
        }
        return new AppConfig();
    }
}