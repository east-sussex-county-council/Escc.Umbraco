using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Services;

namespace Escc.Umbraco.Media
{
    /// <summary>
    /// Gets the ids of media items linked within a related links property
    /// </summary>
    public class RelatedLinksMediaIdProvider : IMediaIdProvider
    {
        private readonly List<string> _propertyEditorAlises = new List<string>();
        private readonly IMediaService _mediaService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlMediaIdProvider" /> class.
        /// </summary>
        /// <param name="propertyEditorAliases">The aliases of the property editors that might be responsible for saving media ids.</param>
        /// <param name="mediaService">The Umbraco media service</param>
        /// <exception cref="ArgumentNullException">mediaService</exception>
        public RelatedLinksMediaIdProvider(IEnumerable<string> propertyEditorAliases, IMediaService mediaService)
        {
            if (propertyEditorAliases == null)
            {
                throw new ArgumentNullException(nameof(propertyEditorAliases));
            }

            _propertyEditorAlises.AddRange(propertyEditorAliases);
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        }

        /// <summary>
        /// Determines whether this instance can read the type of property identified by its property editor alias
        /// </summary>
        /// <param name="propertyType">The property defined on the document type.</param>
        /// <returns></returns>
        public bool CanReadPropertyType(PropertyType propertyType)
        {
            return _propertyEditorAlises.Contains(propertyType.PropertyEditorAlias.ToUpperInvariant());
        }

        /// <summary>
        /// Reads media ids from the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public IEnumerable<int> ReadProperty(Property property)
        {
            var mediaIds = new List<int>();

            if (!String.IsNullOrEmpty(property?.Value?.ToString()))
            {
                var relatedLinks = JsonConvert.DeserializeObject<JArray>(property.Value.ToString());
                foreach (var relatedLink in relatedLinks)
                {
                    try
                    {
                        var uri = new Uri(relatedLink.Value<string>("link"), UriKind.RelativeOrAbsolute);
                        string mediaPath = (uri.IsAbsoluteUri ? uri.AbsolutePath : uri.ToString());
                        if (!mediaPath.StartsWith("/media/", StringComparison.OrdinalIgnoreCase)) continue;

                        var mediaItem = _mediaService.GetMediaByPath(mediaPath);
                        if (mediaItem != null)
                        {
                            mediaIds.Add(mediaItem.Id);
                        }
                    }
                    catch (UriFormatException)
                    {
                        // if someone entered an invalid URL in a related links field, just ignore it and move on
                    }
                }
                
            }

            return mediaIds;
        }
    }
}
