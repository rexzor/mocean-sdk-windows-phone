using System;
using System.Collections.Generic;

namespace com.moceanmobile.mast
{
    public class AdDescriptor
    {
        private readonly Dictionary<String, String> adInfo;

        public AdDescriptor(Dictionary<String, String> adInfo)
        {
            this.adInfo = adInfo;
        }

        public string Type
        {
            get
            {
                string value = null;
                adInfo.TryGetValue("type", out value);
                return value;
            }
        }

        public string URL
        {
            get
            {
                string value = null;
                adInfo.TryGetValue("url", out value);
                return value;
            }
        }

        public string Track
        {
            get
            {
                string value = null;
                adInfo.TryGetValue("track", out value);
                return value;
            }
        }

        public string Image
        {
            get
            {
                string value = null;
                adInfo.TryGetValue("img", out value);
                return value;
            }
        }

        public string Text
        {
            get
            {
                string value = null;
                adInfo.TryGetValue("text", out value);
                return value;
            }
        }

        public string Content
        {
            get
            {
                string value = null;
                adInfo.TryGetValue("content", out value);
                return value;
            }
        }

        public override string ToString()
        {
            string ret = string.Format("Type:{0}, URL:{1}", this.Type, this.URL);
            return ret;
        }
    }
}
