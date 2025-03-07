using System.Collections.Generic;

namespace TEngine
{
    internal class UIModule : Module, IUIModule
    {
        private IResourceModule _resourceModule;

        public override void OnInit()
        {
        }

        public override void Shutdown()
        {
            
        }

        public void Init()
        {
            _resourceModule = ModuleSystem.GetModule<IResourceModule>();
        }
    }
}