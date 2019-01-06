using AngleSharp;
using AngleSharp.Extensions;
using AngleSharp.Network;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TsearchParser.Models.Application;
using TsearchParser.Models.Domain;

namespace TsearchParser.Services
{
    public class Scraper
    {
        private readonly ILogger<Scraper> _logger;
        private readonly CommandLineOptions _commandLineOptions;
        private readonly PlainTextMarkupFormatter _plainTextMarkupFormatter;

        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:64.0) Gecko/20100101 Firefox/64.0";

        public Scraper(ILogger<Scraper> logger, CommandLineOptions commandLineOptions, PlainTextMarkupFormatter plainTextMarkupFormatter)
        {
            _logger = logger;
            _commandLineOptions = commandLineOptions;
            _plainTextMarkupFormatter = plainTextMarkupFormatter;
        }

        private IBrowsingContext CreateBrowsingContext()
        {
            var browsingContextConfig = Configuration.Default.WithDefaultLoader().WithCss().WithLocaleBasedEncoding().WithCookies();
            var browsingContext = BrowsingContext.New(browsingContextConfig);
            return browsingContext;
        }

        private async Task<List<Article>> GetPageArticlesAsync(int offset, IBrowsingContext browsingContext, CancellationToken cancellationToken)
        {
            var documentRequest = new DocumentRequest(new Url("http://tsear.ch/posts/" + _commandLineOptions.Channel + "/" + offset))
            {
                Method = HttpMethod.Get,
            };
            documentRequest.Headers.TryAdd("User-Agent", UserAgent);
            var pageContent = await browsingContext.OpenAsync(documentRequest, cancellationToken);
            var nodes = pageContent.QuerySelectorAll("div.col-lg-12 > div.panel-flat");

            var items = new List<Article>();
            foreach (var node in nodes)
            {
                var dateTimeElement = node.QuerySelector("div.panel-heading > div.media-left > div.text-size-small");
                var textElement = node.QuerySelector("div.panel-body");

                if (dateTimeElement != null && textElement != null)
                {
                    items.Add(new Article
                    {
                        DateTime = dateTimeElement.ToHtml(_plainTextMarkupFormatter).Trim(),
                        Text = textElement.ToHtml(_plainTextMarkupFormatter).Trim(),
                    });
                }
            }
            _logger.LogInformation($"{items.Count} items found on {documentRequest.Target}, status code: {pageContent.StatusCode}");
            return items;
        }

        private async Task<List<Article>> GetAllArticlesAsync(IBrowsingContext browsingContext, CancellationToken cancellationToken)
        {
            var allItems = new List<Article>();
            List<Article> pageItems;

            const int limit = 10;
            var offset = 0;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                pageItems = await GetPageArticlesAsync(offset, browsingContext, cancellationToken);
                allItems.AddRange(pageItems);
                offset += limit;
            } while (pageItems.Count > 0);

            allItems.Reverse();
            return allItems;
        }

        public async Task<List<Article>> ScrapeAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("scraping started");
            var browsingContext = CreateBrowsingContext();
            var articles = await GetAllArticlesAsync(browsingContext, cancellationToken);
            _logger.LogInformation($"scraping finished ({articles.Count} partners total)");
            return articles;
        }
    }
}
