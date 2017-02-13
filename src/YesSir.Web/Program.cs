using Nancy;
using System;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Nancy.Owin;
using Nancy.Routing.Constraints;
using Nancy.Conventions;
using Nancy.Responses;
using System.Reflection;
using System.Text;

namespace YesSir.Backend {
	public class Startup {
		public void Configure(IApplicationBuilder app) {
			app.UseOwin(x => x.UseNancy());
		}
	}

	public class EmailRouteSegmentConstraint : RouteSegmentConstraintBase<string> {
		public override string Name
		{
			get { return "email"; }
		}

		protected override bool TryMatch(string constraint, string segment, out string matchedValue) {
			if (segment.Contains("@")) {
				matchedValue = segment;
				return true;
			}

			matchedValue = null;
			return false;
		}
	}

	public class SimpleModule : NancyModule {
		public SimpleModule() {
			Get("/subscribe/{value}", args => {
				File.AppendAllText(@"subscriptions.txt", args.value + Environment.NewLine);

				return HttpStatusCode.OK;
			});

			Get("/", _ => View["Content/index"]);

			After += ctx => {
				if (ctx.Response.ContentType == "text/html")
					ctx.Response.ContentType = "text/html; charset=utf-8";
			};
		}
	}

	public class Bootstrapper : DefaultNancyBootstrapper {
		protected override void ConfigureConventions(NancyConventions nancyConventions) {
			nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("imgs", "Content/imgs"));
			nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("fonts", "Content/fonts"));

			base.ConfigureConventions(nancyConventions);
		}
	}

	public class Program {
		public static void Main(string[] args) {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			var host = new WebHostBuilder()
						.UseContentRoot(Directory.GetCurrentDirectory() + "/Content")
						.UseKestrel()
						.UseIISIntegration()
						.UseStartup<Startup>()
						.UseUrls("http://localhost:80")
						.Build();

			Console.WriteLine("Starting Nancy on http://localhost:80");
			host.Start();
			Console.ReadLine();
		}
	}
}
