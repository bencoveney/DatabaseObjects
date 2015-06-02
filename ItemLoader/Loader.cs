﻿namespace ItemLoader
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SqlClient;
	using Items;

	/// <summary>
	/// Loads database objects into a model
	/// </summary>
	public class Loader
	{
		#region SQL Strings

		/// <summary>
		/// Query to load basic item information
		/// </summary>
		private const string ItemDataAttributesQuery = @"
WITH ForeignKeys AS (
	/* Find which columns of tables are foreign key constraints */
	SELECT
		t.name AS ParentTable,
		c.name AS ParentColumn,
		rt.name AS ReferencedTable,
		rc.name AS ReferencedColumn
	FROM
		sys.foreign_key_columns AS fk
		INNER JOIN sys.tables AS t ON fk.parent_object_id = t.object_id
		INNER JOIN sys.columns AS c ON fk.parent_object_id = c.object_id AND fk.parent_column_id = c.column_id
		INNER JOIN sys.tables AS rt ON fk.referenced_object_id = rt.object_id
		INNER JOIN sys.columns AS rc ON fk.referenced_object_id = rc.object_id AND fk.referenced_column_id = rc.column_id
)
SELECT
	COLUMN_NAME,
	IS_NULLABLE,
	DATA_TYPE, 
	CHARACTER_MAXIMUM_LENGTH,
	ReferencedTable,
	ReferencedColumn
FROM
	INFORMATION_SCHEMA.COLUMNS
	LEFT OUTER JOIN ForeignKeys ON INFORMATION_SCHEMA.COLUMNS.TABLE_NAME = ParentTable AND INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME = ParentColumn
WHERE
	Table_Name = @itemName";

		/// <summary>
		/// Query to load basic collection information for an item
		/// </summary>
		private const string ItemDataAttributesQuery = @"
SELECT
	/*fk.name AS Name,*/
	t.name AS TableWithForeignKey,
	c.name AS ForeignKeyColumn
FROM
	sys.foreign_key_columns AS fk
	INNER JOIN sys.tables AS t ON fk.parent_object_id = t.object_id
	INNER JOIN sys.columns AS c ON fk.parent_object_id = c.object_id AND fk.parent_column_id = c.column_id
WHERE
	fk.referenced_object_id = OBJECT_ID(@itemName)
	AND t.name NOT LIKE '%Collection'";

		/// <summary>
		/// Query to find the columns with foreign keys on collection tables
		/// </summary>
		private const string CollectionReferencesQuery = @"
SELECT
	c.name AS referencedTable
FROM
	sys.foreign_key_columns AS fk
	INNER JOIN sys.tables AS t ON fk.parent_object_id = t.object_id
	INNER JOIN sys.columns AS c ON fk.parent_object_id = c.object_id
		AND fk.parent_column_id = c.column_id
WHERE
	t.name = @collectionName";

		/// <summary>
		/// Query to find names of items
		/// </summary>
		private const string ItemNamesQuery = @"
SELECT
	SchemaTables.Table_Name
FROM
	Information_Schema.Tables AS SchemaTables
WHERE
	SchemaTables.Table_Type = 'BASE TABLE'
	AND SchemaTables.Table_Name != 'sysdiagrams'
	AND SchemaTables.Table_Name != '__RefactorLog'";

		/// <summary>
		/// Query to find names of collections
		/// </summary>
		private const string CollectionNamesQuery = @"
SELECT
	SchemaTables.Table_Name
FROM
	Information_Schema.Tables AS SchemaTables
WHERE
	SchemaTables.Table_Name LIKE '%Collection'";

		/// <summary>
		/// Query to find names of categories
		/// </summary>
		private const string CategoryNamesQuery = @"
SELECT
	SchemaTables.Table_Name
FROM
	Information_Schema.Tables AS SchemaTables
WHERE
	SchemaTables.Table_Name LIKE '%Category'";

		/// <summary>
		/// Query to find which columns on which tables have unique constraints
		/// This query does not handle unique constraints which apply to multiple columns
		/// </summary>
		private const string UniqueConstraintsQuery = @"
SELECT
	SchemaConstraint.CONSTRAINT_NAME,
	SchemaConstraint.CONSTRAINT_TYPE,
	SchemaConstraint.TABLE_NAME,
	ConstrainedColumn.COLUMN_NAME,
	SchemaConstraint.IS_DEFERRABLE,
	SchemaConstraint.INITIALLY_DEFERRED,
FROM
	INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS SchemaConstraint
	INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE AS ConstrainedColumn ON SchemaConstraint.CONSTRAINT_NAME = ConstrainedColumn.CONSTRAINT_NAME
WHERE
	SchemaConstraint.CONSTRAINT_TYPE IN ('UNIQUE', 'PRIMARY KEY')
	/* Dont populate unique constraints for categories/collections because they arent built yet */
	AND SchemaConstraint.TABLE_NAME NOT LIKE '%Category%'
	AND SchemaConstraint.TABLE_NAME NOT LIKE '%Collection%'
	AND SchemaConstraint.TABLE_NAME != '__RefactorLog'
	/* Only add constraints which reference a single value (for now...) */
	AND SchemaConstraint.CONSTRAINT_NAME IN (
		SELECT
			CONSTRAINT_NAME
		FROM
			INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE
		GROUP BY
			CONSTRAINT_NAME
		HAVING
			SUM(1) <= 1
	)";

		/// <summary>
		/// Query to find the primary keys on a table
		/// </summary>
		private const string PrimaryIdentifiersQuery = @"
SELECT
	/*TC.CONSTRAINT_NAME,*/
	/*TC.CONSTRAINT_TYPE,*/
	TC.TABLE_NAME,
	CC.COLUMN_NAME
FROM
	INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC
	INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE AS CC ON TC.CONSTRAINT_NAME = CC.CONSTRAINT_NAME
WHERE
	TC.CONSTRAINT_TYPE = 'PRIMARY KEY'
	AND TC.TABLE_NAME != '__RefactorLog'
	AND TC.TABLE_NAME NOT LIKE '%Collection'";

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="Loader"/> class.
		/// </summary>
		public Loader()
		{
			Model = new Model();
		}

		/// <summary>
		/// Gets or sets the model that is being populated
		/// </summary>
		public Model Model { get; set; }

		/// <summary>
		/// Loads a model from the database
		/// </summary>
		/// <param name="connection">The connection.</param>
		public void Load(SqlConnection connection)
		{
			connection.Open();

			foreach (string itemName in this.GetItemNames(connection))
			{
				// TODO Make Add only take the string/item?
				Item newItem = new Item(itemName);
				this.Model.Items.Add(itemName, newItem);
				this.PopulateAttributesForItemBase(connection, newItem);
			}

			foreach (string categoryName in this.GetCategoryNames(connection))
			{
				Category newCategory = new Category(categoryName);
				this.Model.Categories.Add(categoryName, newCategory);
				this.PopulateAttributesForItemBase(connection, newCategory);
			}

			foreach (string collectionName in this.GetCollectionNames(connection))
			{
				this.PopulateDataAttributes(connection, collectionName);
			}

			this.PopulateAdditionalData();

			this.PopulateUniqueConstraints(connection);

			this.PopulatePrimaryKeys(connection);

			connection.Close();

			Model.Validate();
		}

		/// <summary>
		/// Gets the item names.
		/// </summary>
		/// <param name="connection">The connection.</param>
		/// <returns>A list of names of items</returns>
		public List<string> GetItemNames(SqlConnection connection)
		{
			List<string> itemNames = new List<string>();

			using (SqlCommand command = new SqlCommand(ItemNamesQuery, connection))
			{
				using (SqlDataReader result = command.ExecuteReader())
				{
					while (result.Read())
					{
						itemNames.Add(result.GetString(0));
					}
				}
			}

			return itemNames;
		}

		/// <summary>
		/// Gets the category names.
		/// </summary>
		/// <param name="connection">The connection.</param>
		/// <returns>A list of names of categories</returns>
		public List<string> GetCategoryNames(SqlConnection connection)
		{
			List<string> categoryNames = new List<string>();

			using (SqlCommand command = new SqlCommand(CategoryNamesQuery, connection))
			{
				using (SqlDataReader result = command.ExecuteReader())
				{
					while (result.Read())
					{
						categoryNames.Add(result.GetString(0));
					}
				}
			}

			return categoryNames;
		}

		/// <summary>
		/// Gets the collection names.
		/// </summary>
		/// <param name="connection">The connection.</param>
		/// <returns>A list of names of collections</returns>
		public List<string> GetCollectionNames(SqlConnection connection)
		{
			List<string> collectionNames = new List<string>();

			using (SqlCommand command = new SqlCommand(CollectionNamesQuery, connection))
			{
				using (SqlDataReader result = command.ExecuteReader())
				{
					while (result.Read())
					{
						collectionNames.Add(result.GetString(0));
					}
				}
			}

			return collectionNames;
		}

		/// <summary>
		/// Gets the attributes for item.
		/// </summary>
		/// <param name="connection">The connection.</param>
		/// <param name="item">The item.</param>
		/// <exception cref="System.NotImplementedException">Foreign key pointing to column which isn't the primary key (and named item.itemID)</exception>
		public void PopulateAttributesForItemBase(SqlConnection connection, Thing item)
		{
			// Value Attributes
			using (SqlCommand command = new SqlCommand(ItemAttributesQuery, connection))
			{
				command.Parameters.Add("@itemName", SqlDbType.NVarChar).Value = item.Name;
				using (SqlDataReader result = command.ExecuteReader())
				{
					while (result.Read())
					{
						// Keep the name of the db column safe to decorate the object with
						string columnName = result.GetString(0);
						string attributeName = columnName;

						// Remove the items name from the beginning of the attribute
						// e.g. KitchenID becomes ID
						if (attributeName.StartsWith(item.Name) && !string.IsNullOrEmpty(attributeName.Substring(item.Name.Length)))
						{
							attributeName = attributeName.Substring(item.Name.Length);
						}

						// Calculate the datatype
						IType type;

						// If there is no referenced column
						if (result.IsDBNull(4))
						{
							// TODO Data type size
							// TODO I broke this
							type = (IType)null; // this.SqlTypeToSystemType(result.GetString(2));
						}
						else
						{
							string referencedItem = result.GetString(4);
							string referencedColumn = result.GetString(5);

							if (string.Equals(referencedItem + "ID", referencedColumn, System.StringComparison.InvariantCultureIgnoreCase))
							{
								// If we're referencing the primary key of the item type
								// TODO check if we're referencing the key explicity
								// TODO perform the same logic on the itemName here that is done elsewhere
								type = new ItemType(referencedItem);
								attributeName = referencedItem;
							}
							else
							{
								// If we're referencing some other attribute of the item
								throw new NotImplementedException("Foreign key pointing to column which isn't the primary key (and named item.itemID)");
							}
						}

						// TODO guessing at empty, could be the other type/both
						Nullability nullability = result.GetString(1) == "YES" ? Nullability.Empty : Nullability.Invalid;

						// TODO What about default value?
						DataAttribute attribute = new DataAttribute(attributeName, type, nullability);
						attribute.Details["SqlColumn"] = columnName;

						if (!result.IsDBNull(3))
						{
							attribute.Constraints.Add(new StringLengthConstraint(LengthComparison.ShorterThan, result.GetInt32(3) + 1));
						}

						item.Attributes.Add(attribute);
					}
				}
			}

			// Collection Attributes
			using (SqlCommand command = new SqlCommand(ItemCollectionAttributesQuery, connection))
			{
				command.Parameters.Add("@itemName", SqlDbType.NVarChar).Value = item.Name;
				using (SqlDataReader result = command.ExecuteReader())
				{
					while (result.Read())
					{
						// Read results
						string tableName = result.GetString(0);
						string columnName = result.GetString(1);

						// Create the friendly name
						string collectionName = string.Format("{0}s", tableName.Replace("Collection", string.Empty));

						string itemTypeName = result.GetString(0);
						ItemType itemType = new ItemType(itemTypeName);

						DataAttribute collection = new DataAttribute(collectionName, itemType, Nullability.Empty);
						collection.Details["SqlColumn"] = string.Format("{0}.{1}", tableName, columnName);

						item.Attributes.Add(collection);
					}
				}
			}
		}

		/// <summary>
		/// Populates attributes for collections
		/// </summary>
		/// <param name="connection">The connection.</param>
		/// <param name="collectionName">Name of the collection.</param>
		public void PopulateDataAttributes(SqlConnection connection, string collectionName)
		{
			// Value Attributes
			using (SqlCommand command = new SqlCommand(CollectionReferencesQuery, connection))
			{
				string[] referencedTables = new string[2];

				command.Parameters.Add("@collectionName", SqlDbType.NVarChar).Value = collectionName;
				using (SqlDataReader result = command.ExecuteReader())
				{
					for (int i = 0; i < 2; i++)
					{
						result.Read();
						string columnName = result.GetString(0);
						string itemName = columnName.Substring(0, columnName.Length - 2);
						referencedTables[i] = itemName;
					}
				}

				DataAttribute firstCollection = new DataAttribute(referencedTables[1] + "s", new ItemType(referencedTables[1]), Nullability.Invalid);
				firstCollection.Details["SqlColumn"] = string.Format("{0}.{1}ID", collectionName, referencedTables[1]);
				Model.Items[referencedTables[0]].Attributes.Add(firstCollection);

				DataAttribute secondCollection = new DataAttribute(referencedTables[0] + "s", new ItemType(referencedTables[0]), Nullability.Invalid);
				secondCollection.Details["SqlColumn"] = string.Format("{0}.{1}ID", collectionName, referencedTables[0]);
				Model.Items[referencedTables[1]].Attributes.Add(secondCollection);
			}
		}

		/// <summary>
		/// Populates the additional data such as descriptions.
		/// </summary>
		public void PopulateAdditionalData()
		{
			Model.Items["Container"].Description = "A container is a basic tool, consisting of any device creating a partially or fully enclosed space that can be used to contain, store, and transport objects or materials. In commerce, it includes any receptacle or enclosure for holding a product used in packaging and shipping. Things kept inside of a container are protected by being inside of its structure. The term is most frequently applied to devices made from materials that are durable and at least partly rigid.";
			Model.Items["Person"].Description = "A person is a being, such as a human, that has certain capacities or attributes constituting personhood, which in turn is defined differently by different authors in different disciplines, and by different cultures in different times and places. In ancient Rome, the word persona (Latin) or prosopon (πρόσωπον; Greek) originally referred to the masks worn by actors on stage. The various masks represented the various personae in the stage play.";
			Model.Items["Kitchen"].Description = "A kitchen is a room or part of a room used for cooking and food preparation. In the West, a modern residential kitchen is typically equipped with a stove, a sink with hot and cold running water, a refrigerator and kitchen cabinets arranged according to a modular design. Many households have a microwave oven, a dishwasher and other electric appliances. The main function of a kitchen is cooking or preparing food but it may also be used for dining, food storage, entertaining, dishwashing, laundry.";
			Model.Items["Foodstuff"].Description = "Food is any substance consumed to provide nutritional support for the body. It is usually of plant or animal origin, and contains essential nutrients, such as fats, proteins, vitamins, or minerals. The substance is ingested by an organism and assimilated by the organism's cells to provide energy, maintain life, or stimulate growth. Historically, people secured food through two methods: hunting and gathering, and agriculture. Today, most of the food energy required by the ever increasing population of the world is supplied by the food industry.";
		}

		/// <summary>
		/// Writes the reader to console.
		/// </summary>
		/// <param name="reader">The reader.</param>
		public void WriteReaderToConsole(SqlDataReader reader)
		{
			for (int i = 0; i < reader.FieldCount; i++)
			{
				Console.Write("{0}, ", reader.GetName(i));
			}

			Console.WriteLine();

			while (reader.Read())
			{
				for (int i = 0; i < reader.FieldCount; i++)
				{
					Console.Write("{0}, ", reader.GetValue(i));
				}

				Console.WriteLine();
			}
		}

		/// <summary>
		/// Adds processes unique constraints from the database and adds them to the relevant items
		/// </summary>
		/// <param name="connection">The connection.</param>
		public void PopulateUniqueConstraints(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(UniqueConstraintsQuery, connection))
			{
				using (SqlDataReader result = command.ExecuteReader())
				{
					while (result.Read())
					{
						string itemName = result.GetString(2);
						string columnName = result.GetString(3);

						// Check whether the column matches the nameID pattern, if it is then strip the ID
						// this is duplicated code and should be refactored
						string attributeName = string.Equals(itemName + "ID", columnName, System.StringComparison.InvariantCultureIgnoreCase) ? "ID" : columnName;

						// Add a unique constraint to the attribute
						DataAttribute attribute = Model.Items[itemName].Attributes[attributeName];
						attribute.Constraints.Add(new AttributeConstraint(attribute, CollectionComparison.IsUniqueWithin));
					}
				}
			}
		}

		/// <summary>
		/// Adds model information gained from primary key analysis
		/// </summary>
		/// <param name="connection">The connection.</param>
		public void PopulatePrimaryKeys(SqlConnection connection)
		{
			using (SqlCommand command = new SqlCommand(PrimaryIdentifiersQuery, connection))
			{
				using (SqlDataReader result = command.ExecuteReader())
				{
					while (result.Read())
					{
						string tableName = result.GetString(0);
						string columnName = result.GetString(1);

						// Remove the items name from the beginning of the attribute
						// e.g. KitchenID becomes ID
						if (columnName.StartsWith(tableName) && !string.IsNullOrEmpty(columnName.Substring(tableName.Length)))
						{
							columnName = columnName.Substring(tableName.Length);
						}

						// Assign the attribute as the primary key
						Thing thing = Model.Items.ContainsKey(tableName) ? (Thing)Model.Items[tableName] : (Thing)Model.Categories[tableName];
						DataAttribute primaryKey = thing.Attributes[columnName];
						thing.IntegerIdentifer = (DataAttribute)primaryKey;
					}
				}
			}
		}
	}
}
