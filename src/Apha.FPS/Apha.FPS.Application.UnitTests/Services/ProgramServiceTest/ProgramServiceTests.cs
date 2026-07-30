using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.ProgramServiceTest
{
    public class ProgramServiceTests
    {
        private readonly IProgramRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProgramService _sut;

        public ProgramServiceTests()
        {
            _mockRepository = Substitute.For<IProgramRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProgramService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllProgramsAsync_ReturnsMappedDtos()
        {
            var programs = new List<Program> { new Program { ProgramNo = "P1" } };
            var dtos = new List<ProgramDto> { new ProgramDto { ProgramNo = "P1" } };

            _mockRepository.GetAllProgramsAsync().Returns(programs);
            _mockMapper.Map<IEnumerable<ProgramDto>>(programs).Returns(dtos);

            var result = await _sut.GetAllProgramsAsync();

            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetAllProgramsAsync();
        }

        [Fact]
        public async Task GetAllProgramsForAllUsersAsync_ReturnsMappedDtos()
        {
            var programs = new List<Program> { new Program { ProgramNo = "P1" }, new Program { ProgramNo = "P2" } };
            var dtos = new List<ProgramDto> { new ProgramDto { ProgramNo = "P1" }, new ProgramDto { ProgramNo = "P2" } };

            _mockRepository.GetAllProgramsForAllUsers().Returns(programs);
            _mockMapper.Map<IEnumerable<ProgramDto>>(programs).Returns(dtos);

            var result = await _sut.GetAllProgramsForAllUsersAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetAllProgramsForAllUsers();
        }

        [Fact]
        public async Task GetAllProgramsForAllUsersAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            var emptyPrograms = new List<Program>();
            var emptyDtos = new List<ProgramDto>();

            _mockRepository.GetAllProgramsForAllUsers().Returns(emptyPrograms);
            _mockMapper.Map<IEnumerable<ProgramDto>>(emptyPrograms).Returns(emptyDtos);

            var result = await _sut.GetAllProgramsForAllUsersAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetAllProgramsForAllUsers();
        }

        [Fact]
        public async Task GetAllProgramsAsync_WithQuery_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string>();
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<Program>();
            var pagedResult = new PaginatedResult<ProgramDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAllProgramsAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProgramDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetAllProgramsAsync(query);

            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAllProgramsAsync(mappedParams);
        }

        [Fact]
        public async Task GetProgramByIdAsync_ValidId_ReturnsMappedDto()
        {
            var program = new Program { ProgramNo = "P1" };
            var dto = new ProgramDto { ProgramNo = "P1" };

            _mockRepository.GetProgramByIdAsync("P1").Returns(program);
            _mockMapper.Map<ProgramDto?>(program).Returns(dto);

            var result = await _sut.GetProgramByIdAsync("P1");

            result.Should().Be(dto);
            await _mockRepository.Received(1).GetProgramByIdAsync("P1");
        }

        [Fact]
        public async Task GetProgramByIdAsync_NotFound_ReturnsNull()
        {
            _mockRepository.GetProgramByIdAsync("P2").Returns((Program?)null);
            _mockMapper.Map<ProgramDto?>(null).Returns((ProgramDto?)null);

            var result = await _sut.GetProgramByIdAsync("P2");

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddProgramAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new ProgramDto { ProgramNo = "P1", ProgramName = "Test" };
            var entity = new Program { ProgramNo = "P1", ProgramName = "Test" };
            var added = new Program { ProgramNo = "P1", ProgramName = "Test" };
            var expected = new ProgramDto { ProgramNo = "P1", ProgramName = "Test" };

            _mockMapper.Map<Program>(dto).Returns(entity);
            _mockRepository.AddProgramAsync(entity).Returns(added);
            _mockMapper.Map<ProgramDto>(added).Returns(expected);

            var result = await _sut.AddProgramAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<Program>(dto);
            await _mockRepository.Received(1).AddProgramAsync(entity);
        }

        [Fact]
        public async Task AddProgramAsync_MissingProgramNo_Throws()
        {
            var dto = new ProgramDto { ProgramNo = "", ProgramName = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddProgramAsync(dto));
        }       

        [Fact]
        public async Task UpdateProgramAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new ProgramDto { ProgramNo = "P1", ProgramName = "Test" };
            var entity = new Program { ProgramNo = "P1", ProgramName = "Test" };
            var updated = new Program { ProgramNo = "P1", ProgramName = "Test2" };
            var expected = new ProgramDto { ProgramNo = "P1", ProgramName = "Test2" };

            _mockRepository.GetProgramByIdAsync(dto.ProgramNo).Returns(entity);
            _mockMapper.Map(dto, entity).Returns(entity);
            _mockRepository.UpdateProgramAsync(entity, "P1").Returns(updated);
            _mockMapper.Map<ProgramDto>(updated).Returns(expected);

            var result = await _sut.UpdateProgramAsync(dto);

            result.Should().Be(expected);
            await _mockRepository.Received(1).UpdateProgramAsync(entity, "P1");
        }

        [Fact]
        public async Task UpdateProgramAsync_NotFound_Throws()
        {
            var dto = new ProgramDto { ProgramNo = "P2", ProgramName = "Test" };
            _mockRepository.GetProgramByIdAsync(dto.ProgramNo).Returns((Program?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateProgramAsync(dto));
        }

        [Fact]
        public async Task DeleteProgramAsync_ValidId_ReturnsTrue()
        {
            var entity = new Program { ProgramNo = "P1" };
            _mockRepository.GetProgramByIdAsync("P1").Returns(entity);
            _mockRepository.DeleteProgramAsync("P1").Returns(true);

            var result = await _sut.DeleteProgramAsync("P1");

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteProgramAsync("P1");
        }

        [Fact]
        public async Task DeleteProgramAsync_NotFound_Throws()
        {
            _mockRepository.GetProgramByIdAsync("P2").Returns((Program?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteProgramAsync("P2"));
        }       

        [Fact]
        public async Task AddProgramAsync_DuplicateKey_PostgresException_ThrowsFriendlyInvalidOperationException()
        {
            var dto = new ProgramDto { ProgramNo = "P1", ProgramName = "Test" };
            var entity = new Program { ProgramNo = "P1", ProgramName = "Test" };
            var duplicate = BuildUniqueViolation();

            _mockMapper.Map<Program>(dto).Returns(entity);
            _mockRepository.AddProgramAsync(entity).ThrowsAsync(duplicate);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddProgramAsync(dto));
            ex.Message.Should().Contain("P1");
            ex.Message.Should().Contain("already exists");
            ex.InnerException.Should().BeSameAs(duplicate);
        }

        [Fact]
        public async Task AddProgramAsync_DuplicateKey_WrappedInDbUpdateException_ThrowsFriendlyInvalidOperationException()
        {
            var dto = new ProgramDto { ProgramNo = "P1", ProgramName = "Test" };
            var entity = new Program { ProgramNo = "P1", ProgramName = "Test" };
            var wrapped = new DbUpdateException("save failed", BuildUniqueViolation());

            _mockMapper.Map<Program>(dto).Returns(entity);
            _mockRepository.AddProgramAsync(entity).ThrowsAsync(wrapped);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddProgramAsync(dto));
            ex.Message.Should().Contain("already exists");
            ex.InnerException.Should().BeSameAs(wrapped);
        }

        [Fact]
        public async Task AddProgramAsync_NonUniqueViolation_RethrowsOriginalException()
        {
            var dto = new ProgramDto { ProgramNo = "P1", ProgramName = "Test" };
            var entity = new Program { ProgramNo = "P1", ProgramName = "Test" };
            var original = new InvalidOperationException("some other failure");

            _mockMapper.Map<Program>(dto).Returns(entity);
            _mockRepository.AddProgramAsync(entity).ThrowsAsync(original);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddProgramAsync(dto));
            ex.Should().BeSameAs(original);
        }

        // Builds a PostgresException carrying a unique/primary key violation (SqlState 23505),
        // mimicking how Npgsql surfaces a duplicate key error.
        private static PostgresException BuildUniqueViolation() =>
            new(
                messageText: "duplicate key value violates unique constraint",
                severity: "ERROR",
                invariantSeverity: "ERROR",
                sqlState: PostgresErrorCodes.UniqueViolation,
                detail: null,
                hint: null,
                position: 0,
                internalPosition: 0,
                internalQuery: null,
                where: null,
                schemaName: null,
                tableName: null,
                columnName: null,
                dataTypeName: null,
                constraintName: "tbluser_program_y2025_pkey");
    }
}