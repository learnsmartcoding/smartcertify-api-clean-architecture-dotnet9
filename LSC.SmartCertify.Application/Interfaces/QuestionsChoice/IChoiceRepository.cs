using LSC.SmartCertify.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.Interfaces.QuestionsChoice
{
    public interface IChoiceRepository
    {
        Task<IEnumerable<Choice>> GetAllChoicesAsync(int questionId);
        Task<Choice?> GetChoiceByIdAsync(int id);
        Task AddChoiceAsync(Choice choice);
        Task UpdateChoiceAsync(Choice choice);
        Task DeleteChoiceAsync(Choice choice);
    }
}
