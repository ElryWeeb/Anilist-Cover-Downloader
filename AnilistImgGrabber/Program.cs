// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
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

var lines = File.ReadAllLines("Manga.txt");
var client = new Client();
List<string> recheck = new List<string>();

foreach (var line in lines)
{
    
    Console.WriteLine("Scanning for: " + line);
    tryagain:
    var results = await client.GetMediaBySearch(line, MediaTypes.MANGA, 1, 10);
    
    if (results == null || results.Media == null)
    {
        Console.WriteLine("Rate-limited. Waiting 1 Minute.");
        Thread.Sleep(61000);
        goto tryagain;
    }
    if (results.Media.Length < 1)
    {
        Console.WriteLine("Couldnt find " + line + " on Anilist.");
        Console.WriteLine("Adding Manga to NotFound.txt");
        if (!File.Exists("NotFound.txt"))
        {
            File.Create("NotFound.txt").Close();
        }
        File.AppendAllText("NotFound.txt",line + Environment.NewLine);
        continue;
    }

    var result = results.Media.First();

    string imageUrl = result.CoverImageExtraLarge;
    string saveFile = line.Replace("'", "");
    saveFile = saveFile.Replace(".", "-");
    saveFile = saveFile.Replace("!", "");
    saveFile = saveFile.Replace("?", "");
    string fileType = imageUrl.Split('.').Last();
    
    if (line == result.EnglishTitle || line == result.RomajiTitle)
    {
        //Download
        using (var webClient = new HttpClient())
        {
            webClient.Timeout = TimeSpan.FromSeconds(5);
            using (var s = webClient.GetStreamAsync(imageUrl).Result)
            {
                using (var fs = new FileStream("Output\\" + saveFile + "." + fileType, FileMode.OpenOrCreate))
                {
                    s.CopyTo(fs);
                }

            }
        }
        
    }
    else
    {
        Console.WriteLine($"We couldn't confirm the result for {line}, adding for a later Re-Check.");
        recheck.Add(line);
    }

    Thread.Sleep(1000); //So we don't get rate-limited
}

Process process = new Process();
foreach (var check in recheck)
{
    Console.WriteLine("Please review all Manga that were not 100% Identifiable.");
    tryagain:
    var results = await client.GetMediaBySearch(check, MediaTypes.MANGA, 1, 10);
    
    if (results == null)
    {
        Console.WriteLine("Rate-limited. Waiting 1 Minute.");
        Thread.Sleep(61000);
        goto tryagain;
    }

    var result = results.Media.First();

    string imageUrl = result.CoverImageExtraLarge;
    string saveFile = check.Replace("'", "");
    saveFile = saveFile.Replace(".", "-");
    saveFile = saveFile.Replace("!", "");
    saveFile = saveFile.Replace("?", "");
    string fileType = imageUrl.Split('.').Last();
    process.StartInfo.UseShellExecute = true;
    process.StartInfo.FileName = "chrome";
    process.StartInfo.Arguments = @result.SiteUrl; 
    process.Start();
    Console.WriteLine("Please check your Browser if this is result is correct.");
    Console.WriteLine("Press Y if it is, any other if not.");
    var answer = Console.ReadKey(true);

    if (answer.Key == ConsoleKey.Y)
    {
        //Download
        using (var webClient = new HttpClient())
        {
            webClient.Timeout = TimeSpan.FromSeconds(5);
            using (var s = webClient.GetStreamAsync(imageUrl).Result)
            {
                using (var fs = new FileStream("Output\\" + saveFile + "." + fileType, FileMode.OpenOrCreate))
                {
                    s.CopyTo(fs);
                }

            }
        }
    }
    else
    {
        Console.WriteLine("Adding Manga to NotFound.txt");
        if (!File.Exists("NotFound.txt"))
        {
            File.Create("NotFound.txt").Close();
        }
        File.AppendAllText("NotFound.txt",check + Environment.NewLine);
    }
    Thread.Sleep(1000); //So we don't get rate-limited
}


Console.WriteLine("Done, press any Key to exit.");
Console.ReadKey(true);