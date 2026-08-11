using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.PublicationTypeServiceTest
{
    public class PublicationTypeServiceTests
    {
        private readonly IPublicationTypeRepository _repository;
        private readonly IMapper _mapper;
        private readonly PublicationTypeService _service;

        public PublicationTypeServiceTests()
        {
            _repository = Substitute.For<IPublicationTypeRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new PublicationTypeService(_repository, _mapper);
        }

        private static PublicationTypeDto MakeDto(string type = "RPC", string? description = null)
            => new PublicationTypeDto { Type = type, Description = description };

        private static PublicationType MakeEntity(string type = "RPC", string? description = null)
            => new PublicationType { Type = type, Description = description };

        [Fact]
        public async Task CreatePublicationTypeAsync_DuplicateTypeCode_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = MakeDto("RPC");
            _repository.PublicationTypeExistsAsync("RPC").Returns(true);

            // Act
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.CreatePublicationTypeAsync(dto));

            // Assert
            Assert.Single(exception.Errors);
            Assert.Equal("PUBLICATION_TYPE_ALREADY_EXISTS", exception.Errors[0].Code);
            Assert.Equal("Type code 'RPC' already exists.", exception.Errors[0].Message);
            await _repository.DidNotReceive().AddPublicationTypeAsync(Arg.Any<PublicationType>());
        }

        [Fact]
        public async Task CreatePublicationTypeAsync_ValidDto_ReturnsMappedCreatedDto()
        {
            // Arrange
            var dto = MakeDto("NRT", "Narrative");
            var entity = MakeEntity("NRT", "Narrative");
            var created = MakeEntity("NRT", "Narrative");
            var resultDto = MakeDto("NRT", "Narrative");

            _repository.PublicationTypeExistsAsync("NRT").Returns(false);
            _mapper.Map<PublicationType>(dto).Returns(entity);
            _repository.AddPublicationTypeAsync(entity).Returns(created);
            _mapper.Map<PublicationTypeDto>(created).Returns(resultDto);

            // Act
            var result = await _service.CreatePublicationTypeAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NRT", result.Type);
            Assert.Equal("Narrative", result.Description);
            await _repository.Received(1).AddPublicationTypeAsync(entity);
        }
    }
}
