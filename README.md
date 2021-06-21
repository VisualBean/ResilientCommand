[![.Net](https://github.com/VisualBean/ResilientCommand/actions/workflows/dotnet.yml/badge.svg)](https://github.com/VisualBean/ResilientCommand/actions/workflows/dotnet.yml) [![NuGet version](https://badge.fury.io/nu/ResilientCommand.svg)](https://badge.fury.io/nu/ResilientCommand)

# ResilientCommand
A Resiliency library for wrapping dependency calls. Heavily inspired by Hystrix

The idea is simple; protect against up/down- stream problems, by encapsulating calls to these dependencies in reliability patterns.
This project seeks to do just that. 

There is a Getting-Started Guide [here](https://github.com/VisualBean/ResilientCommand/wiki/Getting-Started).   
Please check the [wiki](https://github.com/VisualBean/ResilientCommand/wiki) for more information
or the [examples](https://github.com/VisualBean/ResilientCommand/tree/main/ResilientCommand.Examples) some usage examples.

# Getting started
## Step 1:
Download the NuGet package [here](https://www.nuget.org/packages/ResilientCommand/)

## Step 2:
Either add your commands as singletons/scoped yourself or use the helper
``` csharp
services.AddResilientCommands(typeof(Startup));
```

The above will look through the assembly for all Resilient Commands.

## Step 3:
Create a command for your specific scenario (in this particular example we are using the [HttpClientFactory dependency injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#consumption-patterns)

``` csharp
class GetUsersCommand : ResilientCommand<IEnumerable<User>>
{
    private readonly HttpClient client;

    public GetUsersCommand(HttpClient client)
    {
        this.client = client;
    }
    protected override async Task<IEnumerable<User>> RunAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync("api/users");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<IEnumerable<User>>(content);
    }
}
```

## Step 4:
Inject and use your command.
``` csharp
public class MyService()
{
    private UsersCommand usersCommand;

    public MyService(UsersCommand usersCommand)
    {
        this.usersCommand = usersCommand;
    }

    public async Task<IEnumerable<Users>>GetUsers(CancellationToken cancellationToken)
    {
        return await this.usersCommand.ExecuteAsync(cancellationToken);
    }
}
```
