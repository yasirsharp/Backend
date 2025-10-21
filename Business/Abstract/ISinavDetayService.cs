using Core.Utilities.Results;
using Core.Utilites.Results.Pagination;
using Entity.Concrete;
using Entity.DTOs;
using System;

namespace Business.Abstract
{
    public interface ISinavDetayService
    {
        IDataResult<List<SinavDetay>> GetAll();
        IDataResult<List<SinavDetayDTO>> GetByBolumId(int bolumId);
        IDataResult<List<SinavDetayDTO>> GetByDerslikId(int derslikId);
        IDataResult<List<SinavDetayDTO>> GetByAkademikPersonelId(int akademikPersonelId);
        IDataResult<List<SinavDetayDTO>> GetByDBAPId(int akademikPersonelId);
        IDataResult<List<SinavDetayDTO>> GetByDerslikler(int[] derslikIds);
        IDataResult<List<SinavDetayDTO>> GetByDersliklerAndBolum(int[] derslikIds, int bolumId);
        IDataResult<List<SinavDetayDTO>> GetByDersliklerAndAkademikPersonel(int[] derslikIds, int akademikPersonelId);
        IDataResult<List<SinavDetayDTO>> GetAllDetails();
        IDataResult<SinavDetayDTO> GetById(int sinavDetayId);
        IDataResult<List<SinavDetayDTO>> GetByDateRange(DateTime startDate, DateTime endDate);
        IDataResult<List<SinavDetayDTO>> GetByDateRangeAndBolum(DateTime startDate, DateTime endDate, int bolumId);
        IDataResult<List<SinavDetayDTO>> GetByDateRangeAndDerslik(DateTime startDate, DateTime endDate, int derslikId);
        IDataResult<List<SinavDetayDTO>> GetByDateRangeAndAkademikPersonel(DateTime startDate, DateTime endDate, int akademikPersonelId);
        IResult Add(SinavKayitDTO sinavKayitDTO);
        IResult Delete(SinavDetay sinavDetay);
        IResult Update(SinavGuncelleDTO sinavGuncelleDTO);
        IDataResult<PagedResult<SinavDetay>> GetPagedList(PaginationParams paginationParams);
    }
}
