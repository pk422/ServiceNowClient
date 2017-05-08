using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNow.Interface
{
	interface IAttachmentAPIClient<T>
	 where T : ServiceNow.Record
	{
		void Delete(string id);
		RESTSingleResponse<T> GetById(string id);
		RESTQueryResponse<T> GetByQuery(string query);
		RESTSingleResponse<T> Post(T record);
		RESTSingleResponse<T> Put(T record);
	}
}
