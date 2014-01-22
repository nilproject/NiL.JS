var sysasm = ReferenceError().GetType().BaseType.BaseType.Assembly;
console.log(sysasm.GetType("System.Double").GetMethod("Parse").Invoke(null, ["123"]))