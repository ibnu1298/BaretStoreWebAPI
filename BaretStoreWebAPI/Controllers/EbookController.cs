using AutoMapper;
using BaretStoreWebAPI;
using BaretStoreWebAPI.DAL;
using BaretStoreWebAPI.DTO;
using BaretStoreWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BaretStoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EbookController : ControllerBase
    {
        private readonly IEbook _ebook;
        private readonly IEMail _email;
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public EbookController(IEbook ebook, IEMail email, IMapper mapper, DataContext context)
        {
            _ebook = ebook;
            _email = email;
            _mapper = mapper;
            _context = context;
        }
        MultipleEbookDTO ebook = new MultipleEbookDTO();
        BaseResponse baseResponse = new BaseResponse();

        [HttpGet("OrderByName")]
        public async Task<ActionResult> GetOrderByName()
        {
            try
            {
                var results = await _ebook.GetAllOrderByName();
                if (results.Count() <= 0)
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = "Belum Ada Data";
                    return StatusCode((int)HttpStatusCode.NotFound, baseResponse);
                }
                if (results != null)
                {
                    ebook.IsSucceeded = true;
                    ebook.Message = $"Berhasil mengambil {results.Count()} Link berdasarkan Nama";
                    ebook.Ebooks = _mapper.Map<List<EbookDTO>>(results);
                }
                return Ok(ebook);

            }
            catch (Exception ex)
            {
                baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpGet("OrderBySKU")]
        public async Task<ActionResult> GetOrderBySKU()
        {
            try
            {
                var results = await _ebook.GetAllOrderBySKU();
                if (results != null)
                {
                    ebook.IsSucceeded = true;
                    ebook.Message = "Berhasil mengambil Link berdasarkan SKU";
                    ebook.Ebooks = _mapper.Map<List<EbookDTO>>(results);
                }
                return Ok(ebook);
            }
            catch (Exception ex) 
            {
                baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("Disable")]
        public async Task<ActionResult> DisableLink(ReadSKU sku)
        {
            try
            {
                var results = await _ebook.DisableEbook(sku.SKU);
                var eBook = _mapper.Map<EbookResponseDTO>(results);
                if (results != null)
                {
                    eBook.IsSucceeded = true;
                    eBook.Message = "Link has been Successfully Disabled";
                }
                return Ok(eBook);
            }
            catch (Exception ex)
            {
                baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("Enable")]
        public async Task<ActionResult> EnableLink(ReadSKU sku)
        {
            try
            {
                var results = await _ebook.EnableEbook(sku.SKU);
                var eBook = _mapper.Map<EbookResponseDTO>(results);
                if(results != null)
                {
                    eBook.IsSucceeded = true;
                    eBook.Message = "Link has been Successfully Activated";
                }
                return Ok(eBook);
            }
            catch(Exception ex)
            {
                baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("AddSingleEBook")]
        public async Task<ActionResult> AddLinkEbook(AddEbookDTO obj)
        {
            try
            {
                var newEbook = _mapper.Map<Ebook>(obj);
                var result = await _ebook.Insert(newEbook);
                var ebook = _mapper.Map<EbookResponseDTO>(result);
                if(result != null)
                {
                    ebook.IsSucceeded = true;
                    ebook.Message = "Link E-Book Berhasil Didaftarkan";
                }

                return Ok(ebook);
            }
            catch (Exception ex)
            {
                baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("AddMultipleEBook")]
        public async Task<ActionResult> AddMultipleLinkEbook(AddMultipleEbookDTO obj)
        {
            try
            {                
                var data = _mapper.Map<Ebook[]>(obj.Ebooks);
                var dupDisable = await _ebook.CheckDuplicateSKUButDisable(data);
                if (dupDisable.Count() > 0)
                {
                    ebook.IsSucceeded = true;
                    ebook.Message = $"Insert Failed, {dupDisable.Count()} Link E-Book is registered but disabled";
                    ebook.Ebooks = _mapper.Map<List<EbookDTO>>(dupDisable);
                    return BadRequest(ebook);
                }
                var duplicate = await _ebook.CheckDuplicate(data);
                if (duplicate.Count() > 0)
                {
                    ebook.IsSucceeded = true;
                    ebook.Message = $"Insert Failed, {duplicate.Count()} Link E-Book already registered";
                    ebook.Ebooks = _mapper.Map<List<EbookDTO>>(duplicate);
                    return BadRequest(ebook);
                }
                if (duplicate != null) { }
                var result = await _ebook.InsertMultiple(data);
                if(result != null)
                {
                    ebook.IsSucceeded = true;
                    ebook.Message = $"{result.Count()} Link E-Book Berhasil Ditambahkan";
                    ebook.Ebooks = _mapper.Map<List<EbookDTO>>(result);
                }
                return Ok(ebook);
            }
            catch (Exception ex)
            {
                baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("GetMultipleEBookBySKU")]
        public async Task<ActionResult> GetMultipleLinkEbookBySKU(SendEbookDTO obj)
        {
            try
            {
                var checkSKU = await _ebook.CheckSKU(obj.SKU);
                string skuNotFound = string.Join(",", checkSKU.ToArray());    
                var result = await _ebook.GetsBySKU(obj.SKU);
                if (result.Count() > 0)
                {
                    ebook.IsSucceeded = true;
                    ebook.Message = checkSKU.Count() > 0 ? $"Berhasil Mengambil {result.Count()} Link E-Book | {skuNotFound}" : $"Berhasil Mengambil {result.Count()} Link E-Book";
                    ebook.Ebooks = _mapper.Map<List<EbookDTO>>(result);
                }
                else
                {
                    baseResponse.IsSucceeded = false;
                    baseResponse.Message = $"Link E-Book Not Found";
                
                    return NotFound(baseResponse);
                }
                return Ok(ebook);
            }
            catch (Exception ex)
            {
                baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("SendEbook")]
        public async Task<ActionResult> SendEbook(SendEmailDTO obj)
        {
            try
            {
               
                CreateEmailDTO createEmailDTO = new CreateEmailDTO();
                obj.NameTo = string.IsNullOrEmpty(obj.NameTo) ? "" : char.ToUpper(obj.NameTo[0]) + obj.NameTo.Substring(1);
                List<string> ebookPDF = new List<string>();
                var checkSKU = await _ebook.CheckSKU(obj.SKU);
                string skuNotFound = string.Join(",", checkSKU.ToArray());
                var result = await _ebook.GetsBySKU(obj.SKU);
                if (result.Count() > 0)
                {                  
                    foreach (var item in result)
                    {
                        ebookPDF.Add($"<div\r\n  style=\"\r\n    width: 396px;\r\n    height: 18px;\r\n    border-radius:10px;\r\n    max-height: 18px;\r\n    background-color: #f5f5f5;\r\n    padding: 5px;\r\n    color: #222;\r\n    font-family: arial;\r\n    font-style: normal;\r\n    font-weight: bold;\r\n    font-size: 13px;\r\n    border: 1px solid #ddd;\r\n    line-height: 1;\r\n  \"\r\n>\r\n  <a\r\n    href=\"{item.EbookLinkPDF}\"\r\n    style=\"\r\n      display: inline-block;\r\n      overflow: hidden;\r\n      text-overflow: ellipsis;\r\n      white-space: nowrap;\r\n      text-decoration: none;\r\n      padding: 1px 0px;\r\n      border: none;\r\n      width: 100%;\r\n    \"\r\n    target=\"_blank\"\r\n  >\r\n    <img\r\n      style=\"vertical-align: bottom; border: none\"\r\n      src=\"https://ci5.googleusercontent.com/proxy/ORvV3xG38euXIc6bFyebnloLAfX2ANOPQZ41RiT31C946RvLzqP0EjQ99QhDAW1g9JhHIshDAV3Nbf5R6QGoKro4lbpVpP9NlapozNV-zeWTf-hQKSoTXog=s0-d-e1-ft#https://drive-thirdparty.googleusercontent.com/16/type/application/pdf\"\r\n    />&nbsp;\r\n    <span style=\"color: #15c; text-decoration: none; vertical-align: bottom\"\r\n      >{item.SKU}. {item.EbookName}</span\r\n    >\r\n  </a>\r\n      \r\n</div>\r\n<br />\r\n");
                    }
                }
                else
                {
                    baseResponse.IsSucceeded = true;
                    baseResponse.Message = $"Link E-Book Not Found";                  
                    return NotFound(baseResponse);
                }
                obj.BodyHTML = $"<div style=\"font-weight: 500;color:#000\">\r\n      <div>\r\n      Hallo Kak{(string.IsNullOrEmpty(obj.NameTo)?"":$" {obj.NameTo}")}, ini aku kirim {result.Count()} E-Book sesuai request yaa\r\n</div><div>\r\n      Semoga suka ya, happy reading dan sehat selalu &#128522;\r\n    </div>\r\n   <br/><div>Tolong untuk chat seller \"Produk Diterima\" ya kak</div>\r\n   <br/>{string.Join("", ebookPDF.ToArray())}<div >\r\n      Jika ada request atau pertanyaan\r\n    </div><div> boleh chat langsung admin Shopee nya yaa\r\n    </div><div >\r\n      Terimakasih &#129303;\r\n    </div>\r\n    </div>";
                #region Check Email
                var checkEmailTo = await _email.CheckFormatEmail(obj.SendTo);
                if (!checkEmailTo.IsSucceeded)
                {
                    baseResponse.IsSucceeded = checkEmailTo.IsSucceeded;
                    baseResponse.Message = $"{checkEmailTo.Message}, Email To: {obj.SendTo}";
                    return BadRequest(baseResponse);
                }
                var checkEmailFrom = await _email.CheckFormatEmail(obj.SendFrom);
                if (!checkEmailFrom.IsSucceeded)
                {
                    baseResponse.IsSucceeded = checkEmailFrom.IsSucceeded;
                    baseResponse.Message = $"{checkEmailFrom.Message}, Email From: {obj.SendFrom}";
                    return BadRequest(baseResponse);
                }
                #endregion
                var sendEmail = await _email.SendEmailAsync(obj);
                #region Register Email
                var checkEmail = await _email.CheckEmail(obj.SendTo);
                if (!checkEmail.IsSucceeded && sendEmail.IsSucceeded)
                {
                    baseResponse.IsSucceeded = true;
                    baseResponse.Message = $"{sendEmail.Message}\nRegister Failed: {checkEmail.Message}";
                }
                if (checkEmail.IsSucceeded && sendEmail.IsSucceeded)
                {
                    createEmailDTO.Email = obj.SendTo;
                    createEmailDTO.Name = obj.NameTo;
                    var addEmail = await _email.Insert(createEmailDTO);
                    baseResponse.IsSucceeded = true;
                    baseResponse.Message = $"{sendEmail.Message}\nRegister Email Succeeded";
                }
                #endregion
                return Ok(baseResponse);
             
            }
            catch (Exception ex)
            {
                baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("GetMultipleEBookByName")]
        public async Task<ActionResult> GetMultipleLinkEbookByName(EbookNameDTO obj)
        {
            try
            {
                var result = await _ebook.GetByName(obj.EbookName);
                if (result.Count() > 0)
                {
                    ebook.IsSucceeded = true;
                    ebook.Message = $"Berhasil Mengambil {result.Count()} Link E-Book";
                    ebook.Ebooks = _mapper.Map<List<EbookDTO>>(result);
                }
                else
                {
                    ebook.IsSucceeded = false;
                    ebook.Message = $"Link E-Book Not Found";
                    ebook.Ebooks = null;
                    return NotFound(ebook);
                }
                return Ok(ebook);
            }
            catch (Exception ex)
            {
                baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("Edit")]
        public async Task<ActionResult> EditEBook(AddEbookDTO obj)
        {
            try
            {
                var mapping = _mapper.Map<Ebook>(obj);
                var edit = await _ebook.UpdateEbook(mapping);
                var ebook = _mapper.Map<EbookResponseDTO>(edit);
                if (edit != null)
                {
                    ebook.IsSucceeded = true;
                    ebook.Message = "Edit Data E-Book Berhasil";
                }
                return Ok(ebook);
            }catch (Exception ex)
            {
                baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        [HttpPost("GetAllDuplicate")]
        public async Task<ActionResult> GetAllDup(AddMultipleEbookDTO obj)
        {
            try
            {
                var data = _mapper.Map<Ebook[]>(obj.Ebooks);
                var results = await _ebook.CheckDuplicate(data);
                if (results.Count() > 0)
                {
                    ebook.IsSucceeded = true;
                    ebook.Message = $"Berhasil Mengambil {results.Count()} E-Book Duplicate";
                    ebook.Ebooks = _mapper.Map<List<EbookDTO>>(results);
                }
                else
                {
                    ebook.IsSucceeded = false;
                    ebook.Message = "Tidak Ada Data Duplicate";
                    ebook.Ebooks = null;
                    return NotFound(ebook);
                }
             
                return Ok(ebook);
            }
            catch (Exception ex)
            {
                baseResponse.IsSucceeded = false;
                baseResponse.Message = ex.Message;
                return StatusCode((int)HttpStatusCode.InternalServerError, baseResponse);
            }
        }
        //[HttpPost("Paging/{page}/{take}")]
        //public async Task<ActionResult> EbookPaging(EbookNameDTO obj, int page, float take)
        //{
        //    try
        //    {
        //        ReadMultipleEBookDTOPage readMultipleEBookDTOPage = new ReadMultipleEBookDTOPage();
        //        var results = await _ebook.GetByName(obj.BookName);
        //        if (_context.LinkEbooks == null)
        //            return NotFound();
        //        var pageResults = take;
        //        var pageCount = Math.Ceiling(_context.LinkEbooks.Count() / pageResults);
        //        var Ebooks = results.Skip((page - 1) * (int)pageResults)
        //            .Take((int)pageResults)
        //            .ToList();
        //        readMultipleEBookDTOPage.Message = $"Berhasil Memngambil {results.Count()} Ebook";
        //        readMultipleEBookDTOPage.IsSucceeded = true;
        //        readMultipleEBookDTOPage.CurrentPage = page;
        //        readMultipleEBookDTOPage.Pages = (int)pageCount;
        //        readMultipleEBookDTOPage.Ebooks = _mapper.Map<List<EbookResponseDTO>>(Ebooks);
        //        return Ok(readMultipleEBookDTOPage);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

    }
}
