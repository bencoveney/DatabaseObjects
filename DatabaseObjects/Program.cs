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
			Model model = Model.LoadFromDatabase(ConnectionString);

			foreach (Table table in model.Tables)
			{
				Console.WriteLine(table.Name);

				foreach (Column column in table.Columns)
				{
					Console.WriteLine(column.Name);

					Console.WriteLine(column.DataType.DataType);
				}

				foreach (Constraint constraint in table.Constraints)
				{
					Console.WriteLine(constraint.Name);

					Console.WriteLine(constraint.ConstraintType);
				}
			}

			foreach (Routine routine in model.Routines)
			{
				Console.WriteLine(routine.Name);

				Console.WriteLine(routine.RoutineType);

				foreach (RoutineParameter parameter in routine.Parameters)
				{
					Console.WriteLine(parameter.Name);

					Console.WriteLine(parameter.Mode);

					Console.WriteLine(parameter.DataType.DataType);
				}
			}

			Console.ReadLine();
		}
	}
}
