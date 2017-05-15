namespace DatabaseObjects
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// A table in the database
    /// TODO enforce constraints and columns not being assigned multiple times
    /// </summary>
    public class Table
    {
        /// <summary>
        /// The columns
        /// </summary>
        private Collection<Column> columns;

        /// <summary>
        /// The constraints
        /// </summary>
        private Collection<Constraint> constraints;

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="schema">The schema.</param>
        /// <param name="name">The name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// catalog
        /// or
        /// schema
        /// or
        /// name
        /// </exception>
        public Table(string catalog, string schema, string name)
        {
            if (string.IsNullOrEmpty(catalog))
            {
                throw new ArgumentNullException("catalog");
            }

            if (string.IsNullOrEmpty(schema))
            {
                throw new ArgumentNullException("schema");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            this.Catalog = catalog;
            this.Schema = schema;
            this.Name = name;

            this.columns = new Collection<Column>();
            this.constraints = new Collection<Constraint>();
        }

        /// <summary>
        /// Gets the catalog the table is in.
        /// </summary>
        /// <value>
        /// The catalog.
        /// </value>
        public string Catalog { get; private set; }

        /// <summary>
        /// Gets the schema the table is in.
        /// </summary>
        /// <value>
        /// The schema.
        /// </value>
        public string Schema { get; private set; }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the columns the table contains.
        /// TODO maybe an array would be better as this would enforce the column order more strictly than a collection
        /// </summary>
        /// <value>
        /// The columns.
        /// </value>
        public Collection<Column> Columns
        {
            get
            {
                return this.columns;
            }
        }

        /// <summary>
        /// Gets the constraints.
        /// </summary>
        /// <value>
        /// The constraints.
        /// </value>
        public Collection<Constraint> Constraints
        {
            get
            {
                return this.constraints;
            }
        }

        /// <summary>
        /// Loads the tables from the database.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <returns>
        /// The loaded collections.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">dataProvider;dataProvider cannot be null</exception>
        public static Collection<Table> LoadFromDatabase(IObjectDataProvider dataProvider)
        {
            Collection<Table> result = new Collection<Table>();

            if (dataProvider == null)
            {
                throw new ArgumentNullException("dataProvider", "dataProvider cannot be null");
            }

            using (IDataReader reader = dataProvider.LoadTableData())
            {
                while (reader.Read())
                {
                    // Read the result data
                    string tableCatalog = reader.GetString(0);
                    string tableSchema = reader.GetString(1);
                    string tableName = reader.GetString(2);

                    // Build the new table
                    Table table = new Table(tableCatalog, tableSchema, tableName);

                    result.Add(table);
                }
            }

            // Populate additional schema objects
            foreach (Table table in result)
            {
                Column.PopulateColumns(table, dataProvider);
                Constraint.PopulateUniqueConstraints(table, dataProvider);
            }

            Constraint.PopulateReferentialConstraints(result, dataProvider);

            return result;
        }

        /// <summary>
        /// Gets the column with the specified name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>The column.</returns>
        public Column GetColumn(string columnName)
        {
            return this.Columns.Single(column => column.Name == columnName);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", this.Catalog, this.Schema, this.Name);
        }
    }
}
