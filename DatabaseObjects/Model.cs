namespace DatabaseObjects
{
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Singleton class to represent an entire database and it's objects.
	/// </summary>
	public static class Model
	{
		/// <summary>
		/// Initializes static members of the <see cref="Model"/> class.
		/// </summary>
		static Model()
		{
			Model.Tables = new List<Table>();
			Model.Routines = new List<Routine>();
		}

		/// <summary>
		/// Gets the tables represented in the database.
		/// </summary>
		/// <value>
		/// The tables.
		/// </value>
		public static List<Table> Tables
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the routines represented in the database.
		/// </summary>
		/// <value>
		/// The routines.
		/// </value>
		public static List<Routine> Routines
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets all columns for all tables in the database.
		/// </summary>
		/// <value>
		/// The columns.
		/// </value>
		public static IEnumerable<Column> AllColumns
		{
			get
			{
				return Tables.SelectMany(table => table.Columns);
			}
		}

		/// <summary>
		/// Gets all constraints for all tables in the database.
		/// </summary>
		/// <value>
		/// The constraints.
		/// </value>
		public static IEnumerable<Constraint> AllConstraints
		{
			get
			{
				return Tables.SelectMany(table => table.Constraints);
			}
		}

		/// <summary>
		/// Gets all parameters for all routines in the database.
		/// </summary>
		/// <value>
		/// All parameters.
		/// </value>
		public static IEnumerable<RoutineParameter> AllParameters
		{
			get
			{
				return Routines.SelectMany(routine => routine.Parameters);
			}
		}

		/// <summary>
		/// Loads the objects from the database.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		public static void LoadFromDatabase(string connectionString)
		{

			// Load the database objects
			Table.LoadTables(connectionString);
			Routine.LoadRoutines(connectionString);
		}
	}
}
