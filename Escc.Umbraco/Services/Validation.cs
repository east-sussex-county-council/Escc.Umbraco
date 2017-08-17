using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Escc.Umbraco.Services
{
    public class Validation
    {
        /// <summary>
        /// Check if the name of the Media item is valid
        /// </summary>
        /// <param name="mediaItem">
        /// Media item to validate
        /// </param>
        /// <returns>
        /// True if media item name is valid
        /// </returns>
        public static bool ValidMediaName(IMedia mediaItem)
        {
            var ValidName = true;

            // Get lowercase version of the actual filename and the containing Media item
            var fileName = mediaItem.GetValue<string>("umbracoFile").ToLowerInvariant();
            var mediaName = mediaItem.Name.ToLowerInvariant();

            // Check that the media item name is not the same as the file name
            if (fileName.EndsWith(mediaName, true, null))
            {
                ValidName = false;
            }

            // Check that filename does not end with a file extension
            // TODO: probably should be a config setting
            var extensionsList = new List<string> { ".jpg", ".png", ".gif", ".bmp", ".tif", ".tiff", ".jpeg", ".jif", ".jfif", ".pdf", ".pcd", ".jp2", ".jpx", ".j2k", ".j2c" };

            // Use contains just in case the image being uploaded already existed before this code was implemented.
            // If an image already exists when another is uploaded the filename containing the extension stays and a (1) is appended to the end of the file name.
            if (extensionsList.Any(f => mediaName.Contains(f)))
            {
                ValidName = false;
            }

            return ValidName;
        }
    }
}
