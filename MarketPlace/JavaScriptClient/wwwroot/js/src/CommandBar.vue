<template>
    <nav class="navbar navbar-inverse bg-inverse navbar-toggleable-md">
        <button class="navbar-toggler navbar-toggler-right" type="button" data-toggle="collapse" data-target="#commandBar" aria-controls="commandBar" aria-expanded="false" aria-label="Toggle navigation">
            <span class="navbar-toggler-icon"></span>
        </button>
        <a class="navbar-brand" href="#">MarketPlace</a>

        <div class="collapse navbar-collapse" id="commandBar">
            <div class="form-inline my-2 my-lg-0">
                <button v-if="userAuthenticated" class="btn btn-primary" v-on:click="add" v-bind:disabled="isFormInUse">New Product</button>
            </div>
        </div>

        <div class="form-inline my-2 my-lg-0">
            <button v-if="!userAuthenticated" id="login" v-on:click="login" class="btn btn-secondary">Login</button>
            <button v-if="userAuthenticated" v-on:click="logout" class="btn btn-secondary">Hello {{userName}} - Logout</button>
        </div>
    </nav>
</template>

<script>
    import applicationUserManager from "./ApplicationUserManager"
    export default {
        props: ['isFormInUse'],
        data() {
            return {
                userAuthenticated: false,
                userName : ""
            }
        },
        async mounted() {
            let user = await applicationUserManager.getUser();
            this.userAuthenticated = user && user.profile && user.profile.name ? true : false;
            this.userName = user && user.profile && user.profile.name ? user.profile.name : "";
        },
        methods: {
            add() {
                this.$emit('add');
            },
            async login() {
                let user = await applicationUserManager.login();
                this.userAuthenticated = user && user.profile && user.profile.name ? true : false;
                this.userName = user && user.profile && user.profile.name ? user.profile.name : "";
            },
            async logout() {
                await applicationUserManager.logout();
                this.userAuthenticated = false;
                this.userName = "";
            }
        }
    }
</script>