using BaretStoreWebAPI.DTO;
using BaretStoreWebAPI.Models;
using Microsoft.EntityFrameworkCore;


namespace BaretStoreWebAPI.DAL
{
    public interface IEbook : ICrud<Ebook>
    {
        Task<Ebook[]> InsertMultiple(Ebook[] obj);
        Task<IEnumerable<Ebook>> GetAllOrderByName();
        Task<IEnumerable<Ebook>> GetAllOrderBySKU();
        Task<Ebook> DisableEbook(int sku);
        Task<Ebook> EnableEbook(int sku);
        Task<Ebook> UpdateEbook(Ebook Ebook);
        Task<List<Ebook>> GetsBySKU(int[] obj);
        Task<IEnumerable<Ebook>> CheckDuplicate(Ebook[] obj);
        Task<IEnumerable<Ebook>> CheckDuplicateSKUButDisable(Ebook[] obj);
        Task<List<int>> CheckSKU(int[] obj);

    }

    public class EbookDAL : IEbook
    {
        private readonly DataContext _context;

        public EbookDAL(DataContext context)
        {
            _context = context;
        }
        #region unused
        public async Task<IEnumerable<Ebook>> GetAll()
        {
            return null;
        }
        public async Task<IEnumerable<Ebook>> GetByName(string name)
        {
            try
            {
                var result = await _context.Ebooks.Where(x => x.EbookName.Contains(name)).ToListAsync();
                if (result.Count <= 0) throw new Exception($"Error : Buku dengan nama {name} tidak ditemukan");
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public Task<Ebook> GetById(int id)
        {
            return GetById(id);
        }
        public Task<Ebook> Update(Ebook obj)
        {
            return Update(obj);
        }
        public Task Delete(int id)
        {
            return Delete(id);
        }
        #endregion

        public async Task<IEnumerable<Ebook>> GetAllOrderByName()
        {
            try
            {
                var results = await _context.Ebooks.OrderBy(x => x.EbookName).Where(x => x.RowStatus == 0).ToListAsync();
                return results;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public async Task<IEnumerable<Ebook>> GetAllOrderBySKU()
        {
            try
            {
                var results = await _context.Ebooks.OrderBy(x => x.SKU).Where(x => x.RowStatus == 0).ToListAsync();
                if (!(results.Count > 0))
                    throw new Exception("Error: Belum ada data");
                return results;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public async Task<Ebook> Insert(Ebook obj)
        {
            try
            {
                var result = await _context.Ebooks.FirstOrDefaultAsync(x => x.SKU == obj.SKU);
                if (result != null) throw new Exception($"Error: SKU {obj.SKU} - Sudah Ada \neBook Name: {obj.EbookName}");
                _context.Ebooks.Add(obj);
                await _context.SaveChangesAsync();
                return obj;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        
        public async Task<Ebook[]> InsertMultiple(Ebook[] obj)
        {
            try
            {
                foreach (var item in obj)
                {
                   var result = await _context.Ebooks.FirstOrDefaultAsync(x => x.SKU == item.SKU);
                   if (result != null) throw new Exception($"Error: SKU {item.SKU} - Sudah Ada \neBook Name: {item.EbookName}");
                   _context.Ebooks.Add(item);                    
                }
                await _context.SaveChangesAsync();
                return obj;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message );
            }            
        }
        public async Task<Ebook> DisableEbook(int sku)
        {
            try
            {
                var check = await _context.Ebooks.FirstOrDefaultAsync(x => x.SKU == sku && x.RowStatus == 0);
                if (check == null) throw new Exception($"Error: Data Tidak Ditemukan");
                check.RowStatus = 1;
                await _context.SaveChangesAsync();
                return check;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public async Task<Ebook> EnableEbook(int sku)
        {
            try
            {
                var check = await _context.Ebooks.FirstOrDefaultAsync(x => x.SKU == sku && x.RowStatus == 1);
                if (check == null) throw new Exception($"Error: Data Tidak ditemukan");
                check.RowStatus = 0;
                await _context.SaveChangesAsync();
                return check;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public async Task<Ebook> UpdateEbook(Ebook Ebook)
        {
            try
            {
                var edit = await _context.Ebooks.FirstOrDefaultAsync(c => c.SKU == Ebook.SKU && Ebook.RowStatus == 0);
                if (edit == null)
                    throw new Exception($"Error: Data Tidak ditemukan");
                if (!string.IsNullOrEmpty(Ebook.EbookName)) edit.EbookName = Ebook.EbookName;
                if (!string.IsNullOrEmpty(Ebook.EbookLinkPDF)) edit.EbookLinkPDF = Ebook.EbookLinkPDF;
                await _context.SaveChangesAsync();
                return edit;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<Ebook>> GetsBySKU(int[] obj)
        {
            try
            {
                List<Ebook> eBooks = new List<Ebook>();
                foreach (var item in obj)
                {
                    var result = await _context.Ebooks.FirstOrDefaultAsync(x => x.SKU == item && x.RowStatus == 0);
                    if (result != null)
                    {
                        if (eBooks.Contains(eBooks.FirstOrDefault(x => x.SKU == item))) eBooks.Remove(result);
                        eBooks.Add(result);                       
                    }
                }
                return eBooks;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        } 
        public async Task<List<int>> CheckSKU(int[] obj)
        {
            try
            {
                List<int> sku = new List<int>();
                foreach (var item in obj)
                {
                    var result = await _context.Ebooks.FirstOrDefaultAsync(x => x.SKU == item && x.RowStatus == 0);
                    if (result == null)
                    {
                        if (sku.Contains( item)) sku.Remove(item);
                        sku.Add(item);                       
                    }
                }
                return sku;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<IEnumerable<Ebook>> CheckDuplicate(Ebook[] obj)
        {
            try
            {
                List<Ebook> emails = new List<Ebook>();
                foreach (var item in obj)
                {
                    var result = await _context.Ebooks.FirstOrDefaultAsync(x => x.SKU == item.SKU);
                    if (result != null) emails.Add(result);
                }
                return emails;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<IEnumerable<Ebook>> CheckDuplicateSKUButDisable(Ebook[] obj)
        {
            try
            {
                List<Ebook> emails = new List<Ebook>();
                foreach (var item in obj)
                {
                    var result = await _context.Ebooks.FirstOrDefaultAsync(x => x.SKU == item.SKU && x.RowStatus == 1);
                    if (result != null) emails.Add(result);
                }
                return emails;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
