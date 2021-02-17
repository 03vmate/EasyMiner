using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace EasyMiner
{
    public struct XMRigConfig
    {
        public string host;
        public string algo;
        public string user;
    }

    public class XMRig
    {
        public struct XMRigStats
        {
            public UInt16 acceptedShares;
            public UInt16 invalidShares;
            public UInt64 difficulty;
            public int hashrateCurrent;
            public int hashrate60s;
            public int hashrate15m;
        };

        public XMRigStats stats = new XMRigStats();
        public string minerPath = "";
        public string minerOutput = "";
        public string minerHashrate = "";
        public int acceptedShares = 0;
        public int declinedShares = 0;
        public Process proc = null;
        public XMRig(string customPath = null)
        {
            if (customPath == null) ResourceHandler();
            else minerPath = customPath;
        }

        public void StopMining()
        {
            if(proc != null) proc.Kill(true);
            proc = null;
        }

        public void StartMining(XMRigConfig config)
        {
            if (proc != null) return;
            var _proc = new Process();
            _proc.StartInfo.FileName = minerPath + "\\xmrigDaemon.exe";
            _proc.StartInfo.Arguments = $"-o {config.host} -a {config.algo} -u {config.user} -p @EasyMiner_{System.Environment.MachineName}";
            _proc.StartInfo.UseShellExecute = false;
            _proc.StartInfo.RedirectStandardOutput = true;
            _proc.StartInfo.RedirectStandardInput = true;
            _proc.StartInfo.CreateNoWindow = true;
            
            _proc.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    minerOutput += e.Data + "\n";

                    //Parsing from stdout to stats
                    string[] split = e.Data.Split(' ');
                    if (e.Data.Contains("accepted"))
                    {
                        string[] shares = split[6].Substring(1, split[6].Length - 2).Split('/');
                        stats.acceptedShares = UInt16.Parse(shares[0]);
                        stats.invalidShares = UInt16.Parse(shares[1]);
                        stats.difficulty = UInt64.Parse(split[8]);
                    }
                    else if (e.Data.Contains("speed"))
                    {
                        string[] _h = {split[4],split[5],split[6]};
                        stats.hashrateCurrent = _h[0] != "n/a" ? Convert.ToInt32(Convert.ToDouble(_h[0])) : 0;
                        stats.hashrate60s = _h[1] != "n/a" ? Convert.ToInt32(Convert.ToDouble(_h[1])) : 0;
                        stats.hashrate15m = _h[2] != "n/a" ? Convert.ToInt32(Convert.ToDouble(_h[2])) : 0;
                    }
                    else if(e.Data.Contains("new job from"))
                    {
                        stats.difficulty = UInt64.Parse(split[10]);
                    }
                }
            });

            proc = _proc;

            proc.Start();
            proc.BeginOutputReadLine();
        }

        private void ResourceHandler()
        {
            if(!Directory.Exists(Path.GetTempPath() + @"\easyminer")) Directory.CreateDirectory(Path.GetTempPath() + @"\easyminer");

            if (!File.Exists(Path.GetTempPath() + @"\easyminer\xmrigMiner.exe") || !File.Exists(Path.GetTempPath() + @"\easyminer\xmrigDaemon.exe") || !File.Exists(Path.GetTempPath() + @"\easyminer\WinRing0x64.sys"))
            {
                File.WriteAllBytes(Path.GetTempPath() + @"\easyminer\xmrigDaemon.exe", Properties.Resources.xmrigDaemon);
                File.WriteAllBytes(Path.GetTempPath() + @"\easyminer\xmrigMiner.exe", Properties.Resources.xmrigMiner);
                File.WriteAllBytes(Path.GetTempPath() + @"\easyminer\WinRing0x64.sys", Properties.Resources.WinRing0x64);
            }

            minerPath = Path.GetTempPath() + @"\easyminer\";
        }
    }
}
