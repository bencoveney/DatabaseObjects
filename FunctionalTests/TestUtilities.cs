using DatabaseObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalTests
{
	[TestClass()]
	public class AssemblyUtilities
	{
		/// <summary>
		/// The connection string of the database where the TestDatabase project has been deployed.
		/// </summary>
		private const string ConnectionString = @"Data Source=BENSDESKTOP\SQLEXPRESS;Initial Catalog=ItemsDB;Integrated Security=True";

		/// <summary>
		/// The database model used to test functionality.
		/// </summary>
		public static Model DatabaseModel;

		[AssemblyInitialize()]
		public static void Initialise(TestContext testContext)
		{
			DatabaseModel = Model.LoadFromDatabase(ConnectionString);
		}
	}
}
