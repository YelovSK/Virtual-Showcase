using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimpleFileBrowser;
using TMPro;

public class ModelLoader : MonoBehaviour
{
	[SerializeField] TMP_Text modelText;
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

		if (FileBrowser.Success)
		{
			string path = FileBrowser.Result[0];
			Debug.Log(path);
			PlayerPrefs.SetString("modelPath", path);
			modelText.text = "Current model: " + path.Split('\\').Last();
		}
	}

}
