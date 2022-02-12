var path = require("path");

module.exports = {
    mode: "production",
    entry: "./build/src/Library.js",
    output: {
        path: path.join(__dirname, "..", "SoManyFeeds.Server", "WebRoot"),
        filename: "somanyfeeds.js",
    },
};
