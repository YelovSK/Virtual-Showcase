using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;

public class ModelLoader : MonoBehaviour
{
    public void ShowFileExplorer()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Objects", ".obj"));
        FileBrowser.SetDefaultFilter(".obj");
        FileBrowser.AddQuickLink("Desktop", "Desktop", null);
        FileBrowser.AddQuickLink("Local models", "Assets\\Objects", null);
        StartCoroutine(ShowLoadDialogCoroutine());
    }

	IEnumerator ShowLoadDialogCoroutine()
	{
		yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");

		Debug.Log(FileBrowser.Success);

		if (FileBrowser.Success)
		{
			string path = FileBrowser.Result[0];
			Debug.Log(path);
			GlobalVars.modelPath = path;
		}
	}

}
