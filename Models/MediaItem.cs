using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace TMDBPosterViewer.Models;

public partial class MediaItem : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string originalTitle = string.Empty;

    [ObservableProperty]
    private string posterPath = string.Empty;

    [ObservableProperty]
    private double popularity;

    [ObservableProperty]
    private string mediaType = string.Empty; // movie or tv

    [ObservableProperty]
    private double voteAverage;

    [ObservableProperty]
    private int voteCount;

    [ObservableProperty]
    private ObservableCollection<PosterImage> posters = new();
}

public partial class PosterImage : ObservableObject
{
    [ObservableProperty]
    private string filePath = string.Empty;

    [ObservableProperty]
    private string language = string.Empty;

    [ObservableProperty]
    private double aspectRatio;

    [ObservableProperty]
    private int height;

    [ObservableProperty]
    private int width;

    [ObservableProperty]
    private string fullImageUrl = string.Empty;
}