class DataLayer {
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

const dl = new DataLayer();

dl.getAllProducts()
    .then(console.log)
    .then(() => dl.getProductById(1))
    .then(console.log)
    .then(() => dl.insertProduct({ id: 0, name: "New Product", description: "Added through javascript", price: 9876 }))
    .then(console.log)
    .then(() => dl.updateProduct(1, { id: 1, name: "Modified Product", description: "Modified through javascript", price: 9876 }))
    .then(() => dl.getProductById(1))
    .then(console.log)
    .then(() => dl.deleteProduct(1))
    .then(console.log);