<template>
    <div class="card">
        <div class="card-block">
            <div class="form-group row">
                <label for="productName" class="col-sm-2">Name</label>
                <input class="form-control col-sm-10" v-model="product.name" type="text" placeholder="name" id="productName" />
            </div>
            <div class="form-group row">
                <label for="productDescription" class="col-sm-2">Description</label>
                <textarea class="form-control col-sm-10" v-model="product.description" placeholder="description" id="productDescription"></textarea>
            </div>
            <div class="form-group row">
                <label for="productPrice" class="col-sm-2">Price</label>
                <input class="form-control col-sm-10" v-model:number="product.price" type="text" placeholder="price" id="productPrice" />
            </div>
        </div>
        <div class="card-footer">
            <button class="btn btn-primary" v-on:click="save">Save</button>
            <button class="btn btn-secondary" v-on:click="cancel">Cancel</button>
        </div>
    </div> 
</template>

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