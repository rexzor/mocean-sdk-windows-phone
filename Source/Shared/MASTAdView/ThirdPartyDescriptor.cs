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

namespace com.moceanmobile.mast
{
    public class ThirdPartyDescriptor
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
