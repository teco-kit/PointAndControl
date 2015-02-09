var toast = function(msg){
    $("<div class='ui-loader ui-overlay-shadow ui-body-e ui-corner-all toast'>"+msg+"</div>")
    .css({ padding: "5px",
        width: "290px",
        left: ($(window).width() - 300)/2,
        top: "0.7em" })
    .appendTo( $.mobile.pageContainer ).delay( 1500 )
    .fadeOut( 400, function(){
        $(this).remove();
    });
}

function sendcmd(cmd) {
	// get device id from URL
	var cmdurl = window.location.href.replace(/\/\w*\.\w*\?/ig, '/?') + '&cmd=' + cmd;
    console.log(cmdurl);
	$.getJSON(cmdurl, function(data){
		console.log('success')
	});

  }