using EcommDesignsHub.Models;
using System.Collections.Generic;

namespace EcommDesignsHub.Models.ViewModel
{
    public class JobApplicantsViewModel
    {
        public Job Job { get; set; }
        public List<Application> Applications { get; set; } = new List<Application>();
    }
}
