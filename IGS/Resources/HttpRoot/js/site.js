// check if vibration is supported
var supportsVibrate = "vibrate" in navigator;
var trackingId = -1;

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

    if (supportsVibrate) {
        navigator.vibrate([100, 100, 100, 100, 100]);
    }
}

var updateNewDeviceDialogue = function () {
    var selected = $('#newdevicedd').find(":selected");

    $('#newdevicename').val(selected.text());
}


var addDeviceFromList = function () {
    $.getJSON('/?dev=server&cmd=addDeviceFromList&val=' + $('#newdevicedd').val() + ':' + $('#newdevicename').val(), function (data) {

        if (!data) {
            return;
        }

        if (data.success) {
            toast("Gerät hinzugefügt")

            // close dialogue
            $(':mobile-pagecontainer').pagecontainer('change', '#listdevices');
        }

        //TODO: check response          
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

        optionItems.push('<option>Gerät wählen...</option>')

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
var updateDeviceList = function (event) {
    $.getJSON('/?dev=server&cmd=list', function (data) {
        var listItems = [];
        var optionItems = [];

        if (!data || !data.devices || data.devices.length == 0) {
            $('#devicelist').html('<li>Keine Geräte</li>');
            $('#devicelist').listview('refresh');

            return;
        }

        optionItems.push('<option>Gerät wählen...</option>')

        for (var i = 0; i < data.devices.length; i++) {
            var device = data.devices[i];
            listItems.push('<li><a href="/?dev=' + device.id + '&cmd=getControlPath" data-ajax="false">' + device.name + '</a></li>');
            optionItems.push('<option value="' + device.id + '">' + device.name + '</option>');
        }

        $('#devicelist').html(listItems.join(''));
        $('#devicelist').listview('refresh');

        $('#select-device').html(optionItems.join(''));
        $('#select-device').selectmenu('refresh');
    });
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

        if (data.success) {
            if (supportsVibrate) {
                navigator.vibrate(500);
            }
            $(':mobile-pagecontainer').pagecontainer('change', '#point');
        }
    });
}

var selectDevice = function () {
    $.getJSON('/?dev=server&cmd=selectDevice', function (data) {
        var items = [];

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

var locateDevice = function () {
    var selectedDevice = $('#select-device').val();
    if (selectedDevice == "")
        return;
    $.getJSON('/?dev=' + selectedDevice + '&cmd=addDeviceLocation', function (data) {
        if (supportsVibrate) {
            navigator.vibrate(500);
        }

        toast('Location sent');
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

            if (trackingId == -1) {
                // redirect users on pages where tracking is required
                var hash = ui.absUrl ? $.mobile.path.parseUrl(ui.absUrl).hash : "";

                if (hash == '#point') {
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

    // locate device
    $('#locatedevice').on('click', function (event) {
        locateDevice();
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
        if (hash == '#listdevices' || hash == '#locate') {
            updateDeviceList(event);
        }

        if (hash == '#adddevice') {
            updateNewDeviceDD(event);
        }
    });
});