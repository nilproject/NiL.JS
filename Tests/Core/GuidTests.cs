using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;
using NiL.JS.Extensions;
using System;

namespace Tests.Core;

	[TestClass]
	public class GuidTests
	{
		[TestMethod]
		public void AsParsesGuids()
		{
			string script = @"var output = '5d063342-47c6-4948-b29b-0487e0884265'";
			Context jsContext = new Context();
			jsContext.Eval(script, true);
			Guid output = jsContext.GetVariable("output").As<Guid>();

			Assert.AreEqual(Guid.Parse("5d063342-47c6-4948-b29b-0487e0884265"), output);
		}

		[TestMethod]
		public void AsParsesNullableGuids()
		{
			string script = @"var output = '5d063342-47c6-4948-b29b-0487e0884265'";
			Context jsContext = new Context();
			jsContext.Eval(script, true);
			Guid? output = jsContext.GetVariable("output").As<Guid?>();

			Assert.AreEqual(Guid.Parse("5d063342-47c6-4948-b29b-0487e0884265"), output.Value);			
		}

		[TestMethod]
		public void AsThrowsOnInvalidGuids()
		{
			string script = @"var output = '5d063342-47c6-4948-b29b-0487e08842'";
			Context jsContext = new Context();
			jsContext.Eval(script, true);
			Assert.ThrowsException<FormatException>(() => jsContext.GetVariable("output").As<Guid>());

			script = @"var output = '5d063342-47c6-4948-b29b-0487e088426j'";
			Assert.ThrowsException<FormatException>(() => jsContext.GetVariable("output").As<Guid>());
			
			script = @"var output = '5d063342-47c6-4948-b29b-0487e08842'";
			jsContext.Eval(script, true);
			Assert.ThrowsException<FormatException>(() => jsContext.GetVariable("output").As<Guid?>());
		}
	}
