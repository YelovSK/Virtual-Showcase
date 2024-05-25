using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GLTFast.Schema;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;
using VirtualShowcase.Core;
using VirtualShowcase.ModelLoading;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Menu.Options
{
    public class ModelOptions : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private GameObject modelRowPrefab;

        [SerializeField]
        private Transform modelRowParent;

        [SerializeField]
        private Toggle simplifyMeshToggle;

        [SerializeField]
        private Slider maxTriCountSlider;

        [SerializeField]
        private Toggle showRealSizeToggle;

        [SerializeField]
        private Toggle loadImmediatelyToggle;

        #endregion

        private readonly List<ModelRow> _modelRows = new();

        #region Event Functions

        private void Awake()
        {
            SetDefaults();
        }

        #endregion

        private void SetDefaults()
        {
            simplifyMeshToggle.isOn = MyPrefs.SimplifyMesh;
            SetSimplifyMesh(simplifyMeshToggle);

            maxTriCountSlider.value = MyPrefs.MaxTriCount;
            SetMaxTriCount(maxTriCountSlider);

            showRealSizeToggle.isOn = MyPrefs.ShowRealModelSize;
            SetShowRealSize(showRealSizeToggle);

            loadImmediatelyToggle.isOn = MyPrefs.LoadModelImmediately;
            SetLoadImmediately(loadImmediatelyToggle);

            foreach (string model in MyPrefs.ModelPaths)
            {
                if (!File.Exists(model))
                {
                    MyPrefs.RemoveModelPath(model);
                    continue;
                }
                InstantiateModelRow(model);
            }
        }

        public void SetSimplifyMesh(Toggle sender)
        {
            MyPrefs.SimplifyMesh = sender.isOn;
            maxTriCountSlider.transform.parent.gameObject.SetActive(sender.isOn);
        }

        public void SetMaxTriCount(Slider sender)
        {
            MyPrefs.MaxTriCount = (int)sender.value;
        }

        public void SetShowRealSize(Toggle sender)
        {
            MyPrefs.ShowRealModelSize = sender.isOn;
        }

        public void SetLoadImmediately(Toggle sender)
        {
            MyPrefs.LoadModelImmediately = sender.isOn;
        }

        public void RemoveAllModels()
        {
            while (_modelRows.Count > 0)
            {
                RemoveModel(_modelRows[0].FullPath);
            }
        }

        public void AddModel()
        {
            FileBrowser.SetFilters(false, new FileBrowser.Filter("Models", ModelLoaderClient.SUPPORTED_EXTENSIONS));
            FileBrowser.AddQuickLink("Desktop", "Desktop");
            FileBrowser.AddQuickLink("Local models", "Models");
            StartCoroutine(ShowLoadDialogCoroutine());
        }

        private IEnumerator ShowLoadDialogCoroutine()
        {
            yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, true, null, null,
                "Select model", "Load");

            if (!FileBrowser.Success)
            {
                yield break;
            }


            foreach (string path in FileBrowser.Result)
            {
                if (MyPrefs.AddModelPath(path))
                {
                    InstantiateModelRow(path);
                    if (MyPrefs.LoadModelImmediately)
                    {
                        MyEvents.ModelAdded?.Invoke(gameObject, path);
                    }
                }
            }
        }

        private void InstantiateModelRow(string path)
        {
            GameObject gameObject = Instantiate(modelRowPrefab, modelRowParent);
            var modelRow = gameObject.GetComponent<ModelRow>();
            modelRow.SetModel(path);
            modelRow.OnRemove.AddListener(() => RemoveModel(path));
            _modelRows.Add(modelRow);
        }

        private void RemoveModel(string path)
        {
            ModelRow modelRow = _modelRows.First(row => row.FullPath == path);
            _modelRows.Remove(modelRow);
            Destroy(modelRow.gameObject);
            MyPrefs.RemoveModelPath(path);

            MyEvents.ModelRemoved?.Invoke(gameObject, path);
        }
    }
}