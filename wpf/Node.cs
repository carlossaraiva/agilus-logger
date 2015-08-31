﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace AgilusLogger
{
    using static Directory;
    using static Path;

    public static class Node
    {
        private static Process _process;

        public static event EventHandler OnExit;

        private static event EventHandler OnBeginProcess;

        public static readonly string LoggerPath = Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "agilus-logger");

        public static string ServicePath;

        private static NodeAction _command;

        private static string _fileName;

        private static string _flags;

        private static string[] _loggerFiles;

        private static string[] _serviceFiles;

        public static async void SetupNode(NodeAction command, string serviceName, string servicePort)
        {
            _command = command;
            ServicePath = Combine(LoggerPath, serviceName);
            OnBeginProcess?.Invoke(null, new EventArgs());
            switch (command.Id)
            {
                case 1:
                    _flags = $"-n {serviceName} -p {servicePort}";
                    _fileName = $"{LoggerPath}\\{_command.Value}";
                    await Task.Run(() => ExecuteInstall());
                    await Task.Run(() => UpdateNpm());
                    OnExit?.Invoke(null, new EventArgs());
                    break;

                case 2:
                    _flags = $"delete agiluslogger{serviceName}porta{servicePort}.exe";
                    _fileName = _command.Value;
                    await Task.Run(() => ExecuteUninstall());
                    OnExit?.Invoke(null, new EventArgs());
                    break;

                case 3:
                    _flags = string.Empty;
                    break;

                default:
                    _flags = string.Empty;
                    break;
            }
        }

        private static void ExecuteInstall()
        {
            if (!Exists(LoggerPath))
            {
                CreateDirectory(LoggerPath);
            }

            if (!Exists(ServicePath))
            {
                CreateDirectory(ServicePath);
            }

            _loggerFiles = GetFiles("dist\\logger");
            _serviceFiles = GetFiles("dist\\service");

            foreach (var s in _loggerFiles)
            {
                if (s == null) continue;
                var destFile = Path.Combine(LoggerPath, Path.GetFileName(s));
                File.Copy(s, destFile, true);
            }

            foreach (var s in _serviceFiles)
            {
                if (s == null) continue;
                var destFile = Path.Combine(ServicePath, Path.GetFileName(s));
                File.Copy(s, destFile, true);
            }

            StartProcess();
        }

        private static void ExecuteUninstall()
        {
            StartProcess();
        }

        private static void StartProcess()
        {
            _process = new Process
            {
                StartInfo =
                {
                    FileName = _fileName,
                    Arguments = _flags,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    RedirectStandardOutput = false,
                    Verb = "runas"
                }
            };
            {
                _process.ErrorDataReceived += (o, s) => MessageBox.Show("Erro ocorreu");
                _process.OutputDataReceived += (o, s) => MessageBox.Show("Tudo certo");
                _process.Start();
            }
        }

        private static void UpdateNpm()
        {
            var update = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),$"npm\\npm.cmd"),
                    Arguments = $"install -p {LoggerPath}",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = false,
                    Verb = "runas"
                }
            };
        }
    }

    public class NodeAction
    {
        private NodeAction(string value, int id)
        {
            Value = value;
            Id = id;
        }

        public int Id { get; set; }
        public static NodeAction Install => new NodeAction("install.cmd", 1);
        public static NodeAction Uninstall => new NodeAction("sc.exe", 2);
        public static NodeAction Update => new NodeAction(@"Update", 3);
        public string Value { get; set; }
    }
}