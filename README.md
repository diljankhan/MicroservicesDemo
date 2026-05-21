# MicroservicesDemo

The New Multi-Database Design
Instead of one database with three schemas, we will provision three independent databases. Notice that we maintain logical links (like CustomerId and ProductId), but we completely eliminate any cross-database dependencies or assumptions.

1. Customers Microservice Database
This database only cares about user/customer identities.

CREATE DATABASE CustomersMicroserviceDB;
GO
USE CustomersMicroserviceDB;
GO

CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE
);
GO

2. Catalog Microservice Database
This database holds product descriptions and active pricing data.

CREATE DATABASE CatalogMicroserviceDB;
GO
USE CatalogMicroserviceDB;
GO

CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL
);
GO

3. Orders Microservice Database
This is where the shift happens. In an isolated microservice database, this table cannot have foreign keys pointing to the other databases. It stores the CustomerId and ProductId purely as raw integer values (logical keys).
CREATE DATABASE OrdersMicroserviceDB;
GO
USE OrdersMicroserviceDB;
GO

CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    CustomerId INT NOT NULL,  -- Logical reference only!
    ProductId INT NOT NULL   -- Logical reference only!
);
GO


Step 1: Create the Microservices Solution Structure
In a production microservices environment, each microservice usually gets its own completely separate Git repository and Visual Studio Solution. However, while we are learning and developing locally, it is easiest to keep them inside one solution, but grouped cleanly.

Open Visual Studio 2022 and perform the following structural steps:

Create a new Blank Solution and name it MicroservicesDemo.

Inside Solution Explorer, right-click the solution and create three Solution Folders:

01.CustomersService

02.CatalogService

03.OrdersService

Your Visual Studio Solution Explorer tree will look like this empty skeleton:

MicroservicesDemo/ (Solution)
│
├── 📁 01.CustomersService/
├── 📁 02.CatalogService/
└── 📁 03.OrdersService/


Step 2: Add the Projects

Now, let's create the independent Web API applications. Unlike the modular monolith where we had class libraries, every microservice is an independent, runnable ASP.NET Core Web API project with its own port, its own Program.cs, and its own configurations.

1. Create the Customers Microservice
Right-click on the 01.CustomersService solution folder -> Add -> New Project.

Select ASP.NET Core Web API.

Name the project: Demo.Services.Customers.API.

Choose .NET 8.0 or .NET 9.0.

Uncheck "Configure for HTTPS" (keep it simple for local development for now) or leave it checked if you prefer, but ensure you know its port. Let's assume this service runs on Port 5001.

2. Create the Catalog Microservice
Right-click on the 02.CatalogService solution folder -> Add -> New Project.

Select ASP.NET Core Web API.

Name the project: Demo.Services.Catalog.API.

Target the same .NET version. Let's assume this service runs on Port 5002.

3. Create the Orders Microservice
Right-click on the 03.OrdersService solution folder -> Add -> New Project.

Select ASP.NET Core Web API.

Name the project: Demo.Services.Orders.API.

Target the same .NET version. Let's assume this service runs on Port 5003.

The Golden Rule of Microservice Solutions
Look closely at your Solution Explorer now:
<img width="568" height="288" alt="image" src="https://github.com/user-attachments/assets/f757cf89-1e3c-4088-8c07-72cd252cdebf" /></br>
MicroservicesDemo/
│
├── 📁 01.CustomersService/
│   └── 🚀 Demo.Services.Customers.API
│
├── 📁 02.CatalogService/
│   └── 🚀 Demo.Services.Catalog.API
│
└── 📁 03.OrdersService/
    └── 🚀 Demo.Services.Orders.API

</br>  




    Step 3: Configure Multiple Startup Projects

    Because these apps must talk to each other across the network, you need them all running at the exact same time when you hit "Play" in Visual Studio.

Right-click the top-level solution node (MicroservicesDemo) and select Properties.

On the left menu, click Startup Project.

Select the radio button for Multiple startup projects.

Set the action for all three API projects to Start:

Demo.Services.Customers.API -> Start

Demo.Services.Catalog.API -> Start

Demo.Services.Orders.API -> Start

Click Apply and OK.

Now, when you click the Start button in Visual Studio, three console windows and three separate browser tabs (Swagger pages) will launch simultaneously.


















