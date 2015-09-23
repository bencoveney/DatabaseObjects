namespace DatabaseObjects
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	/// <summary>
	/// Singleton class to represent an entire database and it's objects.
	/// </summary>
	public class Model
	{
		/// <summary>
		/// The tables
		/// </summary>
		private Collection<Table> tables;

		/// <summary>
		/// The routines
		/// </summary>
		private Collection<Routine> routines;

		/// <summary>
		/// Initializes a new instance of the <see cref="Model" /> class.
		/// </summary>
		/// <param name="tables">The tables collection.</param>
		/// <param name="routines">The routines collection.</param>
		public Model(Collection<Table> tables, Collection<Routine> routines)
		{
			this.tables = tables;
			this.routines = routines;
		}

		/// <summary>
		/// Gets the tables represented in the database.
		/// </summary>
		/// <value>
		/// The tables.
		/// </value>
		public Collection<Table> Tables
		{
			get
			{
				return this.tables;
			}
		}

		/// <summary>
		/// Gets the routines represented in the database.
		/// </summary>
		/// <value>
		/// The routines.
		/// </value>
		public Collection<Routine> Routines
		{
			get
			{
				return this.routines;
			}
		}

		/// <summary>
		/// Gets all columns for all tables in the database.
		/// </summary>
		/// <value>
		/// The columns.
		/// </value>
		public IEnumerable<Column> AllColumns
		{
			get
			{
				return this.Tables.SelectMany(table => table.Columns);
			}
		}

		/// <summary>
		/// Gets all constraints for all tables in the database.
		/// </summary>
		/// <value>
		/// The constraints.
		/// </value>
		public IEnumerable<Constraint> AllConstraints
		{
			get
			{
				return this.Tables.SelectMany(table => table.Constraints);
			}
		}

		/// <summary>
		/// Gets all parameters for all routines in the database.
		/// </summary>
		/// <value>
		/// All parameters.
		/// </value>
		public IEnumerable<RoutineParameter> AllParameters
		{
			get
			{
				return this.Routines.SelectMany(routine => routine.Parameters);
			}
		}

		/// <summary>
		/// Loads the objects from the database.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		/// <returns>The model.</returns>
		public static Model LoadFromDatabase(string connectionString)
		{
			return new Model(Table.LoadFromDatabase(connectionString), Routine.LoadFromDatabase(connectionString));
		}
	}
}
