Title: Seeding data using Entity Framework in ASP.NET Core
Published: 17/9/2016
Tags:
- CSharp
- C#
- Entity Framework
- EF
- ASP.NET
- ASP.NET Core
- SQL
RedirectFrom: seeding-data-using-entity-framework-in-asp-net-core
---
With thanks to [coderhints.com](http://coderhints.com/ef7-seed-user/) for the initial steps in doing this. I've done a bit of refactoring and added some additional steps to enable setting up a lot of data quickly and easily.

I'm going to demonstrate how you can set up some default information in your database using Entity Framework, and a default user for your application. Firstly find the ConfigureServices method in the Startup class. This is the first way my approach differs from above; ConfigureServices is where the DbContext is added to the application and it's where it should be setup, while Configure is where you tell the application how to respond to HTTP requests [1]. At the end of this method add the following code.

```csharp
var serviceProvider = services.BuildServiceProvider();
var dbContext = serviceProvider.GetService<ApplicationDbContext>();
var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
dbContext.Database.Migrate();
dbContext.EnsureSeedData(userManager, roleManager).Wait();
```

The serviceProvider is what allows us to retrieve instances of the services we need, in this case the ApplicationDbContext, UserManager and RoleManager. Calling the migrate method ensures we have applied the latest migrations and are working with an up to date schema. Finally that horrible red line there is the method where we're going to our seeding. So flick over to your ApplicationDbContext class and add a new method with the following signature

```csharp
public async Task EnsureSeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
```

This method needs to be rerunnable without throwing exceptions for adding repeat items, and nor do you want that, so every call you make that's going to update the database should have a check to make sure that data doesn't already exist.

# Adding a default user

We're going to start by setting up our default 'Administrator' user. To do that we need to have an Administrator role.

```csharp
// Add roles
if (!await Roles.AnyAsync(r => string.Equals(r.Name, "Administrator", System.StringComparison.OrdinalIgnoreCase)))
    await roleManager.CreateAsync(new IdentityRole("Administrator"));
```

Here we check for the roles existence and if it isn't there, we add it. This should be repeated for any roles you require.
Now to create our new user

```csharp
// Add Admin user
var adminUser = userManager.Users.FirstOrDefault(u => string.Equals(u.UserName, "admin@wbates.net", System.StringComparison.OrdinalIgnoreCase));
if (adminUser == null)
{
    adminUser = new ApplicationUser
    {
        UserName = "admin@wbates.net",
        Email = "admin@wbates.net"
    };
    var result = await userManager.CreateAsync(adminUser, "AReallyDifficultImpossibleToGuessPassword123#");
    if (result != IdentityResult.Success)
        throw new System.Exception($"Unable to create '{adminUser.UserName}' account: {result}");
}
```

As always, check for the users existence and because we'll be wanting to ensure this user is enabled and has the 'Administrator' role we'll keep a reference to it. It's worth noting that no Exception is normally thrown if the user has not been created, instead the object returned from the CreateAsync call will tell us and give a message if it has failed. The most common reason for this is that the password is not complicated enough, hence the '#' on the password above.

```csharp
await userManager.SetLockoutEnabledAsync(adminUser, false);
```

Well we wouldn't want our administrator to be locked out of the system would we?

```csharp
// Check AdminUserRoles
var adminRoles = await userManager.GetRolesAsync(adminUser);
if (!adminRoles.Any(r => string.Equals(r, "Administrator")))
    await userManager.AddToRoleAsync(adminUser, "Administrator");
```

Finally we check that the user is in the admin role, and we're done with setting up our default user. Check the previous post for how ASP.NET Core configuration allows us to set some of the above defaults using .json files instead of hard coding them as we have here.

# Adding seed data

Assuming you've added additional DbSets to your ApplicationDbContext for your models, they'll be available in the EnsureSeedData method. As above you'll want to check for each values existing before adding it, but otherwise create an instance of the model and .Add it to the relevant DbSet. The changes won't be saved until you call SaveChangesAsync and it has completed, so leave that until the end. Here's a quick sample that shows how to add some records and save the changes to finish.

```csharp
// Ensure default MediaTypes
if (!MediaTypes.Any(t => string.Equals(t.Name, "DVD", System.StringComparison.OrdinalIgnoreCase)))
{
    var mediaType = new MediaType { Name = "DVD", Description = "Digital Versatile Disc" }
    MediaTypes.Add(mediaType);
}
await SaveChangesAsync();
```

# Top tips

Use ASP.NET's Configuration Options[2] to describe default values and allow them to be set by using .json files for different environments.
Move most of this additional configuration out to an extension method on the IServicesCollection. Keep the configure method clean.
Define the roles as auto properties in a static class so you don't need to use strings like “Administrator” to reference them everywhere