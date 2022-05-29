# Virtual Showcase

View-dependent stereo running in Unity. The main use is to view imported .obj files with motion parallax. Off-axis projection is used to achieve proper camera perspective.

## DEPENDENCIES FOR USAGE IN THE EDITOR

1. Download the project: `git clone --recurse-submodules https://github.com/YelovSK/Virtual-Showcase.git`. [UnityMeshSimplifier](https://github.com/Whinarn/UnityMeshSimplifier) should get downloaded if you clone with `--recurse-submodules` flag.

---

2. Import these assets from the Unity Asset Store:

-   [Runtime OBJ Importer](https://assetstore.unity.com/packages/tools/modeling/runtime-obj-importer-49547) for loading .obj files at runtime.

-   [Runtime File Browser](https://assetstore.unity.com/packages/tools/gui/runtime-file-browser-113006) for browsing and selecting .obj files. To get it working with the new input system, look in the README in the GitHub [repository](https://github.com/yasirkula/UnitySimpleFileBrowser).

-   [Big Furniture Pack](https://assetstore.unity.com/packages/3d/props/furniture/big-furniture-pack-7717) for the room decoration. To change the materials to be URP compatible, select the materials in `BigFurniturePack/Models/Materials` and go to _Edit_ → _Rendering_ → _Materials_ → _Convert Selected Built-in Materials to URP_.

---

3. Go to [BlazeFaceBarracuda](https://github.com/keijiro/BlazeFaceBarracuda) repository and download [jp.keijiro.mediapipe.blazeface](https://github.com/keijiro/BlazeFaceBarracuda/tree/main/Packages/jp.keijiro.mediapipe.blazeface) package and put it into `Packages`.

---

4. Download this .fbx [model](https://free3d.com/3d-model/pc-monitor-69557.html) and put it in `Assets/Objects` via the file explorer, **not dragging into Unity** (the .meta file with guid gets rewritten if dragged inside the Unity window).

---

5. Download this .fbx [pack](https://www.cgtrader.com/free-3d-models/electronics/video/photography-studio-objects) and put the downloaded `uploads_files_873805_studio_objs.fbx` file into `Packages/Objects` the same way as the above model.

---

6. You should be asked to import TextMeshPro after the first run.

## DEFAULT CONTROLS AND KEY BINDS

-   **ESC** - toggle menu
-   **TAB** - toggle mono/stereo view
-   **S** - cycle main scenes
-   **C** - toggle calibration
-   **P** - toggle webcam preview
-   **R** - reset the position of the loaded object
-   **X/Y/Z** + **Mouse drag** - rotate object around X/Y/Z-axis
-   **LMB + Mouse drag** - move the loaded object on the ground
-   **RMB + Mouse drag** - move the loaded object up/down
-   **Mouse wheel** - scale the loaded object

## USED CODE SOURCES

[NativeCounter](https://coffeebraingames.wordpress.com/2021/10/24/some-dots-utilities-nativecounter-and-nativesum/) for keeping count of pixels inside of a job.

[KalmanFilter](https://gist.github.com/davidfoster/48acce6c13e5f7f247dc5d5909dce349) for smoothing eye keypoints.
