using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace RegionOrebroLan.Data.IntegrationTests.Configuration.Extensions
{
	public static class ConfigurationExtension
	{
		#region Methods

		[CLSCompliant(false)]
		public static IDictionary<string, IConnectionSetting> ConnectionSettings(this IConfiguration configuration)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			var connections = new Dictionary<string, IConnectionSetting>(StringComparer.OrdinalIgnoreCase);

			foreach(var item in configuration.GetSection("Connections").GetChildren())
			{
				var connectionSetting = new ConnectionSetting
				{
					ConnectionString = item.GetSection("ConnectionString").Value,
					ProviderName = item.GetSection("ProviderName").Value
				};

				connections.Add(item.Key, connectionSetting);
			}

			return connections;
		}

		#endregion
	}
}