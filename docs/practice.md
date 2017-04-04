# Lab 03 - The View

We're now going to focus on the user interface of our JavaScript project.
There are many javascript frameworks to choose from. In this project we're going to use [Vue.js](https://vuejs.org/)  
We are going to start in a simple way that will allow us to quickly build a user interface. However, some of the html we will write will be enclosed between quotes, which is not very practical and is error prone.
In a following lab we will modify our code to use Single File Components, which will be a better option but will require some initial setup that we want to skip for now because we want to focus on the UI.

### Add a dependency to vue.js

In the Solution Explorer, right click on the JavascriptClient project, select Add -> New Item.

Under ASP.NET Core -> Web -> General, select "npm Configuration File".
Under Name, leave ```package.json``` and click Add.
Replace the content with

```json
{
  "version": "1.0.0",
  "name": "asp.net",
  "private": true,
  "devDependencies": {
  },
  "dependencies": {
    "vue": "^2.2.6"
  }
}
```

Ensure to save the file so that Visual Studio can start the download of the framework.

### Add a reference to vue.js in the index.html page

For now we are going to manually add the vue.js reference to the page. In a following lab we are going to find a better solution.

- On the Solution Explorer, click on "Show All Files". 
- Open node_modules/vue/dist/ 
- Copy the vue.js file into the wwwroot/js folder
- Open JavascriptClient/wwwroot/index.html
- Replace the head node with the following code:

```html
<head>
    <meta charset="utf-8" />
    <title></title>
    <script src="js/vue.js"></script>
    <script src="js/datalayer.js"></script>
</head>
```

### Create the Vue Instance

- In the Solution Explorer, right click on JavaScriptClient -> wwwroot -> js, select Add Item
- Under ASP.NET Core -> Web -> Scripts, select "javascript file".
- Under Name, type ```viewmodel.js``` and click Add.
- Add a reference to viewmodel.js in the index.html page by replacing the head node of index.html with

```html
<head>
    <meta charset="utf-8" />
    <title></title>
    <script src="js/vue.js"></script>
    <script src="js/datalayer.js"></script>
    <script src="js/viewmodel.js"></script>
</head>
```

- Open JavaScriptClient/wwwroot/js/viewmodel.js.
- Add the code to wait that the DOM is loaded:

```js
window.onload = () => {

};
```

Every Vue vm is bootstrapped by creating a root Vue instance with the Vue constructor function. 

```js
window.onload = () => {
    const vm = new Vue({
        // options
    });
};
```

When you instantiate a Vue instance, you need to pass in an options object which can contain options for data, template, element to mount on, methods, lifecycle callbacks and more.

```js
window.onload = () => {
    const vm = new Vue({
        el: '#app', //this is the id of the element on the page that will be taken care of by Vue
        data: { 
            //this is the data that Vue will use to render the html
        }
    });
};
```

For now, let's add some fake product in the data object, just to see if it works. 
We will call the server later. 

```js
window.onload = () => {
    const vm = new Vue({
        el: "#app",
        data: {
            products: [
                { id: 1, name: "Product 1", description: "Fake product", price: 111 },
                { id: 2, name: "Product 2", description: "Another fake product", price: 222 },
                { id: 3, name: "Product 3", description: "Yet another product", price: 333 }
            ],
        }
    });
};
```

Now let's move to index.html, where we will define the html to render.
At the core of Vue.js is a system that enables us to declaratively render data to the DOM using straightforward template syntax:

```html
    <div id="app">
        <section>
            <article v-for="product in products">
                {{ product.id }} - {{product.name}} - {{product.description}} - {{product.price}}
            </article>
        </section>
    </div>
```

As you can see, this template is essentially HTML mixed with a few special Mustache-like tags. A Mustache tag begins with two opening braces ({{) and ends with two closing braces (}}).

Inside each tag is the name of a variable (in this case, id, name, description and price). When Vue processes the template, it replaces each variable name with an actual value.

The v-for attribute you are seeing is called a directive. Directives are prefixed with v- to indicate that they are special attributes provided by Vue, and as you may have guessed, they apply special reactive behavior to the rendered DOM. Here it is basically saying "repeat the article tag for each product item found in the products property of the data object".
The data and the DOM are now linked, and everything is now reactive. 

### Test the application

Run The application by pressing F5, then right click on the JavaScript project in the Solution Explorer, select Debug -> Start New Instance and navigate to ```http://localhost:5001```. The index.html page should show the 3 fake products.

### Call the service and update the products with the result

Each Vue instance goes through a series of initialization steps when it is created - for example, it needs to set up data observation, compile the template, mount the instance to the DOM, and update the DOM when data changes. Along the way, it will also invoke some lifecycle hooks, which give us the opportunity to execute custom logic. We can use the mounted hook to call the server and replace the products array with the result.

```js
window.onload = () => {
    const vm = new Vue({
        el: "#app",
        data: {
            products: [
                { id: 1, name: "Product 1", description: "Fake product", price: 111 },
                { id: 2, name: "Product 2", description: "Another fake product", price: 222 },
                { id: 3, name: "Product 3", description: "Yet another product", price: 333 }
            ],
        },
        mounted: function () {
            new DataLayer()
                .getAllProducts()
                .then(products => this.products = products);
        }
    });
};
```

Save the file and refresh the page. You should now see the products coming from the server instead of the fake ones.
We can in fact remove the fake product items altogether now, but be sure to leave the property, initializing it with an empty array.

```js
window.onload = () => {
    const vm = new Vue({
        el: "#app",
        data: {
            products: [ ],
        },
        mounted: function () {
            new DataLayer()
                .getAllProducts()
                .then(products => this.products = products);
        }
    });
};
```

We can also remove the lines of code at the end of the declaration of our DataLayer class that we used in the previous lab to test if everything was working.

### Replacing the Product item using a Component

What are Components?

Components are one of the most powerful features of Vue. They help you extend basic HTML elements to encapsulate reusable code. At a high level, components are custom elements that Vue's compiler attaches behavior to.

To register a global component, you can use Vue.component(tagName, options). For example:

```js
Vue.component('product-item', {
  // options
});
```

Once registered, a component can be used in an instance's template as a custom element, ```<product-item></product-item>```. 

Every component instance has its own isolated scope. This means you cannot (and should not) directly reference parent data in a child component's template. Data can be passed down to child components using **props**.
A prop is a custom attribute for passing information from parent components. A child component needs to explicitly declare the props it expects to receive using the props option.

Here is the code of our product-item component.

Make sure the component is registered before you instantiate the root Vue instance.

```js
Vue.component('product-item', {
    props: ['prod'],
    template: 
`<article>
    {{ prod.id }} - {{prod.name}} - {{prod.description}} - {{prod.price}}
</article>`
});
```

We can now change the index.html like this.

```html
<div id="app">
    <section>
        <product-item v-for="product in products" v-bind:prod="product" :key="product.id"></product-item>
    </section>
</div>
```

Refresh the page. You should see the same result as before, with articles rendered for each server side product.

The next step that we want to achieve is to have the user select a particular product in the list, then give the possibility to edit the fields through some textboxes.

- On the HTML templates we need
    - A Button for each product item
    - Textboxes to edit each field
- On the javascript code we need
    - To introduce the concept of current item
    - To handle and emit events

Let's start with the HTML templates
We will first add a button on the template of the child component

### Add a button on the Product Item Component

```js
Vue.component('product-item', {
    props: ['prod'],
    template: 
`<article>
{{ prod.id }} - {{prod.name}} - {{prod.description}} - {{prod.price}}
<button>Select</button>
</article>`
});
```
Refreshing the page will show the button for each product item

Now we want to add some textboxes to the parent element that will display the currently selected item. We won't need a textbox for the id, because we don't want the user to edit it.

### Add textboxes to the parent 

```html
<div id="app">
    <div>
        <input type="text" placeholder="name" />
        <input type="text" placeholder="description" />
        <input type="text" placeholder="price" />
    </div> 

    <section>
        <product-item v-for="product in products" v-bind:prod="product" :key="product.id"></product-item>
    </section>
</div>
```

Now we want to let the child component communicate with the parent component.

So first of all, we need the child component to handle the button click event. We can use the v-on directive to listen to DOM events and run some JavaScript when they're triggered. v-on can accept the name of a method you'd like to call.

```js
Vue.component('product-item', {
    props: ['prod'],
    template: 
`<article>
{{ prod.id }} - {{prod.name}} - {{prod.description}} - {{prod.price}}
<button v-on:click="select">Select</button>
</article>`,
    methods: {
        select: function () {
            console.log("someone clicked on the button!");
            console.log(this.prod);
        }
    }
});
```

If you refresh the page and click on the button you will see a message on the console.

We have learned that the parent can pass data down to the child using props, but how do we communicate back to the parent when something happens? This is where Vue's custom event system comes in.

Every Vue instance implements an events interface, which means it can:

- Listen to an event using ```$on(eventName)```
- Trigger an event using ```$emit(eventName)```

In addition, a parent component can listen to the events emitted from a child component using v-on directly in the template where the child component is used.

So what we need to do is trigger a custom event in the child and listen to the event in the parent.
Let's do that.

This is what our child becomes:

```js
Vue.component('product-item', {
    props: ['prod'],
    template: 
`<article>
{{ prod.id }} - {{prod.name}} - {{prod.description}} - {{prod.price}}
<button v-on:click="select">Select</button>
</article>`,
    methods: {
        select: function () {
            this.$emit('selected', this.prod); //triggers a 'selected' event and passes this.prod to the callback function 
        }
    }
});
```

Now we need to handle the event in the parent. Let's change the html first.

```html
<div id="app">
        <div>
            <input type="text" placeholder="name" />
            <input type="text" placeholder="description" />
            <input type="text" placeholder="price" />
        </div> 

        <section>
            <product-item v-for="product in products" v-bind:prod="product" :key="product.id" v-on:selected="productSelected"></product-item>
        </section>
    </div>
```

Finally, let's change the javascript of the parent

```js
const vm = new Vue({
    el: "#app",
    data: {
        products: []
    },
    mounted: function () {
        new DataLayer()
            .getAllProducts()
            .then(products => this.products = products);
    },
    methods: {
        productSelected: function (selectedProduct) {
            console.log(selectedProduct);
        }
    }
});
```

If you refresh the page you should see a message on the console when you click on the button.

So now what can we do to show the values on the textboxes?
We can use the data-binding engine of Vue.
Vue provides the v-model directive that makes two-way binding between form input and app state a breeze.

Let's first introduce a new ```current``` property on the parent. It will represent the currently selected product. We will initialize it with a new product object. 

```js
const vm = new Vue({
    el: "#app",
    data: {
        products: [],
        current: {id:0, name:"", description:"", price:0}
    },
    mounted: function () {
        new DataLayer()
            .getAllProducts()
            .then(products => this.products = products);
    },
    methods: {
        productSelected: function (selectedProduct) {
            console.log(selectedProduct);
        }
    }
});
```

Now let's bind the current property to the textboxes.

```html
<div id="app">
    <div>
        <input v-model="current.name" type="text" placeholder="name" />
        <input v-model="current.description" type="text" placeholder="description" />
        <input v-model:number="current.price" type="text" placeholder="price" />
    </div> 

    <section>
        <product-item v-for="product in products" v-bind:prod="product" :key="product.id" v-on:selected="productSelected"></product-item>
    </section>
</div>
```

If you refresh the page you should see the price texbox having a value of 0, while the name and description textboxes are empty.

The next step is simply to switch the current item with the selected item. To do that we can modify the productSelected method of the parent.

```js
const vm = new Vue({
    el: "#app",
    data: {
        products: [],
        current: {id:0, name:"", description:"", price:0}
    },
    mounted: function () {
        new DataLayer()
            .getAllProducts()
            .then(products => this.products = products);
    },
    methods: {
        productSelected: function (selectedProduct) {
            this.current = selectedProduct;
        }
    }
});
``` 

If you refresh the page and click on a Select button you will see the textbox changing values accordingly.
Note that in the method we simply update the state of our app without touching the DOM - all DOM manipulations are handled by Vue, and the code you write is focused on the underlying logic.
As we already said, the data and the DOM are now linked, and everything is now reactive. How do we know? Just select a product and set the fields to a different value by typing on the textboxes. You should see the product item in the list update accordingly.

Now we want to propagate the edited values to the server. We need a new "Save" button on the parent.

```html
<div id="app">
    <div>
        <input v-model="current.name" type="text" placeholder="name" />
        <input v-model="current.description" type="text" placeholder="description" />
        <input v-model:number="current.price" type="text" placeholder="price" />
        <button v-on:click="save">Save</button>
    </div> 

    <section>
        <product-item v-for="product in products" v-bind:prod="product" :key="product.id" v-on:selected="productSelected"></product-item>
    </section>
</div>
``` 

We also need to handle the click event with a save method where we invoke our DataLayer.

```js
const vm = new Vue({
    el: "#app",
    data: {
        products: [],
        current: {id:0, name:"", description:"", price:0}
    },
    mounted: function () {
        new DataLayer()
            .getAllProducts()
            .then(products => this.products = products);
    },
    methods: {
        productSelected: function (selectedProduct) {
            this.current = selectedProduct;
        },
        save: function () {
            new DataLayer().updateProduct(this.current.id, this.current);
        }
    }
});
```

Refresh the page, select a product, edit the values and click on the Save button. If you inspect the network you will see the request going through and you should get a 204 NoContent as a response. You may want to put a breakpoint on the service to see the product sent by the client being updated on the server.

The add requires yet another button and an event-handling method.

Let's add the button on the parent html.

```html
<div id="app">
    <button v-on:click="add">New Product</button>
    <div>
        <input v-model="current.name" type="text" placeholder="name" />
        <input v-model="current.description" type="text" placeholder="description" />
        <input v-model:number="current.price" type="text" placeholder="price" />
        <button v-on:click="save">Save</button>
    </div> 

    <section>
        <product-item v-for="product in products" v-bind:prod="product" :key="product.id" v-on:selected="productSelected"></product-item>
    </section>
</div>
```

Now let's handle the add by adding a method where modify the current selected product by referencing it to a new product object.

```js
const vm = new Vue({
    el: "#app",
    data: {
        products: [],
        current: {id:0, name:"", description:"", price:0}
    },
    mounted: function () {
        new DataLayer()
            .getAllProducts()
            .then(products => this.products = products);
    },
    methods: {
        productSelected: function (selectedProduct) {
            this.current = selectedProduct; // this.products.find(p => p.id == selectedProduct.id) ;
        },
        save: function () {
            new DataLayer().updateProduct(this.current.id, this.current);
        },
        add: function () {
            this.current = { id: 0, brand: "", name: "", price: 0 }
        }
    }
});
``` 

As a last step, let's change the save method so that the createProduct of our DataLayer is invoked if the current.id is 0. Also, let's push the product returned by the createProduct into our products array, so that the list of products is automatically refreshed by Vue.

```js
const vm = new Vue({
    el: "#app",
    data: {
        products: [],
        current: {id:0, name:"", description:"", price:0}
    },
    mounted: function () {
        new DataLayer()
            .getAllProducts()
            .then(products => this.products = products);
    },
    methods: {
        productSelected: function (selectedProduct) {
            this.current = selectedProduct; // this.products.find(p => p.id == selectedProduct.id) ;
        },
        save: function () {
            if (this.current.id == 0) {
                new DataLayer().insertProduct(this.current)
                    .then(product => {
                        this.products.push(product);
                    });
            } else {
                new DataLayer().updateProduct(this.current.id, this.current);
            }
        },
        add: function () {
            this.current = { id: 0, brand: "", name: "", price: 0 }
        }
    }
});
```

Refresh the page, click on the Add button, type some values on the textboxes and click on Save. You should see the network request going through and the new Product being added on the server.
You may want to put a breakpoint on the service to see the product being added server side.

We also want to give the user the opportunity to delete a given product. In order to do so we need to 

- Add a button to the child component
- Handle the click on the child component
- Emit a custom event on the child component
- Handle the child custom event on the parent
- Invoke the datalayer to delete the product
- Remove the product from the parent array in order to have Vue refresh the UI

Let's add the button to the child component.

```js
Vue.component('product-item', {
    props: ['prod'],
    template: 
`<article>
{{ prod.id }} - {{prod.name}} - {{prod.description}} - {{prod.price}}
<button v-on:click="select">Select</button>
<button>Delete</button>
</article>`,
    methods: {
        select: function () {
            this.$emit('selected', this.prod);
        }
    }
});
```

Let's handle the click event

```js
Vue.component('product-item', {
        props: ['prod'],
        template: 
`<article>
    {{ prod.id }} - {{prod.name}} - {{prod.description}} - {{prod.price}}
    <button v-on:click="select">Select</button>
    <button v-on:click="remove">Delete</button>
</article>`,
        methods: {
            select: function () {
                this.$emit('selected', this.prod);
            }
        }
    });
```

Let's emit a custom event

```js
Vue.component('product-item', {
    props: ['prod'],
    template: 
`<article>
{{ prod.id }} - {{prod.name}} - {{prod.description}} - {{prod.price}}
<button v-on:click="select">Select</button>
<button v-on:click="remove">Delete</button>
</article>`,
    methods: {
        select: function () {
            this.$emit('selected', this.prod);
        },
        remove: function () {
            this.$emit('deleting', this.prod);
        }
    }
});
```

Let's handle the child custom event on the parent. 
First, let's change the html to handle the deleting custom event with a productSelected method.

```html
<div id="app">
    <button v-on:click="add">New Product</button>
    <div>
        <input v-model="current.name" type="text" placeholder="name" />
        <input v-model="current.description" type="text" placeholder="description" />
        <input v-model:number="current.price" type="text" placeholder="price" />
        <button v-on:click="save">Save</button>
    </div> 

    <section>
        <product-item v-for="product in products" v-bind:prod="product" :key="product.id" v-on:selected="productSelected" v-on:deleting="productDeleting"></product-item>
    </section>
</div>
```

Now let's write the method that invokes the datalayer and removes the product from the products array.

```js
const vm = new Vue({
    el: "#app",
    data: {
        products: [],
        current: {id:0, name:"", description:"", price:0}
    },
    mounted: function () {
        new DataLayer()
            .getAllProducts()
            .then(products => this.products = products);
    },
    methods: {
        productSelected: function (selectedProduct) {
            this.current = selectedProduct; // this.products.find(p => p.id == selectedProduct.id) ;
        },
        save: function () {
            if (this.current.id == 0) {
                new DataLayer().insertProduct(this.current)
                    .then(product => {
                        this.products.push(product);
                    });
            } else {
                new DataLayer().updateProduct(this.current.id, this.current);
            }
        },
        add: function () {
            this.current = { id: 0, brand: "", name: "", price: 0 }
        },
        productDeleting: function (product) {
            new DataLayer().deleteProduct(product.id)
                .then(() => this.products.splice(this.products.indexOf(product), 1));
        }
    }
});
```

If you refresh the page and click on a Delete button you should see the network traffic going to the server and the product being removed from the user interface.

As an extra excercise, try to refactor our View by creating a Vue Component for the form. See if you can do it by yourself, then continue reading this walkthrough.

The javascript of the component can become:

```js
Vue.component('product-form', {
    props: ['product'],
    template:
`<div>
    <input v-model="product.name" type="text" placeholder="name" />
    <input v-model="product.description" type="text" placeholder="description" />
    <input v-model:number="product.price" type="text" placeholder="price" />
    <button v-on:click="save">Save</button>
</div> 
`,
    methods: {
        save: function () {
            this.$emit('saving', this.product);
        }
    }
});
```

The index.html can become

```html
<div id="app">
    <button v-on:click="add">New Product</button>
    
    <product-form v-bind:product="current" v-on:saving="productSaving"></product-form>

    <section>
        <product-item v-for="product in products" v-bind:prod="product" :key="product.id" v-on:selected="productSelected" v-on:deleting="productDeleting"></product-item>
    </section>
</div>
```

And the javascript of the main Vue instance can become

```js
const vm = new Vue({
    el: "#app",
    data: {
        products: [],
        current: {id:0, name:"", description:"", price:0}
    },
    mounted: function () {
        new DataLayer()
            .getAllProducts()
            .then(products => this.products = products);
    },
    methods: {
        productSelected: function (selectedProduct) {
            this.current = selectedProduct; // this.products.find(p => p.id == selectedProduct.id) ;
        },
        productSaving: function (productToSave) {
            if (productToSave.id == 0) {
                new DataLayer().insertProduct(productToSave)
                    .then(product => {
                        this.products.push(product);
                    });
            } else {
                new DataLayer().updateProduct(productToSave.id, productToSave);
            }
        },
        add: function () {
            this.current = { id: 0, brand: "", name: "", price: 0 }
        },
        productDeleting: function (product) {
            new DataLayer().deleteProduct(product.id)
                .then(() => this.products.splice(this.products.indexOf(product), 1));
        }
    }
});
```

If you refresh the page you should see no difference in behaviour nor interface, but our code is cleaner now.

Let's now add a Cancel button in the product-form Component.

```js
Vue.component('product-form', {
    props: ['product'],
    template:
`<div>
    <input v-model="product.name" type="text" placeholder="name" />
    <input v-model="product.description" type="text" placeholder="description" />
    <input v-model:number="product.price" type="text" placeholder="price" />
    <button v-on:click="save">Save</button>
    <button v-on:click="cancel">Cancel</button>
</div> 
`,
    methods: {
        save: function () {
            this.$emit('saving', this.product);
        },
        cancel: function () {
            this.$emit('cancel', this.product);
        }
    }
});
```

Let's handle the cancel event of the child with a cancel method on the parent.

```html
<div id="app">
    <button v-on:click="add">New Product</button>
    
    <product-form v-bind:product="current" v-on:saving="productSaving" v-on:cancel="cancel"></product-form>

    <section>
        <product-item v-for="product in products" v-bind:prod="product" :key="product.id" v-on:selected="productSelected" v-on:deleting="productDeleting"></product-item>
    </section>
</div>
```

In the method, let's reset the properties of the current item.

```js
const vm = new Vue({
    el: "#app",
    data: {
        products: [],
        current: {id:0, name:"", description:"", price:0}
    },
    mounted: function () {
        new DataLayer()
            .getAllProducts()
            .then(products => this.products = products);
    },
    methods: {
        productSelected: function (selectedProduct) {
            this.current = selectedProduct; // this.products.find(p => p.id == selectedProduct.id) ;
        },
        productSaving: function (productToSave) {
            if (productToSave.id == 0) {
                new DataLayer().insertProduct(productToSave)
                    .then(product => {
                        this.products.push(product);
                    });
            } else {
                new DataLayer().updateProduct(productToSave.id, productToSave);
            }
        },
        add: function () {
            this.current = { id: 0, brand: "", name: "", price: 0 };
        },
        productDeleting: function (product) {
            new DataLayer().deleteProduct(product.id)
                .then(() => this.products.splice(this.products.indexOf(product), 1));
        },
        cancel: function (product) {
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
        }
    }
});
```  

If you refresh a page, select a product, modify the values on the textboxes and click on the cancel button, you should see the properties restored to their original values coming from the server.  
If you click on the add button, modify the values on the textboxes and click on the cancel button, you should see the properties restored to their original default values.  

## If time permits

We may want to 

- show the form only if necessary
- disable the select and delete buttons while the user is using the form

In order to do this, we first have to add a boolean property ```isFormInUse```. We will set it to true when a user clicks on Add or Select, and to false when the user clicks on Save or Cancel. We will then show/hide the form and enable/disable the buttons according to the value of this property.

Let's add the isFormInUse boolean property to our parent. Let's set it to false during initialization and at the end of the cancel and productSaving methods; let's set it to true at the end of the add and productSelected method. 

```js
const vm = new Vue({
    el: "#app",
    data: {
        products: [],
        current: { id: 0, name: "", description: "", price: 0 },
        isFormInUse : false
    },
    mounted: function () {
        new DataLayer()
            .getAllProducts()
            .then(products => this.products = products);
    },
    methods: {
        productSelected: function (selectedProduct) {
            this.current = selectedProduct; 
            this.isFormInUse = true;
        },
        productSaving: function (productToSave) {
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
        add: function () {
            this.current = { id: 0, brand: "", name: "", price: 0 };
            this.isFormInUse = true;
        },
        productDeleting: function (product) {
            new DataLayer().deleteProduct(product.id)
                .then(() => this.products.splice(this.products.indexOf(product), 1));
        },
        cancel: function (product) {
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
    }
});
```

In the product-item Vue Component, let's add a buttonsDisabled property and let's bind the disabled property of the two buttons to the value of the buttonsDisabled property.

```js
Vue.component('product-item', {
    props: ['prod', 'buttonsDisabled'],
    template: 
`<article>
{{ prod.id }} - {{prod.name}} - {{prod.description}} - {{prod.price}}
<button v-on:click="select" v-bind:disabled="buttonsDisabled">Select</button>
<button v-on:click="remove" v-bind:disabled="buttonsDisabled">Delete</button>
</article>`,
    methods: {
        select: function () {
            this.$emit('selected', this.prod);
        },
        remove: function () {
            this.$emit('deleting', this.prod);
        }
    }
});
```

Now for the final step, let's change the html of the parent. 

- The disabled property of the add button has to be bound to the isFormInUse property
- The buttonsDisabled property of the product-item has to be bound to the isFormInUse property
- The form has to be rendered only if isFormInUse is true. We are going to use the v-if directive

```html
<div id="app">
    <button v-on:click="add" v-bind:disabled="isFormInUse">New Product</button>
    
    <product-form v-if="isFormInUse" v-bind:product="current" v-on:saving="productSaving" v-on:cancel="cancel"></product-form>

    <section>
        <product-item v-for="product in products" v-bind:prod="product"  v-bind:buttons-disabled="isFormInUse" :key="product.id" v-on:selected="productSelected" v-on:deleting="productDeleting"></product-item>
    </section>
</div>
``` 

Refresh the page and check that the form gets rendered only when a product is selected or when the New Product button is pressed.

As you can see, the user interface is functioning but it's not really nice looking. In a following lab we're going to change that by using Bootstrap 4.

# Next steps

```
git add .
git commit -m "student: step 3 complete"
git checkout step04start 
```