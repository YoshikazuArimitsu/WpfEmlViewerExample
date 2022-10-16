using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <summary>
    /// EmlView.xaml の相互作用ロジック
    /// </summary>
    public partial class EmlView : UserControl
    {
        public string Source
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(EmlView), new PropertyMetadata(null));

        public EmlView()
        {
            InitializeComponent();
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
    }
}
