using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using Exceptionless;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.WebApi;

namespace Escc.Umbraco.UnpublishOverrides
{
    /// <summary>
    /// API to manipulate Umbraco content expiry dates according to the policy specified in the &lt;UnpublishOverrides&gt; section of web.config
    /// </summary>
    [Authorize]
    public class UnpublishOverridesApiController : UmbracoApiController
    {
        /// <summary>
        /// Ensures the unpublish dates for all published content match the policy specified in web.config
        /// </summary>
        [HttpPost]
        public void EnsureUnpublishDatesMatchPolicy()
        {
            var rootnodes = UmbracoContext.ContentCache.GetAtRoot();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = Request.Headers.Authorization;


            foreach (var node in rootnodes)
            {
                SetOrRemoveUnpublishDate(node);

                LogHelper.Info<UnpublishOverridesApiController>($"Starting firing unpublish dates for node {node.Id} {node.Name}");
                client.PostAsync(Request.RequestUri.ToString().Replace("EnsureUnpublishDatesMatchPolicy", "SetOrRemoveUnpublishDateForChildNodes") + "?parentNodeId=" + node.Id, new StringContent(String.Empty));
                LogHelper.Info<UnpublishOverridesApiController>($"Completed firing unpublish dates for node {node.Id} {node.Name}");
            }
        }

        [HttpPost]
        public void SetOrRemoveUnpublishDateForChildNodes(int parentNodeId)
        {

            var parentNode = UmbracoContext.ContentCache.GetById(parentNodeId);

            if (parentNode != null)
            {
                LogHelper.Info<UnpublishOverridesApiController>($"Starting updating unpublish dates for node {parentNode.Id} {parentNode.Name}");
                SetOrRemoveUnpublishDateForChildNodes(parentNode);
                LogHelper.Info<UnpublishOverridesApiController>($"Completed updating unpublish dates for node {parentNode.Id} {parentNode.Name}");
            }
        }

        /// <summary>
        /// Sets or removes the unpublish date for child nodes.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <remarks>Use recursion rather than .Descendants() to avoid generating a long-running query that times out when there's a lot of content</remarks>
        private void SetOrRemoveUnpublishDateForChildNodes(IPublishedContent node)
        {
            foreach (var child in node.Children())
            {
                SetOrRemoveUnpublishDate(child);
                if (node.Level > 2)
                {
                    SetOrRemoveUnpublishDateForChildNodes(child);
                }
                else
                {
                    LogHelper.Info<UnpublishOverridesApiController>($"Starting firing unpublish dates for node {child.Id} {child.Name}");
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = Request.Headers.Authorization;
                    client.PostAsync(Regex.Replace(Request.RequestUri.ToString(), "parentNodeId=[0-9]+", "parentNodeId=" + child.Id), new StringContent(String.Empty));
                    LogHelper.Info<UnpublishOverridesApiController>($"Completed firing unpublish dates for node {child.Id} {child.Name}");
                }
            }

        }

        private void SetOrRemoveUnpublishDate(IPublishedContent publishedContent)
        {
            var contentService = ApplicationContext.Current.Services.ContentService;
            var shouldBeNever = UnpublishOverrides.CheckOverride(publishedContent);

            try
            {
                var unpublishDate = publishedContent.GetPropertyValue<DateTime?>("unpublishAt");
                if (unpublishDate.HasValue && shouldBeNever)
                {
                    var node = contentService.GetById(publishedContent.Id);
                    node.ExpireDate = null;
                    contentService.SaveAndPublishWithStatus(node);
                }
                else if (!unpublishDate.HasValue && !shouldBeNever)
                {
                    var node = contentService.GetById(publishedContent.Id);
                    node.ExpireDate = DateTime.Now.AddMonths(6);
                    contentService.SaveAndPublishWithStatus(node);
                }
            }
            catch (Exception e)
            {
                e.ToExceptionless().Submit();
            }
        }
    }
}