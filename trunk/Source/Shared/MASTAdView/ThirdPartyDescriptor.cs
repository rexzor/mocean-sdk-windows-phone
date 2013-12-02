using System;
using System.Collections.Generic;
using System.Text;

namespace com.moceanmobile.mast
{
    internal class ThirdPartyDescriptor
    {
        public static ThirdPartyDescriptor DescriptorFromClientSideExtrnalCampaign(string content)
        {
            string start = "<external_campaign";
            string end = "</external_campaign>";

            int startIndex = content.IndexOf(start);
            int endIndex = content.IndexOf(end, startIndex);

            if ((startIndex == -1) || (endIndex == -1))
                return null;

            ThirdPartyDescriptor thirdPartyDescriptor = new ThirdPartyDescriptor();

            int length = endIndex - startIndex + end.Length;
            content = content.Substring(startIndex, length);

            System.IO.StringReader stringReader = new System.IO.StringReader(content);

            System.Xml.XmlReaderSettings readerSettings = new System.Xml.XmlReaderSettings();
            readerSettings.IgnoreWhitespace = true;
            readerSettings.IgnoreComments = true;
            readerSettings.IgnoreProcessingInstructions = true;

            System.Xml.XmlReader reader = System.Xml.XmlReader.Create(stringReader, readerSettings);

            Stack<string> elementStack = new Stack<string>();

            Dictionary<string, string> elementAttributes = new Dictionary<string, string>();
            string elementContent = null;

            reader.ReadToDescendant("external_campaign");

            while (reader.Read())
            {
                System.Xml.XmlNodeType nodeType = reader.NodeType;
                switch (nodeType)
                {
                    case System.Xml.XmlNodeType.Element:
                        elementStack.Push(reader.Name);
                        elementAttributes.Clear();
                        elementContent = null;

                        if (reader.HasAttributes)
                        {
                            for (int i = 0, c = reader.AttributeCount; i < c; ++i)
                            {
                                reader.MoveToAttribute(i);
                                string attributeName = reader.Name;
                                if (reader.ReadAttributeValue())
                                {
                                    string attributeValue = reader.Value;
                                    if (attributeValue != null)
                                    {
                                        elementAttributes[attributeName] = attributeValue;
                                    }
                                }
                            }
                        }
                        break;

                    case System.Xml.XmlNodeType.EndElement:
                        if (elementStack.Count == 0)
                            break;

                        string key = elementStack.Pop();

                        if (elementContent != null)
                        {
                            if (key == "param")
                            {
                                if (elementAttributes.TryGetValue("name", out key))
                                {
                                    thirdPartyDescriptor.campaignParams[key] = elementContent;
                                }
                            }
                            else
                            {
                                thirdPartyDescriptor.properties[key] = elementContent;
                            }
                        }
                        
                        elementAttributes.Clear();
                        elementContent = null;
                        break;

                    case System.Xml.XmlNodeType.Text:
                    case System.Xml.XmlNodeType.CDATA:
                        elementContent = reader.Value;
                        break;
                }
            }

            return thirdPartyDescriptor;
        }


        private ThirdPartyDescriptor()
        {

        }

        private Dictionary<string, string> properties = new Dictionary<string, string>();
        public Dictionary<string, string> Properties
        {
            get { return this.properties; }
        }

        private Dictionary<string, string> campaignParams = new Dictionary<string, string>();
        public Dictionary<string, string> Params
        {
            get { return this.campaignParams; }
        }
    }
}
