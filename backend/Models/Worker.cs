using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Models {
    public class Worker {
        [Key]
        public string Name { get; set; }
        public List<LoadHistory> LoadHistory { get; set; }
    }
}