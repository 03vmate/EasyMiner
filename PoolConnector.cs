using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace EasyMiner
{
    struct PoolStats
    {
        //Pool stats
        public int poolFee;
        public int paymentInterval;
        public int blocksFound;
        public int blocksFoundSolo;
        public int currentMiners;
        public int currentMinersSolo;
        public int currentWorkers;
        public int currentWorkersSolo;
        public long currentHashrate;
        public long currentHashrateSolo;
        public float currentEffort;
        public long networkHashrate;
        public long difficulty;
        public long lastBlockFound;
        public long avgdiff;
        public int coinDiffTarget;
        public int denom;
        public int lastReward;
        //User stats
        public int pendingBalace;
        public int totalPaid;
        public float roundContrib;
        
    }

    class PoolConnector
    {
        string host = "https://api.uplexa.online";
        MainWindow main = (MainWindow)Application.Current.MainWindow;
        public PoolConnector() { }

        public async Task<string> AsyncGetStats(string endpoint)
        {
            HttpClient httpClient = new HttpClient();
            var result = await httpClient.GetAsync(host + "/" + endpoint);
            return await result.Content.ReadAsStringAsync();
        }

        public PoolStats GetPoolStats(string addr = null)
        {
            try
            {
                Task<string> poolStatFetch = Task.Run<string>(async () => await AsyncGetStats("stats"));
                dynamic data = JsonConvert.DeserializeObject<dynamic>(poolStatFetch.Result);
                long avgdiff = 0;
                int diffcount = 0;
                foreach (var asd in data.charts.difficulty)
                {
                    avgdiff += asd.ToObject<long[]>()[1];
                    diffcount++;
                }

                PoolStats s = new PoolStats();
                s.lastReward = data.lastblock.reward;
                s.coinDiffTarget = data.config.coinDifficultyTarget;
                s.avgdiff = avgdiff / diffcount;
                s.poolFee = data.config.fee;
                s.paymentInterval = data.config.paymentsInterval;
                s.blocksFound = data.pool.totalBlocks;
                s.blocksFoundSolo = data.pool.totalBlocksSolo;
                s.currentMiners = data.pool.miners;
                s.currentMinersSolo = data.pool.minersSolo;
                s.currentWorkers = data.pool.workers;
                s.currentWorkersSolo = data.pool.workersSolo;
                s.currentHashrate = data.pool.hashrate;
                s.currentHashrateSolo = data.pool.hashrateSolo;
                s.lastBlockFound = data.pool.lastBlockFound;
                long diff = data.network.difficulty;
                int difftarget = data.config.coinDifficultyTarget;
                int hashrate = data.pool.hashrate;
                float roundHashes = data.pool.roundHashes;
                s.currentEffort = roundHashes / diff;
                s.networkHashrate = Convert.ToInt64(diff / difftarget);
                s.difficulty = diff;
                int denom = data.config.denominationUnit;
                s.denom = denom;
                long roundscore = data.pool.roundScore;

                if (addr != null)
                {
                    poolStatFetch = Task.Run<string>(async () => await AsyncGetStats("stats_address?address=" + addr));
                    data = JsonConvert.DeserializeObject<dynamic>(poolStatFetch.Result);

                    int balance = 0;
                    try { balance = data.stats.balance; } catch { }
                    s.pendingBalace = balance / denom;

                    int paid = 0;
                    try { paid = data.stats.paid; } catch { }
                    s.totalPaid = paid / denom;

                    float roundsc = 0;
                    try { roundsc = data.stats.roundScore; } catch { }
                    s.roundContrib = (roundsc * 100) / roundscore;
                }
                return s;
            }
            catch(Exception e)
            {
                if (main.debugmode != null &&  main.debugmode.IsChecked == true) MessageBox.Show("Error connecting to pool: " + e.Message);
            }
            return new PoolStats();
        }
    }
}
