# Require alternative text for images

Umbraco does not by default require images to have alternative text, yet this is a basic requirement for accessibility. The logical place for the alternative text to come from is the image's `Name` property in Umbraco, but by default this contains the filename, so by requiring that the names of media items do not contain an image file extension we can enforce the `Name` to be updated before an image is used.

Property editors are responsible for allowing media to be added to a page, so to track down the media used on a page, this project defines `IMediaIdProvider` and implements it for common property editors. 

These can be loaded from `web.config` using:

    var providers = new MediaIdProvidersFromConfig(ApplicationContext.Current.Services.MediaService, ApplicationContext.Current.Services.DataTypeService).LoadProviders();

A typical default `web.config` for this would be:

	<configuration>

	  <configSections>
	    <sectionGroup name="Escc.Umbraco">
	      <section name="MediaIdProviders" type="System.Configuration.NameValueSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
		</sectionGroup>
   	  </configSections>

	  <Escc.Umbraco>
		<MediaIdProviders>
		  <add key="MediaPickerIdProvider" value="Umbraco.MediaPicker,Umbraco.MultipleMediaPicker,Umbraco.MultiNodeTreePicker" />
		  <add key="GridHtmlMediaIdProvider" value="Umbraco.Grid" />
		  <add key="HtmlMediaIdProvider" value="Umbraco.TinyMCEv3" />
		  <add key="RelatedLinksIdProvider" value="Umbraco.RelatedLinks" />
		  <add key="UrlMediaIdProvider" value="Escc.Umbraco.PropertyEditors.UrlPropertyEditor" />
		</MediaIdProviders>
	  </Escc.Umbraco>

	</configuration>
