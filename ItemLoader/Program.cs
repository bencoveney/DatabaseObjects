﻿namespace ItemLoader
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Items;
	using ItemSerialiser;

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
		public static IEnumerable<DatabaseTable> Tables { get; private set; }

		/// <summary>
		/// Mains the specified arguments.
		/// </summary>
		/// <param name="args">The arguments.</param>
		public static void Main(string[] args)
		{
			DatabaseModel.LoadFromDatabase(ConnectionString);

			Model model = DatabaseModel.ConstructModel();

			XmlCreator creator = new XmlCreator(model);
			string output = creator.TransformText();

			////Loader loader = new Loader();

			////using (SqlConnection connection = new SqlConnection(ConnectionString))
			////{
			////    loader.Load(connection);
			////}

			////XmlCreator creator = new XmlCreator(loader.Model);
			////string output = creator.TransformText();
		}
	}
}
