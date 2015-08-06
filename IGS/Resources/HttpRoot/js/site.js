// check if vibration is supported
var supportsVibrate = "vibrate" in navigator;
var trackingId = -1;
var beforeRegister;
var editDevice = "";
var editMode = false;

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

// TODO: abstract commands into single function
var updateDeviceList = function () {
    $.getJSON('/?dev=server&cmd=list', function (data) {
        var listItems = [];

        if (editMode) {
            $('#devicelist').listview('option', 'icon', 'location')
            listItems.push('<li data-icon="plus"><a href="#adddevice" style="text-align:center; background:lightgrey" data-ajax="false">Gerät hinzufügen</a></li>')
        } else {
            $('#devicelist').listview('option', 'icon', 'carat-r');
        }

        if (!data || !data.devices || data.devices.length == 0) {
            $('#devicelist').html('<li>Keine Geräte</li>');
            $('#devicelist').listview('refresh');

            return;
        }


        for (var i = 0; i < data.devices.length; i++) {
            var device = data.devices[i];
            var target;
            if (editMode)
                target = '"javascript:changeDevicePosition(\'' + device.id + '\');"';
            else
                target = '"/?dev=' + device.id + '&cmd=getControlPath" data-ajax="false"';
            listItems.push('<li><a href=' + target + '>' + device.name + '</a></li>');
        }

        $('#devicelist').html(listItems.join(''));
        $('#devicelist').listview('refresh');
    });
}

var updateDeviceDD = function () {
    $.getJSON('/?dev=server&cmd=list', function (data) {
        var optionItems = [];

        if (!data || !data.devices || data.devices.length == 0) {
            $('#devicedd').html('<option>Keine Geräte gefunden</option>');
            $('#devicedd').selectmenu('refresh');
            return;
        }

        optionItems.push('<option value="">Gerät wählen...</option>')

        for (var i = 0; i < data.devices.length; i++) {
            var device = data.devices[i];
            optionItems.push('<option value="' + device.id + '">' + device.name + '</option>');
        }

        $('#devicedd').html(optionItems.join(''));
        $('#devicedd').selectmenu('refresh');
    });
}

var switchEditMode = function () {
    editMode = !editMode;
    updateDeviceList();
}

var registerUser = function () {
    $.getJSON('/?dev=server&cmd=addUser', function (data) {
        var items = [];

        if (!data) {
            return;
        }

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
            if (beforeRegister) {
                $(':mobile-pagecontainer').pagecontainer('change', beforeRegister);
            } else {
                $(':mobile-pagecontainer').pagecontainer('change', '#point');
            }
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
                navigator.vibrate(500);
            }
            window.location.assign('/?dev=' + data.devices[0].id + '&cmd=getControlPath');
        }
    });
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

            if (trackingId < 0) {
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

    // clear device vectors on server, temporary workaround
    $('#devicedd').on('change', function (event) {
        editDevice = $('#devicedd').val();
        clearDeviceVectors();
    });

    // add device from dialogue
    $('#adddevicebutton').on('click', function (event) {
        addDeviceFromList();
    });


    $(document).on('pagecontainerbeforetransition', function (event, ui) {
        var hash = ui.absUrl ? $.mobile.path.parseUrl(ui.absUrl).hash : "";
        if (hash == '#listdevices') {
            editMode = false;
            updateDeviceList();
        }

        if (hash == '#locate') {
            //updateDeviceDD();
            clearDeviceVectors();
        }

        if (hash == '#adddevice') {
            updateNewDeviceDD(event);
        }
    });

    $(document).on('pagecontainerbeforechange', function (event, ui) {
        var hash = ui.absUrl ? $.mobile.path.parseUrl(ui.absUrl).hash : "";

        if (hash == '#register' && ui.fromPage) {
            // store where we are coming from
            beforeRegister = ui.fromPage;
        }

        if (hash == '#point' || hash == '#locate') {
            // redirect to register site if not registered
            if (trackingId < 0) {
                event.preventDefault();
                toast('Bitte erst registrieren');
                $(':mobile-pagecontainer').pagecontainer('change', '#register');
            }
        }
    });
});