using System;
using System.Linq;

namespace AutoMapper.Mappers
{
	public class FlagsEnumMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			Type enumDestType = TypeHelper.GetEnumerationType(context.DestinationType);

			if (context.SourceValue == null)
			{
				return mapper.CreateObject(context);				
			}

			return Enum.Parse(enumDestType, context.SourceValue.ToString());
		}

		public bool IsMatch(ResolutionContext context)
		{
			var sourceEnumType = TypeHelper.GetEnumerationType(context.SourceType);
			var destEnumType = TypeHelper.GetEnumerationType(context.DestinationType);

			return sourceEnumType != null
				&& destEnumType != null
				&& sourceEnumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any()
				&& destEnumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any();
		}
	}
}