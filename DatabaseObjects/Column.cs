namespace DatabaseObjects
{
	using System;
	using System.Collections.Generic;
	using System.Data.SqlClient;
	using System.Globalization;
	using System.Linq;

	/// <summary>
	/// Represents a table's column in the database
	/// </summary>
	public class Column
	{
		/// <summary>
		/// Query to find the columns for all tables in the database
		/// </summary>
		private const string ColumnsQuery = @"
SELECT
	DatabaseColumns.COLUMN_NAME,
	DatabaseColumns.ORDINAL_POSITION,
	DatabaseColumns.COLUMN_DEFAULT,
	DatabaseColumns.IS_NULLABLE,
	DatabaseColumns.DATA_TYPE,
	CAST(DatabaseColumns.CHARACTER_MAXIMUM_LENGTH AS INT) AS CHARACTER_MAXIMUM_LENGTH,
	CAST(DatabaseColumns.NUMERIC_PRECISION AS INT) AS NUMERIC_PRECISION,
	CAST(DatabaseColumns.NUMERIC_PRECISION_RADIX AS INT) AS NUMERIC_PRECISION_RADIX,
	CAST(DatabaseColumns.NUMERIC_SCALE AS INT) AS NUMERIC_SCALE,
	CAST(DatabaseColumns.DATETIME_PRECISION AS INT) AS DATETIME_PRECISION,
	DatabaseColumns.CHARACTER_SET_NAME,
	DatabaseColumns.COLLATION_NAME
FROM
	INFORMATION_SCHEMA.COLUMNS As DatabaseColumns
WHERE
	DatabaseColumns.TABLE_CATALOG = @TableCatalog
	AND DatabaseColumns.TABLE_SCHEMA = @TableSchema
	AND DatabaseColumns.TABLE_NAME = @TableName
ORDER BY
	TABLE_NAME,
	ORDINAL_POSITION";

		/// <summary>
		/// Initializes a new instance of the <see cref="Column" /> class.
		/// </summary>
		/// <param name="name">Name of the column.</param>
		/// <param name="type">The type.</param>
		/// <param name="ordinalPosition">The ordinal position.</param>
		/// <param name="columnDefault">The column default.</param>
		/// <param name="isNullable">if set to <c>true</c> the column can be set to null.</param>
		public Column(string name, SqlType type, int ordinalPosition, string columnDefault, bool isNullable)
		{
			// Populate member variables
			// A lot of the varibales here are duplicated from the tables class. maybe these columns should just be properties of the tables?
			this.Name = name;
			this.DataType = type;
			this.OrdinalPosition = ordinalPosition;
			this.ColumnDefault = columnDefault;
			this.IsNullable = isNullable;
		}

		/// <summary>
		/// Gets the name of the column.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the SQL type of the data stored in this column.
		/// </summary>
		/// <value>
		/// The type.
		/// </value>
		public SqlType DataType
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the ordinal position.
		/// </summary>
		/// <value>
		/// The ordinal position.
		/// </value>
		public int OrdinalPosition { get; private set; }

		/// <summary>
		/// Gets the column default.
		/// </summary>
		/// <value>
		/// The column default.
		/// </value>
		public string ColumnDefault { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this instance can be set to null.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance can be set to null; otherwise, <c>false</c>.
		/// </value>
		public bool IsNullable { get; private set; }

		/// <summary>
		/// Loads the columns from the database for a specific table.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <param name="connection">The connection.</param>
		public static void PopulateColumns(Table table, SqlConnection connection)
		{
			if (table == null)
			{
				throw new ArgumentNullException("table", "table cannot be null");
			}

			// Query the database for the column data
			using (SqlCommand command = new SqlCommand(ColumnsQuery, connection))
			{
				command.Parameters.AddWithValue("TableCatalog", table.Catalog);
				command.Parameters.AddWithValue("TableSchema", table.Schema);
				command.Parameters.AddWithValue("TableName", table.Name);

				using (SqlDataReader result = command.ExecuteReader())
				{
					while (result.Read())
					{
						// Read the result data
						string name = (string)result["COLUMN_NAME"];
						int ordinalPosition = (int)result["ORDINAL_POSITION"];
						string columnDefault = result.GetNullableString("COLUMN_DEFAULT");
						bool isNullable = ((string)result["IS_NULLABLE"]).Equals("NO") ? false : true;

						// Read the result data for the routine's return type
						string dataType = (string)result["DATA_TYPE"];
						int? characterMaximumLength = result.GetNullable<int>("CHARACTER_MAXIMUM_LENGTH");
						int? numericPrecision = result.GetNullable<int>("NUMERIC_PRECISION");
						int? numericPrecisionRadix = result.GetNullable<int>("NUMERIC_PRECISION_RADIX");
						int? numericScale = result.GetNullable<int>("NUMERIC_SCALE");
						int? dateTimePrecision = result.GetNullable<int>("DATETIME_PRECISION");
						string characterSetName = result.GetNullableString("CHARACTER_SET_NAME");
						string collationName = result.GetNullableString("COLLATION_NAME");

						// Build the proper data structure for return type
						SqlType type = new SqlType(dataType, characterMaximumLength, characterSetName, collationName, numericPrecision, numericPrecisionRadix, numericScale, dateTimePrecision);

						// Build the new column
						table.Columns.Add(new Column(name, type, ordinalPosition, columnDefault, isNullable));
					}
				}
			}
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", this.Name, this.DataType);
		}
	}
}