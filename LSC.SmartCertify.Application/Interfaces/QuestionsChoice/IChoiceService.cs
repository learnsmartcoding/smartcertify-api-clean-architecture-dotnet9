using LSC.SmartCertify.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.Interfaces.QuestionsChoice
{
    public interface IChoiceService
    {
        Task<IEnumerable<ChoiceDto>> GetAllChoicesAsync(int questionId);
        Task<ChoiceDto?> GetChoiceByIdAsync(int choiceId);
        Task AddChoiceAsync(CreateChoiceDto dto);
        Task UpdateChoiceAsync(int choiceId, UpdateChoiceDto dto);
        Task UpdateUserChoiceAsync(int choiceId, UpdateUserChoice dto);
        Task DeleteChoiceAsync(int choiceId);
    }

}
