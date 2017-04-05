export default class{
    constructor() {
        this.serviceUrl = "http://localhost:5000/api/products";
    }
    getAllProducts() {
        return fetch(this.serviceUrl).then(function (response) {
            return response.json();
        });
    }
    getProductById(id) {
        return fetch(this.serviceUrl + "/" + id).then(function (response) {
            return response.json();
        });
    }

    insertProduct(product) {
        return fetch(this.serviceUrl, {
            method: 'POST',
            body: JSON.stringify(product),
            headers: new Headers({
                'Content-Type': 'application/json'
            })
        }).then(function (response) {
            return response.json();
        });
    }

    updateProduct(id, product) {
        return fetch(this.serviceUrl + "/" + id, {
            method: 'PUT',
            body: JSON.stringify(product),
            headers: new Headers({
                'Content-Type': 'application/json'
            })
        });
    }

    deleteProduct(id) {
        return fetch(this.serviceUrl + "/" + id, {
            method: 'DELETE'
        });
    }
}
