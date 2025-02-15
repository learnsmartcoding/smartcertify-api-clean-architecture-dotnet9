using LSC.SmartCertify.Application.Interfaces.QuestionsChoice;
using LSC.SmartCertify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Infrastructure
{
    public class ChoiceRepository : IChoiceRepository
    {
        private readonly SmartCertifyContext _context;

        public ChoiceRepository(SmartCertifyContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Choice>> GetAllChoicesAsync(int questionId)
        {
            return await _context.Choices.Where(c => c.QuestionId == questionId).ToListAsync();
        }

        public async Task<Choice?> GetChoiceByIdAsync(int id)
        {
            return await _context.Choices.FirstOrDefaultAsync(c => c.ChoiceId == id);
        }

        public async Task AddChoiceAsync(Choice choice)
        {
            await _context.Choices.AddAsync(choice);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateChoiceAsync(Choice choice)
        {
            _context.Choices.Update(choice);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteChoiceAsync(Choice choice)
        {
            _context.Choices.Remove(choice);
            await _context.SaveChangesAsync();
        }
    }
}
