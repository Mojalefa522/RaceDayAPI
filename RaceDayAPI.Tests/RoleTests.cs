using System.Net;
using System.Net.Http.Json;

namespace RaceDayAPI.Tests
{
    public class RoleTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public RoleTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });
        }

        [Fact]
        public async Task Unauthenticated_CreateEvent_Returns401()
        {
            var newEvent = new
            {
                name = "Test Event",
                description = "Testing unauthenticated access",
                location = "Test Location",
                startDate = DateTime.Now.AddDays(1),
                endDate = DateTime.Now.AddDays(2)
            };

            var response = await _client.PostAsJsonAsync("/api/events", newEvent);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CreateEvent_Returns403()
        {
            var registerDto = new
            {
                email = "participant@test.com",
                password = "Password123!",
                fullName = "Test Participant",
                role = "Participant",
                gender = "Male",
                emergencyContact = "1234567890",
                medicalNotes = "None"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new
            {
                email = "participant@test.com",
                password = "Password123!"
            };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var newEvent = new
            {
                name = "Hacker Event",
                description = "Should not be created",
                location = "Test Location",
                startDate = DateTime.Now.AddDays(1),
                endDate = DateTime.Now.AddDays(2)
            };
            var response = await _client.PostAsJsonAsync("/api/events", newEvent);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        [Fact]
        public async Task Organiser_CreateEvent_Returns201()
        {
            var registerDto = new
            {
                email = "organiser@test.com",
                password = "Password123!",
                fullName = "Test Organiser",
                role = "Organiser",
                companyName = "Test Company",
                contactPhone = "1234567890",
                website = "https://test.com"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new
            {
                email = "organiser@test.com",
                password = "Password123!"
            };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var newEvent = new
            {
                name = "Organiser Event",
                description = "This should be created",
                location = "Test Location",
                startDate = DateTime.Now.AddDays(1),
                endDate = DateTime.Now.AddDays(2)
            };
            var response = await _client.PostAsJsonAsync("/api/events", newEvent);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        [Fact]
        public async Task Participant_EnrolInEvent_Returns200()
        {
            var organiserReg = new
            {
                email = "org2@test.com",
                password = "Password123!",
                fullName = "Test Org",
                role = "Organiser",
                companyName = "Test Co",
                contactPhone = "123",
                website = "https://test.com"
            };
            await _client.PostAsJsonAsync("/api/auth/register", organiserReg);
            await _client.PostAsJsonAsync("/api/auth/login", new { email = "org2@test.com", password = "Password123!" });

            var newEvent = new
            {
                name = "Enrolment Test Event",
                description = "Testing",
                location = "Test Location",
                startDate = DateTime.Now.AddDays(1),
                endDate = DateTime.Now.AddDays(2)
            };
            var eventResponse = await _client.PostAsJsonAsync("/api/events", newEvent);
            var eventData = await eventResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            int eventId = eventData.GetProperty("eventId").GetInt32();

            var newCategory = new
            {
                name = "10km",
                description = "10km run",
                distance = 10.0,
                maxCapacity = 100
            };
            var categoryResponse = await _client.PostAsJsonAsync($"/api/events/{eventId}/categories", newCategory);
            var categoryData = await categoryResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            int categoryId = categoryData.GetProperty("categoryId").GetInt32();

            await _client.PostAsync("/api/auth/logout", null);

            var participantReg = new
            {
                email = "part2@test.com",
                password = "Password123!",
                fullName = "Test Part",
                role = "Participant",
                gender = "Male",
                emergencyContact = "123",
                medicalNotes = "None"
            };
            await _client.PostAsJsonAsync("/api/auth/register", participantReg);
            await _client.PostAsJsonAsync("/api/auth/login", new { email = "part2@test.com", password = "Password123!" });

            var enrolDto = new { categoryId = categoryId };
            var response = await _client.PostAsJsonAsync("/api/enrolments", enrolDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task Organiser_ViewEventEnrolments_Returns200()
        {
            var organiserReg = new
            {
                email = "org3@test.com",
                password = "Password123!",
                fullName = "Test Org",
                role = "Organiser",
                companyName = "Test Co",
                contactPhone = "123",
                website = "https://test.com"
            };
            await _client.PostAsJsonAsync("/api/auth/register", organiserReg);
            await _client.PostAsJsonAsync("/api/auth/login", new { email = "org3@test.com", password = "Password123!" });

            var newEvent = new
            {
                name = "View Enrolments Event",
                description = "Testing",
                location = "Test Location",
                startDate = DateTime.Now.AddDays(1),
                endDate = DateTime.Now.AddDays(2)
            };
            var eventResponse = await _client.PostAsJsonAsync("/api/events", newEvent);
            var eventData = await eventResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            int eventId = eventData.GetProperty("eventId").GetInt32();

            var newCategory = new
            {
                name = "10km",
                description = "10km run",
                distance = 10.0,
                maxCapacity = 100,
                minAge = 10,
                maxAge = 99
            };
            var categoryResponse = await _client.PostAsJsonAsync($"/api/events/{eventId}/categories", newCategory);
            var categoryData = await categoryResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            int categoryId = categoryData.GetProperty("categoryId").GetInt32();

            await _client.PostAsync("/api/auth/logout", null);

            var participantReg = new
            {
                email = "part3@test.com",
                password = "Password123!",
                fullName = "Test Part",
                role = "Participant",
                gender = "Male",
                emergencyContact = "123",
                medicalNotes = "None"
            };
            await _client.PostAsJsonAsync("/api/auth/register", participantReg);
            await _client.PostAsJsonAsync("/api/auth/login", new { email = "part3@test.com", password = "Password123!" });

            await _client.PostAsJsonAsync("/api/enrolments", new { categoryId = categoryId });

            await _client.PostAsync("/api/auth/logout", null);

            await _client.PostAsJsonAsync("/api/auth/login", new { email = "org3@test.com", password = "Password123!" });

            var response = await _client.GetAsync($"/api/events/{eventId}/enrolments");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task Participant_ViewResults_Returns200()
        {
            var participantReg = new
            {
                email = "part4@test.com",
                password = "Password123!",
                fullName = "Test Part",
                role = "Participant",
                gender = "Male",
                emergencyContact = "123",
                medicalNotes = "None"
            };
            await _client.PostAsJsonAsync("/api/auth/register", participantReg);
            await _client.PostAsJsonAsync("/api/auth/login", new { email = "part4@test.com", password = "Password123!" });

            var response = await _client.GetAsync("/api/results/my-results");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }//
    }
}