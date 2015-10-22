using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.UI;

namespace Escc.Umbraco
{
    public class WebApiHandler : DelegatingHandler
    {
        /// <summary>
        /// This is a "temporary" work-around to allow custom messages to be displayed when saving content.
        /// See https://our.umbraco.org/forum/umbraco-7/developing-umbraco-7-packages/53699-User-Message-former-Speech-bubble-in-custom-event#comment-197032
        /// The problem is solved in Umbraco 7.3.0+ at which time this could be removed and replaced with the code (or similar) currently commented out
        /// in ContentService_Publishing in UnpublishAtEventHandler.cs
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.AbsolutePath.ToLower() == "/umbraco/backoffice/umbracoapi/content/postsave")
            {
                return base.SendAsync(request, cancellationToken)
                    .ContinueWith(task =>
                    {
                        var response = task.Result;
                        try
                        {
                            var data = response.Content;
                            var content = ((ObjectContent)(data)).Value as ContentItemDisplay;
                            if (content != null)
                            {
                                if (content.Notifications.Count > 0)
                                {
                                    foreach (var notification in content.Notifications)
                                    {
                                        if (notification.Header.Equals("Publish") && notification.Message.ToLower().Contains("publishing was cancelled"))
                                        {
                                            //change the default notification to our custom message
                                            notification.NotificationType = SpeechBubbleIcon.Error;
                                            notification.Header = "Invalid Date";

                                            // The Publish event has been cancelled, so we only need to handle error conditions
                                            if (content.ExpireDate.HasValue)
                                            {
                                                if (UnpublishOverrides.UnpublishOverrides.CheckOverride(content))
                                                {
                                                    notification.Message = "You cannot enter an 'Unpublish at' date for this page";
                                                    continue;
                                                }

                                                if (content.ExpireDate < DateTime.Now.AddDays(1).AddMinutes(-10))
                                                {
                                                    notification.Message = "The 'Unpublish at' date must be at least 1 day in the future";
                                                    continue;
                                                }

                                                notification.Message = "The 'Unpublish at' date cannot be more than 6 months in the future";
                                            }
                                            else
                                            {
                                                if (!UnpublishOverrides.UnpublishOverrides.CheckOverride(content))
                                                {
                                                    notification.Message = "The 'Unpublish at' date is required";
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error<WebApiHandler>("Error changing custom unpublish date error message.", ex);
                        }
                        return response;
                    }, cancellationToken);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}