# Add tracking data for internal links

If you select content or media in the back office, Umbraco saves the node id to help it track the target page - for example, updating the link if it's moved.

If you simply paste a URL it doesn't do that, so `TrackInternalLinksEventHandler` looks up those pasted internal links and converts them to the tracked format. Links to content nodes are looked up in the content cache (`umbraco.config`) and links to media nodes are looked up in the built-in `Internal` Examine index.

`TrackInternalLinksEventHandler` is enabled automatically when you install this project as a NuGet package.