using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TEngine
{
    [Serializable]
    public struct AssetsRefInfo
    {
        public int instanceId;

        public Object refAsset;

        public AssetsRefInfo(Object refAsset)
        {
            this.refAsset = refAsset;
            instanceId = this.refAsset.GetInstanceID();
        }
    }

    public sealed class AssetsReference : MonoBehaviour
    {
        [SerializeField] private GameObject _sourceGameObject;

        [SerializeField] private List<AssetsRefInfo> _refAssetInfoList;

        private IResourceModule _resourceModule;

        private void OnDestroy()
        {
            if (_resourceModule == null)
            {
                _resourceModule = ModuleSystem.GetModule<IResourceModule>();
            }

            if (_resourceModule == null)
            {
                throw new GameFrameworkException($"resourceModule is null.");
            }

            if (_sourceGameObject != null)
            {
                _resourceModule.UnloadAsset(_sourceGameObject);
            }

            ReleaseRefAssetInfoList();
        }

        private void ReleaseRefAssetInfoList()
        {
            if (_refAssetInfoList != null)
            {
                foreach (var refInfo in _refAssetInfoList)
                {
                    _resourceModule.UnloadAsset(refInfo.refAsset);
                }

                _refAssetInfoList.Clear();
            }
        }

        public AssetsReference Ref(GameObject source, IResourceModule resourceModule = null)
        {
            if (source == null)
            {
                throw new GameFrameworkException($"Source gameObject is null.");
            }

            if (source.scene.name != null)
            {
                throw new GameFrameworkException($"Source gameObject is in scene.");
            }

            _resourceModule = resourceModule;
            _sourceGameObject = source;
            return this;
        }

        public AssetsReference Ref<T>(T source, IResourceModule resourceModule = null) where T : UnityEngine.Object
        {
            if (source == null)
            {
                throw new GameFrameworkException($"Source gameObject is null.");
            }

            _resourceModule = resourceModule;
            if (_refAssetInfoList == null)
            {
                _refAssetInfoList = new List<AssetsRefInfo>();
            }
            _refAssetInfoList.Add(new AssetsRefInfo(source));
            return this;
        }

        public static AssetsReference Instantiate(GameObject source, Transform parent = null, IResourceModule resourceModule = null)
        {
            if (source == null)
            {
                throw new GameFrameworkException($"Source gameObject is null.");
            }

            if (source.scene.name != null)
            {
                throw new GameFrameworkException($"Source gameObject is in scene.");
            }

            GameObject instance = Object.Instantiate(source, parent);
            return instance.AddComponent<AssetsReference>().Ref(source, resourceModule);
        }

        public static AssetsReference Ref(GameObject source, GameObject instance, IResourceModule resourceModule = null)
        {
            if (source == null)
            {
                throw new GameFrameworkException($"Source gameObject is null.");
            }

            if (source.scene.name != null)
            {
                throw new GameFrameworkException($"Source gameObject is in scene.");
            }

            var comp = instance.GetComponent<AssetsReference>();
            return comp ? comp : instance.AddComponent<AssetsReference>().Ref(source, resourceModule);
        }

        public static AssetsReference Ref<T>(T source, GameObject instance, IResourceModule resourceModule = null) where T : UnityEngine.Object
        {
            if (source == null)
            {
                throw new GameFrameworkException($"Source gameObject is null.");
            }

            var comp = instance.GetComponent<AssetsReference>();
            return comp ? comp : instance.AddComponent<AssetsReference>().Ref(source, resourceModule);
        }
    }
}