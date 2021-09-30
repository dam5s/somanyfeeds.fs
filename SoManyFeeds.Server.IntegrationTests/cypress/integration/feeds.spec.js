/// <reference types="Cypress" />

const tabs = {
    clickRead: () => cy.get("a").contains("Read").click(),
    clickManage: () => cy.get("a").contains("Manage").click(),
}

const signInScreen = {
    checkDisplayed: () => cy.get("h1").should("have.text", "Authentication required"),
    clickSignUp: () => cy.get("a").contains("Sign up now").click(),
    submitFormWith: (email, password) => {
        cy.get("input[name='email']").type(email);
        cy.get("input[name='password']").type(password);
        cy.get("button").contains("Sign in").click();
    },
}

const signUpScreen = {
    checkDisplayed: () => cy.get("h1").should("have.text", "Registration"),
    submitFormWith: (name, email, password, confirmation) => {
        cy.get("input[name='name']").type(name);
        cy.get("input[name='email']").type(email);
        cy.get("input[name='password']").type(password);
        cy.get("input[name='passwordConfirmation']").type(confirmation);
        cy.get("button").contains("Sign up").click();
    },
}

const welcomeScreen = {
    checkDisplayed: () => cy.get("h1").should("have.text", "Welcome"),
}

const readScreen = {
    checkDisplayed: () => cy.get("h2").should("have.text", "Articles")
}

const manageScreen = {
    checkDisplayed: () => {
        cy.get("h2").should("have.text", "Feeds");
        cy.get("h3").should("contain.text", "Add feed");
    },
    checkHasNoFeeds: () => {
        cy.get("p").should("have.text", "You have not subscribed to any feeds yet.");
        cy.get(".card-list .card").should("have.length", 0);
    },
    checkHasFeedCount: (count) => {
        cy.get("h3").should("contain.text", "Your feeds");
        cy.get(".card-list .card").should("have.length", count);
    },
    checkHasFeed: (title, url) => {
        cy.get(".card dt").should("contain", title);
        cy.get(".card dd a").should("contain", url);
    },
    searchFor: (text) => {
        cy.get("input[name='searchText']").type(text);
        cy.get("button").contains("Search").click();
    },
    checkHasResultCount: (length) => {
        cy.get("h3").should("contain", "Search results");
        cy.get(".card-list .card").should("have.length", length);
    },
    checkHasResult: (title, url) => {
        cy.get(".card dd").should("contain", title);
        cy.get(".card dd a").should("contain", url);
    },
    subscribe: () => cy.get(".card .actions button").contains("Subscribe").click(),
    unsubscribe: () => {
        cy.get("button").contains("Unsubscribe").click();
        cy.get("h3").should("contain.text", "Unsubscribe");
        cy.get("button").contains("Yes, unsubscribe").click();
    },
    goBack: () => {
        cy.get(".back-link").contains("Back").click();
    }
}

describe("Feeds", () => {

    const serverUrl = "http://localhost:9090";

    it("supports CRUD flow", () => {
        cy.visit(serverUrl);

        welcomeScreen.checkDisplayed();
        tabs.clickRead();

        signInScreen.checkDisplayed();
        signInScreen.clickSignUp();

        signUpScreen.checkDisplayed();
        signUpScreen.submitFormWith("Damo", "damo@example.com", "supersecret", "supersecret");

        signInScreen.checkDisplayed();
        signInScreen.submitFormWith("damo@example.com", "supersecret");

        readScreen.checkDisplayed();

        tabs.clickManage();

        manageScreen.checkDisplayed()
        manageScreen.checkHasNoFeeds()
        manageScreen.searchFor("http://localhost:9092/index.html")

        manageScreen.checkHasResultCount(1);
        manageScreen.checkHasResult("Stories by Damien Le Berrigaud on Medium", "http://localhost:9092/rss.xml");
        manageScreen.subscribe();

        manageScreen.goBack();

        manageScreen.checkHasFeedCount(1);
        manageScreen.checkHasFeed("Stories by Damien Le Berrigaud on Medium", "http://localhost:9092/rss.xml");
        manageScreen.unsubscribe();

        manageScreen.checkHasNoFeeds();
    });
});
