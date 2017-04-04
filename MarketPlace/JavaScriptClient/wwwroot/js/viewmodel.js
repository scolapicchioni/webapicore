window.onload = () => {
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
};