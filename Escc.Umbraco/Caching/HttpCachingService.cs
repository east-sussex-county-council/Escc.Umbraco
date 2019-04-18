using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Escc.Umbraco.Caching
{
    /// <summary>
    /// Manage HTTP caching for Umbraco content
    /// </summary>
    public class HttpCachingService : IHttpCachingService
    {
        /// <summary>
        /// Sets the content of the HTTP cache headers from well-known Umbraco properties.
        /// </summary>
        /// <param name="content">The Umbraco published content node.</param>
        /// <param name="isPreview">if set to <c>true</c> Umbraco is in preview mode.</param>
        /// <param name="cachePolicy">The cache policy.</param>
        /// <param name="cacheExpiryDates">Additional dates which should cause the cache to expire.</param>
        /// <param name="defaultCachePeriodInSeconds">The default cache period in seconds.</param>
        public void SetHttpCacheHeadersFromUmbracoContent(IPublishedContent content, bool isPreview, HttpCachePolicyBase cachePolicy, IEnumerable<IExpiryDateSource> cacheExpiryDates = null, int defaultCachePeriodInSeconds = 86400)
        {
            // Default to 24 hours, but allow calling code or an Umbraco property on specific pages to override this
            var defaultCachePeriod = new TimeSpan(0, 0, 0, defaultCachePeriodInSeconds);
            var pageCachePeriod = ParseTimeSpan(content.GetProperty("cache")?.Value?.ToString());
            var cachePeriod = (pageCachePeriod == TimeSpan.Zero) ? defaultCachePeriod : pageCachePeriod;

            var expiryDates = new List<DateTime>();
            if (cacheExpiryDates != null)
            {
                foreach (var dateSource in cacheExpiryDates)
                {
                    if (dateSource != null)
                    {
                        var expiryDate = dateSource.ExpiryDate;
                        if (expiryDate.HasValue) expiryDates.Add(expiryDate.Value);
                    }
                }
            }

            SetHttpCacheHeaders(DateTime.UtcNow,
                cachePeriod,
                expiryDates,
                isPreview, cachePolicy);
        }

        /// <summary>
        /// Set HTTP cache headers based on data passed in.
        /// </summary>
        /// <param name="relativeToDate">The start date cache time spans are relative to, usually <see cref="DateTime.UtcNow"/>.</param>
        /// <param name="defaultCachePeriod">The default cache period.</param>
        /// <param name="contentExpiryDates">Expiry dates for any content, either part of a page or the whole page.</param>
        /// <param name="isPreview">if set to <c>true</c> [is preview].</param>
        /// <param name="cachePolicy">The cache policy.</param>
        private void SetHttpCacheHeaders(DateTime relativeToDate, TimeSpan defaultCachePeriod, IList<DateTime> contentExpiryDates, bool isPreview, HttpCachePolicyBase cachePolicy)
        {
            // Only do this if it's enabled in web.config
            if (!IsHttpCachingEnabled()) return;

            // Never use HTTP caching for anyone who can edit the site
            if (isPreview)
            {
                cachePolicy.SetCacheability(HttpCacheability.NoCache);
                cachePolicy.SetMaxAge(new TimeSpan(0));
                cachePolicy.AppendCacheExtension("must-revalidate, proxy-revalidate");
            }
            else
            {
                // Get cache period. 
                var freshness = WorkOutCacheFreshness(relativeToDate, defaultCachePeriod, contentExpiryDates);

                // Cache the page
                // Use "private" setting so that shared caches aren't used, because shared caches might cache the mobile view and serve it to a desktop user.
                // "Vary: User-Agent" should be the best way to prevent that, but .NET converts it to "Vary: *" which effectively prevents caching.
                cachePolicy.SetCacheability(HttpCacheability.Private);
                cachePolicy.SetExpires(freshness.FreshUntil.ToUniversalTime());
                cachePolicy.SetMaxAge(freshness.FreshFor);
            }
        }

        /// <summary>
        /// Works out how long to cache based on a default period and any expiry dates, relative to a start date.
        /// </summary>
        /// <param name="relativeToDate">The start date cache time spans are relative to, usually <see cref="DateTime.UtcNow"/>.</param>
        /// <param name="defaultCachePeriod">The default cache period.</param>
        /// <param name="contentExpiryDates">Expiry dates for any content, either part of a page or the whole page.</param>
        /// <returns>An absolute time and relative timespan representing how long to cache the content for.</returns>
        public CacheFreshness WorkOutCacheFreshness(DateTime relativeToDate, TimeSpan defaultCachePeriod, IList<DateTime> contentExpiryDates)
        {
            if (contentExpiryDates == null) throw new ArgumentNullException("contentExpiryDates");

            // Convert date to UTC so that it can be compared on an equal basis
            relativeToDate = relativeToDate.ToUniversalTime();

            // How long is this page fresh for?
            var freshness = new CacheFreshness()
            {
                FreshFor = defaultCachePeriod,
                FreshUntil = relativeToDate.Add(defaultCachePeriod)
            };

            for (var i = 0; i < contentExpiryDates.Count; i++)
            {
                // Convert date to UTC so that it can be compared on an equal basis
                contentExpiryDates[i] = contentExpiryDates[i].ToUniversalTime();

                // Check if some or all of the content expires sooner than the default cache period?
                OverrideDueToContentExpiry(freshness, relativeToDate, contentExpiryDates[i]);
            }

            return freshness;

        }

        private static void OverrideDueToContentExpiry(CacheFreshness freshness, DateTime relativeToDate, DateTime expiryDate)
        {
            if (expiryDate > relativeToDate && expiryDate < freshness.FreshUntil)
            {
                freshness.FreshFor = expiryDate.Subtract(relativeToDate);
                freshness.FreshUntil = relativeToDate.Add(freshness.FreshFor);
            }
        }

        /// <summary>
        /// Parses a time span from hardcoded values which are expected to be found in an Umbraco dropdown list property
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static TimeSpan ParseTimeSpan(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                switch (text)
                {
                    case "5 minutes":
                        return new TimeSpan(0, 5, 0);
                    case "10 minutes":
                        return new TimeSpan(0, 10, 0);
                    case "30 minutes":
                        return new TimeSpan(0, 30, 0);
                    case "1 hour":
                        return new TimeSpan(1, 0, 0);
                }
            }
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Determines whether HTTP caching is enabled for Umbraco pages 
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if Umbraco caching is enabled; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsHttpCachingEnabled()
        {
            // Look in config. If caching not explicitly disabled, it's enabled. Can also be explicitly enabled to allow override of higher-level web.config.
            var config = ConfigurationManager.GetSection("Escc.Umbraco/GeneralSettings") as NameValueCollection;
            if (config == null || String.IsNullOrEmpty(config["HttpCachingEnabled"]) || config["HttpCachingEnabled"].ToUpperInvariant() != "FALSE") return true;

            return false;
        }




    }
}