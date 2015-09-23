namespace DatabaseObjects
{
	using System;
	using System.Globalization;

	/// <summary>
	/// Represents a data type from the database
	/// </summary>
	public class SqlType
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SqlType"/> class.
		/// </summary>
		/// <param name="dataType">Type of the data.</param>
		/// <param name="characterMaximumLength">Maximum length of the character.</param>
		/// <param name="characterSetName">Name of the character set.</param>
		/// <param name="collationName">Name of the collation.</param>
		/// <param name="numericPrecision">The numeric precision.</param>
		/// <param name="numericPrecisionRadix">The numeric precision radix.</param>
		/// <param name="numericScale">The numeric scale.</param>
		/// <param name="dateTimePrecision">The date time precision.</param>
		public SqlType(string dataType, int? characterMaximumLength, string characterSetName, string collationName, int? numericPrecision, int? numericPrecisionRadix, int? numericScale, int? dateTimePrecision)
		{
			this.DataType = dataType;
			this.CharacterMaximumLength = characterMaximumLength;
			this.CharacterSetName = characterSetName;
			this.CollationName = collationName;
			this.NumericPrecision = numericPrecision;
			this.NumericPrecisionRadix = numericPrecisionRadix;
			this.NumericScale = numericScale;
			this.DateTimePrecision = dateTimePrecision;
		}

		/// <summary>
		/// Gets the type of the data.
		/// </summary>
		/// <value>
		/// The type of the data.
		/// </value>
		public string DataType
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the maximum number of characters that can be stored (regardless of char width)
		/// </summary>
		/// <value>
		/// The maximum length of the character.
		/// </value>
		public int? CharacterMaximumLength
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the name of the character set.
		/// </summary>
		/// <value>
		/// The name of the character set.
		/// </value>
		public string CharacterSetName
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the name of the collation.
		/// </summary>
		/// <value>
		/// The name of the collation.
		/// </value>
		public string CollationName
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the numeric precision.
		/// </summary>
		/// <value>
		/// The numeric precision.
		/// </value>
		public int? NumericPrecision
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the numeric precision radix (either 5 or 10).
		/// 5 precision with 10 radix means the number can represent 5 decimal digits
		/// 10 precision with 2 radix means the number can represent 10 bits
		/// </summary>
		/// <value>
		/// The numeric precision radix.
		/// </value>
		public int? NumericPrecisionRadix
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the numeric scale.
		/// </summary>
		/// <value>
		/// The numeric scale.
		/// </value>
		public int? NumericScale
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the date time precision.
		/// </summary>
		/// <value>
		/// The date time precision.
		/// </value>
		public int? DateTimePrecision
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a value indicating whether this type is an integer type.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is integer; otherwise, <c>false</c>.
		/// </value>
		public bool IsInteger
		{
			get
			{
				switch (this.DataType.ToUpperInvariant())
				{
					case "BIGINT":
					case "INT":
					case "SMALLINT":
					case "TINYINT":
						return true;
					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this type is a text type.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is text; otherwise, <c>false</c>.
		/// </value>
		public bool IsText
		{
			get
			{
				switch (this.DataType.ToUpperInvariant())
				{
					case "CHAR":
					case "NCHAR":
					case "VARCHAR":
					case "NVARCHAR":
					case "TEXT":
					case "NTEXT":
						return true;
					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Gets a model system type from the column's data type.
		/// </summary>
		/// <returns>A model system type.</returns>
		public System.Type SystemType
		{
			get
			{
			// TODO full list available here: https://msdn.microsoft.com/en-us/library/cc716729%28v=vs.110%29.aspx
			// TODO Xml not implemented
			switch (this.DataType.ToUpperInvariant())
			{
				case "BIT":
					return typeof(bool);

				case "BIGINT":
					return typeof(long);

				case "INT":
					return typeof(int);

				case "SMALLINT":
					return typeof(short);

				case "TINYINT":
					return typeof(byte);

				case "CHAR":
				case "NCHAR":
				case "VARCHAR":
				case "NVARCHAR":
				case "TEXT":
				case "NTEXT":
					return typeof(string);

				case "DATETIME":
				case "DATETIME2":
				case "SMALLDATETIME":
					return typeof(DateTime);

				case "DATETIMEOFFSET":
					return typeof(DateTimeOffset);

				case "DECIMAL":
				case "MONEY":
				case "NUMERIC":
				case "SMALLMONEY":
					return typeof(decimal);

				case "FLOAT":
					return typeof(double);

				case "REAL":
					return typeof(float);

				// TODO filestream ?
				case "BINARY":
				case "IMAGE":
				case "ROWVERSION":
				case "TIMESTAMP":
				case "VARBINARY":
					return typeof(byte[]);

				case "SQL_VARIANT":
					return typeof(object);

				case "TIME":
					return typeof(TimeSpan);

				case "UNIQUEIDENTIFIER":
					return typeof(Guid);

				default:
					return typeof(object);
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
			// Add the text length to string data types
			switch (this.DataType.ToUpperInvariant())
			{
				case "CHAR":
				case "NCHAR":
				case "VARCHAR":
				case "NVARCHAR":
				case "TEXT":
				case "NTEXT":
					return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", this.DataType, this.CharacterMaximumLength);

				default:
					return this.DataType;
			}
		}
	}
}
