﻿using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Escc.Umbraco
{
    /// <summary>
    /// Read target URLs from a combination of two fields on an Umbraco page
    /// </summary>
    public class TargetUrlReader : ITargetUrlReader
    {
        /// <summary>
        /// Read target URLs from a combination of two fields on an Umbraco page
        /// </summary>
        /// <param name="content">The Umbraco page.</param>
        /// <param name="multiNodeTreePickerAlias">The alias of a multi node tree picker field.</param>
        /// <param name="textboxAlias">The alias of a textbox field.</param>
        /// <param name="relativeOrAbsolute">Specify whether relative or absolute URLs are required.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">content</exception>
        public IEnumerable<Uri> GetTargetUrls(IPublishedContent content, string multiNodeTreePickerAlias, string textboxAlias, UriKind relativeOrAbsolute = UriKind.Relative)
        {
            if (content == null) throw new ArgumentNullException("content");

            var targetUrls = new List<Uri>();

            GetUrlsFromMultiNodeTreePicker(content, multiNodeTreePickerAlias, targetUrls, relativeOrAbsolute);
            GetUrlsFromTextField(content, textboxAlias, targetUrls, relativeOrAbsolute);

            return targetUrls;
        }

        private static void GetUrlsFromMultiNodeTreePicker(IPublishedContent content, string propertyAlias, List<Uri> targetUrls, UriKind relativeOrAbsolute)
        {
            if (String.IsNullOrEmpty(propertyAlias)) return;

            var multiNodeTreePicker = content.GetPropertyValue<IEnumerable<IPublishedContent>>(propertyAlias);
            if (multiNodeTreePicker != null)
            {
                foreach (var targetUrl in multiNodeTreePicker)
                {
                    try
                    {
                        if (relativeOrAbsolute == UriKind.Relative)
                        {
                            targetUrls.Add(new Uri(targetUrl.Url, UriKind.Relative));
                        }
                        else
                        {
                            targetUrls.Add(new Uri(targetUrl.UrlWithDomain(), UriKind.Absolute));
                        }
                    }
                    catch (UriFormatException)
                    {
                    }
                }
            }
        }

        private static void GetUrlsFromTextField(IPublishedContent content, string propertyAlias, List<Uri> targetUrls, UriKind relativeOrAbsolute)
        {
            if (String.IsNullOrEmpty(propertyAlias)) return;

            var urlList = content.GetPropertyValue<string>(propertyAlias);
            if (!String.IsNullOrWhiteSpace(urlList))
            {
                var urlLines = urlList.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var urlLine in urlLines)
                {
                    try
                    {
                        var url = new Uri(urlLine, UriKind.RelativeOrAbsolute);

                        if (relativeOrAbsolute == UriKind.Relative)
                        {
                            if (url.IsAbsoluteUri) url = new Uri(url.PathAndQuery, UriKind.Relative);
                        }
                        else if (relativeOrAbsolute == UriKind.Absolute)
                        {
                            if (!url.IsAbsoluteUri) url = null;
                        }

                        if (url != null) targetUrls.Add(url);
                    }
                    catch (UriFormatException)
                    {
                    }
                }
            }
        }   
    }
}
