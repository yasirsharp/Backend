using Business.Abstract;
using Business.Constants;
using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using DataAccess.Abstract;
using Entity.Concrete;
using System.Linq.Expressions;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Core.CrossCuttingConcerns.Validation;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.Business;
using Business.BusinessAspects.Autofac;
using Core.Aspects.Autofac.Caching;

namespace Business.Concrete
{
    public class AkademikPersonelManager : IAkademikPersonelService
    {
        private readonly IAkademikPersonelDal _akademikPersonelDal;
        IDBAPDal _dBAPDal;
        ISinavDetayDal _sinavDetayDal;

        public AkademikPersonelManager(IAkademikPersonelDal akademikPersonelDal, IDBAPDal dBAPDal,ISinavDetayDal sinavDetayDal)
        {
            _akademikPersonelDal = akademikPersonelDal;
            _dBAPDal = dBAPDal;
            _sinavDetayDal = sinavDetayDal;
        }

        //[SecuredOperation("akademik.personel")]
        [ValidationAspect(typeof(AkademikPersonelValidator))]
        public IResult Add(AkademikPersonel akademikPersonel)
        {
            _akademikPersonelDal.AddAkademikPersonelWithUserOperationClaim(akademikPersonel);
            return new SuccessResult(Messages.AkademikPersonelAdded);
        }

        private IResult CheckIfPersonelExists(int akademikPersonelId)
        {
            var result = _akademikPersonelDal.Get(q => q.Id == akademikPersonelId);
            if (result == null)
            {
                return new ErrorResult(Messages.AkademikPersonelNotFound);
            }
            return new SuccessResult();
        }


        public IResult Delete(AkademikPersonel akademikPersonel)
        {
            IResult result = BusinessRules.Run(CheckIfPersonelExists(akademikPersonel.Id),
                EslesmeSorgusu(akademikPersonel));

            if (result != null)
            {
                return result;
            }
            _akademikPersonelDal.DeleteAkademikPersonelWithUserOperationClaim(akademikPersonel);
            return new SuccessResult(Messages.AkademikPersonelDeleted);
        }

        private IResult EslesmeSorgusu(AkademikPersonel akademikPersonel)
        {
            var dbap = _dBAPDal.GetDetails(q => q.AkademikPersonelId == akademikPersonel.Id);
            if (dbap.Count > 0)
            {
                string message = $"{akademikPersonel.Ad} için {dbap.Count} tane Bölüm-Ders-Akademik Personel Eşleştirmesi bulunmaktadır.\n";
                foreach (var item in dbap)
                {
                    message += $"{item.BolumAd} \n{item.DersAd} \n{item.AkademikPersonelAd} ({item.Unvan})\n";
                }
                return new ErrorResult(message);
            }
            return new SuccessResult();
        }

        public IDataResult<AkademikPersonel> GetById(int akademikPeronelId)
        {
            return new SuccessDataResult<AkademikPersonel>(_akademikPersonelDal.Get(q => q.Id == akademikPeronelId));
        }

        public IDataResult<List<AkademikPersonel>> GetList(Expression<Func<AkademikPersonel, bool>> filter = null)
        {
            return new SuccessDataResult<List<AkademikPersonel>>(_akademikPersonelDal.GetAll(filter), $"{_akademikPersonelDal.GetAll(filter).Count} tane bulundu.");
        }

        public IDataResult<PagedResult<AkademikPersonel>> GetPagedList(PaginationParams paginationParams)
        {
            var searchTerm = paginationParams.SearchTerm?.ToLower();
            Expression<Func<AkademikPersonel, bool>> filter = null;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filter = ap => ap.Ad.ToLower().Contains(searchTerm);
            }

            var pagedResult = _akademikPersonelDal.GetPaged(paginationParams, filter);
            return new SuccessDataResult<PagedResult<AkademikPersonel>>(pagedResult, $"Toplam {pagedResult.TotalCount} akademik personel bulundu.");
        }

        public IResult Update(AkademikPersonel akademikPersonel)
        {
            _akademikPersonelDal.UpdateAkademikPersonelWithUserOperationClaim(akademikPersonel);
            return new SuccessResult(Messages.AkademikPersonelUpdated);
        }
        
    }
}
