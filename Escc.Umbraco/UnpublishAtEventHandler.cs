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
            if (UnpublishOverrides.UnpublishOverrides.IsEnabled)
            {
                // Check that node is OK to publish
                ContentService.Publishing += ContentService_Publishing;
            }
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
                        // Check for override
                        if (UnpublishOverrides.UnpublishOverrides.CheckOverride(entity))
                        {
                            // Date not allowed because there is an override
                            e.CancelOperation(new EventMessage("Publish Failed", "You cannot enter an 'Unpublish at' date for this page", EventMessageType.Error));
                        }
                        
                        // Date cannot be less than 1 day in the future
                        else if (entity.ExpireDate < DateTime.Now.AddDays(1).AddMinutes(-10))
                        {
                            e.CancelOperation(new EventMessage("Publish Failed", "The 'Unpublish at' date must be at least 1 day in the future", EventMessageType.Error));
                        }

                        // Date cannot be more than 6 months in the future
                        else if (entity.ExpireDate > maxDate)
                        {
                            e.CancelOperation(new EventMessage("Publish Failed", "The 'Unpublish at' date cannot be more than 6 months in the future", EventMessageType.Error));
                        }
                    }
                    else
                    {
                        // Check for no override
                        if (!UnpublishOverrides.UnpublishOverrides.CheckOverride(entity))
                        {
                            // Date is required as no override exists
                            e.CancelOperation(new EventMessage("Publish Failed", "The 'Unpublish at' date is required", EventMessageType.Error));                            
                        }

                        // No date is OK because there is an override
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<UnpublishAtEventHandler>("Error checking page expiry date.", ex);
                e.CancelOperation(new EventMessage("Publish Failed", ex.Message, EventMessageType.Error));
            }
        }
    }
}
