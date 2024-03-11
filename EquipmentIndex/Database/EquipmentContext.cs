using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EquipmentIndex.Database;

namespace EquipmentIndex.Database;

public class EquipmentContext : DbContext
{
		public EquipmentContext()
		{}
        protected override void OnConfiguring(DbContextOptionsBuilder opbuilder)
        {
                base.OnConfiguring(opbuilder);
                opbuilder.UseSqlServer("Data Source=.;Initial Catalog=ElasticTransfer;Integrated Security=true;Trust Server Certificate=true;");

        }
        public DbSet<EquipmentIndexes>? EquipmentIndexes { get; set; }


}
