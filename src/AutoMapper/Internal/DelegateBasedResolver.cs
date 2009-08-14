using System;

namespace AutoMapper
{
	internal class DelegateBasedResolver<TSource> : IValueResolver
	{
		private readonly Func<TSource, object> _method;

		public DelegateBasedResolver(Func<TSource, object> method)
		{
			_method = method;
		}

		public ResolutionResult Resolve(ResolutionResult source)
		{
			if (source.Value != null && ! (source.Value is TSource))
			{
				throw new ArgumentException("Expected obj to be of type " + typeof(TSource) + " but was " + source.Value.GetType());
			}

			object result;
			try
			{
				result = _method((TSource)source.Value);
			}
			catch (NullReferenceException)
			{
				result = null;
			}

			return new ResolutionResult(result);
		}
	}
}