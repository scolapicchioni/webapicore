<template>
    <div id="app" class="container">
        <command-bar v-on:add="add" v-bind:isFormInUse="isFormInUse"></command-bar>
        
        <product-form v-if="isFormInUse" v-bind:product="current" v-on:saving="productSaving" v-on:cancel="cancel"></product-form>

        <section class="row">
            <product-item v-for="product in products" v-bind:prod="product" v-bind:buttons-disabled="isFormInUse" :key="product.id" v-on:selected="productSelected" v-on:deleting="productDeleting"></product-item>
        </section>
    </div>
</template>

<script>
    import DataLayer from "./datalayer"
    import ProductForm from './ProductForm.vue'
    import ProductItem from './ProductItem.vue'  
    import CommandBar from './CommandBar.vue'  

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
            ProductItem,
            CommandBar
        }
    }
</script>