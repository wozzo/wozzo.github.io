Title: Testing a Cake Addin
Published: 21/8/2017
Tags:
- Cake
- Addin
- Build
- GitHub
- Bower
- Cake.Bower
- Testing
- Unit
- xUnit
- Shouldly
---

Tasting the cake would be the obvious pun here, but hopefully by now you've gotten stuck in and had more than a slice. Today I'm going to walkthrough setting up some simple unit tests on the Markdown-Pdf addin I wrote in [part 1](http://wbates.net/baking-a-cake-addin/).
In your solution add a new .Net framework class library project. Add the Cake.Testing nuget package to this solution along with your testing framework of choice. I'll be using [xUnit](https://xunit.github.io/) along with the [Shouldly](https://github.com/shouldly/shouldly) assertion package.

## Fixture
Create a new class for your test fixture called MarkdownPdfRunnerFixture. This class should inherit from Cake.Testing's ToolFixture\<TSetting\> where TSetting is the settings class from the addin that we want to test.
Add a constructor that calls the base constructor passing in a string with the tool's name.
```csharp
public MarkdownPdfFixture() : base("markdown-pdf") { }
```
then add a property which will hold an action for configuring the settings
```csharp
public Action<MarkdownPdfRunnerSettings> RunnerSettings;
```
Finally add override the RunTool method and use it to instantiate a runner and call its Run method
```csharp
protected override void RunTool()
{
    var tool = new MarkdownPdfRunner(FileSystem, Environment, ProcessRunner, Tools);
    tool.Run(RunnerSettings);
}
```
That's the fixture complete. We're going to try various settings out and then call the Run() method on the fixture, which will in turn call the RunTool method we just created and will return a [ToolFixtureResult](http://cakebuild.net/api/Cake.Testing.Fixtures/ToolFixtureResult/) to us. We'll be able to query this result and check that the correct arguments were sent to the tool.

## Tests
I'm not going to go through each test I'll write for this class. You can check the code [here](https://github.com/wozzo/Cake_Addin_Blog_Posts/tree/master/Part%202) if you want to see them all. I'll just showcase a couple of examples.
In the first test the configurator is null, so default settings should be used. In this case that means that no arguments are to get passed to the markdown-pdf tool so the Args should be empty.
```csharp
[Fact]
public void No_Settings_Should_Use_Correct_Argument_Provided_In_MarkdownPdfRunnerSettings()
{
    fixture.RunnerSettings = null;
    var result = fixture.Run();
    result.Args.ShouldBe("");
}
```
Here we use the WithFilePath method and pass in a test string. Because the desired action here is to pass that string as the only argument that's what we check for
```csharp
[Fact]
public void WithFilePath_Settings_Should_Use_Correct_Argument_Provided_In_MarkdownPdfRunnerSettings()
{
    fixture.RunnerSettings = s => s.WithFilePath(TestFilePath);
    var result = fixture.Run();
    result.Args.ShouldBe(TestFilePath);
}
```
The WithHelp() method should use the --help switch
```csharp
[Fact]
public void WithHelp_Settings_Should_Use_Correct_Argument_Provided_In_MarkdownPdfRunnerSettings()
{
    fixture.RunnerSettings = s => s.WithHelp();
    var result = fixture.Run();
    result.Args.ShouldBe("--help");
}
```
Finally a combination test that checks that all the correct settings are applied and in the correct order. 
```csharp
[Fact]
public void WithFilePath_And_CssFilePath_Settings_Should_Use_Correct_Argument_Provided_In_MarkdownPdfRunnerSettings()
{
    fixture.RunnerSettings = s => s.WithFilePath(TestFilePath).WithCssPath(TestCssFilePath);
    var result = fixture.Run();
    result.Args.ShouldBe($"--css-path {TestCssFilePath} {TestFilePath}");
}
```

## Run the tests
Hopefully you've added a cake build script to your project by now, in which case you can add the following to it to get cake to run your unit tests as part of a task
```csharp
#tool "xunit.runner.console"
...
var testResultsPath = MakeAbsolute(Directory(artifacts + "./test-results"));
var testAssemblies = new List<FilePath> { MakeAbsolute(File("./src/Cake.Markdown-Pdf.Tests/bin/" + configuration + "/Cake.Markdown-Pdf.Tests.dll")) };

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    CreateDirectory(testResultsPath);

    var settings = new XUnit2Settings {
        XmlReportV1 = true,
        NoAppDomain = true,
        OutputDirectory = testResultsPath,
    };
    settings.ExcludeTrait("Category", "Integration");
    
    XUnit2(testAssemblies, settings);
});
```

Happy testing :)