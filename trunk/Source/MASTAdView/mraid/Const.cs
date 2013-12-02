using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.moceanmobile.mast.mraid
{
    internal enum State
    {
        Loading,
        Default,
        Expanded,
        Resized,
        Hidden,
    };

    internal enum PlacementType
    {
        Inline,
        Interstitial,
    };

    internal enum Feature
    {
        SMS,
        Tel,
        Calendar,
        StorePicture,
        InlineVideo,
    };

    internal enum ForceOrientation
    {
        Portrait,
        Landscape,
        None,
    };

    internal enum CustomClosePosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        Center,
        BottomLeft,
        BottomCenter,
        BottomRight,
    };

    internal class Const
    {
        public const string StateLoading = "loading";
        public const string StateDefault = "default";
        public const string StateExpanded = "expanded";
        public const string StateResized = "resized";
        public const string StateHidden = "hidden";

        public const string PlacementTypeInline = "inline";
        public const string PlacementTypeInterstitial = "interstitial";

        public const string FeatureSMS = "sms";
        public const string FeatureTel = "tel";
        public const string FeatureCalendar = "calendar";
        public const string FeatureStorePicture = "storePicture";
        public const string FeatureInlineVideo = "inlineVideo";

        public const string EventReady = "ready";

        public const string True = "true";
        public const string False = "false";

        // The Uri class will normalize host and scheme to lower so these need to be lower
        // even if not lower in the bridge code.
        public const string Scheme = "mraid";
        public const string CommandClose = "close";
        public const string CommandOpen = "open";
        public const string CommandUpdateCurrentPosition = "updatecurrentposition";
        public const string CommandExpand = "expand";
        public const string CommandSetExpandProperties = "setexpandproperties";
        public const string CommandResize = "resize";
        public const string CommandSetResizeProperties = "setresizeproperties";
        public const string CommandSetOrientationProperties = "setorientationproperties";
        public const string CommandPlayVideo = "playvideo";
        public const string CommandCreateCalendarEvent = "createcalendarevent";
        public const string CommandStorePicture = "storepicture";

        public const string CommandArgUrl = "url";
        public const string CommandArgEvent = "event";

        public const string PropertiesWidth = "width";
        public const string PropertiesHeight = "height";

        // TOOD: Need entries for expand properties and need to update the classes to make use of them.

        public const string ResizePropertiesCustomClosePosition = "customClosePosition";
        public const string ResizePropertiesOffsetX = "offsetX";
        public const string ResizePropertiesOffsetY = "offsetY";
        public const string ResizePropertiesAllowOffscreen = "allowOffscreen";

        public const string ResizePropertiesCCPositionTopLeft = "top-left";
        public const string ResizePropertiesCCPositionTopCenter = "top-center";
        public const string ResizePropertiesCCPositionTopRight = "top-right";
        public const string ResizePropertiesCCPositionCenter = "center";
        public const string ResizePropertiesCCPositionBottomLeft = "bottom-left";
        public const string ResizePropertiesCCPositionBottomCenter = "bottom-center";
        public const string ResizePropertiesCCPositionBottomRight = "bottom-right";
        public const string OrientationPropertiesForceOrientationNone = "none";
        public const string OrientationPropertiesForceOrientationPortrait = "portrait";
        public const string OrientationPropertiesForceOrientationLandscape = "landscape";
    }
}
