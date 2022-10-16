using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WpfEmlViewerExample
{
    public class MainWindowViewModel
    {
        private readonly AppSettings _mySettings;
        public string Source => "https://www.google.com";

        public MainWindowViewModel(IOptions<AppSettings> mySettings, IConfiguration configuration,
            ILogger<MainWindowViewModel> logger)
        {
            _mySettings = mySettings.Value;
            logger.LogInformation($"new {nameof(MainWindowViewModel)}");
        }

    }
}
