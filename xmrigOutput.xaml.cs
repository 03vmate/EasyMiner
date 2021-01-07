using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EasyMiner
{
    /// <summary>
    /// Interaction logic for xmrigOutput.xaml
    /// </summary>
    /// 
    public partial class xmrigOutput : Window
    {
        DispatcherTimer timer;
        public xmrigOutput()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += tick;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tick(object sender, EventArgs e)
        {
            MainWindow main = (MainWindow)Application.Current.MainWindow;
            if (main.miner.proc != null)
            {
                textblock.Text = main.getXmrigOutput();
                scrollv.ScrollToEnd();
            }
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textblock.Text);
        }
    }
}
