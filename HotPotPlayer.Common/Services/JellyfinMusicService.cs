using CommunityToolkit.Mvvm.Collections;
using HotPotPlayer.Bilibili.Models.User;
using HotPotPlayer.Models;
using Jellyfin.Sdk;
using Jellyfin.Sdk.Generated.Items;
using Jellyfin.Sdk.Generated.Models;
using Microsoft.UI.Dispatching;
using NAudio.CoreAudioApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SortOrder = Jellyfin.Sdk.Generated.Models.SortOrder;

namespace HotPotPlayer.Services
{
    public class JellyfinMusicService: ServiceBaseWithConfig
    {
        public JellyfinMusicService(ConfigBase config, DispatcherQueue uiQueue = null, AppBase app = null) : base(config, uiQueue, app) { }
        
        #region State
        private LocalServiceState _state = LocalServiceState.Idle;

        public LocalServiceState State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        #endregion
        #region Property
        private ObservableGroupedCollection<int, BaseItemDto> _localAlbumGroup = [];
        private ReadOnlyObservableGroupedCollection<int, BaseItemDto> _localAlbumGroup2;
        public ReadOnlyObservableGroupedCollection<int, BaseItemDto> LocalAlbumGroup
        {
            get => _localAlbumGroup2 ??= new(_localAlbumGroup);
        }

        private ObservableCollection<PlayListItem> _localPlayListList;

        public ObservableCollection<PlayListItem> LocalPlayListList
        {
            get => _localPlayListList;
            set => Set(ref _localPlayListList, value);
        }
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
                        "My-Jellyfin-Client",
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
        private BaseItemDto musicDto;

        private bool IsLogin = false;
        #endregion

        public async Task LoadJellyfinMusicAsync()
        {
            if (!IsLogin)
            {
                await JellyfinLoginAsync();
            }

            var views = await JellyfinApiClient.UserViews.GetAsync().ConfigureAwait(false);
            musicDto = views.Items.FirstOrDefault(v => v.CollectionType == BaseItemDto_CollectionType.Music);

            var albumsResult = await JellyfinApiClient.Items.GetAsync(param => 
            {
                param.QueryParameters = new ItemsRequestBuilder.ItemsRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                    ParentId = musicDto.Id,
                    SortBy = [ItemSortBy.ProductionYear, ItemSortBy.PremiereDate, ItemSortBy.SortName],
                    SortOrder = [SortOrder.Descending],
                    IncludeItemTypes = [BaseItemKind.MusicAlbum],
                    Recursive =true,
                    Fields = [ItemFields.PrimaryImageAspectRatio, ItemFields.SortName],
                    ImageTypeLimit = 1,
                    EnableImageTypes = [ImageType.Primary, ImageType.Backdrop, ImageType.Banner, ImageType.Thumb],
                    StartIndex = 0,
                    Limit = 100,
                };
            }).ConfigureAwait(false);

            var albumGroups = GroupAllAlbumByYear(albumsResult);
            UIQueue.TryEnqueue(() =>
            {
                foreach (var albumGroup in albumGroups)
                {
                    _localAlbumGroup.AddGroup(albumGroup);
                }
                LocalPlayListList = new();
            });

            //foreach (var view in views.Items)
            //{
            //    var requestInformation = JellyfinApiClient.Items[view.Id.Value].Images[ImageType.Primary.ToString()]
            //        .ToGetRequestInformation();
            //    var uri = JellyfinApiClient.BuildUri(requestInformation);

            //    Debug.WriteLine($"{view.Id} - {view.Name} - {uri.ToString()}");
            //}
        }

        static IEnumerable<IGrouping<int, BaseItemDto>> GroupAllAlbumByYear(BaseItemDtoQueryResult albums)
        {
            var r = albums.Items.GroupBy(a => a.ProductionYear ?? 0).OrderByDescending(g => g.Key);
            return r;
        }

        public Uri GetPrimaryJellyfinImage(BaseItemDto_ImageTags tag, Guid? parentId)
        {
            if (!tag.AdditionalData.TryGetValue("Primary", out object value)) return null;
            var requestInformation = JellyfinApiClient.Items[parentId.Value].Images[ImageType.Primary.ToString()].ToGetRequestInformation(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Items.Item.Images.Item.WithImageTypeItemRequestBuilder.WithImageTypeItemRequestBuilderGetQueryParameters
                {
                    Tag = value.ToString(),
                    FillHeight = 300,
                    FillWidth = 300,
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

        public async Task<BaseItemDto> GetAlbumInfoAsync(BaseItemDto album)
        {
            var result = await JellyfinApiClient.Items[album.Id.Value].GetAsync(param =>
            {
                param.QueryParameters = new Jellyfin.Sdk.Generated.Items.Item.WithItemItemRequestBuilder.WithItemItemRequestBuilderGetQueryParameters
                {
                    UserId = userDto.Id,
                };
            }).ConfigureAwait(false);
            
            return result;
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
            return uri.ToString();
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

        public void AddAlbumToPlayList(string playList, AlbumItem album)
        {

        }

        public void AddMusicToPlayList(string playList, MusicItem music)
        {

        }

        public void PlayListMusicDelete(MusicItem music)
        {

        }

        public void PlayListMusicUp(MusicItem music)
        {

        }

        public void PlayListMusicDown(MusicItem music)
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
            JellyfinApiClient.Dispose();
            base.Dispose();
        }
    }
}
