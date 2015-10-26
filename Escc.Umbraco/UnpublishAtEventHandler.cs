using System;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;

namespace Escc.Umbraco
{
    /// <summary>
    /// Manage page expiry
    /// </summary>
    class UnpublishAtEventHandler : IApplicationEventHandler
    {
        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            // Used to override the standard "3rd party" message with our own
            GlobalConfiguration.Configuration.MessageHandlers.Add(new WebApiHandler()); 

            // Check that node is OK to publish
            ContentService.Publishing += ContentService_Publishing;
        }

        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        static void ContentService_Publishing(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            // Check if the node has an override for the UnPublish date.
            // Need to check in the Publishing event as the URL is not assigned until now.
            try
            {
                // Get default time period. Expiry time will be the same as the node creation time.
                var maxDate = DateTime.Now.AddMonths(6);

                // Check if there is an override for this content element. 
                // If not, check that the unPublish date is within allowed date range.
                foreach (var entity in e.PublishedEntities)
                {
                    if (entity.Id == 0)
                    {
                        // Do a save to get the Id and other info
                        ApplicationContext.Current.Services.ContentService.Save(entity);
                    }


                    if (entity.ExpireDate.HasValue)
                    {
                        // Check there isn't an override
                        if (UnpublishOverrides.UnpublishOverrides.CheckOverride(entity))
                        {
                            // Date not allowed because there is an override
                            entity.ChangePublishedState(PublishedState.Saved);
                            e.Cancel = true;
                            continue;
                        }

                        // Ensure date is valid
                        if (entity.ExpireDate >= DateTime.Now.AddDays(1).AddMinutes(-10) && entity.ExpireDate <= maxDate)
                        {
                            continue;
                        }

                        // validation failed
                        entity.ChangePublishedState(PublishedState.Saved);
                        e.Cancel = true;
                    }
                    else
                    {
                        // Date is not allowed if there is an override
                        if (UnpublishOverrides.UnpublishOverrides.CheckOverride(entity))
                        {
                            // No date is OK because there is an override
                            continue;
                        }

                        // Date is required as no override exists
                        entity.ChangePublishedState(PublishedState.Saved);
                        e.Cancel = true;
                    }

                    // Won't work until V7.3.0!! See http://issues.umbraco.org/issue/U4-5927
                    // As a work-around, messages are currently being displayed in SendAsync in WebApiHandler.cs
                    // e.Messages.Add(new EventMessage("Save Failed", "Unpublish date is not valid", EventMessageType.Error));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<UnpublishAtEventHandler>("Error checking page expiry date.", ex);
            }
        }
    }
}
