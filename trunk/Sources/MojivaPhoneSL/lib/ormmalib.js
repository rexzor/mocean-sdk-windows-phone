var JSON = JSON || {};
JSON.stringify = JSON.stringify || function (obj) {
	var t = typeof (obj);
	if (t != "object" || obj === null) {
		if (obj === null) {
			return "null";
		}
		// simple data type  
		if (t == "string") obj = '"' + obj + '"';
		return String(obj);
	}
	else {
		// recurse array or object  
		var n, v, json = [], arr = (obj && obj.constructor == Array);
		for (n in obj) {
			v = obj[n]; t = typeof (v);
			if (t == "string") v = '"' + v + '"';
			else if (t == "object" && v !== null) v = JSON.stringify(v);
			json.push((arr ? "" : '"' + n + '":') + String(v));
		}
		return (arr ? "[" : "{") + String(json) + (arr ? "]" : "}");
	}
};

JSON.parse = JSON.parse || function (str) {
	if (str === "") str = '""';
	eval("var p=" + str + ";");
	return p;
};

var TMP_OBJ;
var DEFAULT_POSITION = {};

function getUA() {
	return navigator.userAgent;
}

function setTmpObj() {
	TMP_OBJ = arguments;
}

function fireEvent() {
	var event = arguments[0].toString();
	var eventArgs = Array.prototype.slice.call(arguments).slice(1, arguments.length);
	for (var i = 0; i < eventArgs.length; i++) {
		if (!isNaN(Number(eventArgs[i]))) {
			eventArgs[i] = Number(eventArgs[i]);
		}
	}
	Ormma.fireEvent(event, eventArgs);
}

(function () {
// 		var ORMMA_STATE_UNKNOWN = "unknown";
// 		var ORMMA_STATE_HIDDEN = "hidden";
// 		var ORMMA_STATE_DEFAULT = "default";
// 		var ORMMA_STATE_EXPANDED = "expanded";
// 		var ORMMA_STATE_RESIZED = "resized";
// 
// 		var ORMMA_EVENT_ERROR = "error";
// 		var ORMMA_EVENT_HEADING_CHANGE = "headingChange";
// 		var ORMMA_EVENT_KEYBOARD_CHANGE = "keyboardChange";
// 		var ORMMA_EVENT_LOCATION_CHANGE = "locationChange";
// 		var ORMMA_EVENT_NETWORK_CHANGE = "networkChange";
// 		var ORMMA_EVENT_ORIENTATION_CHANGE = "orientationChange";
// 		var ORMMA_EVENT_READY = "ready";
// 		var ORMMA_EVENT_RESPONSE = "response";
// 		var ORMMA_EVENT_SCREEN_CHANGE = "screenChange";
// 		var ORMMA_EVENT_SHAKE = "shake";
// 		var ORMMA_EVENT_SIZE_CHANGE = "sizeChange";
// 		var ORMMA_EVENT_STATE_CHANGE = "stateChange";
// 		var ORMMA_EVENT_TILT_CHANGE = "tiltChange";

	/**
	* The main ad controller object
	*/
	window.Ormma = {
		/**
		* The object that holds all types of OrmmaAdController events and associated listeners
		*/
		events: [],

		/**
		* Holds the current dimension values
		*/
		dimensions: {},

		/**
		* Holds the default dimension values
		*/
		defaultDimensions: DEFAULT_POSITION,

		expandProperties: {
			"use-background": "false",
			"background-color": "#0000ff",
			"background-opacity": "0.9",
			"is-modal": "true"
		},

		shakeProperties: {
			interval: 0,
			intensity: 0
		},

		/**
		* Holds the current property values
		*/
		properties: {},

		resizeProperties: {
			transition: ORMMA_STATE_UNKNOWN
		},

		state: ORMMA_STATE_DEFAULT,
		lastState: ORMMA_STATE_DEFAULT,

		/**
		* addEventListener adds an event listener to the listener array
		* @param {String} event The event name
		* @param {Function} listener The listener function
		* @returns nothing
		*/
		addEventListener: function (event, listener) {
			if (typeof listener == 'function') {
				if (!this.events[event]) {
					this.events[event] = [];

					if (event == ORMMA_EVENT_SHAKE) {
						sendNotify("startShakeListener");
					}
				}
				if (!this.events[event].listeners) {
					this.events[event].listeners = [];
				}
				if (getListenerIndex(event, listener) === -1) {
					this.events[event].listeners.splice(0, 0, listener);
				}
			}
		},

		/**
		* removeEventListener removes an event listener from the listener array
		* @param {String} event The event name
		* @param {Function} listener The listener function
		* @returns nothing
		*/
		removeEventListener: function (event, listener) {
			if (typeof listener == 'function' && this.events[event] && this.events[event].listeners) {
				var listenerIndex = getListenerIndex(event, listener);
				if (listenerIndex !== -1) {
					this.events[event].listeners.splice(listenerIndex, 1);
				}
			}
		},

		/**
		* reset the window size to the original state
		* @param {Function} listener The listener function
		* @returns nothing
		*/
		close: function () {
			sendNotify("close");
			fireEvent(ORMMA_EVENT_STATE_CHANGE, ORMMA_STATE_DEFAULT);
		},

		open: function (URL, controls) {
			sendNotify("open", URL);
			fireEvent(ORMMA_EVENT_STATE_CHANGE, ORMMA_STATE_DEFAULT);
		},

		/**
		* Use this method to hide the web viewer.
		* @param none
		* @returns nothing
		*/
		hide: function () {
			sendNotify("hide");
			fireEvent(ORMMA_EVENT_STATE_CHANGE, ORMMA_STATE_HIDDEN);
		},

		show: function () {
			sendNotify("show");
			fireEvent(ORMMA_EVENT_STATE_CHANGE, this.lastState);
		},

		/**
		* resize resizes the display window
		* @param {Object} dimensions The new dimension values of the window
		* @param {Object} properties Additional properties, such as transition effects
		* @param {Function} listener The listener function
		* @returns nothing
		*/
		resize: function (width, height) {
			//resize: function (dimensions, properties, listener) {
			// 			this.dimensions = dimensions;
			//          this.properties = properties;

			//sendNotify("resize", JSON.stringify(dimensions), JSON.stringify(properties));
			sendNotify("resize", width, height);

			if (typeof listener == 'function') {
				this.addEventListener('resize', listener);
			}
			fireEvent(ORMMA_EVENT_SIZE_CHANGE, { "width": width, "height": height });
			fireEvent(ORMMA_EVENT_STATE_CHANGE, null);
		},

		expand: function (dimensions, url) {
			//window.external.Notify("expand");
			this.dimensions = dimensions;
			sendNotify("expand", JSON.stringify(dimensions), JSON.stringify(this.expandProperties), url);
			var data = { dimensions: dimensions,
				properties: this.expandProperties
			};
			fireEvent('sizeChange', data);
			fireEvent('stateChange', ORMMA_STATE_EXPANDED);
		},

		getState: function () {
			return this.state;
		},

		setState: function (state) {
			this.state = state;
		},

		getSize: function () {
			TMP_OBJ = null;
			sendNotify("getSize");
			var size = { width: TMP_OBJ[0], height: TMP_OBJ[1] };
			return size;
		},

		getMaxSize: function () {
			TMP_OBJ = null;
			sendNotify("getMaxSize");
			var maxSize = { width: TMP_OBJ[0], height: TMP_OBJ[1] };
			return maxSize;
		},

		supports: function (feature) {
			switch (feature) {
				case "screen":
				case "orientation":
				case "heading":
				case "location":
				case "tilt":
				case "network":
				case "sms":
				case "phone":
				case "email":
				case "camera":
				case "shake":
				case "level-1":
				case "level-2":
				case "level-3":
					return true;
				case "calendar":
				default:
					return false;
			}

			return false;
		},

		getDefaultPosition: function () {
			return JSON.stringify(DEFAULT_POSITION);
		},

		getResizeProperties: function () {
			return JSON.stringify(this.resizeProperties);
		},

		setResizeProperties: function (properties) {
			this.resizeProperties = properties;
		},

		getExpandProperties: function () {
			return JSON.stringify(this.expandProperties);
		},

		setExpandProperties: function (properties) {
			this.expandProperties = properties;
		},

		/**
		* fireEvent fires an event
		* @private
		* @param {String} event The event name
		* @param {Object} additional information about the event
		* @returns nothing
		*/
		fireEvent: function (event, args) {
			var len, i;
			if (Ormma.events[event] && Ormma.events[event].listeners) {
				len = Ormma.events[event].listeners.length;
				for (i = len - 1; i >= 0; i--) {
					//(Ormma.events[event].listeners[i])(event, args);
					try {
						(Ormma.events[event].listeners[i]).apply(this, args);
					}
					catch (error) {
						this.fireError(error.message, Ormma.events[event].listeners[i]);
					}
				}
			}
		},

		fireError: function (message, action) {
			//window.external.Notify(message);
			var data = { message: message, action: action };
			fireEvent(ORMMA_EVENT_ERROR, data);
		},

		/**
		* Level 2 methods
		*/

		getOrientation: function () {
			TMP_OBJ = null;
			sendNotify("getOrientation");
			if (TMP_OBJ.length > 0) {
				return (+TMP_OBJ[0]);
			}
			else {
				return -1;
			}
		},

		getHeading: function () {
			TMP_OBJ = null;
			sendNotify("getHeading");
			if (TMP_OBJ.length > 0) {
				return (+TMP_OBJ[0]);
			}
			else {
				return -1;
			}
		},

		getLocation: function () {
			TMP_OBJ = null;
			sendNotify("getLocation");
			var loc = { lat: TMP_OBJ[0], lon: TMP_OBJ[1], acc: TMP_OBJ[2] };
			return JSON.stringify(loc);
		},

		getTilt: function () {
			TMP_OBJ = null;
			sendNotify("getTilt");
			var tilt = { x: TMP_OBJ[0], y: TMP_OBJ[1], z: TMP_OBJ[2] };
			return JSON.stringify(tilt);
		},

		getNetwork: function () {
			TMP_OBJ = null;
			sendNotify("getNetwork");
			return TMP_OBJ[0];
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

		setShakeProperties: function (properties) {
			this.shakeProperties = JSON.parse(properties);
			sendNotify("setShakeProperties", this.shakeProperties.intensity, this.shakeProperties.interval);
		},

		getShakeProperties: function () {
			return JSON.stringify(this.shakeProperties);
		},

		/**
		* Level 3 methods
		*/

		addAsset: function (URL, alias) {
			sendNotify("addAsset", URL, alias);
		},

		addAssets: function (assets) {
			for (var assetName in assets) {
				this.addAsset(assets[assetName], assetName);
			}
		},

		removeAsset: function (alias) {
			sendNotify("removeAsset", alias);
		},

		removeAllAssets: function () {
			sendNotify("removeAllAssets");
		},

		getAssetURL: function (alias) {
			TMP_OBJ = null;
			sendNotify("getAssetURL", alias);
			return TMP_OBJ[0];
		},

		getCacheRemaining: function () {
			TMP_OBJ = null;
			sendNotify("getCacheRemaining");
			return TMP_OBJ[0];
		},

		request: function (uri, display) {
			sendNotify("request", uri, display);
			return false;
		},

		storePicture: function (url) {
			sendNotify("storePicture", url);
		}
	};

	/**
	* The private methods
	*/

	/**
	* sendNotify sends function name and arguments by string
	* @private
	* @param {Function} function name
	* @params {String} arguments
	* @returns nothing
	*/
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

	/**
	* getListenerIndex retrieves the index of listener from the event listener array
	* @private
	* @param {String} event The event name
	* @param {Function} listener The listener function
	* @returns the index value of the listener array, -1 if the listener doesn't exist
	*/
	function getListenerIndex(event, listener) {
		var len, i;
		if (Ormma.events[event] && Ormma.events[event].listeners) {
			len = Ormma.events[event].listeners.length;
			for (i = len - 1; i >= 0; i--) {
				if (Ormma.events[event].listeners[i] === listener) {
					return i;
				}
			}
		}
		return -1;
	}
})();
