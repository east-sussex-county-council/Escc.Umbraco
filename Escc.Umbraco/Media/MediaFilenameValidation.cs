using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Events;
using Umbraco.Core.Models;

namespace Escc.Umbraco.Media
{
    public class MediaFilenameValidation
    {

        public static Tuple<bool, string> ValidMediaItem(IMedia mediaItem)
        {
            var Valid = true;
            var ErrorMessage = "";
            var fileName = "";

            try
            {
                // Get lowercase version of the actual filename and the containing Media item
                fileName = mediaItem.GetValue<string>("umbracoFile").ToLowerInvariant();
            }
            catch (Exception)
            {
                Valid = false;
                ErrorMessage = string.Format("The media item '{0}' doesn't contain a media file such as an image. You must add a file to the media item before it can be used.", mediaItem.Name);
            }
            if(fileName == "")
            {
                Valid = false;
                ErrorMessage = string.Format("The media item '{0}' doesn't contain a media file such as an image. You must add a file to the media item before it can be used.", mediaItem.Name);
            }

            var Result = new Tuple<bool, string>(Valid, ErrorMessage);
            return Result;
        }

        /// <summary>
        /// Check if the name of the Media item is valid
        /// </summary>
        /// <param name="mediaItem">
        /// Media item to validate
        /// </param>
        /// <returns>
        /// True if media item name is valid
        /// </returns>
        public static Tuple<bool,string> ValidMediaName(IMedia mediaItem)
        {
            var Valid = true;
            var ErrorMessage = "";
            var fileName = mediaItem.GetValue<string>("umbracoFile").ToLowerInvariant();
            var mediaName = "";

            mediaName = mediaItem.Name.ToLowerInvariant();
            // Check that the media item name is not the same as the file name
            if (fileName.EndsWith(mediaName, true, null))
            {
                Valid = false;
                ErrorMessage = string.Format("The media item '{0}' has the same name as its file. You need to change the title before you can use it, It needs to be a description of what the image shows. This makes the image accessible to people who can't see it.", mediaItem.Name);
            }

            var Result = new Tuple<bool, string>(Valid, ErrorMessage);
            return Result;
        }

        public static Tuple<bool, string> CheckMediaForFileExtensions(IMedia mediaItem)
        {
            var Valid = true;
            var ErrorMessage = "";
            var mediaName = mediaItem.Name.ToLowerInvariant();

            // Check that filename does not end with a file extension
            var extensionsList = new List<string> { ".jpg", ".png", ".gif", ".bmp", ".tif", ".tiff", ".jpeg", ".jif", ".jfif", ".pdf", ".pcd", ".jp2", ".jpx", ".j2k", ".j2c", ".svg" };

            // Use contains just in case the image being uploaded already existed before this code was implemented.
            // If an image already exists when another is uploaded the filename containing the extension stays and a (1) or (2) etc..  is appended to the end of the file name.
            if (extensionsList.Any(f => mediaName.Contains(f)))
            {
                Valid = false;
                ErrorMessage = string.Format("The media item '{0}' contains a file extension in its name. You need to change the title before you can use it, It needs to be a description of what the image shows. This makes the image accessible to people who can't see it.", mediaItem.Name);
            }

            var Result = new Tuple<bool, string>(Valid, ErrorMessage);
            return Result;
        }

        public static Tuple<bool, string> CheckMediaForImage(IMedia mediaItem)
        {
            var Valid = true;
            var ErrorMessage = "";
            var fileName = mediaItem.GetValue<string>("umbracoFile").ToLowerInvariant();

            // Check that filename does not end with a file extension
            var extensionsList = new List<string> { ".jpg", ".png", ".gif", ".bmp", ".tif", ".tiff", ".jpeg", ".jif", ".jfif", ".pdf", ".pcd", ".jp2", ".jpx", ".j2k", ".j2c", ".svg" };

            // Use contains just in case the image being uploaded already existed before this code was implemented.
            // If an image already exists when another is uploaded the filename containing the extension stays and a (1) or (2) etc.. is appended to the end of the file name.
            if (!extensionsList.Any(f => fileName.Contains(f)))
            {
                Valid = false;
                ErrorMessage = string.Format("The media item '{0}' doesn't appear to be an image. You will need to change the media item to an image before it can be used. If you didn't intend to upload as an image then try deleting the media and uploading as a file. ", mediaItem.Name);
            }

            var Result = new Tuple<bool, string>(Valid, ErrorMessage);
            return Result;
        }
    }

   
}

