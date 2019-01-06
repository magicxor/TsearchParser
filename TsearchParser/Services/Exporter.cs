using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using TsearchParser.Models.Application;
using TsearchParser.Models.Domain;

namespace TsearchParser.Services
{
    public class Exporter
    {
        private readonly ILogger<Exporter> _logger;
        private readonly CommandLineOptions _commandLineOptions;

        public Exporter(ILogger<Exporter> logger, CommandLineOptions commandLineOptions)
        {
            _logger = logger;
            _commandLineOptions = commandLineOptions;
        }

        public void Export(IList<Article> items)
        {
            if (items.Count == 0)
            {
                _logger.LogInformation($"Export has been cancelled: items.Count == 0");
            }
            else
            {
                _logger.LogInformation($"Exporting {items.Count} items to {_commandLineOptions.Output}...");
                foreach (var item in items)
                {
                    File.AppendAllText(_commandLineOptions.Output, 
$@"{item.DateTime}
{item.Text}
-----------------
");
                }
                _logger.LogInformation($"Export has been successfully finished");
            }
        }
    }
}
