using System.Collections;
using System.Linq;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using VirtualVitrine.Core;

namespace VirtualVitrine.UI.Menu
{
    public class ModelLoader : MonoBehaviour
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
            FileBrowser.AddQuickLink("Local models", "Assets\\Objects");
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
            PlayerPrefs.SetString("modelPath", path);
            Destroy(GlobalManager.loadedObject);
            GlobalManager.loadedObject = null;
            modelText.text = "Current model: " + path.Split('\\').Last();
        }
        #endregion
    }
}