
# NoRe.Database.SqLite

 - https://github.com/NoctusRex/NoRe.Database.SqLite
 - https://www.nuget.org/packages/NoRe.Database.SqLite/

## Usage
This project offers a class to create and access a SqLite database. 

## Dependencies

 - .NET Framework 4.8
 - NoRe.Database.Core
 - NoRe.Core
 - System.Data.SQLite
	 - System.Data.SQLite.Core
	 - System.Data.SQLite.EF6
	 - System.Data.SQLite.Linq
	 - EntityFramework

## Classes
### UML
![uml diagramm](https://raw.githubusercontent.com/NoctusRex/NoRe.Database.SqLite/master/uml.jpg)
### Description

#### SqLiteConfiguration
##### Usage
Contains information used in the SQLite connection string for creating and connection to the database.
By using the default constructor the path of the configuration file is set to ".../*StartupDirectory*/SqLiteConfiguration.xml".
##### Attributes
 - **public string DatabasePath**: The path to the database file.
 - **public string DatabaseVersion**: The version of the database (3 recommended).
 - **public string Pwd**: Optional password for the database.

##### Functions
 - **public SqLiteConfiguration()**: Constructor that sets the configuration file path to ".../*StartupDirectory*/SqLiteConfiguration.xml".
 - **public SqLiteConfiguration(string)**: Constructor to manually setthe configuration path.
 - **public override void Read()**: Reads a SqLiteConfiguration from the file specified with the path property.
 - **public override string ToString()**: Returns the SQLite connection string using the filled properties.

#### SqLiteWrapper
##### Usage
This class is used to access a SQLite database. If the database is not present it will be created.

This class implements IDatabase which implements IDisposable.

**Example**

     using (SqLiteWrapper wrapper = new SqLiteWrapper(Path, "3"))
     {
    	  List< Query > queries = new List< Query >
    	   {
    	   	new Query("INSERT INTO test (id, value) VALUES (45618, 'test')"),
    	  	new Query("INSERT INTO test (id, value) VALUES (3, 'test')")
    	   };
    	
    	   wrapper.ExecuteTransaction(queries);
    	   wrapper.ExecuteNonQuery("DELETE FROM test WHERE id = @0 OR id = @1", 45618, 3);
    	
    	 Table t = wrapper.ExecuteReader("SELECT * FROM test");
    	 string value = t.GetValue< string >(1, "value");
       }

##### Attributes

 - **private SqLiteConfiguration Configuration**: Contains information used in the SQLite connection string for creating and connection to the database.
 - **public SQLiteConnection Connection**: The connection used to access the database.
 - **public SQLiteTransaction Transaction**: A transaction object.

##### Functions

- **public SqLiteWrapper(string, string, string="", bool=false)**: This constructor uses the values specified in the parameters to create the connection string. If the boolean parameter is false no configuration file is created. Then the connection is tested and an exception is thrown if no connection could be established.
- **public SqLiteWrapper(string="")**: Creates and/or loads the configuration from the default configuration path or from the path specified in the parameters. The values loaded from the file are used to create the connection string. Then the connection is tested and an exception is thrown if no connection could be established.
 - **public  int ExecuteNonQuery(string, params object[])**: Executes a sql statement with no result and returns the amount of rows that changed.
 - **public  T ExecuteScalar< T >(string, params object[])**: Executes a sql statement that only returns one value.
 - **public  Table ExecuteReader(string, params object[])**: Executes a sql statement that returns multiple values like a "SELECT"- statement.
 -  **public void ExecuteTransaction(List< Query > / string, params object[])**: Executes a list of sql statements after starting a transaction. If an error occurs the transaction is rolled back and an exception is thrown. If all statements were executed successfully the transaction is committed.
 - **public bool TestConnection(out string)**: Tries to connect to the database and returns true if the connection was successfully created. The out parameter returns an error message if the result is false.
 - **private void RollbackTransaction()**: Tries to rollback the current transaction and finally closes the current connection and disposes the transaction.
 - **private void CommitTransaction()**: Tries to commit the current transaction and finally closes the current connection and disposes the transaction.
 - **private void StartTransaction()**: Tries to start a new transaction an opens the connection if not open yet. In case of an exception the transaction is disposed.
 -  **private SQLiteCommand GetCommand(string, params object[])**: Returns a prepared SqLiteCommand object.
 - **public void Dispose()**: Disposes the transaction and connection.