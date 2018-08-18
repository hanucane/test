using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Test.Models
{
    public class Participants
    {
        public int id { get; set; }
        [ForeignKey("UsersId")]
        public int UsersId { get; set; }
        public Users User { get; set; }
        [ForeignKey("ActivitiesId")]
        public int ActivitiesId { get; set; }
        public Activities Activities { get; set; }

    }
}