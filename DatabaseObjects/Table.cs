namespace DatabaseObjects
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data.SqlClient;
	using System.Globalization;
	using System.Linq;

	/// <summary>
	/// A table in the database
	/// TODO enforce constraints and columns not being assigned multiple times
	/// </summary>
	public class Table
	{
		/// <summary>
		/// The columns
		/// </summary>
		private Collection<Column> columns;

		/// <summary>
		/// The constraints
		/// </summary>
		private Collection<Constraint> constraints;

		/// <summary>
		/// Initializes a new instance of the <see cref="Table"/> class.
		/// </summary>
		/// <param name="catalog">The catalog.</param>
		/// <param name="schema">The schema.</param>
		/// <param name="name">The name.</param>
		/// <exception cref="System.ArgumentNullException">
		/// catalog
		/// or
		/// schema
		/// or
		/// name
		/// </exception>
		public Table(string catalog, string schema, string name)
		{
			if (string.IsNullOrEmpty(catalog))
			{
				throw new ArgumentNullException("catalog");
			}

			if (string.IsNullOrEmpty(schema))
			{
				throw new ArgumentNullException("schema");
			}

			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			this.Catalog = catalog;
			this.Schema = schema;
			this.Name = name;

			this.columns = new Collection<Column>();
			this.constraints = new Collection<Constraint>();
		}

		/// <summary>
		/// Gets the catalog the table is in.
		/// </summary>
		/// <value>
		/// The catalog.
		/// </value>
		public string Catalog { get; private set; }

		/// <summary>
		/// Gets the schema the table is in.
		/// </summary>
		/// <value>
		/// The schema.
		/// </value>
		public string Schema { get; private set; }

		/// <summary>
		/// Gets the name of the table.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the columns the table contains.
		/// TODO maybe an array would be better as this would enforce the column order more strictly than a collection
		/// </summary>
		/// <value>
		/// The columns.
		/// </value>
		public Collection<Column> Columns
		{
			get
			{
				return this.columns;
			}
		}

		/// <summary>
		/// Gets the constraints.
		/// </summary>
		/// <value>
		/// The constraints.
		/// </value>
		public Collection<Constraint> Constraints
		{
			get
			{
				return this.constraints;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the table is an item.
		/// </summary>
		public bool RepresentsItem
		{
			get
			{
				return !this.RepresentsRelationship && !this.RepresentsCategory;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the table is a relationship.
		/// </summary>
		public bool RepresentsRelationship
		{
			get
			{
				return this.Name.Contains("Collection");
			}
		}

		/// <summary>
		/// Gets a value indicating whether the table is a category.
		/// </summary>
		public bool RepresentsCategory
		{
			get
			{
				return this.Name.Contains("Category");
			}
		}

		/// <summary>
		/// Loads the tables from the database.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		/// <returns>The loaded collections.</returns>
		public static Collection<Table> LoadFromDatabase(string connectionString)
		{
			const string TablesQuery = @"
SELECT
	SchemaTables.Table_Catalog,
	SchemaTables.Table_Schema,
	SchemaTables.Table_Name
FROM
	Information_Schema.Tables AS SchemaTables
WHERE
	SchemaTables.Table_Type = 'BASE TABLE'
	AND SchemaTables.Table_Name != 'sysdiagrams'
	AND SchemaTables.Table_Name != '__RefactorLog'";

			Collection<Table> result = new Collection<Table>();

			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();

				// Query the database for the table data
				using (SqlCommand command = new SqlCommand(TablesQuery, connection))
				{
					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							// Read the result data
							string tableCatalog = reader.GetString(0);
							string tableSchema = reader.GetString(1);
							string tableName = reader.GetString(2);

							// Build the new table
							Table table = new Table(tableCatalog, tableSchema, tableName);

							result.Add(table);
						}
					}
				}

				// Populate additional schema objects
				foreach (Table table in result)
				{
					Column.PopulateColumns(table, connection);
					Constraint.PopulateUniqueConstraints(table, connection);
				}

				Constraint.PopulateReferentialConstraints(result, connection);
			}

			return result;
		}

		/// <summary>
		/// Gets the column with the specified name.
		/// </summary>
		/// <param name="columnName">Name of the column.</param>
		/// <returns>The column.</returns>
		public Column GetColumn(string columnName)
		{
			return this.Columns.Single(column => column.Name == columnName);
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", this.Catalog, this.Schema, this.Name);
		}
	}
}
