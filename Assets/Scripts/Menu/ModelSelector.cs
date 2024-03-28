using System.Collections;
using SimpleFileBrowser;
using UnityEngine;
using VirtualShowcase.Core;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Menu
{
    public class ModelSelector : MonoBehaviour
    {
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
                {
                    Events.ModelAdded?.Invoke(gameObject, path);
                }
            }
        }
    }
}