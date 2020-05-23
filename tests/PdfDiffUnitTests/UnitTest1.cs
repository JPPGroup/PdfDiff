using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using PdfDiff;

namespace PdfDiffUnitTests
{
    public class Tests
    {
        WebApplicationFactory<PdfDiff.Startup> _factory;

        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Startup>();
        }

        [Test]
        public async Task VerifyFailureOnMissingFiles()
        {
            HttpClient client = _factory.CreateClient();
            HttpResponseMessage response = await client.PostAsync("/api/difference", null);

            if (!response.IsSuccessStatusCode)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public async Task VerifyFileUploadAndResponse()
        {
            HttpClient client = _factory.CreateClient();
            

            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                content.Add(CreateFileContent("Original.pdf"));
                content.Add(CreateFileContent("Modified.pdf"));

                HttpResponseMessage response = await client.PostAsync("/api/difference", content);

                /*System.Net.Http.HttpContent contentResponse = response.Content;
                var contentStream = await content.ReadAsStreamAsync(); // get the actual content stream
                contentStream.Position = 0;*/
                string path = Path.GetTempFileName();
                using (FileStream file = File.Create(path))
                {
                    await response.Content.CopyToAsync(file);
                    //await contentStream.CopyToAsync(file);
                }
            }
        }

        private StreamContent CreateFileContent(string fileName)
        {
            StreamContent fileContent = new StreamContent(File.Open(fileName, FileMode.Open));
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") 
            { 
                Name = "\"files\"", 
                FileName = "\"" + fileName + "\""
            }; // the extra quotes are key here
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return fileContent;
        }
    }
}