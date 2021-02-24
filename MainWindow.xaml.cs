using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        Thread HttpThread;
        PoolStats poolStats;
        bool poolStatsContainsUserdata = false;
        bool exiting = false;
        byte selectedScreen = 0;
        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += tick;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();

            StopMining.Visibility = Visibility.Hidden;

            selectedScreen = 0;
            mineGrid.Visibility = Visibility.Visible;
            statsGrid.Visibility = Visibility.Hidden;
            helpGrid.Visibility = Visibility.Hidden;

            conf.host = "de.uplexa.online:1111";
            conf.algo = "cn-extremelite/upx2";

            try
            {
                if (File.Exists(System.IO.Path.GetTempPath() + @"\easyminer\addr"))
                {
                    string save = File.ReadAllLines(System.IO.Path.GetTempPath() + @"\easyminer\addr")[0].Trim();
                    if (save.Length == 98 && save.Substring(0, 4) == "UPX1")
                    {
                        addressBox.Text = save;
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("Error reading saved data: " + e.Message);
            }
            

            HttpThread = new Thread(new ThreadStart(httpThread));
            HttpThread.Start();
        }

        private void httpThread()
        {
            try
            {
                while (!exiting)
                {
                    if (verifyAddr(true))
                    {
                        poolStats = pool.GetPoolStats(conf.user);
                        poolStatsContainsUserdata = true;
                    }
                    else
                    {
                        poolStats = pool.GetPoolStats();
                        poolStatsContainsUserdata = false;
                    }
                    System.Threading.Thread.Sleep(5000);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("Error updating pool stats: " + e.Message);
            }
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
            try
            {
                if (!verifyAddr()) return;
                File.WriteAllText(System.IO.Path.GetTempPath() + @"\easyminer\addr", conf.user);
                miner.StartMining(conf);
                StopMining.Visibility = Visibility.Visible;
                startMining.Visibility = Visibility.Hidden;
            }
            catch (Exception er)
            {
                MessageBox.Show("Error: " + er.Message);
            }
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
            try
            {
                switch (selectedScreen)
                {
                    case 0:
                        if (miner.proc != null)
                        {
                            hr.Content = FormatMinerHashrate(miner.stats.hashrateCurrent);
                            hr60.Content = FormatMinerHashrate(miner.stats.hashrate60s);
                            hr15.Content = FormatMinerHashrate(miner.stats.hashrate15m);
                            acceptedShares.Content = miner.stats.acceptedShares;
                            diff.Content = miner.stats.difficulty;

                            if (miner.stats.hashrateCurrent != 0)
                            {
                                long networkHashrate = poolStats.avgdiff / poolStats.coinDiffTarget;
                                float share = networkHashrate / miner.stats.hashrateCurrent;
                                int dailyBlocks = 86400 / poolStats.coinDiffTarget;
                                int blockReward = poolStats.lastReward / poolStats.denom;
                                int estEarn = Convert.ToInt32(blockReward * dailyBlocks / share);
                                earnings.Content = estEarn + " UPX";
                            }
                        }
                        else
                        {
                            hr.Content = "-";
                            hr60.Content = "-";
                            hr15.Content = "-";
                            acceptedShares.Content = "-";
                            earnings.Content = "-";
                            diff.Content = "-";
                        }
                        break;
                    case 1:
                        if (verifyAddr(true) && poolStatsContainsUserdata)
                        {
                            pendingBalance.Content = poolStats.pendingBalace + " UPX";
                            totalPaid.Content = poolStats.totalPaid + " UPX";
                            roundContrib.Content = poolStats.roundContrib.ToString("0.##") + "%";
                        }
                        network.Content = FormatHashrate(poolStats.networkHashrate);
                        lastBlock.Content = FormatTime(DateTimeOffset.Now.ToUnixTimeMilliseconds() - poolStats.lastBlockFound) + " ago";
                        effort.Content = Convert.ToByte(poolStats.currentEffort * 100) + "%";
                        break;
                }
            }
            catch(Exception err)
            {
                MessageBox.Show("Error: " + err.Message);
            }
        }

        private void onClosing(object sender, EventArgs e)
        {
            exiting = true;
            miner.StopMining();
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

        private bool verifyAddr(bool silent = false)
        {
            if (conf.user.Length != 98 || conf.user.Substring(0, 4) != "UPX1")
            {
                if(!silent) MessageBox.Show("Invalid uPlexa address!");
                return false;
            }
            return true;
        }

        private void showLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                xmrigOutput _xmrigOutput = new xmrigOutput();
                _xmrigOutput.textblock.Text = miner.minerOutput;
                _xmrigOutput.Show();
            }
            catch(Exception err)
            {
                MessageBox.Show("Error: " + err.Message);
            }
        }

        public string getXmrigOutput()
        {
            return miner.minerOutput;
        }

        private void addressBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(addressBox.Text == "Enter your uPlexa Address Here") addressBox.Text = "";
        }

        private void addressBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            conf.user = addressBox.Text;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ExitButton();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
