HTTP Caching
============

The `HttpCachingService` lets you set HTTP caching headers from your controller. You can set a default time to cache every page, and use date properties to manage parts of a page with their own expiry dates.

An example:

	var expiryProperty = new ExpiryDateFromPropertyValue(model.Content, "someExpiryDatePropertyAlias");
	var defaultTimeToCache = new TimeSpan(1,0,0,0);

    HttpCachingService.SetHttpCacheHeadersFromUmbracoContent(model.Content, UmbracoContext.Current.InPreviewMode, Response.Cache, new IExpiryDateSource[] { expiryProperty }, defaultTimeToCache);

It needs to check the expiry date of the page, which is not available as a property of an `IPublishedContent`, so you will need another implementation of `IExpiryDateSource`. You should use a solution such as Examine to avoid doing this by hitting the Umbraco database on every page view - see the [Escc.Umbraco.Expiry](https://github.com/east-sussex-county-council/Escc.Umbraco.Expiry) project for an implementation you can use.

You can also add a `Dropdown` property with the alias `cache`. The `HttpCachingService` looks for hard-coded values of `5 minutes`, `10 minutes`, `30 minutes` or `1 hour` to allow any page to override the default cache time with its own setting. This is useful for pages which are particularly time-sensitive.

You can use the following configuration setting to disable caching, which can be useful for development environments where you always need the latest content.

	<configuration>

	  <configSections>
	    <sectionGroup name="Escc.Umbraco">
	      <section name="GeneralSettings" type="System.Configuration.NameValueSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
	    </sectionGroup>
	  </configSections>

	  <Escc.Umbraco>
	    <GeneralSettings>
	      <add key="HttpCachingEnabled" value="false" />
	    </GeneralSettings>
	  </Escc.Umbraco>

	</configuration>
