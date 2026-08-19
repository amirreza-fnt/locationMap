using System.Net.Http.Json;
using LocationMap.API.Services.Interfaces;

namespace LocationMap.API.Services;

public sealed class ShortLinkClient(HttpClient http, ILogger<ShortLinkClient> logger) : IShortLinkClient
{
    public async Task<string?> CreateShortUrlAsync(string longUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(longUrl))
            return null;

        try
        {
            using var response = await http.PostAsJsonAsync("/api/links", new { url = longUrl }, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Short-links API returned {Status} for {Url}", (int)response.StatusCode, longUrl);
                return null;
            }

            var created = await response.Content.ReadFromJsonAsync<CreatedShortLink>(ct);
            return string.IsNullOrWhiteSpace(created?.ShortUrl) ? null : created.ShortUrl;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not create short link for {Url}", longUrl);
            return null;
        }
    }

    private sealed class CreatedShortLink
    {
        public string? ShortUrl { get; set; }
    }
}
