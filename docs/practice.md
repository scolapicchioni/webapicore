# Lab 06 - Identity Server

So far anyone can read, create, update and delete our products. 
What we would like to have is an application where:

- Any user can view the list of products
- Only authenticated users may add a new product

In order to achieve this, we have to set up an authentication and authorization system.

## Authentication / Authorization process

The flow that we're going to use will be as follows:

Instead of authenticating the user itself, the Client Application will redirect the user to an Authentication Server, that will provide the user the possibility to

- Register
- Login
- Logout

Once the user is logged on, the Authentication Server will provide a Token that will be used by the Client Application to access the API.

The Authorization System on the Service Application will then use the Token to understand if the User is authorized to perform the actions that the client is requesting and it will grant or deny access. 

## IdentityServer4

Our Authentication System will be a .NET Core Web Application that is going to use IdentityServer4, an Open Source OpenID Connect and OAuth 2.0 framework for .NET. 

IdentityServer4 offers:

- Authentication as a Service
    - Single sign-on (and out) over multiple application types, allowing for centralized login logic and workflow for all of your applications (web, native, mobile, services).
- Access Control for APIs
    - Issue access tokens for APIs for various types of clients, e.g. server to server, web applications, SPAs and native/mobile apps.
- Federation Gateway
    - Support for external identity providers like (Azure) Active Directory, Google, Facebook etc. This shields your applications from the details of how to connect to these external providers.
- Focus on Customization
    - The most important part - many aspects of IdentityServer can be customized to fit your needs. Since IdentityServer is a framework and not a boxed product or a SaaS, you can write code to adapt the system the way it makes sense for your scenarios.

We are going to follow a series of [Tutorials](https://identityserver4.readthedocs.io/en/release/quickstarts/0_overview.html), mixing and modifying the steps to suit our needs.

### Add A New Identity Server Project

- In the Solution Explorer, right click on the solution and select Add -> New Project
- Select "ASP.NET Core Web Application (.NET Core)"
- In the Name textbox, type IdentityServer
- In the "ASP.NET Core Web Application (.NET Core) - IdentityServer" window, be sure to 
    - Select ```ASP.NET Core 1.1```
    - Select the ```Web Application``` Template
    - Click on ```Change Authentication```
    - Select ```Individual User Accounts```

By default Visual Studio uses IIS Express to host your web project. This is totally fine, besides that you won’t be able to see the real time log output to the console.

IdentityServer makes extensive use of logging whereas the “visible” error message in the UI or returned to clients are deliberately vague.

It's recommended to run IdentityServer in the console host. You can do this by switching the launch profile in Visual Studio. You also don’t need to launch a browser every time you start IdentityServer - you can turn that off as well.

- In the Solution Explorer, right click your IdentityServer project and select Properties.
- In the Properties window, select Debug
- In the Debug tab, under Web Server Settings -> App URL, type ```http://localhost:5002/```
- In the Profile Dropdown, select IdentityServer
- Under Web Server Settings -> App URL, type ```http://localhost:5002/```

When you switch to self-hosting, the web server port defaults to 5000. 
You can configure this in Program.cs - we use the following configuration for the IdentityServer host:

```cs
public static void Main(string[] args) {
    var host = new WebHostBuilder()
        .UseKestrel()
        .UseUrls("http://localhost:5002")
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .UseStartup<Startup>()
        .UseApplicationInsights()
        .Build();

    host.Run();
}
```

Set the IdentityServer project as startup project, by right clicking on the IdentityServer project in the Solution Explorer and selecting Set As Startup Project.

### Add IdentityServer packages

In the Solution Explorer, right click on the IdentityServer Project / Dependencies and select ```Manage NuGet Packages```. Click on Browse and type ```identityserver4.aspnetidentity```. Select ```IdentityServer4.AspNetIdentity``` package and click Install.

IdentityServer is designed for extensibility, and one of the extensibility points is the storage mechanism used for data that IdentityServer needs. This tutorial shows to how configure IdentityServer to use EntityFramework (EF) as the storage mechanism for this data.

In the Solution Explorer, right click on the Identity Server Project / Dependencies and select ```Manage NuGet Packages```. Click on Browse and type ```identityserver4.entityframework```. Select ```IdentityServer4.EntityFramework``` package and click Install.

We are now going to configure our IdentityServer Framework. We need to specify:

- **ApiResources**: The WebService we want to protect
- **IdentityResources**: The identity information that we want to disclose to the client (such as UserID and Profile)
- **Clients**: The JavaScript Application that will request the Token and that will try to access the WebService
- **Users**: The humans using the Client Application

We will first create a class with static methods that will return the data we need, so that it will be easy to find and eventually modify all the necessary configuration. We will use data returned by its methods to seed the database that will persist the configuration.

### Add a Config Class

In the root of your IdentityServer project, add a new class named ```Config```.

### Resources

Resources are something you want to protect with IdentityServer - either identity data of your users, or APIs.

Every resource has a unique name - and clients use this name to specify to which resources they want to get access to.

Identity data are Identity information (aka claims) about a user, e.g. name or email address.

APIs resources represent functionality a client wants to invoke - typically modelled as Web APIs, but not necessarily.

Scopes define the resources in your system that you want to protect, e.g. APIs.

### Defining the API

All you need to do to add an API is to create an object of type ApiResource and set the appropriate properties.

In your Config class, add the following method:

```cs
///requires using IdentityServer4.Models;
public static IEnumerable<ApiResource> GetApiResources() {
    return new List<ApiResource>{
        //requires using IdentityModel;
        new ApiResource("MarketplaceService", "Marketplace Service" ,new [] { JwtClaimTypes.Name})
    };
}
```

### Defining Clients

Clients represent applications that can request tokens from your identityserver.

The details vary, but you typically define the following common settings for a client:

- a unique client ID
- a secret if needed
- the allowed interactions with the token service (called a grant type)
- a network location where identity and/or access token gets sent to (called a redirect URI)
- a list of scopes (aka resources) the client is allowed to access

A JavaScript client uses the so called *implicit flow* to request an identity and access token from JavaScript.

In your Config class, add the following method:

```cs
public static IEnumerable<Client> GetClients() {
    return new List<Client> {
        new Client {
            ClientId = "js",
            ClientName = "JavaScript Client",
            AllowedGrantTypes = GrantTypes.Implicit,
            AllowAccessTokensViaBrowser = true,

            RedirectUris =           { "http://localhost:5001/callback.html" },
            PostLogoutRedirectUris = { "http://localhost:5001/index.html" },
            AllowedCorsOrigins =     { "http://localhost:5001" },

            //requires using IdentityServer4;
            AllowedScopes = {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                "MarketplaceService"
            }
        }
    };
}
```

### Defining identity resources

Identity resources are data like user ID, name, or email address of a user. An identity resource has a unique name, and you can assign arbitrary claim types to it. These claims will then be included in the identity token for the user. The client will use the scope parameter to request access to an identity resource.

The OpenID Connect specification specifies a couple of standard identity resources. The minimum requirement is, that you provide support for emitting a unique ID for your users - also called the subject id. This is done by exposing the standard identity resource called **openid**.
The IdentityResources class supports all scopes defined in the specification (openid, email, profile, telephone, and address). 
If you want to support them all, you can add them to your list of supported identity resources.
We will expose **openid** and **profile**.

In your Config class, add the following method:

```cs
public static IEnumerable<IdentityResource> GetIdentityResources() {
    return new List<IdentityResource> {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile()
    };
}
```

### Defining users

A user is a human that is using a registered client to access resources.

In your Config class, add the following method:

```cs
//requires using IdentityServer.Models;
public static List<ApplicationUser> GetUsers() {
    return new List<ApplicationUser> {
        new ApplicationUser {
            UserName = "alice@gmail.com",
            Email = "alice@gmail.com"
        },
        new ApplicationUser {
            UserName = "bob@gmail.com",
            Email = "bob@gmail.com"
        }
    };
}
```

We will now proceed to add the IdentityServer framework to our application and configure it so that it uses EntityFramework for Users (thanks to Microsoft IdentityFramework), Resources and Clients.

### Configure

IdentityServer uses the usual pattern to configure and add services to an ASP.NET Core host. In ConfigureServices the required services are configured and added to the DI system. In Configure the middleware is added to the HTTP pipeline.

Modify your ConfigureServices method of the Startup.cs file to look like this:

```cs
public void ConfigureServices(IServiceCollection services) {
    // Add framework services.
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

    services.AddIdentity<ApplicationUser, IdentityRole>() 
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    services.AddMvc();

    // Add application services.
    services.AddTransient<IEmailSender, AuthMessageSender>();
    services.AddTransient<ISmsSender, AuthMessageSender>();

    var connectionString = Configuration.GetConnectionString("DefaultConnection"); 
    //requires using System.Reflection;
    var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
    
    // configure Identity Server with EF stores for users, clients and resources
    services.AddIdentityServer()
        .AddTemporarySigningCredential()
        .AddAspNetIdentity<ApplicationUser>() 
        .AddConfigurationStore(builder =>
            builder.UseSqlServer(connectionString, options =>
                options.MigrationsAssembly(migrationsAssembly)))
        .AddOperationalStore(builder =>
            builder.UseSqlServer(connectionString, options =>
                options.MigrationsAssembly(migrationsAssembly)));
}
``` 

The ```AddTemporarySigningCredential``` extension creates temporary key material for signing tokens on every start. This might be useful to get started, but needs to be replaced by some persistent key material for production scenarios. See the cryptography docs for more information.

The ```AddAspNetIdentity``` extension method is invoked to use the ASP.NET Identity users; it requires a generic parameter which is your ASP.NET Identity user type (the same one needed in the AddIdentity method from the template).

The calls to ```AddConfigurationStore``` and ```AddOperationalStore``` are registering the EF-backed store implementations.

The "builder" callback function passed to these APIs is the EF mechanism to allow you to configure the ```DbContextOptionsBuilder``` for the ```DbContext``` for each of these two stores. This is how our ```DbContext``` classes can be configured with the database provider you want to use. In this case by calling ```UseSqlServer``` we are using SqlServer. As you can also tell, this is where the connection string is provided.

The "options" callback function in ```UseSqlServer``` is what configures the assembly where the EF migrations are defined. EF requires the use of migrations to define the schema for the database.

It is the responsibility of your hosting application to define these migrations, as they are specific to your database and provider.

We’ll add the migrations next.

### Adding migrations

**NOTE: ENSURE THAT YOUR PROJECT COMPILES BEFORE CONTINUING WITH THE FOLLOWING STEP**

To create the migrations, open a command prompt in the IdentityServer project directory. In the command prompt run these two commands:

```
dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb
dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb
```

You should now see a ~/Data/Migrations/IdentityServer folder in the project. This contains the code for the newly created migrations.

### Initialize the database

Now that we have the migrations, we can write code to create the database from the migrations. We will also seed the database with some initial configuration data.

In ```Startup.cs``` add this method to help initialize the database:

```cs
private void InitializeDatabase(IApplicationBuilder app) {
    using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope()) {
        //let's first ensure that the DB schema matches our DbContexts
        serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
        //requires using IdentityServer4.EntityFramework.DbContexts;
        serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

        var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        context.Database.Migrate();

        //now let's add the Clients
        foreach (var client in Config.GetClients()) {
            var existingClient = context.Clients.FirstOrDefault(c => c.ClientId == client.ClientId);
            if (existingClient != null)
                context.Clients.Remove(existingClient);
            //requires using IdentityServer4.EntityFramework.Mappers;
            context.Clients.Add(client.ToEntity());
        }
        context.SaveChanges();

        //let's add the IdentityResources
        foreach (var resource in Config.GetIdentityResources()) {
            var existingIdentityResource = context.IdentityResources.FirstOrDefault(c => c.Name == resource.Name);
            if (existingIdentityResource != null)
                context.IdentityResources.Remove(existingIdentityResource);
            context.IdentityResources.Add(resource.ToEntity());
        }
        context.SaveChanges();

        //let's add the ApiResources
        foreach (var resource in Config.GetApiResources()) {
            var existingApiResource = context.ApiResources.FirstOrDefault(c => c.Name == resource.Name);
            if (existingApiResource != null)
                context.ApiResources.Remove(existingApiResource);
            context.ApiResources.Add(resource.ToEntity());
        }
        context.SaveChanges();

        // let's use Microsoft Identity Framework to add a couple of users
        //requires using Microsoft.AspNetCore.Identity;
        var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var user in Config.GetUsers()) {
            userManager.CreateAsync(user, "Pa$$w0rd").Wait();
            //requires using System.Security.Claims;
            //requires using IdentityModel;
            userManager.AddClaimAsync(user, new Claim(JwtClaimTypes.Name, user.Email, JwtClaimTypes.Name)).Wait();
        }
    }
}
```

And then we can invoke this from the Configure method:

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
    loggerFactory.AddConsole(Configuration.GetSection("Logging"));
    loggerFactory.AddDebug();

    if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
        app.UseDatabaseErrorPage();
        app.UseBrowserLink();
    } else {
        app.UseExceptionHandler("/Home/Error");
    }

    app.UseStaticFiles();

    // this will do the initial DB population
    InitializeDatabase(app);

    app.UseIdentity();

    // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715
    app.UseIdentityServer();

    app.UseMvc(routes => {
        routes.MapRoute(
            name: "default",
            template: "{controller=Home}/{action=Index}/{id?}");
    });
}
```

If you run the server and navigate the browers to ```http://localhost:5002/.well-known/openid-configuration```, you should see the so-called discovery document. This will be used by your clients and APIs to download the necessary configuration data.

### Adding the UI

All the protocol support needed for OpenID Connect is already built into IdentityServer. You need to provide the necessary UI parts for login, logout, consent and error.

We will use an MVC-based sample UI as a starting point.

This UI can be found in the Quickstart UI repo [https://github.com/IdentityServer/IdentityServer4.Samples/tree/release](https://github.com/IdentityServer/IdentityServer4.Samples/tree/release). 
You can either clone or download this repo and drop the controllers, views, and models into your web application.

- Delete the following folders from your IdentityServer project:
    - Controllers
    - Models
    - Views 
- Go to the Quickstarts\6_AspNetIdentity\src\IdentityServerWithAspNetIdentity
- Copy the following folders into your IdentityServer4 project: 
    - Controllers
    - Models
    - Quickstart
    - Views 

Search ```IdentityServerWithAspNetIdentity``` and replace it with ```IdentityServer``` in all files of your IdentityServer project.

You should be able to run your application and log in with the following credentials:

- UserName: alice@gmail.com
- Password: Pa$$w0rd

You should also be able to register a new user.

The IdentityServer application is complete. We can now proceed to implement the security bits on our Web Api Project.

## Web API Configuration

Let's add the authentication middleware to our API host. The job of that middleware is to:

- validate the incoming token to make sure it is coming from a trusted issuer
- validate that the token is valid to be used with this api (aka scope)

Add the following package to your MarketPlaceService project:

```"IdentityServer4.AccessTokenValidation": "1.1.0"```

You also need to add the middleware to your pipeline. It must be added **before** MVC:

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
    loggerFactory.AddConsole(Configuration.GetSection("Logging"));
    loggerFactory.AddDebug();

    app.UseCors("default");

    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MarketPlace API V1");
    });

    app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions {
        Authority = "http://localhost:5002",
        RequireHttpsMetadata = false,

        ApiName = "MarketplaceService"
    });

    app.UseMvc();
}
```

### Authorization

Authorization in MVC is controlled through the ```AuthorizeAttribute``` attribute and its various parameters. At its simplest applying the AuthorizeAttribute attribute to a controller or action limits access to the controller or action to any authenticated user.

The following code limits access to the Create Action of our ProductsController.

```cs
[HttpPost]
[SwaggerOperation("createProduct")]
[ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
//requires using Microsoft.AspNetCore.Authorization;
[Authorize]
public IActionResult Create([FromBody] Product product) {
//the rest of the code that was already there
```

Now only authenticated users can access the create function.

To test this:
- Run the IdentityServer by pressing F5
- Run the MarketplaceService by right clicking on the Solution Explorer -> MarketplaceService Project and selecting Debug -> Start New Instance
- Run the JavaScriptClient by right clicking on the Solution Explorer -> JavaScriptClient Project and selecting Debug -> Start New Instance
- Open the Developer Tools by clicking on F12
- Open the Network traffic Tab
- Click on the New Product button
- Insert some values for name, desription and price
- Click on Save

On the Network tab in the developer tools you should see that the response is a 401 Unauthorized.

## The JavaScript Client

We need to cofigure the JavaScript Client so that the user can login to IdentityServer, invoke the web API with an access token issued by IdentityServer, and logout of IdentityServer.

### Reference oidc-client

In the JavaScriptClient project we need a library that works in JavaScript and is designed to run in the browser. The ```oidc-client``` library is one such library. It is available via NPM, Bower, as well as a direct download from github.

On the Solution Explorer, open the package.json file in your JavaScriptClient project and replace its content with:

```json
{
  "version": "1.0.0",
  "name": "asp.net",
  "private": true,
  "devDependencies": {
  },
  "dependencies": {
    "vue": "^2.2.4",
    "swagger-client": "^2.1.32",
    "oidc-client": "^1.3.0"
  }
}
```

Save the file so that Visual Studio starts downloading the npm package.

### Authentication class

Let's create a JavaScript ApplicationUserManager class that will take care of the client configuration and login / logout of our user. Our class will extend the UserManager class and it will auto configure itself during creation. The UserManager class is included in the oidc-client library and it  manages the OpenID Connect protocol.

First, add a new ```applicationusermanager.js``` file by right clicking on the Solution Explorer -> ```JavaScriptClient/wwwroot/js/src``` and selecting ```Add New Item```. In the Add New Item window, select ASP.NET Core -> Web -> Scripts -> JavaScript File, type ```applicationusermanager.js``` as file name and click on Add.

In order to extend the UserManager, we first need to import the classes from the oidc-client package.
Add in ```import``` to the applicationusermanager.js file as follows:

```js
import * as Oidc from 'oidc-client/lib/oidc-client.js'
```

Now create an ApplicationUserManager class that extends UserManager and add a constructor.

```js
class ApplicationUserManager extends Oidc.UserManager {
    constructor() {

    }
}
```

Next, we can call the constructor of our UserManager base class. It requires a configuration object. Add this code to configure the UserManager in the constructor of your ApplicationUserManager class:

```js
super({
    authority: "http://localhost:5002",
    client_id: "js",
    redirect_uri: "http://localhost:5001/callback.html",
    response_type: "id_token token",
    scope: "openid profile MarketplaceService",
    post_logout_redirect_uri: "http://localhost:5001/index.html"
});

```

The UserManager provides a ```getUser``` API to know if the user is logged into the JavaScript application. It uses a JavaScript Promise to return the results asynchronously. The returned User object has a profile property which contains the claims for the user. Add this code in the constructor of your ApplicationUserManager class to detect if the user is logged into the JavaScript application:

```js
this.getUser().then(user => {
    if (user)
        this.log("User logged in", user.profile);
    else
        this.log("User not logged in");
}).catch(error => this.log("Problem trying to read the user", error));
```

Now that our constructor is complete, let's add a helper method to log messages to the console.

```js
log(...parameters) {
    for(const parameter of parameters){
        let msg;
        if (parameter instanceof Error) 
            msg = `Error: ${parameter.message}`;
        else if (typeof parameter !== 'string') 
            msg = JSON.stringify(parameter, null, 2);
        else
            msg = parameter;
        console.log(`${msg} \r\n`);
    }
}

```

Next, we want to implement the ```login``` function. The UserManager provides a signinRedirect to log the user in. Add this code to implement the login method in our ApplicationUserManager class:

```js
async login() {
    await this.signinRedirect();
    return await this.getUser();
}
```

The UserManager provides a signoutRedirect to log the user out. Let's implement a logout method in our ApplicationUserManager class:

```js
async logout() {
    return await this.signoutRedirect();
}
```

At the bottom of our applicationusermanager.js file, after the closing bracket of the ApplicationUserManager class, let's register a global ```authenticationManager``` constant  and export it so that it can be used wherever necessary:

```js
const applicationUserManager = new ApplicationUserManager();
export { applicationUserManager as default };
```

Our class is complete. Let's use it in the DataLayer.

The User object obtained through the getUser in the above code also has an ```access_token``` property which can be used to authenticate with a web API. The ```access_token``` will be passed to the web API via the Authorization header with the Bearer scheme. 
What we need to do next is to add the bearer token to our invocation in the createProduct in the DataLayer. 

Open the ```JavaScriptClient/wwwroot/js/src/datalayer.js``` file.

At the beginning of the file, add an ```import``` to reference our new applicationUserManager object.

```js
import applicationUserManager from "./ApplicationUserManager"
```

Replace the ```insertProduct``` method with the following code: 

```js
async insertProduct(product) {
    const user = await applicationUserManager.getUser();
    const client = await new Swagger({
        url: this.url,
        usePromise: true
    });
    const data = await client.Products.createProduct({ product }, {
        clientAuthorizations: {
            api_key: new Swagger.ApiKeyAuthorization('Authorization', 'Bearer ' + user.access_token, 'header')
        }
    });
    return data.obj;
}
```

We now want to give the user the chance to log on. We need a button and a method for that.

### Add a login button to the navigation bar

Open the ```CommandBar.vue``` Vue Component, locate the div component with the ```commandBar``` id and replace it with the following code:

```html
<div class="collapse navbar-collapse" id="commandBar">
    <div class="form-inline my-2 my-lg-0">
        <button class="btn btn-primary" v-on:click="add" v-bind:disabled="isFormInUse">New Product</button>
    </div>

    <div class="form-inline my-2 my-lg-0">
        <button id="login" v-on:click="login" class="btn btn-secondary">Login</button>
    </div>
</div>
```

### Import tha applicationUserManager in the CommandBar

At the top of the ```<script>``` section of the ```CommandBar``` component, add in ```import``` statement to make use of the ```applicationUserManager``` object.

```js
import applicationUserManager from "./ApplicationUserManager"
```
### Add a login method to the CommandBar 

Locate the ```methods``` property of the ```CommandBar``` and add a new method:

```js
async login() {
    let user = await applicationUserManager.login();
}
```

### callback.html

This HTML file is the designated ```redirect_uri``` page once the user has logged into IdentityServer. It will complete the OpenID Connect protocol sign-in handshake with IdentityServer. The code for this is all provided by the UserManager class we used earlier. Once the sign-in is complete, we can then redirect the user back to the main index.html page. 

Locate the file called oidc-client.js in the ~/node_modules/oidc-client/dist folder and copy it into your application’s ~/wwwroot/js folder.

In the ```JavaScriptClient/wwwroot``` folder, create a new HTML page named ```callback.html```.
Replace its content with the following code to complete the signin process:

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title></title>
</head>
<body>
    <script src="js/oidc-client.js"></script>
    <script>
        new Oidc.UserManager().signinRedirectCallback().then(function () {
            window.location = "index.html";
        }).catch(function (e) {
            console.error(e);
        });
    </script>
</body>
</html>
```

We are ready to test the login process and the creation of a new product.

- Run webpack by opening a command line on the JavaScriptClient project where your webpack.config.js is located and typing ```webpack```
- Run the IdentityServer by going back to Visual Studio and pressing F5
- Run the MarketplaceService by right clicking on the Solution Explorer -> MarketplaceService Project and selecting Debug -> Start New Instance
- Run the JavaScriptClient by right clicking on the Solution Explorer -> JavaScriptClient Project and selecting Debug -> Start New Instance
- Open the Developer Tools by clicking on F12
- Click on the New Product button
- Insert some values for name, description and price
- Click on Save
    - The product is not added
- Click on the Login Button
    - You get redirected to the login action in the IdentityServer 
    - Login with a ```bob@gmail.com``` username and a ```Pa$$w0rd``` password
    - You get asked for consent to share your data with the Client. Click on Allow. 
    - You get briefly redirected to the ```callback.html``` page and then the ```index.html```
- Click on the New Product button
- Insert some values for name, description and price
- Click on Save
    - The product shows up as a new product

### Show the New Product button only if the user is authenticated

As a visual help to the user, we want to make sure to display the ```New Product``` button only if the user is authenticated. In order to do that we need to add an ```isUserAuthenticated``` boolean property to our CommandBar component and then render the button only when the value of this property is ```true```. We will set the property by checking if the user returned by the applicationUserManager.getUser() is not null. 

Let's start by adding a new data property that we will link to the DOM.

```js
import applicationUserManager from "./ApplicationUserManager"
export default {
    props: ['isFormInUse'],
    data() {
        return {
            userAuthenticated: false
        }
    }
    // ...the code that was here before
```

Now let's check again during startup and eventually refresh our property:

```js
async mounted() {
    let user = await applicationUserManager.getUser();
    this.userAuthenticated = user && user.profile && user.profile.name ? true : false;
}
// ...the code that was here before
``` 

Let's also modify the login method to update the property if needed:

```js
async login() {
    let user = await applicationUserManager.login();
    this.userAuthenticated = user && user.profile && user.profile.name ? true : false;
}
```

Now let's bind the DOM to the property by modifying the New Product button:

```html
<button v-if="userAuthenticated" class="btn btn-primary" v-on:click="add" v-bind:disabled="isFormInUse">New Product</button>
```

We are ready to test the login process and the creation of a new product.

- Run webpack by opening a command line on the JavaScriptClient project where your webpack.config.js is located and typing ```webpack```
- Run the IdentityServer by pressing F5
- Run the MarketplaceService by right clicking on the Solution Explorer -> MarketplaceService Project and selecting Debug -> Start New Instance
- Run the JavaScriptClient by right clicking on the Solution Explorer -> JavaScriptClient Project and selecting Debug -> Start New Instance
- There is no New Product button
- Click on the Login Button
- After a couple of quick redirect, tha index.html is shown and the New Product is visible
- Click on the New Product button
- Insert some values for name, description and price
- Click on Save
    - The product shows up as a new product

### Add a Logout button and show the login / logout only if necessary

As an excercise, try to add a logout button that invokes a logout method when clicked. Make sure that both the login and the logout button are rendered only if necessary. See if you can also show the user name in the logout button text. Hint: you may need another data property.

When you're done, continue reading this document.

Modify the ```CommandBar.vue``` template as follows:

```html
<button id="login" v-if="!isUserAuthenticated" v-on:click="login" class="btn btn-secondary">Login</button>
<button id="logout" v-if="isUserAuthenticated" v-on:click="logout" class="btn btn-secondary">Hello {{userName}} - Logout</button>
``` 

Add a new ```userName``` data property and initialize it as an empty string:

```js
data() {
    return {
        userAuthenticated: false,
        userName : ""
    }
}
```

Refresh the property during startup by modifying the ```mounted``` method as follows:

```js
async mounted() {
    let user = await applicationUserManager.getUser();
    this.userAuthenticated = user && user.profile && user.profile.name ? true : false;
    this.userName = user && user.profile && user.profile.name ? user.profile.name : "";
}
```

Refresh the property during login by modifying the ```login``` method as follows:

```js
async login() {
    let user = await applicationUserManager.login();
    this.userAuthenticated = user && user.profile && user.profile.name ? true : false;
    this.userName = user && user.profile && user.profile.name ? user.profile.name : "";
}
```

Add a new ```logout``` method to invoke the applicationUserManager.logout() method and refresh the ```userAuthenticated``` and ```userName``` properties

```js
async logout() {
    await applicationUserManager.logout();
    this.userAuthenticated = false;
    this.userName = "";
}
```

We are ready to test the page.

- Run webpack by opening a command line on the JavaScriptClient project where your webpack.config.js is located and typing ```webpack```
- Run the IdentityServer by pressing F5
- Run the MarketplaceService by right clicking on the Solution Explorer -> MarketplaceService Project and selecting Debug -> Start New Instance
- Run the JavaScriptClient by right clicking on the Solution Explorer -> JavaScriptClient Project and selecting Debug -> Start New Instance
- There is no New Product button and no Logout button, but the Login button is visible
- Click on the Login Button
- After a couple of quick redirect, the index.html is shown and 
    - The New Product is visible
    - The Logout Button is visible
    - The name of the logged on user is shown
    - The Login Button is not visible
- Click on the Logout button
- Logout from IdentityServer
- Return to http://localhost:5001
- There is no New Product button and no Logout button, but the Login button is visible
    

Our Create operation is now protected, but we still did not protect the edit and delete operations. This is what we're going to do on our next and last lab.

# Next steps

```
git add .
git commit -m "student: step 6 complete"
git checkout step07start
```