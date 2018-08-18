using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Test.Models
{
    public class Activities : BaseEntity
    {
        public class CustomDateAttribute : RangeAttribute
        {
            public CustomDateAttribute() : base(
                typeof(DateTime), DateTime.Now.ToString(), "01/01/2050") 
            { } 

        }
        public int id { get; set; }
        [Required]
        [MinLength(2, ErrorMessage="Title must be at least 2 characters")]
        public string title { get; set; }
        [Required]
        [DataType(DataType.Time)]
        public DateTime event_time { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [CustomDate(ErrorMessage="Date must be in the future")]
        public DateTime event_date { get; set; }
        [Range(0,999999)]
        public int duration { get; set; }
        [Required]
        [MinLength(10, ErrorMessage="Description must be at least 10 characters")]
        public string description { get; set; }
        public int CreatorId { get; set; }
        public Users Creator { get; set; }
        public List<Participants> Participant { get; set; }
        public Activities()
        {
            Participant = new List<Participants>();
        }
    }
}
