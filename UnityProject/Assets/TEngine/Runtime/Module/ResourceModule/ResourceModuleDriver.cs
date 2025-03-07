using System;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    [DisallowMultipleComponent]
    public class ResourceModuleDriver : MonoBehaviour
    {
        #region Propreties

        private const int DefaultPriority = 0;

        private IResourceModule m_ResourceModule;

        private bool m_ForceUnloadUnusedAssets = false;

        private bool m_PreorderUnloadUnusedAssets = false;

        private bool m_PerformGCCollect = false;

        private AsyncOperation m_AsyncOperation = null;

        private float m_LastUnloadUnusedAssetsOperationElapseSeconds = 0f;

        [SerializeField] private float m_MinUnloadUnusedAssetsInterval = 60f;

        [SerializeField] private float m_MaxUnloadUnusedAssetsInterval = 300f;

        [SerializeField] private bool m_UseSystemUnloadUnusedAssets = true;

        /// <summary>
        /// 当前最新的包裹版本。
        /// </summary>
        public string PackageVersion { set; get; }

        /// <summary>
        /// 资源包名称。
        /// </summary>
        [SerializeField] private string packageName = "DefaultPackage";

        /// <summary>
        /// 资源包名称。
        /// </summary>
        public string PackageName
        {
            get => packageName;
            set => packageName = value;
        }

        /// <summary>
        /// 资源系统运行模式。
        /// </summary>
        [SerializeField] private EPlayMode playMode = EPlayMode.EditorSimulateMode;

        /// <summary>
        /// 资源系统运行模式。
        /// <remarks>编辑器内优先使用。</remarks>
        /// </summary>
        public EPlayMode PlayMode
        {
            get
            {
#if UNITY_EDITOR
                //编辑器模式使用。
                return (EPlayMode)UnityEditor.EditorPrefs.GetInt("EditorPlayMode");
#else
                if (playMode == EPlayMode.EditorSimulateMode)
                {
                    playMode = EPlayMode.OfflinePlayMode;
                }
                //运行时使用。
                return playMode;
#endif
            }
            set
            {
#if UNITY_EDITOR
                playMode = value;
#endif
            }
        }

        /// <summary>
        /// 是否支持边玩边下载。
        /// </summary>
        [SerializeField] private bool m_UpdatableWhilePlaying = false;

        /// <summary>
        /// 是否支持边玩边下载。
        /// </summary>
        public bool UpdatableWhilePlaying => m_UpdatableWhilePlaying;

        /// <summary>
        /// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）
        /// </summary>
        [SerializeField] public long Milliseconds = 30;

        public int m_DownloadingMaxNum = 10;

        /// <summary>
        /// 获取或设置同时最大下载数目。
        /// </summary>
        public int DownloadingMaxNum
        {
            get => m_DownloadingMaxNum;
            set => m_DownloadingMaxNum = value;
        }

        public int m_FailedTryAgain = 3;

        public int FailedTryAgain
        {
            get => m_FailedTryAgain;
            set => m_FailedTryAgain = value;
        }

        /// <summary>
        /// 获取当前资源适用的游戏版本号。
        /// </summary>
        public string ApplicableGameVersion => m_ResourceModule.ApplicableGameVersion;

        /// <summary>
        /// 获取当前内部资源版本号。
        /// </summary>
        public int InternalResourceVersion => m_ResourceModule.InternalResourceVersion;

        /// <summary>
        /// 获取或设置无用资源释放的最小间隔时间，以秒为单位。
        /// </summary>
        public float MinUnloadUnusedAssetsInterval
        {
            get => m_MinUnloadUnusedAssetsInterval;
            set => m_MinUnloadUnusedAssetsInterval = value;
        }

        /// <summary>
        /// 获取或设置无用资源释放的最大间隔时间，以秒为单位。
        /// </summary>
        public float MaxUnloadUnusedAssetsInterval
        {
            get => m_MaxUnloadUnusedAssetsInterval;
            set => m_MaxUnloadUnusedAssetsInterval = value;
        }

        /// <summary>
        /// 使用系统释放无用资源策略。
        /// </summary>
        public bool UseSystemUnloadUnusedAssets
        {
            get => m_UseSystemUnloadUnusedAssets;
            set => m_UseSystemUnloadUnusedAssets = value;
        }

        /// <summary>
        /// 获取无用资源释放的等待时长，以秒为单位。
        /// </summary>
        public float LastUnloadUnusedAssetsOperationElapseSeconds => m_LastUnloadUnusedAssetsOperationElapseSeconds;

        [SerializeField] private float m_AssetAutoReleaseInterval = 60f;

        [SerializeField] private int m_AssetCapacity = 64;

        [SerializeField] private float m_AssetExpireTime = 60f;

        [SerializeField] private int m_AssetPriority = 0;

        /// <summary>
        /// 获取或设置资源对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float AssetAutoReleaseInterval
        {
            get => m_ResourceModule.AssetAutoReleaseInterval;
            set => m_ResourceModule.AssetAutoReleaseInterval = m_AssetAutoReleaseInterval = value;
        }

        /// <summary>
        /// 获取或设置资源对象池的容量。
        /// </summary>
        public int AssetCapacity
        {
            get => m_ResourceModule.AssetCapacity;
            set => m_ResourceModule.AssetCapacity = m_AssetCapacity = value;
        }

        /// <summary>
        /// 获取或设置资源对象池对象过期秒数。
        /// </summary>
        public float AssetExpireTime
        {
            get => m_ResourceModule.AssetExpireTime;
            set => m_ResourceModule.AssetExpireTime = m_AssetExpireTime = value;
        }

        /// <summary>
        /// 获取或设置资源对象池的优先级。
        /// </summary>
        public int AssetPriority
        {
            get => m_ResourceModule.AssetPriority;
            set => m_ResourceModule.AssetPriority = m_AssetPriority = value;
        }

        #endregion

        private void Start()
        {
            m_ResourceModule = ModuleSystem.GetModule<IResourceModule>();
            if (m_ResourceModule == null)
            {
                Log.Fatal("Resource module is invalid.");
                return;
            }

            if (PlayMode == EPlayMode.EditorSimulateMode)
            {
                Log.Info("During this run, ResourceModule will use editor resource files, which you should validate first.");
#if !UNITY_EDITOR
                PlayMode = EPlayMode.OfflinePlayMode;
#endif
            }

            m_ResourceModule.DefaultPackageName = PackageName;
            m_ResourceModule.PlayMode = PlayMode;
            m_ResourceModule.Milliseconds = Milliseconds;
            m_ResourceModule.HostServerURL = Settings.UpdateSetting.GetResDownLoadPath();
            m_ResourceModule.FallbackHostServerURL = Settings.UpdateSetting.GetFallbackResDownLoadPath();
            m_ResourceModule.DownloadingMaxNum = DownloadingMaxNum;
            m_ResourceModule.FailedTryAgain = FailedTryAgain;
            m_ResourceModule.UpdatableWhilePlaying = UpdatableWhilePlaying;
            m_ResourceModule.Initialize();
            m_ResourceModule.AssetAutoReleaseInterval = m_AssetAutoReleaseInterval;
            m_ResourceModule.AssetCapacity = m_AssetCapacity;
            m_ResourceModule.AssetExpireTime = m_AssetExpireTime;
            m_ResourceModule.AssetPriority = m_AssetPriority;
            m_ResourceModule.SetForceUnloadUnusedAssetsAction(ForceUnloadUnusedAssets);
            Log.Info($"ResourceModule Run Mode：{PlayMode}");
        }

        #region 释放资源

        /// <summary>
        /// 强制执行释放未被使用的资源。
        /// </summary>
        /// <param name="performGCCollect">是否使用垃圾回收。</param>
        public void ForceUnloadUnusedAssets(bool performGCCollect)
        {
            m_ForceUnloadUnusedAssets = true;
            if (performGCCollect)
            {
                m_PerformGCCollect = true;
            }
        }


        private void Update()
        {
            m_LastUnloadUnusedAssetsOperationElapseSeconds += Time.unscaledDeltaTime;
            if (m_AsyncOperation == null && (m_ForceUnloadUnusedAssets || m_LastUnloadUnusedAssetsOperationElapseSeconds >= m_MaxUnloadUnusedAssetsInterval ||
                                             m_PreorderUnloadUnusedAssets && m_LastUnloadUnusedAssetsOperationElapseSeconds >= m_MinUnloadUnusedAssetsInterval))
            {
                Log.Info("Unload unused assets...");
                m_ForceUnloadUnusedAssets = false;
                m_PreorderUnloadUnusedAssets = false;
                m_LastUnloadUnusedAssetsOperationElapseSeconds = 0f;
                m_AsyncOperation = Resources.UnloadUnusedAssets();
                if (m_UseSystemUnloadUnusedAssets)
                {
                    m_ResourceModule.UnloadUnusedAssets();
                }
            }

            if (m_AsyncOperation is { isDone: true })
            {
                m_AsyncOperation = null;
                if (m_PerformGCCollect)
                {
                    Log.Info("GC.Collect...");
                    m_PerformGCCollect = false;
                    GC.Collect();
                }
            }
        }

        #endregion
    }
}