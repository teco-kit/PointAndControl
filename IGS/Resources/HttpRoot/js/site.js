// check if vibration is supported
var supportsVibrate = "vibrate" in navigator;
var trackingId = null;
var beforeRegister;
var editDevice = "";
var editMode = false;

var deviceList = null;

//added for AR
var videoContainer = null;

var vibrate = function (pattern) {
    if (supportsVibrate) {
        navigator.vibrate(pattern);
    }
}

var toast = function (msg) {
    $("<div class='ui-loader ui-overlay-shadow ui-body-e ui-corner-all toast'>" + msg + "</div>")
    .css({
        padding: "5px",
        width: "290px",
        left: ($(window).width() - 300) / 2,
        top: "0.7em"
    })
    .appendTo($.mobile.pageContainer).delay(1500)
    .fadeOut(400, function () {
        $(this).remove();
    });

//    if (supportsVibrate) {
//        navigator.vibrate([100, 100, 100, 100, 100]);
//    }
}

var logMessage = function (message) {
    $.getJSON('/?dev=server&cmd=log&val=' + message, function (data) { });
}

var updateProgressBar = function (curVectors, minVectors) {
    if (minVectors <= 0) {
        text = curVectors + " Eingaben gespeichert";
        // hide progressbar
        $('#positionprogressbar').hide();
        // show navbar
        $('#locate .ui-navbar').show();

        return;
    }

    var completion = curVectors / minVectors * 100;

    if (completion <= 100) {
        var text = curVectors + "/" + minVectors + " Eingaben gespeichert";
        // update and display progressbar
        $('#positionprogressbar').show();
        // hide navbar
        $('#locate .ui-navbar').hide();

        // update text
        $('#positionprogresslabel').text(text);
    }

    // initialize statusbar
    if (completion == 0) {
        $('#positionprogressbar').children().eq(0).css({ width: "0%" });
        return;
    }

    // else animate
    $('#positionprogressbar').children().eq(0).animate({
        width: completion + "%"
    }, 500, function () {
        if (completion >= 100) {
            $('#positionprogresslabel').text(curVectors + " Eingaben gespeichert");
            // hide progressbar
            $('#positionprogressbar').hide();
            // display navbar
            $('#locate .ui-navbar').show();
        }
    });

}

var updateNewDeviceDialogue = function () {
    var selected = $('#newdevicedd').find(":selected");

    $('#newdevicename').val(selected.text());
}


var changeDevicePosition = function (deviceId) {
    if (!deviceId || deviceId == "") {
        $(':mobile-pagecontainer').pagecontainer('change', '#listdevices');
        return;
    }

    editDevice = deviceId;
    $(':mobile-pagecontainer').pagecontainer('change', '#locate');
}


var addDeviceFromList = function () {
    if ($('#newdevicedd').val() == "")
        return;

    $.getJSON('/?dev=server&cmd=addDeviceFromList&val=' + $('#newdevicedd').val() + ':' + $('#newdevicename').val(), function (data) {

        if (!data) {
            return;
        }

        if (data.success) {
            toast("Gerät hinzugefügt")

            // set position for new device
            changeDevicePosition(data.deviceId)
        }

        //TODO: check response          
    });
}

var clearDeviceVectors = function () {
    if (editDevice == "")
        return;

    $.getJSON('/?dev=server&cmd=resetDeviceVectorList&val=' + editDevice, function (data) {

        if (!data) {
            return;
        }

        if (data.msg != '') {
            toast(data.msg);
        }

        updateProgressBar(data.vectorCount, data.vectorMin);

        //if (data.vectorCount >= data.vectorMin) {
        //    $('#positiondevicebutton').button('enable');
        //} else {
        //    $('#positiondevicebutton').button('disable');
        //}

    });
}

var addDeviceVector = function () {
    if (editDevice == "")
        return;

    $.getJSON('/?dev=server&cmd=addDeviceVector&val=' + editDevice, function (data) {

        if (!data) {
            return;
        }

        if (data.msg != '') {
            toast(data.msg);
        }

        if (data.success) {
            vibrate(500);
        }

        updateProgressBar(data.vectorCount, data.vectorMin);

        //if (data.vectorCount >= data.vectorMin) {
        //    $('#positiondevicebutton').button('enable');
        //} else {
        //    $('#positiondevicebutton').button('disable');
        //}

    });
}

var saveDevicePosition = function () {
    if (editDevice == "")
        return;

    $.getJSON('/?dev=server&cmd=setDevicePosition&val=' + editDevice, function (data) {

        if (!data) {
            return;
        }

        if (data.msg != '') {
            toast(data.msg);
        }

        if (data.success) {
            vibrate(500);

            // close view
            $(':mobile-pagecontainer').pagecontainer('change', '#listdevices');
        }
    });
}

var updateNewDeviceDD = function (event) {
    $('#newdevicename').val('');

    $.getJSON('/?dev=server&cmd=discoverDevices', function (data) {
        var optionItems = [];

        if (!data || !data.devices || data.devices.length == 0) {
            $('#newdevicedd').html('<option>Keine Geräte gefunden</option>');
            $('#newdevicedd').selectmenu('disable');
            $('#newdevicedd').selectmenu('refresh');

            return;
        }
        
        optionItems.push('<option value="">Gerät wählen...</option>')

        for (var i = 0; i < data.devices.length; i++) {
            var device = data.devices[i];
            optionItems.push('<option value="' + device.id + '">' + device.name + '</option>');
        }

        $('#newdevicedd').html(optionItems.join(''));
        $('#newdevicedd').selectmenu('enable');
        $('#newdevicedd').selectmenu('refresh');
    });
}

//TODO: abstract commands into single function
var updateDeviceList = function () {
    $.getJSON('/?dev=server&cmd=list', function (data) {

        if (!data || !data.devices || data.devices.length == 0) {
            deviceList = null;
        } else {
            // store device list for later use
            deviceList = data.devices;
        }

        $(':mobile-pagecontainer').pagecontainer('change', '#listdevices', { allowSamePageTransition: true });
        // implicit with above call
        // updateDeviceListView();
    });
}

var updateDeviceListView = function () {
    var listItems = [];

    if (editMode) {
        $('#devicelist').listview('option', 'icon', 'location')
        listItems.push('<li data-icon="plus"><a href="#adddevice" style="text-align:center; background:lightgrey" data-ajax="false">Gerät hinzufügen</a></li>')
    } else {
        $('#devicelist').listview('option', 'icon', 'carat-r');
    }

    if (!deviceList || deviceList.length == 0) {
        $('#devicelist').html('<li>Keine Geräte</li>');
        $('#devicelist').listview('refresh');

        return;
    }

    for (var i = 0; i < deviceList.length; i++) {
        var device = deviceList[i];
        var target;
        if (editMode)
            target = '"javascript:changeDevicePosition(\'' + device.id + '\');"';
        else
            target = '"javascript:selectItem(\'' + device.id + '\');"'
        listItems.push('<li><a href=' + target + '><img src="img/icons/' + device.id + '.png">' + device.name + '</a></li>');
    }

    $('#devicelist').html(listItems.join(''));
    $('#devicelist').listview('refresh');
}

var updateMapFromList = function () {
    $.getJSON('/?dev=server&cmd=list', function (data) {
        var a = document.getElementById("mapSvg");
        var svgDoc = a.contentDocument;
        var circles = svgDoc.getElementsByTagName("a");
        for (var i = 0; i < circles.length; i++) { circles[i].setAttribute("visibility", "hidden") }

        var listItems = [];

        if (!data || !data.devices || data.devices.length == 0) {
            return;
        }

        // store device list for later use
        deviceList = data.devices;

        for (var i = 0; i < deviceList.length; i++) {
            var device = deviceList[i];

            for (var j = 0; j < circles.length; j++) {
                if (circles[j].id == device.id)
                    circles[j].setAttribute("visibility", "visible")
            }
        }

        $.mobile.loading('hide');
    });
}

var switchEditMode = function () {
    editMode = !editMode;
    updateDeviceListView();
}

var registerUser = function () {
    $.getJSON('/?dev=server&cmd=addUser', function (data) {
        var items = [];

        if (!data) {
            return;
        }

        if (data.trackingId != '')
            trackingId = data.trackingId;

        if (data.success) {
            // start polling server
            setTimeout(pollStatus, 1000);
        }

        //TODO: check response          
    });
}

var activateGestureControl = function () {
    $.getJSON('/?dev=server&cmd=activateGestureCtrl', function (data) {
        var items = [];

        if (!data) {
            return;
        }

        if (data.msg != '') {
            toast(data.msg);
        }

        if (data.trackingId != '')
            trackingId = data.trackingId;

        if (data.success) {
            vibrate(500);
            $(':mobile-pagecontainer').pagecontainer('change', '#interaction');
        }
    });
}

var selectDevice = function () {
    $.getJSON('/?dev=server&cmd=selectDevice', function (data) {
        if (!data || !data.devices) {
            return;
        }

        if (data.msg != '') {
            toast(data.msg);
        }

        if (data.success) {
            if (supportsVibrate) {
                //disable vibration for test navigator.vibrate(500);
            }
            if (data.devices.length == 1) {
                selectItem(data.devices[0].id);
            } else {
                if (supportsVibrate) {
                    navigator.vibrate(500);
                }
                deviceList = data.devices;
                $(':mobile-pagecontainer').pagecontainer('change', '#listdevices', { changeHash: false });
            }
        }
    });
}

var pollDevice = function () {
    $.getJSON('/?dev=server&cmd=pollDevice', function (data) {
        // restart request if we are still on the ar page
        var hash = $.mobile.path.parseLocation().hash;
        if (hash == '#ar')
            pollDevice();

        if (!data || !data.devices) {
            return;
        }

        if (data.msg != '') {
            toast(data.msg);
        }

        var arItems = [];

		var centerX = $('#arview').width()/2;
		var centerY = $('#arview').height()/2;
		var scale = 318;

        if (data.success) {
            // create devices
            for (i = 0; i < data.devices.length; i++) {
                var device = data.devices[i];

                var x = centerX + (Math.sin(device.angle / 180 * Math.PI) * device.radius * scale); 
                var y = centerY - (Math.cos(device.angle / 180 * Math.PI) * device.radius * scale);

				var position = '"position: absolute; left: ' + x + 'px; top: ' + y + 'px;"';
                 
                arItems.push('<div style=' + position + ' onclick="selectItem(\'' + device.id + '\')"><img src="img/icons/' + device.id + '.png"></div>');
            }

            $('#arview').html(arItems.join(''));
            $('#arview').show();

            //$('#arview a').attr("href", "/?dev=" + data.devices[0].id + "&cmd=getControlPath");
            //$('#arview img').attr("src", "img/icons/" + data.devices[0].id + ".png");
            //$('#arview span').text(data.devices[0].name);
            //$('#arview').show();
        } else {
            $('#arview').hide();
        }

    });
}

var selectItem = function (id) {
    //toast(id);
	vibrate(500);
    window.location.assign('/?dev=' + id + '&cmd=getControlPath');
}

var errorCallback = function(error) {
  console.log('navigator.getUserMedia error: ', error);
}

var gotDevices = function(deviceInfos) {
	
  for (var i = deviceInfos.length - 1; i > 0; --i) {
    var deviceInfo = deviceInfos[i];

	if (deviceInfo.kind === 'videoinput') {
      return deviceInfo.deviceId;
    }
  }
}

var startVideo = function(videoSource) {
	if (window.stream) {
		//window.stream.getTracks().forEach(function(track) { track.stop(); });
		return;
	}
	
	var constraints = {
		audio: false,
		video: {deviceId: videoSource}
	};
	
	navigator.mediaDevices.getUserMedia(constraints)
	.then(function(stream) {  
		window.stream = stream; // make stream available to console
		//attachMediaStream(videoContainer, stream);
		videoContainer.srcObject = stream;
		// Refresh button list in case labels have become available
		//return navigator.mediaDevices.enumerateDevices();
	})
	.catch(errorCallback);
}

var pollStatus = function () {
    // get status from server and generate toast
    $.getJSON('/?dev=server&cmd=popup', function (data) {
        var items = [];

        if (!data) {
            return;
        }

        if (data.msg != '') {
            toast(data.msg);
        }

        if (data.trackingId != '') {
            trackingId = data.trackingId;

            if (trackingId != null && trackingId < 0) {
                // redirect users on pages where tracking is required
                var hash = $.mobile.path.parseLocation().hash;

                if (hash == '#point' || hash == '#locate') {
                        $(':mobile-pagecontainer').pagecontainer('change', '#register');
                }
            }
        }

        //TODO: react properly on content/status
        if (data.success) {
            setTimeout(pollStatus, 1000);
        } else {
            setTimeout(registerUser(), 1000);
        }
    });
}

$(function (event) {
    // add user on server
    registerUser();

    // activate gesture control
    $('#activate').on('click', function (event) {
        activateGestureControl();
    });

    // get device pointed at
    $('#pointdevice').on('click', function (event) {
        selectDevice();
    });

    // add another device vector
    $('#locatedevice').on('click', function (event) {
        addDeviceVector();
    });

    // set device location
    $('#positiondevicebutton').on('click', function (event) {
        saveDevicePosition();
    });

    // update new device dialogue
    $('#newdevicedd').on('change', function (event) {
        updateNewDeviceDialogue();
    });

    // add device from dialogue
    $('#adddevicebutton').on('click', function (event) {
        addDeviceFromList();
    });


    $(document).on('pagecontainerbeforetransition', function (event, ui) {
        var hash = ui.absUrl ? $.mobile.path.parseUrl(ui.absUrl).hash : "";
        if (hash == '#listdevices') {
            editMode = false;
            if (!deviceList) {
                updateDeviceList();
            } else {
                updateDeviceListView();
                logMessage("List started");
            }
        }

        if (hash == '#locate') {
            clearDeviceVectors();
            $('#locate h1').text(editDevice + " positionieren");
        }

        if (hash == '#point') {
            logMessage("Pointing started");
        }

        if (hash == '#adddevice') {
            updateNewDeviceDD(event);
        }

        if (hash == '#ar') {
            logMessage("Camera started");
			videoContainer = $('#video')[0];
			navigator.mediaDevices.enumerateDevices()
			.then(gotDevices)
			.then(startVideo)
			.catch(errorCallback);
            pollDevice();
        }
        
        if (hash == '#map') {
            logMessage("Map started");
            $.mobile.loading('show');
            updateMapFromList();
        }
    });

    $(document).on('pagecontainerbeforechange', function (event, ui) {
        var hash = ui.absUrl ? $.mobile.path.parseUrl(ui.absUrl).hash : "";

        if (hash == '#register' && ui.fromPage) {
            // store where we are coming from
            beforeRegister = ui.prevPage[0].baseURI;
        }

        if (hash == '#point' || hash == '#locate') {
            // redirect to register site if not registered
            if (trackingId != null && trackingId < 0) {
                event.preventDefault();
                toast('Bitte erst registrieren');
                $(':mobile-pagecontainer').pagecontainer('change', '#register');
            }
        }
		
    });

    var svg = document.getElementById("mapSvg");
    svg.addEventListener("load", function () {
        updateMapFromList();
    }, false);
});