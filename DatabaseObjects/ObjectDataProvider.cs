namespace DatabaseObjects
{
	using System;
	using System.Data;
	using System.Data.SqlClient;

	/// <summary>
	/// Provides object data loaded from an SQL Server database
	/// </summary>
	internal class SqlObjectDataProvider : IObjectDataProvider
	{
		/// <summary>
		/// SQL query to find table data
		/// </summary>
		private const string TablesQuery = @"
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

		/// <summary>
		/// SQL query to find table column data
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
		/// SQL query to find unique parameter data
		/// </summary>
		private const string UniqueConstraintsQuery = @"
SELECT
	DatabaseConstraints.CONSTRAINT_NAME AS ConstraintName,
	DatabaseConstraints.CONSTRAINT_TYPE AS Type,
	ConstrainedColumn.COLUMN_NAME AS ColumnName,
	DatabaseConstraints.IS_DEFERRABLE AS IsDeferrable,
	DatabaseConstraints.INITIALLY_DEFERRED AS InitiallyDeferred
FROM
	INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS DatabaseConstraints
	INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE AS ConstrainedColumn ON DatabaseConstraints.CONSTRAINT_NAME = ConstrainedColumn.CONSTRAINT_NAME
WHERE
	DatabaseConstraints.CONSTRAINT_CATALOG = @TableCatalog
	AND DatabaseConstraints.CONSTRAINT_SCHEMA = @TableSchema
	AND DatabaseConstraints.TABLE_NAME = @TableName
	AND CONSTRAINT_TYPE IN ('PRIMARY KEY', 'UNIQUE')";

		/// <summary>
		/// SQL query to find referential constraint data
		/// </summary>
		private const string ReferentialConstraintsQuery = @"
SELECT
	DatabaseConstraints.CONSTRAINT_NAME AS ConstraintName,
	ConstrainedColumn.COLUMN_NAME AS ColumnName,
	DatabaseConstraints.IS_DEFERRABLE AS IsDeferrable,
	DatabaseConstraints.INITIALLY_DEFERRED AS InitiallyDeferred,
	ReferencedConstraint.TABLE_CATALOG AS ReferencedCatalog,
	ReferencedConstraint.TABLE_SCHEMA AS ReferencedSchema,
	ReferencedConstraint.TABLE_NAME AS ReferencedTable,
	ReferencedColumn.COLUMN_NAME AS ReferencedColumn
FROM
	INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS DatabaseConstraints
	INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE AS ConstrainedColumn ON DatabaseConstraints.CONSTRAINT_NAME = ConstrainedColumn.CONSTRAINT_NAME
	INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS ConstraintReference ON DatabaseConstraints.CONSTRAINT_NAME = ConstraintReference.CONSTRAINT_NAME
	INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS ReferencedConstraint ON ConstraintReference.UNIQUE_CONSTRAINT_NAME = ReferencedConstraint.CONSTRAINT_NAME
	INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE AS ReferencedColumn ON ReferencedConstraint.CONSTRAINT_NAME = ReferencedColumn.CONSTRAINT_NAME

WHERE
	DatabaseConstraints.CONSTRAINT_CATALOG = @TableCatalog
	AND DatabaseConstraints.CONSTRAINT_SCHEMA = @TableSchema
	AND DatabaseConstraints.TABLE_NAME = @TableName
	AND DatabaseConstraints.CONSTRAINT_TYPE = 'FOREIGN KEY'";

		/// <summary>
		/// SQL query to find routine data
		/// </summary>
		private const string RoutinesQuery = @"
SELECT
	DatabaseRoutines.SPECIFIC_CATALOG,
	DatabaseRoutines.SPECIFIC_SCHEMA,
	DatabaseRoutines.SPECIFIC_NAME,
	DatabaseRoutines.ROUTINE_TYPE,
	DatabaseRoutines.DATA_TYPE,
	DatabaseRoutines.CHARACTER_MAXIMUM_LENGTH,
	DatabaseRoutines.COLLATION_NAME,
	DatabaseRoutines.CHARACTER_SET_NAME,
	DatabaseRoutines.NUMERIC_PRECISION,
	DatabaseRoutines.NUMERIC_PRECISION_RADIX,
	DatabaseRoutines.NUMERIC_SCALE,
	DatabaseRoutines.DATETIME_PRECISION,
	DatabaseRoutines.ROUTINE_DEFINITION,
	DatabaseRoutines.CREATED,
	DatabaseRoutines.LAST_ALTERED
FROM
	INFORMATION_SCHEMA.ROUTINES AS DatabaseRoutines";

		/// <summary>
		/// SQL query to find routine parameter data
		/// </summary>
		private const string RoutineParametersQuery = @"
SELECT
	RoutineParameters.PARAMETER_NAME,
	RoutineParameters.ORDINAL_POSITION,
	RoutineParameters.PARAMETER_MODE,
	RoutineParameters.DATA_TYPE,
	RoutineParameters.CHARACTER_MAXIMUM_LENGTH,
	RoutineParameters.COLLATION_NAME,
	RoutineParameters.CHARACTER_SET_NAME,
	RoutineParameters.NUMERIC_PRECISION,
	RoutineParameters.NUMERIC_PRECISION_RADIX,
	RoutineParameters.NUMERIC_SCALE,
	RoutineParameters.DATETIME_PRECISION
FROM
	INFORMATION_SCHEMA.PARAMETERS AS RoutineParameters
WHERE
	RoutineParameters.SPECIFIC_CATALOG = @RoutineCatalog
	AND RoutineParameters.SPECIFIC_SCHEMA = @RoutineSchema
	AND RoutineParameters.SPECIFIC_NAME = @RoutineName";

		/// <summary>
		/// The SQL connection used to fetch object data
		/// </summary>
		private SqlConnection sqlConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlObjectDataProvider"/> class.
		/// </summary>
		/// <param name="sqlConnection">The SQL connection.</param>
		/// <exception cref="System.ArgumentNullException">sqlConnection;sqlConnection cannot be null</exception>
		public SqlObjectDataProvider(SqlConnection sqlConnection)
		{
			if (sqlConnection == null)
			{
				throw new ArgumentNullException("sqlConnection", "sqlConnection cannot be null");
			}

			if (sqlConnection.State != ConnectionState.Open)
			{
				sqlConnection.Open();
			}

			this.sqlConnection = sqlConnection;
		}

		/// <summary>
		/// Load the data describing all tables.
		/// </summary>
		/// <returns>
		/// The table data.
		/// </returns>
		public IDataReader LoadTableData()
		{
			using (SqlCommand command = new SqlCommand(SqlObjectDataProvider.TablesQuery, this.sqlConnection))
			{
				return command.ExecuteReader();
			}
		}

		/// <summary>
		/// Load the data describing all columns for a given table.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns>
		/// The column data
		/// </returns>
		/// <exception cref="System.ArgumentNullException">table;table cannot be null</exception>
		public IDataReader LoadColumnDataForTable(Table table)
		{
			if (table == null)
			{
				throw new ArgumentNullException("table", "table cannot be null");
			}

			using (SqlCommand command = new SqlCommand(SqlObjectDataProvider.ColumnsQuery, this.sqlConnection))
			{
				command.Parameters.AddWithValue("TableCatalog", table.Catalog);
				command.Parameters.AddWithValue("TableSchema", table.Schema);
				command.Parameters.AddWithValue("TableName", table.Name);

				return command.ExecuteReader();
			}
		}

		/// <summary>
		/// Load the data describing all unique constraints for a given table.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns>
		/// The constraint data.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">table;table cannot be null</exception>
		public IDataReader LoadUniqueConstraintDataForTable(Table table)
		{
			if (table == null)
			{
				throw new ArgumentNullException("table", "table cannot be null");
			}

			using (SqlCommand command = new SqlCommand(SqlObjectDataProvider.UniqueConstraintsQuery, this.sqlConnection))
			{
				command.Parameters.AddWithValue("TableCatalog", table.Catalog);
				command.Parameters.AddWithValue("TableSchema", table.Schema);
				command.Parameters.AddWithValue("TableName", table.Name);

				return command.ExecuteReader();
			}
		}

		/// <summary>
		/// Load the data describing all check constraints for a given table.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns>
		/// The constraint data.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">table;table cannot be null</exception>
		/// <exception cref="System.NotImplementedException">Check constrains are not yet supported</exception>
		public IDataReader LoadCheckConstraintsDataForTable(Table table)
		{
			if (table == null)
			{
				throw new ArgumentNullException("table", "table cannot be null");
			}

			throw new NotImplementedException("Check constrains are not yet supported");
		}

		/// <summary>
		/// Load the data describing all referential constraints for a given table.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns>
		/// The constraint data.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">table;table cannot be null</exception>
		public IDataReader LoadReferentialConstraintsDataForTable(Table table)
		{
			if (table == null)
			{
				throw new ArgumentNullException("table", "table cannot be null");
			}

			using (SqlCommand command = new SqlCommand(SqlObjectDataProvider.ReferentialConstraintsQuery, this.sqlConnection))
			{
				command.Parameters.AddWithValue("TableCatalog", table.Catalog);
				command.Parameters.AddWithValue("TableSchema", table.Schema);
				command.Parameters.AddWithValue("TableName", table.Name);

				return command.ExecuteReader();
			}
		}

		/// <summary>
		/// Load the data describing all routines.
		/// </summary>
		/// <returns>
		/// The routine data.
		/// </returns>
		public IDataReader LoadRoutineData()
		{
			using (SqlCommand command = new SqlCommand(SqlObjectDataProvider.RoutinesQuery, this.sqlConnection))
			{
				return command.ExecuteReader();
			}
		}

		/// <summary>
		/// Load the data describing all parameters for a given routine.
		/// </summary>
		/// <param name="routine">The routine.</param>
		/// <returns>
		/// The parameter data
		/// </returns>
		/// <exception cref="System.ArgumentNullException">routine;routine cannot be null</exception>
		public IDataReader LoadParametersDataForRoutine(Routine routine)
		{
			if (routine == null)
			{
				throw new ArgumentNullException("routine", "routine cannot be null");
			}

			using (SqlCommand command = new SqlCommand(SqlObjectDataProvider.RoutineParametersQuery, this.sqlConnection))
			{
				command.Parameters.AddWithValue("RoutineCatalog", routine.Catalog);
				command.Parameters.AddWithValue("RoutineSchema", routine.Schema);
				command.Parameters.AddWithValue("RoutineName", routine.Name);

				return command.ExecuteReader();
			}
		}
	}
}
