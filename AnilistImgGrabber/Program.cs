// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Net;
using Anilist4Net;
using Anilist4Net.Enums;

Console.WriteLine("Checking for Manga.txt");


if (!File.Exists("Manga.txt"))
{
    Console.WriteLine("Could not find file in " + AppDomain.CurrentDomain.BaseDirectory);
    Thread.Sleep(500);
    Environment.Exit(0);
}

if (!Directory.Exists("Output"))
{
    Directory.CreateDirectory("Output");
}

var lines = File.ReadLines("Manga.txt");
var client = new Client();

Process process = new Process();
try
{
    foreach (var line in lines)
    {
        Console.WriteLine("Scanning for: " + line);
        var results = client.GetMediaBySearch(line, MediaTypes.MANGA, 1, 1);

        var result = results.Result.Media;

        var res = result.First();

        string imageUrl = res.CoverImage.ExtraLarge;
        string saveFile = line.Replace("'", "");
        saveFile = saveFile.Replace(".", "-");
        string fileType = imageUrl.Split('.').Last();

        if (line == res.EnglishTitle || line == res.RomajiTitle)
        {
            Console.WriteLine("Found: " + line);
            using (var webClient = new HttpClient())
            {
                webClient.Timeout = TimeSpan.FromSeconds(5);
                tryagain:
                try
                {
                    using (var s = webClient.GetStreamAsync(imageUrl))
                    {
                        Console.WriteLine("Downloading: " + line);
                        using (var fs = new FileStream("Output\\" + saveFile + "." + fileType, FileMode.OpenOrCreate))
                        {
                            s.Result.CopyTo(fs);
                        }
                    }
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.Timeout || e.Status == WebExceptionStatus.ConnectFailure)
                    {
                        Console.WriteLine("Trying again.");
                        goto tryagain;
                    }
                }

            }
        }
        else
        {
            Console.WriteLine($"We couldn't confirm the result for {line}");
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = "chrome";
            process.StartInfo.Arguments = @res.SiteUrl;
            process.Start();
            Console.WriteLine("Please check your Browser if this is result is correct.");
            Console.WriteLine("Press Y if it is, any other if not.");
            var answer = Console.ReadKey(true);

            if (answer.Key == ConsoleKey.Y)
            {
                Console.WriteLine("Found: " + line);
                using (var webClient = new HttpClient())
                {
                    webClient.Timeout = TimeSpan.FromSeconds(5);
                    tryagain:
                    try
                    {
                        using (var s = webClient.GetStreamAsync(imageUrl))
                        {
                            Console.WriteLine("Downloading: " + line);
                            using (var fs = new FileStream("Output\\" + saveFile + "." + fileType, FileMode.OpenOrCreate))
                            {
                                s.Result.CopyTo(fs);
                            }
                        }
                    }
                    catch (WebException e)
                    {
                        if (e.Status == WebExceptionStatus.Timeout || e.Status == WebExceptionStatus.ConnectFailure)
                        {
                            Console.WriteLine("Trying again.");
                            goto tryagain;
                        }
                    }

                }
            }
            else
            {
                Console.WriteLine("Adding Manga to notfound.txt");
                if (!File.Exists("notfound.txt"))
                {
                    File.Create("notfound.txt").Close();
                }

                File.AppendAllText("notfound.txt", line);
            }
        }
    }

    Thread.Sleep(500); //So we dont get rate-limited
}
catch
{
    Console.ReadKey(true);
}

Console.WriteLine("Done, press any Key to exit.");
Console.ReadKey(true);
Environment.Exit(0);
