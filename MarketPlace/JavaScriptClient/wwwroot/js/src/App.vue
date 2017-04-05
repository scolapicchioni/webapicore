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
    import applicationUserManager from "./ApplicationUserManager"

    export default {
        name: 'app',
        data() {
            return {
                products: [],
                current: { id: 0, name: "", description: "", price: 0, userName: "" },
                isFormInUse: false
            }
        },
        async mounted () {
            this.products = await new DataLayer().getAllProducts(); 

            let user = await applicationUserManager.getUser();
            this.current.userName = user && user.profile && user.profile.name ? user.profile.name : "" ;
        },
        methods: {
            productSelected (selectedProduct) {
                this.current = selectedProduct;
                this.isFormInUse = true;
            },
            async productSaving (productToSave) {
                if (productToSave.id == 0) {
                    const product = await new DataLayer().insertProduct(productToSave);
                    this.products.push(product);
                } else {
                    new DataLayer().updateProduct(productToSave.id, productToSave);
                }
                this.isFormInUse = false;
            },
            async add() {
                let user = await applicationUserManager.getUser();
                this.current = { id: 0, brand: "", name: "", price: 0, userName: user && user.profile && user.profile.name ? user.profile.name : "" };
                this.isFormInUse = true;
            },
            async productDeleting (product) {
                await new DataLayer().deleteProduct(product.id);
                this.products.splice(this.products.indexOf(product), 1);
            },
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
        },
        components: {
            ProductForm,
            ProductItem,
            CommandBar
        }
    }
</script>