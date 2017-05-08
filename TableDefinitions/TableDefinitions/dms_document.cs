using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNow.TableDefinitions
{
	public class dms_document : Record
	{
		[JsonProperty("name")]
		public string DocumentName { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("department")]
		public string Department { get; set; }

		[JsonProperty("type")]
		public string TypeOfDocument { get; set; }

		[JsonProperty("Classification")]
		public string Classification { get; set; }

		[JsonProperty("Auto increment revision")]
		public bool AutoIncrementRevision { get; set; }

		[JsonProperty("Name format")]
		public string NameFormat { get; set; }



	}
}
