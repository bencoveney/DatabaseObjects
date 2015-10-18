namespace FunctionalTests
{
	using DatabaseObjects;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Provides common utilities for the assembly
	/// </summary>
	[TestClass]
	public class AssemblyUtilities
	{
		/// <summary>
		/// The connection string of the database where the TestDatabase project has been deployed.
		/// </summary>
		private const string ConnectionString = @"Data Source=BENSDESKTOP\SQLEXPRESS;Initial Catalog=ItemsDB;Integrated Security=True";

		/// <summary>
		/// Gets the shared database model.
		/// After initialization this model should not be altered manually.
		/// </summary>
		public static Model DatabaseModel { get; private set; }

		/// <summary>
		/// Called when the test assembly is initialized.
		/// Populates the shared Model object.
		/// </summary>
		/// <param name="testContext">The test context.</param>
		[AssemblyInitialize]
		public static void Initialise(TestContext testContext)
		{
			DatabaseModel = Model.LoadFromDatabase(ConnectionString);
		}
	}
}
