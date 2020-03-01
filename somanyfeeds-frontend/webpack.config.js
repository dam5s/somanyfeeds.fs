// Note this only includes basic configuration for development mode.
// For a more comprehensive configuration check:
// https://github.com/fable-compiler/webpack-config-template

var path = require("path");

module.exports = {
    mode: "production",
    entry: "./somanyfeeds-frontend.fsproj",
    output: {
        path: path.join(__dirname, "..", "somanyfeeds-server", "WebRoot"),
        filename: "somanyfeeds.js",
    },
    module: {
        rules: [{
            test: /\.fs(x|proj)?$/,
            use: "fable-loader",
        }]
    }
};
