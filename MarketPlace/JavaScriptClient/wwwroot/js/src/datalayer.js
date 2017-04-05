import applicationUserManager from "./ApplicationUserManager"
import Swagger from "swagger-client"

export default class {
    constructor() {
        this.url = `http://localhost:5000/swagger/v1/swagger.json`;
    }
    getAllProducts() {
        return new Swagger({
            url: this.url,
            usePromise: true
        }).then(client => client.Products.getProducts())
          .then(data => data.obj);
    }

    getProductById(id) {
        return new Swagger({
            url: this.url,
            usePromise: true
        }).then(client => client.Products.getProduct({ id }))
        .then(data => data.obj);
    }

    async insertProduct(product) {
        const user = await applicationUserManager.getUser();
        const client = await new Swagger({
            url: this.url,
            usePromise: true
        });

        const data = await client.Products.createProduct({ product }, {
            clientAuthorizations: {
                api_key: new Swagger.ApiKeyAuthorization('Authorization', 'Bearer ' + user.access_token, 'header')
            }
        });
        return data.obj;

        //return new Swagger({
        //    url: this.url,
        //    usePromise: true
        //}).then(client => client.Products.createProduct({ product }))
        //.then(data => data.obj);
    }

    updateProduct(id, product) {
        return new Swagger({
            url: this.url,
            usePromise: true
        }).then(client => client.Products.updateProduct({ id, product }))
        .then(data => data.obj);
    }

    deleteProduct(id) {
        return new Swagger({
            url: this.url,
            usePromise: true
        }).then(client => client.Products.deleteProduct({ id }))
        .then(data => data.obj);
    }
}
