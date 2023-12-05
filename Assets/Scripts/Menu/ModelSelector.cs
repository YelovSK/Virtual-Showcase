using System.Collections;
using System.Linq;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;

namespace VirtualVitrine.Menu
{
    public class ModelSelector : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private TMP_Text baseModelText;

        #endregion

        public void ShowFileExplorer()
        {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("GLB", ".glb"));
            FileBrowser.SetDefaultFilter(".glb");
            FileBrowser.AddQuickLink("Desktop", "Desktop");
            FileBrowser.AddQuickLink("Local models", "Models");
            StartCoroutine(ShowLoadDialogCoroutine());
        }

        private IEnumerator ShowLoadDialogCoroutine()
        {
            yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, false, null, null,
                "Select model", "Load");

            if (!FileBrowser.Success) yield break;

            string path = FileBrowser.Result.First();

            if (MyPrefs.AddModelPath(path)) ModelTextBehavior.InstantiateModelName(baseModelText, path);
        }
    }
}