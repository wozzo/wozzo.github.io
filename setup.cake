#load nuget:https://www.myget.org/F/cake-contrib/api/v2?package=Cake.Recipe&prerelease

Environment.SetVariableNames();

BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./src",
                            title: "Wozzo.Blog",
                            repositoryOwner: "wozzo",
                            repositoryName: "wozzo.github.io",
                            appVeyorAccountName: "wozzo",
                            wyamRecipe: "Blog",
                            wyamTheme: "CleanBlog",
                            webLinkRoot: "/");

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context);

var siteUrl = new System.Uri(Argument("url", "http://wbates.net/"));

Task("Tweet-New-Blog-Post")
	.IsDependentOn("Force-Publish-Documentation")
    .WithCriteria(() => BuildParameters.ShouldGenerateDocumentation)
    .WithCriteria(() => DirectoryExists(BuildParameters.WyamRootDirectoryPath))
	.WithCriteria(() => BuildParameters.CanPostToTwitter && BuildParameters.ShouldPostToTwitter)
    .Does(() => { 
		// Check to see if there are any new posts
		var sourceCommit = GitLogTip("./");
		var filesChanged = GitDiff("./", sourceCommit.Sha);
		var newPosts = false;

		var wyamDocsFolderDirectoryName = BuildParameters.WyamRootDirectoryPath.GetDirectoryName();
		var wyamPostsPath = System.IO.Path.Combine(wyamDocsFolderDirectoryName, "input\\posts");
		Information("Posts {0}", wyamPostsPath);
		foreach(var file in filesChanged.Where(x =>  
			x.Path.StartsWith(wyamPostsPath, StringComparison.OrdinalIgnoreCase)
			&& !x.OldExists))
		{
			Information("New post found at: {0}", file.Path);
			var uri = new System.Uri(siteUrl, "posts/" + System.IO.Path.GetFileNameWithoutExtension(file.Path));
			Information("Post url: {0}", uri);

			string title = null;
			using(System.IO.StreamReader reader = new System.IO.StreamReader(file.Path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (line.StartsWith("Title: ", StringComparison.OrdinalIgnoreCase))
						title = line.Replace("Title: ", "");
					if (string.Equals(line, "---", StringComparison.OrdinalIgnoreCase))
						break;
				}
			}

			if (string.IsNullOrWhiteSpace(title))
				title = System.IO.Path.GetFileNameWithoutExtension(file.Path);
			Information("Post title: {0}", title);

			SendMessageToTwitter(string.Format("New blog post: {0}. {1}", title, uri));
		}
	});

Task("Build-Blog")
	.IsDependentOn("Force-Publish-Documentation")
	.IsDependentOn("Tweet-New-Blog-Post");

Build.RunNuGet();