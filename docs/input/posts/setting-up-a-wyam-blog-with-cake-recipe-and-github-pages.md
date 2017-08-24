Title: Setting up a Wyam blog with Cake.Recipe and GitHub pages
Published: 24/8/2017
Tags: Cake, Cake.Recipe, Recipe, Build, Preview, Wyam, GitHub, Pages, Blog
---
I first saw Wyam in action when I was working on the Cake.Recipe blog series, and was so impressed I decided to move the entire blog to it.
This post is going to document that process for others to follow.

# Setting up the repositories

You'll need to setup a pages repository. You can follow the instructions [on GitHub](https://pages.github.com/) for how to setup this repository. For personal pages the master branch must be the one that contains the output from the Wyam build. For project pages you can have the master branch for the code, and use a `gh-pages` for the site. Either way you need to create an orphaned branch at this point. Ensure both are completely empty to start off with. Since I'm creating a personal page, my new branch will be called develop (this is so cake will still generate the documentation)
```
git checkout --orphan develop
```
Do an initial commit on each branch at this point. Switch to the main branch and add a folder called docs. The majority of our work will be in this branch

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
                            repositoryName: "wozzo.github.io",
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

Cake.Recipe has everything setup with either parameters, or Environment variables. The credentials and settings for most of what we'll need is done through environment variables.
Log in to AppVeyor and add a new project, select your \<username\>.github.io repository.
If you're doing a personal page then change your appveyor.yml file so that the master branch isn't on the whitelist and add the main branch is. You'll also need to change the target task to the Publish-Documentation task, since we're not building a solution at all.

```
#---------------------------------#
#  Build Script                   #
#---------------------------------#
build_script:
  - ps: .\build.ps1 -Target Publish-Documentation

# Tests
test: off

#---------------------------------#
#        Branches to build        #
#---------------------------------#
branches:
  # Whitelist
  only:
    - main
    - dev
    - /release/.*/
    - /hotfix/.*/

#---------------------------------#
#  Build Cache                    #
#---------------------------------#
cache:
- Tools -> build.ps1
```

## Environment Variables

Go to github and create a new access token and add this to your appveyor project with the name `WYAM_ACCESS_TOKEN`. Copy the link to the repository as well and add that as the `WYAM_DEPLOY_REMOTE` environment variable. The last one is the branch `WYAM_DEPLOY_BRANCH`, which should either by gh-pages or master
