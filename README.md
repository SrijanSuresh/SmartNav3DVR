# SmartNav 3D VR

**Unity Version:** 6000.2.4f1  
**Hardware Used:** Pixel 6a (Android), generic Google Cardboard-style VR headset  

[📁 Project Drive Link](https://drive.google.com/file/d/1z3EncP3V4uHxdxPjlcRzEcN0PwG8mqCr/view?usp=sharing) | [🎥 Demo Video](https://drive.google.com/file/d/1r660mKqullkwb9Lw5ttz4q8tsvWqjRNU/view?usp=sharing)

---

## Directory Hierarchy

![Directory Screenshot](./path-to-directory-screenshot.png)

---

## Features Implemented

- **Day/Night Cycle:** Sun/moon arcs with animated movement. Building lights and cart headlights toggle automatically at night.  
- **Gaze-Based Teleportation:** Reticle + loading indicator allows teleporting into hubs by staring.  
- **Head Rotation:** Implemented using phone gyroscope; works in VR mode on Android.  
- **Point of View:** Supports First Person and Third Person perspectives.  
- **Custom Prefab Stereo Camera:** Built left/right eye prefab for VR stereo rendering since Google Cardboard plugin crashed.  
- **Scene Interaction:** Interact with packages and deliver to hubs.  
- **Hubs/Flag Station:** Flag stations with packages and hubs to drop along with a checklist.  

---

## Extra Functionalities & Features (Extra Credit)

### Custom VR Camera Prefab (Critical Fix)
- Google's Cardboard XR plugin repeatedly crashed the Android app.  
- Built **BinocHead**, a custom VR camera rig prefab with **left/right cameras**.  
- Each camera renders half of the screen (`0–0.5` and `0.5–1.0`).  
- Added **GyroControl script** to rotate the prefab using the device gyroscope.  
- **Result:** Stable stereo VR view that runs on phone + headset, full control, avoids crashes, delivers a real VR experience.

### Additional Features
- Cart headlights turn on only at night.  
- Building interior lighting with transparent glass walls for realistic night glow.  
- Custom skyboxes for day/night cycles with stars and sun.  
- Variety of buildings for a city-like environment.  
- Minimap that always stays in the scene.  
- Custom inventory GUI with limits for package pickup/drop at stations.  
- Jump and rotation controls for Xbox/PlayStation controllers.  

---

## References
- **Unity Documentation:** Input System (Gyroscope, AttitudeSensor)  
- **Google Cardboard XR Plugin:** [GitHub](https://github.com/googlevr/cardboard-xr-plugin) (attempted, unstable)  
- **Community Resources:** StackOverflow / Unity Forums for Tracked Pose Driver and gyro rotation troubleshooting  

---

## Notes
- Tested on Pixel 6a with Cardboard-style headset.  
- Custom stereo prefab ensured stable VR rendering where Cardboard plugin failed.  
- Screen recordings captured on phone demonstrate:  
  - Head rotation  
  - Reticle & loader  
  - Gaze teleport in/out of cart  
  - Cart movement along ellipse  
  - Day/Night cycle (headlights + building lights)

---

## Build & Run Instructions
1. Clone or download the repository.  
2. Open project in **Unity 6000.2.4f1**.  
3. Build for **Android/iOS** platform.  
4. Install APK on a compatible phone with Cardboard-style headset.  
5. Launch and enjoy VR experience!

