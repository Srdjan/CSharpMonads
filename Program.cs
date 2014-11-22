using System;
using System.Console;
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
			//-1) Maybe monad usage: input regular string, intermediate result int...
			var maybeM = new MaybeM<string>("I'm a string, wrapped up in the Maybe Monad!");
			var result1 = maybeM.Bind(ExtractVowels)
											 	  .Bind(Length);
			WriteLine("Result is: \{result1.Show()}");

			//-2) Maybe monad usage: null string
			maybeM = new MaybeM<string>(null);
			var result2 = maybeM.Bind(ExtractVowels)
													.Bind(Length);
			WriteLine("Result is: \{result2.Show()}");

			//-3) Writer monad usage
			var writerM = new WriterM<string>("I'm a string, wrapped up in the Maybe Monad!");
			var result3 = writerM.Bind(ExtractVowels)
													 .Bind(Length);
			WriteLine("Result is: \{result3.Show()}");

			ReadLine();
		}
	}

	//-- monad interface
	//--
	public interface IMonad<T> {
		//IMonad<T> From(T @value);//note: implemented as constructor

		IMonad<T2> Bind<T2>(Func<T, T2> f) where T2 : class;

		// simple access the wrapped value(s), optional but helpfull :)
		string Show();
	}

	//-- Maybe monad
	//--
	class MaybeM<T> : IMonad<T> {
		internal T _value;

		public MaybeM(T value) {
			_value = value;
		}

		public IMonad<T2> Bind<T2>(Func<T, T2> f) where T2 : class {
			if (_value != null) {
				return new MaybeM<T2>(f(_value));
			}
			return new MaybeM<T2>(null);
		}

		public string Show() {
			return _value == null ? "null" : _value.ToString();
		}
	}

	//-- Writer monad 
	//--
	class WriterM<T> : IMonad<T> {
		internal T _value;
		List<string> _info;

		public WriterM(T @value) {
			_value = value;
			_info = new List<string>();
		}

		WriterM(T @value, List<string> info) {
			_value = @value;
			_info = info;
		}

		public IMonad<T2> Bind<T2>(Func<T, T2> f) where T2 : class {
			try {
				var result = (f(_value));
				_info.Add("\{(f.Method).Name}()-> \{result}");
				return new WriterM<T2>(f(_value), _info);
			}
			catch (Exception ex) {
				_info.Add("Exception: \{ex.Message} thrown for \{(f.Method).Name}()");
				return new WriterM<T2>(default(T2), _info);
			}
		}

		public string Show() {
			var valStr = _value.ToString();
			_info.ForEach(s => valStr += "\r\n\t" + s);
			return valStr;
		}
	}
}
