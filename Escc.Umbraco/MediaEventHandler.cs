using System.Linq;
using Escc.Umbraco.Services;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Escc.Umbraco
{
    class MediaEventHandler : IApplicationEventHandler
    {
        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            MediaService.Saved += MediaService_Saved;
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
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
                var ValidateMediaItem = Validation.ValidMediaItem(mediaItem);
                if (ValidateMediaItem.Item1 == false)
                {
                    e.Messages.Add(new EventMessage("Invalid Media", string.Format("#The save has not been cancelled, However {0}", ValidateMediaItem.Item2), EventMessageType.Warning));
                    break;
                }

                var ValidateForImage = Validation.CheckMediaForImage(mediaItem);
                if (ValidateForImage.Item1 == false)
                {
                    e.Messages.Add(new EventMessage("Invalid Media Item", string.Format("#The save has not been cancelled, However {0}", ValidateForImage.Item2), EventMessageType.Warning));
                    break;
                }

                var ValidateName = Validation.ValidMediaName(mediaItem);
                if (ValidateName.Item1 == false)
                {
                    e.Messages.Add(new EventMessage("Invalid Name", string.Format("#The save has not been cancelled, However {0}", ValidateName.Item2), EventMessageType.Warning));
                    break;
                }

                var ValidateForFileExtensions = Validation.CheckMediaForFileExtensions(mediaItem);
                if (ValidateForFileExtensions.Item1 == false)
                {
                    e.Messages.Add(new EventMessage("Invalid Name", string.Format("#The save has not been cancelled, However {0}", ValidateForFileExtensions.Item2), EventMessageType.Warning));
                    break;
                }

            }
        }
    }
}
