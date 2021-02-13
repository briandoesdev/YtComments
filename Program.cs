using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

// Commandline
using Utility.CommandLine;

// Google
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace YtComments
{
    class Program
    {
        [Argument('v', "video-id", "VideoId to retrieve comments from")]
        private static string videoId { get; set; }
        
        [Argument('k', "api-key", "Google ApiKey")]
        private static string apiKey { get; set; }

        [Argument('f', "filename", "Filename to save as")]
        private static string filename { get; set; }

        private List<CommentThreadSnippet> CommentsList { get; set; } = new List<CommentThreadSnippet>();

        static void Main(string[] args)
        {
            Arguments.Populate();

            try
            {
                if(string.IsNullOrWhiteSpace(videoId))
                    throw new ArgumentNullException("VideoId was not specificed");
                
                if(string.IsNullOrWhiteSpace(apiKey))
                    throw new ArgumentNullException("ApiKey was not specificed");

                if(videoId.Length > 11)
                    throw new ArgumentException("Invalid VideoId length");

                new Program().Run(apiKey: apiKey).GetAwaiter().GetResult();

                Console.WriteLine($@"Saved to C:\temp\{filename}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
            
        }

        private async Task Run(string apiKey)
        {
            YouTubeService service = new YouTubeService(initializer: new BaseClientService.Initializer{
                ApplicationName = "youtube-data-api",
                ApiKey = apiKey
            });

            var nextPageToken = "";
            while(nextPageToken != null)
            {
                var commentsList = service.CommentThreads.List("snippet");
                commentsList.VideoId = videoId;
                commentsList.MaxResults = 100;
                commentsList.PageToken = nextPageToken;
                CommentThreadListResponse resp = await commentsList.ExecuteAsync();

                foreach (var item in resp.Items)
                    CommentsList.Add(item.Snippet);


                nextPageToken = resp.NextPageToken;
            }

            string json = JsonConvert.SerializeObject(CommentsList);
            File.WriteAllText($@"C:\temp\{filename}", json);
        }
    }
}
