using System;
using System.Collections.Generic;

namespace AutoMapper.Mappers
{
    public static class MapperRegistry
    {
        public static Func<IEnumerable<IObjectMapper>> AllMappers = () => new IObjectMapper[]
        {
            new DataReaderMapper(),
            new TypeMapMapper(TypeMapObjectMapperRegistry.AllMappers()),
            new StringMapper(),
            new FlagsEnumMapper(),
            new EnumMapper(),
            new ArrayMapper(),
			new EnumerableToDictionaryMapper(),
            new DictionaryMapper(),
            new ListSourceMapper(),
            new EnumerableMapper(),
            new AssignableMapper(),
            new TypeConverterMapper(),
            new NullableMapper()
        };
    }
}