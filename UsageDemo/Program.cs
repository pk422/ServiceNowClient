using System;
using ServiceNow;
using ServiceNow.Interface;
using ServiceNow.TableAPI;
using ServiceNow.TableDefinitions;

using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Net;
using System.Web;

namespace UsageDemo
{
	class Program
	{
		static string myInstance = "";
		static string instanceUserName = "";
		static string instancePassword = "";

		static void Main(string[] args)
		{

			// Shows basic CRUD operations with a simple small subset of a record
			//basicOperations();
			string sys_id = CreateDocument();

			 AttachDocument(sys_id);

			// Demonstrates a more advanced retrieval including related resource by link and dot traversal.
			//retrieveByQuery();

			// Break
			Console.WriteLine("\n\rCompleted.");
			Console.ReadLine();
		}

		private static void AttachDocument(string sys_id)
		{

			try
			{

				// Read file data
				FileStream fs = new FileStream("C:\\Users\\pradeep\\Desktop\\New Text Document.txt", FileMode.Open, FileAccess.Read);
				byte[] data = new byte[fs.Length];
				fs.Read(data, 0, data.Length);
				fs.Close();

				// Generate post objects
				Dictionary<string, object> postParameters = new Dictionary<string, object>();
				postParameters.Add("filename", "New Text Document.txt");
				postParameters.Add("table_name", "dms_document");
				postParameters.Add("table_sys_id", sys_id);
				postParameters.Add("file", new FormUpload.FileParameter(data, "New Text Document.txt", "text/plain"));

				// Create request and receive response
				string postURL = "https://dev27787.service-now.com/api/now/attachment/upload";
				HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(postURL, postParameters);

				// Process response
				StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
				string fullResponse = responseReader.ReadToEnd();
				webResponse.Close();

			}
			catch (WebException ex)
			{


			}
			catch (Exception ex)
			{

				
			}
			
			//return false;
		}

		private static string CreateDocument()
		{
			TableAPIClient<dms_document> userClient = new TableAPIClient<dms_document>("dms_document", myInstance, instanceUserName, instancePassword);

			// Deomstrate the Create (POST) operation:
			var createDocument = userClient.Post(new dms_document
			{
				DocumentName = "Test Document1",
				Classification = DocumentClassification.Public.ToString(),
				AutoIncrementRevision = true,
				NameFormat = "Default",
				Description = "Test Description for document1 creation for c# client",
				 Department="Process Management",
				TypeOfDocument = DocumentType.Procedure.ToString()
			});

			return createDocument.Result.sys_id;

		}

		static void basicOperations()
		{
			TableAPIClient<sys_user> userClient = new TableAPIClient<sys_user>("sys_user", myInstance, instanceUserName, instancePassword);

			// Deomstrate the Create (POST) operation:
			var createdUser = userClient.Post(new sys_user()
			{
				employee_number = "012345",
				first_name = "Tester",
				last_name = "McTester",
				email = "tester@testcompany.com",
				phone = "",
				// You can use Name instead of sys_id, but if service now does not find your value it will ignore it without any warning.
				location = new ResourceLink() { value = "VISALIA COURTHOUSE" }
			});
			Console.WriteLine("User Created: " + createdUser.Result.first_name + " " + createdUser.Result.last_name + " (" + createdUser.Result.sys_id + ")");


			// Deonstrate the GetById (GET) operation:
			var retrievedUser = userClient.GetById(createdUser.Result.sys_id);
			Console.WriteLine("User Retrieved: " + retrievedUser.Result.first_name + " " + retrievedUser.Result.last_name + " (" + retrievedUser.Result.sys_id + ")");
			Console.WriteLine("              : eMail: " + retrievedUser.Result.email);
			Console.WriteLine("              : Location: " + retrievedUser.Result.Location_name);


			// Demonstrate Update (PUT) operation:
			Console.WriteLine("\n\nUpdating User");
			if (retrievedUser.Result != null)
			{
				var d = retrievedUser.Result;
				d.email = "newEmail@testcompany.com";

				// Set the location using the Guid of a good location, otherwise handle it.
				try
				{
					d.location = new ResourceLink() { value = findLocationId("VISALIA DISTRICT OFFICE") };
				}
				catch (Exception ex)
				{
					Console.WriteLine("Unable to set new user location: " + ex.Message);
				}

				var updatedUser = userClient.Put(d);
				Console.WriteLine("              : eMail: " + updatedUser.Result.email);
				Console.WriteLine("              : Location: " + updatedUser.Result.Location_name);
			}


			// Domonstrate Delete operation
			Console.Write("\n\nDeleting User");
			userClient.Delete(retrievedUser.Result.sys_id);
			Console.WriteLine("...Done");
		}


		static void retrieveByQuery()
		{
			Console.WriteLine("\n\nRetrieving active, unresolved incidents");
			var query = @"active=true^u_resolved=false";
			TableAPIClient<incident> client = new TableAPIClient<incident>("incident", myInstance, instanceUserName, instancePassword);

			IRestQueryResponse<incident> response = client.GetByQuery(query);

			Console.WriteLine(response.ResultCount + " records found. \n\nPress return to list results.");
			Console.ReadLine();
			foreach (incident r in response.Result)
			{
				DateTime openedOn = DateTime.Parse(r.opened_at);
				ResourceLink openedFor = r.caller_id;

				Console.WriteLine(r.number + " :  " + r.short_description + " (Opened " + openedOn.ToShortDateString() + " for " + r.caller_first_name + ")");
			}
		}


		// You would of course have these cached somewhere most likely.
		static string findLocationId(string locationName)
		{
			TableAPIClient<location> locationClient = new TableAPIClient<location>("cmn_location", myInstance, instanceUserName, instancePassword);
			var query = @"name=" + locationName;

			IRestQueryResponse<location> locationResult = locationClient.GetByQuery(query);
			if (locationResult.ResultCount == 0) throw new Exception(String.Format("No location by the name {0} was found.", locationName));
			if (locationResult.ResultCount > 1) throw new Exception(String.Format("Multiple locations found by the name {0}.", locationName));

			// We found our location lets return it
			return locationResult.Result.First().sys_id;
		}

		private static string NewDataBoundary()
		{
			Random rnd = new Random();
			string formDataBoundary = "";
			while (formDataBoundary.Length < 15)
			{
				formDataBoundary = formDataBoundary + rnd.Next();
			}
			formDataBoundary = formDataBoundary.Substring(0, 15);
			formDataBoundary = "-----------------------------" + formDataBoundary;
			return formDataBoundary;
		}

		public static HttpWebResponse MultipartFormDataPost(string postUrl, IEnumerable<Cookie> cookies, Dictionary<string, string> postParameters)
		{
			string boundary = NewDataBoundary();

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postUrl);

			// Set up the request properties
			request.Method = "POST";
			request.ContentType = "multipart/form-data; boundary=" + boundary;
			//request.UserAgent = "PhasDocAgent 1.0";
			//request.CookieContainer = new CookieContainer();

			//foreach (var cookie in cookies)
			//{
			//	request.CookieContainer.Add(cookie);
			//}

			#region WRITING STREAM
			using (Stream formDataStream = request.GetRequestStream())
			{
				foreach (var param in postParameters)
				{
					if (param.Value.StartsWith("file://"))
					{
						string filepath = param.Value.Substring(7);

						// Add just the first part of this param, since we will write the file data directly to the Stream
						string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n",
							boundary,
							param.Key,
							Path.GetFileName(filepath) ?? param.Key,
						 MimeMapping.GetMimeMapping(filepath));

						formDataStream.Write(Encoding.UTF8.GetBytes(header), 0, header.Length);

						// Write the file data directly to the Stream, rather than serializing it to a string.

						byte[] buffer = new byte[2048];

						FileStream fs = new FileStream(filepath, FileMode.Open);

						for (int i = 0; i < fs.Length;)
						{
							int k = fs.Read(buffer, 0, buffer.Length);
							if (k > 0)
							{
								formDataStream.Write(buffer, 0, k);
							}
							i = i + k;
						}
						fs.Close();
					}
					else
					{
						string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n",
							boundary,
							param.Key,
							param.Value);
						formDataStream.Write(Encoding.UTF8.GetBytes(postData), 0, postData.Length);
					}
				}
				// Add the end of the request
				byte[] footer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
				formDataStream.Write(footer, 0, footer.Length);
				request.ContentLength = formDataStream.Length;
				formDataStream.Close();
			}
			#endregion

			return request.GetResponse() as HttpWebResponse;
		}

	}
}
