using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace TeamsManager.Models
{
    public class EncryptionModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Key { get; set; }
        [Required]
        public string IV { get; set; }
    }

    public class EncryptionDbContext : DbContext
    {
        public DbSet<EncryptionModel> Encryptions { get; set; }
    }
}