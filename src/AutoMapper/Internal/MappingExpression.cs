using System;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper
{
	internal class MappingExpression : IMappingExpression
	{
		private readonly TypeMap _typeMap;
		private readonly Func<Type, object> _typeConverterCtor;

		public MappingExpression(TypeMap typeMap, Func<Type, object> typeConverterCtor)
		{
			_typeMap = typeMap;
			_typeConverterCtor = typeConverterCtor;
		}

		public void ConvertUsing<TTypeConverter>()
		{
			ConvertUsing(typeof(TTypeConverter));
		}

		public void ConvertUsing(Type typeConverterType)
		{
			var converter = new DeferredInstantiatedConverter(typeConverterType, () => _typeConverterCtor(typeConverterType));

			_typeMap.UseCustomMapper(source => converter.Convert(source.SourceValue));
		}

		public IMappingExpression WithProfile(string profileName)
		{
			_typeMap.Profile = profileName;

			return this;
		}
	}

	internal class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>, IMemberConfigurationExpression<TSource>, IFormatterCtorConfigurator
	{
		private readonly TypeMap _typeMap;
		private readonly Func<Type, object> _serviceCtor;
		private PropertyMap _propertyMap;

		public MappingExpression(TypeMap typeMap, Func<Type, object> serviceCtor)
		{
			_typeMap = typeMap;
			_serviceCtor = serviceCtor;
		}

		public IMappingExpression<TSource, TDestination> ForMember(Expression<Func<TDestination, object>> destinationMember,
																   Action<IMemberConfigurationExpression<TSource>> memberOptions)
		{
		    var memberInfo = ReflectionHelper.FindProperty(destinationMember);
		    IMemberAccessor destProperty = memberInfo.ToMemberAccessor();
			ForDestinationMember(destProperty, memberOptions);
			return new MappingExpression<TSource, TDestination>(_typeMap, _serviceCtor);
		}

		public IMappingExpression<TSource, TDestination> ForMember(string name,
																   Action<IMemberConfigurationExpression<TSource>> memberOptions)
		{
			IMemberAccessor destProperty = new PropertyAccessor(typeof(TDestination).GetProperty(name));
			ForDestinationMember(destProperty, memberOptions);
			return new MappingExpression<TSource, TDestination>(_typeMap, _serviceCtor);
		}

		public void ForAllMembers(Action<IMemberConfigurationExpression<TSource>> memberOptions)
		{
			_typeMap.GetPropertyMaps().Each(x => ForDestinationMember(x.DestinationProperty, memberOptions));
		}

		public IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>()
			where TOtherSource : TSource
			where TOtherDestination : TDestination
		{
			_typeMap.IncludeDerivedTypes(typeof(TOtherSource), typeof(TOtherDestination));

			return this;
		}

		public IMappingExpression<TSource, TDestination> WithProfile(string profileName)
		{
			_typeMap.Profile = profileName;

			return this;
		}

		public void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			_propertyMap.AddFormatterToSkip<TValueFormatter>();
		}

		public IFormatterCtorExpression<TValueFormatter> AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			var formatter = new DeferredInstantiatedFormatter(() => (IValueFormatter)_serviceCtor(typeof(TValueFormatter)));

			AddFormatter(formatter);

			return new FormatterCtorExpression<TValueFormatter>(this);
		}

		public IFormatterCtorExpression AddFormatter(Type valueFormatterType)
		{
			var formatter = new DeferredInstantiatedFormatter(() => (IValueFormatter)_serviceCtor(valueFormatterType));

			AddFormatter(formatter);

			return new FormatterCtorExpression(valueFormatterType, this);
		}

		public void AddFormatter(IValueFormatter formatter)
		{
			_propertyMap.AddFormatter(formatter);
		}

		public void NullSubstitute(object nullSubstitute)
		{
			_propertyMap.SetNullSubstitute(nullSubstitute);
		}

		public IResolverConfigurationExpression<TSource, TValueResolver> ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver
		{
			var resolver = new DeferredInstantiatedResolver(() => (IValueResolver)_serviceCtor(typeof(TValueResolver)));

			ResolveUsing(resolver);

			return new ResolutionExpression<TSource, TValueResolver>(_propertyMap);
		}

		public IResolverConfigurationExpression<TSource> ResolveUsing(Type valueResolverType)
		{
			var resolver = new DeferredInstantiatedResolver(() => (IValueResolver)_serviceCtor(valueResolverType));

			ResolveUsing(resolver);

			return new ResolutionExpression<TSource>(_propertyMap);
		}

		public IResolutionExpression<TSource> ResolveUsing(IValueResolver valueResolver)
		{
			_propertyMap.AssignCustomValueResolver(valueResolver);

			return new ResolutionExpression<TSource>(_propertyMap);
		}

		public void MapFrom(Func<TSource, object> sourceMember)
		{
			_propertyMap.AssignCustomValueResolver(new DelegateBasedResolver<TSource>(sourceMember));
		}

		public void Ignore()
		{
			_propertyMap.Ignore();
		}

		public void UseDestinationValue()
		{
			_propertyMap.UseDestinationValue = true;
		}

		public void SetMappingOrder(int mappingOrder)
		{
			_propertyMap.SetMappingOrder(mappingOrder);
		}

		public void ConstructFormatterBy(Type formatterType, Func<IValueFormatter> instantiator)
		{
			_propertyMap.RemoveLastFormatter();
			_propertyMap.AddFormatter(new DeferredInstantiatedFormatter(instantiator));
		}

		public void ConvertUsing(Func<TSource, TDestination> mappingFunction)
		{
			_typeMap.UseCustomMapper(source => mappingFunction((TSource)source.SourceValue));
		}

        public void ConvertUsing(Func<ResolutionContext, TSource, TDestination> mappingFunction)
        {
            _typeMap.UseCustomMapper(source => mappingFunction(source, (TSource)source.SourceValue));
        }

		public void ConvertUsing(ITypeConverter<TSource, TDestination> converter)
		{
			ConvertUsing(converter.Convert);
		}

        public void ConvertUsing(IWithContextTypeConverter<TSource, TDestination> converter)
        {
            ConvertUsing(converter.Convert);
        }

		public void ConvertUsing<TTypeConverter>() where TTypeConverter : ITypeConverter<TSource, TDestination>
		{
			var converter = new DeferredInstantiatedConverter<TSource, TDestination>(() => (TTypeConverter)_serviceCtor(typeof(TTypeConverter)));

			ConvertUsing(converter.Convert);
		}

		public IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeFunction)
		{
			_typeMap.AddBeforeMapAction((src, dest) => beforeFunction((TSource)src, (TDestination)dest));

			return this;
		}

		public IMappingExpression<TSource, TDestination> BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>
		{
			Action<TSource, TDestination> beforeFunction = (src, dest) => ((TMappingAction)_serviceCtor(typeof(TMappingAction))).Process(src, dest);

			return BeforeMap(beforeFunction);
		}

		public IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction)
		{
			_typeMap.AddAfterMapAction((src, dest) => afterFunction((TSource)src, (TDestination)dest));

			return this;
		}

		public IMappingExpression<TSource, TDestination> AfterMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>
		{
			Action<TSource, TDestination> afterFunction = (src, dest) => ((TMappingAction)_serviceCtor(typeof(TMappingAction))).Process(src, dest);

			return AfterMap(afterFunction);
		}

		public IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> ctor)
		{
			_typeMap.DestinationCtor = src => ctor((TSource) src);

			return this;
		}

		private void ForDestinationMember(IMemberAccessor destinationProperty, Action<IMemberConfigurationExpression<TSource>> memberOptions)
		{
			_propertyMap = _typeMap.FindOrCreatePropertyMapFor(destinationProperty);

			memberOptions(this);
		}
	}
}
