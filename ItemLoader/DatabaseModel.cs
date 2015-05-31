﻿namespace ItemLoader
{
	using System.Collections.Generic;
	using System.Linq;
	using Items;

	/// <summary>
	/// Singleton class to represent an entire database and it's objects.
	/// </summary>
	public static class DatabaseModel
	{
		/// <summary>
		/// Initializes static members of the <see cref="DatabaseModel"/> class.
		/// </summary>
		static DatabaseModel()
		{
			DatabaseModel.Tables = new List<DatabaseTable>();
		}

		/// <summary>
		/// Gets the tables represented in the database.
		/// </summary>
		/// <value>
		/// The tables.
		/// </value>
		public static List<DatabaseTable> Tables
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets all columns for all tables in the database.
		/// </summary>
		/// <value>
		/// The columns.
		/// </value>
		public static IEnumerable<DatabaseColumn> Columns
		{
			get
			{
				return Tables.SelectMany(table => table.Columns);
			}
		}

		/// <summary>
		/// Gets all constraints for all tables in the database.
		/// </summary>
		/// <value>
		/// The constraints.
		/// </value>
		public static IEnumerable<DatabaseConstraint> Constraints
		{
			get
			{
				return Tables.SelectMany(table => table.Constraints);
			}
		}

		/// <summary>
		/// Loads the objects from the database.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		public static void LoadFromDatabase(string connectionString)
		{
			// Load the tables
			DatabaseTable.LoadTables(connectionString);
		}

		/// <summary>
		/// Constructs a model from the loaded database objects.
		/// </summary>
		/// <returns>A model build from the schema</returns>
		public static Model ConstructModel()
		{
			Model result = new Model();

			// Populate base items
			foreach (DatabaseTable table in Tables.Where(table => table.RepresentsItem))
			{
				Item item = new Item(table.Name);

				// Add additional details
				item.Details.Add("SqlCatalog", table.Catalog);
				item.Details.Add("SqlSchema", table.Schema);
				item.Details.Add("SqlTable", table.Name);

				// Add columns (which aren't relationships)
				foreach (DatabaseColumn column in table.Columns.Where(dbColumn => !dbColumn.IsReferencer && !dbColumn.IsReferenced))
				{
					item.Attributes.Add(new ValueAttribute(column.Name, column.GetSystemType(), column.GetNullability()));
				}

				result.AddItem(item);
			}

			// Populate categories
			foreach (DatabaseTable table in Tables.Where(table => table.RepresentsCategory))
			{
				Category category = new Category(table.Name);

				// Add additional details
				category.Details.Add("SqlCatalog", table.Catalog);
				category.Details.Add("SqlSchema", table.Schema);
				category.Details.Add("SqlTable", table.Name);

				// Add columns (which aren't relationships)
				foreach (DatabaseColumn column in table.Columns.Where(dbColumn => !dbColumn.IsReferencer && !dbColumn.IsReferenced))
				{
					category.Attributes.Add(new ValueAttribute(column.Name, column.GetSystemType(), column.GetNullability()));
				}

				result.AddCategory(category);
			}

			// Populate relationships from link tables
			foreach (DatabaseTable table in Tables.Where(t => t.RepresentsRelationship))
			{
				// Find the first columns which are foreign keys and assume those are the right and left sides of the relationship
				IEnumerable<DatabaseColumn> columnsWithForeignKeys = table.Columns.Where(column => column.IsReferencer);

				DatabaseColumn leftColumn = columnsWithForeignKeys.OrderBy(column => column.OrdinalPosition).ToList()[0];
				Thing leftThing = result.Things.Single(thing => leftColumn.ReferencedColumn.Table.IsThingMatch(thing));

				DatabaseColumn rightColumn = columnsWithForeignKeys.OrderBy(column => column.OrdinalPosition).ToList()[1];
				Thing rightThing = result.Things.Single(thing => rightColumn.ReferencedColumn.Table.IsThingMatch(thing));

				Relationship relationship = new Relationship(table.Name, leftThing, rightThing);

				// Add additional details
				relationship.Details.Add("SqlCatalog", table.Catalog);
				relationship.Details.Add("SqlSchema", table.Schema);
				relationship.Details.Add("SqlTable", table.Name);

				result.AddRelationship(relationship);
			}

			// Populate relationships from foreign keys for tables we haven't processed yet
			foreach (DatabaseTable table in Tables)
			{
				// Find the columns with foreign keys
				IEnumerable<DatabaseColumn> columnsWithForeignKeys = table.Columns.Where(column => column.IsReferencer);

				// If this is a relationship table then the first two columns are already a relationship
				if (table.RepresentsRelationship)
				{
					columnsWithForeignKeys = columnsWithForeignKeys.Skip(2);
				}

				foreach (DatabaseColumn column in columnsWithForeignKeys)
				{
					// Get the thing for this table
					Thing leftThing = result.Things.Single(table.IsThingMatch);

					// Find the table being referenced by the foreign key
					Thing rightThing = result.Things.Single(column.ReferencedColumn.Table.IsThingMatch);

					// Find the constraint which is doing the referencing
					// TODO move to DatabaseColumn property
					DatabaseConstraint constraint = Constraints.Single(dbConstraint => dbConstraint.Type == ConstraintType.ForeignKey && dbConstraint.Columns.Contains(column));

					// Create the relationship
					// The left hand side (referencer) can only point to a single item on the right hand side (referenced)
					// TODO it might be possible to infer more detail here using unique and NOT NULL constraints
					Relationship relationship = new Relationship(constraint.Name, leftThing, rightThing);
					relationship.RightLink.AmountLower = 1;
					relationship.RightLink.AmountUpper = 1;

					// Add additional details
					relationship.Details.Add("SqlCatalog", table.Catalog);
					relationship.Details.Add("SqlSchema", table.Schema);
					relationship.Details.Add("SqlTable", table.Name);
					relationship.Details.Add("SqlColumns", string.Join(", ", constraint.Columns));

					result.AddRelationship(relationship);
				}
			}

			return result;
		}
	}
}
