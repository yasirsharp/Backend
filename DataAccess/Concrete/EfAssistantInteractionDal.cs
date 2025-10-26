using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entity.Concrete;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess.Concrete
{
    /// <summary>
    /// YasirSharp AI - AssistantInteraction EF Implementation
    /// </summary>
    public class EfAssistantInteractionDal : EfEntityRepositoryBase<AssistantInteraction, DuzceUniversiteContext>, IAssistantInteractionDal
    {
        public List<AssistantInteraction> GetUserHistory(int userId, int limit = 10)
        {
            using (var context = new DuzceUniversiteContext())
            {
                return context.AssistantInteractions
                    .Where(x => x.UserId == userId && x.Status == true)
                    .OrderByDescending(x => x.Timestamp)
                    .Take(limit)
                    .ToList();
            }
        }

        public List<AssistantInteraction> GetByPageContext(string pageContext)
        {
            using (var context = new DuzceUniversiteContext())
            {
                return context.AssistantInteractions
                    .Where(x => x.PageContext == pageContext && x.Status == true)
                    .OrderByDescending(x => x.Timestamp)
                    .ToList();
            }
        }

        public List<AssistantInteraction> GetPopularQuestions(int limit = 10)
        {
            using (var context = new DuzceUniversiteContext())
            {
                // En çok sorulan soruları grupla ve say
                return context.AssistantInteractions
                    .Where(x => x.Status == true)
                    .GroupBy(x => x.Question)
                    .OrderByDescending(g => g.Count())
                    .Take(limit)
                    .SelectMany(g => g.Take(1)) // Her gruptan bir örnek al
                    .ToList();
            }
        }
    }
}
