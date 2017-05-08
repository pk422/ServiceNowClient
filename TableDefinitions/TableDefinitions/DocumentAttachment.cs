using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNow.TableDefinitions
{
	public class DocumentAttachment
	{
		[JsonProperty("table_name")]
		public string TableName { get; set; }

		[JsonProperty("table_sys_id")]
		public string SysIdForDocument { get; set; }

		[JsonProperty("attachment")]
		public string Attachment { get; set; }

		


	}
}
