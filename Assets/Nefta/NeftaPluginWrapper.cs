#if UNITY_EDITOR
using Nefta.Editor;
#elif UNITY_IOS
using System;
using System.Runtime.InteropServices;
using AOT;
#endif
using UnityEngine;

namespace Nefta
{
    public class NeftaPluginWrapper : MonoBehaviour
    {
#if UNITY_EDITOR
        private NeftaPlugin _pluginWrapper;
#elif UNITY_IOS
        private delegate void OnReadyDelegate(string configuration);
        private delegate void OnBidDelegate(string pId, float price, int expirationTime);
        private delegate void OnFailDelegate(string pId, int code, string error);
        private delegate void OnLoadDelegate(string pId, int width, int height);
        private delegate void OnChangeDelegate(string pId);
 
        [MonoPInvokeCallback(typeof(OnReadyDelegate))] 
        private static void OnReady(string configuration) {
            _listener?.IOnReady(configuration);
        }

        [MonoPInvokeCallback(typeof(OnBidDelegate))] 
        private static void OnBid(string pId, float price, int expirationTime) {
            _listener?.IOnBid(pId, price, expirationTime);
        }

        [MonoPInvokeCallback(typeof(OnChangeDelegate))] 
        private static void OnLoadStart(string pId) {
            _listener?.IOnLoadStart(pId);
        }

        [MonoPInvokeCallback(typeof(OnFailDelegate))] 
        private static void OnLoadFail(string pId, int code, string error) {
            _listener?.IOnLoadFail(pId, code, error);
        }

        [MonoPInvokeCallback(typeof(OnLoadDelegate))] 
        private static void OnLoad(string pId, int width, int height) {
            _listener?.IOnLoad(pId, width, height);
        }

        [MonoPInvokeCallback(typeof(OnFailDelegate))] 
        private static void OnShowFail(string pId, int code, string error) {
            _listener?.IOnShowFail(pId, code, error);
        }

        [MonoPInvokeCallback(typeof(OnChangeDelegate))] 
        private static void OnShow(string pId) {
            _listener?.IOnShow(pId);
        }

        [MonoPInvokeCallback(typeof(OnChangeDelegate))] 
        private static void OnClick(string pId) {
            _listener?.IOnClick(pId);
        }

        [MonoPInvokeCallback(typeof(OnChangeDelegate))] 
        private static void OnClose(string pId) {
            _listener?.IOnClose(pId);
        }

        [MonoPInvokeCallback(typeof(OnChangeDelegate))] 
        private static void OnReward(string pId) {
            _listener?.IOnReward(pId);
        }

        [DllImport ("__Internal")]
        private static extern void NeftaPlugin_EnableLogging(bool enable);

        [DllImport ("__Internal")]
        private static extern IntPtr UnityWrapper_Init(string appId);

        [DllImport ("__Internal")]
        private static extern void UnityWrapper_RegisterCallbacks(OnReadyDelegate onReady, OnBidDelegate onBid, OnChangeDelegate onLoadStart, OnFailDelegate onLoadFail, OnLoadDelegate onLoad, OnFailDelegate onShowFail, OnChangeDelegate onShow, OnChangeDelegate onClick, OnChangeDelegate onReward, OnChangeDelegate onClose);

        [DllImport ("__Internal")]
        private static extern void UnityWrapper_Record(int type, int category, int subCategory, string nameValue, long value, string customPayload);
            
        [DllImport ("__Internal")]
        private static extern void UnityWrapper_SetPublisherUserId(string publisherUserId);

        [DllImport ("__Internal")]
        private static extern void UnityWrapper_EnableAds(bool enable);

        [DllImport ("__Internal")]
        private static extern void UnityWrapper_CreateBannerWithId(string pId, int position, bool autoRefresh);

        [DllImport ("__Internal")]
        private static extern void UnityWrapper_SetFloorPriceWithId(string pId, float floorPrice);

        [DllImport ("__Internal")]
        private static extern string UnityWrapper_GetPartialBidRequest(string pid);
            
        [DllImport ("__Internal")]
        private static extern void UnityWrapper_BidWithId(string pId);
            
        [DllImport ("__Internal")]
        private static extern void UnityWrapper_LoadWithId(string pId);

        [DllImport ("__Internal")]
        private static extern void UnityWrapper_LoadWithBidResponse(string pId, string bidResponse);

        [DllImport ("__Internal")]
        private static extern int UnityWrapper_CanShowWithId(string pId);

        [DllImport ("__Internal")]
        private static extern void UnityWrapper_ShowWithId(string pId);

        [DllImport ("__Internal")]
        private static extern void UnityWrapper_HideWithId(string pId);
            
        [DllImport ("__Internal")]
        private static extern void UnityWrapper_CloseWithId(string pId);

        [DllImport ("__Internal")]
        private static extern void UnityWrapper_MuteWithId(string pId, bool mute);

        [DllImport ("__Internal")]
        private static extern void UnityWrapper_SetOverride(string root);

        [DllImport ("__Internal")]
        private static extern string UnityWrapper_GetNuid(bool present);

        private static INeftaListener _listener;
#elif UNITY_ANDROID
        private AndroidJavaObject _pluginWrapper;
#endif

		public static NeftaPluginWrapper Instance;
        
        public static void EnableLogging(bool enable)
        {
#if UNITY_EDITOR
            NeftaPlugin.EnableLogging(enable);
#elif UNITY_IOS
            NeftaPlugin_EnableLogging(enable);
#endif
        }

        public void Init(string appId, NeftaPluginListener listener)
		{
			Instance = this;
#if UNITY_EDITOR
            _pluginWrapper = NeftaPlugin.Init(gameObject, appId);
            _pluginWrapper._listener = listener;
#elif UNITY_IOS
            UnityWrapper_Init(appId);
            _listener = listener;
            UnityWrapper_RegisterCallbacks(OnReady, OnBid, OnLoadStart, OnLoadFail, OnLoad, OnShowFail, OnShow, OnClick, OnClose, OnReward);
#elif UNITY_ANDROID
            AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject _unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");

            _pluginWrapper = new AndroidJavaObject("com.nefta.sdk.Unity.UnityWrapper", _unityActivity, appId, listener);
#endif
            DontDestroyOnLoad(gameObject);
        }
        
#if !UNITY_EDITOR && UNITY_ANDROID
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                _pluginWrapper.Call("OnPause");
            }
            else
            {
                 _pluginWrapper.Call("OnResume");
            }
        }
#endif

        public string GetNuid(bool present)
        {
            string nuid = null;
#if UNITY_EDITOR
            nuid = _pluginWrapper.GetNuid(present);
#elif UNITY_IOS
            nuid = UnityWrapper_GetNuid(present);
#elif UNITY_ANDROID
            nuid = _pluginWrapper.Call<string>("GetNuid", present);
#endif
            return nuid;
        }
        
        public void SetPublisherUserId(string publisherUserId)
        {
#if UNITY_EDITOR
            _pluginWrapper.SetPublisherUserId(publisherUserId);
#elif UNITY_IOS
            UnityWrapper_SetPublisherUserId(publisherUserId);
#elif UNITY_ANDROID
            _pluginWrapper.Call("SetPublisherUserId", publisherUserId);
#endif
        }

        public void Record(int type, int category, int subCategory, string nameValue, long value, string customPayload)
        {
#if UNITY_EDITOR
            _pluginWrapper.Record(type, category, subCategory, nameValue, value, customPayload);
#elif UNITY_IOS
            UnityWrapper_Record(type, category, subCategory, nameValue, value, customPayload);
#elif UNITY_ANDROID
            _pluginWrapper.Call("Record", type, category, subCategory, nameValue, value, customPayload);
#endif
        }
        
        public void EnableAds(bool enable)
        {
#if UNITY_EDITOR
            _pluginWrapper.EnableAds(enable);
#elif UNITY_IOS
            UnityWrapper_EnableAds(enable);
#elif UNITY_ANDROID
            _pluginWrapper.Call("EnableAds", enable);
#endif
        }

        public void SetFloorPrice(string placementId, float floorPrice)
        {
#if UNITY_EDITOR
            _pluginWrapper.SetFloorPrice(placementId, floorPrice);
#elif UNITY_IOS
            UnityWrapper_SetFloorPriceWithId(placementId, floorPrice);
#elif UNITY_ANDROID
            _pluginWrapper.Call("SetFloorPrice", placementId, floorPrice);
#endif
        }
        
        public string GetPartialBidRequest(string placementId)
        {
            string partialBid = null;
#if UNITY_EDITOR
            partialBid = _pluginWrapper.GetPartialBidRequest(placementId);
#elif UNITY_IOS
            partialBid = UnityWrapper_GetPartialBidRequest(placementId);
#elif UNITY_ANDROID
            partialBid = _pluginWrapper.Call<string>("GetPartialBidRequestAsString", placementId);
#endif
            return partialBid;
        }
        
        public void Bid(string id)
        {
#if UNITY_EDITOR
            NeftaPlugin.Instance.Bid(id);
#elif UNITY_IOS
            UnityWrapper_BidWithId(id);
#elif UNITY_ANDROID
            _pluginWrapper.Call("Bid", id);
#endif
        }

        public void CreateBanner(string id, int position, bool autoRefresh)
        {
#if UNITY_EDITOR
            NeftaPlugin.Instance.CreateBanner(id, position, autoRefresh);
#elif UNITY_IOS
            UnityWrapper_CreateBannerWithId(id, position, autoRefresh);
#elif UNITY_ANDROID
            _pluginWrapper.Call("CreateBanner", id, position, autoRefresh);
#endif
        }
        
        public void Load(string id)
        {
#if UNITY_EDITOR
            NeftaPlugin.Instance.Load(id);
#elif UNITY_IOS
            UnityWrapper_LoadWithId(id);
#elif UNITY_ANDROID
            _pluginWrapper.Call("Load", id);
#endif
        }

        public void LoadWithBidResponse(string id, string bidResponse)
        {
#if UNITY_EDITOR
            NeftaPlugin.Instance.LoadWithBidResponse(id, bidResponse);
#elif UNITY_IOS
            UnityWrapper_LoadWithBidResponse(id, bidResponse);
#elif UNITY_ANDROID
            _pluginWrapper.Call("LoadWithBidResponse", id, bidResponse);
#endif
        }

        public int CanShow(string id)
        {
#if UNITY_EDITOR
            return _pluginWrapper.CanShow(id);
#elif UNITY_IOS
            return UnityWrapper_CanShowWithId(id);
#elif UNITY_ANDROID
            return _pluginWrapper.Call<int>("CanShow", id);
#endif
        }
        
        public void Show(string id)
        {
#if UNITY_EDITOR
            _pluginWrapper.Show(id);
#elif UNITY_IOS
            UnityWrapper_ShowWithId(id);
#elif UNITY_ANDROID
            _pluginWrapper.Call("Show", id);
#endif
        }
        
        public void Hide(string id)
        {
#if UNITY_EDITOR
            _pluginWrapper.Hide(id);
#elif UNITY_IOS
            UnityWrapper_HideWithId(id);
#elif UNITY_ANDROID
            _pluginWrapper.Call("Hide", id);
#endif
        }
        
        public void Close(string id)
        {
#if UNITY_EDITOR
            _pluginWrapper.Close(id);
#elif UNITY_IOS
            UnityWrapper_CloseWithId(id);
#elif UNITY_ANDROID
            _pluginWrapper.Call("Close", id);
#endif
        }
        
        public void Mute(string id, bool mute)
        {
#if UNITY_EDITOR
            _pluginWrapper.Mute(id, mute);
#elif UNITY_IOS
            UnityWrapper_MuteWithId(id, mute);
#elif UNITY_ANDROID
            _pluginWrapper.Call<string>("Mute", id, mute);
#endif
        }
        
        public void SetOverride(string root) {
#if UNITY_EDITOR
         
#elif UNITY_IOS
            UnityWrapper_SetOverride(root);
#elif UNITY_ANDROID
            _pluginWrapper.Call("SetOverride", root);
#endif
        }
    }
}