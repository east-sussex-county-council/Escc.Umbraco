# Control unpublishing dates for content

The unpublish overrides configuration allows you enforce all content nodes to unpublish after 6 months as a method of enforcing content review. This is enabled when the `UnpublishOverridesSection` is present in `web.config`, even if it is blank. 

You can prevent content nodes from having an unpublish date based on either their document type alias or URL. This is useful when you never want content for your site to disappear, like a home page.

	<configuration>
  		<configSections>
	  		<section name="UnpublishOverridesSection" type="Escc.Umbraco.UnpublishOverrides.UnpublishOverridesSection, Escc.Umbraco" />
   		</configSections>

		<UnpublishOverridesSection>
            <ContentTypes>
           		<add name="HomePage" level="*"/>
                <add name="HomePageItems" level="*"/>
				<add name="ContentPage" level="2"/>
            </ContentTypes>
            <Paths>
                <add name="/about-this-site/" children="*"/>
                <add name="/banners/" children=""/>
            </Paths>
		</UnpublishOverridesSection>
	</configuration>

## Prevent content from unpublishing based on its document type

You can prevent content from having an unpublish date based on its document type and level in the content tree.

Using `add name="HomePage" level="*"` would prevent a page with a document type alias of `HomePage` at any level in the content tree from having an unpublish date.

Using `add name="ContentPage" level="2"` would prevent pages with a document type alias of `ContentPage` that are at level 2 of the content tree from having an unpublish date.

The document type alias in the `name` attribute is case sensitive. The example below would configure overrides for two different document types:

	<UnpublishOverridesSection>
        <ContentTypes>
        	<add name="HomePage" level="*"/>
			<add name="homepage" level="*"/>
        </ContentTypes>
	</UnpublishOverridesSection>

## Prevent content from unpublishing based on its URL

It is also possible to prevent content from having an unpublish date based on its URL. This is useful when you have an area of a site which should never be unpublished and that has a mixture of document types, and the same document types should have an unpublish date when used elsewhere.

	<UnpublishOverridesSection>
        <Paths>
            <add name="/about-this-site/" children="*"/>
            <add name="/banners/" children=""/>
        </Paths>
	</UnpublishOverridesSection>

The `children=` attribute works similarly to the `level` attribute. If you have `children="*"` then the override also applies to all children under that path, whereas if it is left blank (eg `children=""`) then the override only applies to the single page at the specified path.

## Unpublishing dates API

By default the above policies are applied by `UnpublishAtEventHandler` when a content node is published, which means that when you update the configuration existing content may not match the new configuration until it is next republished. An API allows you to revisit existing content and ensure it complies with the current configuration. 

You can call the API with a `POST` request to the following URL. This request must be authenticated using the authentication method configured for web APIs on the consuming site.

	https://hostname/umbraco/api/UnpublishOverridesApi/EnsureUnpublishDatesMatchPolicy/