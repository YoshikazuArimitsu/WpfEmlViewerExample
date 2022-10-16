using AngleSharp.Html;
using AngleSharp.Html.Parser;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfEmlViewerExample.Services
{
    public class EmlContent : IDisposable
    {
        public string BaseDir { get; set; }
        public string HtmlUri { get; set; }

        public EmlContent()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            BaseDir = tempDirectory;
            HtmlUri = new System.Uri(Path.Combine(BaseDir, "index.html")).AbsoluteUri;
        }

        public static void RecursiveDelete(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists)
                return;

            foreach (var dir in baseDir.EnumerateDirectories())
            {
                RecursiveDelete(dir);
            }
            var files = baseDir.GetFiles();
            foreach (var file in files)
            {
                file.IsReadOnly = false;
                file.Delete();
            }
            baseDir.Delete();
        }

        public void Dispose()
        {
            RecursiveDelete(new DirectoryInfo(BaseDir));
        }
    }

    public class EmlExtractorService
    {
        public EmlExtractorService(
            ILogger<EmlExtractorService> logger
            )
        {

        }

        public EmlContent Extract(Stream stream)
        {
            var content = new EmlContent();
            var mimeMessage = MimeMessage.Load(stream);

            var htmlStream = new MemoryStream();
            var cidDictionary = new Dictionary<string, string>();

            // ディレクトリ下に保存
            foreach (var part in mimeMessage.BodyParts)
            {
                var mimePart = part as MimePart;
                var textPart = part as TextPart;

                if (mimePart != null)
                {
                    if (textPart != null)
                    {
                        if (textPart.IsHtml)
                        {
                            textPart.Content.DecodeTo(htmlStream);
                        }

                    }
                    else
                    {
                        var savePath = Path.Combine(content.BaseDir, mimePart.FileName);

                        using (var s = File.Create(savePath))
                            mimePart.Content.DecodeTo(s);

                        if (mimePart.ContentId != null)
                        {
                            cidDictionary.Add(mimePart.ContentId, mimePart.FileName);
                        }

                    }
                }
            }

            // cidイメージ対応
            {
                htmlStream.Seek(0, SeekOrigin.Begin);
                var parser = new HtmlParser();
                var doc = parser.ParseDocument(htmlStream);

                var imgNodes = doc.QuerySelectorAll("img");
                foreach (var imgNode in imgNodes)
                {
                    var imgSrc = imgNode.GetAttribute("src");
                    if (imgSrc != null && imgSrc.StartsWith("cid:"))
                    {
                        // cid:* をファイル名に置換
                        var cid = imgSrc.Substring(4);
                        string? fileName;
                        if (cidDictionary.TryGetValue(cid, out fileName))
                        {
                            imgNode.SetAttribute("src", fileName);
                        }
                    }
                }

                using (StreamWriter outputFile = new StreamWriter(Path.Combine(content.BaseDir, "index.html")))
                {
                    doc.ToHtml(outputFile, new PrettyMarkupFormatter());
                }
            }

            return content;
        }
    }
}
