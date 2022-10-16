using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WpfEmlViewerExample.Services;

namespace WpfEmlViewerExample
{
    public class MainWindowViewModel
    {
        private readonly AppSettings _settings;

        public string? _source;
        public string? Source {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
            }
        }

        public EmlContent? EmlContent
        {
            get; set;
        }

        public MainWindowViewModel(
            IOptions<AppSettings> settings,
            IConfiguration configuration,
            ILogger<MainWindowViewModel> logger,
            EmlExtractorService extractor)
        {
            _settings = settings.Value;
            logger.LogInformation($"new {nameof(MainWindowViewModel)}");

            using (var fs = File.OpenRead(_settings.EmlFile!)) {
                EmlContent = extractor.Extract(fs);
                Source = EmlContent.HtmlUri;
            }

        }
    }
}
