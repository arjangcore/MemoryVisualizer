using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Helpers
{
	public static class Verify
	{
		[DebuggerStepThrough]
		public static void IsTrue(bool condition)
		{
			if (!condition)
			{
				throw new ArgumentException("Condition is not true", "condition");
			}
		}

		[DebuggerStepThrough]
		public static void IsTrue(bool condition, string paramName)
		{
			if (!condition)
			{
                throw new ArgumentException("Condition with parameter name is not true", "condition" + ":" + "paramName");
			}
		}

		public static void IsTrueWithMessage(bool condition, string message)
		{
			if (!condition)
				throw new ArgumentException(message);
		}

		[DebuggerStepThrough]
		public static void AssertNotNull(object obj)
		{
			Verify.IsTrue(obj != null);
		}

		public static void VerifyNotNull(object obj, string paramName)
		{
			if (obj == null)
				throw new ArgumentNullException(paramName);
		}

		public static void VerifyNotNull(object obj)
		{
			VerifyNotNull(obj, "value");
		}

		[DebuggerStepThrough]
		public static void AssertIsNotNaN(double d)
		{
			Verify.IsTrue(!Double.IsNaN(d));
		}

		[DebuggerStepThrough]
		public static void AssertIsFinite(double d)
		{
			Verify.IsTrue(!Double.IsInfinity(d) && !(Double.IsNaN(d)));
		}
	}
}
