Title: ASP.NET Core Configuration
Published: 24/4/2016
Tags:
- CSharp
- C#
- ASP.NET
- ASP.NET Core
RedirectFrom: asp-net-core-configuration
---
Only just beginning to dive into ASP.NET Core (or ASP.NET 5 if you prefer), but there's a great deal to like. Configuration in Core is extremely simple. Values can simply be added to a json (or other format, but why bother) configuration file and are then available to your application where Configuration is instantiated.
The default projects will include the following lines in the Startup method of Startup.cs.

```csharp
var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
Configuration = builder.Build().ReloadOnChanged("appsettings.json");
```
This will load the values from the appsettings.json file (if it exists) into the Configuration property. Multiple calls to this method can be chained together (see environment specific config below). If appsettings.json looked like this

```json
{
    "Logging": {
        "IncludeScopes": false,
        "LogLevel": {
            "Default": "Verbose",
            "System": "Information",
            "Microsoft": "Information"
        }
    },
    "MyConfigValue": "An important configuration string"
}
```

The logging stuff is default, I've added the MyConfigValue key, which means that once Configuration is instantiated I can then access it's value within the Startup class with the following:

```csharp
Configuration["MyConfigValue"];
```

The Logging.LogLevel.Default value can be retrieved using

```csharp
Configuration["Logging:LogLevel:Default"];
```

By creating config classes specific to different areas of your application you can use the built in DI to inject that config only where it's needed.

If we replace MyConfigValue in the appsettings.json file above with the following key

```json
"MyConfig": {
    "Value1": "This is the first config value",
    "Value2": "And this is the second",
    "Number1": 15
}
```
Now add a new class to the project which can be called anything but I'm going with MyConfig.cs

```csharp
public class MyConfig
{
    public string Value1 { get; set; }
    public string Value2 { get; set; }
    public int Number1 { get; set; }
}
```
As you can see the properties match up. We now need to configure the options to be populated into a MyConfig object. Back to startup.cs and find the ConfigureServices method. Add the following lines

```csharp
services.AddOptions();
services.Configure<MyConfig>(Configuration.GetSection("MyConfig");
```

This takes in the Configuration property and configures it so that when we ask for a MyConfig object it will provide it using whatever it has read from appsettings.json file by matching the classes properties to the keys with the same name. To see that in use create a new controller and add a constructor passing in IOptions as a parameter. This will provide an accessor to a populated config object. which is then accessible in the constructor and can be assigned as a property on that controller.

```csharp
public MyController(IOptions<MyConfig> myConfig)
{
    _myConfig = myConfig.Value;
}
```

The _myConfig object will now contain our config for this section.

# Environment specific config

You can easily add additional config files for each environment by calling AddJsonFile on the ConfigurationBuilder multiple times. Since I wrote this the default configure method has the following code.

```csharp
var builder = new ConfigurationBuilder()
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
```

The later files take priority over the earlier ones, overwriting any earlier values. See this page for info on how to set the EnvironmentName. I also recommend adding *.development.json to your .gitignore files so that your development environment configuration isn't committed.