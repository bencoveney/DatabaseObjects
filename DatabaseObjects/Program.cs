namespace DatabaseObjects
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Launches the program
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// The connection string
		/// </summary>
		private const string ConnectionString = @"Data Source=BENSDESKTOP\SQLEXPRESS;Initial Catalog=ItemsDB;Integrated Security=True";

		/// <summary>
		/// Mains the specified arguments.
		/// </summary>
		public static void Main()
		{
			Model.LoadFromDatabase(ConnectionString);

			Console.ReadLine();
		}
	}
}
