namespace DatabaseObjects
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Launches the program
	/// </summary>
	public class Program
	{
		/// <summary>
		/// The connection string
		/// </summary>
		private const string ConnectionString = @"Data Source=BENSDESKTOP\SQLEXPRESS;Initial Catalog=ItemsDB;Integrated Security=True";

		/// <summary>
		/// Gets the tables.
		/// </summary>
		/// <value>
		/// The tables.
		/// </value>
		public static IEnumerable<Table> Tables { get; private set; }

		/// <summary>
		/// Mains the specified arguments.
		/// </summary>
		/// <param name="args">The arguments.</param>
		public static void Main(string[] args)
		{
			Model.LoadFromDatabase(ConnectionString);

			Console.ReadLine();
		}
	}
}
