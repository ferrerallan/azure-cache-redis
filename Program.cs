using Modes;
using Newtonsoft.Json;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
var app = builder.Build();
var connectionString = "allan-redis.redis.cache.windows.net:6380,password=sVjkHDdco6aE1vtGjPHaXNLWtlrMqRjUEAzCaJE5F5g=,ssl=True,abortConnect=False";
var redisConnection = ConnectionMultiplexer.Connect(connectionString);
IDatabase db = redisConnection.GetDatabase();

app.MapGet("/", () => "Hello World!");

app.MapGet("/list", () =>
{
    
    //bool wasSet = db.StringSet("favorite:flavor", "i-love-rocky-road");
    string value = db.StringGet("guitars");
    return value;
});

app.MapGet("/list-countries", () =>
{
    
    //bool wasSet = db.StringSet("favorite:flavor", "i-love-rocky-road");
    string value = db.StringGet("countries");
    return value;
});

app.MapPost("/create", (Guitar guitar) =>
{
    string value = db.StringGet("guitars");
    var guitars = JsonConvert.DeserializeObject<List<Guitar>>(value);
    guitars.Add(guitar);
    string serializedValue = JsonConvert.SerializeObject(guitars);
    bool added = db.StringSet("guitars", serializedValue);
    return guitars;
});

app.MapPost("/build", () =>
{

    var stat = new List<Guitar>(){
        new Guitar(){
            name = "Gibson L-5 CES",
            brand = "Gibson",
            price = 899.99d
        },
        new Guitar(){
            name = "Line 6 JTV-59",
            brand = "Line 6",
            price = 899.99d
        }
    };    

    string serializedValue = JsonConvert.SerializeObject(stat);
    bool added = db.StringSet("guitars", serializedValue);
});


app.MapPost("/build-countries", async (IHttpClientFactory httpClientFactory) =>
{

     var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            "https://restcountries.com/v3.1/all");

    var httpClient = httpClientFactory.CreateClient();
    var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

    if (httpResponseMessage.IsSuccessStatusCode)
    {
        using var contentStream =
            await httpResponseMessage.Content.ReadAsStreamAsync();
        
        var stringContent = await new StreamReader(contentStream).ReadToEndAsync();
        var countries = JsonConvert.DeserializeObject<List<Country>>(stringContent);

        string serializedValue = JsonConvert.SerializeObject(countries);
        bool added = db.StringSet("countries", serializedValue);
    }

    
});

app.Run();
