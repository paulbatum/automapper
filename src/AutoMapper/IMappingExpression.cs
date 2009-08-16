using System;
using System.Linq.Expressions;

namespace AutoMapper
{
	public interface IMappingExpression
	{
		void ConvertUsing<TTypeConverter>();
		void ConvertUsing(Type typeConverterType);
		IMappingExpression WithProfile(string profileName);
	}

	public interface IMappingExpression<TSource, TDestination>
	{
		IMappingExpression<TSource, TDestination> ForMember(Expression<Func<TDestination, object>> destinationMember, Action<IMemberConfigurationExpression<TSource>> memberOptions);
		IMappingExpression<TSource, TDestination> ForMember(string name, Action<IMemberConfigurationExpression<TSource>> memberOptions);
		void ForAllMembers(Action<IMemberConfigurationExpression<TSource>> memberOptions);
		IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>()
			where TOtherSource : TSource
			where TOtherDestination : TDestination;
		IMappingExpression<TSource, TDestination> WithProfile(string profileName);
		void ConvertUsing(Func<TSource, TDestination> mappingFunction);
        void ConvertUsing(Func<ResolutionContext, TSource, TDestination> mappingFunction);
        void ConvertUsing(ITypeConverter<TSource, TDestination> converter);
        void ConvertUsing<TTypeConverter>() where TTypeConverter : ITypeConverter<TSource, TDestination>;
		IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeFunction);
		IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction);
		IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> ctor);
	}

	public interface IMemberConfigurationExpression<TSource>
	{
		void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter;
		IFormatterCtorExpression<TValueFormatter> AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter;
		IFormatterCtorExpression AddFormatter(Type valueFormatterType);
		void AddFormatter(IValueFormatter formatter);
		void FormatNullValueAs(string nullSubstitute);
		IResolverConfigurationExpression<TSource, TValueResolver> ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver;
		IResolverConfigurationExpression<TSource> ResolveUsing(Type valueResolverType);
		IResolutionExpression<TSource> ResolveUsing(IValueResolver valueResolver);
		void MapFrom(Func<TSource, object> sourceMember);
		void Ignore();
		void SetMappingOrder(int mappingOrder);
		void UseDestinationValue();
	}

	public interface IResolutionExpression<TSource>
	{
		void FromMember(Func<TSource, object> sourceMember);
		void FromMember(string sourcePropertyName);
	}

	public interface IResolverConfigurationExpression<TSource, TValueResolver>
		where TValueResolver : IValueResolver
	{
		IResolverConfigurationExpression<TSource, TValueResolver> FromMember(Func<TSource, object> sourceMember);
		IResolverConfigurationExpression<TSource, TValueResolver> FromMember(string sourcePropertyName);
		IResolverConfigurationExpression<TSource, TValueResolver> ConstructedBy(Func<TValueResolver> constructor);
	}

	public interface IResolverConfigurationExpression<TSource> : IResolutionExpression<TSource>
	{
		IResolutionExpression<TSource> ConstructedBy(Func<IValueResolver> constructor);
	}
}