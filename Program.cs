using System;
using System.Collections.Generic;
using System.Linq;

//http://hodzanassredin.github.io/2014/06/21/yet-another-monad-guide.html
//http://blogs.claritycon.com/blog/2013/08/functional-concepts-c/

namespace ConsoleApplication1 {
	class Program {
		//-- helper functions
		static string ToUpper(string s) => s.ToUpper();
		static string ToLower(string s) => s.ToLower();
		static string Trim(string s) => s.Trim();
		static string Substring(string s) => s.Substring(3);
		static int Length(string s) => s.Length;
		static string ToString(int i) => "\"\{i}\"";
		//--

		static string ExtractVowels(string text) {
			var vowels = text.Where(c => "aeiouAEIOU".Contains(c)).ToArray();
			return new string(vowels);
		}
		static void Main(string[] args) {
			// Maybe monad usage: input regular string, intermediate result int...
			var maybeStr = new Maybe<string>("I'm the wrapped up value!");
			var result = maybeStr.Bind(ExtractVowels)
													 .Bind(Length)
													 .Bind(ToString);
			Console.WriteLine(result.Value);

			// Maybe monad usage: null string
			maybeStr = new Maybe<string>(null);
			result = maybeStr.Bind(ExtractVowels)
											 .Bind(ToLower);
			Console.WriteLine(result.Value == null ? "null" : result.Value);

			// Writer monad usage
			var writerMaybeMonad = new WriterT<string>("Nice, nice, and nice...");
			result = writerMaybeMonad.Bind(ExtractVowels)
															 .Bind(Length)
															 .Bind(ToString);
			Console.WriteLine(result.Value);
			foreach (var s in result.Info) Console.WriteLine(s);

			Console.ReadLine();
		}
	}

	public interface IMonad<T> {
		//note: Return is implemented by constructor
		//IMonad<T> Return(T @value);
		IMonad<T2> Bind<T2>(Func<T, T2> f);

		//optional, but helpfull :)
		T Value { get; }
		List<string> Info { get; }
	}

	class Maybe<T> : IMonad<T> {
		public T Value { get; }
		public List<string> Info { get; internal set; }

		public Maybe(T @value) {
			Value = @value;
			Info = new List<string>();
		}

		public virtual IMonad<T2> Bind<T2>(Func<T, T2> f) {
			if (Value != null) {
				return new Maybe<T2>(f(Value));
			}
			return new Maybe<T2>(default(T2));
		}
	}

	class Writer<T> : IMonad<T> {
		public T Value { get; }
		public List<string> Info { get; internal set; }

		public Writer(T @value) {
			Value = @value;
		}

		public IMonad<T2> Bind<T2>(Func<T, T2> f) {
			if (Value != null) {
				var result = f(Value);
				var ret = new Writer<T2>(result);
				ret.Info.Add("Result for \{f.GetType().Name} is {result}");
				return ret;
			}
			Info.Add("\{f.GetType().Name} not applied, value == null");
			return new Writer<T2>(default(T2));
		}
	}

	class WriterT<T> : Maybe<T> {
		public WriterT(T @value) : base(@value) { }
		WriterT(T @value, List<string> info) : base(@value) {
			Info = info;
		}

		public override IMonad<T2> Bind<T2>(Func<T, T2> f) {
			var result = base.Bind(f);
			var w = new WriterT<T2>(result.Value, Info);
			w.Info.Add("Result for \{(f.Method).Name}() is \{result.Value.GetType()} -> \{result.Value}");
			return w;
		}
	}
}
