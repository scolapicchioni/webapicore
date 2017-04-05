<template>
    <div class="col-md-4">
        <article class="card">
            <div class="card-header">
                {{ prod.id }}
            </div>
            <div class="card-block">
                <h4 class="card-title">{{prod.name}}</h4>
                <h6 class="card-subtitle mb-2 text-muted">{{prod.price}}</h6>
                <p class="card-text">{{prod.description}}</p>
                <p class="card-text text-muted">{{prod.userName}}</p>
            </div>
            <div class="card-footer">
                <button v-if="userIsOwner" class="btn btn-secondary" v-on:click="select" v-bind:disabled="buttonsDisabled">Select</button>
                <button v-if="userIsOwner" class="btn btn-danger" v-on:click="remove" v-bind:disabled="buttonsDisabled">Delete</button>
            </div>
        </article>
    </div>
</template>

<script>
    import applicationUserManager from "./ApplicationUserManager"
    export default {
        props: ['prod', 'buttonsDisabled'],
        data() {
            return {
                userIsOwner: false
            }
        },
        async mounted() {
            let user = await applicationUserManager.getUser();
            this.userIsOwner = user != undefined && user.profile.name == this.prod.userName;
        },
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