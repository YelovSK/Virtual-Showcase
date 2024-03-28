using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VirtualShowcase.Core;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Menu
{
    public class ModelTextBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private static readonly Color hoverColor = Color.red;

        private Color baseColor;
        private TextMeshProUGUI textMeshPro;
        private string _modelPath;

        public void SetModelName(string path)
        {
            // Object is disabled by default, so cannot assign this in the Awake().
            textMeshPro = GetComponent<TextMeshProUGUI>();
            baseColor = textMeshPro.color;

            _modelPath = path;
            textMeshPro.text = Path.GetFileNameWithoutExtension(path);

            gameObject.name = path;
            gameObject.SetActive(true);
        }
        
        #region IPointerClickHandler Members
        
        public void OnPointerClick(PointerEventData eventData)
        {
            MyPrefs.RemoveModelPath(_modelPath);
            Events.ModelRemoved?.Invoke(gameObject, _modelPath);
            Destroy(gameObject);
        }

        #endregion

        #region IPointerEnterHandler Members

        public void OnPointerEnter(PointerEventData eventData)
        {
            textMeshPro.color = hoverColor;
        }

        #endregion

        #region IPointerExitHandler Members

        public void OnPointerExit(PointerEventData eventData)
        {
            textMeshPro.color = baseColor;
        }

        #endregion
    }
}