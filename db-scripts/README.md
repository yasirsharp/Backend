# 📁 Database Scripts

Bu klasör veritabanı migration scriptlerini içerir.

## 📄 Dosyalar

### `add-audit-fields.sql`
**Amaç**: Tüm tablolara audit (denetim) alanları ekler  
**Tarih**: 2025-10-20

**Eklenen Alanlar**:
- `CreatedDate` (DATETIME NOT NULL) - Kaydın oluşturulma zamanı
- `UpdatedDate` (DATETIME NULL) - Kaydın son güncellenme zamanı

**Etkilenen Tablolar**:
1. Users
2. Bolum
3. Ders
4. Derslik
5. AkademikPersonel
6. SinavDetay
7. DersBolumAkademikPersonel
8. BolumAkademikPersoneller
9. DersBolum
10. DerslikBolum
11. SinavDerslik
12. OperationClaims
13. UserOperationClaims

**Nasıl Çalıştırılır**:
```sql
-- SQL Server Management Studio'da veya Azure Data Studio'da:
-- 1. add-audit-fields.sql dosyasını aç
-- 2. İlk satırdaki database adını kontrol et (DuzceUniversiteSinavTakvimiDB)
-- 3. Execute et (F5)
```

**Güvenlik**:
- Script idempotent'tir (birden fazla çalıştırılabilir)
- IF NOT EXISTS kontrolü yapar, mevcut alanları etkilemez
- Mevcut veriler için CreatedDate = GETDATE() olarak ayarlanır

---

## 🏗️ Backend Değişiklikleri

Audit alanları backend'de otomatik yönetilir:

### IEntity.cs
```csharp
public interface IEntity
{
    DateTime CreatedDate { get; set; }
    DateTime? UpdatedDate { get; set; }
}
```

### EfEntityRepositoryBase.cs
```csharp
public void Add(TEntity entity)
{
    entity.CreatedDate = DateTime.Now; // ✅ Otomatik
    // ... Add logic
}

public void Update(TEntity entity)
{
    entity.UpdatedDate = DateTime.Now; // ✅ Otomatik
    // ... Update logic
}
```

**Avantajlar**:
- ✅ Developer'lar CreatedDate/UpdatedDate'i manuel set etmek zorunda değil
- ✅ Tüm entity'ler otomatik olarak audit özelliği kazanır
- ✅ Tek merkezi yönetim (EfEntityRepositoryBase)

---

## 📋 Migration Checklist

- [x] IEntity interface'ine CreatedDate/UpdatedDate eklendi
- [x] EfEntityRepositoryBase'e Add/Update interceptor eklendi
- [x] Tüm Entity class'larına audit properties eklendi
- [ ] SQL migration script'i çalıştırıldı (add-audit-fields.sql)
- [ ] Testler yapıldı (yeni kayıt oluşturma/güncelleme)
- [ ] Frontend'de audit alanları gösterildi (opsiyonel)

---

## 🔄 Sıradaki Migration

Pagination sistemi eklenecek...
