using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace RegionOrebroLan.Data.Common
{
	public interface IProviderFactories
	{
		#region Methods

		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
		DbProviderFactory Get(string name);

		#endregion
	}
}