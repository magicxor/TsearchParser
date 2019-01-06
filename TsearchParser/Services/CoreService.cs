using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TsearchParser.Services
{
    public class CoreService
    {
        private readonly ILogger<CoreService> _logger;
        private readonly Scraper _scraper;
        private readonly Exporter _exporter;

        public CoreService(ILogger<CoreService> logger, Scraper scraper, Exporter exporter)
        {
            _logger = logger;
            _scraper = scraper;
            _exporter = exporter;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var items = await _scraper.ScrapeAsync(cancellationToken);
                _exporter.Export(items);
            }
            catch (OperationCanceledException e)
            {
                _logger.LogError(e, "Operation canceled");
            }
        }
    }
}
