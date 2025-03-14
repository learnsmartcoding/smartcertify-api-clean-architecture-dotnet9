using AutoMapper;
using LSC.SmartCertify.Application.DTOs;
using LSC.SmartCertify.Application.Interfaces.QuestionsChoice;
using LSC.SmartCertify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Infrastructure
{

    public class QuestionRepository : IQuestionRepository
    {
        private readonly SmartCertifyContext _context;
        private readonly IMapper mapper;

        public QuestionRepository(SmartCertifyContext context, IMapper mapper)
        {
            _context = context;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<Question>> GetAllQuestionsAsync()
        {
            return await _context.Questions.Include(q => q.Choices).ToListAsync();
        }

        public async Task<Question?> GetQuestionByIdAsync(int id)
        {
            return await _context.Questions.Include(q => q.Choices).FirstOrDefaultAsync(q => q.QuestionId == id);
        }

        public async Task<Question> AddQuestionAsync(Question question)
        {
            await _context.Questions.AddAsync(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task UpdateQuestionAsync(Question question)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuestionAsync(Question question)
        {
            //currently case delete not enable hence we use like this, if we enable case deleteing in table
            // Remove all choices associated with the question
            var choices = _context.Choices.Where(c => c.QuestionId == question.QuestionId);
            _context.Choices.RemoveRange(choices);

            // Now remove the question
            _context.Questions.Remove(question);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuestionAndChoicesAsync(int id, QuestionDto dto)
        {
            var question = await GetQuestionByIdAsync(id);
            if (question == null)
                throw new KeyNotFoundException("Question not found");

            // Map basic properties (excluding choices)
            mapper.Map(dto, question);

            // Handle Choices separately
            var existingChoiceIds = question.Choices.Select(c => c.ChoiceId).ToList();
            var incomingChoiceIds = dto.Choices.Select(c => c.ChoiceId).ToList();

            // Find choices to delete
            var choicesToDelete = question.Choices.Where(c => !incomingChoiceIds.Contains(c.ChoiceId)).ToList();
            foreach (var choice in choicesToDelete)
            {
                _context.Choices.Remove(choice);
            }

            // Update existing choices and add new ones
            foreach (var choiceDto in dto.Choices)
            {
                var existingChoice = question.Choices.FirstOrDefault(c => c.ChoiceId == choiceDto.ChoiceId);
                if (existingChoice != null)
                {
                    // Update existing choice
                    mapper.Map(choiceDto, existingChoice);
                }
                else
                {
                    // Add new choice
                    var newChoice = mapper.Map<Choice>(choiceDto);
                    question.Choices.Add(newChoice);
                }
            }

            await UpdateQuestionAsync(question);
        }

    }
}
