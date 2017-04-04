# Lab 02 - The Javascript Client

Time to create our second project. We will focus on a JavaScript DataLayer that will communicate with our previous project. We will not build the user interface, yet. We'll do that in the following project.
In this lab we're going to use the *fetch api*. We will change that in a following project by using the Swagger Client.

### Create the project

From the File menu, select Add > New Project.

Select the ASP.NET Core Web Application (.NET Core) project template. 
Name the project **JavaScriptClient**, clear Host in the cloud, and tap OK.

In the New ASP.NET Core Web Application (.NET Core) - JavaScriptClient dialog, 
select the 1.1 Empty template. Do not check Enable Docker Support. Tap OK.

### Setup the ports of the projects

Now that we have two projects, we want to make sure that both can start and that they can communicate with each other. Right now, Visual Studio is configured to start Internet Information Server Express on a randomly assigned port. Let's configure them to use port 5000 and 5001.

### Configure MarketPlaceService to start from port 5000 

On the Solution Explorer, right click the MarketPlaceService project, select Properties.
In the Debug tab of the MarketPlaceService Properties window, change the App Url to ```http://localhost:5000/```

### Configure JavaScriptClient to start from port 5001

On the Solution Explorer, right click the MarketPlaceService project, select Properties.
In the Debug tab of the JavaScriptClient window, change the App Url to ```http://localhost:5001/```

In the JavaScriptClient project, open the Program.cs file.

Replace the content with:

```cs
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace JavaScriptClient {
    public class Program {
        public static void Main(string[] args) {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:5001")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
```

### Add an html page

In the Solution Explorer, right click the wwwroot folder of the JavascriptClient project, select Add -> New Item.

Under ASP.NET Core -> Web -> Content, select HTML Page.
Under Name, type ```index.html``` and click Add.

### Configure the middleware to serve static files

Right now, if we right click on the JavaScript project in the Solution Explorer, we select Debug -> Start New Instance and we try to navigate to ```http://localhost:5001/index.html``` we get ```Hello World``` as a response instead of the content of our page. This is because the only middleware configured in the Kestrel pipeline always respond with that string, no matter which address we request.
Let's change that.
We need to add a new package containing the middleware that can serve static files.
To do that:
- On the Solution Explorer, right click JavaScript Client -> Dependencies -> Manage Nu Get Packages
- In the NuGet Window, click on the tab Browse
- In the textbox, type ```Microsoft.AspNetCore.StaticFiles```
- When the package is found, click on Install
- On the Solution Explorer, right click on JavaScriptClient -> Startup.s
- Replace the content with

```cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JavaScriptClient {
    public class Startup {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            loggerFactory.AddConsole();

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
``` 

Right click on the JavaScript project in the Solution Explorer, select Debug -> Start New Instance and navigate to ```http://localhost:5001```. You should now get our empty index.html page.

### Add DataLayer.js

In the Solution Explorer, right click the wwwroot folder of the JavascriptClient project, select Add -> New Folder.
Name the Folder **js**

In the Solution Explorer, right click the js folder under the wwwroot folder of the JavascriptClient project, select Add -> New Item.

Under ASP.NET Core -> Web -> Script, select JavaScript file.
Under Name, type ```datalayer.js``` and click Add.

### Get Products

We're going to create a DataLayer class with the CRUD methods that invoke the server and parse the response.
Let's start with a GetAllProducts. We're going to use the fetch API for now.

- Create a class DataLayer 
- Add a constructor that initializes a serviceUrl property with the ```"http://localhost:5000/api/products"``` address
- Add a getAllProducts method that returns the result of a fetch method invoking this.serviceUrl and then the result of the response.json() method

```js
class DataLayer {
    constructor() {
        this.serviceUrl = "http://localhost:5000/api/products";
    }
    getAllProducts() {
        return fetch(this.serviceUrl).then(function (response) {
            return response.json();
        });
    }
}
```

- At the of the file, let's test the DataLayer by making an instance of it, calling the getAllProducts and then logging the result to the console

```js
const dl = new DataLayer();

dl.getAllProducts()
    .then(console.log);
```  

### Add the datalayer to index.html

- Open the index.html file
- Drag the datalayer.js file into the code pane of the index.html, after the title tag, before the end of the head node. Alternatively, you can replace the content of index.html with

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title></title>
    <script src="js/datalayer.js"></script>
</head>
<body>

</body>
</html>
``` 

### Test the application

Run the application by pressing F5, then right click on the Solution Explorer -> JavaScriptClient -> Debug -> Start New Instance

Now press F12 in your browser to open the Developer Tools and inspect the Network Traffic (refresh if you don't see anything).
You should see this message

```
Fetch API cannot load http://localhost:5000/. No 'Access-Control-Allow-Origin' header is present on the requested resource. Origin 'http://localhost:5001' is therefore not allowed access. The response had HTTP status code 404. If an opaque response serves your needs, set the request's mode to 'no-cors' to fetch the resource with CORS disabled.
```

This is because the Service application does not accept incoming requests from the JavaScript client. 
Let's fix this.

### Add the Cors NuGet Package

To setup CORS for your MarketPlaceService application, add the Microsoft.AspNetCore.Cors package to your project.

- In the Solution Explorer, right click on MarketPlaceService -> Dependencies -> Manage NuGetPackages.
- Click on the tab Browse
- Type ```Microsoft.AspNetCore.Cors```
- Click Add

### Register and configure the CORS services in Startup.cs:

In the Solution Explorer, right click on MarketPlaceService -> Startup.cs. Replace the content of the ConfigureServices method with this

```cs
public void ConfigureServices(IServiceCollection services) {
    // requires using Microsoft.EntityFrameworkCore;
    services.AddDbContext<MarketPlaceContext>(opt => opt.UseInMemoryDatabase());

    services.AddCors(options =>
        options.AddPolicy("default", policy =>
            policy.WithOrigins("http://localhost:5001")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
        )
    );

    // Add framework services.
    services.AddMvc();

    services.AddSingleton<IProductsRepository, ProductsRepository>();
}
```

### Add CORS middleware

Replace the content of the Configure method with this:

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
    loggerFactory.AddConsole(Configuration.GetSection("Logging"));
    loggerFactory.AddDebug();

    app.UseCors("default");

    app.UseMvc();
}
```

### Test the application

Run the application by pressing F5, then right click on the Solution Explorer -> JavaScriptClient -> Debug -> Start New Instance

Now press F12 in your browser to open the Developer Tools and inspect the Console (refresh if you don't see anything). You should see an array with one product object.

### Add getProductById method

In the datalayer.js file, add a new method getProductById in the DataLayer class. The method should invoke the fetch method passing the this.serviceUrl + id as an address.

```js
getProductById(id) {
    return fetch(this.serviceUrl + "/" + id).then(function (response) {
        return response.json();
    });
}
``` 

Now let's see if it works by adding the new invocation at the end of our file:

```js
const dl = new DataLayer();

dl.getAllProducts()
    .then(console.log)
    .then(() => dl.getProductById(1))
    .then(console.log);
```

Refresh the page and check that the console shows the product.

### Add insertProduct method

In the datalayer.js file, add a new method insertProduct in the DataLayer class. The method should invoke the fetch method passing the this.serviceUrl as an address but also adding an object to configure the POST http  method, the content of the body and the ContentType header.

```js
insertProduct(product) {
    return fetch(this.serviceUrl, {
        method: 'POST',
        body: JSON.stringify(product),
        headers: new Headers({
            'Content-Type': 'application/json'
        })
    }).then(function (response) {
        return response.json();
    });
}
``` 

Now let's test it by adding the invocation at the end of our file:

```js
const dl = new DataLayer();

dl.getAllProducts()
    .then(console.log)
    .then(() => dl.getProductById(1))
    .then(console.log)
    .then(() => dl.insertProduct({ id: 0, name: "New Product", description: "Added through javascript", price: 9876 }))
    .then(console.log);
```

Refresh the page and check that the console shows the new product (with a different id value).

### Add updateProduct method

In the datalayer.js file, add a new method updateProduct in the DataLayer class. The method should invoke the fetch method passing the this.serviceUrl + an id as an address, but also adding an object to configure the PUT http  method, the content of the body and the ContentType header.

```js
updateProduct(id, product) {
    return fetch(this.serviceUrl + "/" + id, {
        method: 'PUT',
        body: JSON.stringify(product),
        headers: new Headers({
            'Content-Type': 'application/json'
        })
    });
}
``` 

Now let's test it by adding the invocation at the end of our file:

```js
const dl = new DataLayer();

dl.getAllProducts()
    .then(console.log)
    .then(() => dl.getProductById(1))
    .then(console.log)
    .then(() => dl.insertProduct({ id: 0, name: "New Product", description: "Added through javascript", price: 9876 }))
    .then(console.log)
    .then(() => dl.updateProduct(1, { id: 1, name: "Modified Product", description: "Modified through javascript", price: 9876 }))
    .then(() => dl.getProductById(1))
    .then(console.log);
```

Refresh the page and check that the console shows the modified product.

### Add deleteProduct method

In the datalayer.js file, add a new method deleteProduct in the DataLayer class. The method should invoke the fetch method passing the this.serviceUrl + an id as an address, but also adding an object to configure the DELETE http  method.

```js
deleteProduct(id) {
    return fetch(this.serviceUrl + "/" + id, {
        method: 'DELETE'
    });
}
```

Now let's test it by adding the invocation at the end of our file:

```js
const dl = new DataLayer();

dl.getAllProducts()
    .then(console.log)
    .then(() => dl.getProductById(1))
    .then(console.log)
    .then(() => dl.insertProduct({ id: 0, name: "New Product", description: "Added through javascript", price: 9876 }))
    .then(console.log)
    .then(() => dl.updateProduct(1, { id: 1, name: "Modified Product", description: "Modified through javascript", price: 9876 }))
    .then(() => dl.getProductById(1))
    .then(console.log)
    .then(() => dl.deleteProduct(1))
    .then(console.log);
```

Refresh the page and check that the console shows the response.

# Next steps

```
git add .
git commit -m "student: step 2 complete"
git checkout step03start 
```