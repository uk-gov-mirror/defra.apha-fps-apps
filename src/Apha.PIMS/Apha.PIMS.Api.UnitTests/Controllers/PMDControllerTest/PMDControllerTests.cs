using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace Apha.PIMS.Api.UnitTests.Controllers.PMDControllerTest;

public class PMDControllerTests
{
    private readonly IMilestoneService _service;
    private readonly IMapper _mapper;
    private readonly PMDController _controller;

    public PMDControllerTests()
    {
        _service = Substitute.For<IMilestoneService>();
        _mapper = Substitute.For<IMapper>();
        _controller = new PMDController(_service, _mapper)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task GetProjectYearManagers_AsAdmin_PassesNullEmail()
    {
        // Arrange
        const int year = 2026;
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "API-PMDAdmin") }, "TestAuth"));

        var dtoList = new List<ProjectYearManagerDto>
        {
            new() { ProjectYear = year, ParentProject = "PP001", Manager = "J. Smith", ManagerNumber = "M001" }
        };
        var resList = new List<ProjectYearManagerRes>
        {
            new() { ProjectYear = year, ParentProject = "PP001", Manager = "J. Smith", ManagerNumber = "M001" }
        };

        _service.GetProjectYearManagersAsync(year, null, false).Returns(dtoList);
        _mapper.Map<List<ProjectYearManagerRes>>(dtoList).Returns(resList);

        // Act
        var result = await _controller.GetProjectYearManagers(year);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(resList, ok.Value);
        await _service.Received(1).GetProjectYearManagersAsync(year, null, false);
    }

    [Fact]
    public async Task GetProjectYearManagers_AsAdminAndProjectManager_PassesNullEmail()
    {
        // Arrange
        const int year = 2026;
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Role, "API-PMDAdmin"),
                    new Claim(ClaimTypes.Role, "API-PIMSProjectManager"),
                    new Claim(ClaimTypes.Email, "manager@apha.gov.uk")
                },
                "TestAuth"));

        var dtoList = new List<ProjectYearManagerDto>
        {
            new() { ProjectYear = year, ParentProject = "PP001", Manager = "J. Smith", ManagerNumber = "M001" }
        };
        var resList = new List<ProjectYearManagerRes>
        {
            new() { ProjectYear = year, ParentProject = "PP001", Manager = "J. Smith", ManagerNumber = "M001" }
        };

        _service.GetProjectYearManagersAsync(year, null, false).Returns(dtoList);
        _mapper.Map<List<ProjectYearManagerRes>>(dtoList).Returns(resList);

        // Act
        var result = await _controller.GetProjectYearManagers(year);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(resList, ok.Value);
        await _service.Received(1).GetProjectYearManagersAsync(year, null, false);
    }

    [Fact]
    public async Task GetProjectYearManagers_AsProjectManager_PassesEmailFromClaim()
    {
        // Arrange
        const int year = 2026;
        const string email = "manager@apha.gov.uk";
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Role, "API-PIMSProjectManager"),
                    new Claim(ClaimTypes.Email, email)
                },
                "TestAuth"));

        var dtoList = new List<ProjectYearManagerDto>
        {
            new() { ProjectYear = year, ParentProject = "PP001", Manager = "J. Smith", ManagerNumber = "M001" }
        };
        var resList = new List<ProjectYearManagerRes>
        {
            new() { ProjectYear = year, ParentProject = "PP001", Manager = "J. Smith", ManagerNumber = "M001" }
        };

        _service.GetProjectYearManagersAsync(year, email, true).Returns(dtoList);
        _mapper.Map<List<ProjectYearManagerRes>>(dtoList).Returns(resList);

        // Act
        var result = await _controller.GetProjectYearManagers(year);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(resList, ok.Value);
        await _service.Received(1).GetProjectYearManagersAsync(year, email, true);
    }
}
