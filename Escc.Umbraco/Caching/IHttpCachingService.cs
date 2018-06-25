using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Core.Models;

namespace Escc.Umbraco.Caching
{
    /// <summary>
    /// Manage HTTP caching for Umbraco content
    /// </summary>
    public interface IHttpCachingService
    {
        /// <summary>
        /// Sets the content of the HTTP cache headers from well-known Umbraco properties.
        /// </summary>
        /// <param name="content">The Umbraco published content node.</param>
        /// <param name="isPreview">if set to <c>true</c> Umbraco is in preview mode.</param>
        /// <param name="cachePolicy">The cache policy.</param>
        /// <param name="cacheExpiryDates">Additional dates which should cause the cache to expire.</param>
        /// <param name="defaultCachePeriodInSeconds">The default cache period in seconds.</param>
        void SetHttpCacheHeadersFromUmbracoContent(IPublishedContent content, bool isPreview, HttpCachePolicyBase cachePolicy, IEnumerable<IExpiryDateSource> cacheExpiryDates = null, int defaultCachePeriodInSeconds = 86400);

        /// <summary>
        /// Works out how long to cache based on a default period and any expiry dates, relative to a start date.
        /// </summary>
        /// <param name="relativeToDate">The start date cache time spans are relative to, usually <see cref="DateTime.UtcNow"/>.</param>
        /// <param name="defaultCachePeriod">The default cache period.</param>
        /// <param name="contentExpiryDates">Expiry dates for any content, either part of a page or the whole page.</param>
        /// <returns>An absolute time and relative timespan representing how long to cache the content for.</returns>
        CacheFreshness WorkOutCacheFreshness(DateTime relativeToDate, TimeSpan defaultCachePeriod, IList<DateTime> contentExpiryDates);
    }
}