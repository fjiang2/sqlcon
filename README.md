# **sqlcon**

console application which connects multiple sql servers in tree structures

See more information, click http://www.datconn.com/products/sqlcon.html

## Overview

### All Command and Help

```javascript
\localdb> help
Path points to server, database,tables, data rows
      \server\database\table\filter\filter\....
Notes: table names support wildcard matching, e.g. Prod*,Pro?ucts
exit                    : quit application
help                    : this help
?                       : this help
rem                     : comments or remarks
ver                     : display version
cls                     : clears the screen
echo /?                 : display text
dir,ls /?               : display path(server, database, table)
cd,chdir /?             : change path
md,mkdir /?             : create path or filter
rd,rmdir /?             : remove path or filter
type /?                 : type content of table
set /?                  : update values
let /?                  : assign value to variable, see more info
del,erase /?            : delete path
ren,rename /?           : rename database, table, column name
attrib /?               : add/remove primary key, foreign key and identity key
copy /?                 : copy table schema or rows
xcopy /?                : copy large size table
comp /?                 : compare table schema or data
compare path1 [path2]   : compare table scheam or data
          /s            : compare schema, otherwise compare data
          /e            : compare common existing tables only
          /col:c1,c2    : skip columns defined during comparing
sync table1 table2      : synchronize, make table2 is the same as table1
import /?               : import data into database
export /?               : generate SQL script, JSON, C# code
clean /?                : clean duplicated rows
mount /?                : mount new database server
umount /?               : unmount database server
open /?                 : open result file
load /?                 : load JSON, XML data and cfg file
save /?                 : save data
edit /?                 : open GUI edit window
chk,check /?            : check syntax of key-value table
last                    : display last result

<File Command>
lcd [path]              : change or display current directory
ldir [path]             : display local files on the directory
ltype [path]            : display local file content
path [path]             : set environment variable PATH
run [path]file          : run a batch program (.sqc)
call [path]file [/dump] : call Tie program (.sqt), if option /dump used, memory dumps to output file
execute [path]file      : execute sql script(.sql)

<Schema Commands>
find /?                 : see more info
show view               : show all views
show proc               : show all stored proc and func
show index              : show all indices
show vw viewnames       : show view structure
show pk                 : show all tables with primary keys
show npk                : show all tables without primary keys

<State Command>
show connection         : show connection-string list
show current            : show current active connection-string
show var                : show variable list

<SQL Command>
type [;] to execute following SQL script or functions
select ... from table where ...
update table set ... where ...
delete from table where...
create table ...
drop table ...
alter ...
exec ...
<Variables>
  maxrows               : max number of row shown on select query
  DataReader            : true: use SqlDataReader; false: use Fill DataSet
```


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

### Search in Table

```csharp
\localdb\Northwind\dbo.Products> type *tofu*
+-----------+---------------+------------+------------+------------------+-----------+------------
| ProductID | ProductName   | SupplierID | CategoryID | QuantityPerUnit  | UnitPrice | UnitsInStoc
+-----------+---------------+------------+------------+------------------+-----------+------------
| 14        | Tofu          | 6          | 7          | 40 - 100 g pkgs. | 23.2500   | 35
| 74        | Longlife Tofu | 4          | 7          | 5 kg pkg.        | 10.0000   | 4
+-----------+---------------+------------+------------+------------------+-----------+------------
<2 rows>

\localdb\Northwind\dbo.Products> type UnitPrice>100
+-----------+-------------------------+------------+------------+----------------------+----------
| ProductID | ProductName             | SupplierID | CategoryID | QuantityPerUnit      | UnitPrice
+-----------+-------------------------+------------+------------+----------------------+----------
| 29        | Thüringer Rostbratwurst | 12         | 6          | 50 bags x 30 sausgs. | 123.7900
| 38        | Côte de Blaye           | 18         | 1          | 12 - 75 cl bottles   | 263.5000
+-----------+-------------------------+------------+------------+----------------------+----------
<2 rows>

\localdb\Northwind\dbo.Products> type *tofu* /t
+-----------------+------------------+---------------+
| ProductID       | 14               | 74            |
| ProductName     | Tofu             | Longlife Tofu |
| SupplierID      | 6                | 4             |
| CategoryID      | 7                | 7             |
| QuantityPerUnit | 40 - 100 g pkgs. | 5 kg pkg.     |
| UnitPrice       | 23.2500          | 10.0000       |
| UnitsInStock    | 35               | 4             |
| UnitsOnOrder    | 0                | 20            |
| ReorderLevel    | 0                | 5             |
| Discontinued    | False            | False         |
+-----------------+------------------+---------------+
<2 rows>
```

### Output as JSON

```javascript
\localdb\Northwind\dbo.Products> type *tofu* /json
{
  "Products" : [
    {
      "ProductID" : 14,
      "ProductName" : "Tofu",
      "SupplierID" : 6,
      "CategoryID" : 7,
      "QuantityPerUnit" : "40 - 100 g pkgs.",
      "UnitPrice" : (decimal)23.2500,
      "UnitsInStock" : 35,
      "UnitsOnOrder" : 0,
      "ReorderLevel" : 0,
      "Discontinued" : false
    },
    {
      "ProductID" : 74,
      "ProductName" : "Longlife Tofu",
      "SupplierID" : 4,
      "CategoryID" : 7,
      "QuantityPerUnit" : "5 kg pkg.",
      "UnitPrice" : (decimal)10.0000,
      "UnitsInStock" : 4,
      "UnitsOnOrder" : 20,
      "ReorderLevel" : 5,
      "Discontinued" : false
    }
  ]
}
```

### Next Command

```javascript
```
