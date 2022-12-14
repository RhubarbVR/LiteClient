using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using Rhubarb_Shared;

using RhubarbCloudClient;

using System.Net.Http;
using System.Reflection;

namespace Rhubarb_Web.Client
{
	public class Program
	{
		public class CookieHandler : DelegatingHandler
		{
			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
				request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

				return await base.SendAsync(request, cancellationToken);
			}
		}

		public static async Task Main(string[] args) {
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("#app");
			builder.RootComponents.Add<HeadOutlet>("head::after");
#if DEBUG
			var targetURI = RhubarbAPIClient.LocalUri;
#else
			var targetURI = new Uri("https://api.rhubarbvr.net/");
#endif
			builder.Services.AddScoped<LightModeManager>();

			builder.Services
				.AddTransient<CookieHandler>()
				.AddScoped(sp => sp
					.GetRequiredService<IHttpClientFactory>()
					.CreateClient("API"))
				.AddHttpClient("API", client => { client.BaseAddress = targetURI; client.Timeout = TimeSpan.FromSeconds(90); }).AddHttpMessageHandler<CookieHandler>();
			builder.Services.AddScoped(ser => {
				ser.GetService<LightModeManager>();
				var ret = new RhubarbAPIClient(ser.GetRequiredService<HttpClient>());
				ret.OnLogin += (user) => Console.WriteLine($"Welcome: {user.UserName}");
				ret.OnLogout += () => {
					var path = new Uri(ser.GetRequiredService<NavigationManager>().Uri).PathAndQuery;

					if (path.ToLower().StartsWith("Client") || path == "/" || string.IsNullOrEmpty(path)) {
						ser.GetRequiredService<NavigationManager>().NavigateTo("/Login");
					}
				};
				ret.HasGoneOfline += () => ser.GetRequiredService<NavigationManager>().NavigateTo("/");
				return ret;
			});
			static IEnumerable<string> GetFiles() {
				return Assembly.GetAssembly(typeof(RhubarbAPIClient))?.GetManifestResourceNames() ?? Array.Empty<string>();
			}
			builder.Services.AddScoped<Localisation>((thing) => new DynamicLocalisation(GetFiles, (item) => new StreamReader(Assembly.GetAssembly(typeof(RhubarbAPIClient)).GetManifestResourceStream(item)).ReadToEnd(), () => {

			}));
			await builder.Build().RunAsync();
		}
	}
}