﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using Examine;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Security;
using UmbracoExamine;

namespace Escc.Umbraco
{
    class ExamineEventHandler : IApplicationEventHandler
    {
        private const string NodeLinksIndexer = "NodeLinksIndexer";

        private const string LinkMatchPattern = "<a.* href=\"(/.*?)[\\.|\"].*>(.*?)</a>";

        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ExamineManager.Instance.IndexProviderCollection[NodeLinksIndexer].GatheringNodeData += NodeLinksIndexer_GatheringNodeData;
        }

        private void NodeLinksIndexer_GatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        {
            // Find outgoing links and add the destination node Id to the index.
            // e.g. <a href="/{localLink:18746}" title="Your Council">Your Council</a>
            try
            {
                //check if this is 'Content' (as opposed to media, etc...)
                if (e.IndexType == IndexTypes.Content)
                {
                    e.Fields.Add("NodeLinksTo", OutgoingLinks(e));
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error<UnpublishAtEventHandler>("Error in NodeLinksIndexer_GatheringNodeData.", ex);
            }
        }

        private string OutgoingLinks(IndexingNodeDataEventArgs e)
        {
            // Check field for links to another node

            // option 1: <a href="/{localLink:18746}" title="Your Council">Your Council</a>
            // Extract and return node Id

            // option 2: <a href="/leisureandtourism/countryside">Discover East Sussex</a>
            // check if the link goes to an existing node in Umbraco and if so, return its Id

            // Don't process this style of link (absolute Url)
            // <a href="https://public.govdelivery.com/accounts/UKESCC/subscriber/new">clicking here</a>

            // Tests
            // <a href="/{localLink:18746}">Your Council</a>
            // <a href="/{localLink:18747}" title="My Council">My Council</a>
            // <a title="My Council" href="/{localLink:18748}">Our Council</a>
            // <a href="/test/page/link">Test Page link</a>
            // <a href="mailto:test@test.test">Email Me</a>
            //
            // RegEx
            // <a.* href=\"(/.*?)\".*>(.*?)</a>
            //
            // Gives
            // Match	                                                        $1	                     $2
            // <a href="/{localLink:18746}">Your Council</a>	                /{localLink:18746}	     Your Council
            // <a href="/{localLink:18747}" title="My Council">My Council</a>	/{localLink:18747}	     My Council
            // <a title="My Council" href="/{localLink:18748}">Our Council</a>	/{localLink:18748}	     Our Council
            // <a href="/test/page/link">Test Page link</a>	                    /test/page/link	         Test Page link

            //var regexExp = "<a.* href=\"/{localLink:([^}]*)}\".*>(.*?)</a>";
            //const string regexExp = "<a.* href=\"(/.*?)\".*>(.*?)</a>";
            var regex = new Regex(LinkMatchPattern);

            var linkedNodes = new List<string>();

            // Look through each content field on the node
            foreach (var fld in e.Fields)
            {
                // Find all matches (links) in the content field
                var matches = regex.Matches(fld.Value);

                // Check each of the matches
                foreach (Match match in matches)
                {
                    var m = match.Groups[1].ToString();

                    // Convert to the node Id
                    var nodeId = GetNodeId(m);

                    // Add the Node Id to the list, if it isn't empty
                    if (!string.IsNullOrEmpty(nodeId)) linkedNodes.Add(nodeId);
                }
            }

            // return a space separated string of distinct node Ids
            return String.Join(" ", linkedNodes.Distinct().Select(x => x.ToString()).ToArray());
        }

        private string GetNodeId(string linkString)
        {
            //Figure out which of these we have and get the node Id
            // <a href="/{localLink:18746}">Your Council</a>	                /{localLink:18746}	     Your Council
            // <a href="/{localLink:18747}" title="My Council">My Council</a>	/{localLink:18747}	     My Council
            // <a title="My Council" href="/{localLink:18748}">Our Council</a>	/{localLink:18748}	     Our Council
            // <a href="/test/page/link">Test Page link</a>	                    /test/page/link	         Test Page link

            var rtnId = string.Empty;

            if (linkString.Contains("{localLink:"))
            {
                rtnId = Regex.Match(linkString, @"\d+").Value;
            }
            else // starts with a "/"
            {
                // Make sure we have a current Umbraco Context
                if (UmbracoContext.Current == null)
                {
                    var dummyContext = new HttpContextWrapper(new HttpContext(new SimpleWorkerRequest("/", string.Empty, new StringWriter())));
                    UmbracoContext.EnsureContext(
                        dummyContext,
                        ApplicationContext.Current,
                        new WebSecurity(dummyContext, ApplicationContext.Current),
                        false);
                }

                var linkNode = uQuery.GetNodeByUrl(linkString);

                // Only record the link if the destination page was found
                if (linkNode.Id != -1)
                {
                    rtnId = linkNode.Id.ToString();
                }
            }

            return rtnId;
        }
    }
}
