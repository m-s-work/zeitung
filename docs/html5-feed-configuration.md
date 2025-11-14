# HTML5 Feed Parser Configuration Guide

The Zeitung Worker supports parsing articles from HTML pages using CSS selectors, in addition to standard RSS/Atom and RDF feeds.

## Supported Feed Types

- **rss** (default): Standard RSS 2.0 and Atom feeds
- **rdf**: RSS 1.0/RDF feeds
- **html5**: HTML pages with custom CSS selector configuration

## HTML5 Feed Configuration

HTML5 feeds require a `HtmlConfig` object that defines CSS selectors for extracting article data from HTML pages. This configuration is similar to [html2rss](https://github.com/html2rss/html2rss) format.

### Basic Structure

```json
{
  "Name": "Site Name",
  "Url": "https://example.com/news",
  "Type": "html5",
  "HtmlConfig": {
    "ItemsSelector": "article.news-item",
    "Title": {
      "Selector": "h2 a",
      "Extractor": "text"
    },
    "Link": {
      "Selector": "h2 a",
      "Extractor": "href"
    },
    "Description": {
      "Selector": "p.summary",
      "Extractor": "text"
    },
    "PublishedAt": {
      "Selector": "time",
      "Extractor": "datetime"
    },
    "Category": {
      "Selector": "span.tag",
      "Extractor": "text"
    }
  }
}
```

### Configuration Fields

#### ItemsSelector (required)
CSS selector that identifies article containers on the page. Each matching element will be parsed as a separate article.

Example: `"article.post"`, `"div.news-item"`, `".content article"`

#### Title (required)
Configuration for extracting the article title.

#### Link (required)
Configuration for extracting the article URL. Relative URLs are automatically converted to absolute URLs based on the feed URL.

#### Description (optional)
Configuration for extracting the article summary/description.

#### PublishedAt (optional)
Configuration for extracting the publication date. The parser will attempt to parse various date formats.

#### Category (optional)
Configuration for extracting article categories. Can match multiple elements.

### Selector Configuration

Each field uses a `SelectorConfig` object:

```json
{
  "Selector": "CSS selector",
  "Extractor": "extraction method"
}
```

#### Available Extractors

- **text** (default): Extracts the text content of the element
- **href**: Extracts the `href` attribute (for links)
- **src**: Extracts the `src` attribute (for images)
- **datetime**: Extracts the `datetime` attribute or text content (for time elements)
- **attribute**: Extracts a custom attribute (requires `Attribute` field)

#### Custom Attribute Extraction

```json
{
  "Selector": "img.thumbnail",
  "Extractor": "attribute",
  "Attribute": "data-src"
}
```

## Example Configurations

### DerBrutkasten (Austrian Tech News)

```json
{
  "Name": "DerBrutkasten",
  "Url": "https://brutkasten.com/news/newsticker",
  "Description": "Austrian Startup & Tech News",
  "Type": "html5",
  "HtmlConfig": {
    "ItemsSelector": "article.newsticker-item",
    "Title": {
      "Selector": "h3.title a",
      "Extractor": "text"
    },
    "Link": {
      "Selector": "h3.title a",
      "Extractor": "href"
    },
    "Description": {
      "Selector": "div.teaser",
      "Extractor": "text"
    },
    "PublishedAt": {
      "Selector": "time",
      "Extractor": "datetime"
    },
    "Category": {
      "Selector": "span.category",
      "Extractor": "text"
    }
  }
}
```

### Ingenieur.de (German Engineering News)

```json
{
  "Name": "Ingenieur.de Bau",
  "Url": "https://www.ingenieur.de/technik/fachbereiche/bau/",
  "Description": "Engineering & Construction News (German)",
  "Type": "html5",
  "HtmlConfig": {
    "ItemsSelector": "article.article-teaser",
    "Title": {
      "Selector": "h2.headline a",
      "Extractor": "text"
    },
    "Link": {
      "Selector": "h2.headline a",
      "Extractor": "href"
    },
    "Description": {
      "Selector": "p.intro",
      "Extractor": "text"
    },
    "PublishedAt": {
      "Selector": "time",
      "Extractor": "datetime"
    },
    "Category": {
      "Selector": ".meta span.tag",
      "Extractor": "text"
    }
  }
}
```

## Adding HTML5 Feeds

1. Add the feed configuration to `appsettings.json` under the `RssFeeds` array
2. Set `Type` to `"html5"`
3. Define the `HtmlConfig` with appropriate CSS selectors
4. Test the configuration by inspecting the target website's HTML structure

## Tips for Creating Configurations

1. **Inspect the HTML**: Use browser developer tools to examine the page structure
2. **Test Selectors**: Use the browser console to test CSS selectors: `document.querySelectorAll('your-selector')`
3. **Start Simple**: Begin with basic selectors and refine as needed
4. **Handle Changes**: Websites may change their HTML structure; configurations may need updates
5. **Multiple Categories**: The `Category` selector can match multiple elements per article

## CSS Selector Reference

Common CSS selector patterns:

- `.class` - Elements with a specific class
- `#id` - Element with a specific ID
- `element` - Elements by tag name
- `parent child` - Descendant selector
- `parent > child` - Direct child selector
- `[attribute]` - Elements with an attribute
- `[attribute="value"]` - Elements with a specific attribute value
- `.class1.class2` - Elements with multiple classes
- `selector1, selector2` - Multiple selectors

For more information on CSS selectors, see [MDN CSS Selectors](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Selectors).

## Troubleshooting

### No Articles Found
- Verify the `ItemsSelector` matches article containers
- Check if the page requires JavaScript (HTML5 parser only works with static HTML)
- Inspect the actual HTML source (View Page Source) not the rendered DOM

### Missing Data
- Ensure selectors are scoped to individual article elements
- Check if the data is in a different attribute (use appropriate extractor)
- Verify relative URLs are being resolved correctly

### Date Parsing Issues
- The parser attempts to handle various date formats
- Prefer `datetime` attribute on `<time>` elements when available
- Check browser console for date parsing warnings

## Validation

Run the Worker in one-off mode to test configurations:

```bash
dotnet run --project Zeitung.Worker -- --run-once
```

Check logs for parsing results and any errors.
