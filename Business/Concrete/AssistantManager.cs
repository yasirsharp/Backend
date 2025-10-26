using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.Concrete;
using Entity.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Concrete
{
    /// <summary>
    /// YasirSharp AI - Assistant Service Implementation
    /// </summary>
    public class AssistantManager : IAssistantService
    {
        private readonly IAssistantInteractionDal _assistantInteractionDal;
        private readonly IUserAssistantPreferenceDal _userAssistantPreferenceDal;

        public AssistantManager(
            IAssistantInteractionDal assistantInteractionDal,
            IUserAssistantPreferenceDal userAssistantPreferenceDal)
        {
            _assistantInteractionDal = assistantInteractionDal;
            _userAssistantPreferenceDal = userAssistantPreferenceDal;
        }

        public IDataResult<Entity.DTOs.BotResponseDto> AskQuestion(AskQuestionDto dto)
        {
            // 1. Intent Detection (Niyet Tespiti)
            var intent = BotRules.DetectIntent(dto.Question);
            
            // 2. Generate Smart Answer (Akıllı Cevap Üret)
            var botResponse = BotRules.GenerateAnswer(intent, dto.Question, dto.PageContext, dto.UserRole);
            botResponse.DetectedIntent = intent;
            
            // 3. Save interaction and get ID (Etkileşimi kaydet ve ID'yi al)
            var interaction = new AssistantInteraction
            {
                UserId = dto.UserId,
                Question = dto.Question,
                Answer = botResponse.Answer,
                PageContext = dto.PageContext,
                FeatureUsed = intent,
                Language = dto.Language ?? "tr",
                Timestamp = DateTime.Now,
                CreatedDate = DateTime.Now,
                Status = true
            };

            _assistantInteractionDal.Add(interaction);
            
            // 4. Set InteractionId in response (Feedback için gerekli)
            botResponse.InteractionId = interaction.Id;
            
            // 5. Update Last Interaction Date
            var preference = _userAssistantPreferenceDal.GetByUserId(dto.UserId);
            if (preference != null)
            {
                preference.LastInteractionDate = DateTime.Now;
                _userAssistantPreferenceDal.Update(preference);
            }
            
            return new SuccessDataResult<Entity.DTOs.BotResponseDto>(botResponse, "Bot yanıtı başarıyla oluşturuldu.");
        }

        public IResult LogInteraction(LogInteractionDto dto)
        {
            // ⚠️ Frontend'den gelen answer'ı değil, kendi generate ettiğimiz answer'ı kullan
            // Bu yüzden dto.Answer'ı kullanmıyoruz, intent detection yapıp cevap üretiyoruz
            
            // 1. Intent Detection (Niyet Tespiti)
            var intent = BotRules.DetectIntent(dto.Question);
            
            // 2. Generate Smart Answer (Akıllı Cevap Üret)
            var userRole = "student"; // TODO: Get from AuthContext or UserService
            var botResponse = BotRules.GenerateAnswer(intent, dto.Question, dto.PageContext, userRole);
            
            // 3. Save Interaction (Etkileşimi Kaydet)
            var interaction = new AssistantInteraction
            {
                UserId = dto.UserId,
                Question = dto.Question,
                Answer = botResponse.Answer, // Backend'in ürettiği akıllı cevap
                PageContext = dto.PageContext,
                FeatureUsed = intent, // Tespit edilen intent'i feature olarak kaydet
                Language = dto.Language ?? "tr",
                Timestamp = DateTime.Now
            };

            _assistantInteractionDal.Add(interaction);
            
            // 4. Update Last Interaction Date (Son etkileşim tarihini güncelle)
            var preference = _userAssistantPreferenceDal.GetByUserId(dto.UserId);
            if (preference != null)
            {
                preference.LastInteractionDate = DateTime.Now;
                _userAssistantPreferenceDal.Update(preference);
            }

            return new SuccessResult(Messages.InteractionLogged);
        }

        public IDataResult<List<AssistantInteraction>> GetUserHistory(int userId, int limit = 10)
        {
            var history = _assistantInteractionDal.GetUserHistory(userId, limit);
            return new SuccessDataResult<List<AssistantInteraction>>(history);
        }

        public IDataResult<PageGuideDto> GetPageGuide(string pageName, string language = "tr")
        {
            // TODO: Bu data constant'lardan gelecek (pageGuides.ts benzeri)
            // Şimdilik hardcoded örnek
            var guide = GetPageGuideData(pageName, language);
            
            if (guide == null)
            {
                return new ErrorDataResult<PageGuideDto>(Messages.PageGuideNotFound);
            }

            return new SuccessDataResult<PageGuideDto>(guide);
        }

        public IDataResult<List<QuickActionDto>> GetQuickActions(string role, string language = "tr")
        {
            // TODO: Bu data constant'lardan gelecek (quickActions.ts benzeri)
            // Şimdilik hardcoded örnek
            var actions = GetQuickActionsData(role, language);
            return new SuccessDataResult<List<QuickActionDto>>(actions);
        }

        public IDataResult<AnalyticsDto> GetAnalytics()
        {
            var totalInteractions = _assistantInteractionDal.GetAll(x => x.Status == true).Count;
            var totalUsers = _assistantInteractionDal.GetAll(x => x.Status == true)
                .Select(x => x.UserId)
                .Distinct()
                .Count();

            // En çok sorulan sorular
            var popularQuestions = _assistantInteractionDal.GetAll(x => x.Status == true)
                .GroupBy(x => x.Question)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new PopularQuestionDto
                {
                    Question = g.Key,
                    Count = g.Count()
                })
                .ToArray();

            // En çok kullanılan sayfalar
            var popularPages = _assistantInteractionDal.GetAll(x => x.Status == true)
                .GroupBy(x => x.PageContext)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new PageUsageDto
                {
                    PageName = g.Key,
                    Count = g.Count()
                })
                .ToArray();

            // En çok kullanılan özellikler
            var popularFeatures = _assistantInteractionDal.GetAll(x => x.Status == true && x.FeatureUsed != null)
                .GroupBy(x => x.FeatureUsed)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new FeatureUsageDto
                {
                    FeatureName = g.Key,
                    Count = g.Count()
                })
                .ToArray();

            var analytics = new AnalyticsDto
            {
                TotalInteractions = totalInteractions,
                TotalUsers = totalUsers,
                PopularQuestions = popularQuestions,
                PopularPages = popularPages,
                PopularFeatures = popularFeatures
            };

            return new SuccessDataResult<AnalyticsDto>(analytics);
        }

        public IResult SubmitFeedback(SubmitFeedbackDto dto)
        {
            var interaction = _assistantInteractionDal.Get(x => x.Id == dto.InteractionId);
            
            if (interaction == null)
            {
                return new ErrorResult("Etkileşim bulunamadı.");
            }

            // Geri bildirimi kaydet
            interaction.IsHelpful = dto.IsHelpful;
            interaction.ErrorReport = dto.ErrorReport;
            interaction.FeedbackTimestamp = DateTime.Now;
            interaction.UpdatedDate = DateTime.Now;

            _assistantInteractionDal.Update(interaction);

            return new SuccessResult("Geri bildiriminiz kaydedildi. Teşekkür ederiz! 🙏");
        }

        public IDataResult<List<AssistantInteraction>> GetErrorReports(int skip = 0, int take = 50)
        {
            // Hata bildirimi yapılmış etkileşimleri getir
            var errorReports = _assistantInteractionDal
                .GetAll(x => x.Status == true && x.ErrorReport != null && x.ErrorReport != "")
                .OrderByDescending(x => x.FeedbackTimestamp)
                .Skip(skip)
                .Take(take)
                .ToList();

            return new SuccessDataResult<List<AssistantInteraction>>(
                errorReports, 
                $"{errorReports.Count} hata bildirimi bulundu."
            );
        }

        #region Helper Methods (Hardcoded Data - TODO: Move to constants)

        private PageGuideDto GetPageGuideData(string pageName, string language)
        {
            // Türkçe page guides
            var guidesTR = new Dictionary<string, PageGuideDto>
            {
                ["dashboard"] = new PageGuideDto
                {
                    PageName = "dashboard",
                    Title = "Dashboard",
                    Description = "Sistem özeti ve istatistiklerini görüntüleyin.",
                    Features = new[] { "Toplam kullanıcı sayısı", "Toplam sınav sayısı", "Son aktiviteler", "Hızlı erişim butonları" },
                    Tips = new[] { "Dashboard'da güncel istatistikleri takip edebilirsiniz", "Hızlı erişim butonları ile sık kullanılan sayfalara kolayca gidebilirsiniz" }
                },
                ["calendar"] = new PageGuideDto
                {
                    PageName = "calendar",
                    Title = "Sınav Takvimi",
                    Description = "Sınavları takvim üzerinde görüntüleyin ve yönetin.",
                    Features = new[] { "Sınav ekleme", "Sınav düzenleme", "Sınav silme", "Drag & Drop ile ders ekleme", "Filtreleme ve arama" },
                    Tips = new[] { "Sidebar'dan dersleri sürükleyerek hızlıca sınav ekleyebilirsiniz", "Takvim üzerinde sınavları sürükleyerek tarih değiştirebilirsiniz" }
                },
                ["announcements"] = new PageGuideDto
                {
                    PageName = "announcements",
                    Title = "Duyurular",
                    Description = "Sistem duyurularını görüntüleyin.",
                    Features = new[] { "Aktif duyurular", "Süresi dolmuş duyurular", "Duyuru filtreleme", "Okundu işaretleme" },
                    Tips = new[] { "Önemli duyuruları kaçırmamak için düzenli kontrol edin", "Duyuruları okuduktan sonra 'Okundu' olarak işaretleyebilirsiniz" }
                }
            };

            // İngilizce page guides (TODO: Implement)
            var guidesEN = new Dictionary<string, PageGuideDto>();

            var guides = language == "tr" ? guidesTR : guidesEN;
            return guides.ContainsKey(pageName) ? guides[pageName] : null;
        }

        private List<QuickActionDto> GetQuickActionsData(string role, string language)
        {
            // Türkçe quick actions
            var actionsTR = new List<QuickActionDto>
            {
                new QuickActionDto { Id = "dashboard", Label = "Dashboard", Icon = "layout-dashboard", Path = "/dashboard", Description = "Sistem özetini görüntüle" },
                new QuickActionDto { Id = "calendar", Label = "Sınav Takvimi", Icon = "calendar", Path = "/calendar", Description = "Sınavları görüntüle ve yönet" },
                new QuickActionDto { Id = "announcements", Label = "Duyurular", Icon = "megaphone", Path = "/announcements", Description = "Sistem duyurularını oku" }
            };

            // Admin için ek aksiyonlar
            if (role == "admin" || role == "super.admin")
            {
                actionsTR.AddRange(new[]
                {
                    new QuickActionDto { Id = "users", Label = "Kullanıcı Yönetimi", Icon = "users", Path = "/admin/users", Description = "Kullanıcıları yönet" },
                    new QuickActionDto { Id = "bolumler", Label = "Bölüm Yönetimi", Icon = "building", Path = "/admin/bolumler", Description = "Bölümleri yönet" }
                });
            }

            // İngilizce quick actions (TODO: Implement)
            var actionsEN = new List<QuickActionDto>();

            return language == "tr" ? actionsTR : actionsEN;
        }

        #endregion
    }
}
