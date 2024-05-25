using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VirtualShowcase.Core;
using VirtualShowcase.Enums;

namespace VirtualShowcase.Showcase
{
    public class SceneButton : MonoBehaviour
    {
        [SerializeField]
        private MainScenes _scene;

        [SerializeField]
        private Button _button;
        
        private void Awake()
        {
            _button.interactable = SceneManager.GetActiveScene().name != MySceneManager.Instance.GetSceneName(_scene);
            _button.onClick.AddListener(() => SceneManager.LoadScene(MySceneManager.Instance.GetSceneName(_scene)));
            
            var colors = _button.colors;
            colors.disabledColor = Color.gray;
            _button.colors = colors;
        }
    }
}