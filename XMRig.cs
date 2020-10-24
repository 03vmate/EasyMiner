using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace EasyMiner
{
    struct XMRigConfig
    {
        public string host;
        public string algo;
        public string user;
        public string pass;
        public int threads;
        public int priority;
    }


    class XMRig
    {
        public string minerPath = "";
        public string minerOutput = "";
        public Process proc = null;
        public XMRig(string customPath = null)
        {
            if (customPath == null) ResourceHandler();
            else minerPath = customPath;
        }

        public void StopMining()
        {
            if(proc != null) proc.Kill(true);
        }

        public void StartMining(XMRigConfig config)
        {
            if (proc != null) return;
            var _proc = new Process();
            _proc.StartInfo.FileName = minerPath + "\\xmrigDaemon.exe";
            _proc.StartInfo.Arguments = $"-o {config.host} -a {config.algo} -u {config.user} -p {config.pass} -t {Convert.ToString(config.threads)} --cpu-priority {Convert.ToString(config.priority)}";
            _proc.StartInfo.UseShellExecute = false;
            _proc.StartInfo.RedirectStandardOutput = true;
            _proc.StartInfo.RedirectStandardInput = true;
            _proc.StartInfo.CreateNoWindow = true;
            _proc.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    minerOutput += e.Data + "\n";
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
