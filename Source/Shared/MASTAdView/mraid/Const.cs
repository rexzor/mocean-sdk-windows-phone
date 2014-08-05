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
    public enum State
    {
        Loading,
        Default,
        Expanded,
        Resized,
        Hidden,
    };

    public enum Feature
    {
        SMS,
        Tel,
        Calendar,
        StorePicture,
        InlineVideo,
    };

    public enum ForceOrientation
    {
        Portrait,
        Landscape,
        None,
    };

    public enum CustomClosePosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        Center,
        BottomLeft,
        BottomCenter,
        BottomRight,
    };

    public enum PlacementType
    {
        Inline,
        Interstitial,
    };

    public class Const
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
