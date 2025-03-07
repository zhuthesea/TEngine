using TEngine;

namespace GameLogic
{
    [EventInterface(EEventGroup.GroupUI)]
    public interface ILoginUI
    {
        void ShowLoginUI(UnityEngine.Networking.NetworkError tNetworkError);

        void CloseLoginUI();
    }
}