using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace TMDBPosterViewer.Services;

public class TMDBService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.themoviedb.org/3";
    private const string ImageBaseUrl = "https://image.tmdb.org/t/p";
    private const string PreviewImageSize = "w500";
    private const string OriginalImageSize = "original";

    public TMDBService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
    }

    public async Task<JObject> SearchMoviesAsync(string query, string language = "zh-CN")
    {
        var url = $"{BaseUrl}/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}&language={language}";
        var response = await _httpClient.GetStringAsync(url);
        return JObject.Parse(response);
    }

    public async Task<JObject> SearchTVShowsAsync(string query, string language = "zh-CN")
    {
        var url = $"{BaseUrl}/search/tv?api_key={_apiKey}&query={Uri.EscapeDataString(query)}&language={language}";
        var response = await _httpClient.GetStringAsync(url);
        return JObject.Parse(response);
    }

    public async Task<JObject> GetImagesAsync(int movieId,string type)
    {
        var url = $"{BaseUrl}/{type}/{movieId}/images?api_key={_apiKey}";
        var response = await _httpClient.GetStringAsync(url);
        return JObject.Parse(response);
    }

    public string GetPreviewImageUrl(string posterPath)
    {
        return $"{ImageBaseUrl}/{PreviewImageSize}{posterPath}";
    }

    public string GetOriginalImageUrl(string posterPath)
    {
        return $"{ImageBaseUrl}/{OriginalImageSize}{posterPath}";
    }

    public async Task DownloadImageAsync(string posterPath, string savePath)
    {
        var imageUrl = GetOriginalImageUrl(posterPath);
        var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
        await File.WriteAllBytesAsync(savePath, imageBytes);
    }
}