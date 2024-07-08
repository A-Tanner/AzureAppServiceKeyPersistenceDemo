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

		//Stand up a client that references the data storage container that has the keys
		/*** NOTE: The container must already exist in your storage account ***/
		var container = new BlobContainerClient(builder.Configuration.GetConnectionString("StorageAccount"),
			builder.Configuration.GetValue<string>("DataProtection:StorageAccountContainerName"));

		//Stand up a blob client to read/write the required keys to a specific blob
		/*** NOTE: Your blob name MUST terminate in '.xml' to be accessed, but it need not exist in the container prior to running ***/
		var blobClient = container.GetBlobClient(builder.Configuration.GetValue<string>("DataProtection:BlobName"));

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