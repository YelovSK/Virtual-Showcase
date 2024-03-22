# Virtual Showcase

View-dependent stereo in the Unity game engine. Allows viewing runtime imported *.glb* models with motion parallax. Off-axis projection is used to achieve proper camera perspective.

Please note that the project is specific to my use-case, making it available in a public repo was not the main goal.

**Download Windows build in [releases](https://github.com/YelovSK/Virtual-Showcase/releases).**

## SET UP INSTRUCTIONS

I would advise against it. If you just want to see how it works, download the build from the releases. You might have issues changing/adding functionality, as the code quality is questionable at best with heavy coupling.

1. Clone the repo: `git clone https://github.com/YelovSK/Virtual-Showcase.git`.

---

2. Import these assets from the Unity Asset Store:

-   [Big Furniture Pack](https://assetstore.unity.com/packages/3d/props/furniture/big-furniture-pack-7717) for the room decoration. To change the materials to be URP compatible, select the materials in `Assets/BigFurniturePack/Models/Materials` and go to _Edit_ → _Rendering_ → _Materials_ → _Convert Selected Built-in Materials to URP_.

---

3. Download these .fbx models - [PcMonitor.fbx](https://free3d.com/3d-model/pc-monitor-69557.html), [uploads_files_873805_studio_objs.fbx](https://www.cgtrader.com/free-3d-models/electronics/video/photography-studio-objects) and put them in [Assets/Objects](Assets/Objects) via the file explorer, **not dragging into Unity** (the .meta file with guid gets rewritten if dragged inside the Unity window). I don't want to add them to the repo due to licensing issues.

---

4. You should be asked to import TextMeshPro after the first run (the essentials are enough).

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

## Previews

https://user-images.githubusercontent.com/47487825/175299319-9899d9b6-4eba-4607-b306-db12a93fd4b3.mp4

Recorded before implementing position interpolation, so movement appears a bit choppy due to a low FPS webcam.
