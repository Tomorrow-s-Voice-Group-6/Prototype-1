using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using TVAttendance.CustomControllers;
using TVAttendance.Data;
using TVAttendance.Models;
using TVAttendance.Utilities;

namespace TVAttendance.Controllers
{
    public class SingerController : ElephantController
    {
        private readonly TomorrowsVoiceContext _context;

        public SingerController(TomorrowsVoiceContext context)
        {
            _context = context;
        }

        // GET: Singer
        public async Task<IActionResult> Index(
        string? SearchString,
        int? ChapterID,
        string? actionButton,
        string? YoungDOB,
        string? OldestDOB,
        string? ToDate,
        string? FromDate,
        int? page,
        IFormFile excelDoc,
        bool ActiveStatus = true,
        string sortDirection = "asc",
        string sortField = "Full Name"
        )
        {
            ViewData["Filtering"] = "btn-outline-secondary";
            int numFilters = 0;

            var singers = _context.Singers
                .Include(s => s.Chapter)
                .AsNoTracking();

            PopulateLists();
            
            singers = singers.Where(s => s.Status == ActiveStatus);

            string[] sortOptions = new[] { "Full Name", "E-Contact Phone", "Chapter" };

            if(actionButton == "Import")
            {
                ImportSingers(excelDoc);
            }

            // Filtering
            #region Filtering
            if (ChapterID.HasValue)
            {
                singers = singers.Where(c => c.ChapterID == ChapterID);
                numFilters++;
            }
            if (!SearchString.IsNullOrEmpty())
            {
                singers = singers.Where(s => s.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || s.FirstName.ToUpper().Contains(SearchString.ToUpper()));
                numFilters++;
            }
            if (!OldestDOB.IsNullOrEmpty())
            {
                int age = int.Parse(OldestDOB);
                DateTime oldestAge = DateTime.Now.AddYears(-age);
                singers = singers.Where(s => s.DOB > oldestAge);
                numFilters++;
            }
            if (!YoungDOB.IsNullOrEmpty())
            {
                int age = int.Parse(YoungDOB);
                DateTime youngestAge = DateTime.Now.AddYears(-age);
                singers = singers.Where(s => s.DOB < youngestAge);
                numFilters++;
            }
            if (!FromDate.IsNullOrEmpty())
            {
                singers = singers.Where(s => s.RegisterDate > DateTime.Parse(FromDate));
                numFilters++;
            }
            if (!ToDate.IsNullOrEmpty())
            {
                singers = singers.Where(s => s.RegisterDate < DateTime.Parse(ToDate));
                numFilters++;
            }
            if (!ActiveStatus)
            {
                numFilters++;
            }
            if (numFilters != 0)
            {
                ViewData["Filtering"] = "btn-danger";
                ViewData["numFilters"] = $"({numFilters} Filter{(numFilters > 1 ? "s" : "")} Applied)";
                ViewData["ShowFilter"] = "show";
            }
            #endregion

            // Sorting
            #region Sorting
            if (!String.IsNullOrEmpty(actionButton))
            {
                page = 1;

                if (sortOptions.Contains(actionButton))
                {
                    if (actionButton == sortField)
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;
                }
            }

            if (sortField == "Full Name")
            {
                singers = sortDirection == "asc"
                    ? singers.OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
                    : singers.OrderByDescending(p => p.FirstName).ThenByDescending(p => p.LastName);
            }
            else if (sortField == "Chapter")
            {
                singers = sortDirection == "asc"
                    ? singers.OrderByDescending(p => p.Chapter.City).ThenBy(p => p.FirstName).ThenBy(p => p.LastName)
                    : singers.OrderBy(p => p.Chapter.City).ThenBy(p => p.FirstName).ThenBy(p => p.LastName);
            }
            #endregion

            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            // Pagination
            int pageSize = 10;
            var pagedData = await PaginatedList<Singer>.CreateAsync(singers.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }


        // GET: Singer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var singer = await _context.Singers
                .Include(s => s.Chapter)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (singer == null)
            {
                return NotFound();
            }

            return View(singer);
        }

        // GET: Singer/Create
        public IActionResult Create()
        {


            ViewData["ModalPopup"] = "hide";

            PopulateLists();
            return View();
        }

        // POST: Singer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,FirstName,LastName,DOB," +
            "Address,Status,RegisterDate," +
            "EmergencyContactFirstName,EmergencyContactLastName," +
            "EmergencyContactPhone,Street,City,Province,PostalCode,ChapterID")] Singer singer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(singer);
                    await _context.SaveChangesAsync();
                }

                ViewData["ModalPopup"] = "display";
            }
            catch(DbUpdateException ex)
            {
                string message = ex.GetBaseException().Message;
                if (message.Contains("UNIQUE") && message.Contains("Singers.DOB"))
                {
                    ModelState.AddModelError("SingerCompositeKey", "Unable to save changes." +
                        "  You cannot have duplicate Singers.  First name, last name, and Date of Birth must be Unique.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            

            PopulateLists(singer);
            return View(singer);
        }

        // GET: Singer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var singer = await _context.Singers
                .Include(s => s.Chapter)
                .FirstOrDefaultAsync(s => s.ID == id);
            if (singer == null)
            {
                return NotFound();
            }

            PopulateLists();
            return View(singer);
        }

        // POST: Singer/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var singerToUpdate = await _context.Singers
                .Include(s => s.Chapter)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (singerToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Singer>(singerToUpdate, "",
                s=>s.FirstName, s=>s.LastName, s => s.DOB,
                s => s.RegisterDate, s => s.EmergencyContactFirstName, s => s.EmergencyContactLastName,
                s => s.EmergencyContactPhone, s => s.ChapterID, s=>s.Street, s=>s.City,s=>s.Province, s=>s.PostalCode))
            {
                try
                {
                    _context.Update(singerToUpdate);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Details", new {singerToUpdate.ID});
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SingerExists(singerToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
            }

            PopulateLists(singerToUpdate);
            return View(singerToUpdate);
        }

        // GET: Singer/Edit/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var singer = await _context.Singers
                .Include(s => s.Chapter)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (singer == null)
            {
                return NotFound();
            }

            return View(singer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var singerToUpdate = await _context.Singers
                .Include(s => s.Chapter)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (singerToUpdate == null)
            {
                return NotFound();
            }

            try
            {
                singerToUpdate.Status = false;
                _context.Update(singerToUpdate);
                await _context.SaveChangesAsync();

                var returnUrl = ViewData["returnURL"]?.ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction(nameof(Index));
                }
                return Redirect(returnUrl);

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SingerExists(singerToUpdate.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: Singer/Edit/5
        public async Task<IActionResult> Restore(int? id)
        {
            var singerToUpdate = await _context.Singers
                .Include(s => s.Chapter)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (singerToUpdate == null)
            {
                return NotFound();
            }

            try
            {
                singerToUpdate.Status = true;
                _context.Update(singerToUpdate);
                await _context.SaveChangesAsync();
                if (ModelState.IsValid)
                {
                    var returnURL = ViewData["returnURL"]?.ToString();
                    if (string.IsNullOrEmpty(returnURL))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    return RedirectToAction(returnURL);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SingerExists(singerToUpdate.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }


            return View(singerToUpdate);
        }

        [HttpPost]
        public async void ImportSingers(IFormFile excelDoc)
        {
            string feedback = string.Empty;
            if (excelDoc != null)
            {
                string mimeType=excelDoc.ContentType;
                long excelLength=excelDoc.Length;
                if (!(mimeType == "" || excelLength == 0))
                {
                    if(mimeType.Contains("Excel") || mimeType.Contains("spreadsheet"))
                    {
                        ExcelPackage excel;
                        using (var memoryStream = new MemoryStream())
                        {
                            await excelDoc.CopyToAsync(memoryStream);
                            excel = new ExcelPackage(memoryStream);
                        }

                        var workSheet = excel.Workbook.Worksheets[0];
                        var start = workSheet.Dimension.Start;
                        var end = workSheet.Dimension.End;
                        int successCount = 0;
                        int errorCount = 0;

                        var headers = new List<string> 
                        {   
                            "",
                            "First Name", //input 0
                            "Last Name", //input 1
                            "DOB", //input 2
                            "Street", //input 3
                            "City", //input 4
                            "Province", //input 5
                            "Postal Code", //input 6
                            "Emergency Contact First Name", //input 7
                            "Emergency Contact Last Name", //input 8
                            "Emergency Contact Phone", //input 9
                            "Chapter" //input 10
                        };

                        if (workSheet.Cells[1,1].Text == headers[1] && workSheet.Cells[1, 2].Text == headers[2]
                            && workSheet.Cells[1, 3].Text == headers[3] && workSheet.Cells[1, 4].Text == headers[4] && workSheet.Cells[1, 5].Text == headers[5]
                            && workSheet.Cells[1, 6].Text == headers[6] && workSheet.Cells[1, 7].Text == headers[7]
                            && workSheet.Cells[1, 8].Text == headers[8] && workSheet.Cells[1, 9].Text == headers[9]
                            && workSheet.Cells[1, 10].Text == headers[10] && workSheet.Cells[1, 11].Text == headers[11])
                        {
                            var chapters = _context.Chapters;

                            var input = new List<string> { };

                            for (int row = start.Row + 1; row <= end.Row; row++)
                            {
                                Singer singer = new Singer();

                                for (int col = start.Column; col <= end.Column; col++)
                                {
                                    input.Add(workSheet.Cells[row, col].Text);
                                }

                                if (input[0] == "")
                                {
                                    break;
                                }

                                //Grab chapter ID
                                IQueryable<int> filteredChapter = chapters.Where(c => c.City == input[10]).Select(c => c.ID);
                                int chapterID = filteredChapter.FirstOrDefault();

                                var singerProvince = new Province();

                                switch (input[5])
                                {
                                    case "Alberta":
                                        singerProvince = Province.Alberta;
                                        break;
                                    case "British Columbia":
                                        singerProvince = Province.BritishColumbia;
                                        break;
                                    case "Manitoba":
                                        singerProvince = Province.Manitoba;
                                        break;
                                    case "New Brunswick":
                                        singerProvince = Province.NewBrunswick;
                                        break;
                                    case "New Foundland":
                                        singerProvince = Province.NewFoundland;
                                        break;
                                    case "Nova Scotia":
                                        singerProvince = Province.NovaScotia;
                                        break;
                                    case "Nunavut":
                                        singerProvince = Province.Nunavut;
                                        break;
                                    case "North West Territories":
                                        singerProvince = Province.NWTerritories;
                                        break;
                                    case "Ontario":
                                        singerProvince = Province.Ontario;
                                        break;
                                    case "Prince Edward Island":
                                        singerProvince = Province.PrinceEdwardIsland;
                                        break;
                                    case "Quebec":
                                        singerProvince = Province.Quebec;
                                        break;
                                    case "Saskatchewan":
                                        singerProvince = Province.Saskatchewan;
                                        break;
                                    case "Yukon":
                                        singerProvince = Province.Yukon;
                                        break;
                                }

                                singer.FirstName = input[0].Trim();
                                singer.LastName = input[1].Trim();
                                singer.DOB = DateTime.Parse(input[2].Trim());
                                singer.Status = true;
                                singer.RegisterDate = DateTime.Now;
                                singer.EmergencyContactFirstName = input[7].Trim();
                                singer.EmergencyContactLastName = input[8].Trim();
                                singer.EmergencyContactPhone = input[9].Trim();
                                singer.ChapterID = chapterID;
                                singer.Street = input[3].Trim();
                                singer.City = input[4].Trim();
                                singer.Province = singerProvince;
                                singer.PostalCode = input[6].Trim();

                                input.Clear();
                                

                                try
                                {
                                    _context.Singers.Add(singer);
                                    _context.SaveChanges();
                                    successCount++;
                                }
                                catch (DbUpdateException e)
                                {
                                    errorCount++;
                                    if (e.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                                    {
                                        feedback += $"Error: Record {singer.FirstName} {singer.LastName} was rejected as a duplicate.\n";
                                    }
                                    else
                                    {
                                        feedback += $"Error: Record {singer.FirstName} {singer.LastName} caused an error.\n";
                                    }
                                    _context.Singers.Remove(singer);
                                }
                                catch(Exception e)
                                {
                                    errorCount++;
                                    if (e.GetBaseException().Message.Contains("correct format"))
                                    {
                                        feedback += $"Error: Record {singer.FirstName} {singer.LastName}"
                                            + " was rejected becuase it was not in the correct format.\n";
                                    }
                                    else
                                    {
                                        feedback += $"Error: Record {singer.FirstName} {singer.LastName}"
                                            + " caused and error.\n";
                                    }
                                } 
                            }

                            feedback += $"Finished Importing {successCount + errorCount} records." +
                                $" {successCount} inserted to database and {errorCount} rejected";
                        }
                        else
                        {
                            feedback = "Could not upload excel file.  Headers in the first row are incorrect." +
                                "\nHere are the incorrect headers in the file:\n";

                            int columnCount = 1;

                            for (int i = 0; i < 9; i++)
                            {
                                if (workSheet.Cells[i, columnCount].Text == headers[i])
                                {
                                    feedback += $"Incorrect Header: {workSheet.Cells[i, columnCount].Text} || Correct Header Name: {headers[i]}\n";
                                }

                                columnCount++;
                            }
                            errorCount++;
                        }
                    }
                    else
                    {
                        feedback = "Error: File is not an Excel spreadsheet";
                    }
                }
                else
                {
                    feedback = "Error: File appears to be empty";
                }
            }
            else
            {
                feedback = "Error: No file uploaded";
            }

            TempData["feedback"] = feedback;
        }

        //Delete action is not in use for Singers
        // GET: Singer/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var singer = await _context.Singers
        //        .Include(s => s.Chapter)
        //        .FirstOrDefaultAsync(m => m.ID == id);
        //    if (singer == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(singer);
        //}

        //// POST: Singer/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var singer = await _context.Singers.FindAsync(id);
        //    if (singer != null)
        //    {
        //        _context.Singers.Remove(singer);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool SingerExists(int id)
        {
            return _context.Singers.Any(e => e.ID == id);
        }

        private SelectList ChapterList(int? selectedId)
        {
            return new SelectList(_context.Chapters
                .OrderBy(m => m.City), "ID", "City", selectedId);
        }

        private void PopulateLists(Singer singer = null)
        {
            ViewData["ChapterID"] = ChapterList(singer?.ChapterID);
        }
    }
}
