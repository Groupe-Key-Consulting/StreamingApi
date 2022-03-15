using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

var userNames = new[]
{
    "Zachariah Mcmillan", "Khloe Short", "Summer Carter", "Wyatt Guerra", "Londyn Small", "Heidi Gentry", "Anastasia Mora", "Elsie Preston", "Kate Love", "Nathen Leon", "Kamari George", "Nyasia Pope", "Moriah Riggs", "Addisyn Dalton", "Simone Carr", "Jacqueline Cross", "Moses Roman", "Jamal Hawkins", "Alex Obrien", "Anton Porter"
};

app.MapGet("stream/users", GetAllUsers);

app.Run();

/// <summary>
/// Writes all available users progressively on the http response,
/// without consuming excessive memory on the server
/// </summary>
async Task GetAllUsers(HttpResponse response, CancellationToken cancellationToken)
{
    var countUsers = 0; // Number of users added to the buffer
    var isFirstUser = true;

    try
    {
        response.ContentType = "application/json";
        await response.WriteAsync("[\n", cancellationToken);

        foreach (var user in ReadAllUsers())
        {
            if (!isFirstUser)
                await response.WriteAsync($",\n", cancellationToken);

            // CancellationToken is very useful here to stop the computations if the client ends up the request by closing the browser tab for example.
            // This will prevent the server to do unnecessary computations
            await response.WriteAsync("\t" + JsonSerializer.Serialize(user), cancellationToken);
            countUsers++;

            isFirstUser = false;
        }

        await response.WriteAsync("\n]", cancellationToken);
    }
    catch (OperationCanceledException oce)
    {
        Console.Error.WriteLine(oce.Message);
    }
}

IEnumerable<AppUser> ReadAllUsers()
{
    const int dbSize = 50; // The number of users to generate. Increase it to million and see how server's memory evols
    const int delay = 500; // Time to wait between two batches

    for (var i = 1; i <= dbSize; i++)
    {
        Console.WriteLine($"{i}");

        yield return new AppUser
        (
            i,
            userNames[Random.Shared.Next(userNames.Length)],
            DateTime.Now.AddDays(i)
        );

        Thread.Sleep(delay); // Simulates a delay reading data
    }
}

internal record AppUser(long Id, string FullName, DateTime BirthDate)
{
    public string Email => $"{FullName.ToLower().Replace(" ", ".")}@streaming.com";
}