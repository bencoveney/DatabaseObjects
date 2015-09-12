namespace ItemLoader
{
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Singleton class to represent an entire database and it's objects.
	/// </summary>
	public static class DatabaseModel
	{
		/// <summary>
		/// Initializes static members of the <see cref="DatabaseModel"/> class.
		/// </summary>
		static DatabaseModel()
		{
			DatabaseModel.Tables = new List<DatabaseTable>();
			DatabaseModel.Routines = new List<DatabaseRoutine>();
		}

		/// <summary>
		/// Gets the tables represented in the database.
		/// </summary>
		/// <value>
		/// The tables.
		/// </value>
		public static List<DatabaseTable> Tables
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
		public static List<DatabaseRoutine> Routines
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
		public static IEnumerable<DatabaseColumn> AllColumns
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
		public static IEnumerable<DatabaseConstraint> AllConstraints
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
		public static IEnumerable<DatabaseRoutineParameter> AllParameters
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
			DatabaseTable.LoadTables(connectionString);
			DatabaseRoutine.LoadRoutines(connectionString);
		}
	}
}
