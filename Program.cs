using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1 {
	class Program {
		class Number {
			public int Value { get; }
			public Number(int value) {
				Value = value;
			}
			public override string ToString() {
				return Value.ToString();
			}
		}
		//-- helper functions
		static string ToUpper(string s) => s.ToUpper();
		static string ToLower(string s) => s.ToLower();
		static string Trim(string s) => s.Trim();
		static string Substring(string s) => s.Substring(30);
		static Number Length(string s) => new Number(s.Length);
		static string ExtractVowels(string text) {
			var vowels = text.Where(c => "aeiouAEIOU".Contains(c)).ToArray();
			return new string(vowels);
		}
		//--

		class Product {
			public string SKU { get; }
			public string Name { get; }
			public int ManufacturerId { get; }

			public Product(string sku, string name, int manufacturerId) {
				SKU = sku;
				Name = name;
				ManufacturerId = manufacturerId;
			}
		}
		static string Show<T>(T t) => "\"\{t.ToString()}\"";

		class Manufacturerer {
			public string Name { get; }
			public int Id { get; }

			public Manufacturerer(int id, string name) {
				Id = Id;
				Name = name;
			}
		}

		static void Main(string[] args) {
			// Maybe monad usage: input regular string, intermediate result int...
			var maybeM = new MaybeM<string>("I'm a string, wrapped up in the Maybe Monad!");
			var result = maybeM.Bind(ExtractVowels)
													 .Bind(Length);
			Console.WriteLine("Result is: \{result.Value.Value}");

			// Maybe monad usage: null string
			maybeM = new MaybeM<string>(null);
			var result2 = maybeM.Bind(ExtractVowels)
											 .Bind(Length);
			Console.WriteLine("Result is null?: \{result2.Value == null}");

			// Writer monad usage
			var writerM = new WriterM<string>("I'm a string, wrapped up in the Maybe Monad!");
			var result3 = writerM.Bind(ExtractVowels)
															 .Bind(Length);
			Console.WriteLine("Result is: \{result3.Value.Value}");
			foreach (var s in result3.Info) Console.WriteLine(s);

			Console.ReadLine();
		}
	}

	public interface IMonad<T> {
		//note: Return is implemented by constructor
		//IMonad<T> Return(T @value);
		IMonad<T2> Bind<T2>(Func<T, T2> f) where T2 : class;

		// public access to wrapped up value, optional but helpfull :)
		T Value { get; }

		// used in WriterM
		List<string> Info { get; }
	}

	//-- Maybe monad
	class MaybeM<T> : IMonad<T> {
		public T Value { get; }
		public List<string> Info { get; internal set; }

		public MaybeM(T @value) {
			Value = @value;
			Info = new List<string>();
		}

		public virtual IMonad<T2> Bind<T2>(Func<T, T2> f) where T2 : class {
			if (Value != null) {
				return new MaybeM<T2>(f(Value));
			}
			return new MaybeM<T2>(null);
		}
	}

	//-- Writer + Maybe combined
	class WriterM<T> : MaybeM<T> {
		public WriterM(T @value) : base(@value) {	}

		WriterM(T @value, List<string> info) : base(@value) {
			Info = info;
		}

		public override IMonad<T2> Bind<T2>(Func<T, T2> f) {
			try {
				var result = base.Bind(f);
				var w = new WriterM<T2>(result.Value, Info);
				w.Info.Add("Result for \{(f.Method).Name}() is: \{result.Value.GetType()} -> \{result.Value}");
				return w;
			}
			catch (Exception ex) {
				var w = new WriterM<T2>(default(T2), Info);
				w.Info.Add("Exception: \{ex.Message} thrown for \{(f.Method).Name}()");
				return w;
			}
		}
	}
}
