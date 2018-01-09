using System;
using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Models;

namespace Escc.Umbraco.PropertyTypes
{
    /// <summary>
    /// Service to read data from an Umbraco related links field
    /// </summary>
    public class RelatedLinksService : IRelatedLinksService
    {
        private readonly IUrlTransformer[] _urlTransformers;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelatedLinksService"/> class.
        /// </summary>
        public RelatedLinksService() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelatedLinksService"/> class.
        /// </summary>
        /// <param name="urlTransformers">The URL transformers.</param>
        public RelatedLinksService(params IUrlTransformer[] urlTransformers)
        {
            _urlTransformers = urlTransformers;
        }

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
            if (propertyValue != null)
            {
                propertyValue.Each(relatedLink => links.Add(LinkViewModelFromRelatedLink(relatedLink)));
            }

            return links;
        }

        private HtmlLink LinkViewModelFromRelatedLink(RelatedLink relatedLink)
        {
            try
            {
                var link = new HtmlLink()
                {
                    Text = relatedLink.Caption,
                    Url = new Uri(relatedLink.Link, UriKind.RelativeOrAbsolute)
                };

                if (_urlTransformers != null)
                {
                    foreach (var transformer in _urlTransformers)
                    {
                        link.Url = transformer.TransformUrl(link.Url);
                    }
                }

                return link;
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