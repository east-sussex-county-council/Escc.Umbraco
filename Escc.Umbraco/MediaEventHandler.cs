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
                // Check if media item name is valid
                if (Validation.ValidMediaName(mediaItem)) continue;

                // media item name is invalid, so add a message
                var errMsg = string.Format("{0} is not a valid name, you must change the name before the image can be used.", mediaItem.Name);

                // Show as a warning message, as the save is not cancelled
                e.Messages.Add(new EventMessage("Invalid Name", errMsg, EventMessageType.Warning));
            }
        }
    }
}
