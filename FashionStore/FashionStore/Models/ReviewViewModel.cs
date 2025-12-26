using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FashionStore.Models
{
    public class ReviewViewModel
    {
        public string FullName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SizeName { get; set; }
        public string ColorName { get; set; }
    }
}