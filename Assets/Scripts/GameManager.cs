using UnityEngine.Events;
using VirtualShowcase.Utilities;

namespace VirtualShowcase
{
    // TODO: idfk what's this class, I was just trying some shit.
    public class GameManager : MonoSingleton<GameManager>
    {
        #region Event Functions

        private void Start()
        {
            menuLoaded?.Invoke();
        }

        #endregion

        public event UnityAction menuLoaded;
        public event UnityAction mainSceneLoaded;
        public event UnityAction mainSceneSwitched;
        public event UnityAction quit;
    }

    public enum eGameState
    {
        InMenu,
        InMainScene,
        InTransition,
    }
}