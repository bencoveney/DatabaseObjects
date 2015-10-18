using DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
	class MockObjectDataProvider : IObjectDataProvider
	{
		public IDataReader LoadTableData()
		{
			DataTable mockTableData = new DataTable();
			mockTableData.Columns.Add("Table_Catalog");
			mockTableData.Columns.Add("Table_Schema");
			mockTableData.Columns.Add("Table_Name");

			DataRow firstRow = mockTableData.NewRow();
			firstRow["Table_Catalog"] = "TestCatalog";
			firstRow["Table_Schema"] = "TestSchema";
			firstRow["Table_Name"] = "FirstTestTable";
			mockTableData.Rows.Add(firstRow);

			DataRow secondRow = mockTableData.NewRow();
			secondRow["Table_Catalog"] = "TestCatalog";
			secondRow["Table_Schema"] = "TestSchema";
			secondRow["Table_Name"] = "SecondTestTable";
			mockTableData.Rows.Add(secondRow);

			DataRow thirdRow = mockTableData.NewRow();
			thirdRow["Table_Catalog"] = "TestCatalog";
			thirdRow["Table_Schema"] = "TestSchema";
			thirdRow["Table_Name"] = "ThirdTestTable";
			mockTableData.Rows.Add(thirdRow);

			return mockTableData.CreateDataReader();
		}

		public IDataReader LoadColumnDataForTable(Table table)
		{
			DataTable mockTableData = new DataTable();
			return mockTableData.CreateDataReader();
		}

		public IDataReader LoadUniqueConstraintDataForTable(Table table)
		{
			DataTable mockTableData = new DataTable();
			return mockTableData.CreateDataReader();
		}

		public IDataReader LoadCheckConstraintsDataForTable(Table table)
		{
			DataTable mockTableData = new DataTable();
			return mockTableData.CreateDataReader();
		}

		public IDataReader LoadReferentialConstraintsDataForTable(Table table)
		{
			DataTable mockTableData = new DataTable();
			return mockTableData.CreateDataReader();
		}

		public IDataReader LoadRoutineData()
		{
			DataTable mockTableData = new DataTable();
			return mockTableData.CreateDataReader();
		}

		public IDataReader LoadParametersDataForRoutine(Routine routine)
		{
			DataTable mockTableData = new DataTable();
			return mockTableData.CreateDataReader();
		}
	}
}
