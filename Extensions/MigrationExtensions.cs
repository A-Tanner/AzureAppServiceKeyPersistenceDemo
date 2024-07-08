using AppServiceSlotSwapDebug.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;

namespace AppServiceSlotSwapDebug.Extensions
{
	public static class MigrationExtensions
	{
		public static void ApplyMigrations(this IApplicationBuilder app)
		{
			using IServiceScope scope = app.ApplicationServices.CreateScope();
			using AppDbContext context = scope.ServiceProvider.GetService<AppDbContext>();
			if (context.Database.GetPendingMigrations().Any())
			{
				context.Database.Migrate();
			}
		}
	}
}
 