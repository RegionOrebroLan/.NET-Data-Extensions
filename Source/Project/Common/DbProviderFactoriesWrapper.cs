using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using RegionOrebroLan.DependencyInjection;

namespace RegionOrebroLan.Data.Common
{
	[ServiceConfiguration(ServiceType = typeof(IProviderFactories))]
	public class DbProviderFactoriesWrapper : IProviderFactories
	{
		#region Methods

		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
		public virtual DbProviderFactory Get(string name)
		{
			/*
				When System.Data.Common.DbProviderFactories has been ported, use the following code instead:
				return DbProviderFactories.GetFactory(name);
			*/

			if(name != null && name.Equals("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
				return SqlClientFactory.Instance;

			throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Can not get a factory for proivider with invariant-name \"{0}\".", name));
		}

		#endregion
	}
}