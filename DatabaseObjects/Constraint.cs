﻿namespace DatabaseObjects
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// The type of database constraint
    /// </summary>
    public enum ConstraintType
    {
        /// <summary>
        /// A limit on the values that can be placed in a column
        /// </summary>
        Check,

        /// <summary>
        /// A requirement that the column's values not be duplicated
        /// </summary>
        Unique,

        /// <summary>
        /// A special case of unique constraint
        /// </summary>
        PrimaryKey,

        /// <summary>
        /// Points to a primary key on another table
        /// </summary>
        ForeignKey
    }

    /// <summary>
    /// Represents a constraint applied to a table in the database
    /// TODO Defaults?
    /// </summary>
    public class Constraint
    {
        /// <summary>
        /// The columns
        /// </summary>
        private IList<Column> columns;

        /// <summary>
        /// Initializes a new instance of the <see cref="Constraint" /> class
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="isDeferrable">if set to <c>true</c> [is deferrable].</param>
        /// <param name="initiallyDeferred">if set to <c>true</c> [initially deferred].</param>
        /// <exception cref="System.ArgumentNullException">table;table cannot be null</exception>
        public Constraint(string name, ConstraintType type, bool isDeferrable, bool initiallyDeferred)
        {
            // Populate member variables
            this.Name = name;
            this.ConstraintType = type;
            this.columns = new List<Column>();
            this.IsDeferrable = isDeferrable;
            this.InitiallyDeferred = initiallyDeferred;
        }

        /// <summary>
        /// Gets the name of the constraint.
        /// </summary>
        /// <value>
        /// The name of the constraint.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type of the constraint.
        /// </summary>
        /// <value>
        /// The type of the constraint.
        /// </value>
        public ConstraintType ConstraintType { get; private set; }

        /// <summary>
        /// Gets the column names covered by this constraint.
        /// </summary>
        /// <value>
        /// The column names.
        /// </value>
        public IList<Column> Columns
        {
            get
            {
                return this.columns;
            }
        }

        /// <summary>
        /// Gets the column this constraint refers to if it is a foreign key. This data is not always applicable, should possibly be in an inherited class?
        /// </summary>
        /// <value>
        /// The referred column.
        /// </value>
        public Column ReferencedColumn { get; private set; }

        /// <summary>
        /// Gets a value indicating whether enforcement of this constraint can be deferred.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is deferrable; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeferrable { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the constraint is deferred until just before the transaction commits.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [initially deferred]; otherwise, <c>false</c>.
        /// </value>
        public bool InitiallyDeferred { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is unique constraint.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is unique constraint; otherwise, <c>false</c>.
        /// </value>
        public bool IsUniqueConstraint
        {
            get
            {
                return this.ConstraintType == ConstraintType.PrimaryKey || this.ConstraintType == ConstraintType.Unique;
            }
        }

        /// <summary>
        /// Loads unique and primary key constraints from the database.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <exception cref="System.ArgumentNullException">
        /// table;table cannot be null
        /// or
        /// dataProvider;dataProvider cannot be null
        /// </exception>
        public static void PopulateUniqueConstraints(Table table, IObjectDataProvider dataProvider)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table", "table cannot be null");
            }

            if (dataProvider == null)
            {
                throw new ArgumentNullException("dataProvider", "dataProvider cannot be null");
            }

            using (IDataReader result = dataProvider.LoadUniqueConstraintDataForTable(table))
            {
                while (result.Read())
                {
                    // Read the result data
                    string name = (string)result["ConstraintName"];
                    string typeAsText = (string)result["Type"];
                    string columnName = (string)result["ColumnName"];
                    bool isDeferrable = ((string)result["IsDeferrable"]).Equals("NO") ? false : true;
                    bool initiallyDeferred = ((string)result["InitiallyDeferred"]).Equals("NO") ? false : true;

                    // Convert the constraint type
                    ConstraintType type = (ConstraintType)Enum.Parse(typeof(ConstraintType), typeAsText.Replace(" ", string.Empty), true);

                    // If the constraint has already been read, then this is just an additional column
                    // Otherwise it is a new constraint
                    if (table.Constraints.Any(constraint => constraint.Name == name))
                    {
                        // Add the column to the constraint
                        table.Constraints.Single(constraint => constraint.Name == name).AddColumn(table.GetColumn(columnName));
                    }
                    else
                    {
                        // Build the new constraint
                        Constraint newConstraint = new Constraint(name, type, isDeferrable, initiallyDeferred);
                        newConstraint.AddColumn(table.GetColumn(columnName));

                        table.Constraints.Add(newConstraint);
                    }
                }
            }
        }

        /// <summary>
        /// Loads check constraints from the database.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <exception cref="System.ArgumentNullException">table;table cannot be null
        /// or
        /// dataProvider;dataProvider cannot be null</exception>
        public static void PopulateCheckConstraints(Table table, IObjectDataProvider dataProvider)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table", "table cannot be null");
            }

            if (dataProvider == null)
            {
                throw new ArgumentNullException("dataProvider", "dataProvider cannot be null");
            }
        }

        /// <summary>
        /// Loads foreign key constraints from the database.
        /// </summary>
        /// <param name="tables">The tables.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <exception cref="System.ArgumentNullException">
        /// tables;tables cannot be null
        /// or
        /// dataProvider;dataProvider cannot be null
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// The foreign key refers to a table which doesn't exist in the collection
        /// or
        /// The foreign key refers to a column which hasn't been populated for the given table
        /// </exception>
        public static void PopulateReferentialConstraints(IEnumerable<Table> tables, IObjectDataProvider dataProvider)
        {
            if (tables == null)
            {
                throw new ArgumentNullException("tables", "tables cannot be null");
            }

            if (dataProvider == null)
            {
                throw new ArgumentNullException("dataProvider", "dataProvider cannot be null");
            }

            foreach (Table table in tables)
            {
                using (IDataReader result = dataProvider.LoadReferentialConstraintsDataForTable(table))
                {
                    while (result.Read())
                    {
                        // Read the result data
                        string name = (string)result["ConstraintName"];
                        string columnName = (string)result["ColumnName"];
                        bool isDeferrable = ((string)result["IsDeferrable"]).Equals("NO") ? false : true;
                        bool initiallyDeferred = ((string)result["InitiallyDeferred"]).Equals("NO") ? false : true;
                        string referencedCatalog = (string)result["ReferencedCatalog"];
                        string referencedSchema = (string)result["ReferencedSchema"];
                        string referencedTableName = (string)result["ReferencedTable"];
                        string referencedColumnName = (string)result["ReferencedColumn"];

                        // Create the constraint
                        Constraint constraint = new Constraint(name, ConstraintType.ForeignKey, isDeferrable, initiallyDeferred);
                        constraint.AddColumn(table.GetColumn(columnName));

                        // Find the table the foreign key refers to
                        Table referencedTable = tables.SingleOrDefault(t =>
                            t.Catalog == referencedCatalog
                            && t.Schema == referencedSchema
                            && t.Name == referencedTableName);

                        // Check it exists
                        if (referencedTable == null)
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The foreign key refers to a table which doesn't exist in the collection ({0}.{1}.{2})", referencedCatalog, referencedSchema, referencedTableName));
                        }

                        // Find the column on the table
                        Column referencedColumn = referencedTable.Columns.SingleOrDefault(c => c.Name == referencedColumnName);

                        // Check it exists
                        if (referencedColumn == null)
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The foreign key refers to a column which hasn't been populated for the given table ({0}.{1})", referencedTable, referencedColumnName));
                        }

                        // Assign the referenced column to the constraint
                        constraint.ReferencedColumn = referencedColumn;

                        table.Constraints.Add(constraint);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the column to this constraint's list of subject columns.
        /// </summary>
        /// <param name="column">The column.</param>
        public void AddColumn(Column column)
        {
            this.Columns.Add(column);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", this.Name, string.Join(", ", this.Columns.Select<Column, string>(column => column.Name)));
        }
    }
}