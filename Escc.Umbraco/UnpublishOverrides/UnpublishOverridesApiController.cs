using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Exceptionless;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.WebApi;

namespace Escc.Umbraco.UnpublishOverrides
{
    /// <summary>
    /// API to manipulate Umbraco content expiry dates according to the policy specified in the &lt;UnpublishOverrides&gt; section of web.config
    /// </summary>
    /// <seealso cref="Umbraco.Web.WebApi.UmbracoApiController" />
    [Authorize]
    public class UnpublishOverridesApiController : UmbracoApiController
    {
        /// <summary>
        /// Ensures the unpublish dates for all published content match the policy specified in web.config
        /// </summary>
        [HttpPost]
        public void EnsureUnpublishDatesMatchPolicy()
        {
            var contentService = ApplicationContext.Current.Services.ContentService;
            var rootnodes = contentService.GetRootContent();

            foreach (var node in rootnodes)
            {
                if (node.HasPublishedVersion)
                {
                    SetOrRemoveUnpublishDate(contentService, node);

                    var descendants = node.Descendants().Where(descendant => descendant.HasPublishedVersion);
                    foreach (var descendant in descendants)
                    {
                        SetOrRemoveUnpublishDate(contentService, descendant);
                    }
                }
            }
        }

        private void SetOrRemoveUnpublishDate(IContentService contentService, IContent node)
        {
            var shouldBeNever = UnpublishOverrides.CheckOverride(node);

            try
            {
                if (node.ExpireDate.HasValue && shouldBeNever)
                {
                    node.ExpireDate = null;
                    contentService.SaveAndPublishWithStatus(node);
                }
                else if (!node.ExpireDate.HasValue && !shouldBeNever)
                {
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