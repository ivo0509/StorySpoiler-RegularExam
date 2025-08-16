using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Models;
using System.Net;
using System.Text.Json;



namespace StorySpoiler
{

    [TestFixture]

    public class StorySpoilerTests
    {
        private RestClient _client;
        private static string createdStoryId;
        private const string baseURL = "https://d3s5nxhwblsjbi.cloudfront.net";


        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("IvanBG", "ivanbg123");

† † † † † † // —˙Á‰‡‚‡ÏÂ ÍÎËÂÌÚ Ò ÚÓÍÂÌ
† † † † † † var options = new RestClientOptions(baseURL)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            _client = new RestClient(options);

        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseURL);

            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }


        [Test, Order(1)]
        public void CreateStory_WithRequiredFields_ShouldReturnCreated()
        {
            var newStory = new
            {
                Title = "MyTestStory",
                Description = "This is a test story",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newStory);

            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created),
                $"Expected 201 Created but got {response.StatusCode}");

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content!);

            createdStoryId = json.GetProperty("storyId").GetString() ?? string.Empty;
            var msg = json.GetProperty("msg").GetString();



            Assert.That(msg, Is.EqualTo("Successfully created!"));

        }

        [Test, Order(2)]
        public void EditCreatedStory_ShouldPass()
        {
            var changes = new
            {
                Title = "Edited_Story",
                Description = "Edited_Description",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
            request.AddJsonBody(changes);

            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                $"Expected 200 OK but got {response.StatusCode}");

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content!);
            var msg = json.GetProperty("msg").GetString();

            Assert.That(msg, Is.EqualTo("Successfully edited"));



        }

        [Test, Order(3)]
        public void GetAllStories_ShouldReturnListAllStories()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
              $"Expected 200 OK but got {response.StatusCode}");
  
            var stories = JsonSerializer.Deserialize<List<StoryDTO>>(response.Content);
            Assert.That(stories, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteStory_ShouldReturnedOK()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
              $"Expected 200 OK but got {response.StatusCode}");

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content!);
            var msg = json.GetProperty("msg").GetString();

            Assert.That(msg, Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var newStory = new
            {
                Title = "",
                Description = ""
              
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newStory);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            var fakeId = "123";

            var changes = new
            {
                Title = "Neshto si",
                Description = "neshtosi"

            };

            var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);
            request.AddJsonBody(changes);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content!);
            var msg = json.GetProperty("msg").GetString();

            Assert.That(msg, Is.EqualTo("No spoilers..."));

        }


        [Test, Order(7)]
        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            var fakeId = "123";


            var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);
            
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content!);
            var msg = json.GetProperty("msg").GetString();

            Assert.That(msg, Is.EqualTo("Unable to delete this story spoiler!"));

        }

        [OneTimeTearDown]
        public void TearDown() 
        {
            _client?.Dispose();
        }

    }
}