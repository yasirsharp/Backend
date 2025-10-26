using Core.DataAccess;
using Entity.Concrete;
using System.Collections.Generic;

namespace DataAccess.Abstract
{
    /// <summary>
    /// YasirSharp AI - AssistantInteraction Data Access Layer
    /// </summary>
    public interface IAssistantInteractionDal : IEntityRepository<AssistantInteraction>
    {
        /// <summary>
        /// Kullanıcının son N adet etkileşimini getirir
        /// </summary>
        List<AssistantInteraction> GetUserHistory(int userId, int limit = 10);
        
        /// <summary>
        /// Belirli bir sayfadaki tüm etkileşimleri getirir (Analytics için)
        /// </summary>
        List<AssistantInteraction> GetByPageContext(string pageContext);
        
        /// <summary>
        /// En çok sorulan soruları getirir (Analytics için)
        /// </summary>
        List<AssistantInteraction> GetPopularQuestions(int limit = 10);
    }
}
