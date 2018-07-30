using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Xenon.Core
{
    public class Models
    {
        public partial class GfycatImage
        {
            [JsonProperty("gfyItem")] public GfyItem GfyItem { get; set; }
        }

        public class GfyItem
        {
            [JsonProperty("tags")] public string[] Tags { get; set; }

            [JsonProperty("languageCategories")] public object[] LanguageCategories { get; set; }

            [JsonProperty("domainWhitelist")] public object[] DomainWhitelist { get; set; }

            [JsonProperty("geoWhitelist")] public object[] GeoWhitelist { get; set; }

            [JsonProperty("published")] public long Published { get; set; }

            [JsonProperty("nsfw")] public long Nsfw { get; set; }

            [JsonProperty("gatekeeper")] public long Gatekeeper { get; set; }

            [JsonProperty("mp4Url")] public string Mp4Url { get; set; }

            [JsonProperty("gifUrl")] public string GifUrl { get; set; }

            [JsonProperty("webmUrl")] public string WebmUrl { get; set; }

            [JsonProperty("webpUrl")] public string WebpUrl { get; set; }

            [JsonProperty("mobileUrl")] public string MobileUrl { get; set; }

            [JsonProperty("mobilePosterUrl")] public string MobilePosterUrl { get; set; }

            [JsonProperty("extraLemmas")] public string ExtraLemmas { get; set; }

            [JsonProperty("thumb100PosterUrl")] public string Thumb100PosterUrl { get; set; }

            [JsonProperty("miniUrl")] public string MiniUrl { get; set; }

            [JsonProperty("gif100px")] public string Gif100Px { get; set; }

            [JsonProperty("miniPosterUrl")] public string MiniPosterUrl { get; set; }

            [JsonProperty("max5mbGif")] public string Max5MbGif { get; set; }

            [JsonProperty("title")] public string Title { get; set; }

            [JsonProperty("max2mbGif")] public string Max2MbGif { get; set; }

            [JsonProperty("max1mbGif")] public string Max1MbGif { get; set; }

            [JsonProperty("posterUrl")] public string PosterUrl { get; set; }

            [JsonProperty("languageText")] public string LanguageText { get; set; }

            [JsonProperty("views")] public long Views { get; set; }

            [JsonProperty("userName")] public string UserName { get; set; }

            [JsonProperty("description")] public string Description { get; set; }

            [JsonProperty("hasTransparency")] public bool HasTransparency { get; set; }

            [JsonProperty("likes")] public long Likes { get; set; }

            [JsonProperty("dislikes")] public long Dislikes { get; set; }

            [JsonProperty("gfyNumber")] public long GfyNumber { get; set; }

            [JsonProperty("gfyId")] public string GfyId { get; set; }

            [JsonProperty("gfyName")] public string GfyName { get; set; }

            [JsonProperty("avgColor")] public string AvgColor { get; set; }

            [JsonProperty("width")] public long Width { get; set; }

            [JsonProperty("height")] public long Height { get; set; }

            [JsonProperty("frameRate")] public double FrameRate { get; set; }

            [JsonProperty("numFrames")] public long NumFrames { get; set; }

            [JsonProperty("gifSize")] public long GifSize { get; set; }

            [JsonProperty("mp4Size")] public long Mp4Size { get; set; }

            [JsonProperty("webmSize")] public long WebmSize { get; set; }

            [JsonProperty("createDate")] public long CreateDate { get; set; }

            [JsonProperty("source")] public long Source { get; set; }

            [JsonProperty("content_urls")] public ContentUrls ContentUrls { get; set; }
        }

        public class ContentUrls
        {
            [JsonProperty("max2mbGif")] public The100PxGifClass Max2MbGif { get; set; }

            [JsonProperty("max1mbGif")] public The100PxGifClass Max1MbGif { get; set; }

            [JsonProperty("100pxGif")] public The100PxGifClass The100PxGif { get; set; }

            [JsonProperty("max5mbGif")] public The100PxGifClass Max5MbGif { get; set; }

            [JsonProperty("largeGif")] public LargeGif LargeGif { get; set; }
        }

        public class LargeGif
        {
            [JsonProperty("height")] public long Height { get; set; }

            [JsonProperty("width")] public long Width { get; set; }
        }

        public class The100PxGifClass
        {
            [JsonProperty("url")] public string Url { get; set; }

            [JsonProperty("height")] public long Height { get; set; }

            [JsonProperty("width")] public long Width { get; set; }
        }
        
        public class KsoftImage
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("image_url")]
            public string ImageUrl { get; set; }

            [JsonProperty("source")]
            public string Source { get; set; }

            [JsonProperty("subreddit")]
            public string Subreddit { get; set; }

            [JsonProperty("upvotes")]
            public long Upvotes { get; set; }

            [JsonProperty("downvotes")]
            public long Downvotes { get; set; }

            [JsonProperty("nsfw")]
            public bool Nsfw { get; set; }

            [JsonProperty("meta__cached")]
            public bool MetaCached { get; set; }
        }
        
        public class ImgurImage
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }
    }

    public class Data
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public object Title { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("datetime")]
        public long Datetime { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("animated")]
        public bool Animated { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("views")]
        public long Views { get; set; }

        [JsonProperty("bandwidth")]
        public long Bandwidth { get; set; }

        [JsonProperty("vote")]
        public object Vote { get; set; }

        [JsonProperty("favorite")]
        public bool Favorite { get; set; }

        [JsonProperty("nsfw")]
        public bool Nsfw { get; set; }

        [JsonProperty("section")]
        public string Section { get; set; }

        [JsonProperty("account_url")]
        public object AccountUrl { get; set; }

        [JsonProperty("account_id")]
        public object AccountId { get; set; }

        [JsonProperty("is_ad")]
        public bool IsAd { get; set; }

        [JsonProperty("in_most_viral")]
        public bool InMostViral { get; set; }

        [JsonProperty("has_sound")]
        public bool HasSound { get; set; }

        [JsonProperty("tags")]
        public object[] Tags { get; set; }

        [JsonProperty("ad_type")]
        public long AdType { get; set; }

        [JsonProperty("ad_url")]
        public string AdUrl { get; set; }

        [JsonProperty("in_gallery")]
        public bool InGallery { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
    }
    }
}