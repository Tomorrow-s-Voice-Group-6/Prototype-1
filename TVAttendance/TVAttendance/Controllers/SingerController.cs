using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
using TVAttendance.ViewModels;
using static NuGet.Packaging.PackagingConstants;

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
        [Authorize]
        public async Task<IActionResult> Index(
            string? SearchString,
            int? ChapterID,
            string? actionButton,
            string? fred,
            string? YoungDOB,
            string? OldestDOB,
            string? ToDate,
            string? FromDate,
            int? page,
            int? pageSizeID,
            IFormFile excelDoc,
            bool ActiveStatus = true,
            string sortDirection = "asc",
            string sortField = "Full Name")
        {
            ViewData["Filtering"] = "btn-outline-secondary";
            int numFilters = 0;

            var userEmail = User.Identity.Name; // Get the logged-in user's email
            var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            var singers = _context.Singers
                .Include(s => s.Chapter)
                .AsNoTracking();

            PopulateLists();

            singers = singers.Where(s => s.Status == ActiveStatus);

            // If the user is a Director, restrict them to their own chapters unless a specific ChapterID is chosen
            if (userRoles.Contains("Director"))
            {
                var chaptersForDirector = await _context.Chapters
                    .Include(c => c.Directors)
                    .Where(c => c.Directors.Any(d => d.Email == userEmail))
                    .ToListAsync();

                if (chaptersForDirector.Any())
                {
                    var directorChapterIDs = chaptersForDirector.Select(c => c.ID).ToList();

                    // If a ChapterID is provided, allow the director to view it
                    if (ChapterID.HasValue)
                    {
                        singers = singers.Where(s => s.ChapterID == ChapterID);
                    }
                    else
                    {
                        // Default: Restrict to assigned chapters only
                        singers = singers.Where(s => directorChapterIDs.Contains(s.ChapterID));
                    }
                }
            }

            // Supervisors & Admins can see all singers, so no filtering applied to them

            string[] sortOptions = new[] { "Full Name", "E-Contact Phone", "Chapter" };

            if (actionButton == "Import")
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
            if (!string.IsNullOrEmpty(SearchString))
            {
                singers = singers.Where(s => s.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || s.FirstName.ToUpper().Contains(SearchString.ToUpper()));
                numFilters++;
            }
            if (!string.IsNullOrEmpty(OldestDOB))
            {
                int age = int.Parse(OldestDOB);
                DateTime oldestAge = DateTime.Now.AddYears(-age);
                singers = singers.Where(s => s.DOB > oldestAge);
                numFilters++;
            }
            if (!string.IsNullOrEmpty(YoungDOB))
            {
                int age = int.Parse(YoungDOB);
                DateTime youngestAge = DateTime.Now.AddYears(-age);
                singers = singers.Where(s => s.DOB < youngestAge);
                numFilters++;
            }
            if (!string.IsNullOrEmpty(FromDate))
            {
                singers = singers.Where(s => s.RegisterDate > DateTime.Parse(FromDate));
                numFilters++;
            }
            if (!string.IsNullOrEmpty(ToDate))
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

            // Exporting
            if (fred == "Export")
                return ExportData(ChapterID, SearchString, ActiveStatus, YoungDOB, OldestDOB, FromDate, ToDate);

            // Sorting
            #region Sorting
            if (!string.IsNullOrEmpty(actionButton))
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

            singers = sortField switch
            {
                "Full Name" => sortDirection == "asc"
                    ? singers.OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
                    : singers.OrderByDescending(p => p.FirstName).ThenByDescending(p => p.LastName),
                "Chapter" => sortDirection == "asc"
                    ? singers.OrderByDescending(p => p.Chapter.City).ThenBy(p => p.FirstName).ThenBy(p => p.LastName)
                    : singers.OrderBy(p => p.Chapter.City).ThenBy(p => p.FirstName).ThenBy(p => p.LastName),
                _ => singers
            };
            #endregion

            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            // Pagination
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID);
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Singer>.CreateAsync(singers.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }




        // GET: Singer/Details/5
        [Authorize]
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
        [Authorize(Roles = "Director, Supervisor, Admin")]
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
        [Authorize(Roles = "Director, Supervisor, Admin")]
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
                    TempData["SuccessMsg"] = "Successfully created a singer!";
                    ViewData["ModalPopup"] = "display";
                }
                else
                    TempData["ErrorMsg"] = "Error in creating a singer. Please try again.";

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
        [Authorize(Roles = "Director, Supervisor, Admin")]
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
        [Authorize(Roles = "Director, Supervisor, Admin")]
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

        // GET: Singer/Archive/5
        [Authorize(Roles = "Director, Supervisor, Admin")]
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
        [Authorize(Roles = "Director, Supervisor, Admin")]
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
        [Authorize(Roles = "Director, Supervisor, Admin")]
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
        [Authorize(Roles = "Director, Supervisor, Admin")]
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
                                    case "Newfoundland":
                                        singerProvince = Province.NewfoundlandAndLabrador;
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

        }

        [Authorize(Roles = "Director, Supervisor, Admin")]
        public IActionResult ExportData(int? ChapterID, string? singerName, bool? status,
            string? startAge, string? endAge, string? startRegDate, string? endRegDate)
        {
            #region Default filter values
            if (String.IsNullOrEmpty(startAge)) { startAge = "1"; }
            if (String.IsNullOrEmpty(endAge)) { endAge = "80"; } //default max age (no one will be older then 80)
            if (String.IsNullOrEmpty(startRegDate)) {  startRegDate ="2017-01-01"; }
            if (String.IsNullOrEmpty(endRegDate)) { endRegDate = DateTime.Now.ToString("yyyy-MM-dd"); }
            if(status == null) { status = true; } //default status 
            #endregion

           
            //convert age to datetime string
            DateTime startAgeDate = DateTime.Now.AddYears(-(int.Parse(startAge)));
            DateTime endAgeDate = DateTime.Now.AddYears(-int.Parse(endAge));
            DateTime dtRegStart = DateTime.Parse(startRegDate);
            DateTime dtRegEnd = DateTime.Parse(endRegDate);
            //get singers based on date filters for DOB and Register date. The region above sets the required filters if they are null
            var allSingers = _context.Singers
                .Include(s => s.Chapter)
                .Where(s => s.Status == status.Value)
                .Where(s => s.RegisterDate.Date >= dtRegStart.Date && s.RegisterDate.Date <= dtRegEnd.Date)
                .Where(s => s.DOB >= endAgeDate && s.DOB <= startAgeDate)
                .ToList();

            var userEmail = User.Identity.Name; // Get the logged-in user's email
            var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            // If the user is a Director, restrict them to their own chapters unless a specific ChapterID is chosen
            if (userRoles.Contains("Director"))
            {
                var chaptersForDirector = _context.Chapters
                    .Include(c => c.Directors)
                    .Where(c => c.Directors.Any(d => d.Email == userEmail))
                    .ToList();

                if (chaptersForDirector.Any())
                {
                    var directorChapterIDs = chaptersForDirector.Select(c => c.ID).ToList();

                    // If a ChapterID is provided, allow the director to view it
                    if (ChapterID.HasValue)
                    {
                        allSingers = allSingers.Where(s => s.ChapterID == ChapterID).ToList();
                    }
                    else
                    {
                        // Default: Restrict to assigned chapters only
                        allSingers = allSingers.Where(s => directorChapterIDs.Contains(s.ChapterID)).ToList();
                    }
                }
            }

            //Filtering
            if (ChapterID.HasValue)
            {
                allSingers = allSingers.Where(s => s.ChapterID == ChapterID).ToList();
            }
            if (!String.IsNullOrEmpty(singerName))
            {
                allSingers = allSingers.Where(s => s.LastName.ToLower().Contains(singerName.ToLower()) || s.FirstName.ToLower().Contains(singerName.ToLower())).ToList();
            }
            var export = new List<ExportSingersVM>();

            foreach (var s in allSingers)
            {
                export.Add(new ExportSingersVM
                {
                    name = s.FullName,
                    DOB = s.DOB.ToString("yyyy-MM-dd"),
                    RegDate = s.RegisterDate.ToString("yyyy-MM-dd"),
                    chapter = s.Chapter.City,
                    emergencyName = s.EmergFullName,
                    emergencyPhone = s.EmergencyContactPhone
                });
            }

            using (ExcelPackage excel = new ExcelPackage())
            {
                int filterCount = 0;
                string filters = "";
                var workSheet = excel.Workbook.Worksheets.Add("Singers");

                workSheet.Cells[1, 1].Value = "Singers Summary Report";
                workSheet.Cells[1, 1, 1, 6].Merge = true;
                workSheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, 1].Style.Font.Size = 16;
                workSheet.Cells[1, 1].Style.Font.Bold = true;

                workSheet.Cells[2, 1].Value = "Report Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                workSheet.Cells[2, 1, 2, 6].Merge = true;
                workSheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                workSheet.Cells[2, 1].Style.Font.Size = 12;

                workSheet.Cells[3, 1].Value = "Total number of filters applied: ";
                workSheet.Cells[3, 1, 3, 4].Merge = true;
                workSheet.Cells[3, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                workSheet.Cells[3, 1].Style.Font.Size = 12;

                #region Count number of fitlers
                //Check if each filter has a value or isnt the default value assigned when the filter enters this function null,
                //Then count the total number of filters to display
                if (ChapterID.HasValue)
                    filterCount++;
                if (!string.IsNullOrEmpty(singerName))
                    filterCount++;
                if(startAge != "1") //default start age if null
                    filterCount++;
                if(endAge != "80") //default end age if null
                    filterCount++;
                if(dtRegStart.Date != DateTime.Parse("2017-01-01"))
                    filterCount++;
                if (dtRegEnd.Date != DateTime.Now.Date)
                    filterCount++;
                #endregion

                //Display for total count of filters
                workSheet.Cells[3, 5].Value = filterCount;
                workSheet.Cells[3, 5, 3, 6].Merge = true;

                //Col headings
                workSheet.Cells[4, 1].Value = "Name";
                workSheet.Cells[4, 1].Style.Font.Bold = true;
                workSheet.Cells[4, 2].Value = "Date of Birth";
                workSheet.Cells[4, 2].Style.Font.Bold = true;
                workSheet.Cells[4, 3].Value = "Register Date";
                workSheet.Cells[4, 3].Style.Font.Bold = true;
                workSheet.Cells[4, 4].Value = "Chapter";
                workSheet.Cells[4, 4].Style.Font.Bold = true;
                workSheet.Cells[4, 5].Value = "Emergency Contact Name";
                workSheet.Cells[4, 5].Style.Font.Bold = true;
                workSheet.Cells[4, 6].Value = "Emergency Contact Phone";
                workSheet.Cells[4, 6].Style.Font.Bold = true;

                //Display data
                int count = 5;
                foreach(var record in export)
                {
                    workSheet.Cells[count, 1].Value = record.name;
                    workSheet.Cells[count, 2].Value = record.DOB;
                    workSheet.Cells[count, 3].Value = record.RegDate;
                    workSheet.Cells[count, 4].Value = record.chapter;
                    workSheet.Cells[count, 5].Value = record.emergencyName;
                    workSheet.Cells[count, 6].Value = record.phoneFormatted;
                    count++;
                }

                workSheet.Cells.AutoFitColumns();

                try
                {
                    Byte[] data = excel.GetAsByteArray();
                    string fileName = "Singers.xlsx";
                    string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return File(data, mimeType, fileName);
                }
                catch (Exception)
                {
                    TempData["ErrorMsg"] = "Could not build and download the file.";
                    return BadRequest("Could not build and download the file");
                }
            }
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
