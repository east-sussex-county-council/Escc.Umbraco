using Escc.Umbraco.MediaSync;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Escc.Umbraco.Media
{
    class RequireAlternativeTextEventHandler : IApplicationEventHandler
    {
        private readonly IMediaSyncConfigurationProvider _config = new MediaSyncConfigurationFromXml();
        private IEnumerable<IRelatedMediaIdProvider> _mediaIdProviders;

        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            ContentService.Saving += ContentService_Saving;
            MediaService.Saved += MediaService_Saved;
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

                        if (mediaItem != null && mediaItem.ContentType.Alias.ToLower() == "image")
                        {
                            var ValidateMediaItem = MediaFilenameValidation.ValidMediaItem(mediaItem);
                            if(ValidateMediaItem.Item1 == false)
                            {
                                e.CancelOperation(new EventMessage("Invalid Media", string.Format("{0}", ValidateMediaItem.Item2), EventMessageType.Error));
                            }

                            var ValidateForImage = MediaFilenameValidation.CheckMediaForImage(mediaItem);
                            if (ValidateForImage.Item1 == false)
                            {
                                e.CancelOperation(new EventMessage("Invalid Media", string.Format("{0}", ValidateForImage.Item2), EventMessageType.Error));
                                break;
                            }

                            var ValidateName = MediaFilenameValidation.ValidMediaName(mediaItem);
                            if (ValidateName.Item1 == false)
                            {
                                e.CancelOperation(new EventMessage("Invalid Media", string.Format("{0}", ValidateName.Item2), EventMessageType.Error));
                            }

                            var ValidateForFileExtensions = MediaFilenameValidation.CheckMediaForFileExtensions(mediaItem);
                            if (ValidateForFileExtensions.Item1 == false)
                            {
                                e.CancelOperation(new EventMessage("Invalid Media", string.Format("{0}", ValidateForFileExtensions.Item2), EventMessageType.Error));
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Check that the Media item name does not match the filename, or have a common image extension.
        /// Because the media item name is used as the image alt tag.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaService_Saved(IMediaService sender, SaveEventArgs<IMedia> e)
        {
            var imageMediaItems = e.SavedEntities.Where(x => x.ContentType.Alias.ToLower() == "image");

            foreach (var mediaItem in imageMediaItems)
            {
                var ValidateMediaItem = MediaFilenameValidation.ValidMediaItem(mediaItem);
                if (ValidateMediaItem.Item1 == false)
                {
                    e.Messages.Add(new EventMessage("Invalid Media", string.Format("The media was saved. However {0}", ValidateMediaItem.Item2), EventMessageType.Warning));
                    break;
                }

                var ValidateForImage = MediaFilenameValidation.CheckMediaForImage(mediaItem);
                if (ValidateForImage.Item1 == false)
                {
                    e.Messages.Add(new EventMessage("Invalid Media Item", string.Format("The media was saved. However {0}", ValidateForImage.Item2), EventMessageType.Warning));
                    break;
                }

                var ValidateName = MediaFilenameValidation.ValidMediaName(mediaItem);
                if (ValidateName.Item1 == false)
                {
                    e.Messages.Add(new EventMessage("Invalid Name", string.Format("The media was saved. However {0}", ValidateName.Item2), EventMessageType.Warning));
                    break;
                }

                var ValidateForFileExtensions = MediaFilenameValidation.CheckMediaForFileExtensions(mediaItem);
                if (ValidateForFileExtensions.Item1 == false)
                {
                    e.Messages.Add(new EventMessage("Invalid Name", string.Format("The media was saved. However {0}", ValidateForFileExtensions.Item2), EventMessageType.Warning));
                    break;
                }

            }
        }
    }
}
