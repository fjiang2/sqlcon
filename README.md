# **sqlcon**

console application which connects multiple sql servers in tree structures

See more information, click http://www.datconn.com/products/sqlcon.html

## Overview

### Mount database server an list databases

```javascript
\> mount localdb=localhost\sqlexpress /u:sa /p:password
\localdb> dir
 [1]                  Northwind <DB>         29 Tables/Views
 [2]         AdventureWorks2019 <DB>        132 Tables/Views
        2 Database(s)
\localdb>
```

### Change database directory

```javascript
\localdb>
\localdb> cd Northwind
\localdb\Northwind> cd ..\AdventureWorks2019
\localdb\AdventureWorks2019> cd \localdb\Northwind
\localdb\Northwind> 
```

### List Tables and Views

```javascript
\localdb\Northwind> dir
  [1]             dbo.Categories                            <TABLE>
  [2]             dbo.CustomerCustomerDemo                  <TABLE>
  [3]             dbo.CustomerDemographics                  <TABLE>
  [4]             dbo.Customers                             <TABLE>
  [5]             dbo.Employees                             <TABLE>
  [6]             dbo.EmployeeTerritories                   <TABLE>
  [7]             dbo.Order Details                         <TABLE>
  [8]             dbo.Orders                                <TABLE>
  [9]             dbo.Products                              <TABLE>
 [10]             dbo.Region                                <TABLE>
 [11]             dbo.Shippers                              <TABLE>
 [12]             dbo.Suppliers                             <TABLE>
 [13]             dbo.Territories                           <TABLE>
 [14]             dbo.Alphabetical list of products         <VIEW>
 [15]             dbo.Category Sales for 1997               <VIEW>
 [16]             dbo.Current Product List                  <VIEW>
 [17]             dbo.Customer and Suppliers by City        <VIEW>
 [18]             dbo.Invoices                              <VIEW>
 [19]             dbo.Order Details Extended                <VIEW>
 [20]             dbo.Order Subtotals                       <VIEW>
 [21]             dbo.Orders Qry                            <VIEW>
 [22]             dbo.Product Sales for 1997                <VIEW>
 [23]             dbo.Products Above Average Price          <VIEW>
 [24]             dbo.Products by Category                  <VIEW>
 [25]             dbo.Quarterly Orders                      <VIEW>
 [26]             dbo.Sales by Category                     <VIEW>
 [27]             dbo.Sales Totals by Amount                <VIEW>
 [28]             dbo.Summary of Sales by Quarter           <VIEW>
 [29]             dbo.Summary of Sales by Year              <VIEW>
        13 Table(s)
        16 View(s)
```

### Display Table Schema

```javascript
\localdb\Northwind> cd products
\localdb\Northwind\dbo.Products> dir /def
TABLE: dbo.Products
  [1]                [ProductID] int                   ++,pk   not null
  [2]              [ProductName] nvarchar(40)                  not null
  [3]               [SupplierID] int                      fk       null
  [4]               [CategoryID] int                      fk       null
  [5]          [QuantityPerUnit] nvarchar(20)                      null
  [6]                [UnitPrice] money                             null
  [7]             [UnitsInStock] smallint                          null
  [8]             [UnitsOnOrder] smallint                          null
  [9]             [ReorderLevel] smallint                          null
 [10]             [Discontinued] bit                           not null
        10 Column(s)

\localdb\Northwind\dbo.Products> dir ..\Orders /def
TABLE: dbo.Orders
  [1]                  [OrderID] int                   ++,pk   not null
  [2]               [CustomerID] nchar(5)                 fk       null
  [3]               [EmployeeID] int                      fk       null
  [4]                [OrderDate] datetime                          null
  [5]             [RequiredDate] datetime                          null
  [6]              [ShippedDate] datetime                          null
  [7]                  [ShipVia] int                      fk       null
  [8]                  [Freight] money                             null
  [9]                 [ShipName] nvarchar(40)                      null
 [10]              [ShipAddress] nvarchar(60)                      null
 [11]                 [ShipCity] nvarchar(15)                      null
 [12]               [ShipRegion] nvarchar(15)                      null
 [13]           [ShipPostalCode] nvarchar(10)                      null
 [14]              [ShipCountry] nvarchar(15)                      null
        14 Column(s)

```

### Type Table Rows

```csharp

\localdb\Northwind\dbo.Products> type /top:10
+-----------+---------------------------------+------------+------------+---------------------+-----------+------------
| ProductID | ProductName                     | SupplierID | CategoryID | QuantityPerUnit     | UnitPrice | UnitsInStoc
+-----------+---------------------------------+------------+------------+---------------------+-----------+------------
| 1         | Chai                            | 1          | 1          | 10 boxes x 20 bags  | 18.0000   | 39
| 2         | Chang                           | 1          | 1          | 24 - 12 oz bottles  | 19.0000   | 17
| 3         | Aniseed Syrup                   | 1          | 2          | 12 - 550 ml bottles | 10.0000   | 13
| 4         | Chef Anton's Cajun Seasoning    | 2          | 2          | 48 - 6 oz jars      | 22.0000   | 53
| 5         | Chef Anton's Gumbo Mix          | 2          | 2          | 36 boxes            | 21.3500   | 0
| 6         | Grandma's Boysenberry Spread    | 3          | 2          | 12 - 8 oz jars      | 25.0000   | 120
| 7         | Uncle Bob's Organic Dried Pears | 3          | 7          | 12 - 1 lb pkgs.     | 30.0000   | 15
| 8         | Northwoods Cranberry Sauce      | 3          | 2          | 12 - 12 oz jars     | 40.0000   | 6
| 9         | Mishi Kobe Niku                 | 4          | 6          | 18 - 500 g pkgs.    | 97.0000   | 29
| 10        | Ikura                           | 4          | 8          | 12 - 200 ml jars    | 31.0000   | 31
+-----------+---------------------------------+------------+------------+---------------------+-----------+------------
<top 10 rows>

```


### List Tables and Views

```javascript
```
