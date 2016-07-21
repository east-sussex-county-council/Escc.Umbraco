using System;
using System.Collections.Generic;
using AutoMapper;
using Our.Umbraco.PropertyConverters.Models;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Escc.Umbraco.PropertyTypes
{
    /// <summary>
    /// Service to read data from an Umbraco related links field
    /// </summary>
    public class RelatedLinksService : IRelatedLinksService
    {
        /// <summary>
        /// Reads a collection of links from an Umbraco related links property.
        /// </summary>
        /// <param name="content">The Umbraco content.</param>
        /// <param name="propertyAlias">The related links property alias.</param>
        /// <returns></returns>
        public IList<HtmlLink> BuildRelatedLinksViewModelFromUmbracoContent(IPublishedContent content, string propertyAlias)
        {
            var links = new List<HtmlLink>();
            var propertyValue = content.GetPropertyValue<RelatedLinks>(propertyAlias);
            if (propertyValue != null && propertyValue.PropertyData != "[]")
            {
                propertyValue.Each(relatedLink => links.Add(LinkViewModelFromRelatedLink(relatedLink)));
            }

            return links;
        }

        private static HtmlLink LinkViewModelFromRelatedLink(RelatedLink relatedLink)
        {
            try
            {
                return new HtmlLink()
                {
                    Text = relatedLink.Caption,
                    Url = new Uri(relatedLink.Link, UriKind.RelativeOrAbsolute)
                };
            }
            catch (UriFormatException)
            {
                return new HtmlLink()
                {
                    Text = relatedLink.Caption
                };
            }
        }
    }
}