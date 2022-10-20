using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfEmlViewerExample
{
    public class LinkClickedEventArgs : RoutedEventArgs
    {
        private readonly string _href;

        public string Href
        {
            get { return _href; }
        }

        public LinkClickedEventArgs(RoutedEvent routedEvent, string href) : base(routedEvent) {
            _href = href;
        }
    }

    /// <summary>
    /// WebViewに埋め込むホステッドオブジェクト
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class EmlViewHostedObject
    {
        public EmlView? EmlView { get; set; }
        public void HostCallback(string strText)
        {
            EmlView?.HostCallback(strText);
        }
    }

    /// <summary>
    /// EmlView.xaml の相互作用ロジック
    /// </summary>
    public partial class EmlView : UserControl
    {
        private const string EMBED_SCRIPT = @"
// 指定文字列を含む Node の検索
const getTextNodes = (pattern) => {
    const xPathResult = document.evaluate(
        pattern ? `//text()[contains(., ""${pattern}"")]` : '//text()',
        document,
        null,
        XPathResult.ORDERED_NODE_SNAPSHOT_TYPE,
        null
    );
    const result = [];
    let { snapshotLength } = xPathResult;

    while (snapshotLength--)
    {
        result.unshift(xPathResult.snapshotItem(snapshotLength));
    }

    return result;
};

// C#側コールバック呼び出し
const hostCallback = (arg) => {
    chrome.webview.hostObjects.class.HostCallback(arg);
}

// TextNode -> AnchorNode 置き換え
// 対象文字列を**含む** テキストノードの対象文字列部分のみをリンクに置き換える為、
// TextNode -> Span要素 ([対象文字列より前の部分], [対象文字列], [対象文字列より後の部分]) への
// 置き換えを行う。
// モノによってはレイアウトが崩れるかもしれない。
const replaceTextNode = (n, p) => {
    const targetNode = document.createElement('span');

    const splitContent = n.textContent.split(p);

    const beforeNode = n.cloneNode();
    beforeNode.textContent = splitContent[0];

    const anchorNode = document.createElement('a');
    anchorNode.href = `javascript:hostCallback('${p}');`;
    anchorNode.textContent = p;

    const afterNode = n.cloneNode();
    afterNode.textContent = splitContent[1];

    targetNode.appendChild(beforeNode);
    targetNode.appendChild(anchorNode);
    targetNode.appendChild(afterNode);

    n.replaceWith(targetNode);
}

// 指定文字列を含むテキストノードを <a～ に差し替える
const replaceHostCallbackNode = (pattern) => {
    const results = getTextNodes(pattern)

    results.forEach( (r) => {
        replaceTextNode(r, pattern);
    });
}

// dom構築後、パターンを C# 側呼び出しのリンクに置換
document.addEventListener('DOMContentLoaded', () => {
    callbackPatterns.forEach((p) => replaceHostCallbackNode(p));
});
";

        private Task<string>? _embedScriptId;
        private EmlViewHostedObject _hostedObject;

        /// <summary>
        /// 開くURL
        /// </summary>
        public string Source
        {
            get
            {
                return (string)GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(EmlView), new PropertyMetadata(null));

        /// <summary>
        /// リンクに置換する対象
        /// </summary>
        public string[] Patterns
        {
            get
            {
                return (string[])GetValue(PatternProperty);
            }
            set
            {
                SetValue(PatternProperty, value);
            }
        }

        public static readonly DependencyProperty PatternProperty =
            DependencyProperty.Register("Patterns", typeof(string[]), typeof(EmlView), new PropertyMetadata(new string[0]));

        /// <summary>
        /// 置換リンクを踏んだ時のイベント
        /// </summary>
        public static readonly RoutedEvent LinkClickedEvent = EventManager.RegisterRoutedEvent(
            name: "LinkClicked",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(EmlView));

        public event RoutedEventHandler LinkClicked
        {
            add { AddHandler(LinkClickedEvent, value); }
            remove { RemoveHandler(LinkClickedEvent, value); }
        }

        public EmlView()
        {
            InitializeComponent();

            _hostedObject = new EmlViewHostedObject()
            {
                EmlView = this
            };

            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
        }


        private void updateEmbedScript()
        {
            if(_embedScriptId != null)
            {
                webView.CoreWebView2.RemoveScriptToExecuteOnDocumentCreated(_embedScriptId.Result);
            }

            var initLine = $"const callbackPatterns = ['{string.Join("','", Patterns)}'];\r\n";
            var script = initLine + EMBED_SCRIPT;

            _embedScriptId = webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
        }

        private void WebView_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            webView.CoreWebView2.AddHostObjectToScript("class", _hostedObject);
            webView.EnsureCoreWebView2Async();
            updateEmbedScript();
        }

        /// <summary>
        /// DataContext変更ハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// https://github.com/MicrosoftEdge/WebView2Feedback/issues/1136#issuecomment-1255470804
        /// 上記記事で DataContextChanged を捕まえて中 WebView2 の DataContextごと null にすると
        /// 問題を回避できるというコメントに従い実装
        /// </remarks>
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                webView.DataContext = null;
            }
        }

        public void HostCallback(string arg)
        {
            var eventArgs = new LinkClickedEventArgs(LinkClickedEvent, arg);
            RaiseEvent(eventArgs);
        }
    }
}
