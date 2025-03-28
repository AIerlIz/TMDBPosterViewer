using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using TMDBPosterViewer.Models;
using TMDBPosterViewer.Services;
using System.ComponentModel;
using System.Windows.Data;

namespace TMDBPosterViewer.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly TMDBService _tmdbService;
    private readonly AppConfig _appConfig;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isSearching;

    [ObservableProperty]
    private ObservableCollection<MediaItem> searchResults = [];

    [ObservableProperty]
    private MediaItem? selectedMedia;

    public MainViewModel()
    {
        _appConfig = AppConfig.Load();
        _tmdbService = new TMDBService(_appConfig.TmdbApiKey);
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return;

        try
        {
            IsSearching = true;
            SearchResults.Clear();

            var movieResults = await _tmdbService.SearchMoviesAsync(SearchText);
            var tvResults = await _tmdbService.SearchTVShowsAsync(SearchText);

            foreach (var movie in movieResults["results"]?.ToObject<JArray>() ?? new JArray())
            {
                SearchResults.Add(new MediaItem
                {
                    Id = movie["id"]?.Value<int>() ?? 0,
                    Title = movie["title"]?.Value<string>() ?? string.Empty,
                    OriginalTitle = movie["original_title"]?.Value<string>() ?? string.Empty,
                    PosterPath = _tmdbService.GetPreviewImageUrl(movie["poster_path"]?.Value<string>() ?? string.Empty),
                    Popularity = movie["popularity"]?.Value<double>() ?? 0,
                    VoteAverage = movie["vote_average"]?.Value<double>() ?? 0,
                    VoteCount = movie["vote_count"]?.Value<int>() ?? 0,
                    MediaType = "电影"
                });
            }

            foreach (var tv in tvResults["results"]?.ToObject<JArray>() ?? new JArray())
            {
                SearchResults.Add(new MediaItem
                {
                    Id = tv["id"]?.Value<int>() ?? 0,
                    Title = tv["name"]?.Value<string>() ?? string.Empty,
                    OriginalTitle = tv["original_name"]?.Value<string>() ?? string.Empty,
                    PosterPath = _tmdbService.GetPreviewImageUrl(tv["poster_path"]?.Value<string>() ?? string.Empty),
                    Popularity = tv["popularity"]?.Value<double>() ?? 0,
                    VoteAverage = tv["vote_average"]?.Value<double>() ?? 0,
                    VoteCount = tv["vote_count"]?.Value<int>() ?? 0,
                    MediaType = "剧集"
                });
            }

            SearchResults = new ObservableCollection<MediaItem>(
                SearchResults.OrderByDescending(x => x.VoteCount).ThenByDescending(x => x.VoteAverage));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"搜索时发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task LoadPostersAsync(MediaItem media)
    {
        if (media == null) return;

        try
        {   
            SelectedMedia = media;
            media.Posters.Clear();
            var images = media.MediaType == "电影" ?
                await _tmdbService.GetImagesAsync(media.Id,"movie") :
                await _tmdbService.GetImagesAsync(media.Id,"tv");

            System.Diagnostics.Debug.WriteLine($"API返回数据：{images}");
            var posters = images["posters"]?.ToObject<JArray>();
            if (posters == null || !posters.Any())
            {
                MessageBox.Show("未找到该媒体的海报", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var poster in posters)
            {
                var filePath = poster["file_path"]?.Value<string>();
                if (string.IsNullOrEmpty(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"跳过无效海报：{poster}");
                    continue;
                }

                media.Posters.Add(new PosterImage
                {
                    FilePath = filePath,
                    Language = poster["iso_639_1"]?.Value<string>() ?? "unknown",
                    AspectRatio = poster["aspect_ratio"]?.Value<double>() ?? 0,
                    Height = poster["height"]?.Value<int>() ?? 0,
                    Width = poster["width"]?.Value<int>() ?? 0,
                    FullImageUrl = _tmdbService.GetPreviewImageUrl(filePath)
                });
            }

            // 按语言排序：原始语言 > 中文 > 其他
            var originalLanguage = posters.FirstOrDefault()?["iso_639_1"]?.Value<string>() ?? "unknown";
            var sortedPosters = media.Posters.OrderBy(p => p.Language == originalLanguage ? 0 :
                                         p.Language == "zh" ? 1 : 2).ToList();
            media.Posters.Clear();
            foreach (var poster in sortedPosters)
            {
                media.Posters.Add(poster);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载海报时发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DownloadPosterAsync(PosterImage poster)
    {
        if (poster == null || string.IsNullOrEmpty(poster.FilePath)) return;

        try
        {
            var fileName = $"{SelectedMedia?.Title ?? "poster"}_{poster.Language}{Path.GetExtension(poster.FilePath)}";
            var invalidChars = Path.GetInvalidFileNameChars();
            fileName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            var savePath = Path.Combine(_appConfig.PosterSavePath, fileName);

            await _tmdbService.DownloadImageAsync(poster.FilePath, savePath);
            MessageBox.Show($"海报已保存至：{savePath}", "下载成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"下载海报时发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}