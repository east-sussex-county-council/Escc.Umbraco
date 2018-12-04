using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Umbraco.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Services;

namespace Escc.Umbraco.Media
{
    /// <summary>
    /// Gets the ids of media items linked within a grid property that contains HTML
    /// </summary>
    public class GridHtmlMediaIdProvider : IMediaIdProvider
    {
        private readonly List<string> _propertyEditorAlises = new List<string>();
        private readonly IMediaService _mediaService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridHtmlMediaIdProvider" /> class.
        /// </summary>
        /// <param name="propertyEditorAliases">The aliases of the property editors that might be responsible for saving media ids.</param>
        /// <param name="mediaService">The Umbraco media service</param>
        /// <exception cref="ArgumentNullException">mediaService</exception>
        public GridHtmlMediaIdProvider(IEnumerable<string> propertyEditorAliases, IMediaService mediaService)
        {
            if (propertyEditorAliases != null)
            {
                _propertyEditorAlises.AddRange(propertyEditorAliases);
            }
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        }

        /// <summary>
        /// Determines whether this instance can read the type of property identified by its property editor alias
        /// </summary>
        /// <param name="propertyType">The property defined on the document type.</param>
        /// <returns></returns>
        public bool CanReadPropertyType(PropertyType propertyType)
        {
            return _propertyEditorAlises.Contains(propertyType.PropertyEditorAlias, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Reads media ids from the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public IEnumerable<int> ReadProperty(Property property)
        {
            var mediaIds = new List<int>();

            if (property != null && property.Value != null && !String.IsNullOrEmpty(property.Value.ToString()))
            {
                var mediaGuids = ReadMediaGuidsFromGridJson(property.Value.ToString());

                foreach (var mediaGuid in mediaGuids)
                {
                    var mediaItem = _mediaService.GetById(mediaGuid);
                    if (mediaItem != null)
                    {
                        mediaIds.Add(mediaItem.Id);
                    }
                }
            }

            return mediaIds;
        }

        /// <summary>
        /// Reads the media GUIDs from any rich text editor values in an Umbraco grid JSON value.
        /// </summary>
        /// <param name="gridJson">The grid json.</param>
        /// <returns></returns>
        public IEnumerable<Guid> ReadMediaGuidsFromGridJson(string gridJson)
        {
            var mediaGuids = new List<Guid>();

            if (!String.IsNullOrEmpty(gridJson))
            {
                var value = JsonConvert.DeserializeObject<GridValue>(gridJson);
                if (value != null)
                {
                    foreach (var section in value.Sections)
                    {
                        foreach (var row in section.Rows)
                        {
                            foreach (var area in row.Areas)
                            {
                                foreach (var control in area.Controls)
                                {
                                    if (control.Editor.Alias == "rte")
                                    {
                                        ParseMediaIds(control.Value.ToString(), mediaGuids);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return mediaGuids;
        }

        private void ParseMediaIds(string value, List<Guid> mediaGuids)
        {
            var html = new HtmlDocument();
            html.LoadHtml(value);
            var mediaLinks = html.DocumentNode.SelectNodes("//a[starts-with(@data-udi,'umb://media/')]");
            if (mediaLinks != null)
            {
                foreach (var mediaLink in mediaLinks)
                {
                    mediaGuids.Add(new Guid(mediaLink.Attributes["data-udi"].Value.Substring(12)));
                }
            }
        }
    }
}
