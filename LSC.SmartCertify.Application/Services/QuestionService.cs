using AutoMapper;
using LSC.SmartCertify.Application.DTOs;
using LSC.SmartCertify.Application.Interfaces.QuestionsChoice;
using LSC.SmartCertify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Application.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _repository;
        private readonly IMapper _mapper;

        public QuestionService(IQuestionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync()
        {
            var questions = await _repository.GetAllQuestionsAsync();
            return _mapper.Map<IEnumerable<QuestionDto>>(questions);
        }

        public async Task<QuestionDto?> GetQuestionByIdAsync(int id)
        {
            var question = await _repository.GetQuestionByIdAsync(id);
            return question == null ? null : _mapper.Map<QuestionDto>(question);
        }

        public async Task AddQuestionAsync(CreateQuestionDto dto)
        {
            var question = _mapper.Map<Question>(dto);
            await _repository.AddQuestionAsync(question);
        }

        public async Task UpdateQuestionAsync(int id, UpdateQuestionDto dto)
        {
            var question = await _repository.GetQuestionByIdAsync(id);
            if (question == null)
                throw new KeyNotFoundException("Question not found");

            _mapper.Map(dto, question);
            await _repository.UpdateQuestionAsync(question);
        }

        public async Task DeleteQuestionAsync(int id)
        {
            var question = await _repository.GetQuestionByIdAsync(id);
            if (question == null)
                throw new KeyNotFoundException("Question not found");

            await _repository.DeleteQuestionAsync(question);
        }

        public async Task<QuestionDto> AddQuestionAndChoicesAsync(QuestionDto dto)
        {
            var question = _mapper.Map<Question>(dto);
            question.Choices = dto.Choices.Select(c => _mapper.Map<Choice>(c)).ToList();

            await _repository.AddQuestionAsync(question);
            _mapper.Map(question, dto);
            return dto;
        }

        public async Task UpdateQuestionAndChoicesAsync(int id, QuestionDto dto)
        {
            await _repository.UpdateQuestionAndChoicesAsync(id, dto);           
        }

    }
}
