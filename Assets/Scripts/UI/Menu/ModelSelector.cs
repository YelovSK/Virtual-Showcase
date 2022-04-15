using System.Collections;
using System.Linq;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;

namespace VirtualVitrine.UI.Menu
{
    public class ModelSelector : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private TMP_Text modelText;
        #endregion
        
        #region Public Methods
        public void ShowFileExplorer()
        {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Objects", ".obj"));
            FileBrowser.SetDefaultFilter(".obj");
            FileBrowser.AddQuickLink("Desktop", "Desktop");
            FileBrowser.AddQuickLink("Local models", "Assets/Models");
            StartCoroutine(ShowLoadDialogCoroutine());
        }
        #endregion

        #region Private Methods
        private IEnumerator ShowLoadDialogCoroutine()
        {
            yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null,
                "Load Files and Folders", "Load");

            if (!FileBrowser.Success) yield break;
            
            var path = FileBrowser.Result[0];
            MyPrefs.ModelPath = path;
            modelText.text = "Current model: " + path.Split('\\').Last();
            Destroy(ModelLoader.Model);
        }
        #endregion
    }
}