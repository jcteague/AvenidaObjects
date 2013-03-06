using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AvenidaSoftware.Objects {

	[ Serializable ]
	public abstract class Enumeration : IEquatable<Enumeration> {
		static readonly AssemblyScanner assembly_scanner = new AssemblyScanner();

		string value;
		public virtual string Value {
			get { return value; }
			set { this.value = value; }
		}

		string description;
	    public virtual string Description {
			get { return string.IsNullOrEmpty( description ) ? wordify(Value) : description; }
			set { description = value; }
	    }

	    static readonly Dictionary<Type, EnumerationInfo> enumeration_information_cache = new Dictionary<Type, EnumerationInfo>();
        static readonly Dictionary<string,Type> enumeration_types = new Dictionary<string, Type>();

		public static T Parse< T >( string value ) where T : Enumeration {
			if( string.IsNullOrEmpty( value ) ) return null;

			// TODO: verify if in this case FirstOrDefault returns null or an empty instance of the result item type
			var matching_item = GetAll<T>( ).FirstOrDefault( x => x.Value!=null && x.Value.ToUpper( ).Equals( value.ToUpper( ) ) );

			if( matching_item == null ) throw new ApplicationException(  "'"+ value +"' is not a valid " + typeof( T ) );

			return matching_item;
		}


		public static IEnumerable<TEnumeration> GetAll< TEnumeration >( ) where TEnumeration : Enumeration {
			var enumeration_type = typeof( TEnumeration );
		    var enumeration_informations = GetEnumerationInformations(enumeration_type);

            foreach (var field_info in enumeration_informations.Fields){
				var instance = Activator.CreateInstance( enumeration_type );
				yield return field_info.GetValue( instance ) as TEnumeration;
			}

			foreach( var property_info in enumeration_informations.Properties ) {
				var instance = Activator.CreateInstance( enumeration_type );
				yield return property_info.GetValue( instance, null ) as TEnumeration;
			}
		}

	    public static IEnumerable<Enumeration> GetAllEnumeration(string enumeration_name) {
	        var enumeration_type = GetEnumerationType(enumeration_name.ToLower());
            var handler_method_info = typeof(Enumeration).GetMethod("GetAll", BindingFlags.Static | BindingFlags.Public);
            var typed_handler = handler_method_info.MakeGenericMethod(enumeration_type);

            return (IEnumerable<Enumeration>) typed_handler.Invoke (null, null);
	    }

	    static EnumerationInfo GetEnumerationInformations(Type enumeration_type) {
            if ( enumeration_information_cache.ContainsKey(enumeration_type) ) return enumeration_information_cache[enumeration_type];

			var properties = enumeration_type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(x => x.PropertyType == enumeration_type).OrderBy(x => x.Name);
			var fields = enumeration_type.GetFields( BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly ).OrderBy( x => x.Name );

			return new EnumerationInfo { Properties = properties, Fields = fields };
	    }

	    static Type GetEnumerationType(string enumeration_name) {
            if( enumeration_types.ContainsKey(enumeration_name) ) return enumeration_types[enumeration_name];
			
            var enumeration_type  = assembly_scanner.GetAllInheritorsOf( typeof (Enumeration) ).FirstOrDefault(x => x.Name.ToLower() == enumeration_name );;

            if( enumeration_type==null ) throw new Exception( "Could not find enumeration '"+ enumeration_name +"'");

            enumeration_types.Add(enumeration_name,enumeration_type);

	        return enumeration_type;
	    }

		static string wordify( string s ) {
			var wordify_regex = new Regex( "(?<=[a-z])(?<x>[A-Z])|(?<=.)(?<x>[A-Z])(?=[a-z])" );
			return string.IsNullOrEmpty(s) ? String.Empty : wordify_regex.Replace( s.Replace( " ", "" ), " ${x}" );
		}

	    public virtual bool Equals( Enumeration other ) {
			if( ReferenceEquals( null, other ) ) return false;
			return ReferenceEquals( this, other ) || Equals( other.Value.ToUpper( ), Value.ToUpper( ) );
		}

		public override bool Equals( object obj ) {
			if( ReferenceEquals( null, obj ) ) return false;
			if( ReferenceEquals( this, obj ) ) return true;

			return obj is Enumeration && Equals( ( Enumeration ) obj );
		}

		public override int GetHashCode( ) {
			return ( Value != null ? Value.GetHashCode( ) : 0 );
		}

		public static bool operator == (Enumeration left, Enumeration right) {
			return Equals( left, right );
		}

		public static bool operator != ( Enumeration left, Enumeration right ) {
			return !Equals( left, right );
		}

		public override string ToString( ) {
		    return Description;
		}

		public static bool CanParse< T >( string enumeration_value ) where T : Enumeration {
			T result;
			return TryParse( enumeration_value, out result );
		}

		public static T SafeParse< T >( string enumeration_value ) where T : Enumeration {
			T result;
			return TryParse( enumeration_value, out result ) ? result : null;
		}

		public static bool TryParse< T >( string enumeration_value, out T result ) where T : Enumeration {
			try {
				result = Parse<T>( enumeration_value );
				return true;
			} catch( Exception ) {
				result = null;
				return false;
			}
		}
	}

    class EnumerationInfo{
        public IEnumerable<PropertyInfo> Properties { get; set; }
        public IEnumerable<FieldInfo> Fields { get; set; }
    }
}