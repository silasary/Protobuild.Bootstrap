using fastJSON;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml.Linq;

namespace ProtobuildBootstrap
{
    public static class Program
    {
        public static int Main()
        {
            string ProtobuildVersion = null;

            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var cachePath = Path.Combine(basePath, ".protobuild-bootstrap");
            Directory.CreateDirectory(cachePath);

            var ModuleXmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Build", "Module.xml");
            if (File.Exists(ModuleXmlPath))
            {
                var moduleInfo = XDocument.Load(ModuleXmlPath);
                var wantedVersion = moduleInfo.Root.Element("ProtobuildVersion");
                if (wantedVersion != null)
                {
                    ProtobuildVersion = wantedVersion.Value;
                }
            }
            else
            {
                Console.WriteLine("WARNING:  No Module.xml was found.");
            }
            if (ProtobuildVersion == null)
            {
                Console.WriteLine("WARNING:  Unable to determing desired Protobuild version.  Using latest.");

                try
                {

                    using (var webClient = new WebClient())
                    {
                        webClient.Headers[HttpRequestHeader.UserAgent] = "Protobuild/Bootstrap";
                        var v = webClient.DownloadString("https://api.github.com/repos/Protobuild/Protobuild/git/refs/heads/master");
                        var master = JSON.ToDynamic(v);
                        string hash = master.@object.sha;
                        ProtobuildVersion = hash;
                        File.WriteAllText(Path.Combine(cachePath, "master.txt"), hash);
                    }
                }
                catch (WebException)
                {
                    ProtobuildVersion = File.ReadAllText(Path.Combine(cachePath, "master.txt"));
                }
                if (File.Exists(ModuleXmlPath))
                {
                    var moduleInfo = XDocument.Load(ModuleXmlPath);
                    moduleInfo.Root.Add(new XElement("ProtobuildVersion", ProtobuildVersion));
                    moduleInfo.Save(ModuleXmlPath, SaveOptions.None);
                }
            }
            string exePath = Path.Combine(cachePath, $"{ProtobuildVersion}.exe");
            if (!File.Exists(exePath))
            {
                var uri = $"https://github.com/Protobuild/Protobuild/raw/{ProtobuildVersion}/Protobuild.exe";
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add(HttpRequestHeader.UserAgent, "ProtoBuild Boostrapper/1.0");
                    Console.WriteLine($"Downloading Protobuild version {ProtobuildVersion}...");
                    webClient.DownloadFile(uri, exePath);
                }
            }
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = Environment.CommandLine,
                UseShellExecute = false,
            });
            proc.WaitForExit();
            return proc.ExitCode;
        }
    }
}