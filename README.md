# Point and Control

[![Build status](https://ci.appveyor.com/api/projects/status/24i5xtbcmjm15i0e/branch/master?svg=true)](https://ci.appveyor.com/project/berningm/pointandcontrol/branch/master)

## Introduction
The Point-and-Control application makes use of the Microsoft Kinect to enable Point&Click interaction for the control of appliances in smart environments. A backend server determines through collision detection which device the user is pointing at and sends the respective control interface to the user’s smartphone. Any commands the user issues are then sent back to the server which in turn controls the appliance.

## Requirements for use
- 64 bit (x64) processor
- Physical dual-core 3.1-GHz (2 logical cores per physical) or faster processor
- USB 3.0 controller dedicated to the Kinect sensor
- at least 4 GB RAM
- Graphics card that supports DirectX 11
- Windows 8, Windows 8.1, or Windows Embedded 8
- Kinect 4 Windows v2,  SDK/Runtime Environment
- .Net Framework 4.5
- Smartphone with Android OS >= 2.3

You can check your system configuration for the Kinect v2 with the Configuration Verifier Tool: http://go.microsoft.com/fwlink/?LinkID=513889

## Running the IGS
1. Make sure Requirements are met and everything is installed.
1. Install the IGS Android app via the IGS.apk in the IGS/Resources directory.
1. Start IGS server
1. Start android App
1. Click on the wrench icon
1. Enter hostname/IP
1. The app will connect and the console should display an output
1. The system is now ready to use 

## Using the IGS
1. **Register:** Stand visibly in front of the Kinect sensor, raise your right arm holding your smartphone and click the main button as instructed on the screen. If the procedure was successful, the phone will vibrate shortly and continue to the next screen. Now you are logged in and the gesture control is activated. You have to repeat this process when you leave the Kinects field of view.
1. **Point:** Point on the registered device you want to control. If one device was hit, its controls will appear immediately. If more devices were hit, a list with the devices you can choose from will appear.
1. **Control:** You can now use the interface to control the device.

## Customize the Environment
*General note:*	Currently the software only supports `,` as decimal separator.
#### Server:
- **Room Representation:** The coordinate system of the room [x,y,z] is based on the default system of the Kinect. This means, when looking at the front of the sensor, x is aligned from the left to the right, y from the floor to the ceiling and z in the direction of the camera. Corresponding: (width, height, depth) = (x,y,z). 
- **Change the room measurements:** Enter the width, height, and depth of the room in the fields above the “set room” button and confirm.
- **Add a Device through the server GUI:** Enter the type (only included types), the name which will be displayed on the app, IP and port of the device in the respective fields and click add device. The console should confirm „device added to device configuration and device list“.
- **Change the plugwise address for every plugwise device in the room:** Enter host, port and path strings in the respective fields and push the “Change PW address” button.
- **Set the coordinates of the Kinect sensor in the room:** By default the Kinect sensor is aligned with the room coordinate system (T° = H° = 0). Configure the Kinect sensor position in the room by inserting the x, y, z coordinates in the room and the and its tilt (T°) and horizontal angle (H°) in degrees. H° defines the direction the Kinect sensor is facing with respect to the z-axis. Attention (!!): the Kinect representation must be set correctly to transform the coordinates from the camera to the room coordinate system. Please check the graphical output after repositioning the Kinect sensor.
- **Using the 3Dview:** Press the “Activate 3D view” button in the Server GUI to open the 3D view. The view displays the room walls, as well as grey balls representing the device positions. The blue box is the Kinect representation. When gesture control is activated for a user, a green skeleton and a blue ray will appear. The ray is the direction the person is pointing.

#### App:
- **Using the device list:** Push on the “Überspringen” button at the bottom initial screen. A list with all available devices will be shown. Touch one device to get to its control screen.
- **Add a device through the App:** Go to the device list and push the “New Dev” field in the upper right corner. Insert the device specifics as described above and submit.
- **Add coordinates to a specific device:** Activate the gesture control and select the device, for which you want to add coordinates, via the device list. At the bottom of the app you can type in the trigger radius of the device. After entring a value, hold your right hand where the device is and touch “Koordinaten hinzufügen”. The action will be confirmed by a popup message: “Koordinaten hizugefügt”. Now those coordinates for the device are added and it can be selected by pointing. More than one coordinate for a device can be added to create a more fine grained representation.
	
## Working with the Code
- Programming language is C#
- Used Framework/ToolKits/SDKs:
  - Microsoft Kinect for Windows v.2 SDK public preview version 1409
  - .Net Framework 4.5
  - Helix toolkit (NuGet or [GitHub](https://github.com/helix-toolkit/helix-toolkit))
- An empty `configuration.xml` will be created in the output folder if not present.
- The folder `Resources` contains stuff like an example `configuration.xml`, the `igs.apk` and the `HttpRoot`. It is copied to the output folder during the build process.	To disable this functionality, go to: IGS Properties -> Build Events and edit the post-build event commandline.

## Licenses 
- **3D Framework:** [Helix Toolkit](https://github.com/helix-toolkit/helix-toolkit/) [MIT License] 
- **Webserver:** [The Code Project Open License (CPOL)](http://www.codeproject.com/info/cpol10.aspx)
- **IGS** [MIT License]

The MIT Licencense (MIT)

Copyright (c) 2014 Technology for Pervasive Computing, Karlsruhe Institute of Technology

Permission is hereby granted, free of charge, to any person obtaining a copy of this
software and associated documentation files (the "Software"), to deal in the
Software without restriction, including without limitation the rights to use, copy,
modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
to permit persons to whom the Software is furnished to do so, subject to the
following conditions:

The above copyright notice and this permission notice shall be included in all copies
or substantial portions of the Software

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.</br>
