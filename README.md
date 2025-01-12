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

## Project Setup

### Requirements
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
