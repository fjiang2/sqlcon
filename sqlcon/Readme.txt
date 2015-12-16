Examples:



\my\Northwind> help
Path points to server, database,tables, data rows
      \server\database\table\filter\filter\....
Notes: table names support wildcard matching, e.g. Prod*,Pro?ucts
exit                    : quit application
help                    : this help
?                       : this help
cls                     : clears the screen
dir,ls /?               : see more info
cd,chdir /?             : see more info
md,mkdir /?             : see more info
rd,rmdir /?             : see more info
type /?                 : see more info
set /?                  : see more info
del,erase /?            : see more info
ren,rename /?           : see more info
copy /?                 : see more info
comp /?                 : see more info
echo                    : display message
rem                     : records comments/remarks
ver                     : display version
compare path1 [path2]   : compare table scheam or data
          /s            : compare schema otherwise compare data
          /col:c1,c2    : skip columns defined during comparing
export /?               : see more info
clean /?                : see more info

<Commands>
<find> pattern          : find table name or column name
<show view>             : show all views
<show proc>             : show all stored proc and func
<show index>            : show all indices
<show vw> viewnames     : show view structure
<show pk>               : show all tables with primary keys
<show npk>              : show all tables without primary keys
<show connection>       : show connection-string list
<show current>          : show current active connection-string
<show var>              : show variable list
<run> query(..)         : run predefined query. e.g. run query(var1=val1,...);
<sync table1 table2>    : synchronize, make table2 is the same as table1
<xcopy output>          : copy sql script ouput to clipboard
<open log>              : open log file
<open input>            : open input file
<open output>           : open output file
<open schema>           : open schema file
<execute inputfile>     : execute sql script file
<execute variable /s>   : execute script file list defined on the configuration file

type [;] to execute following SQL script or functions
<SQL>
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

\my\Northwind> 

\my\Northwind> dir
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
 [13]             dbo.sysdiagrams                           <TABLE>
 [14]             dbo.Territories                           <TABLE>
 [15]             dbo.Alphabetical list of products         <VIEW>
 [16]             dbo.Category Sales for 1997               <VIEW>
 [17]             dbo.Current Product List                  <VIEW>
 [18]             dbo.Customer and Suppliers by City        <VIEW>
 [19]             dbo.Invoices                              <VIEW>
 [20]             dbo.Order Details Extended                <VIEW>
 [21]             dbo.Order Subtotals                       <VIEW>
 [22]             dbo.Orders Qry                            <VIEW>
 [23]             dbo.Product Sales for 1997                <VIEW>
 [24]             dbo.Products Above Average Price          <VIEW>
 [25]             dbo.Products by Category                  <VIEW>
 [26]             dbo.Quarterly Orders                      <VIEW>
 [27]             dbo.Sales by Category                     <VIEW>
 [28]             dbo.Sales Totals by Amount                <VIEW>
 [29]             dbo.Summary of Sales by Quarter           <VIEW>
 [30]             dbo.Summary of Sales by Year              <VIEW>
	14 Table(s)
	16 View(s)

\my\Northwind> cd customers

\my\Northwind\dbo.Customers> type
+------------+--------------------------------------+-------------------------+--------------------------------+------------------------------------------------+-----------------+---------------+------------+-------------+-------------------+-------------------+
| CustomerID | CompanyName                          | ContactName             | ContactTitle                   | Address                                        | City            | Region        | PostalCode | Country     | Phone             | Fax               |
+------------+--------------------------------------+-------------------------+--------------------------------+------------------------------------------------+-----------------+---------------+------------+-------------+-------------------+-------------------+
| ALFKI      | Alfreds Futterkiste                  | Maria Anders            | Sales Representative           | Obere Str. 57                                  | Berlin          | NULL          | 12209      | Germany     | 030-0074321       | 030-0076545       |
| ANATR      | Ana Trujillo Emparedados y helados   | Ana Trujillo            | Owner                          | Avda. de la Constitución 2222                  | México D.F.     | NULL          | 05021      | Mexico      | (5) 555-4729      | (5) 555-3745      |
| ANTON      | Antonio Moreno Taquería              | Antonio Moreno          | Owner                          | Mataderos  2312                                | México D.F.     | NULL          | 05023      | Mexico      | (5) 555-3932      | NULL              |
| AROUT      | Around the Horn                      | Thomas Hardy            | Sales Representative           | 120 Hanover Sq.                                | London          | NULL          | WA1 1DP    | UK          | (171) 555-7788    | (171) 555-6750    |
| BERGS      | Berglunds snabbköp                   | Christina Berglund      | Order Administrator            | Berguvsvägen  8                                | Luleå           | NULL          | S-958 22   | Sweden      | 0921-12 34 65     | 0921-12 34 67     |
| BLAUS      | Blauer See Delikatessen              | Hanna Moos              | Sales Representative           | Forsterstr. 57                                 | Mannheim        | NULL          | 68306      | Germany     | 0621-08460        | 0621-08924        |
| BLONP      | Blondesddsl père et fils             | Frédérique Citeaux      | Marketing Manager              | 24, place Kléber                               | Strasbourg      | NULL          | 67000      | France      | 88.60.15.31       | 88.60.15.32       |
| BOLID      | Bólido Comidas preparadas            | Martín Sommer           | Owner                          | C/ Araquil, 67                                 | Madrid          | NULL          | 28023      | Spain       | (91) 555 22 82    | (91) 555 91 99    |
| BONAP      | Bon app'                             | Laurence Lebihan        | Owner                          | 12, rue des Bouchers                           | Marseille       | NULL          | 13008      | France      | 91.24.45.40       | 91.24.45.41       |
| BOTTM      | Bottom-Dollar Markets                | Elizabeth Lincoln       | Accounting Manager             | 23 Tsawassen Blvd.                             | Tsawassen       | BC            | T2F 8M4    | Canada      | (604) 555-4729    | (604) 555-3745    |
............
| VAFFE      | Vaffeljernet                         | Palle Ibsen             | Sales Manager                  | Smagsloget 45                                  | Århus           | NULL          | 8200       | Denmark     | 86 21 32 43       | 86 22 33 44       |
| VICTE      | Victuailles en stock                 | Mary Saveley            | Sales Agent                    | 2, rue du Commerce                             | Lyon            | NULL          | 69004      | France      | 78.32.54.86       | 78.32.54.87       |
| VINET      | Vins et alcools Chevalier            | Paul Henriot            | Accounting Manager             | 59 rue de l'Abbaye                             | Reims           | NULL          | 51100      | France      | 26.47.15.10       | 26.47.15.11       |
| WANDK      | Die Wandernde Kuh                    | Rita Müller             | Sales Representative           | Adenauerallee 900                              | Stuttgart       | NULL          | 70563      | Germany     | 0711-020361       | 0711-035428       |
| WARTH      | Wartian Herkku                       | Pirkko Koskitalo        | Accounting Manager             | Torikatu 38                                    | Oulu            | NULL          | 90110      | Finland     | 981-443655        | 981-443655        |
| WELLI      | Wellington Importadora               | Paula Parente           | Sales Manager                  | Rua do Mercado, 12                             | Resende         | SP            | 08737-363  | Brazil      | (14) 555-8122     | NULL              |
| WHITC      | White Clover Markets                 | Karl Jablonski          | Owner                          | 305 - 14th Ave. S. Suite 3B                    | Seattle         | WA            | 98128      | USA         | (206) 555-4112    | (206) 555-4115    |
| WILMK      | Wilman Kala                          | Matti Karttunen         | Owner/Marketing Assistant      | Keskuskatu 45                                  | Helsinki        | NULL          | 21240      | Finland     | 90-224 8858       | 90-224 8858       |
| WOLZA      | Wolski  Zajazd                       | Zbyszek Piestrzeniewicz | Owner                          | ul. Filtrowa 68                                | Warszawa        | NULL          | 01-012     | Poland      | (26) 642-7012     | (26) 642-7012     |
+------------+--------------------------------------+-------------------------+--------------------------------+------------------------------------------------+-----------------+---------------+------------+-------------+-------------------+-------------------+
<91 rows>

\my\Northwind\dbo.Customers> dir /def
TABLE: dbo.Customers
  [1]               [CustomerID] nchar(5)                 pk   not null
  [2]              [CompanyName] nvarchar(40)                  not null
  [3]              [ContactName] nvarchar(30)                      null
  [4]             [ContactTitle] nvarchar(30)                      null
  [5]                  [Address] nvarchar(60)                      null
  [6]                     [City] nvarchar(15)                      null
  [7]                   [Region] nvarchar(15)                      null
  [8]               [PostalCode] nvarchar(10)                      null
  [9]                  [Country] nvarchar(15)                      null
 [10]                    [Phone] nvarchar(24)                      null
 [11]                      [Fax] nvarchar(24)                      null
	11 Column(s)

\my\Northwind\dbo.Customers> cd ..


\my\Northwind> select customerId,companyname
...from customers;
+------------+--------------------------------------+
| customerId | companyname                          |
+------------+--------------------------------------+
| ALFKI      | Alfreds Futterkiste                  |
| ANATR      | Ana Trujillo Emparedados y helados   |
| ANTON      | Antonio Moreno Taquería              |
| AROUT      | Around the Horn                      |
| BERGS      | Berglunds snabbköp                   |
| BLAUS      | Blauer See Delikatessen              |
| BLONP      | Blondesddsl père et fils             |
| BOLID      | Bólido Comidas preparadas            |
.....
| THECR      | The Cracker Box                      |
| TOMSP      | Toms Spezialitäten                   |
| TORTU      | Tortuga Restaurante                  |
| TRADH      | Tradição Hipermercados               |
| TRAIH      | Trail's Head Gourmet Provisioners    |
| VAFFE      | Vaffeljernet                         |
| VICTE      | Victuailles en stock                 |
| VINET      | Vins et alcools Chevalier            |
| WARTH      | Wartian Herkku                       |
| WELLI      | Wellington Importadora               |
| WHITC      | White Clover Markets                 |
| WILMK      | Wilman Kala                          |
| WOLZA      | Wolski  Zajazd                       |
+------------+--------------------------------------+
<91 rows>

\my\Northwind> dir /?
command dir or ls
dir [path]     : display current directory
options: 
   /def        : display table structure
   /pk         : display table primary keys
   /fk         : display table foreign keys
   /ik         : display table identity keys
   /dep        : display table dependencies
   /ind        : display table index/indices
   /sto        : display table storage
   /refresh    : refresh table structure

\my\Northwind> dir \
 [1]                      local <SVR>          3 Databases
 [2]                        xml <SVR>          ? Databases
 [3]                     medsys <SVR>          3 Databases
 [4]                         my <SVR>          3 Databases
 [5]                     aw2014 <SVR>          3 Databases
	5 Server(s)
	
\my\Northwind> export /?
export data, schema, class, and template
option:
   /insert  : export INSERT INTO script on current table/database
   [/if]    : option /if generate if exists row then UPDATE else INSERT
   /create  : generate CREATE TABLE script on current table/database
   /select  : generate SELECT FROM WHERE template
   /update  : generate UPDATE SET WHERE template
   /delete  : generate DELETE FROM WHERE template, delete rows with foreign keys constraints
   /schema  : generate database schema xml file
   /data    : generate database/table data xml file
   /class   : generate C# table class

\my\Northwind> 

