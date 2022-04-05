using Dummiesman;
using UnityEngine;
using VirtualVitrine.FaceTracking.Transform;

namespace VirtualVitrine
{
    public class ModelLoader : MonoBehaviour
    {
        #region Public Fields
        public GameObject LoadedObject { get; private set; }
        #endregion
        
        #region Public Methods
        public void ResetTransform()
        {
            LoadedObject.transform.localPosition = Vector3.zero;
            LoadedObject.transform.localRotation = Quaternion.identity;
            LoadedObject.transform.localScale = Vector3.one;
            
            // default is middle of the screen, put half of height below
            var offset = GameObject.Find("HeadNode").GetComponent<AsymFrustum>().height / 2;
            LoadedObject.transform.Translate(new Vector3(0, -offset, 0));
        }
        #endregion
        
        #region Unity Methods
        private void Start()
        {
            LoadObject();
        }
        #endregion

        #region Private Methods
        private void LoadObject()
        {
            // no model was chosen
            if (PlayerPrefs.GetString("modelPath") == "")
                return;
            
            // model already loaded
            if (GlobalManager.loadedObject != null)
            {
                LoadedObject = GlobalManager.loadedObject;
                LoadedObject.transform.parent = transform;
                print("Loaded model from static var");
            }
            // model loading for the first time
            else
            {
                LoadedObject = new OBJLoader().Load(PlayerPrefs.GetString("modelPath"));
                GlobalManager.loadedObject = LoadedObject;
                LoadedObject.transform.parent = transform;
                ResetTransform();
                print("Loaded new model");
            }

            // keep model loaded between scene switches
            DontDestroyOnLoad(this);
        }
        #endregion
    }
}