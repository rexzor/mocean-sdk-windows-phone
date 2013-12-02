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
                properties.width = dValue;
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
