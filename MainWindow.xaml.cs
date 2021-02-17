using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace EasyMiner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public XMRig miner = new XMRig();
        XMRigConfig conf = new XMRigConfig();
        PoolConnector pool = new PoolConnector();
        DispatcherTimer timer;
        byte selectedScreen = 0;
        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += tick;
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Start();

            StopMining.Visibility = Visibility.Hidden;

            selectedScreen = 0;
            mineGrid.Visibility = Visibility.Visible;
            statsGrid.Visibility = Visibility.Hidden;
            helpGrid.Visibility = Visibility.Hidden;

            conf.host = "de.uplexa.online:1111";
            conf.algo = "cn-extremelite/upx2";

            //DEBUG
            //conf.user = "UPX1rv5G6GW1N1Nv9tLQaBZrUDo2g5uTVH9rmiYVWMNHgyuRRaTBBy36d9LmYawvCvR71NUTTAD3MBY8pVKnP7c5AghMbHFsrR";
            //DEBUG

        }

        public string FormatMinerHashrate(int hashrate)
        {
            if (hashrate == 0)
            {
                if (miner.proc == null)
                {
                    return "Not mining";
                }
                else
                {
                    return "Waiting...";
                }
            }
            return FormatHashrate(hashrate);
            
        }

        public string FormatHashrate(long hashrate)
        {
            if (hashrate < 1000)
            {
                return hashrate + " H/s";
            }
            else if (hashrate < 1000 * 1000)
            {
                return (hashrate / 1000F).ToString("0.00") + " kH/s";
            }
            else
            {
                return (hashrate / 1000000F).ToString("0.00") + " MH/s";
            }
        }

        private void startMining_Click(object sender, RoutedEventArgs e)
        {
            if(addressBox.Text == "Enter you uPlexa Address Here" || addressBox.Text == "")
            {
                MessageBox.Show("Please set your uPlexa address first");
                return;
            }
            saveSettings();
            miner.StartMining(conf);
            StopMining.Visibility = Visibility.Visible;
            startMining.Visibility = Visibility.Hidden;
        }

        public void stopMining_Click(object sender, RoutedEventArgs e)
        {
            miner.StopMining();
            StopMining.Visibility = Visibility.Hidden;
            startMining.Visibility = Visibility.Visible;
        }

        public string FormatTime(long mil)
        {
            double sec = mil / 1000;
            int hours = Convert.ToInt32(sec / 3600);
            int minutes = Convert.ToInt32(sec % 3600 / 60);
            string h = "";
            string m = "";
            if (hours != 0)
            {
                h = hours + "h ";
            }
            if(minutes != 0)
            {
                m = minutes + "m ";
            }
            return (h + m).Trim();
        }

        private void tick(object sender, EventArgs e)
        {
            PoolStats s = new PoolStats();
            if(selectedScreen == 1)
            {
                s = pool.GetPoolStats();
                network.Content = FormatHashrate(s.networkHashrate);
                lastBlock.Content = FormatTime(DateTimeOffset.Now.ToUnixTimeMilliseconds() - s.lastBlockFound);
            }
            if(miner.proc != null)
            {
                hr.Content = FormatMinerHashrate(miner.stats.hashrateCurrent);
                hr60.Content = FormatMinerHashrate(miner.stats.hashrate60s);
                hr15.Content = FormatMinerHashrate(miner.stats.hashrate15m);
                acceptedShares.Content = miner.stats.acceptedShares;
                refusedShares.Content = miner.stats.invalidShares;
                diff.Content = miner.stats.difficulty;
            }
            else
            {
                hr.Content = "Not Mining";
                hr60.Content = "Not Mining";
                hr15.Content = "Not Mining";
                acceptedShares.Content = "Not Mining";
                refusedShares.Content = "Not Mining";
                diff.Content = "Not Mining";
            }

        }

        private void onClosing(object sender, EventArgs e)
        {
            miner.StopMining();
        }

        private void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            const Visibility c = Visibility.Collapsed;
            const Visibility v = Visibility.Visible;
            if(toggleButton.IsChecked == true)
            {
                tt_mine.Visibility = c;
                tt_stats.Visibility = c;
                tt_help.Visibility = c;
                tt_exit.Visibility = c;
            }
            else
            {
                tt_mine.Visibility = v;
                tt_stats.Visibility = v;
                tt_help.Visibility = v;
                tt_exit.Visibility = v;
            }
        }

        private void ExitButton()
        {
            onClosing(null, null);
            System.Windows.Application.Current.Shutdown();
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string df = (string)((ToolTip)((ListViewItem)listView.SelectedItem).ToolTip).Content;
            switch(df)
            {
                case "Mine":
                    selectedScreen = 0;
                    mineGrid.Visibility = Visibility.Visible;
                    statsGrid.Visibility = Visibility.Hidden;
                    helpGrid.Visibility = Visibility.Hidden;
                    break;
                case "Stats":
                    selectedScreen = 1;
                    mineGrid.Visibility = Visibility.Hidden;
                    statsGrid.Visibility = Visibility.Visible;
                    helpGrid.Visibility = Visibility.Hidden;
                    tick(null,null);
                    break;
                case "Help":
                    selectedScreen = 2;
                    mineGrid.Visibility = Visibility.Hidden;
                    statsGrid.Visibility = Visibility.Hidden;
                    helpGrid.Visibility = Visibility.Visible;
                    break;
                case "Exit":
                    ExitButton();
                    break;
            }
        }

        private void saveSettings()
        {
            conf.user = addressBox.Text;
        }

        private void showLog_Click(object sender, RoutedEventArgs e)
        {
            xmrigOutput _xmrigOutput = new xmrigOutput();
            _xmrigOutput.textblock.Text = miner.minerOutput;
            _xmrigOutput.Show();
        }

        public string getXmrigOutput()
        {
            return miner.minerOutput;
        }

        private void addressBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(addressBox.Text == "Enter you uPlexa Address Here") addressBox.Text = "";
        }
    }
}
