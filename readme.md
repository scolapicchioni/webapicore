# MarketPlace Project

We're going to build a simple web application where people can manage products they want to sell.
- Everyone can browse existing products.
- Only authenticated users can add new products.
- Only a product owner can edit or delete a product.

We are going to build 3 projects. All three are .NET Core 1.1 Web Applications built with Visual Studio 2017.

1. Service 
   - .NET Core Web API Controller
   - Swashbuckle
   - Entity Framework Core
   - Sql Server Database
   - Identity Server Client Authentication

This project will be responsible to store the data on the server and respond to the client requests through http and json.

2. Client
   - Javascript (ECMAScript 2017)
   - HTML 5
   - CSS 3
   - Swagger Client 2
   - Vue.js 2
   - Bootstrap 4
   - Open Id Connect Client

This project will interact with the user through a browser by dinamically constructing an HTML user interface and will talk to the server by using javascript and json.

3. Authentication Server
   - Identity Server 4
   - Entity Framework Core

This project will take care of the authentication part. It will issue tokens that will be used by the client application to gain access to the server.

## What you already need to know:
- C#
- Javascript (ECMAScript 2017)
- HTML 5
- CSS 3
- GIT

## What you're going to learn:
- What is REST
- What is .NET Core
- ASP.NET Core 
- What is a Web API Controller
- Kestrel
- Middleware
- Environment variables
- ASP.NET Core Configuration
- Dependency Injection
- Entity Framework Core
- Swagger
- Swashbuckle
- PostMan
- CORS
- Introduction to Vue.js
- Introduction to Bootstrap 4
- Authentication and Authorization
- OAuth 2 and Open Id Connect
- Identity Server 4
- Resource Owner Authorization

## Before you begin, you need
- [Visual Studio 2017 (Community Edition is enough)](https://www.visualstudio.com/thank-you-downloading-visual-studio/?sku=community) 

**Make sure you installed the workload ".NET Core cross-platform development". You can check and install workloads by launching the Visual Studio Installer.**

## For more information on the .NET Core installation

Please see [https://www.microsoft.com/net/core#windowsvs2017](https://www.microsoft.com/net/core#windowsvs2017)

[GIT](https://git-scm.com/downloads)

---

# Our workflow

We are going to split our projects into simple steps. Each step will focus on one task and will build on top of the previous step. We will start with simple projects that will become more and more complex along the way. For example, we will not focus on authentication and authorization at first. We will add it at a later step.

This repo contains different branches. Each branch represents a phase in our project. "Start" branches are the starting points of each step. "Solution" branches are the final versions of each step, given to you just in case you want to check what your project is supposed to become at the end of each lab.
What you have to do is to checkout a start branch corresponding to the lab you want to try (for example Step01Start in order to begin) and follow the instructions you find on the theory.md and practice.md files under the docs folder. When you are done, feel free to commit your work and checkout the next start branch to continue with the next lab.     

# To START

1. Clone this repo 
2. Checkout the Step01Start branch
3. Open theory.md and practice.md in the docs folder to continue

Please refer to the [theory](/docs/theory.md) and [practice](/docs/practice.md) documents to start your lab. 

