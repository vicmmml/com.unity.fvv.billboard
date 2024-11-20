# Custom FVV LIVE Unity Package
Version 1.0.1

## Overview
This package provides an integration with UPM's Live Free Viewpoint Video system using WebRTC for Unity 2020.3 and above.

## Installation
  1. Select `Window/Package Manager` in the menu bar.
  2. In Package Manager window, look at the top-left corner and click `+` button. Then, select `Add package from git URL...` and input the following string: 

    https://github.com/vicmmml/com.unity.fvv.billboard.git


## Key provided features and usage
- `aiortcConnector.cs` is responsible of stablishing the connection with the server by Connect() method. This method also manages the media reception: when a video or audio track is received, it's added to a Mediastream. If it's a video track, its texture is pasted on the billboard of the scene. If it's an audio track, it will be added to an Audiosource in the scene and played.
This script is also responsible of sending JSON messages to the FVV system with the Unity camera posotion, rotation, fov and a few more parameters. These messages are sent only when the camera is moving around the scene.
- `FlyCamera.cs` controls the camera movement. Some parameters like camera's speed, sensitivity or acceleration can be modified. Currently, camera movement is controlled by the mouse and WASD keys.
- Quad_rot prefab: its main components are `TextureAssign.cs`, `CountdownTimer.cs`and the `ChromaKeyShader`. TextureAssign.cs manages the texture rendering of the billboard and camera-dependant billboard rotation. CountdownTimer.cs is responsible of the first call to Connect() when the application is initialized. ChromaKeyShader controls the Chroma-key effect, the mask color can be adjusted as well as its threshold sensitivity.

## Sample Scene
There is a sample basic scene that can be used as a reference on the usage of this package. It can be imported in Package Manager Window, then clic on the custom package "FVV Live billboard visualizer" and Samples -> Sample Scene -> Import. After that, we'll just need to drag the Sample Scene (Sample_Scene_prefabs.unity) to the Hierarchy window. 

All the key features provided by this package are used in the sample scene, already configured and displayed in the scene. In addition, it includes some extra features in order to provide a friendly use.

- XRECO_platform: sample scenery from the XRECO project to integrate with the captured subject showing on the billboard.
- Canvas: it includes a few working options to interact with the FVV Live system such as `Connect/Disconnect`, `Exit` and `Reset Cam` buttons.


  
## Dependencies 
- Unity.WebRTC (version 3.0.0-pre.7)
- TextMeshPro


## FAQ and Troubleshooting

### Error message "InvalidOperationException: Insecure connection not allowed"
In some Unity versions  tools bar at the top Edit -> Project Settings -> Player -> Other Settings -> Configuration -> Allow downloads over HTTP on "Always allowed"
