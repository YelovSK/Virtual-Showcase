using System.Collections;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using VirtualShowcase.Core;

namespace VirtualShowcase.Menu
{
    public class ModelSelector : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private TMP_Text baseModelText;

        #endregion

        public void ShowFileExplorer()
        {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Models", ".glb"));
            FileBrowser.SetDefaultFilter(".glb");
            FileBrowser.AddQuickLink("Desktop", "Desktop");
            FileBrowser.AddQuickLink("Local models", "Models");
            StartCoroutine(ShowLoadDialogCoroutine());
        }

        private IEnumerator ShowLoadDialogCoroutine()
        {
            yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null,
                "Select model", "Load");

            if (!FileBrowser.Success) yield break;

            foreach (string path in FileBrowser.Result)
            {
                if (MyPrefs.AddModelPath(path))
                    ModelTextBehavior.InstantiateModelName(baseModelText, path);
            }
        }
    }
}