//using FLIP.Application.Commands.ProcessId;
//using FLIP.Application.Interfaces;
//using FLIP.Application.Models;
//using FLIP.Infrastructure.Services;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.Configuration;
//using Moq;
//using Moq.Protected;

//namespace FLIP.Tests.Services;

//public class APIIntegrationTests
//{
//    private readonly Mock<IDapperQueries> _dapperMock;
//    private readonly Mock<IConfiguration> _configMock;
//    private readonly IMemoryCache _memoryCache;
//    private readonly APIIntegeration _apiIntegration;

//    public APIIntegrationTests()
//    {
//        _dapperMock = new Mock<IDapperQueries>();
//        _configMock = new Mock<IConfiguration>();

//        var memoryCacheOptions = new MemoryCacheOptions();
//        _memoryCache = new MemoryCache(memoryCacheOptions);

//        _configMock.Setup(c => c["retryCount"]).Returns("1");

//        _apiIntegration = new APIIntegeration(_configMock.Object, _dapperMock.Object, _memoryCache);
//    }

//    [Fact]
//    public async Task ProcessId_ShouldReturnEarly_WhenIdIsInCache()
//    {
//        // Arrange
//        var id = "1234567890";
//        _memoryCache.Set(id, true);
//        var request = new FreelancerDto { Id = id };

//        // Act
//        var result = await _apiIntegration.ProcessId(request);

//        // Assert
//        Assert.True(result.Success);
//        Assert.Equal(200, result.StatusCode);
//    }

//    [Fact]
//    public async Task ProcessId_ShouldInsertData_WhenApiCallSucceeds()
//    {
//        // Arrange
//        var id = "789";
//        var freelancers = new FreelancerData
//        {
//            NationalId = id, IsRide = false 
//        };
//        var logs = new ApiLog() ;
//        var errors = new ErrorLogs();

//        var apiIntegrationMock = new Mock<APIIntegeration>(_configMock.Object, _dapperMock.Object, _memoryCache)
//        {
//            CallBase = true
//        };

//        apiIntegrationMock
//            .Protected()
//            .Setup<Task<(bool, ApiLog, FreelancerData, ErrorLogs)>>("ExecuteParallelApiCallsAsync", id)
//            .ReturnsAsync((true, logs, freelancers, errors));

//        var request = new FreelancerDto { Id = id };

//        // Act
//        var result = await apiIntegrationMock.Object.ProcessId(request);

//        // Assert
//        Assert.True(result.Success);
//        Assert.Equal(200, result.StatusCode);
//        _dapperMock.Verify(x => x.InsertFreelancers(freelancers), Times.Once);
//        _dapperMock.Verify(x => x.InsertLogs(logs), Times.Once);
//    }

//    [Fact]
//    public async Task ProcessId_ShouldInsertRideData_WhenApiCallSucceeds()
//    {
//        // Arrange
//        var id = "789";
//        var freelancers = new FreelancerData
//        {
//            NationalId = id, IsRide = true 
//        };
//        var logs = new ApiLog();
//        var errors = new ErrorLogs();

//        var apiIntegrationMock = new Mock<APIIntegeration>(_configMock.Object, _dapperMock.Object, _memoryCache)
//        {
//            CallBase = true
//        };

//        apiIntegrationMock
//            .Protected()
//            .Setup<Task<(bool, ApiLog, FreelancerData, ErrorLogs)>>("ExecuteParallelApiCallsAsync", id)
//            .ReturnsAsync((true, logs, freelancers, errors));

//        var request = new FreelancerDto { Id = id };

//        // Act
//        var result = await apiIntegrationMock.Object.ProcessId(request);

//        // Assert
//        Assert.True(result.Success);
//        Assert.Equal(200, result.StatusCode);
//        _dapperMock.Verify(x => x.InsertFreelancerRide(freelancers), Times.Once);
//        _dapperMock.Verify(x => x.InsertLogs(logs), Times.Once);
//    }

//    [Fact]
//    public async Task ProcessId_ShouldReturnError_WhenAllApiCallsFail()
//    {
//        // Arrange
//        var id = "fail123";
//        var request = new FreelancerDto { Id = id };

//        var apiIntegrationMock = new Mock<APIIntegeration>(_configMock.Object, _dapperMock.Object, _memoryCache)
//        {
//            CallBase = true
//        };

//        apiIntegrationMock
//            .Protected()
//            .Setup<Task<(bool, ApiLog, FreelancerData, ErrorLogs)>>("ExecuteParallelApiCallsAsync", id)
//            .ReturnsAsync((false, [new() { Message = "All failed" }], [], []));

//        // Act
//        var result = await apiIntegrationMock.Object.ProcessId(request);

//        // Assert
//        Assert.False(result.Success);
//        Assert.Equal(400, result.StatusCode);
//    }

//    [Fact]
//    public async Task ProcessId_ShouldRetryApiCall_WhenApiFails()
//    {
//        // Arrange
//        var id = "retry123";
//        var request = new ProcessIdCommand { Id = id };

//        var apiIntegrationMock = new Mock<APIIntegeration>(_configMock.Object, _dapperMock.Object, _memoryCache)
//        {
//            CallBase = true
//        };

//        var freelancerData = new List<FreelancerData>
//        {
//            new() { NationalId = id, IsRide = false }
//        };

//        apiIntegrationMock
//            .Protected()
//            .Setup<Task<(bool, List<ApiLog>, List<FreelancerData>, List<ErrorLogs>)>>("ExecuteParallelApiCallsAsync", id)
//            .ReturnsAsync((false, [], [], []))
//            .Callback(() => {});

//        apiIntegrationMock
//            .Protected()
//            .Setup<Task<(bool, List<ApiLog>, List<FreelancerData>, List<ErrorLogs>)>>("ExecuteParallelApiCallsAsync", id)
//            .ReturnsAsync((true, new List<ApiLog> { new() }, freelancerData, new List<ErrorLogs>()))
//            .Callback(() => {});

//        _configMock.Setup(c => c["retryCount"]).Returns("2");

//        // Act
//        var result = await apiIntegrationMock.Object.ProcessId(request);

//        // Assert
//        Assert.True(result.Success);
//        _dapperMock.Verify(x => x.InsertFreelancers(It.IsAny<List<FreelancerData>>()), Times.Once); 
//        _dapperMock.Verify(x => x.InsertLogs(It.IsAny<List<ApiLog>>()), Times.Once);
//    }

//    [Fact]
//    public async Task ProcessId_ShouldLogApiCalls_WhenApiCallsSucceed()
//    {
//        // Arrange
//        var id = "log123";
//        var request = new ProcessIdCommand { Id = id };

//        var apiIntegrationMock = new Mock<APIIntegeration>(_configMock.Object, _dapperMock.Object, _memoryCache)
//        {
//            CallBase = true
//        };

//        var apiLogs = new List<ApiLog> { new() { Message = "API Call Succeeded" } };
//        apiIntegrationMock
//            .Protected()
//            .Setup<Task<(bool, List<ApiLog>, List<FreelancerData>, List<ErrorLogs>)>>("ExecuteParallelApiCallsAsync", id)
//            .ReturnsAsync((true, apiLogs, new List<FreelancerData>(), new List<ErrorLogs>()));

//        // Act
//        var result = await apiIntegrationMock.Object.ProcessId(request);

//        // Assert
//        Assert.True(result.Success);
//        _dapperMock.Verify(x => x.InsertLogs(apiLogs), Times.Once);
//        Assert.Equal("API Call Succeeded", apiLogs[0].Message);
//    }

//    [Fact]
//    public async Task ProcessId_ShouldHandleExceptions_WhenApiFailsWithException()
//    {
//        // Arrange
//        var id = "exception123";
//        var request = new ProcessIdCommand { Id = id };

//        var apiIntegrationMock = new Mock<APIIntegeration>(_configMock.Object, _dapperMock.Object, _memoryCache)
//        {
//            CallBase = true
//        };

//        apiIntegrationMock
//            .Protected()
//            .Setup<Task<(bool, List<ApiLog>, List<FreelancerData>, List<ErrorLogs>)>>("ExecuteParallelApiCallsAsync", id)
//            .ThrowsAsync(new Exception("API call failed"));

//        // Act & Assert
//        await Assert.ThrowsAsync<Exception>(() => apiIntegrationMock.Object.ProcessId(request));
//    }
//}
