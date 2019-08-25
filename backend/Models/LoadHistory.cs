using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace backend.Models {
    public class LoadHistory {
        [Key]
        [IgnoreDataMember]
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public int Load { get; set; }

        [Required]
        [IgnoreDataMember]
        public Worker Worker { get; set; }
    }
}