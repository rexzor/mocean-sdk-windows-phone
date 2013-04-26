var maxSize = {width:0, height:0};
var size = {width:0, height:0};
var screenSize = {width:0, height:0};
var network = "unknown";
var geoLocation = {};
var heading = -1;
var tilt = {};
var orientation = -1;
var cacheRemaining = 0;
var defaultPosition = {x:0, y:0, width:0, height:0};
var placementType = "inline";
var version = "1.1.0";
var initState = "default";
var methodNotSupportedMessage = "method not supported";
var eventNotSupportedMessage = "event not supported";

var myJSON = myJSON || {};
myJSON.stringify = myJSON.stringify || function (obj) {
	var t = typeof (obj);
	if (t != "object" || obj === null) {
		if (obj === null) {
			return "null";
		}
		// simple data type  
		if (t == "string") {
			obj = '"' + obj + '"';
		}
		return String(obj);
	}
	else {
		// recurse array or object
		var n, v, json = [], arr = (obj && obj.constructor == Array);
		for (n in obj) {
			v = obj[n]; t = typeof (v);
			if (t == "string") v = '"' + v + '"';
			else if (t == "object" && v !== null) v = myJSON.stringify(v);
			json.push((arr ? "" : '"' + n + '":') + String(v));
		}
		return (arr ? "[" : "{") + String(json) + (arr ? "]" : "}");
	}
};

myJSON.parse = myJSON.parse || function (str) {
	if (str === "") str = '""';
	eval("var p=" + str + ";");
	return p;
}

function alert() {
	var notify = 'javascript_alert|';
	for (var i = 0; i < arguments.length; i++) {
		if (arguments[i] === null) {
			notify += "null";
		}
		else {
			notify += arguments[i].toString();
		}
		if (i < arguments.length - 1) {
			notify += '|';
		}
	}
	//window.external.Notify(notify);
}

(function () {
	var ORMMA_STATE_LOADING = "loading";
	var ORMMA_STATE_DEFAULT = "default";
	var ORMMA_STATE_EXPANDED = "expanded";
	var ORMMA_STATE_RESIZED = "resized";
	var ORMMA_STATE_HIDDEN = "hidden";
 
	var ORMMA_EVENT_READY = "ready";
	var ORMMA_EVENT_ERROR = "error";
	var ORMMA_EVENT_HEADING_CHANGE = "headingChange";
	var ORMMA_EVENT_LOCATION_CHANGE = "locationChange";
	var ORMMA_EVENT_NETWORK_CHANGE = "networkChange";
	var ORMMA_EVENT_ORIENTATION_CHANGE = "orientationChange";
	var ORMMA_EVENT_RESPONSE = "response";
	var ORMMA_EVENT_SCREEN_CHANGE = "screenChange";
	var ORMMA_EVENT_SHAKE = "shake";
	var ORMMA_EVENT_SIZE_CHANGE = "sizeChange";
	var ORMMA_EVENT_STATE_CHANGE = "stateChange";
	var ORMMA_EVENT_TILT_CHANGE = "tiltChange";
	var ORMMA_EVENT_VIEWABLE_CHANGE = "viewableChange";

	var ORMMA_EVENT_KEYBOARD_CHANGE = "keyboardChange";
	
	
	window.ormma = {
		events: [],

		resizeProperties: {
			"transition" : "none"
		},

		expandProperties: {
			width : 100,
			height : 100,
			useCustomClose : false,
			isModal : false,
			lockOrientation : false,
			useBackground : false,
			backgroundColor : "#00ccff",
			backgroundOpacity : 1.0
		},

		shakeProperties: { interval: 0, intensity: 0 },
		lastState: ORMMA_STATE_DEFAULT,
		state: ORMMA_STATE_LOADING,

		ORMMAinited: function () {
			setState(initState);
		},

		setDefaultExpandSize: function () {
			this.expandProperties.width = maxSize.width;
			this.expandProperties.height = maxSize.height;
		},

		supports: function (feature) {
			switch (feature) {
				case "screen":
				case "orientation":
				case "heading":
				case "location":
				case "shake":
				case "tilt":
				case "network":
				case "sms":
				case "phone":
				case "email":
				case "camera":
				case "audio":
				case "video":
				case "map":
				case "level-1":
				case "level-2":
					return true;
				case "calendar":
				default:
					return false;
			}

			return false;
		},

		addEventListener : function(event, listener) {
			if (event == ORMMA_EVENT_KEYBOARD_CHANGE) {
				var msg = '';
				msg = event + ' ' + eventNotSupportedMessage;
				this.raiseEvent(ORMMA_EVENT_ERROR, {message: msg, action:"addEventListener"});
			}
			else if (typeof listener == 'function') {
				if (!this.events[event]) {
					this.events[event] = [];
				}
				if (!this.events[event].listeners) {
					this.events[event].listeners = [];
				}
				if (getListenerIndex(event, listener) === -1) {
					this.events[event].listeners.splice(0, 0, listener);
				}
			}
		},

		removeEventListener: function (event, listener) {
			if (typeof listener == 'function' && this.events[event] && this.events[event].listeners) {
				var listenerIndex = getListenerIndex(event, listener);
				if (listenerIndex !== -1) {
					this.events[event].listeners.splice(listenerIndex, 1);
				}
			}
		},

		open: function (URL) {
			sendNotify("open", URL);
		},

		hide: function () {
			if (this.state != ORMMA_STATE_EXPANDED) {
				setState(ORMMA_STATE_HIDDEN);
				sendNotify("hide");

				this.raiseEvent(ORMMA_EVENT_VIEWABLE_CHANGE);
			}
		},

		show: function () {
			if (this.state == ORMMA_STATE_HIDDEN) {
				sendNotify("show");
				setState(this.lastState);

				this.raiseEvent(ORMMA_EVENT_VIEWABLE_CHANGE);
			}
			else
			{
				this.raiseEvent(ORMMA_EVENT_ERROR, {message: "ad view is visible", action:"show"});
			}
		},

		resize: function (width, height) {
			if (this.state == ORMMA_STATE_DEFAULT) {
				sendNotify("resize", width, height);
				setState(ORMMA_STATE_RESIZED);
			}
		},

		expand: function () {
			if (this.state == ORMMA_STATE_DEFAULT || this.state == ORMMA_STATE_RESIZED) {

				if (defaultPosition.x + this.expandProperties.width <= maxSize.width &&
					defaultPosition.y + this.expandProperties.height <= maxSize.height) {

					var url = '';
					if (arguments.length > 0) {
						url = String(arguments[0]);
					}

					sendNotify("expand", myJSON.stringify(this.expandProperties), url);
					setState(ORMMA_STATE_EXPANDED);
				}
				else
				{
					this.raiseEvent(ORMMA_EVENT_ERROR, {message:"out of bounds", action:"expand"});
				}
			}
			else
			{
				this.raiseEvent(ORMMA_EVENT_ERROR, {message:"state not default or resized", action:"expand"});
			}
		},

		close: function () {
			sendNotify("close");
			switch (this.state) {
				case ORMMA_STATE_DEFAULT:
					hide();
					break;
				case ORMMA_STATE_RESIZED:
				case ORMMA_STATE_EXPANDED:
					setState(ORMMA_STATE_DEFAULT);
					break;
				default:
					break;
			}
		},

		getState: function () {
			return this.state;
		},

		getSize: function () {
			return size;
		},

		getMaxSize: function () {
			return maxSize;
		},

		getScreenSize: function () {
			return screenSize;
		},

		getDefaultPosition: function () {
			return defaultPosition;
		},

		getResizeProperties: function () {
			return this.resizeProperties;
		},

		setResizeProperties: function (properties) {
			this.resizeProperties = properties;
		},

		getExpandProperties: function () {
			return this.expandProperties;
		},

		setExpandProperties: function (properties) {
			this.expandProperties = properties;
		},

		raiseEvent: function () {
			var event = arguments[0].toString();

			var len, i;
			if (this.events[event] && this.events[event].listeners) {
				len = this.events[event].listeners.length;
				for (i = len - 1; i >= 0; i--) {
					try {
						if(event == ORMMA_EVENT_ERROR) {
							(this.events[event].listeners[i]).call(this, arguments[1].message, arguments[1].action);
						}
						else if(event == ORMMA_EVENT_HEADING_CHANGE) {
							(this.events[event].listeners[i]).call(this, arguments[1]);
						}
						else if(event == ORMMA_EVENT_LOCATION_CHANGE) {
							(this.events[event].listeners[i]).call(this, arguments[1].lat, arguments[1].lon, arguments[1].acc);
						}
						else if(event == ORMMA_EVENT_NETWORK_CHANGE) {
							(this.events[event].listeners[i]).call(this, arguments[1].online, arguments[1].connection);
						}
						else if(event == ORMMA_EVENT_ORIENTATION_CHANGE) {
							(this.events[event].listeners[i]).call(this, arguments[1]);
						}
						else if(event == ORMMA_EVENT_RESPONSE) {
							(this.events[event].listeners[i]).call(this, arguments[1], arguments[2]);
						}
						else if(event == ORMMA_EVENT_SCREEN_CHANGE) {
							(this.events[event].listeners[i]).call(this, arguments[1].width, arguments[1].height);
						}
						else if(event == ORMMA_EVENT_SHAKE) {
							(this.events[event].listeners[i]).call(this, null);
						}
						else if(event == ORMMA_EVENT_READY) {
							(this.events[event].listeners[i]).call(this, null);
						}
						else if(event == ORMMA_EVENT_SIZE_CHANGE) {
							(this.events[event].listeners[i]).call(this, arguments[1].width, arguments[1].height);
						}
						else if(event == ORMMA_EVENT_STATE_CHANGE) {
							(this.events[event].listeners[i]).call(this, arguments[1].toString());
						}
						else if(event == ORMMA_EVENT_TILT_CHANGE) {
							(this.events[event].listeners[i]).call(this, arguments[1].x, arguments[1].y, arguments[1].z);
						}
					}
					catch (error) {
						this.fireError(error.message, this.events[event].listeners[i]);
					}
				}
			}
		},

		fireError: function (message, action) {
			var data = { message: message, action: action };
			this.raiseEvent(ORMMA_EVENT_ERROR, { message: message, action: action });
		},


		// Level 2 methods

		createEvent: function (date, title, body) {
			this.raiseEvent(ORMMA_EVENT_ERROR, {message: methodNotSupportedMessage, action:"createEvent"});
		},

		getKeyboard: function () {
			this.raiseEvent(ORMMA_EVENT_ERROR, {message: methodNotSupportedMessage, action:"getKeyboard"});
		},

		getOrientation: function () {
			return orientation;
		},

		getHeading: function () {
			return heading;
		},

		getLocation: function () {
			return geoLocation;
		},

		getTilt: function () {
			return tilt;
		},

		getNetwork: function () {
			return network;
		},

		makeCall: function (number) {
			sendNotify("makeCall", number);
		},

		sendMail: function (recipient, subject, message) {
			sendNotify("sendMail", recipient, subject, message);
		},

		sendSMS: function (recipient, message) {
			sendNotify("sendSMS", recipient, message);
		},

		playAudio: function (URL, properties) {
			sendNotify("playAudio", URL, myJSON.stringify(properties));
		},

		playVideo: function (URL, properties) {
			sendNotify("playVideo", URL, myJSON.stringify(properties));
		},

		openMap: function (POI, fullscreen) {
			sendNotify("openMap", POI, fullscreen);
		},

		setShakeProperties: function (properties) {
			this.shakeProperties = properties;
		},

		getShakeProperties: function () {
			return this.shakeProperties;
		},

		request: function (uri, display) {
			sendNotify("request", uri, display);
			return false;
		},

		storePicture: function (url) {
			sendNotify("storePicture", url);
		},

		getPlacementType: function() {
			return placementType;
		},

		getVersion: function () {
			return version;
		},

		useCustomClose: function (flag) {
			sendNotify("useCustomClose", flag);
		},

		getViewable: function () {
			return this.isViewable();
		},

		isViewable: function () {
			return (this.state != ORMMA_STATE_HIDDEN);
		},

		onExpandClosed: function () {
			setState(ORMMA_STATE_DEFAULT);
		}
	},

	//The private methods

	function sendNotify() {
		var notify = '';
		for (var i = 0; i < arguments.length; i++) {
			if (arguments[i] === null) {
				notify += "null";
			}
			else {
				notify += arguments[i].toString();
			}
			if (i < arguments.length - 1) {
				notify += '|';
			}
		}

		window.external.Notify(notify);
	}

	function getListenerIndex(event, listener) {
		var len = 0;
		var i = -1;
		if (ormma.events[event] && ormma.events[event].listeners) {
			len = ormma.events[event].listeners.length;
			for (i = len - 1; i >= 0; i--) {
				if (ormma.events[event].listeners[i] === listener) {
					return i;
				}
			}
		}
		return -1;
	}

	function setState(state) {
		if (ormma.state == ORMMA_STATE_DEFAULT)
		{
			ormma.lastState = ORMMA_STATE_DEFAULT;
		}
		else
		{
			ormma.lastState = ormma.state;
		}
					
		ormma.state = state;
		ormma.raiseEvent(ORMMA_EVENT_STATE_CHANGE, state);

		window.external.Notify(ormma.lastState);
		window.external.Notify(ormma.state);
	}

	window.Ormma = ormma;
	window.mraid = ormma;

	ormma.ORMMAinited();
})();
