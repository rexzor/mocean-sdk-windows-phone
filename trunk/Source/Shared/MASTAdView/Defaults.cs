using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.moceanmobile.mast
{
    public class Defaults
    {
        public static string VERSION = "3.1.0beta";

        public static string AD_SERVER_URL = "http://ads.moceanads.com/ad";

        // This is the user agent string sent if unable to determine one from the browser control.
#if MAST_PHONE
        public static string ERROR_USER_AGENT = "MASTAdView/" + VERSION + " (Windows Phone, unable to determine ua)";
#elif MAST_STORE
        public static string ERROR_USER_AGENT = "MASTAdView/" + VERSION + " (Windows Store, unable to determine ua)";
#endif

        public static int NETWORK_TIMEOUT_MILLISECONDS = 5000;

        public static string RICHMEDIA_SCRIPT_RESOURCE = "com.moceanmobile.mast.Resources.MASTMRAIDController.js";
        public static string CLOSE_BUTTON_RESOURCE = "com.moceanmobile.mast.Resources.MASTCloseButton.png";

        // Internal browser toolbar images:
        public static string IB_CLOSE_BUTTON_RESOURCE = "com.moceanmobile.mast.Resources.cancel.png";
        public static string IB_BACK_BUTTON_RESOURCE = "com.moceanmobile.mast.Resources.back.png";
        public static string IB_FORWARD_BUTTON_RESOURCE = "com.moceanmobile.mast.Resources.next.png";
        public static string IB_REFRESH_BUTTON_RESOURCE = "com.moceanmobile.mast.Resources.refresh.png";
        public static string IB_OPEN_BUTTON_RESOURCE = "com.moceanmobile.mast.Resources.upload.png";

        public static int CLOSE_BUTTON_SIZE = 50;
        public static int INLINE_CLOSE_BUTTON_SIZE = 28;

        public static string RICHMEDIA_FORMAT = "<!DOCTYPE html><html><head><meta name=\"viewport\" content=\"user-scalable=0;\"/><script>{0}</script><style>body{{margin:0;padding:0;}}</style></head><body>{1}</body></html>";

        // The default movement threshold distance in meters.
        public static double LOCATION_DETECTION_MOVEMENT_THRESHOLD = 1000;
    }
}
