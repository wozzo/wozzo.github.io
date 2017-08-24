Title: Setting up a Wyam blog with Cake.Recipe and GitHub pages
Published: 24/8/2017
Tags: Cake, Cake.Recipe, Recipe, Build, Preview, Wyam, GitHub, Pages, Blog
---
I first saw Wyam in action when I was working on the Cake.Recipe blog series, and was so impressed I decided to move the entire blog to it.
This post is going to document that process for others to follow.

# Setting up the repositories

You'll need two repositories for this to work. One will be you github pages repo. You can follow the instructions [on GitHub](https://pages.github.com/) for how to setup this repository. For now it will remain empty as we're going to setup a second repository from where we will build the site and deploy it. 

You can call the second repository anything you like. I've called mine [Wozzo.Blog](XXXX). You should set it up as an empty repository. Add a folder called docs

# Starting with Wyam

To see how to get Wyam go to their [obtaining](https://wyam.io/docs/usage/obtaining) page.
Note: I had to add %appdata%/local/wyam to my path to allow powershell to run wyam.

# Creating your blog

Wyam uses recipes to setup the outline of your site quickly. In your command line interface of choice browse to the docs folder we created earlier and run the following command
```
wyam new --recipe blog
```
This will create a new folder called input and setup a couple of pages under there. Follow the next few steps to see what this blog will look like, then we'll start adding some content.

# Getting Cake.Recipe

Go to [Cake.Recipe](https://github.com/cake-contrib/Cake.Recipe) on GitHub and download the following files to the root of your blog repository.

* .appveyor.yml
* .gitignore
* build.ps1
* config.wyam
* GitReleaseManager.yaml
* setup.cake

Open up the `setup.cake` file in your favourite text editor and edit the parameters so that it reflects the name of your blog.

```csharp
BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            title: "Wozzo.Blog",
                            repositoryOwner: "wozzo",
                            repositoryName: "Wozzo.Blog",
                            appVeyorAccountName: "wozzo",
                            wyamRecipe: "Blog",
                            wyamTheme: "Phantom");
```
I've also added two additional parameters telling Wyam that it should use the blog recipe and Phantom theme.
At this point you should make your first commit. If you don't GitVersion will throw an exception when attempting to run the cake build.

# Preview

Time to see what it looks like. The cake.recipe Wyam script comes with a Preview task which will fire up a local web server, and run wyam with a watch on your files. This means you can view the site in your browser, edit your posts and the browser will refressh so you can see your changes immediately.

Run the following command in the root of your repository.

```
.\build.ps1 -target preview
```

The first run may take a while as it is going to get all the tools it requires, future runs should be much faster.
Look at the end of the build log and you should find a message like the following

```
Preview server listening on port 5080 and serving from path file:///.../wozzo
.blog/BuildArtifacts/Documentation with virtual directory Wozzo.Blog and LiveReload support
Watching paths(s) file:///.../content, theme, input
Watching configuration file file:///.../wozzo.blog/config.wyam
Hit any key to exit
```

The key parts are the port and virtual directory. Replace them in the following url in your browser.

```
http://localhost:<port>/<virtual directory>
e.g.
http://localhost:5080/Wozzo.Blog
```
The virtual directory is case sensitive by the way ;-)

![Wyam blog in the browser](./assets/images/wyam-in-browser.png)

Isn't it pretty. For some reason when I tried to use the SolidState theme I got exceptions due to missing files. If you get it working, please get in touch.

# AppVeyor

Wyam's site has some really great [instructions for setting up Wyam with AppVeyor](https://wyam.io/docs/deployment/appveyor), but we'll need to change a few things to keep our site working with Cake.Recipe.

Add the output folder to your git ignore, and commit all of your work so far. otherwise it will get deleted in the next step.