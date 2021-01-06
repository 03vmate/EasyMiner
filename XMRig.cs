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
            _proc.StartInfo.Arguments = $"-o {config.host} -a {config.algo} -u {config.user} -p {config.pass} -t {Convert.ToString(config.threads)} --cpu-priority {Convert.ToString(config.priority)}";
            _proc.StartInfo.UseShellExecute = false;
            _proc.StartInfo.RedirectStandardOutput = true;
            _proc.StartInfo.RedirectStandardInput = true;
            _proc.StartInfo.CreateNoWindow = false;
            
            _proc.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    minerOutput += e.Data + "\n";
                    if(e.Data.Contains("accepted"))
                    {
                        int _index = e.Data.IndexOf('(') + 1;
                        int _separator= 0;
                        int _end = 0;
                        for(int i = _index; i < e.Data.Length; i++)
                        {
                            if (e.Data[i] == '/') _separator = i;
                        }
                        for (int i = _index; i < e.Data.Length; i++)
                        {
                            if (e.Data[i] == ')')
                            {
                                _end = i - 1;
                                break;
                            }
                        }
                        string accepted = e.Data.Substring(_index, _separator - _index);
                        string total = e.Data.Substring(_separator + 1, _end - _separator);
                        Debug.WriteLine($"accepted: {accepted}/{total}");

                    }
                    else if(e.Data.Contains("speed"))
                    {

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
