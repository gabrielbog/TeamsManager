using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace TeamsManager.Models
{
    public class PlanModel
    {
        [Key]
        public int Id { get; set; }
        public int IdUser { get; set; }
        public string UserName { get; set; }
        public string PlanName { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; }// to do/ pending/ blocked/ done
    }

    public class PlanDbContext : UserDbContext
    {
        public DbSet<PlanModel> Plans { get; set; }
    }
}