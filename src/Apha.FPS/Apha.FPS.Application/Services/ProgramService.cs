using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using Npgsql;

namespace Apha.FPS.Application.Services
{
    public class ProgramService : IProgramService
    {
        private readonly IProgramRepository _programRepository;
        private readonly IMapper _mapper;

        public ProgramService(IProgramRepository programRepository, IMapper mapper)
        {
            _programRepository = programRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProgramDto>> GetAllProgramsAsync()
        {
            var programs =  await _programRepository.GetAllProgramsAsync();
            return _mapper.Map<IEnumerable<ProgramDto>>(programs);
        }

        public async Task<IEnumerable<ProgramDto>> GetAllProgramsForAllUsersAsync()
        {
            var programs = await _programRepository.GetAllProgramsForAllUsers();
            return _mapper.Map<IEnumerable<ProgramDto>>(programs);
        }

        public async Task<PaginatedResult<ProgramDto>> GetAllProgramsAsync(QueryParameters<string> query)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var programViews = await _programRepository.GetAllProgramsAsync(filter);
            return _mapper.Map<PaginatedResult<ProgramDto>>(programViews);
        }

        public async Task<ProgramDto?> GetProgramByIdAsync(string programNo)
        {
            var program = await _programRepository.GetProgramByIdAsync(programNo);
            return _mapper.Map<ProgramDto?>(program);
        }

        public async Task<ProgramDto> AddProgramAsync(ProgramDto programDto)
        {
            if (string.IsNullOrWhiteSpace(programDto.ProgramNo))
            {
                throw new ArgumentException("Program number is required.");
            }           

            var program = _mapper.Map<Core.Entities.Program>(programDto);

            try
            {
                var addedProgram = await _programRepository.AddProgramAsync(program);
                return _mapper.Map<ProgramDto>(addedProgram);
            }
            catch (Exception ex) when (IsUniqueViolation(ex))
            {
                throw new InvalidOperationException(
                    $"Program '{programDto.ProgramNo}' already exists. " +
                    "Please use a different program.", ex);
            }
        }

        public async Task<ProgramDto> UpdateProgramAsync(ProgramDto programDto)
        {           
            ArgumentNullException.ThrowIfNull(programDto);
           
            if (string.IsNullOrWhiteSpace(programDto.ProgramNo))
            {
                throw new ArgumentException("Program number is required.");
            }            
           
            var originalProgramNo = programDto.ProgramNo;
            var existingProgram = await _programRepository.GetProgramByIdAsync(originalProgramNo);
            if (existingProgram == null)
            {
                throw new KeyNotFoundException($"Program with ID '{originalProgramNo}' not found.");
            }
            _mapper.Map(programDto, existingProgram);
            var updatedProgram = await _programRepository.UpdateProgramAsync(existingProgram, originalProgramNo);
            return _mapper.Map<ProgramDto>(updatedProgram);
        }

        public async Task<bool> DeleteProgramAsync(string programNo)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(programNo);

            // Check if record exists
            var existingEntity = await _programRepository.GetProgramByIdAsync(programNo);
            if (existingEntity == null)
            {
                throw new KeyNotFoundException($"Program with ID '{programNo}' was not found.");
            }

            // DTrig: Restrict deletion when linked Projects exist
            if (await _programRepository.HasLinkedProjectsAsync(programNo))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"Cannot delete Program '{programNo}' because linked Projects exist.",
                        "PROGRAM_HAS_LINKED_PROJECTS")
                ]);
            }

            return await _programRepository.DeleteProgramAsync(programNo);
        }       

        // Detects a unique/primary key constraint violation (SqlState 23505) which surfaces
        // as a PostgresException, usually wrapped inside a DbUpdateException.
        private static bool IsUniqueViolation(Exception? ex)
        {
            for (var current = ex; current is not null; current = current.InnerException)
            {
                if (current is PostgresException pgEx
                    && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
