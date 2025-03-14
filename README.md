# SmartCertify Clean Architecture .NET 9 API

Welcome to the **SmartCertify Clean Architecture .NET 9 API** project! This repository showcases how to build a robust API with a clean architecture using .NET 9, following best practices to design scalable and maintainable applications. 

The project is structured with a focus on **Clean Architecture**, utilizing the **Entity Framework Core (EF Core)** for database access and **Swagger, Scalar,and NSwag** for API documentation.

---

## Episode Overview

### **How to Build an Online Course Certification Platform | Architecture & App Overview**
Welcome to Episode 1 of the SmartCertify series! 🚀 In this video, we’ll kick off by exploring the architecture and overview of the SmartCertify App, a full-stack web application designed to revolutionize online course certification.

Discover the clean architecture design, learn how the app is structured, and see how we’ll use cutting-edge technologies like Angular 19, .NET Core 9 Web API, and Azure. Whether you’re a beginner or an experienced developer, this series will guide you step-by-step to build a powerful, scalable platform for the future of online education.
  
**Key Topics:**
- Project architecture and structure
- App overview and high-level design
- Exploring heavy Azure Services that we will use in this Application

---

### **Install SQL Express, SSMS, Azure Data Studio & Connect to SQL Server | Step-by-Step Guide**
In this video, learn how to install SQL Express, SQL Server Management Studio (SSMS), and Azure Data Studio. We’ll guide you through connecting to a SQLExpress server and accessing your database. Perfect for beginners starting with SQL databases! Stay tuned for more episodes in our SmartCertify series, where we build a robust online course certification platform.
  
**Key Topics:**
- Installation of SQL Tools 
- Azure Data Studio Installation
- SQL Express Installation, SQL Server Management Studio (SSMS) Installation

- ### **Install SQL Server and Tools**

1. **SQL Server Management Studio (SSMS)**  
   Download and install SQL Server Management Studio (SSMS) to manage your SQL Server databases.
   - [SSMS Download (Official)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16)
   - [Direct SSMS Full Setup](https://aka.ms/ssmsfullsetup)
   - [SQL Server Downloads](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

2. **SQL Server Express Edition**  
   SQL Server 2022 Express is a free edition suitable for development and small applications.
   - [SQL Server Express 2022 Download](https://go.microsoft.com/fwlink/p/?linkid=2216019&clcid=0x409&culture=en-us&country=us)

3. **Azure Data Studio**  
   An alternative to SSMS for working with SQL databases, designed for data professionals who prefer a cross-platform tool.
   - [Azure Data Studio](https://azure.microsoft.com/en-us/products/data-studio)
   - [Azure Data Studio Download Link](https://go.microsoft.com/fwlink/?linkid=2216158&clcid=0x409)
   
---
---

### **SQL Basics for Beginners: Step-by-Step Guide with Examples**
In this episode, we dive into the basics of SQL, including database creation, table design, constraints, and SQL commands like SELECT, INSERT, UPDATE, DELETE, etc. This is a must-watch for beginners who want to understand how databases work.

**Key Topics:**
- SQL commands (SELECT, INSERT, UPDATE, DELETE)
- Database design and normalization
- Primary/Foreign Keys and Constraints

---

### **Beginner's Guide to Database Design: Tables, Keys, and Constraints**
Unlock the foundations of database design in this beginner-friendly guide. In this video, we explore key concepts like tables, primary keys, foreign keys, and constraints while walking through a real-world schema for a course certification app. Learn about designing efficient databases, creating relationships between tables, and using constraints to maintain data integrity. This practical, step-by-step guide is perfect for developers, students, or anyone interested in SQL and database management.

**Key Topics:**
- Understanding database design
- Creating tables with constraints
- Primary/Foreign Keys and Indexes
- Setting up databases
- Creating and managing tables
- Understanding primary keys and foreign keys
- Adding constraints for data validation
- SQL basics: SELECT, INSERT, UPDATE, DELETE

---

### **Build .NET Core 9 Web API with Clean Architecture | Project Setup**
In this video, we’ll walk through the process of creating a .NET Core 9 Web API project using Clean Architecture principles. We'll set up the project structure, configure Entity Framework (EF) Core, and generate entities from the database we built in the previous episode. Additionally, we’ll show how to configure Swagger alternatives in .NET Core 9, as Swagger's default scaffold is removed. Perfect for anyone looking to structure their Web API efficiently and learn about the latest .NET Core 9 changes.

**Key Topics:**
- .NET 9 Web API project setup using Clean Architecture Princeples
- Scaffold database entities with EF Core
- Swagger setup and configuration. NSwag and Scalar for API documentation.

---

### **Git & Azure DevOps for absolute beginners!**
Are you an absolute beginner in Git or Azure DevOps? This video is your ultimate guide! We’ll walk you through the basics of Git, including essential commands, and show you how to set up an Azure DevOps account from scratch. Learn how to create repositories, push your local code to the Azure DevOps repo, and manage your projects seamlessly. Whether you're new to version control or looking to understand Azure DevOps, this tutorial has everything you need to get started.

**Key Topics:**
- Git basics and setup
- Azure DevOps account and repository creation
- Pushing code to Git with Git commands
- Setting up your Azure DevOps account.
- Overview of Azure DevOps and its features.
- Step-by-step repo creation in Azure DevOps.
- Using Git commands to push code from local to remote repositories.
- Pro tips for managing your code with Git and Azure DevOps.

- ### **Git Commands to Initialize and Push Code to Repository**
To get started with version control, follow these Git commands to set up a new Git repository and push your code to Azure DevOps.

```
# Initialize a new Git repository in your local project
git init

# Link your local repository to the remote repository on Azure DevOps
git remote add origin <your_remote_api_git_url>

# Fetch the latest information from the remote repository
git fetch origin

# Create a new branch named 'main' and switch to it
git checkout -b main

# Stage all changes in your local project for the next commit
git add .

# Commit the staged changes with a message
git commit -m "Initial commit"

# Push the 'main' branch to the remote repository
git push -u origin main
```

## Git Commands Explanation
    git init
    Initializes a new Git repository in your local project directory.

    git remote add origin <URL>
    Links your local Git repository to a remote repository. In this case, it connects to the Azure DevOps repository.

    git fetch origin
    Fetches updates from the remote repository without merging them into your local branch.

    git checkout -b main
    Creates a new branch named main and switches to it. This step aligns your local branch naming with the remote repository's naming convention.

    git add .
    Stages all the changes you've made in your local repository for the next commit.

    git commit -m "Initial commit"
    Commits the staged changes to the local repository with a descriptive message.

    git push -u origin main
    Pushes the main branch to the remote repository and sets it as the default upstream branch for future pushes/pulls.

---
- ### **Save Money with Azure Policy: Restrict SQL Database to Basic Tier | Full Step-by-Step Guide**
    In this step-by-step Azure tutorial, learn how to create and assign a custom Azure Policy to restrict SQL Database creation to the cost-effective Basic tier. Watch as we:

    Create a new Azure Subscription
    Define a custom Azure Policy with restrictions for SQL Databases
    Assign the policy to the new subscription
    Create an SQL Server and Database under the subscription
    Test the policy enforcement by attempting to select a pricing tier other than Basic
    This tutorial is perfect for Azure beginners, cloud administrators, and developers looking to manage resources effectively and control costs.

    📌 What You’ll Learn:

    - What is Azure Policy?
    - How to define and assign a custom policy
    - Enforcing SQL Database SKU restrictions in Azure
    - Testing policy functionality in real time
 
  **Sample Policy Shown in the Video**
```
{
  "properties": {
    "displayName": "Restrict SQL Database SKU to Basic",
    "policyType": "Custom",
    "mode": "All",
    "description": "This policy restricts the creation of Azure SQL Databases to the Basic SKU only.",
    "metadata": {
      "category": "SQL",
      "createdBy": "your userid guid here",
      "createdOn": "2025-01-10T19:16:33.2197985Z",
      "updatedBy": null,
      "updatedOn": null
    },
    "version": "1.0.0",
    "parameters": {},
    "policyRule": {
      "if": {
        "allOf": [
          {
            "field": "type",
            "equals": "Microsoft.Sql/servers/databases"
          },
          {
            "not": {
              "field": "Microsoft.Sql/servers/databases/sku.name",
              "equals": "Basic"
            }
          }
        ]
      },
      "then": {
        "effect": "deny"
      }
    },
    "versions": [
      "1.0.0"
    ]
  },
  "id": "/subscriptions/your-subscription-id/providers/Microsoft.Authorization/policyDefinitions/yourid",
  "type": "Microsoft.Authorization/policyDefinitions",
  "name": "ee15c7b5-dc76-43f2-aa9e-876f286a460c",
  "systemData": {
    "createdBy": "learnsmartcoding@gmail.com",
    "createdByType": "User",
    "createdAt": "2025-01-10T19:16:33.2054339Z",
    "lastModifiedBy": "learnsmartcoding@gmail.com",
    "lastModifiedByType": "User",
    "lastModifiedAt": "2025-01-10T19:16:33.2054339Z"
  }
}
```



- ### **Mastering Secure Azure SQL Database Connections Effortlessly**
    Learn how to connect your Azure SQL Database to Azure Data Studio in this step-by-step tutorial! We’ll guide you through configuring the Azure SQL server firewall to allow your local system's IP address, ensuring secure and seamless access. Perfect for developers and cloud enthusiasts looking to manage Azure SQL databases efficiently.

    📌 Topics Covered:

    - Configuring the Azure SQL server firewall
    - Finding your local IP address for firewall rules
    - Connecting to Azure SQL Database using Azure Data Studio
    - Troubleshooting common connectivity issues


- ### **Mastering Angular 19 Standalone Components Quickly | Angular 19 first Episode**
    Kickstart your Angular journey with this comprehensive beginner's guide! Learn how to set up your local environment by installing Node.js and Angular, create your first Angular project, and understand the basics of Angular components. This video simplifies key concepts and walks you through everything you need to know to start building Angular applications. Perfect for developers at any level!
 
    - [Angular 19 Repo]( https://github.com/learnsmartcoding/smartcertify-ui-angular19)
    - [YT Video](https://youtu.be/SwJ4RMQnJOo)

- ### **Master Angular Data Binding and Directives | Complete Beginner's Guide**
    Unlock the power of Angular with this comprehensive guide to data binding and directives. Learn how to efficiently bind data between your components and templates using interpolation, property binding, event binding, and two-way binding. Dive into Angular's powerful built-in directives like *ngIf, *ngFor, and explore the latest @if() syntax for cleaner and modern conditional rendering.  Whether you're new to Angular or looking to sharpen your skills, this tutorial is perfect for beginners. Start building interactive and dynamic web apps today!
 
    - [Angular 19 Repo]( https://github.com/learnsmartcoding/smartcertify-ui-angular19)
    - [YT Video](https://youtu.be/gnGPcTCn81w)

- ### **Supercharge Your Angular 19 App with Lazy Loading Techniques & Routing**
    Learn how to implement efficient and modern Angular Routing with Lazy Loading in Angular 19, designed for standalone components. In this video, we’ll cover how to set up dynamic routes, lazy load your components to optimize performance, and handle 404 pages with wildcard routes. Whether you're building small apps or large-scale enterprise solutions, understanding routing and lazy loading is essential for creating seamless, fast-loading applications. Perfect for beginners and intermediate Angular developers!
 
    - [Angular 19 Repo]( https://github.com/learnsmartcoding/smartcertify-ui-angular19)
    - [YT Video](https://youtu.be/pu44fnIompg)

- ### **Build a Full Course App using Angular 19 - Interactive Demo!**
    🚀 Explore the Complete User Journey in Our Course App!

    In this demo, we walk you through how to create an interactive, real-world course app where users can:
    ✅ Browse and select available courses
    ✅ Start, pause, and resume tests
    ✅ Complete tests and receive certifications
    ✅ Access video references for deeper understanding

    This app is built using the latest Angular 18 for the frontend and .NET Core 8 for the backend, integrating powerful features like Azure AD B2C for secure authentication and dynamic UI/UX design.

    Whether you're looking to build a similar app or test your skills, this video is your perfect guide. Watch now and get ready to create an engaging and functional application for online learning!
 
    - [Angular 19 Repo]( https://github.com/learnsmartcoding/smartcertify-ui-angular19)
    - [YT Video](https://youtu.be/M_lCcGwrYlo)

- ### **Mastering Angular Signals in 2025 for EASY State Management**
    In this video, you'll learn all about Angular Signals, a new and powerful way to manage state and keep your UI in sync with data changes in Angular applications. Discover how Signals simplify the process of state management by automatically updating the UI when their value changes, making it easier and more efficient than ever to create reactive applications.

    We also explore Computed Signals and Effects, which enable you to work with dynamic and real-time data efficiently. Plus, see a practical example with our Contacts Component to see Signals in action!

 
    - [Angular 19 Repo](https://github.com/learnsmartcoding/smartcertify-ui-angular19)
    - [YT Video](https://youtu.be/Wm2yRolfK8I)

- ### **.NET Core 9 Web API – Build Full CRUD with PATCH | Clean Architecture & Fluent Validation**
    🚀 Mastering .NET Core 9 Web API – Full CRUD with PATCH, Clean Architecture & Fluent Validation! - Episode 12

    In this episode, we dive deep into .NET Core 9 Web API and implement 3 controllers with complete CRUD operations (GET, POST, PUT, DELETE, PATCH). We follow Clean Architecture principles, ensuring a scalable and maintainable API structure.

    🔥 Key Topics Covered:
    ✅ Implementing GET, POST, PUT, DELETE & PATCH endpoints in .NET Core 9
    ✅ Using Fluent Validation for robust input validation
    ✅ Applying Clean Architecture to structure the Web API efficiently
    ✅ Handling partial updates (PATCH) properly
    ✅ Ensuring best practices for API design and maintainability

    📌 Whether you're a beginner or an experienced developer, this tutorial will help you build a well-structured, professional-grade Web API.
 
    - [.Net 9 Web API Repo](https://github.com/learnsmartcoding/smartcertify-api-clean-architecture-dotnet9)
    - [Angular 19 Repo](https://github.com/learnsmartcoding/smartcertify-ui-angular19)
    - [App Demo](https://smartcertify-web.azurewebsites.net/home)
    - [YT Video](https://youtu.be/lAVUy2U9QgY)

- ### **Mastering API Calls In Angular With A Reusable Service**
    In this video, we’ll build an Angular service to fetch data from a .NET Web API using HttpClient. We’ll cover best practices, how to handle API calls efficiently, and demonstrate a real-time example of fetching and displaying data in an Angular app. 🚀

    👉 What You’ll Learn:
    ✅ How to create an Angular service using ng generate service
    ✅ Use HttpClient to call a .NET Web API
    ✅ Implement best practices for API calls in Angular
    ✅ Handle error responses and implement loading states
    ✅ Display fetched data dynamically in an Angular component

    💡 This is a must-watch for developers working with Angular & .NET Core APIs! Don’t forget to like, share, and subscribe for more hands-on tutorials. 🔥

 
    - [Angular 19 Repo](https://github.com/learnsmartcoding/smartcertify-ui-angular19)
    - [YT Video](https://youtu.be/kqHFDvnjbcw)

-  ### **Angular Input() & Output() Explained | Parent-Child Communication with Filters**
    In this video, Learn how to use @Input() and @Output() in Angular to enable smooth parent-child component communication. In this video, we demonstrate real-world filtering functionality using a course browsing example. The child component handles the filter selection, and the parent component updates the course list accordingly. Master Angular component interaction with this practical hands-on guide! 🚀

    🔹 Topics Covered:
    ✅ Understanding @Input() for passing data from parent to child
    ✅ Using @Output() and EventEmitter for child-to-parent communication
    ✅ Implementing a course filtering system using these concepts
    ✅ Best practices for Angular component interaction

 
    - [Angular 19 Repo](https://github.com/learnsmartcoding/smartcertify-ui-angular19)
    - [YT Video](https://youtu.be/ThkG1Pkf_1s)

### Project Setup

#### Requirements
- **.NET 9 SDK**
- **Visual Studio 2022 or higher**
- **SQL Server or SQL Server Express**
- **SQL Server Management Studio (SSMS)**
- **Azure Data Studio**
- **Azure DevOps account (for CI/CD)**

### Installation

1. Clone this repository:
    ```bash
    git clone https://github.com/learnsmartcoding/smartcertify-api-clean-architecture-dotnet9
    ```

2. Navigate to the project folder:
    ```bash
    cd smartcertify-api-clean-architecture-dotnet9
    ```

3. Open the solution in Visual Studio.

4. Restore the NuGet packages:
    ```bash
    dotnet restore
    ```

5. Run the project:
    ```bash
    dotnet run
    ```

6. Open the Swagger UI to test the API at:  
    `http://localhost:5000/swagger`

---

## Contributing

Feel free to fork the repository and create pull requests. Contributions are welcome!

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.