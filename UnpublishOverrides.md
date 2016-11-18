Override unpublishing dates for content
===================

The unpublish overrides configuration allows you to omit `ContentTypes` and paths to content from having an unpublish date. This is useful when you never want content for your site to disappear, like a homepage.

Example:	

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



You can manipulate overrides for `ContentTypes` by a `level` in the content tree.

Using `add name="HomePage" level="*"` would omit a page of type `HomePage` from any level in the content tree.
Whereas using `add name="ContentPage" level="2"` would omit pages of type `ContentPage` that are at level 2 of the content tree.

It should be noted that a `ContentTypes` name after `name=` is case sensitive. The example below would omit two different `ContentTypes`.

Example:

	<UnpublishOverridesSection>
            <ContentTypes>
                	<add name="HomePage" level="*"/>
					<add name="homepage" level="*"/>
            </ContentTypes>
	</UnpublishOverridesSection>

It is also possible to give content an override by using a URL path. This allows for content in specific areas of a site to be excluded from being unpublished without specifying the `ContentTypes` of those pages. This is useful when you have an area of a site that has a mixture of `ContentTypes` that you only want to keep forever in that specific area.

Example:

	<UnpublishOverridesSection>
            <Paths>
                    <add name="/about-this-site/" children="*"/>
                    <add name="/banners/" children=""/>
            </Paths>
	</UnpublishOverridesSection>

The `children=` field works almost the same as level. If you have `children="*"` then the override also applies to all children under that path. Whereas if it is left blank e.g. `children=""` then the override only applies to the top of the specified path.

