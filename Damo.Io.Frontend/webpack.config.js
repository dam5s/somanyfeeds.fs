var path = require("path");

module.exports = {
    mode: "production",
    entry: "./build/src/App.js",
    output: {
        path: path.join(__dirname, "..", "Damo.Io.Server", "WebRoot"),
        filename: "damo-io.js",
    },
};
