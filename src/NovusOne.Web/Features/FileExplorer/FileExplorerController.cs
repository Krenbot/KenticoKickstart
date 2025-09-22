using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NovusOne.Web.Features.FileExplorer
{
    public class MediaLibraryController : Controller
    {
        private readonly HttpClient _httpClient;

        public MediaLibraryController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> FileTree()
        {
            var tree = await GetMediaLibraryTree();
            return View(tree);
        }

        private async Task<List<MediaNode>> GetMediaLibraryTree()
        {
            // Example: calling Kentico SaaS REST API
            // Replace with your tenant, project, and API key
            string apiUrl = "https://<your-tenant>.kenticoapi.com/api/media?apiKey=<your-api-key>";

            var mediaItems = await _httpClient.GetFromJsonAsync<List<MediaItem>>(apiUrl);

            // Build a tree based on FolderPath (SaaS media has 'folder' or 'path' property)
            var rootNodes = new List<MediaNode>();
            var folderLookup = new Dictionary<string, MediaNode>();

            foreach (var item in mediaItems)
            {
                var pathParts = item.FolderPath.Split('/');
                string currentPath = "";
                MediaNode parent = null;

                foreach (var part in pathParts)
                {
                    currentPath = string.IsNullOrEmpty(currentPath)
                        ? part
                        : $"{currentPath}/{part}";

                    if (!folderLookup.ContainsKey(currentPath))
                    {
                        var node = new MediaNode
                        {
                            Name = part,
                            Path = currentPath,
                            Children = new List<MediaNode>(),
                        };

                        folderLookup[currentPath] = node;

                        if (parent == null)
                            rootNodes.Add(node);
                        else
                            parent.Children.Add(node);
                    }

                    parent = folderLookup[currentPath];
                }

                // Add file as a child
                parent.Children.Add(
                    new MediaNode
                    {
                        Name = item.FileName,
                        Path = item.Url,
                        Children = null,
                    }
                );
            }

            return rootNodes;
        }

        public class MediaNode
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public List<MediaNode> Children { get; set; }
        }

        public class MediaItem
        {
            public string FileName { get; set; }
            public string Url { get; set; }
            public string FolderPath { get; set; }
        }
    }
}
