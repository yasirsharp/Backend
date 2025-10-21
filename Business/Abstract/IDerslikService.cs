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
    public interface IDerslikService
    {
        IDataResult<List<Derslik>> GetList();
        IDataResult<Derslik> GetById(int derslikId);
        IResult Add(Derslik derslik);
        IResult Delete(Derslik derslik);
        IResult Update(Derslik derslik);
        IResult AddDerslikWithBolumler(DerslikEkleDTO derslikEkleDto);
        IResult UpdateDerslikWithBolumler(DerslikGuncelleDTO derslikGuncelleDto);
        IDataResult<List<DerslikWithBolumlerDTO>> GetAllWithBolumler();
        IDataResult<PagedResult<Derslik>> GetPagedList(PaginationParams paginationParams);
    }
}
