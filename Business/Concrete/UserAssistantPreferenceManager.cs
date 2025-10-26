using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entity.Concrete;
using System;

namespace Business.Concrete
{
    /// <summary>
    /// YasirSharp AI - User Preference Service Implementation
    /// </summary>
    public class UserAssistantPreferenceManager : IUserAssistantPreferenceService
    {
        private readonly IUserAssistantPreferenceDal _userAssistantPreferenceDal;

        public UserAssistantPreferenceManager(IUserAssistantPreferenceDal userAssistantPreferenceDal)
        {
            _userAssistantPreferenceDal = userAssistantPreferenceDal;
        }

        public IDataResult<UserAssistantPreference> GetPreference(int userId)
        {
            var preference = _userAssistantPreferenceDal.GetByUserId(userId);
            
            // Eğer kullanıcının tercihi yoksa, default oluştur
            if (preference == null)
            {
                CreateDefaultPreference(userId);
                preference = _userAssistantPreferenceDal.GetByUserId(userId);
            }

            return new SuccessDataResult<UserAssistantPreference>(preference);
        }

        public IResult UpdatePreference(UserAssistantPreference preference)
        {
            var existingPreference = _userAssistantPreferenceDal.GetByUserId(preference.UserId);
            
            if (existingPreference == null)
            {
                return new ErrorResult(Messages.PreferenceNotFound);
            }

            // Güncelle
            existingPreference.IsEnabled = preference.IsEnabled;
            existingPreference.HasCompletedOnboarding = preference.HasCompletedOnboarding;
            existingPreference.PreferredLanguage = preference.PreferredLanguage;
            existingPreference.LastInteractionDate = preference.LastInteractionDate;

            _userAssistantPreferenceDal.Update(existingPreference);
            return new SuccessResult(Messages.PreferenceUpdated);
        }

        public IResult CompleteOnboarding(int userId)
        {
            var preference = _userAssistantPreferenceDal.GetByUserId(userId);
            
            if (preference == null)
            {
                return new ErrorResult(Messages.PreferenceNotFound);
            }

            preference.HasCompletedOnboarding = true;
            _userAssistantPreferenceDal.Update(preference);
            
            return new SuccessResult(Messages.OnboardingCompleted);
        }

        public IResult ToggleBot(int userId, bool isEnabled)
        {
            var preference = _userAssistantPreferenceDal.GetByUserId(userId);
            
            if (preference == null)
            {
                return new ErrorResult(Messages.PreferenceNotFound);
            }

            preference.IsEnabled = isEnabled;
            _userAssistantPreferenceDal.Update(preference);
            
            return new SuccessResult(isEnabled ? Messages.BotEnabled : Messages.BotDisabled);
        }

        public IResult CreateDefaultPreference(int userId)
        {
            var preference = new UserAssistantPreference
            {
                UserId = userId,
                IsEnabled = true,
                HasCompletedOnboarding = false,
                PreferredLanguage = "tr",
                LastInteractionDate = null
            };

            _userAssistantPreferenceDal.Add(preference);
            return new SuccessResult(Messages.PreferenceCreated);
        }
    }
}
