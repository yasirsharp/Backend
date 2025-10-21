using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using Entity.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IDersService
    {
        IDataResult<Ders> GetById(int dersId);
        IDataResult<List<Ders>> GetList(Expression<Func<Ders, bool>> filter = null);
        IResult Add(Ders ders);
        IResult Delete(Ders ders);
        IResult Update(Ders ders);
        IResult AddDersWithBolumler(DersEkleDTO dersEkleDto);
        IResult UpdateDersWithBolumler(DersGuncelleDTO dersGuncelleDto);
        IDataResult<List<DersWithBolumlerDTO>> GetAllWithBolumler();
        IDataResult<bool> IsKodUnique(string kod, int? excludeDersId = null);
        IDataResult<PagedResult<Ders>> GetPagedList(PaginationParams paginationParams);
    }
}
