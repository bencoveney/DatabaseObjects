using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DatabaseObjects;
using System.Linq;

namespace FunctionalTests
{
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
			Table FoodstuffTable = AssemblyUtilities.DatabaseModel.Tables.Single(
				table => string.Equals(table.Name, "Foodstuff", StringComparison.InvariantCulture)
			);

			Assert.IsNotNull(FoodstuffTable);
			Assert.AreEqual(FoodstuffTable.Catalog, "ItemsDB");
			Assert.AreEqual(FoodstuffTable.Schema, "dbo");
		}

		[TestMethod]
		public void TableColumnsAreLoadedFromDatabase()
		{
			Table FoodstuffTable = AssemblyUtilities.DatabaseModel.Tables.Single(
				table => string.Equals(table.Name, "Foodstuff", StringComparison.InvariantCulture)
			);

			Column FoodstuffId = FoodstuffTable.Columns.Single(
				column => string.Equals(column.Name, "FoodstuffID", StringComparison.InvariantCulture)
			);

			Assert.IsNotNull(FoodstuffId);

			// TODO Other column checks

			// TODO Other columns
		}

		/// <summary>
		/// Tests that a routine is loaded correctly from the database
		/// and that all parameters are correctly loaded.
		/// </summary>
		[TestMethod]
		public void RoutineIsLoadedFromDatabase()
		{
			Routine testRoutine;
		}
	}
}
