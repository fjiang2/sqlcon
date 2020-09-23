# sqlcon
console application which connects multiple sql servers in tree structures

See more information, click http://www.datconn.com/products/sqlcon.html

# Overview

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

```csharp
\localdb\Northwind> cd products

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
