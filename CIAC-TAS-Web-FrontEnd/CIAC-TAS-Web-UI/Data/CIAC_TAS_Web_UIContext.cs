using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CIAC_TAS_Web_UI.ModelViews.ASA;

namespace CIAC_TAS_Web_UI.Data
{
    public class CIAC_TAS_Web_UIContext : DbContext
    {
        public CIAC_TAS_Web_UIContext (DbContextOptions<CIAC_TAS_Web_UIContext> options)
            : base(options)
        {
        }

        public DbSet<CIAC_TAS_Web_UI.ModelViews.ASA.PreguntaAsaView> PreguntaAsaView { get; set; } = default!;
    }
}
