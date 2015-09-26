﻿namespace DatabaseObjects
{
	using System;
	using System.Data.SqlClient;
	using System.Globalization;

	/// <summary>
	/// The mode or direction data passes through this parameter in
	/// </summary>
	public enum ParameterMode
	{
		/// <summary>
		/// Not a known routine type
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// The parameter passes data in to the procedure
		/// </summary>
		In = 1,

		/// <summary>
		/// The parameter passes data out of the procedure
		/// </summary>
		Out = 2,

		/// <summary>
		/// The parameter passes data in to and out of the procedure
		/// </summary>
		InOut = 3
	}

	/// <summary>
	/// A parameter for a stored procedure in the database
	/// </summary>
	public class RoutineParameter
	{
		/// <summary>
		/// Gets the date this routine was created.
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
		/// Initializes a new instance of the <see cref="RoutineParameter" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="ordinalPosition">The ordinal position.</param>
		/// <param name="mode">The mode.</param>
		/// <param name="type">The type.</param>
		/// <exception cref="System.ArgumentNullException">routine;routine cannot be null</exception>
		public RoutineParameter(string name, int ordinalPosition, ParameterMode mode, SqlType type)
		{
			this.Name = name;
			this.OrdinalPosition = ordinalPosition;
			this.Mode = mode;
			this.DataType = type;
		}

		/// <summary>
		/// Gets the name of the parameter.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the ordinal position or the parameter for the routine.
		/// </summary>
		/// <value>
		/// The ordinal position.
		/// </value>
		public int OrdinalPosition
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the mode the parameter uses to pass data through.
		/// </summary>
		/// <value>
		/// The mode.
		/// </value>
		public ParameterMode Mode
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the type of data the parameter takes.
		/// </summary>
		/// <value>
		/// The type.
		/// </value>
		public SqlType DataType
		{
			get;
			private set;
		}

		/// <summary>
		/// Loads the parameters from the database for a specific routine
		/// </summary>
		/// <param name="routine">The routine.</param>
		/// <param name="connection">The connection.</param>
		public static void PopulateParameters(Routine routine, SqlConnection connection)
		{
			if (routine == null)
			{
				throw new ArgumentNullException("routine", "routine cannot be null");
			}

			// Query the database for the column data
			using (SqlCommand command = new SqlCommand(RoutineParametersQuery, connection))
			{
				command.Parameters.AddWithValue("RoutineCatalog", routine.Catalog);
				command.Parameters.AddWithValue("RoutineSchema", routine.Schema);
				command.Parameters.AddWithValue("RoutineName", routine.Name);

				using (SqlDataReader result = command.ExecuteReader())
				{
					while (result.Read())
					{
						// Read the result data for the routine parameter
						string name = (string)result["PARAMETER_NAME"];
						int ordinalPosition = (int)result["ORDINAL_POSITION"];
						string modeAsText = (string)result["PARAMETER_MODE"];

						// Build the proper data structure for parameter mode
						ParameterMode mode = (ParameterMode)Enum.Parse(typeof(ParameterMode), modeAsText, true);

						// Read the result data for the routine's return type
						string dataType = (string)result["DATA_TYPE"];
						int? characterMaximumLength = result.GetNullable<int>("CHARACTER_MAXIMUM_LENGTH");
						int? numericPrecision = result.GetNullable<int>("NUMERIC_PRECISION");
						int? numericPrecisionRadix = result.GetNullable<int>("NUMERIC_PRECISION_RADIX");
						int? numericScale = result.GetNullable<int>("NUMERIC_SCALE");
						int? dateTimePrecision = result.GetNullable<int>("DATETIME_PRECISION");
						string characterSetName = result.GetNullableString("CHARACTER_SET_NAME");
						string collationName = result.GetNullableString("COLLATION_NAME");

						// Build the proper data structure for return type
						SqlType returnType = new SqlType(dataType, characterMaximumLength, characterSetName, collationName, numericPrecision, numericPrecisionRadix, numericScale, dateTimePrecision);

						// Remove the @ from the front of the name
						if (name.IndexOf("@", StringComparison.Ordinal) == 0)
						{
							name = name.Substring(1);
						}

						routine.Parameters.Add(new RoutineParameter(name, ordinalPosition, mode, returnType));
					}
				}
			}
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", this.Name, this.DataType);
		}
	}
}
