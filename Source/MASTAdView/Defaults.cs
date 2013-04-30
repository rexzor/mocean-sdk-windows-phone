using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.moceanmobile.mast
{
    internal class Defaults
    {
        public static string VERSION = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major + "." +
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor + "." +
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Revision +
            "beta";

        public static string AD_SERVER_URL = "http://ads.mocean.mobi/ad";

        // This is the user agent string sent if unable to determine one from the browser control.
        public static string ERROR_USER_AGENT = "MASTAdView/3.0 (Windows Phone, unable to determine ua)";

        public static int NETWORK_TIMEOUT_MILLISECONDS = 5000;

        public static string RICHMEDIA_SCRIPT_RESOURCE = "com.moceanmobile.mast.Resources.MASTMRAIDController.js";
        public static string CLOSE_BUTTON_RESOURCE = "com.moceanmobile.mast.Resources.MASTCloseButton.png";

        public static int CLOSE_BUTTON_SIZE = 50;

        public static string RICHMEDIA_FORMAT = "<!DOCTYPE html><html><head><meta name=\"viewport\" content=\"user-scalable=0;\"/><script>{0}</script><script>mraid.getState();</script><style>body{{margin:0;padding:0;}}</style></head><body>{1}</body></html>";

        // The default movement threshold distance in meters.
        public static double LOCATION_DETECTION_MOVEMENT_THRESHOLD = 1000;
    }
}
