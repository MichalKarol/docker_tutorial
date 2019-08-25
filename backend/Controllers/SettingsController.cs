using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly BackendDbContext _context;

        public SettingsController(BackendDbContext context) {
            _context = context;
        }

        [HttpGet]
        public ActionResult<Settings> Get()
        {
            return _context.Settings.First();
        }

        public class JsonSettingsModel {
            public int? Generated { get; set; }
            [Range(1, 1000)]
            public int? Consumed { get; set; }
        }
        
        [HttpPost]
        public void Post([FromBody] JsonSettingsModel jsonSettingsModel)
        {
            Settings settings = _context.Settings.First();
            settings.Generated = jsonSettingsModel.Generated.GetValueOrDefault(settings.Generated);
            settings.Consumed = jsonSettingsModel.Consumed.GetValueOrDefault(settings.Consumed);
            _context.SaveChanges();
        }
    }
}
