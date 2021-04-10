# **sqlcon**

sqlcon is a console application which connects multiple sql servers in tree structures.

* It supports database servers including SQL Sever, SQL Server Express, LocalDb and Azure SQL Database.
* It supports XML data sources such as ADO.NET DataSet, DataTable which can be connected from http web link and ftp file link.
* It supports JSON data sources.
* It supports Data Contract classes in .NET assembly files as data source.

The main features

* Run SQL commands, SQL scripts.
* Search, edit database or table either in console mode or GUI.
* Compare database, tables accross differnet data sources.
* Copy tables among data sources or databases.
* Export/Import data in XML, JSON.
* Export table schema in C# data contract classes, entity classes, Linq to SQL classes.
* Export read only data rows in Enum, List<>, Dictionary<,> and LookUp<,>
* Export data rows into SQL INSERT/UPDATE/DELETE clauses.

See more information, click http://www.datconn.com/products/sqlcon.html

## Overview

### All Commands and Help

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


### Mount database server and list databases

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

\localdb\Northwind> dir pro*
  [9]             dbo.Products                              <TABLE>
 [22]             dbo.Product Sales for 1997                <VIEW>
 [23]             dbo.Products Above Average Price          <VIEW>
 [24]             dbo.Products by Category                  <VIEW>
        1 Table(s)
        3 View(s)
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

\localdb\Northwind\dbo.Products> cd ..
\localdb\Northwind> dir products /dep
<Dependencies>
+-----------+---------------+-----------+-----------+----------+-----------+---------------------------+
| FK_Schema | FK_Table      | FK_Column | PK_Schema | PK_Table | PK_Column | Constraint_Name           |
+-----------+---------------+-----------+-----------+----------+-----------+---------------------------+
| dbo       | Order Details | ProductID | dbo       | Products | ProductID | FK_Order_Details_Products |
+-----------+---------------+-----------+-----------+----------+-----------+---------------------------+
<1 row>

```

### Type Table Rows

```javascript

\localdb\Northwind\dbo.Products> type /top:10
+-----------+---------------------------------+------------+------------+---------------------+-----------+--------------+--------------+--------------+--------------+
| ProductID | ProductName                     | SupplierID | CategoryID | QuantityPerUnit     | UnitPrice | UnitsInStock | UnitsOnOrder | ReorderLevel | Discontinued |
+-----------+---------------------------------+------------+------------+---------------------+-----------+--------------+--------------+--------------+--------------+
| 1         | Chai                            | 1          | 1          | 10 boxes x 20 bags  | 18.0000   | 39           | 0            | 10           | False        |
| 2         | Chang                           | 1          | 1          | 24 - 12 oz bottles  | 19.0000   | 17           | 40           | 25           | False        |
| 3         | Aniseed Syrup                   | 1          | 2          | 12 - 550 ml bottles | 10.0000   | 13           | 70           | 25           | False        |
| 4         | Chef Anton's Cajun Seasoning    | 2          | 2          | 48 - 6 oz jars      | 22.0000   | 53           | 0            | 0            | False        |
| 5         | Chef Anton's Gumbo Mix          | 2          | 2          | 36 boxes            | 21.3500   | 0            | 0            | 0            | True         |
| 6         | Grandma's Boysenberry Spread    | 3          | 2          | 12 - 8 oz jars      | 25.0000   | 120          | 0            | 25           | False        |
| 7         | Uncle Bob's Organic Dried Pears | 3          | 7          | 12 - 1 lb pkgs.     | 30.0000   | 15           | 0            | 10           | False        |
| 8         | Northwoods Cranberry Sauce      | 3          | 2          | 12 - 12 oz jars     | 40.0000   | 6            | 0            | 0            | False        |
| 9         | Mishi Kobe Niku                 | 4          | 6          | 18 - 500 g pkgs.    | 97.0000   | 29           | 0            | 0            | True         |
| 10        | Ikura                           | 4          | 8          | 12 - 200 ml jars    | 31.0000   | 31           | 0            | 0            | False        |
+-----------+---------------------------------+------------+------------+---------------------+-----------+--------------+--------------+--------------+--------------+
<top 10 rows>

```

### Search in Table

```csharp
\localdb\Northwind\dbo.Products> type *tofu*
+-----------+---------------+------------+------------+------------------+-----------+--------------+--------------+--------------+--------------+
| ProductID | ProductName   | SupplierID | CategoryID | QuantityPerUnit  | UnitPrice | UnitsInStock | UnitsOnOrder | ReorderLevel | Discontinued |
+-----------+---------------+------------+------------+------------------+-----------+--------------+--------------+--------------+--------------+
| 14        | Tofu          | 6          | 7          | 40 - 100 g pkgs. | 23.2500   | 35           | 0            | 0            | False        |
| 74        | Longlife Tofu | 4          | 7          | 5 kg pkg.        | 10.0000   | 4            | 20           | 5            | False        |
+-----------+---------------+------------+------------+------------------+-----------+--------------+--------------+--------------+--------------+
<2 rows>

\localdb\Northwind\dbo.Products> type UnitPrice>100
+-----------+-------------------------+------------+------------+----------------------+-----------+--------------+--------------+--------------+--------------+
| ProductID | ProductName             | SupplierID | CategoryID | QuantityPerUnit      | UnitPrice | UnitsInStock | UnitsOnOrder | ReorderLevel | Discontinued |
+-----------+-------------------------+------------+------------+----------------------+-----------+--------------+--------------+--------------+--------------+
| 29        | Thüringer Rostbratwurst | 12         | 6          | 50 bags x 30 sausgs. | 123.7900  | 0            | 0            | 0            | True         |
| 38        | Côte de Blaye           | 18         | 1          | 12 - 75 cl bottles   | 263.5000  | 17           | 0            | 15           | False        |
+-----------+-------------------------+------------+------------+----------------------+-----------+--------------+--------------+--------------+--------------+
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

### Edit Data Rows in Table
```javascript
\localdb\Northwind\dbo.Products> type /edit
```

![Image of Yaktocat](https://github.com/fjiang2/sqlcon/blob/master/images/edit-products.png?raw=true)

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



## SQL Commands

```javascript
\localdb\Northwind> select * from products where unitprice>90;
+-----------+-------------------------+------------+------------+----------------------+-----------+--------------+--------------+--------------+--------------+
| ProductID | ProductName             | SupplierID | CategoryID | QuantityPerUnit      | UnitPrice | UnitsInStock | UnitsOnOrder | ReorderLevel | Discontinued |
+-----------+-------------------------+------------+------------+----------------------+-----------+--------------+--------------+--------------+--------------+
| 9         | Mishi Kobe Niku         | 4          | 6          | 18 - 500 g pkgs.     | 97.0000   | 29           | 0            | 0            | True         |
| 29        | Thüringer Rostbratwurst | 12         | 6          | 50 bags x 30 sausgs. | 123.7900  | 0            | 0            | 0            | True         |
| 38        | Côte de Blaye           | 18         | 1          | 12 - 75 cl bottles   | 263.5000  | 17           | 0            | 15           | False        |
+-----------+-------------------------+------------+------------+----------------------+-----------+--------------+--------------+--------------+--------------+
<3 rows>

```



## Alter Table

```javascript
command attrib: update column property
add primary key, foreign key or identity key
columns:
  attrib [table] +c:col1=varchar(2)+null : add column or alter column
  attrib [table] +c:col1=varchar(10)     : add column or alter column
  attrib [table] -c:col1                 : remove column
primary keys:
  attrib [table] +p:col1,col2            : add primary key
  attrib [table] +p:col1,col2            : remove primary key
foreign keys:
  attrib [table] +f:col1=table2[.col2]   : add foreign key
  attrib [table] -f:col1                 : remove foreign key
identiy key:
  attrib [table] +i:col1                 : add identity
  attrib [table] -i:col1                 : remove identity
refine columns:
  attrib [table] /refine                 : refine column type and nullable
  attrib [table] /refine  /commit        : refine and save changes
  refine option:
    /not-null                            : change to NOT NULL
    /int                                 : convert to int
    /bit                                 : convert to bit
    /string                              : shrink string(NVARCHAR,VARCHAR,NCHAR,CHAR)
```

## Compare Table

```javascript
\localdb\Northwind_prod> compare /?
compare table schema or records
compare path1 [path2]  : compare data
compare [/s]           : compare schema
compare [/e]           : find common existing table names
compare [/count]       : compare number of rows
        [/pk]          : if primary key doesn't exist
                         for example /pk:table1=pk1+pk2,table=pk1

```

### Compare Schema

```javascript
\localdb\Northwind_prod> attrib Products +c:Description=varchar(20)+null

\localdb\Northwind_prod> dir Products /def
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
 [11]              [Description] varchar(20)                       null
        11 Column(s)

\localdb\Northwind_prod> compare Products ..\northwind\products /s /out:c:\temp\cmp_products.sql
server1: (LocalDB)\MSSQLLocalDB default database:Northwind_prod
server2: (LocalDB)\MSSQLLocalDB default database:Northwind
completed to compare table schema [Northwind_prod].dbo.[Products] => [Northwind].dbo.[Products]
ALTER TABLE [Products] ADD [Description] varchar(20) NULL
result in "c:\temp\cmp_products.sql"        

\localdb\Northwind_prod> ltype c:\temp\cmp_Products.sql
-- sqlcon:
-- compare server=(LocalDB)\MSSQLLocalDB db=Northwind_prod
--         server=(LocalDB)\MSSQLLocalDB db=Northwind @ 4/10/2021 9:04:12 AM
ALTER TABLE [Products] ADD [Description] varchar(20) NULL

```

### Compare Data Rows

```javascript

\localdb\Northwind_prod> compare Products ..\northwind\products /out:c:\temp\cmp_products.sql
server1: (LocalDB)\MSSQLLocalDB default database:Northwind_prod
server2: (LocalDB)\MSSQLLocalDB default database:Northwind
failed to compare becuase of different table schemas
result in "c:\temp\cmp_products.sql"

\localdb\Northwind_prod> attrib Products -c:Description

\localdb\Northwind_prod> UPDATE Products SET ProductName='Apple' WHERE ProductID=4;
1 of row(s) affected

\localdb\Northwind_prod> compare Products ..\Northwind /out:c:\temp\cmp_Products.sql
server1: (LocalDB)\MSSQLLocalDB default database:Northwind_prod
server2: (LocalDB)\MSSQLLocalDB default database:Northwind
completed to compare table data [Northwind_prod].dbo.[Products] => [Northwind].dbo.[Products]
UPDATE [Products] SET [ProductName] = N'Apple' WHERE [ProductID] = 4
GO
result in "c:\temp\cmp_Products.sql"

\localdb\Northwind_prod> ltype c:\temp\cmp_Products.sql
-- sqlcon:
-- compare server=(LocalDB)\MSSQLLocalDB db=Northwind_prod
--         server=(LocalDB)\MSSQLLocalDB db=Northwind @ 4/10/2021 9:17:22 AM
UPDATE [Products] SET [ProductName] = N'Apple' WHERE [ProductID] = 4
GO

```

### Compare Multiple Table

```javascript
\localdb\Northwind_prod> compare Custom* ..\Northwind /out:c:\temp\cmp.sql
server1: (LocalDB)\MSSQLLocalDB default database:Northwind_prod
server2: (LocalDB)\MSSQLLocalDB default database:Northwind
completed to compare table data [Northwind_prod].dbo.[CustomerDemographics] => [Northwind].dbo.[CustomerDemographics]
DELETE FROM [CustomerDemographics] WHERE [CustomerTypeID] = N'IT        '
DELETE FROM [CustomerDemographics] WHERE [CustomerTypeID] = N'EE        '
GO

completed to compare table data [Northwind_prod].dbo.[Customers] => [Northwind].dbo.[Customers]
completed to compare table data [Northwind_prod].dbo.[CustomerCustomerDemo] => [Northwind].dbo.[CustomerCustomerDemo]
DELETE FROM [CustomerCustomerDemo] WHERE [CustomerID] = N'ALFKI' AND [CustomerTypeID] = N'IT        '
GO

result in "c:\temp\cmp.sql"

```

## Export Data

```javascript
\localdb\Northwind_prod> export Products /INSERT /out:c:\temp\Products.sql
INSERT clauses (SELECT * FROM [Northwind_prod].dbo.[Products]) generated to "c:\temp\Products.sql", Done on rows(77)

\localdb\Northwind_prod> ltype c:\temp\products.sql
INSERT INTO [Products]([ProductName],[SupplierID],[CategoryID],[QuantityPerUnit],[UnitPrice],[UnitsInStock],[UnitsOnOrder],[ReorderLevel],[Discontinued]) VALUES(N'Chai',1,1,N'10 boxes x 20 bags',18.0000,39,0,10,0)
INSERT INTO [Products]([ProductName],[SupplierID],[CategoryID],[QuantityPerUnit],[UnitPrice],[UnitsInStock],[UnitsOnOrder],[ReorderLevel],[Discontinued]) VALUES(N'Chang',1,1,N'24 - 12 oz bottles',19.0000,17,40,25,0)
INSERT INTO [Products]([ProductName],[SupplierID],[CategoryID],[QuantityPerUnit],[UnitPrice],[UnitsInStock],[UnitsOnOrder],[ReorderLevel],[Discontinued]) VALUES(N'Aniseed Syrup',1,2,N'12 - 550 ml bottles',10.0000,13,70,25,0)


\localdb\Northwind_prod> export Products /INSERT /if /out:c:\temp\Products.sql
INSERT clauses (SELECT * FROM [Northwind_prod].dbo.[Products]) generated to "c:\temp\Products.sql", Done on rows(77)

\localdb\Northwind_prod> ltype c:\temp\products.sql
IF NOT EXISTS(SELECT * FROM [Products] WHERE [ProductID] = 1) INSERT INTO [Products]([ProductName],[SupplierID],[CategoryID],[QuantityPerUnit],[UnitPrice],[UnitsInStock],[UnitsOnOrder],[ReorderLevel],[Discontinued]) VALUES(N'Chai',1,1,N'10 boxes x 20 bags',18.0000,39,0,10,0)
IF NOT EXISTS(SELECT * FROM [Products] WHERE [ProductID] = 2) INSERT INTO [Products]([ProductName],[SupplierID],[CategoryID],[QuantityPerUnit],[UnitPrice],[UnitsInStock],[UnitsOnOrder],[ReorderLevel],[Discontinued]) VALUES(N'Chang',1,1,N'24 - 12 oz bottles',19.0000,17,40,25,0)
IF NOT EXISTS(SELECT * FROM [Products] WHERE [ProductID] = 3) INSERT INTO [Products]([ProductName],[SupplierID],[CategoryID],[QuantityPerUnit],[UnitPrice],[UnitsInStock],[UnitsOnOrder],[ReorderLevel],[Discontinued]) VALUES(N'Aniseed Syrup',1,2,N'12 - 550 ml bottles',10.0000,13,70,25,0)


\localdb\Northwind_prod> export Products /SAVE /IF /out:c:\temp\Products.sql
INSERT_OR_UPDATE clauses (SELECT * FROM [Northwind_prod].dbo.[Products]) generated to "c:\temp\Products.sql", Done on rows(77)

\localdb\Northwind_prod> ltype c:\temp\products.sql
IF NOT EXISTS(SELECT * FROM [Products] WHERE [ProductID] = 1) INSERT INTO [Products]([ProductName],[SupplierID],[CategoryID],[QuantityPerUnit],[UnitPrice],[UnitsInStock],[UnitsOnOrder],[ReorderLevel],[Discontinued]) VALUES(N'Chai',1,1,N'10 boxes x 20 bags',18.0000,39,0,10,0) ELSE UPDATE [Products] SET [ProductName] = N'Chai',[SupplierID] = 1,[CategoryID] = 1,[QuantityPerUnit] = N'10 boxes x 20 bags',[UnitPrice] = 18.0000,[UnitsInStock] = 39,[UnitsOnOrder] = 0,[ReorderLevel] = 10,[Discontinued] = 0 WHERE [ProductID] = 1
IF NOT EXISTS(SELECT * FROM [Products] WHERE [ProductID] = 2) INSERT INTO [Products]([ProductName],[SupplierID],[CategoryID],[QuantityPerUnit],[UnitPrice],[UnitsInStock],[UnitsOnOrder],[ReorderLevel],[Discontinued]) VALUES(N'Chang',1,1,N'24 - 12 oz bottles',19.0000,17,40,25,0) ELSE UPDATE [Products] SET [ProductName] = N'Chang',[SupplierID] = 1,[CategoryID] = 1,[QuantityPerUnit] = N'24 - 12 oz bottles',[UnitPrice] = 19.0000,[UnitsInStock] = 17,[UnitsOnOrder] = 40,[ReorderLevel] = 25,[Discontinued] = 0 WHERE [ProductID] = 2
IF NOT EXISTS(SELECT * FROM [Products] WHERE [ProductID] = 3) INSERT INTO [Products]([ProductName],[SupplierID],[CategoryID],[QuantityPerUnit],[UnitPrice],[UnitsInStock],[UnitsOnOrder],[ReorderLevel],[Discontinued]) VALUES(N'Aniseed Syrup',1,2,N'12 - 550 ml bottles',10.0000,13,70,25,0) ELSE UPDATE [Products] SET [ProductName] = N'Aniseed Syrup',[SupplierID] = 1,[CategoryID] = 2,[QuantityPerUnit] = N'12 - 550 ml bottles',[UnitPrice] = 10.0000,[UnitsInStock] = 13,[UnitsOnOrder] = 70,[ReorderLevel] = 25,[Discontinued] = 0 WHERE [ProductID] = 3
....


```

```javascript
```

```javascript
```

```javascript
```

## Advanced Commands

```javascript
\> mount /?
mount database server
mount alias=server_name   : alias must start with letter
options:
   /db:database           : initial catalog, default is 'master'
   /u:username            : user id, default is 'sa'
   /p:password            : password, default is empty, use Windows Security when /u /p not setup
   /pvd:provider          : default is SQL Server client
        sqldb               SQL Server, default provider
        sqloledb            ODBC Database Server
        file/db/xml         sqlcon Database Schema, default provider for xml file
        file/dataset/json   System.Data.DataSet
        file/dataset/xml    System.Data.DataSet
        file/datalake/json  Dictionary<string, System.Data.DataSet>
        file/datalake/xml   Dictionary<string, System.Data.DataSet>
        file/assembly       .Net assembly dll
        file/c#             C# data contract classes
        riadb               Remote Invoke Agent
   /namespace:xxx           wildcard of namespace name filter on assembly
   /class:xxxx              wildcard of class name filter on assembly
example:
  mount ip100=192.168.0.100\sqlexpress /u:sa /p:p@ss
  mount web=http://192.168.0.100/db/northwind.xml /u:sa /p:p@ss
  mount xml=file://c:\db\northwind.xml
  mount cs=file://c:\db\northwind.cs /pvd:file/c#
  mount dll=file://c:\db\any.dll /pvd:file/assembly /namespace:Sys* /class:Employee*
\>  
\> mount localdb=localhost\sqlexpress /u:sa /p:password
\localdb> dir
 [1]                  Northwind <DB>         29 Tables/Views
 [2]         AdventureWorks2019 <DB>        132 Tables/Views
        2 Database(s)
\localdb>
```
