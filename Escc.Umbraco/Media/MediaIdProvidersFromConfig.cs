using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Services;

namespace Escc.Umbraco.Media
{
    /// <summary>
    /// Instantiates all the available media providers in this project based on the property editor aliases listed in web.config
    /// </summary>
    public class MediaIdProvidersFromConfig
    {
        private readonly IMediaService _mediaService;
        private readonly IDataTypeService _dataTypeService;

        /// <summary>
        /// Creates a new instance of <see cref="MediaIdProvidersFromConfig"/>
        /// </summary>
        /// <param name="mediaService">The Umbraco media service</param>
        /// <param name="dataTypeService">The Umbraco data type service</param>
        public MediaIdProvidersFromConfig(IMediaService mediaService, IDataTypeService dataTypeService)
        {
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            _dataTypeService = dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));
        }

        /// <summary>
        /// Loads all of the available providers from web.config
        /// </summary>
        /// <returns></returns>
        public IList<IMediaIdProvider> LoadProviders()
        {
            var mediaIdProviders = new List<IMediaIdProvider>();

            var config = ConfigurationManager.GetSection("Escc.Umbraco/MediaIdProviders") as NameValueCollection;
            if (config != null)
            {
                if (!string.IsNullOrWhiteSpace(config["MediaPickerIdProvider"]))
                {
                    mediaIdProviders.Add(new MediaPickerIdProvider(config["MediaPickerIdProvider"].Split(','), _dataTypeService));
                }
                if (!string.IsNullOrWhiteSpace(config["HtmlMediaIdProvider"]))
                {
                    mediaIdProviders.Add(new HtmlMediaIdProvider(config["HtmlMediaIdProvider"].Split(','), _mediaService));
                }
                if (!string.IsNullOrWhiteSpace(config["RelatedLinksIdProvider"]))
                {
                    mediaIdProviders.Add(new RelatedLinksMediaIdProvider(config["RelatedLinksIdProvider"].Split(','), _mediaService));
                }
                if (!string.IsNullOrWhiteSpace(config["UrlMediaIdProvider"]))
                {
                    mediaIdProviders.Add(new UrlMediaIdProvider(config["UrlMediaIdProvider"].Split(','), _mediaService));
                }
            }

            return mediaIdProviders;
        }
    }
}
