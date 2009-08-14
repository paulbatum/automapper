using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace AutoMapper.UnitTests.Tests
{
	[TestFixture]
	public class MapperTests : NonValidatingSpecBase
	{
		private class Source
		{
			
		}
		
		private class Destination
		{
			
		}
			
		[Test]
		public void Should_find_configured_type_map_when_two_types_are_configured()
		{
			Mapper.CreateMap<Source, Destination>();

			Mapper.FindTypeMapFor<Source, Destination>().ShouldNotBeNull();
		}
	}
}