# Lab 07 - Resource Based Authorization

We did not yet protect the update and delete operations.

What we would like to have is an application where:

- Every product has an owner
- Products may be updated and deleted only by their respective owners

In order to achieve this, we have to update our MarketPlaceService and our JavaScriptClient.

- MarketPlaceService
    - We'll add a UserName property to the Product class so that we can persist who the owner is
    - We will update the Create action to check the UserName property of the product being added 
    - We will use a Sql Server database instead of the memory store that we've bee using so far. It is not strictly necessary for our scenario, but at least we will have a persisting data store also on our MarketPlace Service and not just on our IdentityServer application.
        - We will configure our DbContext to use Sql instead of InMemory
        - We will create the schema on our DataBase by adding Migrations to our project 
        - We will seed the DB with a couple of example products 
    - We will configure the Authorization by creating 
        - A ProductOwner Policy 
        - A ProductOwner Requirement
        - A ProductOwnerAuthorizationHandler. This handler will succeed only if the ```UserName``` property of the Product being updated/deleted matches the value of the ```name``` claim received in the access_token and the claim has been issued by our own IdentityServer Web Application. 
    - We will check the ProductOwner Policy on Update eventually denying the user the possibility to complete the action if he's not the product's owner
    - We will check the ProductOwner Policy on Delete eventually denying the user the possibility to complete the action if he's not the product's owner   
- JavaScriptClient
    - We will update the User Interface of the Product Item by adding a userName property to product-item
    - We will add a userName property to current product of the Vue instance setting it to the logged on user name  
    - We will pass the Credentials to our MarketPlaceService during update, just like we did during the Create
    - We will pass the Credentials to our MarketPlaceService during delete, just like we did during the Create
    - We will make sure that the Update and Delete buttons are shown only if allowed:
        - We will add a userIsOwner computed property to product-item component
        - We will show the update and delete buttons of each product-item only if userIsOwner is true

## MarketPlaceService

### Add a UserName property to the Product class 

We want to set a relationship between a product and its owner. To do that we will add a new property UserName to the Product class.

Open the ```MarketPlaceService/Models/Product.cs``` file and add a UserName property of type String.

```cs
public string UserName { get; set; }
```

### Update the Create action to check the UserName property of the product being added

We may expect the client to send the Product to create already filled up with the correct UserName property, but just to be sure we are going to check if the UserName property matches the Name property of the User.Identity object, rejecting the request if it doesn't.

Open the ```MarketPlaceService/Controllers/ProductsController.cs```, locate the ```Create``` action and replace the code with the following:

```cs
public IActionResult Create([FromBody] Product product) {
    if (product == null || product.UserName != User.Identity.Name) {
        return BadRequest();
    }
    
    _ProductsRepository.Add(product);

    return CreatedAtRoute("GetProduct", new { id = product.Id }, product);
}
``` 
    
### Use a Sql Server database 

Let's start by configuring our DbContext to use SqlServer instead of InMemory

Add a dependency to the ```Microsoft.EntityFrameworkCore.SqlServer``` package.

Remove the dependency to the ```Microsoft.EntityFrameworkCore.InMemory``` package.

ASP.NET Core implements dependency injection by default. Services (such as the EF database context) are registered with dependency injection during application startup. Components that require these services (such as our Repository) are provided these services via constructor parameters. 

To register MarketPlaceContext as a service, open ```Startup.cs```, locate the ```ConfigureServices``` method and replace the following code 

```cs
services.AddDbContext<MarketPlaceContext>(opt => opt.UseInMemoryDatabase());
```

with this:

```cs
services.AddDbContext<MarketPlaceContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
```

Open the ```appsettings.json``` file and add a connection string as shown in the following example.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-MarketPlaceService;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

The connection string specifies a SQL Server LocalDB database. LocalDB is a lightweight version of the SQL Server Express Database Engine and is intended for application development, not production use. LocalDB starts on demand and runs in user mode, so there is no complex configuration. By default, LocalDB creates .mdf database files in the ```C:/Users/<user>``` directory

### Modify the MarketPlaceContext class

Open the ```MarketPlaceService/Data/MarkePlaceContext.cs``` file and add the following method to the MarketPlaceContext class:

```cs
protected override void OnModelCreating(ModelBuilder builder) {
    base.OnModelCreating(builder);
}
```

### Add Migrations

When you develop a new application, your data model changes frequently, and each time the model changes, it gets out of sync with the database. The EF Core Migrations feature enables EF to create and update the database schema.

### Entity Framework Core NuGet packages for migrations

To work with migrations, you can use the Package Manager Console (PMC) or the command-line interface (CLI). These tutorials show how to use CLI commands. 

The EF tools for the command-line interface (CLI) are provided in ```Microsoft.EntityFrameworkCore.Tools.DotNet```. To install this package, add it to the ```DotNetCliToolReference``` collection in the .csproj file, as shown. (The version numbers in this example were current when the tutorial was written.)

```xml
<ItemGroup>
  <DotNetCliToolReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Tools" Version="1.0.0" />
  <DotNetCliToolReference Include="Microsoft.EntityFrameworkCore.Tools.DotNet" Version="1.0.0" />
</ItemGroup>
```
(You can edit the .csproj file by right-clicking the project name in Solution Explorer and selecting Edit MarketPlaceService.csproj.)

Also add ```Microsoft.EntityFrameworkCore.Design``` between the previous item group

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="1.1.1" />
```

### Create an initial migration

Save your changes and build the project. Then open a command window and navigate to the project folder. Here's a quick way to do that:

In Solution Explorer, right-click the project and choose Open Command Line from the context menu.

Enter the following command in the command window:

```
dotnet ef migrations add InitialCreate
```

### Examine the Up and Down methods

When you executed the migrations add command, EF generated the code that will create the database from scratch. This code is in the Migrations folder, in the file named _InitialCreate.cs. The Up method of the InitialCreate class creates the database tables that correspond to the data model entity sets, and the Down method deletes them, as shown in the following example.

```cs
public partial class InitialCreate : Migration {
    protected override void Up(MigrationBuilder migrationBuilder) {
        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Description = table.Column<string>(nullable: true),
                Name = table.Column<string>(nullable: true),
                Price = table.Column<decimal>(nullable: false),
                UserName = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.DropTable(
            name: "Products");
    }
}
```

Migrations calls the Up method to implement the data model changes for a migration. When you enter a command to roll back the update, Migrations calls the Down method.

This code is for the initial migration that was created when you entered the migrations add InitialCreate command. The migration name parameter ("InitialCreate" in the example) is used for the file name and can be whatever you want. It's best to choose a word or phrase that summarizes what is being done in the migration. For example, you might name a later migration "AddCustomers".

If you created the initial migration when the database already exists, the database creation code is generated but it doesn't have to run because the database already matches the data model. When you deploy the app to another environment where the database doesn't exist yet, this code will run to create your database, so it's a good idea to test it first. 

### Examine the data model snapshot

Migrations also creates a snapshot of the current database schema in Migrations/MarketPlaceContextModelSnapshot.cs. Here's what that code looks like:

```cs
[DbContext(typeof(MarketPlaceContext))]
partial class MarketPlaceContextModelSnapshot : ModelSnapshot {
    protected override void BuildModel(ModelBuilder modelBuilder) {
        modelBuilder
            .HasAnnotation("ProductVersion", "1.1.1")
            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

        modelBuilder.Entity("MarketPlaceService.Models.Product", b => {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd();

            b.Property<string>("Description");

            b.Property<string>("Name");

            b.Property<decimal>("Price");

            b.Property<string>("UserName");

            b.HasKey("Id");

            b.ToTable("Products");
        });
    }
}
```

### Add code to create the database, apply the migrations and initialize the database with test data

In this section, you write a method that is called to create the database, apply every migration and populate it with test data.
In the Data folder, create a new class file named ```DbInitializer.cs``` and replace the template code with the following code, which causes a database to be created when needed and loads test data into the new database.

```cs
using MarketPlaceService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace MarketPlaceService.Data {
    public static class DbInitializer {
        public static void Initialize(MarketPlaceContext context) {
            //using Microsoft.EntityFrameworkCore;
            context.Database.Migrate();

            // Look for any products.
            if (context.Products.Any())
            {
                return;   // DB has been seeded
            }

            var products = new Product[] {
                new Product { Name = "Product 1", Description = "First Sample Product", Price = 1234, UserName="alice@gmail.com" },
                new Product { Name = "Product 2", Description = "Lorem Ipsum", Price = 555 , UserName="alice@gmail.com"},
                new Product { Name = "Product 3", Description = "Third Sample Product", Price = 333 , UserName="bob@gmail.com"},
                new Product { Name = "Product 4", Description = "Fourth Sample Product", Price = 44 , UserName="bob@gmail.com"}
            };

            foreach (var product in products) {
                context.Products.Add(product);
            }

            context.SaveChanges();
        }
    }
}
```

The code checks if there are any products in the database, and if not, it assumes the database is new and needs to be seeded with test data. It loads test data into arrays rather than ```List<T>``` collections to optimize performance.

In ```Startup.cs```, modify the ```Configure``` method to call this seed method on application startup. First, add the context to the method signature so that ASP.NET dependency injection can provide it to your DbInitializer class.

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, MarketPlaceContext context) {
```

Then call your ```DbInitializer.Initialize``` method at the end of the ```Configure``` method.

```cs
app.UseMvc();

DbInitializer.Initialize(context);
```

### Remove the new Product from the Repository Constructor

We don't need to add a new product every time that our Repository gets created as we did before, so let's remove that. Open the ```MarketPlaceService/Data/ProductsRepository.cs``` file, locate the constructor and remove the following line of code:

```cs
Add(new Product { Name = "Product 1", Description = "First Sample Product", Price = 1234 });
```

Now that we have a persistent data store, we can register our Repository as a transient service instead of singleton.
Open the ```Startup``` class, locate the ```ConfigureServices``` method and replace the following code

```cs
services.AddSingleton<IProductsRepository, ProductsRepository>();
```

with this

```cs
services.AddTransient<IProductsRepository, ProductsRepository>();
```

## Authorization

Now that the code for our Database is ready, let's proceed to enforce Authorization Policies.

### Custom Policy-Based Authorization

Role authorization and Claims authorization make use of 

- a requirement 
- a handler for the requirement 
- a pre-configured policy

These building blocks allow you to express authorization evaluations in code, allowing for a richer, reusable, and easily testable authorization structure.

An *authorization policy* is made up of one or more *requirements* and registered at application startup as part of the Authorization service configuration, in ```ConfigureServices``` in the ```Startup.cs``` file.

Open the ```Startup.cs``` and add the following code at the bottom of the ```ConfigureServices``` method  

```cs
services.AddAuthorization(options => {
    options.AddPolicy("ProductOwner", policy => policy.Requirements.Add(new ProductOwnerRequirement()));
});
```

Here you can see a "ProductOwner" policy is created with a single requirement, that of being the owner of a product, which is passed as a parameter to the requirement. ```ProductOwnerRequirement``` is a class that we will create in a following step, so don't worry if your code does not compile.

Policies can usually be applied using the ```Authorize``` attribute by specifying the policy name, but not in this case.
Our authorization depends upon the resource being accessed. A Product has a UserName property. Only the product owner is allowed to update it or delete it, so the resource must be loaded from the product repository before an authorization evaluation can be made. This cannot be done with an Authorize attribute, as attribute evaluation takes place before data binding and before your own code to load a resource runs inside an action. Instead of declarative authorization, the attribute method, we must use imperative authorization, where a developer calls an authorize function within their own code.

### Authorizing within your code

Authorization is implemented as a service, ```IAuthorizationService```, registered in the service collection and available via dependency injection for Controllers to access.

```cs
public class ProductsController : Controller {
    private readonly IProductsRepository _ProductsRepository;
    IAuthorizationService _authorizationService;

    public ProductsController(IProductsRepository ProductsRepository, IAuthorizationService authorizationService) {
        _ProductsRepository = ProductsRepository;
        _authorizationService = authorizationService;
    }
    //same code as before
```

The ```IAuthorizationService``` interface has two methods, one where you pass the resource and the policy name and the other where you pass the resource and a list of requirements to evaluate.
To call the service, load your product within your action then call the AuthorizeAsync, returning a ChallengeResult if the result is false. 

```cs
[HttpPut("{id}")]
[ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
[SwaggerOperation("updateProduct")]
public IActionResult Update(int id, [FromBody] Product product) {
    if (product == null || product.Id != id) {
        return BadRequest();
    }

    var original = _ProductsRepository.Find(id);
    if (original == null) {
        return NotFound();
    }

    if (!_authorizationService.AuthorizeAsync(User, product, "ProductOwner").Result) {
        return new ChallengeResult();
    }

    original.Name = product.Name;
    original.Description = product.Description;
    original.Price = product.Price;

    _ProductsRepository.Update(original);
    return new NoContentResult();
}
```

And

```cs
[HttpDelete("{id}")]
[ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
[SwaggerOperation("deleteProduct")]
public IActionResult Delete(int id) {
    var product = _ProductsRepository.Find(id);
    if (product == null) {
        return NotFound();
    }
    if (!_authorizationService.AuthorizeAsync(User, product, "ProductOwner").Result) {
        return new ChallengeResult();
    }

    _ProductsRepository.Remove(id);
    return new NoContentResult();
}
```

### Requirements

An authorization requirement is a collection of data parameters that a policy can use to evaluate the current user principal. In our ProductOwner policy the requirement we have is a single parameter, the owner. A requirement must implement ```IAuthorizationRequirement```. This is an empty, marker interface. 
Create a new Folder ```Authorization``` in your MarketPlaceService project.
Add a new ```ProductOwnerRequirement``` class and let the class implement the ```IAuthorizationRequirement``` interface by replacing the file content with the following code :

```cs
using Microsoft.AspNetCore.Authorization;
namespace MarketPlaceService.Authorization {
    public class ProductOwnerRequirement : IAuthorizationRequirement {
    }
}
```

A requirement doesn't need to have data or properties.

### Authorization Handlers

An *authorization handler* is responsible for the evaluation of any properties of a requirement. The authorization handler must evaluate them against a provided ```AuthorizationHandlerContext``` to decide if authorization is allowed. A requirement can have multiple handlers. Handlers must inherit ```AuthorizationHandler<T>``` where ```T``` is the requirement it handles.

In the ```MarketPlaceService/Authorization``` folder, add a ```ProductOwnerAuthorizationHandler``` class and replace its content with the following code:

```cs
using MarketPlaceService.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace MarketPlaceService.Authorization {
    public class ProductOwnerAuthorizationHandler : AuthorizationHandler<ProductOwnerRequirement, Product> {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ProductOwnerRequirement requirement, Product resource) {
            if (!context.User.HasClaim(c => c.Type == JwtClaimTypes.Name && c.Issuer == "http://localhost:5002"))
                return Task.CompletedTask;

            var userName = context.User.FindFirst(c => c.Type == JwtClaimTypes.Name && c.Issuer == "http://localhost:5002").Value;

            if (userName == resource.UserName)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
```

In the code above we first look to see if the current user principal has a name claim which has been issued by an Issuer we know and trust. If the claim is missing we can't authorize so we return. If we have a claim, we figure out the value of the claim, and if it matches the UserName of the product then authorization has been successful. Once authorization is successful we call context.Succeed() passing in the requirement that has been successful as a parameter.

Handlers must be registered in the services collection during configuration. Open the Startup.cs and add this line of code at the bottom of the ```ConfigureServices``` method:

```cs
//requires using Microsoft.AspNetCore.Authorization;
//requires using MarketPlaceService.Authorization;
services.AddSingleton<IAuthorizationHandler, ProductOwnerAuthorizationHandler>();
```

Each handler is added to the services collection by using ```services.AddSingleton<IAuthorizationHandler, YourHandlerClass>();``` passing in your handler class.

We are now ready to move to the JavaScriptClient

## JavaScriptClient


### Udate the User Interface of the Product Item 

Let's show the userName property on the product-item template.

Open ```JavaScriptClient/wwwroot/js/src/ProductItem.vue```, locate the ```<template>``` section and add a new paragraph at the end of the card block to display the ```prod.userName``` property:

```html
<div class="card-block">
    <h4 class="card-title">{{prod.name}}</h4>
    <h6 class="card-subtitle mb-2 text-muted">{{prod.price}}</h6>
    <p class="card-text">{{prod.description}}</p>
    <p class="card-text text-muted">{{prod.userName}}</p>
</div>
```

- Run Webpack by opening a command prompt on the JavaScript project where your ```webpack.config.js``` is located and typing ```webpack```
- Run the IdentityServer by pressing F5
- Run the MarketplaceService by right clicking on the Solution Explorer -> MarketplaceService Project and selecting Debug -> Start New Instance
- Run the JavaScriptClient by right clicking on the Solution Explorer -> JavaScriptClient Project and selecting Debug -> Start New Instance
- Notice the user name showing up in each product

## Add a userName property to current product of the App component and initialize it with an empty string  

Open the ```JavaScriptClient/wwwroot/js/src/App.vue```.

Add an ```import``` statement at the beginning of the ```<script>``` section to reference the ```applicationUserManager``` object.

```js
import applicationUserManager from "./ApplicationUserManager"
```

Locate the ```data``` function and replace the ```current``` property with the following code:
 
```js
current: { id: 0, name: "", description: "", price: 0, userName: ""},
```

Update the ```current.userName``` during startup by modifying the ```mounted``` method as follows:

```js
async mounted () {
    this.products = await new DataLayer().getAllProducts(); 

    let user = await applicationUserManager.getUser();
    this.current.userName = user && user.profile && user.profile.name ? user.profile.name : "" ;
}
```

Modify the ```add``` method as follows:

```js
async add() {
    let user = await applicationUserManager.getUser();
    this.current = { id: 0, brand: "", name: "", price: 0, userName: user && user.profile && user.profile.name ? user.profile.name : "" };
    this.isFormInUse = true;
}
```

Modify the ```cancel``` method as follows:

```js
async cancel(product) {
    if (product.id == 0) {
        let user = await authenticationManager.getUser();
        this.current.id = 0;
        this.current.name = "";
        this.current.description = "";
        this.current.price = 0;
        this.current.userName = user && user.profile && user.profile.name ? user.profile.name : "";
    } else {
        const p = await new DataLayer().getProductById(product.id);
        this.current.id = p.id;
        this.current.name = p.name;
        this.current.description = p.description;
        this.current.price = p.price;
        this.current.userName = p.userName;
    }
    this.isFormInUse = false;
}
```

### Pass the Credentials to our MarketPlaceService during update

Just like we did during the Create phase, we need to add the Bearer token to the header of our request during the update invocation. 

Open the ```JavaScriptClient/wwwroot/js/src/datalayer.js``` file and replace the ```updateProduct``` method with the following code:

```js
async updateProduct(id, product) {
    const user = await applicationUserManager.getUser();
    const client = await new Swagger({
        url: this.url,
        usePromise: true
    });

    const data = await client.Products.updateProduct({ id, product }, {
        clientAuthorizations: {
            api_key: new Swagger.ApiKeyAuthorization('Authorization', 'Bearer ' + user.access_token, 'header')
        }
    });
    return data.obj;
}
```

### Pass the Credentials to our MarketPlaceService during delete

Now replace the ```deleteProduct``` with the following code

```js
async deleteProduct(id) {
    const user = await applicationUserManager.getUser();
    const client = await new Swagger({
        url: this.url,
        usePromise: true
    });
    const data = client.Products.deleteProduct({ id }, {
        clientAuthorizations: {
            api_key: new Swagger.ApiKeyAuthorization('Authorization', 'Bearer ' + user.access_token, 'header')
        }
    });
    return data.obj;
}
```

### Show Update and Delete buttons on each product only if allowed

Open the ```JavaScriptClient/wwwroot/js/src/ProductItem.vue``` component.

Add an ```import``` statement at the beginning of the ```<script>``` section

```js
import applicationUserManager from "./ApplicationUserManager"
```

Add a ```userIsOwner``` data property and initialize it to false:

```js
data() {
    return {
        userIsOwner: false
    }
}
```

Update the ```userIsOwner``` during startup:

```js
async mounted() {
    let user = await applicationUserManager.getUser();
    this.userIsOwner = user != undefined && user.profile.name == this.prod.userName;
}
```

Modify the two buttons in the ```<template>``` section as follows:

```html
<div class="card-footer">
    <button v-if="userIsOwner" class="btn btn-secondary" v-on:click="select" v-bind:disabled="buttonsDisabled">Select</button>
    <button v-if="userIsOwner" class="btn btn-danger" v-on:click="remove" v-bind:disabled="buttonsDisabled">Delete</button>
</div>
```

We are ready to test the page.

- Run Webpack by opening a command prompt on the JavaScript project where your ```webpack.config.js``` is located and typing ```webpack```
- Run the IdentityServer by pressing F5
- Run the MarketplaceService by right clicking on the Solution Explorer -> MarketplaceService Project and selecting Debug -> Start New Instance
- Run the JavaScriptClient by right clicking on the Solution Explorer -> JavaScriptClient Project and selecting Debug -> Start New Instance
- If you're not logged on you should not see any Add, Select nor Delete button
- Log on as bob@gmail.com / Pa$$w0rd
- You should now see the New Product button, while Select and Delete are visible only on the product items where the user name is bob@gmail.com and not on those added by alice@gmail.com
- Add a new product. You should succeed.
- Select a product created by bob@gmail.com and modify it. You should succeed.
- Delete a product created by bob@gmail.com. You should succeed.       

This concludes our walkthrough.

# Next steps

```
git add .
git commit -m "student: step 7 complete"
```