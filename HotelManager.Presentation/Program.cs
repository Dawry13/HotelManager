using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using HotelManager.Business;

namespace HotelManager.Presentation
{
    internal static class Program
    {
        public static IServiceProvider Services { get; private set; }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var services = new ServiceCollection();
            services.AddHotelManager();
            // services.AddTransient<FrmPrincipal>(); // registra tus formularios

            Services = services.BuildServiceProvider();

            // Application.Run(Services.GetRequiredService<FrmPrincipal>());
        }
    }
}