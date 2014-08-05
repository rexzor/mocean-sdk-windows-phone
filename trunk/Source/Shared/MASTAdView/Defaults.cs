/*
 * PubMatic Inc. (“PubMatic”) CONFIDENTIAL
 * Unpublished Copyright (c) 2006-2014 PubMatic, All Rights Reserved.
 *
 * NOTICE:  All information contained herein is, and remains the property of PubMatic. The intellectual and technical concepts contained
 * herein are proprietary to PubMatic and may be covered by U.S. and Foreign Patents, patents in process, and are protected by trade secret or copyright law.
 * Dissemination of this information or reproduction of this material is strictly forbidden unless prior written permission is obtained
 * from PubMatic.  Access to the source code contained herein is hereby forbidden to anyone except current PubMatic employees, managers or contractors who have executed 
 * Confidentiality and Non-disclosure agreements explicitly covering such access.
 *
 * The copyright notice above does not evidence any actual or intended publication or disclosure  of  this source code, which includes  
 * information that is confidential and/or proprietary, and is a trade secret, of  PubMatic.   ANY REPRODUCTION, MODIFICATION, DISTRIBUTION, PUBLIC  PERFORMANCE, 
 * OR PUBLIC DISPLAY OF OR THROUGH USE  OF THIS  SOURCE CODE  WITHOUT  THE EXPRESS WRITTEN CONSENT OF PubMatic IS STRICTLY PROHIBITED, AND IN VIOLATION OF APPLICABLE 
 * LAWS AND INTERNATIONAL TREATIES.  THE RECEIPT OR POSSESSION OF  THIS SOURCE CODE AND/OR RELATED INFORMATION DOES NOT CONVEY OR IMPLY ANY RIGHTS  
 * TO REPRODUCE, DISCLOSE OR DISTRIBUTE ITS CONTENTS, OR TO MANUFACTURE, USE, OR SELL ANYTHING THAT IT  MAY DESCRIBE, IN WHOLE OR IN PART.                
 */


/*
 * PubMatic Inc. (“PubMatic”) CONFIDENTIAL
 * Unpublished Copyright (c) 2006-2014 PubMatic, All Rights Reserved.
 *
 * NOTICE:  All information contained herein is, and remains the property of PubMatic. The intellectual and technical concepts contained
 * herein are proprietary to PubMatic and may be covered by U.S. and Foreign Patents, patents in process, and are protected by trade secret or copyright law.
 * Dissemination of this information or reproduction of this material is strictly forbidden unless prior written permission is obtained
 * from PubMatic.  Access to the source code contained herein is hereby forbidden to anyone except current PubMatic employees, managers or contractors who have executed 
 * Confidentiality and Non-disclosure agreements explicitly covering such access.
 *
 * The copyright notice above does not evidence any actual or intended publication or disclosure  of  this source code, which includes  
 * information that is confidential and/or proprietary, and is a trade secret, of  PubMatic.   ANY REPRODUCTION, MODIFICATION, DISTRIBUTION, PUBLIC  PERFORMANCE, 
 * OR PUBLIC DISPLAY OF OR THROUGH USE  OF THIS  SOURCE CODE  WITHOUT  THE EXPRESS WRITTEN CONSENT OF PubMatic IS STRICTLY PROHIBITED, AND IN VIOLATION OF APPLICABLE 
 * LAWS AND INTERNATIONAL TREATIES.  THE RECEIPT OR POSSESSION OF  THIS SOURCE CODE AND/OR RELATED INFORMATION DOES NOT CONVEY OR IMPLY ANY RIGHTS  
 * TO REPRODUCE, DISCLOSE OR DISTRIBUTE ITS CONTENTS, OR TO MANUFACTURE, USE, OR SELL ANYTHING THAT IT  MAY DESCRIBE, IN WHOLE OR IN PART.                
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.moceanmobile.mast
{
    public class Defaults
    {
        public static string VERSION = "3.1.2";

        public static string AD_SERVER_URL = "http://ads.moceanads.com/ad";

        // This is the user agent string sent if unable to determine one from the browser control.
#if MAST_PHONE
        public static string ERROR_USER_AGENT = "MASTAdView/" + VERSION + " (Windows Phone, unable to determine ua)";
#elif MAST_STORE
        public static string ERROR_USER_AGENT = "MASTAdView/" + VERSION + " (Windows Store, unable to determine ua)";
#endif

        public static int NETWORK_TIMEOUT_MILLISECONDS = 5000;

        // How much content is allowed after parsing out click url and image or text content before
	    // falling through and rendering as html vs. native rendering.
	    public static int DESCRIPTOR_THIRD_PARTY_VALIDATOR_LENGTH = 20;
	

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
