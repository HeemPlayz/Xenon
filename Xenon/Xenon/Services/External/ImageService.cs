using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xenon.Core;

namespace Xenon.Services.External
{
    public class ImageService
    {
        private readonly HttpClient _httpClient;
        
        private readonly string[] _acceptableTypes = {"jpg", "jpeg", "png", "gif"};

        public ImageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Models.KsoftImage> ResolveImage(string rawresponse)
        {
            var image = JsonConvert.DeserializeObject<Models.KsoftImage>(rawresponse);
            image.ImageUrl = image.ImageUrl.TrimEnd('/');
            var id = image.ImageUrl.Split('/').Last();
            if (_acceptableTypes.Any(x => image.ImageUrl.EndsWith(x))) return image;
            if (image.ImageUrl.Contains("gfycat.com", StringComparison.OrdinalIgnoreCase))
            {
                if (_acceptableTypes.Any(x => image.ImageUrl.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                {
                    image.ImageUrl = image.ImageUrl;
                }
                else
                {
                    var gfycatImage =
                        JsonConvert.DeserializeObject<Models.GfycatImage>(
                            await _httpClient.GetStringAsync($"https://api.gfycat.com/v1/gfycats/{id}")).GfyItem;
                    image.ImageUrl = gfycatImage.GifUrl;
                }
            }
            else if (image.ImageUrl.Contains("imgur.com"))
            {
                if (image.ImageUrl.Contains("i.imgur.com")) return image;
                var imgurImage =
                    JsonConvert.DeserializeObject<Models.ImgurImage>(
                        await _httpClient.GetStringAsync($"https://api.imgur.com/3/image/{id}"));
                image.ImageUrl = imgurImage.Data.Link;
            }

            return image;
        }
    }
}