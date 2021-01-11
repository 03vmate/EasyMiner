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
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();

            int threads = Environment.ProcessorCount;
            ObservableCollection<string> labels = new ObservableCollection<string>();
            for(int i = 1; i <= threads; i++)
            {
                labels.Add(i.ToString());
            }
            threadsBox.ItemsSource = labels;
            threadsBox.Items.Refresh();
            threadsBox.SelectedValue = threads.ToString();

            StopMining.Visibility = Visibility.Hidden;

            selectedScreen = 3;
            mineGrid.Visibility = Visibility.Hidden;
            statsGrid.Visibility = Visibility.Hidden;
            helpGrid.Visibility = Visibility.Hidden;
            settingsGrid.Visibility = Visibility.Visible;

            conf.host = "mine.uplexa.online:1111";
            conf.algo = "cn-extremelite/upx2";

            //DEBUG
            conf.user = "UPX1rv5G6GW1N1Nv9tLQaBZrUDo2g5uTVH9rmiYVWMNHgyuRRaTBBy36d9LmYawvCvR71NUTTAD3MBY8pVKnP7c5AghMbHFsrR";
            //DEBUG

        }

        private void startMining_Click(object sender, RoutedEventArgs e)
        {
            if(conf.user == null)
            {
                MessageBox.Show("Please set your uPlexa address on the settings tab before mining");
                return;
            }
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

        private void tick(object sender, EventArgs e)
        {
            PoolStats s = new PoolStats();
            s = pool.GetPoolStats();

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
                tt_settings.Visibility = c;
                tt_exit.Visibility = c;
            }
            else
            {
                tt_mine.Visibility = v;
                tt_stats.Visibility = v;
                tt_help.Visibility = v;
                tt_settings.Visibility = v;
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
                    settingsGrid.Visibility = Visibility.Hidden;
                    break;
                case "Stats":
                    selectedScreen = 1;
                    mineGrid.Visibility = Visibility.Hidden;
                    statsGrid.Visibility = Visibility.Visible;
                    helpGrid.Visibility = Visibility.Hidden;
                    settingsGrid.Visibility = Visibility.Hidden;
                    break;
                case "Help":
                    selectedScreen = 2;
                    mineGrid.Visibility = Visibility.Hidden;
                    statsGrid.Visibility = Visibility.Hidden;
                    helpGrid.Visibility = Visibility.Visible;
                    settingsGrid.Visibility = Visibility.Hidden;
                    break;
                case "Settings":
                    selectedScreen = 3;
                    mineGrid.Visibility = Visibility.Hidden;
                    statsGrid.Visibility = Visibility.Hidden;
                    helpGrid.Visibility = Visibility.Hidden;
                    settingsGrid.Visibility = Visibility.Visible;

                    break;
                case "Exit":
                    ExitButton();
                    break;
            }
        }

        private void saveSettings_Click(object sender, RoutedEventArgs e)
        {
            conf.user = addressBox.Text;
            conf.pass = "@" + workerBox.Text;
            conf.threads = threadsBox.SelectedIndex + 1;

            ComboBoxItem selectedPriority = (ComboBoxItem)priorityBox.SelectedItem;
            if (selectedPriority.Content.ToString() == "Low") conf.priority = 1;
            else if (selectedPriority.Content.ToString() == "Normal") conf.priority = 2;
            else if (selectedPriority.Content.ToString() == "High") conf.priority = 4;
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
            if(addressBox.Text == "UPXxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx") addressBox.Text = "";
        }
    }
}
