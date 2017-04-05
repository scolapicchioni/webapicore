var path = require('path')
var webpack = require('webpack');

module.exports = {
    entry: ['babel-polyfill', './wwwroot/js/src/main.js'],
    output: {
        path: path.resolve(__dirname, './wwwroot/js/dist'),
        filename: 'bundle.js'
    },
    module: {
        rules: [
            {
                test: /\.vue$/,
                loader: 'vue-loader'
            },
            {
                test: /\.js$/,
                loader: 'babel-loader',
                exclude: /node_modules/
            }
        ]
    },
    resolve: {
        alias: {
            'vue$': 'vue/dist/vue.esm.js'
        }
    },
    devtool: 'source-map'
}