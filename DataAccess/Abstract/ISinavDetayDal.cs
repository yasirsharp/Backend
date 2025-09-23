using Core.DataAccess;
using Core.Entities;
using Entity.Concrete;
using Entity.DTOs;
using System;
using System.Linq.Expressions;

namespace DataAccess.Abstract
{
    public interface ISinavDetayDal:IEntityRepository<SinavDetay>
    {
        List<SinavDetayDTO> GetByDerslikler(int[] derslikIds);
        List<SinavDetayDTO> GetByDersliklerAndBolum(int[] derslikIds, int bolumId);
        List<SinavDetayDTO> GetByDersliklerAndAkademikPersonel(int[] derslikIds, int akademikPersonelId);
        List<SinavDetayDTO> GetByBolumId(int bolumId);
        List<SinavDetayDTO> GetByDerslikId(int derslikId);
        List<SinavDetayDTO> GetByAkademikPersonelId(int akademikPersonelId);
        List<SinavDetayDTO> GetByDBAPId(int dbapId);
        List<SinavDetayDTO> GetDetails();
        List<SinavDetayDTO> GetSinavDetailsByDateRange(DateTime startDate, DateTime endDate);
        List<SinavDetayDTO> GetSinavDetailsByDateRangeAndBolum(DateTime startDate, DateTime endDate, int bolumId);
        List<SinavDetayDTO> GetSinavDetailsByDateRangeAndDerslik(DateTime startDate, DateTime endDate, int derslikId);
        List<SinavDetayDTO> GetSinavDetailsByDateRangeAndAkademikPersonel(DateTime startDate, DateTime endDate, int akademikPersonelId);
        SinavDetayDTO GetDetail(int sinavDetayId);
        SinavDetay ExistSinav(List<int> derslikIdleri, List<int> gozetmenIdleri, int akademikPersonelId, TimeOnly SinavBaslangicSaati, TimeOnly SinavBitisSaati, DateTime sinavTarihi);
        void AddWithTransaction(SinavKayitDTO sinavKayitDTO);
        void UpdateWithTransaction(SinavGuncelleDTO sinavKayitDTO);
        void DeleteWithTransaction(SinavDetay sinavDetay);
    }
}
