using DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
	class MockInstanceProvider
	{
		public static Column CreateMockColumn()
		{
			return new Column("TestColumn", CreateMockSqlType(), 0, null, true);
		}

		public static SqlType CreateMockSqlType()
		{
			return new SqlType("INT", null, null, null, 10, 10, 0, null);
		}

		public static Constraint CreateMockConstraint()
		{
			return new Constraint("TestConstraint", ConstraintType.Unique, false, false);
		}
	}
}
