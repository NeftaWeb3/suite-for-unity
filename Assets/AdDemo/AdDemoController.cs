using System.Collections.Generic;
using Nefta.Ads;
using UnityEngine;

namespace AdDemo
{
    public class AdDemoController : MonoBehaviour
    {
#if UNITY_IOS
        private const string BannerAdUnitId = "5726295757422592";
#else
        private const string BannerAdUnitId = "5679149674921984";
#endif
        
        [SerializeField] private RectTransform _contentRect;
        [SerializeField] private RectTransform _placementRect;

        [SerializeField] private BannerController bannerPrefab;
        [SerializeField] private InteractiveController _interactivePrefab;

        private NeftaAds _neftaAds;
        private bool _isBannerShown;
        private Dictionary<string, PlacementController> _placementControllers;
        private DebugServer _debugServer;
        
        private void Awake()
        {
            _neftaAds = NeftaAds.Init();
            var debugParams = GetDebugParameters();
            if (debugParams != null)
            {
                _neftaAds.PluginWrapper.SetOverride($"http://{debugParams[0]}:8080");
                
                _debugServer = new DebugServer();
                _debugServer.Init(debugParams[0], debugParams[1]);
            }
            _neftaAds.OnReady = OnReady;
            _neftaAds.OnBid = OnBid;
            _neftaAds.OnLoadStart = OnLoadStart;
            _neftaAds.OnLoadFail = OnLoadFail;
            _neftaAds.OnLoad = OnLoad;
            _neftaAds.OnShowFail = OnShowFail;
            _neftaAds.OnShow = OnShow;
            _neftaAds.OnClose = OnClose;
            _neftaAds.OnUserRewarded = OnUserRewarded;
            _neftaAds.Enable(true);
            
            AdjustOffsets(0);
        }

        private void Update()
        {
            if (_debugServer != null)
            {
                _debugServer.OnUpdate();
            }
            if (_neftaAds != null)
            {
                _neftaAds.OnUpdate();
            }
        }
        
        private void OnReady(Dictionary<string, AdUnit> placements)
        {
            _placementControllers = new Dictionary<string, PlacementController>();
            foreach (var placement in placements)
            {
                if (placement.Value._type == AdUnit.Type.Banner)
                {
                    var bannerController = Instantiate(bannerPrefab, _placementRect);
                    bannerController.SetData(placement.Value, AdjustOffsets);
                    _placementControllers.Add(placement.Key, bannerController);
                }
                else
                {
                    var interactiveController = Instantiate(_interactivePrefab, _placementRect);
                    interactiveController.SetData(placement.Value);
                    _placementControllers.Add(placement.Key, interactiveController);
                }
                
                if (placement.Key == BannerAdUnitId)
                {
                    _neftaAds.SetFloorPrice(BannerAdUnitId, 0.42f);

                    _neftaAds.CreateBanner(BannerAdUnitId, NeftaAds.BannerPosition.Top, true);
                    NeftaAds.Instance.Show(BannerAdUnitId);
                }
            }
        }

        private void OnBid(AdUnit adUnit)
        {
            _placementControllers[adUnit._id].OnBid();
        }
        
        private void OnLoadStart(AdUnit adUnit)
        {
            _placementControllers[adUnit._id].OnLoadStart();
        }
        
        private void OnLoadFail(AdUnit adUnit, string error)
        {
            _placementControllers[adUnit._id].OnLoadFail(error);
        }

        private void OnLoad(AdUnit adUnit)
        {
            _placementControllers[adUnit._id].OnLoad();
        }

        private void OnShowFail(AdUnit adUnit, string error)
        {
            _placementControllers[adUnit._id].OnShowFail(error);
        }

        private void OnShow(AdUnit adUnit)
        {
            _placementControllers[adUnit._id].OnShow();
            if (adUnit._type == AdUnit.Type.Banner)
            {
                AdjustOffsets(adUnit.Height);
            }
        }

        private void OnClose(AdUnit adUnit)
        {
            _placementControllers[adUnit._id].OnClose();
            if (adUnit._type == AdUnit.Type.Banner)
            {
                AdjustOffsets(0);
            }
        }

        private void OnUserRewarded(AdUnit adUnit)
        {
            Debug.Log($"OnUserRewarded for placement {adUnit._id}");
        }
        
        private void AdjustOffsets(int bannerHeight)
        {
            var topObstruction = Screen.height - Screen.safeArea.height - Screen.safeArea.y;
            _contentRect.offsetMax = new Vector2(0, -(topObstruction + bannerHeight) / Screen.height) * ((RectTransform)transform).rect.size.y;
        }

        private string[] GetDebugParameters()
        {
            string root = null;
            string serial = null;
#if UNITY_EDITOR
            root = "192.168.0.223";
            serial = "emulator-sim-4";
#elif UNITY_IOS
            string[] args = System.Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                root = args[1];
            }
            if (args.Length > 2)
            {
                serial = args[2];
            }
#elif UNITY_ANDROID
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");
            root = intent.Call<string>("getStringExtra", "override");
            serial = intent.Call<string>("getStringExtra", "serial");
#endif
            if (!string.IsNullOrEmpty(root))
            {
                return new[]{ root, serial };
            }

            return null;
        }
    }
}
