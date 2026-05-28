# Microservices Demo

The New Multi-Database Design
Instead of one database with three schemas, we will provision three independent databases. Notice that we maintain logical links (like CustomerId and ProductId), but we completely eliminate any cross-database dependencies or assumptions. </br>



### 1. Customers Microservice Database </br>
This database only cares about user/customer identities. </br>
<img width="438" height="345" alt="image" src="https://github.com/user-attachments/assets/379ad44c-5434-4861-bb17-16d85bf63982" /> </br>

```
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
```

</br>
 
### 2. Catalog Microservice Database </br>
This database holds product descriptions and active pricing data.
</br>

```
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
```

</br>
### 3. Orders Microservice Database</br>
This is where the shift happens. In an isolated microservice database, this table cannot have foreign keys pointing to the other databases. It stores the CustomerId and ProductId purely as raw integer values (logical keys).</br>

```
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
```

</br>

### Step 1: Create the Microservices Solution Structure</br>
In a production microservices environment, each microservice usually gets its own completely separate Git repository and Visual Studio Solution. However, while we are learning and developing locally, it is easiest to keep them inside one solution, but grouped cleanly.</br>

Open Visual Studio 2022 and perform the following structural steps:</br>

Create a new Blank Solution and name it MicroservicesDemo.</br>

Inside Solution Explorer, right-click the solution and create three Solution Folders:</br>

01.CustomersService

02.CatalogService

03.OrdersService

Your Visual Studio Solution Explorer tree will look like this empty skeleton:</br>

```
MicroservicesDemo/ (Solution)
│
├── 📁 01.CustomersService/
├── 📁 02.CatalogService/
└── 📁 03.OrdersService/
```

</br>

### Step 2: Add the Projects</br>

Now, let's create the independent Web API applications. Unlike the modular monolith where we had class libraries, every microservice is an independent, runnable ASP.NET Core Web API project with its own port, its own Program.cs, and its own configurations.</br>

#####1. Create the Customers Microservice</br>
Right-click on the 01.CustomersService solution folder -> Add -> New Project.</br>

Select ASP.NET Core Web API.</br>

Name the project: Demo.Services.Customers.API.</br>

Choose .NET 8.0 or .NET 9.0.</br>

Uncheck "Configure for HTTPS" (keep it simple for local development for now) or leave it checked if you prefer, but ensure you know its port. Let's assume this service runs on Port 5001.</br>

2. Create the Catalog Microservice</br>
Right-click on the 02.CatalogService solution folder -> Add -> New Project.</br>

Select ASP.NET Core Web API.</br>
Name the project: Demo.Services.Catalog.API.</br>
Target the same .NET version. Let's assume this service runs on Port 5002.</br>

<h3>3. Create the Orders Microservice</h3></br>
Right-click on the 03.OrdersService solution folder -> Add -> New Project.</br>
Select ASP.NET Core Web API.</br>
Name the project: Demo.Services.Orders.API.</br>
Target the same .NET version. Let's assume this service runs on Port 5003.</br>

The Golden Rule of Microservice Solutions</br>
Look closely at your Solution Explorer now:</br>
<img width="568" height="288" alt="image" src="https://github.com/user-attachments/assets/f757cf89-1e3c-4088-8c07-72cd252cdebf" /></br>

```
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
```

</br>  
Step 3: Configure Multiple Startup Projects</br>
Because these apps must talk to each other across the network, you need them all running at the exact same time when you hit "Play" in Visual Studio.</br>

Right-click the top-level solution node (MicroservicesDemo) and select Properties.</br>
<img width="1008" height="681" alt="image" src="https://github.com/user-attachments/assets/fbee7a8a-6cbc-46fc-b25a-acb2f6ed3991" /></br>

On the left menu, click Startup Project.</br>
Select the radio button for Multiple startup projects.</br>
Set the action for all three API projects to Start:</br>

```
Demo.Services.Customers.API -> Start </br>
Demo.Services.Catalog.API -> Start</br>
Demo.Services.Orders.API -> Start</br>
```

Click Apply and OK.</br>

Now, when you click the Start button in Visual Studio, three console windows and three separate browser tabs (Swagger pages) will launch simultaneously.</br>
<img width="977" height="146" alt="image" src="https://github.com/user-attachments/assets/272cf4f0-c45e-4688-8b48-ae5f099f7a65" />


<img width="938" height="527" alt="image" src="https://github.com/user-attachments/assets/bf3e58f2-39e5-42cc-9874-fbce0b99b1f5" /> 

<img width="717" height="447" alt="image" src="https://github.com/user-attachments/assets/4452d151-b838-48ef-a850-bdc6b9974629" />

<img width="1197" height="410" alt="image" src="https://github.com/user-attachments/assets/d567b0ea-c76f-471f-8315-0119f73f1eaf" />













