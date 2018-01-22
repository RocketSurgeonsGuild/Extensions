//The Inflector class was cloned from Inflector (https://github.com/srkirkland/Inflector)

//The MIT License (MIT)

//Copyright (c) 2013 Scott Kirkland

//Permission is hereby granted, free of charge, to any person obtaining a copy of
//this software and associated documentation files (the "Software"), to deal in
//the Software without restriction, including without limitation the rights to
//use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
//the Software, and to permit persons to whom the Software is furnished to do so,
//subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
//FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
//IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rocket.Surgery.Unions
{
    public class UnionConverter : JsonConverter
    {
        private readonly Type _enumType;
        private readonly IReadOnlyDictionary<object, Type> _enumTypeList;

        private readonly string _camelCasePropertyName;
        private readonly string _pascalCasePropertyName;
        private readonly string _dashCasePropertyName;
        private readonly string _propertyName;

        public UnionConverter(Type rootType, string propertyName)
        {
            _propertyName = propertyName;
            _camelCasePropertyName = Inflector.Camelize(propertyName);
            _pascalCasePropertyName = Inflector.Pascalize(propertyName);
            _dashCasePropertyName = Inflector.Kebaberize(propertyName);
            _enumType  = UnionHelper.GetUnionType(rootType, propertyName);
            _enumTypeList = UnionHelper.GetUnion(_enumType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = (JObject)JToken.ReadFrom(reader);
            var instance = Activator.CreateInstance(GetTypeToDeserializeTo(obj));
            serializer.Populate(obj.CreateReader(), instance);
            return instance;
        }

        private Type GetTypeToDeserializeTo(JObject obj)
        {
            var property =
                obj[_camelCasePropertyName] ??
                obj[_pascalCasePropertyName] ??
                obj[_dashCasePropertyName] ??
                throw new KeyNotFoundException($"Could not find property name for {_propertyName}");
            var value = property.Value<string>();
            var result = Enum.Parse(_enumType, value, true);
            return _enumTypeList[result];
        }

        #region Base Class
        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override bool CanConvert(Type objectType) => true;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
