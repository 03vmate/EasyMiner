using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EasyMiner
{
    struct PoolStats
    {
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
        public int blockFoundEvery;
        public long networkHashrate;
        public long difficulty;
        public long lastBlockFound;

    }
    class PoolConnector
    {
        string host = "https://api.uplexa.online";
        public PoolConnector() { }

        public async Task<string> AsyncGetPoolStats()
        {
            HttpClient httpClient = new HttpClient();
            var result = await httpClient.GetAsync(host + "/stats");
            return await result.Content.ReadAsStringAsync();
        }

        public PoolStats GetPoolStats()
        {
            Task<string> poolStatFetch = Task.Run<string>(async () => await AsyncGetPoolStats());
            dynamic data = JsonConvert.DeserializeObject<dynamic>(poolStatFetch.Result);

            PoolStats s = new PoolStats();
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
            s.currentEffort = Convert.ToSingle(data.pool.roundhashes) / Convert.ToSingle(diff) * 100F;
            try { s.blockFoundEvery = Convert.ToInt32(diff / difftarget / hashrate * 120F); }
            catch { s.blockFoundEvery = 0; }
            s.networkHashrate = Convert.ToInt64(diff / difftarget);
            s.difficulty = diff;

            return s;
        }
    }
}
