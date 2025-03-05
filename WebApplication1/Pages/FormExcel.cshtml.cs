using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClosedXML.Excel;
using WebApplication1.Entities;
using System.IO;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using WebApplication1.Validators;

namespace WebApplication1.Pages
{
    public class FormExcelModel : PageModel
    {
        [BindProperty]
        [DisplayName("Nombre")]
        [Required( AllowEmptyStrings = false, ErrorMessage = "Por favor ingrese un nombre" )]
        public string? name { get; set; }
        [BindProperty]
        [DisplayName("Edad")]
        [NumberValidator(ErrorMessage = "El valor no puede ser cero")]
        public int age { get; set; }
        [BindProperty]
        public bool isMarried { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Por favor seleccione un archivo")]
        public IFormFile? excelFile { get; set; }

        [BindProperty]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor ingrese una fecha")]
        public DateTime? birthDate { get; set; }
        public List<string> headersExcel { get; set; } = new List<string> { };
        public List<Persona> personas { get; set; } = new List<Persona> { };
         
        private readonly ILogger<FormExcelModel> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FormExcelModel(ILogger<FormExcelModel> logger, IHostEnvironment hostEnvironment, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _webHostEnvironment = webHostEnvironment;
        }
        public void OnGet()
        {
            
        }

        public void OnPostSubmit()
        {
            if (ModelState.IsValid)
            {
                if (excelFile != null)
                {
                    try
                    {
                        using (Stream fileUploaded = excelFile.OpenReadStream())
                        {
                            XLWorkbook workbook = new XLWorkbook(fileUploaded);
                            var sheet = workbook.Worksheets.ElementAt(0);
                            foreach (IXLRow row in sheet.Rows())
                            {
                                Persona persona = new Persona();
                                foreach (var cell in row.Cells())
                                {
                                    if (cell.Address.RowNumber == 1 && !cell.Value.IsBlank)
                                    {
                                        this.headersExcel.Add(cell.Value.ToString());
                                    }
                                    else if (cell.Address.RowNumber > 1 && !cell.Value.IsBlank)
                                    {
                                        switch (cell.Address.ColumnLetter)
                                        {
                                            case "A":
                                                persona.Nombre = cell.Value.ToString();
                                                break;
                                            case "B":
                                                persona.Edad = Convert.ToInt32(cell.Value.ToString());
                                                break;
                                            case "C":
                                                persona.Domicilio = cell.Value.ToString();
                                                break;
                                            case "D":
                                                persona.FechaNacimiento = Convert.ToDateTime(cell.Value.ToString());
                                                break;
                                        }
                                    }
                                }
                                if (row.RowNumber() > 1)
                                    personas.Add(persona);
                            }


                            ViewData["headersList"] = this.headersExcel;
                            ViewData["personaList"] = this.personas;


                            this.HttpContext.Session.SetString("headersTable", JsonConvert.SerializeObject(headersExcel));
                            this.HttpContext.Session.SetString("rowsTable", JsonConvert.SerializeObject(personas));
                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
            
        }

        public FileResult OnGetPersonaJson(int indice)
        {

            if (this.HttpContext.Session.Keys.Count() > 0)
            {
                if (this.HttpContext.Session.GetString("rowsTable") != null)
                {
                    personas = JsonConvert.DeserializeObject<List<Persona>>(this.HttpContext.Session.GetString("rowsTable"));
                }
            }

            string txtFilesPath = $"{_hostEnvironment.ContentRootPath}\\wwwroot\\TxtFiles";
            if (!Path.Exists(txtFilesPath))
            {
                Directory.CreateDirectory(txtFilesPath);
            }

            Persona personaSeleccionada = this.personas[indice];

            string personName = personaSeleccionada.Nombre.Split(' ')[0];
            string FileName = $"{txtFilesPath}\\{personName}.txt";
            using (StreamWriter sw = System.IO.File.CreateText(FileName))
            {
                sw.Write(JsonConvert.SerializeObject(personaSeleccionada));
                sw.Dispose();
                sw.Close();
            }

            FileStream fileStream = System.IO.File.OpenRead(FileName);

            //if (System.IO.File.Exists(FileName))
            //{
            //    System.IO.File.OpenRead(FileName).Close();
            //    System.IO.File.Delete(FileName);
            //}

            return File(fileStream, "text/plain", $"{personName}.txt");
        } 
        
    }
}
