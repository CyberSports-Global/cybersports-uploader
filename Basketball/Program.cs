using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using CommandLine;

namespace Basketball
{
    internal class Program
    {
        private static readonly object LiveLock = new object();
        private static readonly object BoxLock = new object();
        private static readonly object PlayByPlayLock = new object();

        private static string _gameName = string.Empty;
        private static string _homeTeamName = string.Empty;
        private static string _awayTeamName = string.Empty;
        private static bool _liveExported = false;
        private static bool _boxExported = false;
        private static bool _playByPlayExported = false;

        private static DateTime _lastLiveDateTime = DateTime.MinValue;
        private static DateTime _lastBoxDateTime = DateTime.MinValue;
        private static DateTime _lastPlayByPlayTime = DateTime.MinValue;

        private static int _liveCount = 0;
        private static int _boxCount = 0;
        private static int _playByPlayCount = 0;

        private static void Main(string[] args)
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version;

            if (version != null)
            {
                Console.WriteLine($"Version: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}");
            }

            Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
        }

        //private static void HandleParseError(IEnumerable<Error> errors)
        //{
        //    Console.Error.WriteLine("One or more errors found while parsing command lines:");
        //    foreach (var error in errors)
        //    {
        //        Console.Error.WriteLine($" - {error}");
        //    }
        //}

        private static void Run(Options options)
        {
            if (options.Verbose)
            {
                Console.WriteLine($"           WatchFolder: {options.WatchFolder}");
                Console.WriteLine($"          OutputFolder: {options.OutputFolder}");
                Console.WriteLine($"              LeftHtml: {options.LeftHtml}");
                Console.WriteLine($"             RightHtml: {options.RightHtml}");
                Console.WriteLine($"            CenterHtml: {options.CenterHtml}");
                Console.WriteLine($"                CssUrl: {options.CssUrl}");
                Console.WriteLine($"              FTP.Host: {options.FtpHost}");
                Console.WriteLine($"            FTP.Folder: {options.FtpFolder}");
                Console.WriteLine($"          FTP.Username: {options.FtpUsername}");
                Console.WriteLine($"          FTP.Password: {options.FtpPassword}");
                Console.WriteLine($"      FTP.LiveFileName: {options.FtpLiveFile}");
                Console.WriteLine($"       FTP.BoxFileName: {options.FtpBoxFile}");
                Console.WriteLine($"FTP.PlayByPlayFileName: {options.FtpPlayByPlayFile}");
                Console.WriteLine($"            AnalyticId: {options.AnalyticId}");
            }

            if (!Directory.Exists(options.OutputFolder))
            {
                Directory.CreateDirectory(options.OutputFolder);
            }

            var watcher = new FileSystemWatcher();

            watcher.Path = options.WatchFolder;
            watcher.EnableRaisingEvents = true;

            watcher.Created += (sender, eventArgs) => UpdateGames(eventArgs, options);
            //watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);

            watcher.Changed += (sender, eventArgs) => UpdateGames(eventArgs, options);
            //watcher.Renamed += new RenamedEventHandler(watcher_Renamed);
            if (options.Verbose)
            {
                Console.WriteLine("FileSystemWatcher ready and listening to changes in: " + watcher.Path);
            }

            new System.Threading.AutoResetEvent(false).WaitOne();
        }

        private static void UpdateGames(FileSystemEventArgs e, Options options)
        {
            var gameId = e.Name.Substring(0, Math.Min(e.Name.Length, 19));
            if (e.Name.EndsWith("asp", StringComparison.OrdinalIgnoreCase))
            {
                lock (LiveLock)
                {
                    if (DateTime.Now.Subtract(_lastLiveDateTime).TotalMilliseconds > 2000)
                    {
                        _liveCount = 1;
                    }
                    else
                    {
                        _liveCount++;
                    }

                    _lastLiveDateTime = DateTime.Now;

                    if (_liveCount != 2)
                    {
                        Console.WriteLine($"Live Count: {_liveCount}");
                        return;
                    }

                    if (gameId != _gameName)
                    {
                        _gameName = gameId;
                        _boxExported = false;
                        _playByPlayExported = false;
                        _liveExported = false;
                        _homeTeamName = string.Empty;
                        _awayTeamName = string.Empty;
                    }

                    try
                    {
                        File.Copy(e.FullPath, options.LiveHtmlPath, true);
                        if (options.Verbose)
                        {
                            Console.WriteLine($"{DateTime.Now:T} - Moved live");
                        }

                        var originalLines = File.ReadAllLines(options.LiveHtmlPath);
                        if (originalLines.Length < 200)
                        {
                            return;
                        }
                        var newLines = new string[originalLines.Length];
                        Array.Copy(originalLines, 12, newLines, 0, originalLines.Length - 12);
                        if (_homeTeamName == string.Empty)
                        {
                            _homeTeamName = originalLines[1];
                        }

                        if (_awayTeamName == string.Empty)
                        {
                            _awayTeamName = originalLines[2];
                        } 
                        if (string.IsNullOrEmpty(options.CssUrl))
                        {
                            newLines[5] = newLines[5].Replace(".asp", ".css");
                        }
                        else
                        {
                            newLines[5] = newLines[5].Replace("BasketballWebcastStyle.asp", options.CssUrl);
                        }
                        newLines[5] += $"{Environment.NewLine}<title>{_awayTeamName} vs {_homeTeamName} - Live Stats</title>";

                        if (!string.IsNullOrEmpty(options.AnalyticId))
                        {
                            newLines[5] += $"{Environment.NewLine}<script async src=\"https://www.googletagmanager.com/gtag/js?id={options.AnalyticId}\"></script><script>window.dataLayer = window.dataLayer || [];function gtag(){{dataLayer.push(arguments);}}gtag('js', new Date());gtag('config', '{options.AnalyticId}');</script>";
                        }
                        newLines[14] = $"{options.LeftHtml}</td>";
                        newLines[16] = options.CenterHtml;
                        newLines[18] = $"{options.RightHtml}</td>";
                        newLines[74] = $"<!-- {newLines[74]} -->";
                        newLines[75] = $"<!-- {newLines[75]} -->";
                        newLines[76] = $"<!-- {newLines[76]} -->";
                        newLines[83] = $"<!-- {newLines[83]} -->";
                        newLines[84] = $"<!-- {newLines[84]} -->";
                        newLines[85] = $"<!-- {newLines[85]} -->";
                        for (var i = newLines.Length - 42; i <= newLines.Length - 42 + 26; i++)
                        {
                            newLines[i] = $"<!-- {newLines[i]} -->";
                        }

                        var boxLink = $"<a CLASS=\"StatCentral\" href=\"{options.FtpBoxFile}\">Box Score</a>";
                        var playByPlayLink = $"<a CLASS=\"StatCentral\" href=\"{options.FtpPlayByPlayFile}\">Play by Play</a>";

                        if (_boxExported && _playByPlayExported)
                        {
                            newLines[114] = $"{boxLink}&nbsp;&nbsp;{playByPlayLink}";
                        }
                        else if (_boxExported)
                        {
                            newLines[114] = $"{boxLink}";
                        }
                        else if (_playByPlayExported)
                        {
                            newLines[114] = $"{playByPlayLink}";
                        }
                        else
                        {
                            newLines[114] = "&nbsp;";
                        }
                        
                        File.WriteAllLines(options.LiveHtmlPath, newLines);
                        if (options.Verbose)
                        {
                            Console.WriteLine($"{DateTime.Now:T} - Edited live");
                        }

                        using (var liveClient = new WebClient())
                        {
                            liveClient.Credentials = new NetworkCredential(options.FtpUsername, options.FtpPassword);
                            liveClient.UploadFile($"ftp://{options.FtpHost}/{options.FtpFolder}/{options.FtpLiveFile}", WebRequestMethods.Ftp.UploadFile, options.LiveHtmlPath);
                            Console.WriteLine($"{DateTime.Now:T} - Uploaded live");
                            _liveExported = true;
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }

            if (e.Name.EndsWith("bx.html", StringComparison.OrdinalIgnoreCase))
            {
                lock (BoxLock)
                {
                    try
                    {
                        if (DateTime.Now.Subtract(_lastBoxDateTime).TotalMilliseconds > 2000)
                        {
                            _boxCount = 1;
                        }
                        else
                        {
                            _boxCount++;
                        }

                        _lastBoxDateTime = DateTime.Now;

                        if (_boxCount != 2)
                        {
                            Console.WriteLine($"Box Count: {_boxCount}");
                            return;
                        }

                        var originalLines = File.ReadAllLines(e.FullPath);
                        var newLines = new string[originalLines.Length];
                        Array.Copy(originalLines, 0, newLines, 0, originalLines.Length);
                        newLines[3] = $"<title>{_awayTeamName} vs {_homeTeamName} - Box Score</title>";
                        newLines[5] = originalLines[5].Replace("#FFFFCC", "#FFFFFF");
                        newLines[7] = string.Empty; 
                        newLines[8] = string.Empty;
                        newLines[9] = $"<center><h4>{_awayTeamName} vs {_homeTeamName}</h4>";
                        newLines[10] = originalLines[10].Replace($"{_homeTeamName} vs {_awayTeamName}</h4>", "");

                        if (!string.IsNullOrEmpty(options.AnalyticId))
                        {
                            newLines[3] += $"{Environment.NewLine}<script async src=\"https://www.googletagmanager.com/gtag/js?id={options.AnalyticId}\"></script><script>window.dataLayer = window.dataLayer || [];function gtag(){{dataLayer.push(arguments);}}gtag('js', new Date());gtag('config', '{options.AnalyticId}');</script>";
                        }

                        File.WriteAllLines(options.BoxHtmlPath, newLines);
                        if (options.Verbose)
                        {
                            Console.WriteLine($"{DateTime.Now:T} - Moved box");
                        }
                        using (var liveClient = new WebClient())
                        {
                            liveClient.Credentials = new NetworkCredential(options.FtpUsername, options.FtpPassword);
                            liveClient.UploadFile($"ftp://{options.FtpHost}/{options.FtpFolder}/{options.FtpBoxFile}", WebRequestMethods.Ftp.UploadFile, options.BoxHtmlPath);
                            Console.WriteLine($"{DateTime.Now:T} - Uploaded box");
                            _boxExported = true;
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }

            if (e.Name.EndsWith("pl.html", StringComparison.OrdinalIgnoreCase))
            {
                lock (PlayByPlayLock)
                {
                    try
                    {
                        if (DateTime.Now.Subtract(_lastPlayByPlayTime).TotalMilliseconds > 2000)
                        {
                            _playByPlayCount = 1;
                        }
                        else
                        {
                            _playByPlayCount++;
                        }

                        _lastPlayByPlayTime = DateTime.Now;

                        if (_playByPlayCount != 2)
                        {
                            Console.WriteLine($"Play by Play Count: {_playByPlayCount}");
                            return;
                        }

                        var originalLines = File.ReadAllLines(e.FullPath);
                        var newLines = new string[originalLines.Length];
                        Array.Copy(originalLines, 0, newLines, 0, originalLines.Length);

                        newLines[3] = $"<title>{_awayTeamName} vs {_homeTeamName} - Box Score</title>";
                        newLines[5] = originalLines[5].Replace("#FFFFCC", "#FFFFFF");
                        newLines[7] = string.Empty;
                        newLines[8] = string.Empty;
                        //newLines[9] = originalLines[9]
                        //    .Replace(
                        //        $"</table></th></table><center> <h4>{_homeTeamName} - Play by Play Report<br>{_homeTeamName} vs. {_awayTeamName}",
                        //        "<center><h4>{_awayTeamName} vs {_homeTeamName}");
                        newLines[10] = string.Empty;
                        newLines[11] = string.Empty;

                        if (!string.IsNullOrEmpty(options.AnalyticId))
                        {
                            newLines[3] += $"{Environment.NewLine}<script async src=\"https://www.googletagmanager.com/gtag/js?id={options.AnalyticId}\"></script><script>window.dataLayer = window.dataLayer || [];function gtag(){{dataLayer.push(arguments);}}gtag('js', new Date());gtag('config', '{options.AnalyticId}');</script>";
                        }

                        File.WriteAllLines(options.PlayByPlayHtmlPath, newLines);
                        if (options.Verbose)
                        {
                            Console.WriteLine($"{DateTime.Now:T} - Moved Play by Play");
                        }
                        using (var liveClient = new WebClient())
                        {
                            liveClient.Credentials = new NetworkCredential(options.FtpUsername, options.FtpPassword);
                            liveClient.UploadFile($"ftp://{options.FtpHost}/{options.FtpFolder}/{options.FtpPlayByPlayFile}", WebRequestMethods.Ftp.UploadFile, options.PlayByPlayHtmlPath);
                            Console.WriteLine($"{DateTime.Now:T} - Uploaded Play by Play");
                            _playByPlayExported = true;
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }
        }
    }
}
