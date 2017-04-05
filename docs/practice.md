# Lab 04 - Styling the View

We will now proceed to improve the appearance of the user interface through the use of [Bootstrap 4](https://v4-alpha.getbootstrap.com/), which is in its aplha 6 version as of the writing of this document (April 2017).  

We are going to modify the HTML of our Vue components to use the structure and css classes required by Bootstrap to style our UI. Much of the HTML we have written so far is enclosed between quotes, which makes it difficult to understand because we have no color coding, no intellisense, no auto complete and so on. This is why, before starting with Bootstrap, we also will modify our code by using Single File Vue Components, which will get us a much better support from Visual Studio. 

With .vue components, we’re entering the realm of advanced JavaScript applications. That means learning to use a few additional tools if you haven’t already:
- Node Package Manager (NPM): Read the Getting Started guide through section 10: Uninstalling global packages.
- Modern JavaScript with ES2015/16: Read through Babel’s Learn ES2015 guide. You don’t have to memorize every feature right now, but keep this page as a reference you can come back to.

We are going to use Webpack, a module bundler that takes a number of “modules” and then bundles them into your final application. 
In Webpack, each module can be transformed by a “loader” before being included in the bundle, and Vue offers the vue-loader plugin to take care of translating .vue single-file components.

Babel will allow us to use ES2015 syntax.

## Setting up webpack and Babel

### Download the packages

In the Solution Explorer, open the package.json file in the JavaScriptClient project. Replace the ```devDependencies``` element with the following code:

```json
"devDependencies": {
   "babel-core": "^6.24.0",
   "babel-loader": "^6.4.1",
   "babel-preset-env": "^1.3.2",
   "babel-polyfill": "^6.23.0",
   "webpack": "^2.3.3",
   "vue-loader": "^11.3.4",
   "vue-template-compiler": "^2.2.6"
 }
 ```

Ensure to save the file so that Visual Studio starts downloading the packages.

### Configure Webpack

We now need to configure WebPack. We will indicate:

- the entry point of our application (a file we still have to create, we'll do it later)
- the output to produce (the file that we will add to our index.html page instead of the js we added so far)
- which loaders to use (vue-loader for .vue files and babel-loader for .js files)

In the root of your JavaScriptClient project, add a new JavaScript file named ```webpack.config.js``` and replace its content with the following code:

```js
var path = require('path')
var webpack = require('webpack');

module.exports = {
    entry: ['babel-polyfill','./wwwroot/js/src/main.js'],
    output: {
        path: path.resolve(__dirname, './wwwroot/js/dist'),
        filename: 'bundle.js'
    },
    module: {
        rules: [
            {
                test: /\.vue$/,
                loader: 'vue-loader'
            },
            {
                test: /\.js$/,
                loader: 'babel-loader',
                exclude: /node_modules/
            }
        ]
    },
    resolve: {
        alias: {
            'vue$': 'vue/dist/vue.esm.js'
        }
    },
    devtool: 'source-map'
}
```

### Configure Babel

We now need to configure Babel so that it can use the Env preset, which automatically determines the Babel plugins you need based on your supported environments. 

In the root of your JavaScriptClient project, add a json file named ```.babelrc``` and replace its content with the following code:

```json
{
  "presets": ["env"]
}
```

## Split our view into Single Page Vue Components

We will now split our viewmodel.js into multiple .vue file, plus one main.js that will create the Vue Instance. 

A *.vue file is a custom file format that uses HTML-like syntax to describe a Vue component. Each *.vue file consists of three types of top-level language blocks: <template>, <script>, and <style>, and optionally additional custom blocks.

Into your ```JavaScriptClient/wwwroot/js``` folder, create a new folder ```src```

### ProductItem Component

Into the ```JavaScriptClient/wwwroot/js/src``` folder, create a new ProductItem.vue file.

Create two sections, one for the template and one for the script:

```html
<template></template>

<script></script>
```

Now open the viewmodel.js file, select the template content of the product-item component and paste it in the ```<template>``` section of the new component:

```html
<template>
    <article>
        {{ prod.id }} - {{prod.name}} - {{prod.description}} - {{prod.price}}
        <button v-on:click="select" v-bind:disabled="buttonsDisabled">Select</button>
        <button v-on:click="remove" v-bind:disabled="buttonsDisabled">Delete</button>
    </article>
</template>
```

Now copy the ```props``` and ```methods``` of the ```product-item``` from ```viewmodel.js``` to the ```<script>``` section of ```ProductItem.vue```, surrounding it in an ```export default {}``` component:

```html
<script>
    export default {
        props: ['prod', 'buttonsDisabled'],
        methods: {
            select() {
                this.$emit('selected', this.prod);
            },
            remove() {
                this.$emit('deleting', this.prod);
            }
        }
    }
</script>
```

### ProductForm

The first Vue Component is ready. Let's create the ProductForm.
In your ```JavaScriptClient/wwwroot/js/src``` folder, create a ```ProductForm.vue``` and replace its code with the following:

```html
<template>

</template>

<script>
    export default {

    }
</script>
```

Now open the viewmodel.js file, select the template content of the product-form component and paste it in the ```<template>``` section of the new component:

```html
<template>
    <div>
        <input v-model="product.name" type="text" placeholder="name" />
        <input v-model="product.description" type="text" placeholder="description" />
        <input v-model:number="product.price" type="text" placeholder="price" />
        <button v-on:click="save">Save</button>
        <button v-on:click="cancel">Cancel</button>
    </div> 
</template>
```
Now copy the ```props``` and ```methods``` of the ```product-form``` from ```viewmodel.js``` to the ```<script>``` section of ```ProductForm.vue```:

```html
<script>
    export default {
        props: ['product'],
        methods: {
            save: function () {
                this.$emit('saving', this.product);
            },
            cancel: function () {
                this.$emit('cancel', this.product);
            }
        }
    }
</script>
```

### App

Now let's create an App Vue Component, that will include ProductItem and ProductForm.

In your ```JavaScriptClient/wwwroot/js/src``` folder, create an ```App.vue``` and replace its code with the following:

```html
<template>

</template>

<script>
    
</script>
```

Now open your ```index.html```, select the content of the ```<body>``` section and copy the code into the ```<template>``` section of your App.js.

```html
<template>
    <div id="app">
        <button v-on:click="add" v-bind:disabled="isFormInUse">New Product</button>

        <product-form v-if="isFormInUse" v-bind:product="current" v-on:saving="productSaving" v-on:cancel="cancel"></product-form>

        <section>
            <product-item v-for="product in products" v-bind:prod="product" v-bind:buttons-disabled="isFormInUse" :key="product.id" v-on:selected="productSelected" v-on:deleting="productDeleting"></product-item>
        </section>
    </div>
</template>
```

Now we need to fill the ```<script>``` section with the content of our old Vue instance, but we will have to make some changes:
- The component won't have an ```el``` property but a ```name``` property
- The ```data``` property will be a function that returns an object
- We will add a ```components``` property to reference the two Vue Component we just made
- We will import those components and the datalayer so that webpack can pass them to the loader and add the transpiled version into the bundle

This means that the ```<script>``` section becomes as follows:

```html
<script>
    import DataLayer from "./datalayer"
    import ProductForm from './ProductForm.vue'
    import ProductItem from './ProductItem.vue'  

    export default {
        name: 'app',
        data() {
            return {
                products: [],
                current: { id: 0, name: "", description: "", price: 0 },
                isFormInUse: false
            }
        },
        mounted () {
            new DataLayer()
                .getAllProducts()
                .then(products => this.products = products);
        },
        methods: {
            productSelected (selectedProduct) {
                this.current = selectedProduct;
                this.isFormInUse = true;
            },
            productSaving (productToSave) {
                if (productToSave.id == 0) {
                    new DataLayer().insertProduct(productToSave)
                        .then(product => {
                            this.products.push(product);
                        });
                } else {
                    new DataLayer().updateProduct(productToSave.id, productToSave);
                }
                this.isFormInUse = false;
            },
            add () {
                this.current = { id: 0, brand: "", name: "", price: 0 };
                this.isFormInUse = true;
            },
            productDeleting (product) {
                new DataLayer().deleteProduct(product.id)
                    .then(() => this.products.splice(this.products.indexOf(product), 1));
            },
            cancel(product) {
                if (product.id == 0) {
                    this.current.id = 0;
                    this.current.name = "";
                    this.current.description = "";
                    this.current.price = 0;
                } else {
                    new DataLayer().getProductById(product.id).then(p => {
                        this.current.id = p.id;
                        this.current.name = p.name;
                        this.current.description = p.description;
                        this.current.price = p.price;
                    });
                }
                this.isFormInUse = false;
            }
        },
        components: {
            ProductForm,
            ProductItem
        }
    }
</script>
``` 

Now we need to create a Vue Instance to render our App component.

Vue recommends using templates to build your HTML in the vast majority of cases. There are situations however, where you really need the full programmatic power of JavaScript. That’s where you can use the render function, a closer-to-the-compiler alternative to templates.

In your ```JavaScriptClient/wwwroot/js/src```, create a ```main.js``` file and replace its content with the following code:

```js
import Vue from 'vue'
import App from './App.vue'

new Vue({
    el: '#app',
    render: h => h(App)
})
```

### DataLayer.js

Let's export the definition of our DataLayer class.
First, move ```JavaScriptClient/wwwroot/js/datalayer.js``` to the ```JavaScriptClient/wwwroot/js/src``` folder.

Now open ```JavaScriptClient/wwwroot/js/src/datalayer.js``` and replace the first line of code

```js
class DataLayer {
```

with

```js
export default class {
``` 

### index.html

We can now change our page to include the bundled file and remove the old code for the vue instance, that is now included in the component.

Open and replace its content with the following code:

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title></title>
</head>
<body>
    <div id="app"></div>
    <script src="js/dist/bundle.js"></script>
</body>
</html>
```

### Build the bundle

Open a command prompt and go to the root folder of your JavaScript project, containing your webpack.config file. Type:

```
webpack
```

You should see the ````bundle.js``` output file in the ```JavaScriptClient/wwwroot/js/dist``` folder.

If you run the application you should see no difference in behaviour, but our code is now better organizeed and easier to change.
We can now proceed to use Bootstrap 4 to improve the appearance of the User Interface.

### Bootstrap setup

Although we could of course download Bootstrap and include it in our project, we're going to use the CDN instead.

Open the JavaScriptClient project / wwwroot / index.html

In the ```<head>``` section, add the links to the bootstrap cs:

```html
<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-alpha.6/css/bootstrap.min.css" integrity="sha384-rwoIResjU2yc3z8GV/NPeZWAv56rSmLldC3R/AZzGRnGxQQKnKkoFVhFQhNUwEyJ" crossorigin="anonymous">
```

Add the JavaScript plugins, jQuery, and Tether near the end of your page, right before the closing ```</body>``` tag. Be sure to place jQuery and Tether first, as the Bootstrap code depends on them.

```html
<script src="https://code.jquery.com/jquery-3.1.1.slim.min.js" integrity="sha384-A7FZj7v+d/sdmMqp/nOQwliLvUsJfDHW+k9Omg/a/EheAdgtzNs3hpfag6Ed950n" crossorigin="anonymous"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/tether/1.4.0/js/tether.min.js" integrity="sha384-DztdAPBWPRXSA/3eYEEUWrWCy7G5KFbe8fFjk5JAIxUYHKkDx6Qin1DkWx51bBrb" crossorigin="anonymous"></script>
<script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-alpha.6/js/bootstrap.min.js" integrity="sha384-vBWWzlZJ8ea9aCX4pEW3rVHjgjt7zpkNpZk+02D9phzyeVkE+jo0ieGizqPLForn" crossorigin="anonymous"></script>
```

### Add the meta viewport tag

Bootstrap is developed mobile first, a strategy in which the code is optimized for mobile devices first and then scale up components as necessary using CSS media queries. To ensure proper rendering and touch zooming for all devices, add the responsive viewport meta tag to your ```<head>```

```html
<meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
``` 

Run the Solution by pressing F5, then right click on the JavaScriptClient project and select Debug -> Start a New Instance.
You should already see a slight improvement in the looks of the page.

### Setting the container

Containers are the most basic layout element in Bootstrap and are required when using the default grid system. We can choose a responsive, fixed-width container (meaning its max-width changes at each breakpoint) or fluid-width (meaning it’s 100% wide all the time).

Let's make our app a fixed-width container by adding the class ```container``` to the App.vue template:

```html
<div id="app" class="container">
```

Save, run webpack, refresh the page in the browser. You should see some margin on the left of our content. 

Since Bootstrap is developed to be mobile first, media queries are used to create sensible breakpoints for layouts and interfaces. These breakpoints are mostly based on minimum viewport widths and are used to scale up elements as the viewport changes. Try to resize the width of the broser to see how the margin changes.

### Styling the buttons

Bootstrap includes six predefined button styles, each serving its own semantic purpose. Let's style our buttons.

First, let's add the classes ```btn btn-primary``` to our New Product button on App.vue

```html
<button class="btn btn-primary" v-on:click="add" v-bind:disabled="isFormInUse">New Product</button>
```

Second, let's change the buttons in the ```product-item``` child template.
Open the JavaScriptClient / wwwroot / js / src / ProductItem.vue file.
Modify the ```<template>``` section as follows:

```html
<article>
    {{ prod.id }} - {{prod.name}} - {{prod.description}} - {{prod.price}}
    <button class="btn btn-secondary" v-on:click="select" v-bind:disabled="buttonsDisabled">Select</button>
    <button class="btn btn-danger" v-on:click="remove" v-bind:disabled="buttonsDisabled">Delete</button>
</article>
```

Third, let's style the buttons on our ```ProductForm.vue``` template.

```html
<div>
    <input v-model="product.name" type="text" placeholder="name" />
    <input v-model="product.description" type="text" placeholder="description" />
    <input v-model:number="product.price" type="text" placeholder="price" />
    <button class="btn btn-primary" v-on:click="save">Save</button>
    <button class="btn btn-secondary" v-on:click="cancel">Cancel</button>
</div>

```

Save, run webpack, refresh the page in the browser. You should see the buttons with the new style. Also note how the mouse pointer icon changes when they are disabled.  

### Styling our products

We're going to use the new Card component introduced in Bootstrap 4

A card is a flexible and extensible content container. It includes options for headers and footers, a wide variety of content, contextual background colors, and powerful display options.

Cards are built with as little markup and styles as possible, but still manage to deliver a ton of control and customization. Built with flexbox, they offer easy alignment and mix well with other Bootstrap components.

Let's use a basic card with mixed content and a fixed width. Cards have no fixed width to start, so they’ll naturally fill the full width of its parent element. 

Add the ```class="card"``` attribute to the ```article``` tag in the ```<template>``` section of the ```ProductItem.vue``` Component:

```html
<article class="card">
    {{ prod.id }} - {{prod.name}} - {{prod.description}} - {{prod.price}}
    <button class="btn btn-secondary" v-on:click="select" v-bind:disabled="buttonsDisabled">Select</button>
    <button class="btn btn-danger" v-on:click="remove" v-bind:disabled="buttonsDisabled">Delete</button>
</article>
```

Save, run webpack, refresh the page in the browser. You will see a border around each product. Now the buttons fill the card.

The building block of a card is the ```.card-block```. Let's use it to add a padded section within a card. 

Add a ```<div class="card-block">``` to wrap the content of the card.

```html
<article class="card">
    <div class="card-block">
        {{ prod.id }} - {{prod.name}} - {{prod.description}} - {{prod.price}}
        <button class="btn btn-secondary" v-on:click="select" v-bind:disabled="buttonsDisabled">Select</button>
        <button class="btn btn-danger" v-on:click="remove" v-bind:disabled="buttonsDisabled">Delete</button>
    </div>
</article>
``` 

Save, run webpack, refresh the page in the browser. There is more space surrounding the content and the buttons are back to a normal size.

### Titles and text

Card titles are used by adding ```.card-title``` to a ```<h*>``` tag. 
Subtitles are used by adding a ```.card-subtitle``` to a ```<h*>``` tag. 
If the ```.card-title``` and the ```.card-subtitle``` items are placed in a ```.card-block``` item, the card title and subtitle are aligned nicely.

Let's add some tags and classes to our template to style it nicely.

```html
<article class="card">
    <div class="card-block">
        <h4 class="card-title">{{ prod.id }} - {{prod.name}}</h4>
        <h6 class="card-subtitle mb-2 text-muted">{{prod.price}}</h6>
        <p class="card-text">{{prod.description}}</p>
        <button class="btn btn-secondary" v-on:click="select" v-bind:disabled="buttonsDisabled">Select</button>
        <button class="btn btn-danger" v-on:click="remove" v-bind:disabled="buttonsDisabled">Delete</button>
    </div>
</article>
```

Save, run webpack, refresh the page in the browser. The content of the block is now aligned nicely.

### Header and footer

Let's add a header and footer within our card by adding two more divs with the classes ```card-header``` and ```card-footer```.
Let's move the prod.id in the header and the two buttons in the footer.

```html
<article class="card">
    <div class="card-header">
        {{ prod.id }}
    </div>
    <div class="card-block">
        <h4 class="card-title">{{prod.name}}</h4>
        <h6 class="card-subtitle mb-2 text-muted">{{prod.price}}</h6>
        <p class="card-text">{{prod.description}}</p>
    </div>
    <div class="card-footer">
        <button class="btn btn-secondary" v-on:click="select" v-bind:disabled="buttonsDisabled">Select</button>
        <button class="btn btn-danger" v-on:click="remove" v-bind:disabled="buttonsDisabled">Delete</button>
    </div>
</article>
```

Save, run webpack, refresh the page in the browser. Now the id and the buttons are in two separate areas.

### Sizing

Cards assume no specific width to start, so they’ll be 100% wide unless otherwise stated. You can change this as needed with grid classes.
Bootstrap includes a powerful mobile-first flexbox grid system for building layouts of all shapes and sizes. It’s based on a 12 column layout and has multiple tiers, one for each media query range. You can use it the Bootstrap predefined classes.
Bootstrap’s grid system uses a series of containers, rows, and columns to layout and align content. It’s built with flexbox and is fully responsive.
Breaking it down, here’s how it works:

- Containers provide a means to center your site’s contents. Use ```.container``` for fixed width or ```.container-fluid``` for full width.
- Rows are horizontal groups of columns that ensure your columns are lined up properly. Bootstrap uses the negative margin method on ```.row``` to ensure all your content is aligned properly down the left side.
- Content should be placed within columns, and only columns may be immediate children of rows.
- Column classes indicate the number of columns you’d like to use out of the possible 12 per row. So, if you want three equal-width columns, you can use ```.col-md-4```.
- Column widths are set in percentages, so they’re always fluid and sized relative to their parent element.
- There are five grid tiers, one for each responsive breakpoint: all breakpoints (extra small), small, medium, large, and extra large.
- Grid tiers are based on minimum widths, meaning they apply to that one tier and all those above it (e.g., ```.col-md-4``` applies to  medium, large, and extra large devices).

We already have a container (the app). Let's add the  ```class="row"``` to our ```<section>``` wrapping the ```<product-item>``` in the ```App.vue```.

```html
<section class="row">
```

Now let's specify how many columns each card should be by wrapping it in a ```<div>``` with a ```class="col-sm-4"``` so that each row will fit 3 products.

```html
<div class="col-md-4">
    <article class="card">
        <div class="card-header">
            {{ prod.id }}
        </div>
        <div class="card-block">
            <h4 class="card-title">{{prod.name}}</h4>
            <h6 class="card-subtitle mb-2 text-muted">{{prod.price}}</h6>
            <p class="card-text">{{prod.description}}</p>
        </div>
        <div class="card-footer">
            <button class="btn btn-secondary" v-on:click="select" v-bind:disabled="buttonsDisabled">Select</button>
            <button class="btn btn-danger" v-on:click="remove" v-bind:disabled="buttonsDisabled">Delete</button>
        </div>
    </article>
</div>
```

Save, run webpack, refresh the page in the browser. Try adding some products to see how they span on the page. Also, resize the page to see how the layout changes when the browser reaches the different breakpoints.

### Styling the form

Bootstrap provides several form control styles, layout options, and custom components for creating a wide variety of forms.
Bootstrap’s form controls expand on the Rebooted form styles with classes. 
Use these classes to opt into their customized displays for a more consistent rendering across browsers and devices.

Let's change the ```<template>``` section of the ```ProductForm.vue``` child component by adding the ```class="form-control"``` attribute to all the input fields.

```html
<div>
    <input class="form-control" v-model="product.name" type="text" placeholder="name" />
    <input class="form-control" v-model="product.description" type="text" placeholder="description" />
    <input class="form-control" v-model:number="product.price" type="text" placeholder="price" />
    <button class="btn btn-primary" v-on:click="save">Save</button>
    <button class="btn btn-secondary" v-on:click="cancel">Cancel</button>
</div> 
```

Save, run webpack, refresh the page in the browser. The controls now have rounded borders and fill the width of the container.

Let's change the ```description``` textbox into a textarea, to give the user the chance to insert a multiline description.

```html
<div>
    <input class="form-control" v-model="product.name" type="text" placeholder="name" />
    <textarea class="form-control" v-model="product.description" placeholder="description"></textarea>
    <input class="form-control" v-model:number="product.price" type="text" placeholder="price" />
    <button class="btn btn-primary" v-on:click="save">Save</button>
    <button class="btn btn-secondary" v-on:click="cancel">Cancel</button>
</div> 
```   

Let's add some labels next to each field to make the form more clear.

```html
<div>
    <label for="productName">Name</label><input class="form-control" v-model="product.name" type="text" placeholder="name" id="productName"/>
    <label for="productDescription">Description</label><textarea class="form-control" v-model="product.description" placeholder="description" id="productDescription"></textarea>
    <label for="productPrice">Price</label><input class="form-control" v-model:number="product.price" type="text" placeholder="price" id="productPrice"/>
    <button class="btn btn-primary" v-on:click="save">Save</button>
    <button class="btn btn-secondary" v-on:click="cancel">Cancel</button>
</div> 
```

Save, run webpack, refresh the page in the browser. The labels are above each control.

### Change Form Layout

Since Bootstrap applies display: block and width: 100% to almost all form controls, forms will by default stack vertically. Additional classes can be used to vary this layout on a per-form basis.

Form groups

The ```.form-group``` class is the easiest way to add some structure to forms. Its only purpose is to provide margin-bottom around a label and control pairing. As a bonus, since it's a class you can use it with ```<fieldset>```s, ```<div>```s, or nearly any other element.

Let's add form groups to our form.

```html
<div>
    <div class="form-group">
        <label for="productName">Name</label>
        <input class="form-control" v-model="product.name" type="text" placeholder="name" id="productName"/>
    </div>
    <div class="form-group">
        <label for="productDescription">Description</label>
        <textarea class="form-control" v-model="product.description" placeholder="description" id="productDescription"></textarea>
    </div>
    <div class="form-group">
        <label for="productPrice">Price</label>
        <input class="form-control" v-model:number="product.price" type="text" placeholder="price" id="productPrice"/>
    </div>
    <button class="btn btn-primary" v-on:click="save">Save</button>
    <button class="btn btn-secondary" v-on:click="cancel">Cancel</button>
</div> 
```

Save, run webpack, refresh the page in the browser. There's now some vertical space between one group and the other.

We can use the ```row``` and ```col-*-*``` grid classes to place each label next to the fields.

```html
<div>
    <div class="form-group row">
        <label for="productName" class="col-sm-2">Name</label>
        <input class="form-control col-sm-10" v-model="product.name" type="text" placeholder="name" id="productName"/>
    </div>
    <div class="form-group row">
        <label for="productDescription" class="col-sm-2">Description</label>
        <textarea class="form-control col-sm-10" v-model="product.description" placeholder="description" id="productDescription"></textarea>
    </div>
    <div class="form-group row">
        <label for="productPrice" class="col-sm-2">Price</label>
        <input class="form-control col-sm-10" v-model:number="product.price" type="text" placeholder="price" id="productPrice"/>
    </div>
    <button class="btn btn-primary" v-on:click="save">Save</button>
    <button class="btn btn-secondary" v-on:click="cancel">Cancel</button>
</div> 
```

Save, run webpack, refresh the page in the browser. The labels are now on the left of each field.

We can also transform our form into a card by adding the correct elements and classes.

```html
<div class="card">
    <div class="card-block">
        <div class="form-group row">
            <label for="productName" class="col-sm-2">Name</label>
            <input class="form-control col-sm-10" v-model="product.name" type="text" placeholder="name" id="productName"/>
        </div>
        <div class="form-group row">
            <label for="productDescription" class="col-sm-2">Description</label>
            <textarea class="form-control col-sm-10" v-model="product.description" placeholder="description" id="productDescription"></textarea>
        </div>
        <div class="form-group row">
            <label for="productPrice" class="col-sm-2">Price</label>
            <input class="form-control col-sm-10" v-model:number="product.price" type="text" placeholder="price" id="productPrice"/>
        </div>
    </div>
    <div class="card-footer">
        <button class="btn btn-primary" v-on:click="save">Save</button>
        <button class="btn btn-secondary" v-on:click="cancel">Cancel</button>
    </div>
</div> 
```

Save, run webpack, refresh the page in the browser. There is a slight border around our form and the buttons are in a separate area at the bottom of the form.

### Navigation

The navbar is a wrapper that positions branding, navigation, and other elements in a concise header. It’s easily extensible and, thanks to the Collapse plugin, can easily integrate responsive behaviors.

Here's what you need to know before getting started with the navbar:

- Navbars require a wrapping ```.navbar``` with ```.navbar-toggleable-*``` for responsive collapsing and color scheme classes.
- Navbars and their contents are fluid by default. Use optional containers to limit their horizontal width.
- Navbars and their contents are built with flexbox, providing easy alignment options via utility classes.
- Navbars are responsive by default, but you can easily modify them to change that. Responsive behavior depends on the Collapse JavaScript plugin.
- Ensure accessibility by using a ```<nav>``` element or, if using a more generic element such as a ```<div>```, add a ```role="navigation"``` to every navbar to explicitly identify it as a landmark region for users of assistive technologies.

Navbars come with built-in support for a handful of sub-components. Choose from the following as needed:

- ```.navbar-brand``` for your company, product, or project name.
- ```.navbar-nav``` for a full-height and lightweight navigation (including support for dropdowns).
- ```.navbar-toggler``` for use with our collapse plugin and other navigation toggling behaviors.
- ```.form-inline``` for any form controls and actions.
- ```.navbar-text``` for adding vertically centered strings of text.
- ```.collapse.navbar-collapse``` for grouping and hiding navbar contents by a parent breakpoint.

Let's create a new Sigle Page Vue Component to separate the navigation bar from the App.

In your ```JavaScriptClient/wwwroot/js/src``` folder, create a ```CommandBar.vue``` file and replace its code with the following:

```html
<template>

</template>

<script>
    export default {

    }
</script>
```

Now move the ```New Product``` button from ```App.vue``` to the ```<template>``` section of ```CommandBar.vue```:

```html
<template>
    <button class="btn btn-primary" v-on:click="add" v-bind:disabled="isFormInUse">New Product</button>
</template>
```

Now let's wrap our ```New Product``` button into a navigation bar:

```html
<template>
    <nav class="navbar navbar-inverse bg-inverse navbar-toggleable-md">
        <button class="navbar-toggler navbar-toggler-right" type="button" data-toggle="collapse" data-target="#commandBar" aria-controls="commandBar" aria-expanded="false" aria-label="Toggle navigation">
            <span class="navbar-toggler-icon"></span>
        </button>
        <a class="navbar-brand" href="#">MarketPlace</a>

        <div class="collapse navbar-collapse" id="commandBar">
            <div class="form-inline my-2 my-lg-0">
                <button class="btn btn-primary" v-on:click="add" v-bind:disabled="isFormInUse">New Product</button>
            </div>
        </div>
    </nav>
</template>
```

Now let's take care of the communication between the ```CommandBar``` component and the ```App``` component. 

- The ```App``` component has to pass down the ```isFormInUse``` property so that the button can eventually be disabled. We will create a prop for that.
- The ```CommandBar``` component has to inform the ```App``` whenever the button gets pressed. We will emit an event for that.

Modify the ```<script>``` section as follows:

```html
<script>
    export default {
        props: ['isFormInUse'],
        methods: {
            add() {
                this.$emit('add');
            }
        }
    }
</script>
```

Now let's go to the ```App``` component to include the ```CommandBar``` and to bind the property and event.
Open the ```JavaScriptClient/wwwroot/js/src/App.vue``` file and modify its ```<template>``` section as follows:

```html
<template>
    <div id="app" class="container">
        <command-bar v-on:add="add" v-bind:isFormInUse="isFormInUse"></command-bar>
        
        <product-form v-if="isFormInUse" v-bind:product="current" v-on:saving="productSaving" v-on:cancel="cancel"></product-form>

        <section class="row">
            <product-item v-for="product in products" v-bind:prod="product" v-bind:buttons-disabled="isFormInUse" :key="product.id" v-on:selected="productSelected" v-on:deleting="productDeleting"></product-item>
        </section>
    </div>
</template>
```

At the beginning of the ```<script>``` section, include an ```import``` for the new ```CommandBar.vue``` as follows:

```html
<script>
    import DataLayer from "./datalayer"
    import ProductForm from './ProductForm.vue'
    import ProductItem from './ProductItem.vue'  
    import CommandBar from './CommandBar.vue'  
    export default {
        // code that was already here


    }
```

Also add ```CommandBar``` between the ```components``` property of the ```App```:

```js
    components: {
            ProductForm,
            ProductItem,
            CommandBar
    }
```

Save, run webpack, refresh the page in the browser. The Button is now in a navigation bar. Note that if you resize the browser, the content of the navbar collapses, a toggle button appears and if you click on it you can see the New Product button slowly sliding down.

Our REST Service is poorly documented and we had to build our JavaScript DataLayer knowing all the details of the different http verbs, content types and data structures. We are going to improve that by using Swagger, Swashbuckle and Swagger-Client in the next lab.    

# Next steps

```
git add .
git commit -m "student: step 4 complete"
git checkout step05start
```