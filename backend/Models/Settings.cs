using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace backend.Models {
    public class Settings {
        [Key]
        [IgnoreDataMember]
        public int Id { get; set; }

        [Range(1, 1000)]
        public int Generated { get; set; }
        
        [Range(1, 1000)]
        public int Consumed { get; set; }
    }
}