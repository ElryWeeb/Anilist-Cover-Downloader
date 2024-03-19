// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using AniListNet; // Contains base classes for interacting with the API
using AniListNet.Parameters; // Contains classes for mutation and filtering
using AniListNet.Objects; // Contains model classes for handling data

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
var client = new AniClient();
Process process = new Process();
foreach (var line in lines)
{
    Console.WriteLine("Scanning for: " + line);
    var results = await client.SearchMediaAsync(new SearchMediaFilter
    {
        
        Query = line, // The term to search for
        Type = MediaType.Manga, // Filters search results to anime only
        Sort = MediaSort.Popularity // Sorts them by popularity
    });

    var result = results.Data.First();
    
    string imageUrl = result.Cover.ExtraLargeImageUrl.ToString();
    string saveFile = line.Replace("'", "");
    saveFile = saveFile.Replace(".", "-");
    string fileType = imageUrl.Split('.').Last();
    
    if (line == result.Title.EnglishTitle || line == result.Title.RomajiTitle)
    {
        //Download
        using (var webClient = new HttpClient())
        {
            using (var s = webClient.GetStreamAsync(imageUrl))
            {
                using (var fs = new FileStream("Output\\" + saveFile + "." + fileType, FileMode.OpenOrCreate))
                {
                    s.Result.CopyTo(fs);
                }
            }
        }
    }
    else
    {
        Console.WriteLine($"We couldn't confirm the result for {line}");
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.FileName = "chrome";
        process.StartInfo.Arguments = @result.Url.AbsoluteUri; 
        process.Start();
        Console.WriteLine("Please check your Browser if this is result is correct.");
        Console.WriteLine("Press Y if it is, any other if not.");
        var answer = Console.ReadKey(true);

        if (answer.Key == ConsoleKey.Y)
        {
            //Download
            using (var webClient = new HttpClient())
            {
                using (var s = webClient.GetStreamAsync(imageUrl))
                {
                    using (var fs = new FileStream("Output\\" + saveFile + "." + fileType, FileMode.OpenOrCreate))
                    {
                        s.Result.CopyTo(fs);
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
            File.AppendAllText("notfound.txt",line);
        }
    }
    Thread.Sleep(100); //So we dont get rate-limited
}

Console.WriteLine("Done, press any Key to exit.");
Console.ReadKey(true);