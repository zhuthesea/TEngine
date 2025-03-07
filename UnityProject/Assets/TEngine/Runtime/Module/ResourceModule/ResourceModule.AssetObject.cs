using System.Collections.Generic;
using YooAsset;

namespace TEngine
{
    internal partial class ResourceModule
    {
        /// <summary>
        /// 资源对象。
        /// </summary>
        private sealed class AssetObject : ObjectBase
        {
            private AssetHandle m_AssetHandle;
            private ResourceModule m_ResourceManager;


            public AssetObject()
            {
                m_AssetHandle = null;
            }

            public static AssetObject Create(string name, object target, object assetHandle, ResourceModule resourceModule)
            {
                if (assetHandle == null)
                {
                    throw new GameFrameworkException("Resource is invalid.");
                }

                if (resourceModule == null)
                {
                    throw new GameFrameworkException("Resource Manager is invalid.");
                }

                AssetObject assetObject = MemoryPool.Acquire<AssetObject>();
                assetObject.Initialize(name, target);
                assetObject.m_AssetHandle = (AssetHandle)assetHandle;
                assetObject.m_ResourceManager = resourceModule;
                return assetObject;
            }

            public override void Clear()
            {
                base.Clear();
                m_AssetHandle = null;
            }

            protected internal override void OnUnspawn()
            {
                base.OnUnspawn();
            }

            protected internal override void Release(bool isShutdown)
            {
                if (!isShutdown)
                {
                    AssetHandle handle = m_AssetHandle;
                    if (handle is { IsValid: true })
                    {
                        handle.Dispose();
                    }
                    handle = null;
                }
            }
        }
    }
}