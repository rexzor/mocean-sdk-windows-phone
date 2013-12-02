using System;
using System.Collections.Generic;

namespace com.moceanmobile.mast.mraid
{
    public class OrientationProperties
    {
        public static OrientationProperties PropertiesFromArgs(Dictionary<string, string> args)
        {
            OrientationProperties properties = new OrientationProperties();

            string value = null;

            if ((args.TryGetValue("allowOrientationChange", out value)) && (value == Const.True))
            {
                properties.allowOrientationChange = true;
            }
            else
            {
                properties.allowOrientationChange = false;
            }

            if (args.TryGetValue("forceOrientation", out value))
            {
                switch (value)
                {
                    case Const.OrientationPropertiesForceOrientationNone:
                        properties.forceOrientation = ForceOrientation.None;
                        break;
                    case Const.OrientationPropertiesForceOrientationPortrait:
                        properties.forceOrientation = ForceOrientation.Portrait;
                        break;
                    case Const.OrientationPropertiesForceOrientationLandscape:
                        properties.forceOrientation = ForceOrientation.Landscape;
                        break;
                }
            }

            return properties;
        }

        private bool allowOrientationChange = true;
        public bool AllowOrientationChange
        {
            get { return this.allowOrientationChange; }
            set { this.allowOrientationChange = value; }
        }

        private ForceOrientation forceOrientation = ForceOrientation.None;
        public ForceOrientation ForceOrientation
        {
            get { return this.forceOrientation; }
            set { this.forceOrientation = value; }
        }

        public override string ToString()
        {
            string allowOrientationChangeString = Const.False;
            if (this.allowOrientationChange)
                allowOrientationChangeString = Const.True;

            string forceOrientationString = string.Empty;
            switch (this.forceOrientation)
            {
                case ForceOrientation.None:
                    forceOrientationString = Const.OrientationPropertiesForceOrientationNone;
                    break;
                case ForceOrientation.Portrait:
                    forceOrientationString = Const.OrientationPropertiesForceOrientationPortrait;
                    break;
                case ForceOrientation.Landscape:
                    forceOrientationString = Const.OrientationPropertiesForceOrientationLandscape;
                    break;
            }

            string ret = string.Format("{{allowOrientationChange:{0},forceOrientation:\"{1}\"}}",
                allowOrientationChangeString, forceOrientationString);

            return ret;
        }
    }
}
