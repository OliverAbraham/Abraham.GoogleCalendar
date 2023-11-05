using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Text;

namespace Abraham.GoogleCalendar;

/// <summary>
/// Read entries from a google calendar easily.
/// Possible with only one line of code.
/// For examples, please refer to the demo project on github.
/// 
/// Author:
/// Oliver Abraham, mail@oliver-abraham.de, https://www.oliver-abraham.de
/// 
/// Source code hosted at: (with demo project)
/// https://github.com/OliverAbraham/Abraham.GoogleCalendar
/// 
/// Nuget Package hosted at: 
/// https://www.nuget.org/packages/Abraham.GoogleCalendar/
/// 
/// </summary>
public class GoogleCalendarReader
{
    #region ------------- Types and constants -------------------------------------------------
    public class Event
    {
        public string Summary { get; set; }
        public string Description { get; set; }
        public DateTime? When { get; internal set; }

        public override string ToString()
        {
            return $"{Summary} {When} {Description}";
        }
    }
    #endregion



    #region ------------- Properties ----------------------------------------------------------
    public List<string> Messages { get; private set; }
    public Events? Events { get; private set; }
    #endregion



    #region ------------- Fields --------------------------------------------------------------
    // If modifying these scopes, delete your previously saved credentials
    // at ~/.credentials/calendar-dotnet-quickstart.json
    private string[] _scopes = { CalendarService.Scope.CalendarReadonly };
    
    // The name of the application. This name is included in the User-Agent HTTP header.
    private string _applicationName;
    private string _credentialsFilename;
    private string _accessTokenFilename;
    private string _credentialsJson;
    #endregion



    #region ------------- Init ----------------------------------------------------------------
    public GoogleCalendarReader()
    {
        _applicationName = "Abraham.GoogleCalenderReader";
        _accessTokenFilename = "token.json";
        Messages = new List<string>();
    }
    #endregion



    #region ------------- Methods -------------------------------------------------------------
    public GoogleCalendarReader UseCredentialsFile(string credentialsFilename)
	{
		_credentialsFilename = credentialsFilename ?? throw new ArgumentNullException(nameof(credentialsFilename));
		return this;
	}

    public GoogleCalendarReader UseCredentials(string json)
	{
		_credentialsJson = json ?? throw new ArgumentNullException(nameof(json));
		return this;
	}

    public GoogleCalendarReader UseAccessTokenFilename(string accessTokenFilename)
	{
		_accessTokenFilename = accessTokenFilename ?? throw new ArgumentNullException(nameof(accessTokenFilename));
		return this;
	}

    public GoogleCalendarReader UseApplicationName(string applicationName)
	{
		_applicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
		return this;
	}

    public List<Event> ReadEvents(int maxEventCount = 999999)
    {
        return ReadEventsByStartTime(DateTime.Now, DateTime.MaxValue, maxEventCount);
    }

    public List<Event> ReadEventsByStartTime(DateTime startTime, DateTime endTime, int maxEventCount = 999999)
    {
        VerifyParameters();
        Messages.Clear();

        var credential = LoginToGoogle();

        Events = ReadEvents(credential, startTime, endTime, maxEventCount);

        var results = MapEventsToResults(Events);
        return results;
    }
    #endregion



    #region ------------- Implementation ------------------------------------------------------
    private UserCredential LoginToGoogle()
    {
        // Authorize
        string credentialJson = string.Empty;
        if (!string.IsNullOrWhiteSpace(_credentialsFilename))
            credentialJson = File.ReadAllText(_credentialsFilename);
        else
            credentialJson = _credentialsJson;
        var credentialBytes = Encoding.UTF8.GetBytes(credentialJson);

        UserCredential credential = null;
        using (var stream = new MemoryStream(credentialBytes, 0, credentialJson.Length, writable: false, publiclyVisible: true))
        {
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                _scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(_accessTokenFilename, true)).Result;
            Messages.Add("Credential file saved to: " + _accessTokenFilename);
        }

        if (credential is null)
            throw new Exception("Login to google failed.");
        return credential;
    }

    private Events? ReadEvents(UserCredential credential, DateTime startTime, DateTime endTime, int maxEventCount)
    {
        // Create Google Calendar API service.
        var service = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = _applicationName,
        });


        // Define parameters of request.
        EventsResource.ListRequest request = service.Events.List("primary");
        request.TimeMin      = startTime;
        request.TimeMax      = endTime;
        request.ShowDeleted  = false;
        request.SingleEvents = true;
        request.MaxResults   = maxEventCount;
        request.OrderBy      = EventsResource.ListRequest.OrderByEnum.StartTime;


        // Read events
        var events = request.Execute();
        Messages.Add("Reading events:");
        if (events.Items is null || events.Items.Count == 0)
        {
            Messages.Add("No events found.");
            return null;
        }

        Messages.Add($"{events.Items.Count} events were read");
        return events;
    }

    private List<Event> MapEventsToResults(Events events)
    {
        var results = new List<Event>();
        if (events.Items is null)
            return results;

        foreach (var item in events.Items)
        {
            Event New = new Event();
            New.Summary = item.Summary;
            New.Description = item.Description;
            if (item.Start != null && item.Start.DateTime != null)
            {
                New.When = item.Start.DateTime;
            }
            else
            {
                if (DateTime.TryParse(item.Start.Date, out DateTime Temp))
                    New.When = Temp;
            }
            results.Add(New);
        }

        return results;
    }

    private void VerifyParameters()
    {
        if (string.IsNullOrEmpty(_credentialsFilename) &&
            string.IsNullOrEmpty(_credentialsJson))
            throw new InvalidOperationException("Please specify a credentials file or the Json credentials directly. Call either method UseCredentialsFile or UseCredentials");

        if (!File.Exists(_credentialsFilename))
            throw new InvalidOperationException("You need to have the credentials file. Go get this file from google. Refer to my README.md for instructions.");

        if (string.IsNullOrEmpty(_applicationName))
            throw new InvalidOperationException("Please specify an application name.");

        if (string.IsNullOrEmpty(_accessTokenFilename))
            throw new InvalidOperationException("Please specify an accessTokenFilename.");
    }
    #endregion
}
