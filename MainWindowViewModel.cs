using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WpfEmlViewerExample.Services;

namespace WpfEmlViewerExample
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private readonly AppSettings _settings;

        public string[]? _linkPatterns;
        public string[]? LinkPatterns
        {
            get { return _linkPatterns; }
            set { _linkPatterns = value; }
        }

        public string? _source;
        public string? Source {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
                NotifyPropertyChanged();
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

            //using (var fs = File.OpenRead(_settings.EmlFile!)) {
            //    EmlContent = extractor.Extract(fs);
            //    Source = EmlContent.HtmlUri;
            //}
            Source = _settings.EmlFile;

            LinkPatterns = _settings.LinkPatterns;
        }
    }
}
