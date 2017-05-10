using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escc.Umbraco.PropertyTypes
{
    /// <summary>
    /// Update a URL and return the modified URL
    /// </summary>
    public interface IUrlTransformer
    {
        /// <summary>
        /// Updates the URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        Uri TransformUrl(Uri url);
    }
}
