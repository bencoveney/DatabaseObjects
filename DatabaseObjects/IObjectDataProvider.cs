namespace DatabaseObjects
{
	using System.Data;

	/// <summary>
	/// Provides raw data about database objects
	/// </summary>
	public interface IObjectDataProvider
	{
		/// <summary>
		/// Load the data describing all tables.
		/// </summary>
		/// <returns>The table data.</returns>
		IDataReader LoadTableData();

		/// <summary>
		/// Load the data describing all columns for a given table.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns>
		/// The column data
		/// </returns>
		IDataReader LoadColumnDataForTable(Table table);

		/// <summary>
		/// Load the data describing all unique constraints for a given table.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns>
		/// The constraint data.
		/// </returns>
		IDataReader LoadUniqueConstraintDataForTable(Table table);

		/// <summary>
		/// Load the data describing all check constraints for a given table.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns>
		/// The constraint data.
		/// </returns>
		IDataReader LoadCheckConstraintsDataForTable(Table table);

		/// <summary>
		/// Load the data describing all referential constraints for a given table.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <returns>
		/// The constraint data.
		/// </returns>
		IDataReader LoadReferentialConstraintsDataForTable(Table table);

		/// <summary>
		/// Load the data describing all routines.
		/// </summary>
		/// <returns>The routine data.</returns>
		IDataReader LoadRoutineData();

		/// <summary>
		/// Load the data describing all parameters for a given routine.
		/// </summary>
		/// <param name="routine">The routine.</param>
		/// <returns>The parameter data</returns>
		IDataReader LoadParametersDataForRoutine(Routine routine);
	}
}
