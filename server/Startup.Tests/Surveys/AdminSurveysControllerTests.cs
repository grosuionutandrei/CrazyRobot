using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using Startup.Tests.TestUtils;
using AuthUserRequest = Generated.AuthUserRequest;
using CreateSurveyRequestDto = Application.Models.Dtos.Surveys.CreateSurveyRequestDto;
using QuestionDto = Application.Models.Dtos.Surveys.QuestionDto;
using QuestionOptionDto = Application.Models.Dtos.Surveys.QuestionOptionDto;
using QuestionResponseDto = Application.Models.Dtos.Surveys.QuestionResponseDto;
using SurveyResponseDto = Application.Models.Dtos.Surveys.SurveyResponseDto;
using SurveyResultsDto = Application.Models.Dtos.Surveys.SurveyResultsDto;
using SurveySubmissionRequestDto = Application.Models.Dtos.Surveys.SurveySubmissionRequestDto;
using UpdateSurveyRequestDto = Application.Models.Dtos.Surveys.UpdateSurveyRequestDto;

namespace Startup.Tests.Surveys;

public class AdminSurveysControllerTests : WebApplicationFactory<Program>
{
    private HttpClient _httpClient;
    private IServiceProvider _scopedServiceProvider;

    [SetUp]
    public void Setup()
    {
        _httpClient = CreateClient();
        _scopedServiceProvider = Services.CreateScope().ServiceProvider;
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ApiTestSetupUtilities.ConfigureTestHost(builder);
        
        builder.ConfigureServices(services =>
        {
            services.DefaultTestConfig();
        });
    }

    [Test]
    public async Task GetAllSurveys_Unauthorized_WhenNoJwtProvided()
    {
        // Create a client without auth header
        var client = CreateClient();
        
        // Act
        var response = await client.GetAsync("/api/surveys/GetAllSurveys");
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetAllSurveys_ReturnsAllSurveys()
    {
        // Act
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_httpClient);
        var response = await _httpClient.GetAsync("/api/surveys/GetAllSurveys");
    
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var surveys = await response.Content.ReadFromJsonAsync<List<SurveyResponseDto>>();
        Assert.That(surveys, Is.Not.Null);
    }

    [Test]
    public async Task CreateSurvey_ReturnsBadRequest_WhenInvalidRequest()
    {
        // Arrange - Missing required fields
        var createDto = new CreateSurveyRequestDto
        {
            // Title is missing
            Description = "Survey Description",
            SurveyType = "Feedback",
            IsActive = true,
            Questions = new List<QuestionDto>()
        };
        
        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/surveys/CreateSurvey", createDto);
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task UpdateSurvey_UpdatesSurvey_WhenExists()
    {
        // Arrange - Create admin and survey
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_httpClient);

        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var admin = MockObjects.GetAdmin();
        ctx.Users.Add(admin);
        var survey = MockObjects.GetSurvey(admin.Id);
        ctx.Surveys.Add(survey);
        await ctx.SaveChangesAsync();
        
        // Update the survey
        var updateDto = new UpdateSurveyRequestDto
        {
            Id = survey.Id,
            Title = "Updated Survey",
            Description = "Updated Description",
            SurveyType = "Feedback",
            IsActive = false,
            Questions = new List<QuestionDto>
            {
                new()
                {
                    Id = survey.Questions.First().Id,
                    QuestionText = "Updated Question",
                    QuestionType = "Text",
                    OrderNumber = 1,
                    Options = new List<QuestionOptionDto>()
                }
            }
        };
        
        // Act
        var response = await _httpClient.PutAsJsonAsync($"/api/surveys/UpdateSurvey/{survey.Id}", updateDto);
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updatedSurvey = await response.Content.ReadFromJsonAsync<SurveyResponseDto>();
        Assert.That(updatedSurvey, Is.Not.Null);
        Assert.That(updatedSurvey.Title, Is.EqualTo("Updated Survey"));
        Assert.That(updatedSurvey.Description, Is.EqualTo("Updated Description"));
        Assert.That(updatedSurvey.IsActive, Is.False);
    }

    [Test]
    public async Task UpdateSurvey_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_httpClient);
        
        var nonExistentId = Guid.NewGuid().ToString();
        var updateDto = new UpdateSurveyRequestDto
        {
            Id = nonExistentId,
            Title = "Updated Survey",
            Description = "Updated Description",
            SurveyType = "Feedback",
            IsActive = true,
            Questions = new List<QuestionDto>()
            {
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    QuestionText = "Updated Question",
                    QuestionType = "Text",
                    OrderNumber = 1,
                    Options = new List<QuestionOptionDto>()
                }
            }
        };
        
        // Act
        var response = await _httpClient.PutAsJsonAsync($"/api/surveys/UpdateSurvey/{nonExistentId}", updateDto);
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task DeleteSurvey_DeletesSurvey_WhenExists()
    {
        // Arrange - Create admin and survey
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_httpClient);

        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var admin = MockObjects.GetAdmin();
        ctx.Users.Add(admin);
        
        var survey = MockObjects.GetSurvey(admin.Id);
        ctx.Surveys.Add(survey);
        
        await ctx.SaveChangesAsync();
    
        // Act
        var response = await _httpClient.DeleteAsync($"/api/surveys/DeleteSurvey/{survey.Id}");
    
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    
        // Verify it's deleted using GetAllSurveys
        var getAllResponse = await _httpClient.GetAsync("/api/surveys/GetAllSurveys");
        var allSurveys = await getAllResponse.Content.ReadFromJsonAsync<List<SurveyResponseDto>>();
        Assert.That(allSurveys.Any(s => s.Id == survey.Id), Is.False);
    }

    [Test]
    public async Task DeleteSurvey_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_httpClient);
        
        // Act
        var response = await _httpClient.DeleteAsync($"/api/surveys/DeleteSurvey/{Guid.NewGuid()}");
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task GetSurveyResults_ReturnsSurveyResults_WhenSurveyExists()
    {
        // Arrange - Create user_guest, survey and survey response
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_httpClient);

        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var admin = MockObjects.GetAdmin();
        ctx.Users.Add(admin);
        var userGuest = MockObjects.GetUser();
        ctx.UserGuests.Add(userGuest);
        
        var survey = MockObjects.GetSurvey(admin.Id);
        ctx.Surveys.Add(survey);
        
        var surveyResponse = MockObjects.GetSurveyResponse(survey, userGuest.Id);
        ctx.SurveyResponses.Add(surveyResponse);
        
        await ctx.SaveChangesAsync();
        
        // Act
        var response = await _httpClient.GetAsync($"/api/surveys/GetSurveysResults");
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var results = await response.Content.ReadFromJsonAsync<List<SurveyResultsDto>>();
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.GreaterThan(0));
        Assert.That(results[0].SurveyId, Is.EqualTo(survey.Id));
    }
}