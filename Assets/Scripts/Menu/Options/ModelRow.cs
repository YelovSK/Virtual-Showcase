using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VirtualShowcase.Menu.Tooltip;

namespace VirtualShowcase.Menu.Options
{
    /// <summary>
    ///     ModelRow is a row in the model list in the model options menu.
    /// </summary>
    public class ModelRow : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private TMP_Text modelNameText;

        [SerializeField]
        private TMP_Text modelSizeText;

        [SerializeField]
        private Button removeButton;

        [SerializeField]
        private TooltipTrigger pathTooltip;

        #endregion

        [NonSerialized]
        public readonly UnityEvent OnRemove = new();

        [NonSerialized]
        public string FullPath;

        #region Event Functions

        private void Awake()
        {
            removeButton.onClick.AddListener(() => OnRemove.Invoke());
        }

        #endregion

        public void SetModel(string path)
        {
            FullPath = path;
            modelNameText.text = Path.GetFileName(FullPath);
            modelSizeText.text = $"{new FileInfo(path).Length / (1024 * 1024)} MB";
            pathTooltip.SetTooltip(path);
        }
    }
}