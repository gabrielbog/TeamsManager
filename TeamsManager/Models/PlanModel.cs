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
        [Required]
        public int IdUser { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
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