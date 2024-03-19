﻿// See https://aka.ms/new-console-template for more information
using AniListNet; // Contains base classes for interacting with the API
using AniListNet.Parameters; // Contains classes for mutation and filtering
using AniListNet.Objects; // Contains model classes for handling data
using System.Text.RegularExpressions;

Console.WriteLine("Enter Path to file to scan");

string fileName = Console.ReadLine();

if (string.IsNullOrEmpty(fileName))
{
    Environment.Exit(0);
}
if (!File.Exists(fileName))
{
    Environment.Exit(0);
}

Console.WriteLine("Enter Path to where to save images.");
string folderName = Console.ReadLine();

if (string.IsNullOrEmpty(folderName))
{
    Environment.Exit(0);
}
if (!Directory.Exists(folderName))
{
    Directory.CreateDirectory(folderName);
}

string RemoveSpecialCharacters(string str)
{
    return Regex.Replace(str, "[^a-zA-Z0-9]+", String.Empty, RegexOptions.Compiled);
}

var lines = File.ReadLines(fileName);
var client = new AniClient();
foreach (var line in lines)
{
    var results = await client.SearchMediaAsync(new SearchMediaFilter
    {
        Query = line, // The term to search for
        Type = MediaType.Manga, // Filters search results to anime only
        Sort = MediaSort.Popularity, // Sorts them by popularity
        Format = new Dictionary<MediaFormat, bool>
        {
            { MediaFormat.Manga, true } // Set to only search for Manga 
        }
    });
    foreach (var result in results.Data){
        Console.WriteLine("Scanning for: " + line);
        if (line != result.Title.EnglishTitle || line != result.Title.NativeTitle || line != result.Title.RomajiTitle)
        {
            Console.WriteLine($"We couldn't confirm the result for {line}");
            Console.WriteLine(result.Url);
            Console.WriteLine("Please check if this is correct. Y/N");
            var answer = Console.ReadKey();

            if (answer.Key == ConsoleKey.Y)
            {
                //Download
                string imageUrl = result.Cover.ExtraLargeImageUrl.ToString();
                string saveFile = RemoveSpecialCharacters(line).ToLower();
                string fileType = imageUrl.Split('.').Last();
                using (var webClient = new HttpClient())
                {
                    using (var s = webClient.GetStreamAsync(imageUrl))
                    {
                        using (var fs = new FileStream(folderName + "\\" + saveFile + "." + fileType, FileMode.OpenOrCreate))
                        {
                            s.Result.CopyTo(fs);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Adding Manga to notfound.txt");
                if (!File.Exists(folderName + "notfound.txt"))
                {
                    File.Create(folderName + "notfound.txt");
                }

                File.AppendText(line);

            }
            
        }
        else
        {
            //Download
            string imageUrl = result.Cover.ExtraLargeImageUrl.ToString();
            string saveFile = RemoveSpecialCharacters(line).ToLower();
            string fileType = imageUrl.Split('.').Last();
            using (var webClient = new HttpClient())
            {
                using (var s = webClient.GetStreamAsync(imageUrl))
                {
                    using (var fs = new FileStream(saveFile + "." + fileType, FileMode.OpenOrCreate))
                    {
                        s.Result.CopyTo(fs);
                    }
                }
            }
        }
    }
    Thread.Sleep(100); //So we dont get rate-limited
}

Console.WriteLine("Done, press any Key to exit.");
Console.ReadKey(true);