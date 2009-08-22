using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.Internal;
using AutoMapper.Mappers;
using LinFu.DynamicProxy;

namespace AutoMapper
{
	public class MappingEngine : IMappingEngine, IMappingEngineRunner
	{
		private readonly IConfigurationProvider _configurationProvider;
		private readonly ProxyFactory _proxyFactory = new ProxyFactory();
		private readonly IObjectMapper[] _mappers;
		private readonly IDictionary<TypePair, IObjectMapper> _objectMapperCache = new Dictionary<TypePair, IObjectMapper>();

		public MappingEngine(IConfigurationProvider configurationProvider)
		{
			_configurationProvider = configurationProvider;
			_mappers = configurationProvider.GetMappers();
			_configurationProvider.TypeMapCreated += ClearTypeMap;
		}

		public IConfigurationProvider ConfigurationProvider
		{
			get { return _configurationProvider; }
		}

		public TDestination Map<TSource, TDestination>(TSource source)
		{
			Type modelType = typeof(TSource);
			Type destinationType = typeof(TDestination);

			return (TDestination)Map(source, modelType, destinationType);
		}

		public TDestination Map<TSource, TDestination>(ResolutionContext parentContext, TSource source)
		{
			Type destinationType = typeof(TDestination);
			Type sourceType = typeof(TSource);
			TypeMap typeMap = ConfigurationProvider.FindTypeMapFor(source, sourceType, destinationType);
			var context = parentContext.CreateTypeContext(typeMap, source, sourceType, destinationType);
			return (TDestination)((IMappingEngineRunner)this).Map(context);
		}

		public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
		{
			Type modelType = typeof(TSource);
			Type destinationType = typeof(TDestination);

			return (TDestination)Map(source, destination, modelType, destinationType);
		}

		public TDestination DynamicMap<TSource, TDestination>(TSource source)
		{
			Type modelType = typeof(TSource);
			Type destinationType = typeof(TDestination);

			return (TDestination)DynamicMap(source, modelType, destinationType);
		}

		public TDestination DynamicMap<TDestination>(object source)
		{
			Type modelType = source == null ? typeof(object) : source.GetType();
			Type destinationType = typeof(TDestination);

			return (TDestination)DynamicMap(source, modelType, destinationType);
		}

		public object DynamicMap(object source, Type sourceType, Type destinationType)
		{
			var typeMap = ConfigurationProvider.FindTypeMapFor(source, sourceType, destinationType);
			if (typeMap == null)
			{
				typeMap = ConfigurationProvider.CreateTypeMap(sourceType, destinationType);
				ConfigurationProvider.AssertConfigurationIsValid(typeMap);
			}

			return Map(source, sourceType, destinationType);
		}

		public object Map(object source, Type sourceType, Type destinationType)
		{
			TypeMap typeMap = ConfigurationProvider.FindTypeMapFor(source, sourceType, destinationType);

			var context = new ResolutionContext(typeMap, source, sourceType, destinationType);

			return ((IMappingEngineRunner)this).Map(context);
		}

		public object Map(object source, object destination, Type sourceType, Type destinationType)
		{
			TypeMap typeMap = ConfigurationProvider.FindTypeMapFor(source, sourceType, destinationType);

			var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType);

			return ((IMappingEngineRunner)this).Map(context);
		}

		object IMappingEngineRunner.Map(ResolutionContext context)
		{
			try
			{
				if (context.SourceValue == null && ShouldMapSourceValueAsNull(context))
				{
					return ObjectCreator.CreateDefaultValue(context.DestinationType);
				}

				var contextTypePair = new TypePair(context.SourceType, context.DestinationType);

				IObjectMapper mapperToUse;

				if (!_objectMapperCache.TryGetValue(contextTypePair, out mapperToUse))
				{
					lock (_objectMapperCache)
					{
						if (!_objectMapperCache.TryGetValue(contextTypePair, out mapperToUse))
						{
							// Cache miss
							mapperToUse = _mappers.FirstOrDefault(mapper => mapper.IsMatch(context));
							_objectMapperCache.Add(contextTypePair, mapperToUse);
						}
					}
				}

				if (mapperToUse == null)
				{
                    if (context.SourceValue != null)
					    throw new AutoMapperMappingException(context, "Missing type map configuration or unsupported mapping.");

				    return ObjectCreator.CreateDefaultValue(context.DestinationType);
				}

				return mapperToUse.Map(context, this);
			}
			catch (Exception ex)
			{
				throw new AutoMapperMappingException(context, ex);
			}
		}

		string IMappingEngineRunner.FormatValue(ResolutionContext context)
		{
			TypeMap contextTypeMap = context.GetContextTypeMap();
			IFormatterConfiguration configuration = contextTypeMap != null
												? ConfigurationProvider.GetProfileConfiguration(contextTypeMap.Profile)
												: ConfigurationProvider.GetProfileConfiguration(Configuration.DefaultProfileName);

			var valueFormatter = new ValueFormatter(configuration);

			return valueFormatter.FormatValue(context);
		}

		object IMappingEngineRunner.CreateObject(ResolutionContext context)
		{
			var typeMap = context.TypeMap;

			if (typeMap != null && typeMap.DestinationCtor != null)
				return typeMap.DestinationCtor(context.SourceValue);

			if (context.DestinationValue != null)
				return context.DestinationValue;

			var destinationType = context.DestinationType;

			if (destinationType.IsInterface)
				return _proxyFactory.CreateProxy(destinationType, new PropertyBehaviorInterceptor());

			return ObjectCreator.CreateObject(destinationType);
		}

		private bool ShouldMapSourceValueAsNull(ResolutionContext context)
		{
			var typeMap = context.GetContextTypeMap();
			if (typeMap != null)
				return ConfigurationProvider.GetProfileConfiguration(typeMap.Profile).MapNullSourceValuesAsNull;

			return ConfigurationProvider.MapNullSourceValuesAsNull;
		}

		private void ClearTypeMap(object sender, TypeMapCreatedEventArgs e)
		{
			_objectMapperCache.Remove(new TypePair(e.TypeMap.SourceType, e.TypeMap.DestinationType));
		}
	}
}
