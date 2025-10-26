using Core.Utilities.Results;
using Entity.Concrete;
using Entity.DTOs;
using System.Collections.Generic;

namespace Business.Abstract
{
    /// <summary>
    /// YasirSharp AI - Assistant Service Interface
    /// </summary>
    public interface IAssistantService
    {
        /// <summary>
        /// Kullanıcı sorusuna akıllı cevap üret (Ana endpoint)
        /// </summary>
        IDataResult<BotResponseDto> AskQuestion(AskQuestionDto dto);
        
        /// <summary>
        /// Kullanıcı-bot etkileşimini logla
        /// </summary>
        IResult LogInteraction(LogInteractionDto dto);
        
        /// <summary>
        /// Kullanıcının etkileşim geçmişini getir
        /// </summary>
        IDataResult<List<AssistantInteraction>> GetUserHistory(int userId, int limit = 10);
        
        /// <summary>
        /// Sayfa rehberini getir (dil bazlı)
        /// </summary>
        IDataResult<PageGuideDto> GetPageGuide(string pageName, string language = "tr");
        
        /// <summary>
        /// Hızlı aksiyonları getir (rol ve dil bazlı)
        /// </summary>
        IDataResult<List<QuickActionDto>> GetQuickActions(string role, string language = "tr");
        
        /// <summary>
        /// Analytics istatistikleri (Admin için)
        /// </summary>
        IDataResult<AnalyticsDto> GetAnalytics();
        
        /// <summary>
        /// Kullanıcı geri bildirimi kaydet (thumbs up/down veya hata bildirimi)
        /// </summary>
        IResult SubmitFeedback(SubmitFeedbackDto dto);
        
        /// <summary>
        /// Hata bildirimleri listesini getir (Admin için)
        /// </summary>
        IDataResult<List<AssistantInteraction>> GetErrorReports(int skip = 0, int take = 50);
    }
}
