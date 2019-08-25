using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkersController : ControllerBase
    {
        private readonly BackendDbContext _context;
        private const int POINTS_NUMBER = 10;

        public WorkersController(BackendDbContext context) {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Worker>> Get()
        {
            return _context.Workers.Select(c => new Worker(){
                Name = c.Name,
                LoadHistory = c.LoadHistory.OrderBy(lh => lh.Timestamp).TakeLast(POINTS_NUMBER).ToList()
            }).ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<Worker> Get(string id) {
            return _context.Workers.Select(c => new Worker(){
                Name = c.Name,
                LoadHistory = c.LoadHistory.OrderBy(lh => lh.Timestamp).TakeLast(POINTS_NUMBER).ToList()
            }).FirstOrDefault(w => w.Name == id);
        }

        public class JsonWorkerModel {
            public string Name { get; set; }
        }

        [HttpPost]
        public void Post([FromBody] JsonWorkerModel jsonWorkerModel) {
            bool isAlreadyCreated = _context.Workers.Any(w => w.Name == jsonWorkerModel.Name);
            if (!isAlreadyCreated) {
                 _context.Workers.Add(new Worker(){Name = jsonWorkerModel.Name});
                 _context.SaveChanges();
            }
        }

        public class JsonLoadModel {
            [Range(0, 100)]
            public int Load { get; set; }
        }

        [HttpPut("{id}")]
        public void Put(string id, [FromBody] JsonLoadModel loadModel) {
            Worker worker = _context.Workers.FirstOrDefault(w => w.Name == id);
            if (worker != null) {
                _context.LoadHistory.Add(new LoadHistory(){ 
                    Load = loadModel.Load,
                    Worker = worker,
                    Timestamp = DateTime.UtcNow,
                });
                _context.SaveChanges();
            }
            
        }

        [HttpDelete("{id}")]
        public void Delete(string id) {
            Worker worker = _context.Workers.FirstOrDefault(w => w.Name == id);
            if (worker != null) {
                _context.Workers.Remove(worker);
                _context.SaveChanges();
            }
        }
    }
}
