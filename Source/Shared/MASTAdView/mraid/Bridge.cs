using System;
using System.Collections.Generic;

#if MAST_PHONE
using Microsoft.Phone.Controls;
#elif MAST_STORE
using WebBrowser = Windows.UI.Xaml.Controls.WebView;
#endif

namespace com.moceanmobile.mast.mraid
{
    public interface BridgeHandler
    {
        void mraidClose(Bridge bridge);
        void mraidOpen(Bridge bridge, string url);
        void mraidUpdateCurrentPosition(Bridge bridge);
        void mraidUpdatedExpandProperties(Bridge bridge);
        void mraidExpand(Bridge bridge, string url);
        void mraidUpdatedOrientationProperties(Bridge bridge);
        void mraidUpdatedResizeProperties(Bridge bridge);
        void mraidResize(Bridge bridge);
        void mraidPlayVideo(Bridge bridge, string url);
        void mraidCreateCalendarEvent(Bridge bridge, string calendarEvent);
        void mraidStorePicture(Bridge bridge, string url);
    }

    public class Bridge
    {
        private readonly BridgeHandler handler;
        private readonly WebBrowser browser;

        public Bridge(BridgeHandler handler, WebBrowser browser)
        {
            this.handler = handler;
            this.browser = browser;
        }

        public WebBrowser Browser
        {
            get { return this.browser; }
        }

        private State state = State.Loading;
        public State State
        {
            get { return this.state; }
        }

        private PlacementType placementType = PlacementType.Inline;
        public PlacementType PlacementType
        {
            get { return this.placementType; }
        }

        private ExpandProperties expandProperties = new ExpandProperties();
        public ExpandProperties ExpandProperties
        {
            get { return this.expandProperties; }
        }

        private ResizeProperties resizeProperties = new ResizeProperties();
        public ResizeProperties ResizeProperties
        {
            get { return this.resizeProperties; }
        }

        private OrientationProperties orientationProperties = new OrientationProperties();
        public OrientationProperties OrientationProperties
        {
            get { return this.orientationProperties; }
        }

        public void SendErrorMessage(string message, string action)
        {
            string js = string.Format("mraid.fireErrorEvent(\"{0}\", \"{1}\");", message, action);
            EvalJS(js);
        }

        public void SetSupportedFeature(Feature feature, bool supported)
        {
            string supportedString = Const.False;
            if (supported)
                supportedString = Const.True;

            string featureString = null;
            switch (feature)
            {
                case Feature.SMS:
                    featureString = Const.FeatureSMS;
                    break;
                case Feature.Tel:
                    featureString = Const.FeatureTel;
                    break;
                case Feature.Calendar:
                    featureString = Const.FeatureCalendar;
                    break;
                case Feature.StorePicture:
                    featureString = Const.FeatureStorePicture;
                    break;
                case Feature.InlineVideo:
                    featureString = Const.FeatureInlineVideo;
                    break;
            }

            if (featureString == null)
                return;

            string js = String.Format("mraid.setSupports(\"{0}\", \"{1}\");", featureString, supportedString);
            EvalJS(js);
        }

        public void SetState(State state)
        {
            this.state = state;

            string stateString = null;
            switch (this.state)
            {
                case State.Loading:
                    stateString = Const.StateLoading;
                    break;
                case State.Default:
                    stateString = Const.StateDefault;
                    break;
                case State.Expanded:
                    stateString = Const.StateExpanded;
                    break;
                case State.Resized:
                    stateString = Const.StateResized;
                    break;
                case State.Hidden:
                    stateString = Const.StateHidden;
                    break;
            }

            string js = String.Format("mraid.setState(\"{0}\");", stateString);
            EvalJS(js);
        }

        public void SendReady()
        {
            string js = String.Format("mraid.fireEvent(\"{0}\");", Const.EventReady);
            EvalJS(js);
        }

        public void SetViewable(bool viewable)
        {
            string viewableString = Const.False;
            if (viewable)
                viewableString = Const.True;

            string js = String.Format("mraid.setViewable(\"{0}\");", viewableString);
            EvalJS(js);
        }

        public void SetScreenSize(double width, double height)
        {
            string js = String.Format("mraid.setScreenSize({{width:{0},height:{1}}});", width, height);
            EvalJS(js);
        }

        public void SetMaxSize(double width, double height)
        {
            string js = String.Format("mraid.setMaxSize({{width:{0},height:{1}}});", width, height);
            EvalJS(js);
        }

        public void SetCurrentPosition(double x, double y, double width, double height)
        {
            string js = String.Format("mraid.setCurrentPosition({{x:{0},y:{1},width:{2},height:{3}}});", x, y, width, height);
            EvalJS(js);
        }

        public void SetDefaultPosition(double x, double y, double width, double height)
        {
            string js = String.Format("mraid.setDefaultPosition({{x:{0},y:{1},width:{2},height:{3}}});", x, y, width, height);
            EvalJS(js);
        }

        public void SetPlacementType(PlacementType pacementType)
        {
            string placementTypeString = null;
            switch (placementType)
            {
                case PlacementType.Inline:
                    placementTypeString = Const.PlacementTypeInline;
                    break;
                case PlacementType.Interstitial:
                    placementTypeString = Const.PlacementTypeInterstitial;
                    break;
            }

            string js = string.Format("mraid.setPlacementType(\"{0}\");", placementTypeString);
            EvalJS(js);
        }

        public void SetExpandProperties(ExpandProperties expandProperties)
        {
            string arg = expandProperties.ToString();
            string js = string.Format("mraid.setExpandProperties({0});", arg);
            EvalJS(js);
        }

        public void SetResizeProperties(ResizeProperties resizeProperties)
        {
            string arg = resizeProperties.ToString();
            string js = string.Format("mraid.setResizeProperties({0});", arg);
            EvalJS(js);
        }

        public void SetOrientationProperties(OrientationProperties orientationProperties)
        {
            string arg = orientationProperties.ToString();
            string js = string.Format("mraid.setOrientationProperties({0});", arg);
            EvalJS(js);
        }

        public void SendPictureAdded(bool success)
        {
            string successString = Const.False;
            if (success)
                successString = Const.True;

            string js = string.Format("mraid.firePictureAddedEvent(\"{0}\");", successString);
            EvalJS(js);
        }

        public bool ParseRequest(Uri uri)
        {
            if (uri.Scheme != Const.Scheme)
                return false;

            string command = uri.Host;
            string query = uri.Query.Trim(new char[] { '?' });

            Dictionary<string, string> args = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(query) == false)
            {
                string[] queryItems = query.Split(new char[] { '&' });
                foreach (string queryItem in queryItems)
                {
                    string[] keyValue = queryItem.Split(new char[] { '=' });
                    if (keyValue.Length == 2)
                    {
                        string key = Uri.UnescapeDataString(keyValue[0]);
                        string value = Uri.UnescapeDataString(keyValue[1]);

                        // TODO: This was done on iOS to preserve formatting, but it looks like
                        // URL/Uri formats here will be escaped as needed by the runtime.
                        //if (key != Const.CommandArgUrl)
                        //    value = Uri.UnescapeDataString(value);

                        args[key] = value;
                    }
                }
            }

            switch (command)
            {
                case Const.CommandClose:
                    this.handler.mraidClose(this);
                    break;

                case Const.CommandOpen:
                    {
                        string url = null;
                        args.TryGetValue(Const.CommandArgUrl, out url);
                        this.handler.mraidOpen(this, url);
                    }
                    break;

                case Const.CommandUpdateCurrentPosition:
                    this.handler.mraidUpdateCurrentPosition(this);
                    break;

                case Const.CommandExpand:
                    {
                        string url = null;
                        args.TryGetValue(Const.CommandArgUrl, out url);
                        this.handler.mraidExpand(this, url);
                    }
                    break;

                case Const.CommandResize:
                    this.handler.mraidResize(this);
                    break;

                case Const.CommandSetExpandProperties:
                    {
                        ExpandProperties expandProperties = ExpandProperties.PropertiesFromArgs(args);
                        this.expandProperties = expandProperties;
                        this.handler.mraidUpdatedExpandProperties(this);
                    }
                    break;

                case Const.CommandSetResizeProperties:
                    {
                        ResizeProperties resizeProperties = ResizeProperties.PropertiesFromArgs(args);
                        this.resizeProperties = resizeProperties;
                        this.handler.mraidUpdatedResizeProperties(this);
                    }
                    break;

                case Const.CommandSetOrientationProperties:
                    {
                        OrientationProperties orientationProperties = OrientationProperties.PropertiesFromArgs(args);
                        this.orientationProperties = orientationProperties;
                        this.handler.mraidUpdatedOrientationProperties(this);
                    }
                    break;

                case Const.CommandPlayVideo:
                    {
                        string url = null;
                        args.TryGetValue(Const.CommandArgUrl, out url);
                        this.handler.mraidPlayVideo(this, url);
                    }
                    break;

                case Const.CommandCreateCalendarEvent:
                    {
                        string calendarEvent = null;
                        args.TryGetValue(Const.CommandArgEvent, out calendarEvent);
                        this.handler.mraidCreateCalendarEvent(this, calendarEvent);
                    }
                    break;

                case Const.CommandStorePicture:
                    {
                        string url = null;
                        args.TryGetValue(Const.CommandArgUrl, out url);
                        this.handler.mraidStorePicture(this, url);
                    }
                    break;
            }

            return true;
        }

        private void EvalJS(string js)
        {
            EvalJS(this.browser, js);
        }

#if MAST_STORE
        async
#endif
        public static void EvalJS(WebBrowser browser, string js)
        {
            try
            {
#if MAST_PHONE
                browser.InvokeScript("eval", new string[] { js });
#elif MAST_STORE
                await browser.InvokeScriptAsync("eval", new string[] { js });
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EvalJS:" + ex.Message);
                // TODO: Bubble this up.
            }
        }
    }
}
