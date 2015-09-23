namespace DatabaseObjects
{
	using System;
	using System.Data.SqlClient;
	using System.Globalization;

	/// <summary>
	/// Extension methods for the SQL Data Reader class
	/// </summary>
	public static class SqlDataReaderExtensions
	{
		/// <summary>
		/// Gets a (potentially null) string safely.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="columnName">Name of the column.</param>
		/// <returns>The value in the specified column.</returns>
		public static string GetNullableString(this SqlDataReader reader, string columnName)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader", "reader cannot be null");
			}

			return reader.GetNullableString(reader.GetOrdinal(columnName));
		}

		/// <summary>
		/// Gets a (potentially null) string safely.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="columnOrdinal">The column ordinal.</param>
		/// <returns>The value in the specified column.</returns>
		public static string GetNullableString(this SqlDataReader reader, int columnOrdinal)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader", "reader cannot be null");
			}

			return reader.IsDBNull(columnOrdinal) ? null : reader.GetString(columnOrdinal);
		}

		/// <summary>
		/// Gets a potentially null value safely.
		/// </summary>
		/// <typeparam name="T">The type of struct to get.</typeparam>
		/// <param name="reader">The reader.</param>
		/// <param name="columnName">Name of the column.</param>
		/// <returns>The value in the specified column.</returns>
		public static T? GetNullable<T>(this SqlDataReader reader, string columnName) where T : struct
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader", "reader cannot be null");
			}

			return reader.GetNullable<T>(reader.GetOrdinal(columnName));
		}

		/// <summary>
		/// Gets a potentially null value safely.
		/// </summary>
		/// <typeparam name="T">The type of struct to get.</typeparam>
		/// <param name="reader">The reader.</param>
		/// <param name="columnOrdinal">The column ordinal.</param>
		/// <returns>The value in the specified column.</returns>
		public static T? GetNullable<T>(this SqlDataReader reader, int columnOrdinal) where T : struct
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader", "reader cannot be null");
			}

			return reader.IsDBNull(columnOrdinal) ? (T?)null : (T?)Convert.ChangeType(reader.GetValue(columnOrdinal), typeof(T), CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Gets a value that indicates whether the column contains non-existent or missing values.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="columnName">Name of the column.</param>
		/// <returns>A value indicating whether the column is null</returns>
		public static bool IsDBNull(this SqlDataReader reader, string columnName)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader", "reader cannot be null");
			}

			return reader.IsDBNull(reader.GetOrdinal(columnName));
		}
	}
}
