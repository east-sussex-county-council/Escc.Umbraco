using System;
using System.IO;
using System.Linq;
using Examine;
using HtmlAgilityPack;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Escc.Umbraco.InternalLinks
{
    /// <summary>
    /// If you select content or media in the back office, Umbraco saves the node id to help it track the target page - for example, updating the link if it's moved.
    /// If you simply paste a URL it doesn't do that, so this event handler looks up those pasted internal links and converts them to the tracked format.
    /// </summary>
    /// <seealso cref="Umbraco.Core.IApplicationEventHandler" />
    public class TrackInternalLinksEventHandler : IApplicationEventHandler
    {
        /// <summary>
        /// ApplicationContext is created and other static objects that require initialization have been setup
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        /// <summary>
        /// Bootup is completed, this allows you to perform any other bootup logic required for the application.
        /// Resolution is frozen so now they can be used to resolve instances.
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        /// <summary>
        /// All resolvers have been initialized but resolution is not frozen so they can be modified in this method
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Saving += ContentService_Saving;
        }

        private void ContentService_Saving(IContentService sender, global::Umbraco.Core.Events.SaveEventArgs<global::Umbraco.Core.Models.IContent> e)
        {
            foreach (var node in e.SavedEntities)
            {
                foreach (var property in node.Properties)
                {
                    if (property.Value != null)
                    {
                        // Parse the property value for HTML
                        var html = new HtmlDocument();
                        html.LoadHtml(property.Value.ToString());

                        // Check for and update pasted links
                        var updated = UpdateLinksToContentNodes(html, UmbracoContext.Current);
                        updated = updated || UpdateLinksToMediaNodes(html);

                        // If a link was found and updated, update the property value
                        if (updated)
                        {
                            using (var writer = new StringWriter())
                            {
                                html.Save(writer);
                                property.Value = writer.ToString();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the links to content nodes by replacing the URL with <c>/{localLink:nodeId}</c>
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private bool UpdateLinksToContentNodes(HtmlDocument html, UmbracoContext context)
        {
            var internalLinks = html.DocumentNode.SelectNodes("//a[starts-with(@href,'/') and not(starts-with(@href,'/{localLink:')) and not(starts-with(@href,'/media/'))]");
            if (internalLinks == null) return false;

            var updated = false;
            foreach (var element in internalLinks)
            {
                var linkTarget = context.ContentCache.GetByRoute(element.GetAttributeValue("href", String.Empty));
                if (linkTarget != null)
                {
                    element.SetAttributeValue("href", "/{localLink:" + linkTarget.Id + "}");
                    element.SetAttributeValue("data-id", linkTarget.Id.ToInvariantString());
                    updated = true;
                }
            }
            return updated;
        }

        /// <summary>
        /// Updates the links to media nodes by adding the media ID in a <c>data-id</c> attribute.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns></returns>
        private bool UpdateLinksToMediaNodes(HtmlDocument html)
        {
            var internalLinks = html.DocumentNode.SelectNodes("//a[starts-with(@href,'/media/') and not(@data-id)]");
            if (internalLinks == null) return false;

            var searcher = ExamineManager.Instance.SearchProviderCollection["InternalSearcher"];
            if (searcher == null) return false;

            var updated = false;
            foreach (var element in internalLinks)
            {
                var criteria = searcher.CreateSearchCriteria("media");
                var filter = criteria.Field("umbracoFile", element.GetAttributeValue("href", String.Empty));
                var results = searcher.Search(filter.Compile());

                if (results.Any())
                {
                    var linkTarget = results.First();
                    element.SetAttributeValue("data-id", linkTarget.Id.ToInvariantString());
                    updated = true;
                }
            }
            return updated;
        }
    }
}