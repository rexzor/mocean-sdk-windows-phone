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

namespace com.moceanmobile.mast.mraid
{
    public class ExpandProperties
    {
        public static ExpandProperties PropertiesFromArgs(Dictionary<string, string> args)
        {
            ExpandProperties properties = new ExpandProperties();

            string value = null;
            double dValue = 0;

            if ((args.TryGetValue("width", out value)) &&  (double.TryParse(value, out dValue)))
            {
                properties.width = dValue;
            }

            if ((args.TryGetValue("height", out value)) &&  (double.TryParse(value, out dValue)))
            {
                properties.height = dValue;
            }

            if ((args.TryGetValue("useCustomClose", out value)) && (value == Const.True))
            {
                properties.useCustomClose = true;
            }
            else
            {
                properties.useCustomClose = false;
            }

            return properties;
        }

        private double width = 0;
        public double Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        private double height = 0;
        public double Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        private bool useCustomClose = false;
        public bool UseCustomClose
        {
            get { return this.useCustomClose; }
            set { this.useCustomClose = value; }
        }

        public override string ToString()
        {
            string useCustomCloseString = Const.False;
            if (this.useCustomClose)
                useCustomCloseString = Const.True;

            string ret = string.Format("{{width:{0},height:{1},useCustomClose:{2}}}", 
                this.width, this.height, useCustomCloseString);

            return ret;
        }
    }
}
