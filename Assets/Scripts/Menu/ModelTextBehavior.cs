using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VirtualShowcase.MainScene;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Menu
{
    public class ModelTextBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private static readonly Color hoverColor = Color.red;
        private Color baseColor;
        private TextMeshProUGUI textMeshPro;

        public string ModelPath => gameObject.name;
        public string ModelName => textMeshPro.text;

        #region Event Functions

        private void Start()
        {
            textMeshPro = GetComponent<TextMeshProUGUI>();
            baseColor = textMeshPro.color;
        }

        #endregion

        #region IPointerClickHandler Members

        public void OnPointerClick(PointerEventData eventData)
        {
            // I'M LOSING MY SANITY.
            // OK. SO... ORIGINALLY THERE COULD BE ONLY ONE MODEL LOADED. NOW THERE CAN BE MULTIPLE.
            // I NEED A WAY TO REMOVE A SPECIFIC MODEL. DIDN'T KNOW HOW,
            // SO I JUST MADE THE MODEL NAME TEXT OBJECT CLICKABLE.
            // THE TMP TEXT STORES THE FILE NAME, WHILE THE GAMEOBJECT NAME STORES THE FULL PATH.
            // IT'S SO FUCKING STUPID. I NEED THE INFO ABOUT THE PATH.
            // THIS SHIT LITERALLY COULDN'T BE MORE COUPLED.
            // ACTUALLY THIS ENTIRE FUCKING PROJECT IS A GARBAGE DUMP.
            // IT'S IMPOSSIBLE TO CHANGE ANYTHING BECAUSE I'M BRAINDEAD AND EVERY. SINGLE. PART. IS. COUPLED.

            MyPrefs.RemoveModelPath(ModelPath);
            ModelLoader.Instance.DeleteModel(ModelPath);
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

        /// <summary>
        ///     XDDD
        /// </summary>
        /// <param name="text">So umm.. there's a "template" object somewhere in the menu hierarchy. Put it here pls.</param>
        /// <param name="path">Full model path</param>
        public static void InstantiateModelName(TMP_Text text, string path)
        {
            TMP_Text cpy = Instantiate(text, text.transform.parent);
            cpy.text = Path.GetFileNameWithoutExtension(path);
            cpy.gameObject.SetActive(true);
            cpy.gameObject.name = path;
        }
    }
}