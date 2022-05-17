using System.Collections;
using System.Linq;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using VirtualVitrine.MainScene;

namespace VirtualVitrine.Menu
{
    public class ModelSelector : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private TMP_Text modelText;

        #endregion


        public void ShowFileExplorer()
        {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Objects", ".obj"));
            FileBrowser.SetDefaultFilter(".obj");
            FileBrowser.AddQuickLink("Desktop", "Desktop");
            FileBrowser.AddQuickLink("Local models", "Models");
            StartCoroutine(ShowLoadDialogCoroutine());
        }


        private IEnumerator ShowLoadDialogCoroutine()
        {
            yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null,
                "Load Files and Folders", "Load");

            if (!FileBrowser.Success) yield break;

            string path = FileBrowser.Result[0];
            MyPrefs.ModelPath = path;
            modelText.text = "Current model: " + path.Split('\\').Last();
            Destroy(ModelLoader.Model);
        }
    }
}