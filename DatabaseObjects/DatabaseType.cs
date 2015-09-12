namespace ItemLoader
{
	using System;

	/// <summary>
	/// Represents a data type from the database
	/// </summary>
	public class DatabaseType
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseType"/> class.
		/// </summary>
		/// <param name="dataType">Type of the data.</param>
		/// <param name="characterMaximumLength">Maximum length of the character.</param>
		/// <param name="characterSetName">Name of the character set.</param>
		/// <param name="collationName">Name of the collation.</param>
		/// <param name="numericPrecision">The numeric precision.</param>
		/// <param name="numericPrecisionRadix">The numeric precision radix.</param>
		/// <param name="numericScale">The numeric scale.</param>
		/// <param name="dateTimePrecision">The date time precision.</param>
		public DatabaseType(string dataType, int? characterMaximumLength, string characterSetName, string collationName, int? numericPrecision, int? numericPrecisionRadix, int? numericScale, int? dateTimePrecision)
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
				switch (this.DataType.ToLower())
				{
					case "bigint":
					case "int":
					case "smallint":
					case "tinyint":
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
				switch (this.DataType.ToLower())
				{
					case "char":
					case "nchar":
					case "varchar":
					case "nvarchar":
					case "text":
					case "ntext":
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
		public Type GetSystemType()
		{
			// TODO full list available here: https://msdn.microsoft.com/en-us/library/cc716729%28v=vs.110%29.aspx
			// TODO Xml not implemented
			switch (this.DataType.ToLower())
			{
				case "bit":
					return typeof(bool);

				case "bigint":
					return typeof(long);

				case "int":
					return typeof(int);

				case "smallint":
					return typeof(short);

				case "tinyint":
					return typeof(byte);

				case "char":
				case "nchar":
				case "varchar":
				case "nvarchar":
				case "text":
				case "ntext":
					return typeof(string);

				case "datetime":
				case "datetime2":
				case "smalldatetime":
					return typeof(DateTime);

				case "datetimeoffset":
					return typeof(DateTimeOffset);

				case "decimal":
				case "money":
				case "numeric":
				case "smallmoney":
					return typeof(decimal);

				case "float":
					return typeof(double);

				case "real":
					return typeof(float);

				// TODO filestream ?
				case "binary":
				case "image":
				case "rowversion":
				case "timestamp":
				case "varbinary":
					return typeof(byte[]);

				case "sql_variant":
					return typeof(object);

				case "time":
					return typeof(TimeSpan);

				case "uniqueidentifier":
					return typeof(Guid);

				default:
					return typeof(object);
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
			switch (this.DataType)
			{
				case "char":
				case "nchar":
				case "varchar":
				case "nvarchar":
				case "text":
				case "ntext":
					return string.Format("{0} ({1})", this.DataType, this.CharacterMaximumLength);

				default:
					return this.DataType;
			}
		}
	}
}
