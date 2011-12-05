/****
 * © 2010-2011 mOcean Mobile. A subsidiary of Mojiva, Inc. All Rights Reserved.
 * */

using System.Xml.Linq;

namespace mOceanWindowsPhone
{
	internal static class GenericThirdParty
	{
		private const string BEGIN = "<!-- client_side_external_campaign";
		private const string END = "-->";
		private const string ROOT_NODE_NAME = "external_campaign";
		private const string CAMPAIGN_ID_NODE_NAME = "campaign_id";
		private const string TYPE_NODE_NAME = "type";
		private const string EXTERNAL_PARAMS_NODE_NAME = "external_params";
		private const string PARAM_NODE_NAME = "param";
		private const string VARIABLES_PARAM_NODE_NAME = "variables";
		private const string TRACK_URL_NODE_NAME = "track_url";

		public class GenericThirdPartyResponse
		{
			public string campaignId = null;
			public string type = null;
			public string variables = null;
			public string trackUrl = null;

			public GenericThirdPartyResponse()
			{
			}
		}


		public static GenericThirdPartyResponse SearchThirdParty(string adContent)
		{
			GenericThirdPartyResponse response = new GenericThirdPartyResponse();

			try
			{
				adContent = adContent.Trim();
				if (!adContent.StartsWith(BEGIN) || !adContent.EndsWith(END))
				{
					return null;
				}

				adContent = adContent.Replace(BEGIN, "").Replace(END, "").Trim();

				XElement rootNode = XElement.Parse(adContent);
				if (rootNode.Name.LocalName == ROOT_NODE_NAME)
				{
					foreach (var node in rootNode.Elements())
					{
						switch (node.Name.LocalName)
						{
							case CAMPAIGN_ID_NODE_NAME:
								response.campaignId = node.Value;
								break;
							case TYPE_NODE_NAME:
								response.type = node.Value;
								break;
							case EXTERNAL_PARAMS_NODE_NAME:
								foreach (var paramNode in node.Elements())
								{
									if (paramNode.Name.LocalName == PARAM_NODE_NAME)
									{
										XAttribute attribute = paramNode.Attribute("name");
										if (attribute != null && paramNode.Attribute("name").Value.ToLower() == VARIABLES_PARAM_NODE_NAME)
										{
											response.variables = paramNode.Value;
											break;
										}
									}
								}
								break;
							case TRACK_URL_NODE_NAME:
								response.trackUrl = node.Value;
								break;
							default:
								break;
						}
					}
				}
			}
			catch (System.Exception)
			{}

			return response;
		}
	}
}
