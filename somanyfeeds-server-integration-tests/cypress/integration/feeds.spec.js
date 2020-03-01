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

        cy.get("input[name='name']").type("My Test Feed");
        cy.get("input[name='url']").type("http://example.com/my/test/feed.rss");
        cy.get("button").contains("Subscribe").click();

        cy.get(".card-list .card").should("have.length", 1);
        cy.get(".card dd").should("contain", "My Test Feed");
        cy.get(".card dd").should("contain", "http://example.com/my/test/feed.rss");

        cy.get("input[name='name']").type("My Test Feed #2");
        cy.get("input[name='url']").type("http://example.com/my/test/feed-2.rss");
        cy.get("button").contains("Subscribe").click();

        cy.get(".card-list .card").should("have.length", 2);
        cy.get(".card dd").should("contain", "My Test Feed #2");
        cy.get(".card dd").should("contain", "http://example.com/my/test/feed-2.rss");

        cy.get("button").contains("Unsubscribe").click();
        cy.get("h3").should("contain", "Unsubscribe");

        cy.get("button").contains("Yes, unsubscribe").click();
        cy.get(".card-list .card").should("have.length", 1);
        cy.get(".card dd").should("contain", "My Test Feed");
    });
});
