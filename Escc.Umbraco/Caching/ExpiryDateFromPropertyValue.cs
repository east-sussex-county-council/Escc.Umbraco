using System;
using Umbraco.Web;
using Umbraco.Core.Models;

namespace Escc.Umbraco.Caching
{
    /// <summary>
    /// Gets a date from a date property on an Umbraco content node
    /// </summary>
    /// <seealso cref="Escc.Umbraco.Caching.IExpiryDateSource" />
    public class ExpiryDateFromPropertyValue : IExpiryDateSource
    {
        private readonly IPublishedContent _umbracoContent;
        private readonly string _propertyAlias;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpiryDateFromPropertyValue"/> class.
        /// </summary>
        /// <param name="content">The Umbraco content.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <exception cref="ArgumentException">propertyAlias must be specified - propertyAlias</exception>
        /// <exception cref="ArgumentNullException">content</exception>
        public ExpiryDateFromPropertyValue(IPublishedContent content, string propertyAlias)
        {
            if (string.IsNullOrEmpty(propertyAlias))
            {
                throw new ArgumentException("propertyAlias must be specified", nameof(propertyAlias));
            }

            _umbracoContent = content ?? throw new ArgumentNullException(nameof(content));
            _propertyAlias = propertyAlias;
        }

        /// <summary>
        /// Gets the expiry date.
        /// </summary>
        /// <value>
        /// The expiry date.
        /// </value>
        public DateTime? ExpiryDate {
            get
            {
                var expiryDate = _umbracoContent.GetPropertyValue<DateTime>(_propertyAlias);
                if (expiryDate != DateTime.MinValue) return expiryDate;
                return null;
            }
        }
    }
}
