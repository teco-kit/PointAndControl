var counter = 0;
var deviceList;
var device;
var demoMode = false;
var u;

//change the devices in this line to the devices for the task
var selDevices = ["Boxee_1", "Plugwise_1", "Plugwise_3", "Boxee_2", "Plugwise_4"];
var demoDevices = ["Plugwise_2", "Plugwise_6"];

var countDown = function () {
    var circle = $('#outercircle');
    if (counter == 0) {
        $("#display").hide();

        //reset circle
        counter = 3;
        circle.css({ width: "700", height: "700" });

        // draw from urn
        device = u.draw();

        var i;
        for (i = 0; i < deviceList.length; i++) {
            if (deviceList[i].id == device)
                $('#display').text(deviceList[i].name);
        }

    }

    $('#circle').text(counter);
    circle.animate({ width: "-=200", height: "-=200" }, 1000, "linear", function () {
        if (counter > 0) {
            countDown();
        } else {
            //fill display
            $("#display").show();

            //log event on server
            var message = "Task started with device " + device;
            if (demoMode)
                message = message + " - Demonstration";
            logMessage(message);
        }
    });

    counter--;
}

var updateDeviceList = function () {
    $.getJSON('/?dev=server&cmd=list', function (data) {
        if (!data || !data.devices || data.devices.length == 0) {
            return;
        }

        // store device list for later use
        deviceList = data.devices;
    });
}

var logMessage = function (message) {
    $.getJSON('/?dev=server&cmd=log&val=' + message, function (data) { });
}

var Urn = function () {
    this.count = 0;
    this.collection = {};
    this.lastDraw = "";

    this.add = function (key, item) {
        if (this.collection[key] != undefined)
            return undefined;
        this.collection[key] = item;
        return ++this.count
    }

    this.remove = function (key) {
        if (this.collection[key] == undefined)
            return undefined;
        delete this.collection[key]
        return --this.count
    }

    this.item = function (key) {
        return this.collection[key];
    }

    this.clear = function () {
        this.collection = {};
        this.count = 0;
    }

    this.draw = function () {
        if (this.count == 0) {
            // use different sources in normal and demo mode
            var source = demoMode ? demoDevices : selDevices;

            // refill urn by copying
            var index;
            for (index = 0; index < source.length; index++) {
                this.add(this.count, source[index]);
            }
            
        }
        var ret = this.lastDraw;

        while (ret == this.lastDraw) {
            // select random index from remaining ones
            var i = Math.floor(Math.random() * this.count);
            var key = Object.keys(this.collection)[i];
            ret = this.collection[key];
        }

        this.lastDraw = ret;

        this.remove(key);

        return ret;
    }
}

$(function (event) {
    u = new Urn;

    updateDeviceList();

    $(document).keyup(function (evt) {
        if (evt.keyCode == 32) {
            countDown();
        }

        if (evt.keyCode == 68) {
            demoMode = !demoMode;

            // reset urn
            u.clear();

            if (demoMode) {
                $('#statusbar').text("Demonstrationsmodus!").slideDown();
            } else {
                $('#statusbar').slideUp().text(" ");
            }
        }
    });
});