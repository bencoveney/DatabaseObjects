Database Objects
================

Database Objects is a library of utility classes for inspecting database schemas.

Features
--------

Provides classes representing various database objects.

Loads the definitions of the following database objects from Microsoft Sql Server Databases:
 * Tables
   * Columns
   * Constraints (with target columns)
 * Routines (Procedures And Functions)
   * Parameters

This includes names, data types and other related information.

Sample Code
-----------

Loading a database model:

```c#
Model model = Model.LoadFromDatabase(ConnectionString);
```

Using some of the loaded model items:

```c#
foreach (Table table in model.Tables)
{
	Console.WriteLine(table.Name);

	foreach (Column column in table.Columns)
	{
		Console.WriteLine(column.Name);

		Console.WriteLine(column.DataType.DataType);
	}

	foreach (Constraint constraint in table.Constraints)
	{
		Console.WriteLine(constraint.Name);

		Console.WriteLine(constraint.ConstraintType);
	}
}

foreach (Routine routine in model.Routines)
{
	Console.WriteLine(routine.Name);

	Console.WriteLine(routine.RoutineType);

	foreach (RoutineParameter parameter in routine.Parameters)
	{
		Console.WriteLine(parameter.Name);

		Console.WriteLine(parameter.Mode);

		Console.WriteLine(parameter.DataType.DataType);
	}
}
```

Contributing
------------

The project includes The following projects:
 * **Database Objects** - The main library.
 * **Tests:**
   * **Functional Tests** - Tests which exercise the codes core functionality.
   * **Test Database** - A database project providing the schema for the functional tests.

The database objects class adheres to all rules in the Microsoft managed code analysis rulesets and a subset of StyleCop's coding standards ruleset (included in the repository).