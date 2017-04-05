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