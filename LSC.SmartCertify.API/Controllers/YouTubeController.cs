using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LSC.SmartCertify.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class YouTubeController : ControllerBase
    {
        private readonly YouTubeService _youtubeService;

        public YouTubeController(YouTubeService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        [AllowAnonymous]
        [HttpGet("{channelId}")]
        public async Task<IActionResult> GetVideos(string channelId)
        {
            try
            {
                var playlistId = await _youtubeService.GetUploadsPlaylistId(channelId);
                if (string.IsNullOrEmpty(playlistId))
                    return NotFound("Uploads playlist not found for the channel.");

                var videos = await _youtubeService.GetVideosFromPlaylist(playlistId, 20);
                return Ok(videos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class ChannelResponse
    {
        public string Kind { get; set; }
        public List<ChannelItem> Items { get; set; }
    }

    public class ChannelItem
    {
        public Snippet Snippet { get; set; }
        public ContentDetails ContentDetails { get; set; }
    }
    public class PlaylistItem
    {
        public PlaylistSnippet Snippet { get; set; }
    }

    public class PlaylistSnippet
    {
        public string Title { get; set; }
        public string Description { get; set; } // Add this
        public ResourceId ResourceId { get; set; }
        public DateTime? PublishedAt { get; set; } // Useful for sorting
        public Thumbnails Thumbnails { get; set; } // Adding thumbnails as they are "important"
    }

    // Add these to support Thumbnails
    public class Thumbnails
    {
        public ThumbnailDetails High { get; set; }
        public ThumbnailDetails Medium { get; set; }
    }

    public class ThumbnailDetails
    {
        public string Url { get; set; }
    }

    public class ResourceId
    {
        public string VideoId { get; set; }
    }

    public class Snippet
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class ContentDetails
    {
        public RelatedPlaylists RelatedPlaylists { get; set; }
    }

    public class RelatedPlaylists
    {
        public string Uploads { get; set; }
    }

    public class PlaylistResponse
    {
        public string Kind { get; set; }
        public List<PlaylistItem> Items { get; set; }
        public string NextPageToken { get; set; }
    }


    public class VideoSnippet
    {
        public string Title { get; set; }
        public string ResourceId { get; set; }
    }

    public class YouTubeOptions
    {
        public string ApiKey { get; set; }
    }

    public class YouTubeVideoDetails
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string VideoId { get; set; }
        public string VideoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    public class YouTubeService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public YouTubeService(HttpClient httpClient, IOptions<YouTubeOptions> options)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
        }


        public async Task<string> GetUploadsPlaylistId(string channelId)
        {
            var url = $"https://www.googleapis.com/youtube/v3/channels?part=contentDetails&id={channelId}&key={_apiKey}";
            var response = await _httpClient.GetFromJsonAsync<ChannelResponse>(url);

            return response?.Items?.FirstOrDefault()?.ContentDetails?.RelatedPlaylists?.Uploads;
        }

        public async Task<List<string>> GetVideosFromPlaylist(string playlistId)
        {
            var url = $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId={playlistId}&maxResults=50&key={_apiKey}";
            var response = await _httpClient.GetFromJsonAsync<PlaylistResponse>(url);

            return response?.Items?.Select(item =>
                $"Title: {item.Snippet.Title}, URL: https://www.youtube.com/watch?v={item.Snippet.ResourceId.VideoId}").ToList();
        }

        public async Task<List<YouTubeVideoDetails>> GetVideosFromPlaylist(string playlistId, int pageNumber = 10)
        {
            var allVideos = new List<YouTubeVideoDetails>();
            string pageToken = null;

            for (int currentPage = 1; currentPage <= pageNumber; currentPage++)
            {
                var url = $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&playlistId={playlistId}&maxResults=50&key={_apiKey}" +
                          (!string.IsNullOrEmpty(pageToken) ? $"&pageToken={pageToken}" : "");

                var response = await _httpClient.GetFromJsonAsync<PlaylistResponse>(url);

                if (response?.Items != null)
                {
                    allVideos.AddRange(response.Items.Select(item => new YouTubeVideoDetails
                    {
                        Title = item.Snippet.Title,
                        Description = item.Snippet.Description, // Now captured
                        VideoId = item.Snippet.ResourceId.VideoId,
                        VideoUrl = $"https://www.youtube.com/watch?v={item.Snippet.ResourceId.VideoId}",
                        ThumbnailUrl = item.Snippet.Thumbnails?.High?.Url ?? item.Snippet.Thumbnails?.Medium?.Url,
                        PublishedAt = item.Snippet.PublishedAt
                    }));
                }

                if (string.IsNullOrEmpty(response?.NextPageToken))
                    break;

                pageToken = response.NextPageToken;
            }

            return allVideos;
        }

    }

}


