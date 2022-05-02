# Virtual Showcase

View-dependent stereo (off-axis projection) running in Unity.

## CONTROLS AND KEY BINDS

-   **ESC** - toggle menu
-   **TAB** - toggle mono/stereo view
-   **C** - toggle calibration
-   **P** - toggle webcam preview
-   **R** - reset the position of the loaded object
-   **X/Y/Z** + **Mouse drag** - rotate object around X/Y/Z-axis
-   **Left Click** - move the loaded object on the ground
-   **Right Click** - move the loaded object up/down
-   **Mouse wheel** - scale the loaded object

## INFO

Website - http://www.st.fmph.uniba.sk/~hampel1/bak/index.html

Bachelor's thesis (SK) - https://www.overleaf.com/read/jstzfmshnmnv

## USED CODE SOURCES

[BlazeFaceBarracuda](https://github.com/keijiro/BlazeFaceBarracuda) for running BlazeFace model via Barracuda (_jp.keijiro.mediapipe.blazeface_ package).

[NativeCounter](https://coffeebraingames.wordpress.com/2021/10/24/some-dots-utilities-nativecounter-and-nativesum/) for keeping count of pixels inside of a job.

[KalmanFilter](https://gist.github.com/davidfoster/48acce6c13e5f7f247dc5d5909dce349) for smoothing eye keypoints.

[UnityMeshSimplifier](https://github.com/Whinarn/UnityMeshSimplifier) for reducing object vertex count.

## UNITY ASSETS

[Runtime OBJ Importer](https://assetstore.unity.com/packages/tools/modeling/runtime-obj-importer-49547) for loading .obj files at runtime.

[Runtime File Browser](https://assetstore.unity.com/packages/tools/gui/runtime-file-browser-113006) for browsing and selecting .obj files.

[Big Furniture Pack](https://assetstore.unity.com/packages/3d/props/furniture/big-furniture-pack-7717) for the room decoration.
