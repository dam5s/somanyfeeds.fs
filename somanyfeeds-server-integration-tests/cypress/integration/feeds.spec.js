/// <reference types="Cypress" />

describe("Feeds", () => {

    const serverUrl = "http://localhost:9090";

    it("supports CRUD flow", () => {
        cy.visit(serverUrl);

        cy.get("h1").should("have.text", "Welcome");
        cy.get("a").contains("Read").click();

        cy.get("h1").should("have.text", "Authentication required");
        cy.get("a").contains("Sign up now").click();

        cy.get("h1").should("have.text", "Registration");
        cy.get("input[name='name']").type("Damo");
        cy.get("input[name='email']").type("damo@example.com");
        cy.get("input[name='password']").type("supersecret");
        cy.get("input[name='passwordConfirmation']").type("supersecret");
        cy.get("button").contains("Sign up").click();

        cy.get("h1").should("have.text", "Authentication required");
        cy.get("input[name='email']").type("damo@example.com");
        cy.get("input[name='password']").type("supersecret");
        cy.get("button").contains("Sign in").click();

        cy.get("h2").should("have.text", "Articles");

        cy.get("a").contains("Manage").click();
        cy.get("h2").should("have.text", "Feeds");
        cy.get("p").should("have.text", "You have not subscribed to any feeds yet.");
        cy.get(".card-list .card").should("have.length", 0);

        cy.get("input[name='searchText']").type("http://localhost:9092/index.html");
        cy.get("button").contains("Search").click();

        cy.get("h3").should("contain", "Search results");
        cy.get(".card-list .card").should("have.length", 1);
        cy.get(".card dd").should("contain", "Stories by Damien Le Berrigaud on Medium")
        cy.get(".card dd a").should("contain", "http://localhost:9092/rss.xml")
        cy.get(".card .actions button").contains("Subscribe").click();

        cy.visit(serverUrl + "/manage");
        cy.get("h2").should("have.text", "Feeds");
        cy.get("h3").should("contain.text", "Add feed");
        cy.get(".card-list .card").should("have.length", 1);
        cy.get(".card dt").should("contain.text", "Stories by Damien Le Berrigaud on Medium");
        cy.get(".card dd").should("contain.text", "http://localhost:9092/rss.xml");

        cy.get("button").contains("Unsubscribe").click();
        cy.get("h3").should("contain.text", "Unsubscribe");
        cy.get("button").contains("Yes, unsubscribe").click();

        cy.get(".card-list .card").should("have.length", 0);
        cy.get("p").should("have.text", "You have not subscribed to any feeds yet.");
    });
});
