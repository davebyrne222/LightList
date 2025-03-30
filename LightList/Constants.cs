namespace LightList;

public class Constants
{
    private const string DatabaseFilename = "LightList.db3";
    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
    
    public const SQLite.SQLiteOpenFlags DbFlags =
        SQLite.SQLiteOpenFlags.ReadWrite |  // open the database in read/write mode
        SQLite.SQLiteOpenFlags.Create |     // create the database if it doesn't exist
        SQLite.SQLiteOpenFlags.SharedCache; // enable multi-threaded database access
    
    public static readonly Amazon.RegionEndpoint AwsRegion = Amazon.RegionEndpoint.EUWest1;
    public const string CognitoPoolId = "eu-west-1oawcmeine";
    public const string CognitoAppClientId = "3euc6s03psji411g49qbb4ntdg";
    public const string AuthRedirectUrl = "LightList://All";
    private static readonly string CognitoRootUrl = $"https://{CognitoPoolId}.auth.{AwsRegion.SystemName}.amazoncognito.com";
    public static readonly string CognitoAuthUrl = $"{CognitoRootUrl}/oauth2/authorize" +
                                                   $"?client_id={CognitoAppClientId}" +
                                                   $"&response_type=code" +
                                                   $"&redirect_uri={AuthRedirectUrl}";
    public static readonly string CognitoTokenExchangeUrl = $"{CognitoRootUrl}/oauth2/token";
    public static readonly string AppSyncEndpoint = $"https://bphzh2xzsrhklp5hobkkvlgq3i.appsync-api.eu-west-1.amazonaws.com/graphql";
}