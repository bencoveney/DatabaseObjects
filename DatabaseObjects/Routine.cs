namespace DatabaseObjects
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Globalization;

    /// <summary>
    /// Defines the type of a routine
    /// </summary>
    public enum RoutineType
    {
        /// <summary>
        /// Not a known routine type
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A stored procedure
        /// </summary>
        Procedure = 1,

        /// <summary>
        /// A function
        /// </summary>
        Function = 2
    }

    /// <summary>
    /// A stored procedure in the database
    /// </summary>
    public class Routine
    {
        /// <summary>
        /// The parameters
        /// </summary>
        private Collection<RoutineParameter> parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="Routine"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="schema">The schema.</param>
        /// <param name="name">The name.</param>
        /// <param name="routineType">Type of the routine.</param>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="definition">The definition.</param>
        /// <param name="created">The created.</param>
        /// <param name="lastAltered">The last altered.</param>
        public Routine(string catalog, string schema, string name, RoutineType routineType, SqlType returnType, string definition, DateTime created, DateTime lastAltered)
        {
            this.Catalog = catalog;
            this.Schema = schema;
            this.Name = name;
            this.RoutineType = routineType;
            this.ReturnType = returnType;
            this.Definition = definition;
            this.Created = created;
            this.LastAltered = lastAltered;

            this.parameters = new Collection<RoutineParameter>();
        }

        /// <summary>
        /// Gets the catalog this routine is in.
        /// </summary>
        /// <value>
        /// The catalog.
        /// </value>
        public string Catalog
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the schema this routine is in.
        /// </summary>
        /// <value>
        /// The schema.
        /// </value>
        public string Schema
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of this routine.
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
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public Collection<RoutineParameter> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        /// <summary>
        /// Gets the type of routine (procedure or function).
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public RoutineType RoutineType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the SQL type of the value returned by this routine.
        /// </summary>
        /// <value>
        /// The type of the return.
        /// </value>
        public SqlType ReturnType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the first 4000 characters of the routine's definition text.
        /// </summary>
        /// <value>
        /// The routine definition.
        /// </value>
        public string Definition
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the date this routine was created.
        /// </summary>
        /// <value>
        /// The created.
        /// </value>
        public DateTime Created
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the date this routine was last altered.
        /// </summary>
        /// <value>
        /// The last altered.
        /// </value>
        public DateTime LastAltered
        {
            get;
            private set;
        }

        /// <summary>
        /// Loads the routines from the database.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <returns>
        /// The loaded routines.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">dataProvider;dataProvider cannot be null</exception>
        public static Collection<Routine> LoadFromDatabase(IObjectDataProvider dataProvider)
        {
            Collection<Routine> result = new Collection<Routine>();

            if (dataProvider == null)
            {
                throw new ArgumentNullException("dataProvider", "dataProvider cannot be null");
            }

            using (IDataReader reader = dataProvider.LoadRoutineData())
            {
                while (reader.Read())
                {
                    // Read the result data for the routine
                    string catalog = (string)reader["SPECIFIC_CATALOG"];
                    string schema = (string)reader["SPECIFIC_SCHEMA"];
                    string name = (string)reader["SPECIFIC_NAME"];
                    string routineTypeAsText = (string)reader["ROUTINE_TYPE"];
                    string definition = (string)reader["ROUTINE_DEFINITION"];
                    DateTime created = (DateTime)reader["CREATED"];
                    DateTime lastAltered = (DateTime)reader["LAST_ALTERED"];

                    // Build the proper data structure for routine type
                    RoutineType routineType = (RoutineType)Enum.Parse(typeof(RoutineType), routineTypeAsText, true);

                    // Build the proper data structure for return type
                    SqlType returnType = null;
                    if (!reader.IsDBNull("DATA_TYPE"))
                    {
                        // Read the result data for the routine's return type
                        string dataType = (string)reader["DATA_TYPE"];
                        int? characterMaximumLength = reader.GetNullable<int>("CHARACTER_MAXIMUM_LENGTH");
                        int? numericPrecision = reader.GetNullable<int>("NUMERIC_PRECISION");
                        int? numericPrecisionRadix = reader.GetNullable<int>("NUMERIC_PRECISION_RADIX");
                        int? numericScale = reader.GetNullable<int>("NUMERIC_SCALE");
                        int? dateTimePrecision = reader.GetNullable<int>("DATETIME_PRECISION");
                        string characterSetName = reader.GetNullableString("CHARACTER_SET_NAME");
                        string collationName = reader.GetNullableString("COLLATION_NAME");

                        returnType = new SqlType(dataType, characterMaximumLength, characterSetName, collationName, numericPrecision, numericPrecisionRadix, numericScale, dateTimePrecision);
                    }

                    // Build the new routine
                    Routine routine = new Routine(catalog, schema, name, routineType, returnType, definition, created, lastAltered);

                    result.Add(routine);
                }
            }

            // Populate parameters
            foreach (Routine routine in result)
            {
                RoutineParameter.PopulateParameters(routine, dataProvider);
            }

            return result;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.ReturnType != null)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2} ({3})", this.Catalog, this.Schema, this.Name, this.ReturnType);
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2} (NULL)", this.Catalog, this.Schema, this.Name);
            }
        }
    }
}
