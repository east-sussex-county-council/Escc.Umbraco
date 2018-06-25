using System;

namespace Escc.Umbraco.Caching
{
    /// <summary>
    /// A source of data about when a resource will expire
    /// </summary>
    public interface IExpiryDateSource
    {
        /// <summary>
        /// Gets the expiry date.
        /// </summary>
        /// <value>
        /// The expiry date.
        /// </value>
        DateTime? ExpiryDate { get; }
    }
}