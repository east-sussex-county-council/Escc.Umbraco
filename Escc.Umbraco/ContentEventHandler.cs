using Escc.Umbraco.MediaSync;
using Escc.Umbraco.Services;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Escc.Umbraco
{
    class ContentEventHandler : IApplicationEventHandler
    {
        private readonly IMediaSyncConfigurationProvider _config = new XmlConfigurationProvider();
        private IEnumerable<IRelatedMediaIdProvider> _mediaIdProviders;

        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Saving += ContentService_Saving;
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        /// <summary>
        /// Validate the content before it is actually saved. 
        /// Ensure all Media images have a valid name, because it is used as the Alt tag.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentService_Saving(IContentService sender, SaveEventArgs<IContent> e)
        {
            if (_mediaIdProviders == null)
            {
                _mediaIdProviders = new List<IRelatedMediaIdProvider>() {
                        new MediaPickerIdProvider(_config, ApplicationContext.Current.Services.DataTypeService),
                        new HtmlMediaIdProvider(_config),
                        new RelatedLinksMediaIdProvider(_config),
                        new UrlMediaIdProvider(_config)
                    };
            }

            foreach (var contentItem in e.SavedEntities)
            {
                // Check that all Images used are named correctly (not the same as the file name)
                foreach (var propertyType in contentItem.PropertyTypes)
                {
                    var p = _mediaIdProviders.FirstOrDefault(m => m.CanReadPropertyType(propertyType));
                    if (p == null) continue;

                    var idList = p.ReadProperty(contentItem.Properties[propertyType.Alias]);

                    foreach (var mediaNodeId in idList)
                    {
                        var mediaItem = uMediaSyncHelper.mediaService.GetById(mediaNodeId);
                        if (Validation.ValidMediaName(mediaItem)) continue;

                        // Cancel the save
                        var errMsg = string.Format("{0}: {1} is not a valid name, you must change the name before the image can be used.", propertyType.Name, mediaItem.Name);
                        e.CancelOperation(new EventMessage("Invalid Media", errMsg, EventMessageType.Error));
                    }
                }
            }
        }
    }
}
