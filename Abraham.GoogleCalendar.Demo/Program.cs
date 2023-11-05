using Abraham.GoogleCalendar;
using Google.Apis.Auth.OAuth2;
using System.Net;

namespace Abraham.GoogleCalendar.Demo;

/// <summary>
/// Demo of the GoogleCalendar Nuget package.
/// Demonstrates how to read all upcoming appointments from a google calendar.
/// 
/// Author:
/// Oliver Abraham, mail@oliver-abraham.de, https://www.oliver-abraham.de
/// 
/// Source code hosted at: 
/// https://github.com/OliverAbraham/Abraham.GoogleCalendar
/// 
/// Nuget Package hosted at: 
/// https://www.nuget.org/packages/Abraham.GoogleCalendar
/// 
/// </summary>
/// 
internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Demo for the Nuget package 'Abraham.GoogleCalendar'\n\n");

        // You need a file to access the google calendar.
        // Instructions:
        // 1. Go to https://console.developers.google.com/apis/credentials
        // 2. Create a new project
        // 3. Create a new OAuth client ID
        // 4. Download the credentials file and save it to a secure location.
        // 5. Set the credentials file path in the code below.
        var credentialsFile = @"C:\Credentials\CoogleCalendarReaderNugetPackageDemo.json";

        var reader = new GoogleCalendarReader()
            .UseCredentialsFile(credentialsFile)
            .UseApplicationName("MyGoogleClient");

        var events = reader.ReadEvents(5); // read the next 5 events since now

        foreach(var message in reader.Messages)
            Console.WriteLine(message);

        Console.WriteLine("\n\nEvents:");
        foreach(var @event in events)
            Console.WriteLine(@event);
       


        Console.WriteLine("\n\nPress any key to end the demo.");
        Console.ReadKey();




        //// one line version:
        //var events = new GoogleCalendarReader()
        //    .UseCredentialsFile(@"C:\Credentials\CoogleCalendarReaderNugetPackageDemo.json")
        //    .UseApplicationName("MyGoogleClient")
        //    .ReadEvents();


        //// reading all events since year 2000:
        //var allEvents = new GoogleCalendarReader()
        //    .UseCredentialsFile(@"C:\Credentials\CoogleCalendarReaderNugetPackageDemo.json")
        //    .UseApplicationName("MyGoogleClient")
        //    .ReadEventsByStartTime(new DateTime(2000,1,1));
    }
}