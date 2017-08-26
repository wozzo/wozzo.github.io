Title: Testing a Cake Addin
Published: 26/8/2017
Tags:
- Cake
- Build
- GitHub
- Cake.Recipe
- Recipe
---
I’ve been playing around with the Cake build tool for a little while now, but having recently had need of a plugin that didn’t yet exist I embarked on a journey that lead me to Cake.Recipe.

> Cake.Recipe is a set of convention based Cake scripts.

If you check out the Content folder in that repository you’ll find a host of pre-written cake scripts that cover a wide range of use cases. They’ve been designed so that they all work together and can be called through a single cake file that contains a few bits of setup. They’re used by most of the addin’s under the cake-contrib team. The only thing that was lacking at the start of this journey was documentation about how to use Cake.Recipe, which is why I’m writing this post.

# Getting started
Before I knew what Cake.Recipe was I was looking around various repositories looking for good ideas for how to improve the addin I was working on at the time (Cake.Bower), and I started to notice that several of the repositories were missing a build.cake file. The .cake file can of course be called whatever you like, but by default it is called build.cake, so for this to be missing, or rather called setup.cake, I found a bit odd.
Looking into the setup.cake file I found something very different from what I’m used to seeing in build.cake files.

```csharp
#load nuget:https://www.myget.org/F/cake-contrib/api/v2?package=Cake.Recipe&prerelease
 
Environment.SetVariableNames();
 
BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./",
                            title: "Cake.Bower",
                            repositoryOwner: "cake-contrib",
                            repositoryName: "Cake.Bower",
                            appVeyorAccountName: "cakecontrib");
 
BuildParameters.PrintParameters(Context);
 
ToolSettings.SetToolSettings(context: Context,
                            dupFinderExcludePattern: new string[] {
                                BuildParameters.RootDirectoryPath + "/src/Cake.Bower.Tests/*.cs" },
                            testCoverageFilter: "+[*]* -[xunit.*]* -[Cake.Core]* -[Cake.Testing]* -[*.Tests]* ",
                            testCoverageExcludeByAttribute: "*.ExcludeFromCodeCoverage*",
                            testCoverageExcludeByFile: "*/*Designer.cs;*/*.g.cs;*/*.g.i.cs");
Build.Run();
```

No tasks. None at all.

The top line is the critical one. It fetches the Cake.Recipe scripts and loads each one providing the methods you can see being used here. The bootstrapped build.ps1 that you’d normally download if you followed the instructions on the “Setting up a new project” tutorial page has been modified to use a default file of setup.cake purely as a convention to distinguish between normal cake builds and recipe builds. You can then modify the parameters in the setup.cake file to achieve the desired result. There are close to 80 different parameters that can be set here that can be used to configure the ~40 odd tasks that cake.recipe provides.

# Parameters
Take a look at the parameters.cake file to see what options are available to you out of the box. These can all be set either in the SetParameters or using environment variables.
Tasks
The tasks.cake file paradoxically doesn’t contain any tasks, but does give you a handy list of tasks that are configured. It also provides you with the first way of overriding cake.recipe functionality by changing the value assigned to these properties, should one of the tasks need tweaking to suit your needs. More on that in a bit.
The tasks are set in a cake file specific to the purpose of the task, i.e. the tasks related to testing are all in testing.cake, the tasks for code analysis are in analyzing.cake, etc. The best starting point for figuring out what is going is in the build.cake file, currently on line 354

```csharp
BuildParameters.Tasks.DefaultTask = Task("Default")
        .IsDependentOn("Package");
```

We can start to see the intended targets for a full build. Don’t get thrown by the lack of dependencies here. For example the Package task target doesn’t only depend on the Export-Release-Notes task. Look above the task definition and find the SetupTasks method. In there you can see more dependencies being set.

This should start to give you an idea what will happen when you run one of the targets listed here. The default is probably the best place to start. If you need a safe project to test it out on, fork Cake.Bower and run .\build.ps1. It will take a while on the first run as it has to download all of the tools and addins required to run.

# Cake.Bower build running
The tasks that are run by the default target are

```
Task                                 Duration
---------------------------------------------------------
Export-Release-Notes                 Skipped
Show-Info                            00:00:00.0093039
Print-AppVeyor-Environment-Variables Skipped
Clean                                00:00:00.0292549
Restore                              00:00:01.0742619
Build                                00:00:05.7580859
DupFinder                            00:00:09.2430267
InspectCode                          00:00:22.4277417
Analyze                              00:00:00.0042806
Install-ReportGenerator              00:00:02.2837979
Install-ReportUnit                   00:00:02.2187105
Install-OpenCover                    00:00:02.2890312
Test-NUnit                           Skipped
Test-xUnit                           00:00:06.9958299
Test-MSTest                          Skipped
Test-VSTest                          Skipped
Test-Fixie                           Skipped
Test                                 00:00:00.0065763
Create-NuGet-Packages                00:00:00.7237132
Create-Chocolatey-Packages           Skipped
Package                              00:00:00.0050486
Default                              00:00:00.0055089
---------------------------------------------------------
Total:                               00:00:53.0741721
```
You can see some of the tasks were skipped. This means the task appeared in the list of dependencies but the criteria to run it (defined using the `WithCriteria` method) were not met. For example the Test-NUnit task will only run if NUnit tests are found. There are none in Cake.Bower so it gets skipped. If I wanted to always skip a step I can find the parameter that governs whether it runs and set it in the setup.cake file. For example if I wanted to disable the InspectCode step I would change my setup.cake’s call to BuildParameters.SetParameters to the following.

```csharp
BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./",
                            title: "Cake.Bower",
                            repositoryOwner: "cake-contrib",
                            repositoryName: "Cake.Bower",
                            appVeyorAccountName: "cakecontrib",
                            shouldRunInspectCode: false);
```

Run it again and this time the task will be skipped but the rest should be the same.

# Replacing a task
With Cake, once a Task has been created you can’t create another with the same name. Calling the .Does(…) method adds an additional action but doesn’t replace the existing one. If you want to replace the tasks actions with something completely different you first need to clear the existing actions. To clear the InspectCode actions use the following

```csharp
BuildParameters.Tasks.InspectCodeTask.Task.Actions.Clear();
```
I’m now free to call the .Does(…) method with the action I want it to undertake and be sure it is the only thing it will do, for example:

```csharp
BuildParameters.Tasks.InspectCodeTask.Does( () => Information("And now for something completely different..."));
```

Running .\build.ps1 now yields the following output when you get to the InspectCode task.

```
========================================
InspectCode
========================================
Executing task: InspectCode
And now for something completely different...
Finished executing task: InspectCode
```

You are also free to create your own tasks and add them as dependencies to the existing ones. Something like this…

```csharp
Task("MakeTea").Does( () => Information("Make Tea not Love"));
BuildParameters.Tasks.InspectCodeTask.Task.Actions.Clear();
BuildParameters.Tasks.InspectCodeTask
    .IsDependentOn("MakeTea")
    .Does( () => Information("And now for something completely different..."));
```
which yields…

```
========================================
MakeTea
========================================
Executing task: MakeTea
Make Tea not Love
Finished executing task: MakeTea
 
========================================
InspectCode
========================================
Executing task: InspectCode
And now for something completely different...
Finished executing task: InspectCode
```

And of course if you find that you need loads of additional scripts for a particular use case consider a pull request into Cake.Recipe