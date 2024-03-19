// See https://aka.ms/new-console-template for more information
using AniListNet; // Contains base classes for interacting with the API
using AniListNet.Parameters; // Contains classes for mutation and filtering
using AniListNet.Objects; // Contains model classes for handling data
using System.Text.RegularExpressions;

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

string RemoveSpecialCharacters(string str)
{
    return Regex.Replace(str, "[^a-zA-Z0-9]+", String.Empty, RegexOptions.Compiled);
}

var lines = File.ReadLines("Manga.txt");
var client = new AniClient();
foreach (var line in lines)
{
    Console.WriteLine("Scanning for: " + line);
    var results = await client.SearchMediaAsync(new SearchMediaFilter
    {
        
        Query = line, // The term to search for
        Type = MediaType.Manga, // Filters search results to anime only
        Sort = MediaSort.Popularity, // Sorts them by popularity
        Format = new Dictionary<MediaFormat, bool>
        {
            { MediaFormat.Manga, true } // Set to only search for Manga 
        },
    });

    var result = results.Data.First();
    
    string imageUrl = result.Cover.ExtraLargeImageUrl.ToString();
    string saveFile = RemoveSpecialCharacters(line).ToLower();
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
        System.Diagnostics.Process.Start(imageUrl);
        Console.WriteLine("Please check your Browser if this is result is correct.");
        Console.WriteLine("Press Y if it is, any other if not.");
        var answer = Console.ReadKey();

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
                File.Create("notfound.txt");
            }

            File.AppendText(line);

        }
    }
    Thread.Sleep(100); //So we dont get rate-limited
}

Console.WriteLine("Done, press any Key to exit.");
Console.ReadKey(true);