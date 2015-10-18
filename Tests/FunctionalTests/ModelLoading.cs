namespace FunctionalTests
{
	using System;
	using System.Linq;
	using DatabaseObjects;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Tests the functionality for loading a model from a database.
	/// </summary>
	[TestClass]
	public class ModelLoading
	{
		/// <summary>
		/// Tests that a table is loaded correctly from the database
		/// and that all columns and constraints are correctly loaded.
		/// </summary>
		[TestMethod]
		public void TableIsLoadedFromDatabase()
		{
			Table foodstuffTable = AssemblyUtilities.DatabaseModel.Tables.Single(
				table => string.Equals(table.Name, "Foodstuff", StringComparison.InvariantCulture));

			Assert.IsNotNull(foodstuffTable);
			Assert.AreEqual(foodstuffTable.Catalog, "ItemsDB");
			Assert.AreEqual(foodstuffTable.Schema, "dbo");
		}

		/// <summary>
		/// Tests that a table is loaded correctly from the database
		/// and that all columns and constraints are correctly loaded.
		/// </summary>
		[TestMethod]
		public void TableColumnsAreLoadedFromDatabase()
		{
			Table foodstuffTable = AssemblyUtilities.DatabaseModel.Tables.Single(
				table => string.Equals(table.Name, "Foodstuff", StringComparison.InvariantCulture));

			Column foodstuffId = foodstuffTable.Columns.Single(
				column => string.Equals(column.Name, "FoodstuffID", StringComparison.InvariantCulture));

			Assert.IsNotNull(foodstuffId);

			Constraint foreignKeyFoodstuffPersonID = foodstuffTable.Constraints.Single(
				constraint => string.Equals(constraint.Name, "FK_Foodstuff_PersonID", StringComparison.InvariantCulture));

			Assert.IsNotNull(foreignKeyFoodstuffPersonID);
		}

		/// <summary>
		/// Tests that a routine is loaded correctly from the database
		/// and that all parameters are correctly loaded.
		/// </summary>
		[TestMethod]
		public void RoutineIsLoadedFromDatabase()
		{
			Routine foodstuffDelete = AssemblyUtilities.DatabaseModel.Routines.Single(
				routine => string.Equals(routine.Name, "FoodstuffDelete", StringComparison.InvariantCulture));

			RoutineParameter foodstuffId = foodstuffDelete.Parameters.Single(
				parameter => string.Equals(parameter.Name, "FoodstuffID", StringComparison.InvariantCulture));

			Assert.IsNotNull(foodstuffId);
		}
	}
}
