![.Net](https://github.com/VisualBean/ResilientCommand/workflows/.Net/badge.svg)

# ResilientCommand
A Resiliency library for wrapping dependency calls. Heavily inspired by Hystrix

The idea is simple; protect against up/down- stream problems, by encapsulating calls to these dependencies in reliability patterns.
This project seeks to do just that. 

# Basic usage
Lets take a basic example.  
Lets say we run a "IsItUp" service. The idea is that people call you to check whether some website or service is up or down.

``` csharp
// The IsUpResult class.
class IsUpResult
{
    private readonly string site;
    private readonly bool isUp;

    public IsUpResult(string site, bool isUp)
    {
        this.site = site;
        this.isUp = isUp;
    }

    public string Site => this.site;

    public bool IsUp => this.isUp;

    public override int GetHashCode()
    {
        return this.site.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj as IsUpResult);
    }

    public bool Equals(IsUpResult other)
    {
        return this.site == other.Site && this.isUp == other.IsUp;
    }

}

// Our command implementation.
class IsItUpCommand : ResilientCommand<IsUpResult>
{
    private readonly string site;
    private readonly HttpClient client;

    public IsItUpCommand(string site)
    {
        this.site = site;
        this.client = new HttpClient();
    }

    protected override async Task<IsUpResult> RunAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(this.site);
        response.EnsureSuccessStatusCode(); // Throws if non-success.

        return new IsUpResult(this.site, true);
    }

    protected override IsUpResult Fallback()
    {
        return new IsUpResult(this.site, false);
    }
}
```

The above would call the site, and in case of any problems, we would use the fallback as a result instead of throwing exceptions.  
The above example is ofcourse very trivial, as this could also simply be handled with a `try/catch => new IsUpResult(this.site, false);`
but hopefully the idea comes across.

# Features

Out of the box, all concrete `ResilientCommand`s have timeout and circuit breakers enabled, with 'somewhat' sane defaults.
## Currently supported:

 |Feature | Description| Enabled By Default |
 |-------|-----------|----------------|------------------|
 | Timeout | If a command runs longer than x, we cancel it. | true |
 | CircuitBreaker | Rolling window of errors, if circuit is broken we resort to fallback. | true |
 | Fallback | A default value to return if the command fails. | false |
 | Semaphore | A way of minimising max parallelism per command. | true |
 | Response Caching | Cache the result based on `CacheKey`. | false |
 | Collapsing | Collapse calls to the same command, within a window, so only one gets called. | false |
 | Grouping | Commands are grouped based on `CommandKey`. | true |
 | Notifications | A somewhat simple event system. | N/A | N/A |

## Potential future support:
- [ ] Ability to inject a cache, instead of having this in the commands themselves.

## CommandKey

`CommandKey` is a way for the `ResilientCommand` to both cache circuitbreakers, but also helps with cachekeys and groupings in general.  

_Note: If no `CommandKey` is supplied, it defaults to `GetType().Name` which is the inheriting class' name._

## Caching

the response cache can be enabled by overriding `GetCacheKey()` which will cause subsequent calls to `ExecuteAsync()` to get the result from the cache.

_Note: Responses are cached per `CommandKey`._

## Semaphore

The semaphore enables us to limit the amount of parallisme that command can have at any given point.  
The semaphore is controlled through the `MaxParallelism` integer in `CommandConfiguration`.  
_Note: Semaphores work per `CommandKey`._  

### Configuration - defaults
``` csharp
CommandConfiguration.CreateConfiguration(
config =>
{
    config.MaxParallelism = 10;
});
```

## Timeout

The timeout makes sure to cancel the current execution if we pass the timeout limit.  

### Configuration - defaults
``` csharp
CommandConfiguration.CreateConfiguration(
config =>
{
    config.ExecutionTimeoutSettings = new ExecutionTimeoutSettings(
    isEnabled: true,
    executionTimeoutInMiliseconds: 10000);
});
```

## CircuitBreaker

The circuit breaker works by looking at a rolling window of errors, and if we get above the configured `failureThreshold` we open the circuit for `durationMiliseconds` until we allow to try again.

### Configuration - defaults
```csharp
CommandConfiguration.CreateConfiguration(
config =>
{
    config.CircuitBreakerSettings = new CircuitBreakerSettings(
      isEnabled: true, 
      failureThreshhold: 0.5, 
      samplingDurationMiliseconds: 10000, 
      minimumThroughput: 20, 
      durationMiliseconds: 5000);
});
```

## Fallback

The fallback can be thought of as a backup value in case of a failure from the dependency.    
The idea is that the fallback will be returned and exceptions swallowed, not causing an outage.  
A fallback can be enabled by overriding `Fallback()` which will cause any exceptions that are thrown to be handled and return the fallback value.  
The fallback value is ment to be a static or at least local value.  

Fallback can be disabled through configuration. If disabled, exceptions will fall through and bubble up.  

_Note: If `Fallback()` has not been overridden nor disabled, the command will throw a `FallbackNotImplementedException` exception._  

### Configuration - defaults
``` csharp 
CommandConfiguration.CreateConfiguration(
config => 
{
    config.FallbackEnabled = true;
});
```
## Collapsing
**Disabled by default**  

Collapsing of requests is the same as 'Debouncing'.   
Simply, only make a single call to a dependency within a given timeframe,   
if multiple calls are being made.  

_Note:_ Commands that enable this should be stateless, as only the first request will go through. Subsequent requests will use the results of the first one.  

### Configuration - defaults
``` csharp 
CommandConfiguration.CreateConfiguration(
config => 
{
    config.CollapserSettings = new CollapserSettings 
    { 
        IsEnabled = false,
        Window = Timespan.FromMilliseconds(100),
     }
});
```


## ResilientCommandNotifier


`ResilientCommandNotifier` is a way to keep tabs on which events has been run as part of the current execution.  
The notifier is set through the singleton factory `EventNotifierFactory`. Only a single notifier is currently supported at a time.

### Example
``` csharp
public class ConsoleEventNotifier : ResilientCommandEventNotifier
{
    public override void MarkEvent(ResillientCommandEventType eventType, CommandKey commandKey)
    {
        Console.WriteLine($"{commandKey}: {eventType}");
    }
}

...
// In startup or whichever entrypoint you are using.
EventNotifierFactory.GetInstance().SetEventNotifier(new ConsoleEventNotifier());
...
```
_Note: The default implementation doesnt do anything. You will have to create a new implementation and set the notifier_

## Configuration

Most features can be configured and/or disabled through the configuration.  
Configuration is injected in the constructor.

### Example
``` csharp
class BasicCommand : ResilientCommand<string>
{
    public BasicCommand() : base(
        configuration: CommandConfiguration.CreateConfiguration(
            config => 
            {
                // all config is here.
            }))
    {
    }
    
    protected override Task<string> RunAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult("MyCommand");
    }
}
```

## Examples

Please checkout the [examples](https://github.com/VisualBean/ResilientCommand/tree/main/ResilientCommand.Example) for basic usage.
