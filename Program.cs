using AppServiceSlotSwapDebug.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Configuration;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using AppServiceSlotSwapDebug.Database;
using AppServiceSlotSwapDebug.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.DataProtection;
using Azure.Identity;
using Azure.Storage.Blobs;

internal class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddControllers();

		// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);
		builder.Services.AddAuthorization();

		builder.Services.AddIdentityCore<User>()
			.AddEntityFrameworkStores<AppDbContext>()
			.AddApiEndpoints()
			.AddDefaultTokenProviders();

		builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

		var container = new BlobContainerClient(builder.Configuration.GetConnectionString("DataStorage"),
			builder.Configuration.GetValue<string>("DataProtection:DataStorageContainerName"));

		BlobClient blobClient = container.GetBlobClient(builder.Configuration.GetValue<string>("DataProtection:BlobName"));

		builder.Services.AddDataProtection()
			.PersistKeysToAzureBlobStorage(blobClient)
			.ProtectKeysWithAzureKeyVault(builder.Configuration.GetValue<Uri>("DataProtection:KeyVaultUri"), new DefaultAzureCredential())
			.SetApplicationName(nameof(AppServiceSlotSwapDebug));

		var app = builder.Build();

		app.UseSwagger();
		app.UseSwaggerUI();
		app.ApplyMigrations();

		app.UseHttpsRedirection();

		app.UseAuthentication();
		app.UseAuthorization();

		app.MapIdentityApi<User>();
		app.MapControllers();

		app.Run();
	}
}