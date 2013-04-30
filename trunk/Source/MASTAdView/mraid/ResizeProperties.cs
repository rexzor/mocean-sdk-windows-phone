using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.moceanmobile.mast.mraid
{
    internal class ResizeProperties
    {
        public static ResizeProperties PropertiesFromArgs(Dictionary<string, string> args)
        {
            ResizeProperties properties = new ResizeProperties();

            string value = null;
            double dValue = 0;

            if ((args.TryGetValue("width", out value)) && (double.TryParse(value, out dValue)))
            {
                properties.width = dValue;
            }

            if ((args.TryGetValue("height", out value)) && (double.TryParse(value, out dValue)))
            {
                properties.height = dValue;
            }

            if (args.TryGetValue("customClosePosition", out value))
            {
                switch (value)
                {
                    case Const.ResizePropertiesCCPositionTopLeft:
                        properties.customClosePosition = CustomClosePosition.TopLeft;
                        break;
                    case Const.ResizePropertiesCCPositionTopCenter:
                        properties.customClosePosition = CustomClosePosition.TopCenter;
                        break;
                    case Const.ResizePropertiesCCPositionTopRight:
                        properties.customClosePosition = CustomClosePosition.TopRight;
                        break;
                    case Const.ResizePropertiesCCPositionCenter:
                        properties.customClosePosition = CustomClosePosition.Center;
                        break;
                    case Const.ResizePropertiesCCPositionBottomLeft:
                        properties.customClosePosition = CustomClosePosition.BottomLeft;
                        break;
                    case Const.ResizePropertiesCCPositionBottomCenter:
                        properties.customClosePosition = CustomClosePosition.BottomCenter;
                        break;
                    case Const.ResizePropertiesCCPositionBottomRight:
                        properties.customClosePosition = CustomClosePosition.BottomRight;
                        break;
                }
            }

            if ((args.TryGetValue("offsetX", out value)) && (double.TryParse(value, out dValue)))
            {
                properties.offsetX = dValue;
            }

            if ((args.TryGetValue("offsetY", out value)) && (double.TryParse(value, out dValue)))
            {
                properties.offsetY = dValue;
            }

            if ((args.TryGetValue("allowOffscreen", out value)) && (value == Const.True))
            {
                properties.allowOffscreen = true;
            }
            else
            {
                properties.allowOffscreen = false;
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

        private CustomClosePosition customClosePosition = CustomClosePosition.TopRight;
        public CustomClosePosition CustomClosePosition
        {
            get { return this.customClosePosition; }
            set { this.customClosePosition = value; }
        }

        private double offsetX = 0;
        public double OffsetX
        {
            get { return this.offsetX; }
            set { this.offsetX = value; }
        }

        private double offsetY = 0;
        public double OffsetY
        {
            get { return this.offsetY; }
            set { this.offsetY = value; }
        }

        private bool allowOffscreen = false;
        public bool AllowOffscreen
        {
            get { return this.allowOffscreen; }
            set { this.allowOffscreen = value; }
        }

        public override string ToString()
        {
            string customClosePositionString = string.Empty;
            switch (this.customClosePosition)
            {
                case CustomClosePosition.TopRight:
                    customClosePositionString = Const.ResizePropertiesCCPositionTopRight;
                    break;
                case CustomClosePosition.TopCenter:
                    customClosePositionString = Const.ResizePropertiesCCPositionTopCenter;
                    break;
                case CustomClosePosition.TopLeft:
                    customClosePositionString = Const.ResizePropertiesCCPositionTopLeft;
                    break;
                case CustomClosePosition.Center:
                    customClosePositionString = Const.ResizePropertiesCCPositionCenter;
                    break;
                case CustomClosePosition.BottomLeft:
                    customClosePositionString = Const.ResizePropertiesCCPositionBottomLeft;
                    break;
                case CustomClosePosition.BottomCenter:
                    customClosePositionString = Const.ResizePropertiesCCPositionBottomCenter;
                    break;
                case CustomClosePosition.BottomRight:
                    customClosePositionString = Const.ResizePropertiesCCPositionBottomRight;
                    break;
            }

            string allowOffscreenString = Const.False;
            if (this.allowOffscreen)
                allowOffscreenString = Const.True;

            string ret = string.Format("{{width:{0},height:{1},customClosePosition:'{2}',offsetX:{3},offsetY:{4},allowOffscreen:{5}}}",
                this.width, this.height, customClosePositionString, this.offsetX, this.offsetY, allowOffscreenString);

            return ret;
        }
    }
}
