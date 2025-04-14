using CommunityToolkit.Mvvm.Collections;
using HotPotPlayer.Bilibili.Models.User;
using HotPotPlayer.Models;
using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Artists;
using Jellyfin.Sdk.Generated.Items;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TagLib.Matroska;
using SortOrder = Jellyfin.Sdk.Generated.Models.SortOrder;

namespace HotPotPlayer.Services
{
    public partial class JellyfinMusicService(ConfigBase config, DispatcherQueue uiQueue = null, AppBase app = null) : ServiceBaseWithConfig(config, uiQueue, app)
    {

        #region State
        private LocalServiceState _state = LocalServiceState.Idle;

        public LocalServiceState State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        #endregion
        #region Property
        public List<BaseItemDto> JellyfinPlayListList { get; set; }
        #endregion
        #region Field
        private string devideId;
        public string DevideId => devideId ??= $"this-is-my-device-id-{Guid.NewGuid():N}";

        private JellyfinSdkSettings sdkClientSettings;

        public JellyfinSdkSettings SdkClientSettings
        {
            get
            {
                if (sdkClientSettings == null)
                {
                    sdkClientSettings = new JellyfinSdkSettings();
                    sdkClientSettings.Initialize(
                        "HotpotPlayer",
                        "0.0.1",
                        "Sample Device",
                        DevideId);
                    sdkClientSettings.SetServerUrl(Config.GetConfig<string>("JellyfinUrl"));
                }
                return sdkClientSettings;
            }
        }

        private HttpClient httpClient;

        private HttpClient HttpClient
        {
            get
            {
                if (httpClient == null)
                {
                    httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("My-Jellyfin-Client", "0.0.1"));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 1.0));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
                }
                return httpClient;
            }
        }

        private JellyfinApiClient jellyfinApiClient;

        private JellyfinApiClient JellyfinApiClient
        {
            get => jellyfinApiClient ??= new JellyfinApiClient(JellyfinRequestAdapter);
        }

        private JellyfinAuthenticationProvider jellyfinAuthenticationProvider;
        private JellyfinAuthenticationProvider JellyfinAuthenticationProvider
        {
            get => jellyfinAuthenticationProvider ??= new JellyfinAuthenticationProvider(SdkClientSettings);
        }

        private JellyfinRequestAdapter jellyfinRequestAdapter;
        private JellyfinRequestAdapter JellyfinRequestAdapter
        {
            get => jellyfinRequestAdapter ??= new JellyfinRequestAdapter(JellyfinAuthenticationProvider, SdkClientSettings, HttpClient);
        }

        private UserDto userDto;
        private BaseItemDto musicLibraryDto;
        public List<BaseItemDto> VideoLibraryDto { get; set; }

        private bool IsLogin = false;
        #endregion

        public async Task<IEnumerable<IGrouping<int, BaseItemDto>>> GetJellyfinAlbumGroupsAsync()
        {
            if (!IsLogin)
            {
                await JellyfinLoginAsync();
            }

            var albumsResult = await GetAlbums().ConfigureAwait(false);
            var albumGroups = GroupAllAlbumByYear(albumsResult);

            return albumGroups;
        }

        private async Task GetViews()
        {
            var views = await JellyfinApiClient.UserViews.GetAsync().ConfigureAwait(false);
            musicLibraryDto = views.Items.FirstOrDefault(v => v.CollectionType == BaseItemDto_CollectionType.Music);

            VideoLibraryDto = views.Items.Where(v => v.CollectionType == null || 
                                                v.CollectionType == BaseItemDto_CollectionType.Movies || 
                                                v.CollectionType == BaseItemDto_CollectionType.Tvshows).ToList();
        }

        public async Task<List<BaseItemDto>> GetVideoViews()
        {
            if (!IsLogin)
            {
                await JellyfinLoginAsync();
            }

            return VideoLibraryDto;
        }

        private async Task<BaseItemDtoQueryResult> GetAlbums()
        {
            var result = await JellyfinApiClient.Items.GetAsync(param =>
            {
                param.QueryParameters = new ItemsRequestBuilder.ItemsRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                    ParentId = musicLibraryDto.Id,
                    SortBy = [ItemSortBy.ProductionYear, ItemSortBy.PremiereDate, ItemSortBy.SortName],
                    SortOrder = [SortOrder.Descending],
                    IncludeItemTypes = [BaseItemKind.MusicAlbum],
                    Recursive = true,
                    Fields = [ItemFields.PrimaryImageAspectRatio, ItemFields.SortName],
                    ImageTypeLimit = 1,
                    EnableImageTypes = [ImageType.Primary, ImageType.Backdrop, ImageType.Banner, ImageType.Thumb],
                    StartIndex = 0,
                    //Limit = 100,
                };
            }).ConfigureAwait(false);
            return result;
        }

        public async Task<List<BaseItemDto>> GetJellyfinPlayListsAsync()
        {
            var result = await JellyfinApiClient.Items.GetAsync(param =>
            {
                param.QueryParameters = new ItemsRequestBuilder.ItemsRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                    SortBy = [ItemSortBy.SortName],
                    SortOrder = [SortOrder.Ascending],
                    IncludeItemTypes = [BaseItemKind.Playlist],
                    Recursive = true,
                    Fields = [ItemFields.PrimaryImageAspectRatio, ItemFields.SortName, ItemFields.CanDelete],
                    StartIndex = 0,
                    //Limit = 100,
                };
            }).ConfigureAwait(false);
            JellyfinPlayListList = result.Items;
            return result.Items;
        }

        public async Task<List<BaseItemDto>> GetJellyfinArtistListAsync(int startIndex = 0, int limit = 50)
        {
            var result = await JellyfinApiClient.Artists.GetAsync(param =>
            {
                param.QueryParameters = new ArtistsRequestBuilder.ArtistsRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                    ParentId = musicLibraryDto.Id,
                    SortBy = [ItemSortBy.SortName],
                    SortOrder = [SortOrder.Ascending],
                    Fields = [ItemFields.PrimaryImageAspectRatio, ItemFields.SortName],
                    ImageTypeLimit = 1,
                    StartIndex = startIndex,
                    EnableImageTypes = [ImageType.Primary, ImageType.Backdrop, ImageType.Banner, ImageType.Thumb],
                    Limit = limit,
                };
            }).ConfigureAwait(false);
            return result.Items;
        }

        public async Task<List<BaseItemDto>> GetJellyfinVideoListAsync(BaseItemDto library, int startIndex = 0, int limit = 50)
        {
            if (!IsLogin)
            {
                await JellyfinLoginAsync();
            }

            var result = await JellyfinApiClient.Items.GetAsync(param =>
            {
                param.QueryParameters = new ItemsRequestBuilder.ItemsRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                    ParentId = library.Id,
                    SortBy = [ItemSortBy.ProductionYear, ItemSortBy.PremiereDate, ItemSortBy.SortName],
                    SortOrder = [SortOrder.Descending],
                    Fields = [ItemFields.PrimaryImageAspectRatio, ItemFields.SortName, ItemFields.Path, ItemFields.ChildCount, ItemFields.MediaSourceCount, ],
                    ImageTypeLimit = 1,
                    StartIndex = startIndex,
                    Limit = limit,
                };
            }).ConfigureAwait(false);
            return result.Items;
        }

        static IEnumerable<IGrouping<int, BaseItemDto>> GroupAllAlbumByYear(BaseItemDtoQueryResult albums)
        {
            var r = albums.Items.GroupBy(a => a.ProductionYear ?? 0).OrderByDescending(g => g.Key);
            return r;
        }

        private Uri GetPrimaryJellyfinImageBase(BaseItemDto_ImageTags tag, Guid? parentId, int widthHeigh, string tagStr = "Primary")
        {
            if (!tag.AdditionalData.TryGetValue(tagStr, out object value)) return null;
            var requestInformation = JellyfinApiClient.Items[parentId.Value].Images[ImageType.Primary.ToString()].ToGetRequestInformation(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Items.Item.Images.Item.WithImageTypeItemRequestBuilder.WithImageTypeItemRequestBuilderGetQueryParameters
                {
                    Tag = value.ToString(),
                    FillHeight = widthHeigh,
                    FillWidth = widthHeigh,
                    Quality = 96
                };
            });
            var uri = JellyfinApiClient.BuildUri(requestInformation);
            return uri;
        }

        public Uri GetPrimaryJellyfinImage(BaseItemDto_ImageTags tag, Guid? parentId)
        {
            return GetPrimaryJellyfinImageBase(tag, parentId, 300);
        }

        public Uri GetPrimaryJellyfinImageLarge(BaseItemDto_ImageTags tag, Guid? parentId)
        {
            return GetPrimaryJellyfinImageBase(tag, parentId, 600);
        }

        public Uri GetPrimaryJellyfinImageSmall(BaseItemDto_ImageTags tag, Guid? parentId)
        {
            return GetPrimaryJellyfinImageBase(tag, parentId, 100);
        }

        public Uri GetPrimaryJellyfinImageVerySmall(BaseItemDto_ImageTags tag, Guid? parentId)
        {
            return GetPrimaryJellyfinImageBase(tag, parentId, 32);
        }

        public Uri GetArtJellyfinImage(BaseItemDto_ImageTags tag, Guid? parentId)
        {
            return GetPrimaryJellyfinImageBase(tag, parentId, 300, "Art");
        }
        public Uri GetBannerJellyfinImage(BaseItemDto_ImageTags tag, Guid? parentId)
        {
            return GetPrimaryJellyfinImageBase(tag, parentId, 300, "Banner");
        }
        public Uri GetBackdropJellyfinImage(List<string> tag, Guid? parentId)
        {
            if (tag == null || tag.Count == 0) return null;
            var requestInformation = JellyfinApiClient.Items[parentId.Value].Images[ImageType.Backdrop.ToString()].ToGetRequestInformation(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Items.Item.Images.Item.WithImageTypeItemRequestBuilder.WithImageTypeItemRequestBuilderGetQueryParameters
                {
                    Tag = tag[0],
                    FillHeight = 1000,
                    FillWidth = 1000,
                    Quality = 96
                };
            });
            var uri = JellyfinApiClient.BuildUri(requestInformation);
            return uri;
        }

        public async Task JellyfinLoginAsync()
        {
            var systemInfo = await JellyfinApiClient.System.Info.Public.GetAsync();
            // Authenticate user.
            var authenticationResult = await JellyfinApiClient.Users.AuthenticateByName.PostAsync(new AuthenticateUserByName
            {
                Username = Config.GetConfig<string>("JellyfinUserName"),
                Pw = Config.GetConfig<string>("JellyfinPassword")
            }).ConfigureAwait(false);

            SdkClientSettings.SetAccessToken(authenticationResult.AccessToken);
            userDto = authenticationResult.User;
            IsLogin = true;

            await GetViews().ConfigureAwait(false);
        }

        public async Task<List<BaseItemDto>> GetAlbumMusicItemsAsync(BaseItemDto album)
        {
            var result = await JellyfinApiClient.Items.GetAsync(param =>
            {
                param.QueryParameters = new ItemsRequestBuilder.ItemsRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                    ParentId = album.Id,
                    Fields = [ItemFields.ItemCounts, ItemFields.PrimaryImageAspectRatio, ItemFields.CanDelete, ItemFields.MediaSourceCount],
                    SortBy = [ItemSortBy.ParentIndexNumber, ItemSortBy.IndexNumber, ItemSortBy.SortName],
                };
            }).ConfigureAwait(false);
            
            return result.Items;
        }

        public async Task<List<BaseItemDto>> GetPlayListMusicItemsAsync(BaseItemDto playlist)
        {
            var result = await JellyfinApiClient.Playlists[playlist.Id.Value].Items.GetAsync(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Playlists.Item.Items.ItemsRequestBuilder.ItemsRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                    Fields = [ItemFields.ItemCounts, ItemFields.PrimaryImageAspectRatio, ItemFields.CanDelete, ItemFields.MediaSourceCount],
                    EnableImageTypes = [ImageType.Primary, ImageType.Backdrop, ImageType.Banner, ImageType.Thumb]
                };
            }).ConfigureAwait(false);

            return result.Items;
        }

        public async Task<BaseItemDto> GetPlayListInfoAsync(BaseItemDto playlist)
        {
            var result = await JellyfinApiClient.Items[playlist.Id.Value].GetAsync(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Items.Item.WithItemItemRequestBuilder.WithItemItemRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                };
            }).ConfigureAwait(false);

            return result;
        }

        public async Task<BaseItemDto> GetItemInfoAsync(BaseItemDto item)
        {
            var result = await JellyfinApiClient.Items[item.Id.Value].GetAsync(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Items.Item.WithItemItemRequestBuilder.WithItemItemRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                };
            }).ConfigureAwait(false);
            
            return result;
        }

        public async Task<BaseItemDto> GetMusicInfoAsync(BaseItemDto music)
        {
            var result = await JellyfinApiClient.Items[music.Id.Value].GetAsync(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Items.Item.WithItemItemRequestBuilder.WithItemItemRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                };
            }).ConfigureAwait(false);

            return result;
        }

        public async Task<BaseItemDto> GetAlbumInfoFromMusicAsync(BaseItemDto music)
        {
            var result = await JellyfinApiClient.Items[music.AlbumId.Value].GetAsync(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Items.Item.WithItemItemRequestBuilder.WithItemItemRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                };
            }).ConfigureAwait(false);

            return result;
        }

        public async Task<List<BaseItemDto>> GetSimilarAlbumAsync(BaseItemDto music)
        {
            var result = await JellyfinApiClient.Items[music.Id.Value].Similar.GetAsync(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Items.Item.Similar.SimilarRequestBuilder.SimilarRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                    Limit = 7,
                    Fields = [ItemFields.PrimaryImageAspectRatio, ItemFields.CanDelete],
                    ExcludeArtistIds = [.. music.AlbumArtists.Select(a => a.Id)]
                };
            }).ConfigureAwait(false);
            return result.Items;
        }

        public async Task<BaseItemDto> GetArtistAsync(Guid id)
        {
            var result = await JellyfinApiClient.Items[id].GetAsync(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Items.Item.WithItemItemRequestBuilder.WithItemItemRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                };
            }).ConfigureAwait(false);
            return result;
        }

        public async Task<List<BaseItemDto>> GetAlbumsFromArtistAsync(Guid id)
        {
            var result = await JellyfinApiClient.Items.GetAsync(param =>
            {
                param.QueryParameters = new ItemsRequestBuilder.ItemsRequestBuilderGetQueryParameters
                {
                    SortOrder = [SortOrder.Descending],
                    IncludeItemTypes = [BaseItemKind.MusicAlbum],
                    Recursive = true,
                    Fields = [ItemFields.ParentId, ItemFields.PrimaryImageAspectRatio],
                    Limit = 10,
                    StartIndex = 0,
                    CollapseBoxSetItems = false,
                    AlbumArtistIds = [id],
                    SortBy = [ItemSortBy.PremiereDate, ItemSortBy.ProductionYear, ItemSortBy.SortName]
                };
            }).ConfigureAwait(false);
            return result.Items;
        }

        public string GetMusicStream(BaseItemDto music)
        {
            var req = JellyfinApiClient.Audio[music.Id.Value].Universal.ToGetRequestInformation(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Audio.Item.Universal.UniversalRequestBuilder.UniversalRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id.Value,
                    DeviceId = DevideId,
                    MaxStreamingBitrate = 876421732,
                    Container = ["opus","webm|opus","ts|mp3","mp3","aac","m4a|aac","m4b|aac","flac","webma","webm|webma","wav","ogg"],
                    TranscodingContainer = "mp4",
                    TranscodingProtocol = Jellyfin.Sdk.Generated.Audio.Item.Universal.MediaStreamProtocol.Hls,
                    AudioCodec = "aac",
                    StartTimeTicks = 0,
                    EnableRedirection = false,
                    EnableRemoteMedia = false,
                    EnableAudioVbrEncoding = false,
                };
            });
            var uri = JellyfinApiClient.BuildUri(req);
            var url_temp = uri.ToString();
            url_temp = url_temp + "&api_key=" + SdkClientSettings.AccessToken;
            return url_temp;
        }

        public string GetVideoStream(BaseItemDto video)
        {
            var req = JellyfinApiClient.Videos[video.Id.Value].Stream.ToGetRequestInformation(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Videos.Item.StreamNamespace.StreamRequestBuilder.StreamRequestBuilderGetQueryParameters
                {
                    Static = true,
                    DeviceId = DevideId,
                    MediaSourceId = video.Id.Value.ToString()
                };
            });
            var uri = JellyfinApiClient.BuildUri(req);
            var url_temp = uri.ToString();
            url_temp = url_temp + "&api_key=" + SdkClientSettings.AccessToken;
            return url_temp;
        }

        public async Task<List<BaseItemDto>> GetSeasons(BaseItemDto bangumi)
        {
            var result = await JellyfinApiClient.Shows[bangumi.Id.Value].Seasons.GetAsync(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Shows.Item.Seasons.SeasonsRequestBuilder.SeasonsRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                    Fields = [ItemFields.ItemCounts, ItemFields.PrimaryImageAspectRatio, ItemFields.CanDelete, ItemFields.MediaSourceCount]
                };
            }).ConfigureAwait(false);
            return result.Items;
        }

        public async Task<List<BaseItemDto>> GetEpisodes(BaseItemDto season)
        {
            var result = await JellyfinApiClient.Shows[season.Id.Value].Episodes.GetAsync(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Shows.Item.Episodes.EpisodesRequestBuilder.EpisodesRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                    SeasonId = season.Id,
                    Fields = [ItemFields.ItemCounts, ItemFields.PrimaryImageAspectRatio, ItemFields.CanDelete, ItemFields.MediaSourceCount, ItemFields.Overview]
                };
            }).ConfigureAwait(false);
            return result.Items;
        }

        sealed class PlayListItemComparer : EqualityComparer<PlayListItemDb>
        {
            public override bool Equals(PlayListItemDb x, PlayListItemDb y)
            {
                if (x.Source == y.Source && x.LastWriteTime == y.LastWriteTime)
                    return true;
                return false;
            }

            public override int GetHashCode(PlayListItemDb obj)
            {
                return obj.Source.GetHashCode() + obj.LastWriteTime.GetHashCode();
            }
        }

        public (IEnumerable<AlbumItem>, List<MusicItem>) GetArtistAlbumGroup(string artistName)
        {
            return (null, null);
        }

        public AlbumItem QueryAlbum(MusicItem musicItem)
        {
            return null;
        }

        public void AddAlbumToPlayList(BaseItemDto playList, AlbumItem album)
        {

        }

        public void AddMusicToPlayList(BaseItemDto playList, MusicItem music)
        {

        }

        public void AddMusicToPlayList(BaseItemDto playList, BaseItemDto music)
        {

        }

        public void PlayListMusicDelete(BaseItemDto music)
        {

        }

        public void PlayListMusicUp(BaseItemDto music)
        {

        }

        public void PlayListMusicDown(BaseItemDto music)
        {

        }

        public void NewPlayList(string title, MusicItem initItem)
        {

        }

        public Task<List<MusicItem>> QueryMusicAsync(string name, bool exactMatch = false)
        {
            return null;
        }

        public override void Dispose()
        {
            //JellyfinApiClient.Dispose();
            //httpClient.Dispose();
        }
    }
}
