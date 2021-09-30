// Note this only includes basic configuration for development mode.
// For a more comprehensive configuration check:
// https://github.com/fable-compiler/webpack-config-template

var path = require("path");

module.exports = {
    mode: "production",
    entry: "./Damo.Io.Frontend.fsproj",
    output: {
        path: path.join(__dirname, "..", "Damo.Io.Server", "WebRoot"),
        filename: "damo-io.js",
    },
    module: {
        rules: [{
            test: /\.fs(x|proj)?$/,
            use: "fable-loader",
        }]
    }
};
