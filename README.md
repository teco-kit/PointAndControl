## PointAndControl


1. ## Introduction
  The Point-and-Control Application makes use of the Microsoft Kinect to enable Point&Click
  interaction for the control of appliances in smart environments. A backend server
  determines through collision detection which device the user is pointing at and sends the
  respective control interface to the user’s smartphone. Any commands the user issues are
  then sent back to the server which in turn controls the appliance.

2. ### Requirements for use
  - Windows 8/8.1 or higher
  - Kinect 4 Windows v.2,  SDK/Runtime Environment. SDK Version >= Public Preview Version 1409
  - .Net Framework 4.5
  - Smartphone with Android OS >= 2.3

3. ### Running the IGS
  - Make sure Requirements are met and everything is installed.
  - Install the IGS Android app via the IGS.apk in the IGS directory.
  - Start IGS server
  - Start android App
  - Click on the wrench icon
  - Enter hostname/IP
  - The app is now connected and the console will display an output
  - The system is now ready to use 

4. ### Using the IGS
	1. #### General UI hint:
		- On the IGS server as well as on the app, numbers will be taken
		as decimal. The format must be x,y (example: 1,0). Do not use x.y
	2. #### Customize/Configure/Setup
		1. ##### Server
			1. **Roomrepresentation:** the room is in a coordinatesystem and its origin is (0, 0, 0) [x,y,z]. Corresponding: (width, height, depth) = (x,y,z). The facing of the Kinect sensor representation is in on default (0° Horizontal facing degree along the z-Axis)
			2. **Add a Device through the server GUI:** insert the type (only included types), the name which will be displayed on the app, IP and port of the device in the belonging fields and click add device. (At success in the console should appear „device added to device configuration and device list“).
			3. **Change the room measurements:** insert the width, height, and depth of the room in the fields above the “set room” button and push the button.
			4. **Change the plugwise address for every plugwise device in the room:** insert host, port and path strings in the belonging fields and push the “Change PW address” button.
			5. **Set the coordinates of your Kinect sensor in the room(placing/replacing):** configure the Kinect sensor position in the room by inserting the x, y, z coordinates in the room and the and its tilting (T°) and horizontal facing (H°) in degree. H° defines the direction the Kinect sensor is facing. Attention (!!): the Kinect representation must be set correctly because the coordinates will be transformed from camera relative coordinates to room relative coordinates. If it it’s not placed correctly, after a repositioning of the Kinect sensor, the saved coordinates are wrong.
		2. ######  App 
			1. **Add a device through the App:** Go to the device list *(see Using the device list in section Point and control below*)  and push the “New Dev” field in the upper right corner. Insert the device specifics as in "*Add a Device through the server GUI*(4.ii.a.b)" above and submit.
			2. **Add coordinates to a specific device:** Active the gesture control *(see below Register your phone [4.iii])* and retrieve the control for the device you want to add coordinates to via device list. On a small field on the bottom of the app you can type in the radius of the device. If it is inserted hold your right hand where the device is and push “Koordinaten hinzufügen”. On the smartphone screen a message with: “Koordinaten hizugefügt” will appear. Now those coordinates for the device are added and it can be selected by the pointing and click control. More than one coordinate for a device can be add for make a more precise form.
	3. #### Register your phone 
		1. **Activating gesture control:** Stand visible in front of the Kinect sensor, raise your right arm with your smartphone in hand and click the button “Bitte Arm heben und hier klicken”. If the procedure was successful, a short vibration will appear and the picture on the screen changed. Now you are logged in and can choose a device by pointing on it if it’s placed in the room.
	4. #### Point and Control 
		1. ##### App
			1. **Using the device list:** Push on the “Überspringen” button in the bottom of the Android-Apps initial screen. A list with all the created devices will be shown. Push on one device to get to its control screen.
			2. **Using the gesture control:** Register your phone like shown in the section “Register your phone”. When the procedure was successful, point on the device which coordinates were added before. If one device was hit, its control will appear immediately. If more devices were hit, a list with the devices you can choose from will appear.
		2. ##### Server
			1. **Using the 3Dview:** Press the “Activate 3D view” button in the Server GUI to open the 3D view. The view displays the room walls, as well as grey balls representing the positions of the devices. The blue box is the Kinect representation. When gesture control is activated for a user, a green skeleton and a blue ray will appear. The ray is the direction the person is pointing.
	
5. ### Working with the Code
   - Programming language
	   - C#
   - Used Framework/ToolKits/SDKs:
	   - Microsoft Kinect for Windows v.2 SDK public preview version 1409
	   - .Net Framework 4.5
	   - Helix toolkit (NuGet or [https://github.com/helix-toolkit/helix-toolkit](https://github.com/helix-toolkit/helix-toolkit "on GitHub"))
   - An empty configuration.xml will be created in the folder of the igs.exe if there isn’t already one. Only there it will be used correctly.
   - The folder “Resources” contains stuff like an example configuration.xml, the igs.apk and the HttpRoot.
   - In the building process, the example configuration.xml, the Android package IGS.apk and the HttpRoot folder, containing the device control pages in the resource folder, will be copied to the target folder. 
<br/> Hint: If the replacement of the configuration.xml in the build process bothers you. Go to: IGS Properties -> Build Events and remove in the post-build event commandline:<br/> „COPY "$(ProjectDir)Resources\configuration.xml" "$(TargetDir)\" /Y“
6. ### Licenses 
	- **Helix Toolkit:** MIT license: [https://github.com/helix-toolkit/helix-toolkit/blob/master/LICENSE](https://github.com/helix-toolkit/helix-toolkit/blob/master/LICENSE "MIT License")
	- **Webserver in the IGS server:** Code Project open license  (CPOL) : [http://www.codeproject.com/info/cpol10.aspx](http://www.codeproject.com/info/cpol10.aspx "Code Project Open License (CPOL)")
	- **IGS:**
		- The MIT License (MIT) <br/>
Copyright (c) 2014 Technology for Pervasive Computing, Karlsruhe Institute of
Technology
Permission is hereby granted, free of charge, to any person obtaining a copy of this
software and associated documentation files (the "Software"), to deal in the
Software without restriction, including without limitation the rights to use, copy,
modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
to permit persons to whom the Software is furnished to do so, subject to the
following conditions:
The above copyright notice and this permission notice shall be included in all copies
or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

