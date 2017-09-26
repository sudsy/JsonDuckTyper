using JsonDuckTyper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace JsonDuckTyperTests
{
    public class JObjectTests
    {

        [Fact]
        public void deserializeFromInterface()
        {
            var anonObj = new
            {
                firstField = 1,
                secondField = "test",
                enumValue = 1
            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<basicInterface>();
            
            Assert.IsAssignableFrom<basicInterface>(newClass);
            Assert.Equal(anonObj.firstField, newClass.firstField);
            Assert.Equal(anonObj.secondField, newClass.secondField);
            Assert.Equal(anonObj.enumValue, (int)newClass.enumValue);
        }


        [Fact]
        public void reSerialize()
        {
            var anonObj = new
            {
                firstField = 1,
                secondField = "test",
                enumValue = 1
            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<basicInterface>();

            var anonSerialized = myObject.ToString();
            var newJObj = JObject.FromObject(newClass);
            
            var newClassSerialized = newJObj.ToString();
            
            Assert.Equal(anonSerialized, newClassSerialized);
            
        }

        [Fact]
        public void reSerializeComposed()
        {
            var anonObj = new
            {
                firstField = 1,
                secondField = "test",
                enumValue = 1
            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<basicInterface>();

            var composedClass = new composedWithInterface {
                composedField = newClass
            };

            var anonSerialized = myObject.ToString();
            var composedSerialized = JsonConvert.SerializeObject(composedClass);
            var newJObj = JObject.Parse(composedSerialized);

            var newClassSerialized = newJObj["composedField"].ToString();

            Assert.Equal(anonSerialized, newClassSerialized);

        }

        

        [Fact]
        public void nullableInterfaceTest()
        {
            var anonObj = new
            {
                
            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<nullableInterface>();

            Assert.IsAssignableFrom<nullableInterface>(newClass);
            Assert.Null(newClass.firstField);
            
        }

        [Fact]
        public void writeToProxy()
        {
            var anonObj = new
            {
                firstField = 1,
                secondField = "test"
            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<basicInterface>();
            newClass.firstField = 2;
            newClass.secondField = "another test";

            Assert.IsAssignableFrom<basicInterface>(newClass);
            Assert.Equal(2, newClass.firstField);
            Assert.Equal("another test", newClass.secondField);
        }

        [Fact]
        public void deserializeFromInheritedInterface()
        {
            var anonObj = new
            {
                firstField = 1,
                secondField = "test",
                thirdField = 3
            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<interfaceWithInheritance>();

            Assert.IsAssignableFrom<interfaceWithInheritance>(newClass);
            Assert.Equal(anonObj.firstField, newClass.firstField);
            Assert.Equal(anonObj.secondField, newClass.secondField);
            Assert.Equal(anonObj.thirdField, newClass.thirdField);
        }

        [Fact]
        public void deserializeFromComposedInterface()
        {
            var anonObj = new
            {
                firstField = 1,
                secondField = "test",
                thirdField = 3,
                composedField = new
                {
                    firstField = 4,
                    secondField = "composed"
                }
            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<composedInterface>();

            Assert.IsAssignableFrom<composedInterface>(newClass);
            Assert.Equal(anonObj.firstField, newClass.firstField);
            Assert.Equal(anonObj.secondField, newClass.secondField);
            Assert.Equal(anonObj.thirdField, newClass.thirdField);
            Assert.Equal(anonObj.composedField.firstField, newClass.composedField.firstField);
            Assert.Equal(anonObj.composedField.secondField, newClass.composedField.secondField);
        }

        [Fact]
        public void deserializeFromComposedWithNulls()
        {
            var anonObj = new
            {
                firstField = 1,
                secondField = "test",
                thirdField = 3,
                
            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<composedInterface>();

            Assert.IsAssignableFrom<composedInterface>(newClass);
            Assert.Equal(anonObj.firstField, newClass.firstField);
            Assert.Equal(anonObj.secondField, newClass.secondField);
            Assert.Equal(anonObj.thirdField, newClass.thirdField);
            //Assert.Equal(0, newClass.composedField.firstField);
            Assert.Equal(null, newClass.composedField);
        }

        [Fact]
        public void deserializeFromComposedInterfaceMissingProperty()
        {
            var anonObj = new
            {
                firstField = 1,
                thirdField = 3,
                composedField = new
                {
                    
                    secondField = "composed"
                }
            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<composedInterface>();

            Assert.IsAssignableFrom<composedInterface>(newClass);
            Assert.Equal(anonObj.firstField, newClass.firstField);
            Assert.Equal(null, newClass.secondField);
            Assert.Equal(anonObj.thirdField, newClass.thirdField);
            Assert.Equal(0, newClass.composedField.firstField);
            Assert.Equal(anonObj.composedField.secondField, newClass.composedField.secondField);
        }

        [Fact]
        public void deserializeFromEnumerableInterface()
        {
            var anonObj = new
            {
                enumString = new[] { "hello" },
                enumDouble = new [] { 1, 2, 3 },
                enumInterface = new []
                {
                    new { firstField = 1, secondField = "two"},
                    new { firstField = 3, secondField = "four"}
                }
                
            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<enumerableInterface>();

            Assert.IsAssignableFrom<enumerableInterface>(newClass);
            Assert.Equal(anonObj.enumString.Length, newClass.enumString.Count());
            Assert.Equal(anonObj.enumString.ElementAt(0), newClass.enumString.ElementAt(0));
            Assert.Equal(anonObj.enumDouble.Length, newClass.enumDouble.Count());
            Assert.Equal(anonObj.enumDouble.ElementAt(0), newClass.enumDouble.ElementAt(0));
            Assert.Equal(anonObj.enumDouble.ElementAt(1), newClass.enumDouble.ElementAt(1));
            Assert.Equal(anonObj.enumDouble.ElementAt(2), newClass.enumDouble.ElementAt(2));
            Assert.Equal(anonObj.enumInterface.Length, newClass.enumInterface.Count());
            Assert.Equal(anonObj.enumInterface.ElementAt(0).firstField, newClass.enumInterface.ElementAt(0).firstField);
            Assert.Equal(anonObj.enumInterface.ElementAt(1).firstField, newClass.enumInterface.ElementAt(1).firstField);
            Assert.Equal(anonObj.enumInterface.ElementAt(0).secondField, newClass.enumInterface.ElementAt(0).secondField);
            Assert.Equal(anonObj.enumInterface.ElementAt(1).secondField, newClass.enumInterface.ElementAt(1).secondField);
            
            
        }

        [Fact]
        public void listOfList()
        {
            var anonObj = new
            {
                listOfList = new []
                {
                    new []
                    {
                        new { firstField = 1, secondField = "two", enumValue = 1},
                        new { firstField = 3, secondField = "four", enumValue = 1}
                    },
                     new []
                    {
                        new { firstField = 5, secondField = "six", enumValue = 1},
                        new { firstField = 7, secondField = "eight", enumValue = 1}
                    }

                }

            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<listOfList>();

            Assert.IsAssignableFrom<listOfList>(newClass);
            Assert.Equal(anonObj.listOfList[0][1].firstField, newClass.listOfList[0][1].firstField);
            Assert.Equal(anonObj.listOfList[0][1].secondField, newClass.listOfList[0][1].secondField);
            Assert.Equal(anonObj.listOfList[1][1].firstField, newClass.listOfList[1][1].firstField);
            Assert.Equal(anonObj.listOfList[1][1].secondField, newClass.listOfList[1][1].secondField);

            JsonConvert.SerializeObject(newClass);
        }


        [Fact]
        public void reSerializeEnumerable()
        {
            var anonObj = new
            {
                enumString = new[] { "hello" },
                enumDouble = new[] { 1.0, 2.0, 3.0 },
                enumInterface = new[]
                {
                    new { firstField = 1, secondField = "two", enumValue = 1 },
                    new { firstField = 3, secondField = "four", enumValue = 1}
                }

            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<enumerableInterface>();

            var anonSerialized = myObject.ToString(Formatting.None);
            //var newJObj = JObject.FromObject(newClass);

            //var newClassSerialized = newJObj.ToString()
            var newClassSerialized = JsonConvert.SerializeObject(newClass);

            Assert.Equal(anonSerialized, newClassSerialized);

        }

        [Fact]
        public void deserializeFromListInterface()
        {
            var anonObj = new
            {
                enumString = new[] { "hello" },
                enumDouble = new[] { 1, 2, 3 },
                enumInterface = new[]
                {
                    new { firstField = 1, secondField = "two"},
                    new { firstField = 3, secondField = "four"}
                },
                enumStandard = new[]
                {
                    new { test = 1 }
                }
            

            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<listInterface>();

            Assert.IsAssignableFrom<listInterface>(newClass);
            Assert.Equal(anonObj.enumString.Length, newClass.enumString.Count());
            Assert.Equal(anonObj.enumString.ElementAt(0), newClass.enumString.ElementAt(0));
            Assert.Equal(anonObj.enumDouble.Length, newClass.enumDouble.Count());
            Assert.Equal(anonObj.enumDouble.ElementAt(0), newClass.enumDouble.ElementAt(0));
            Assert.Equal(anonObj.enumDouble.ElementAt(1), newClass.enumDouble.ElementAt(1));
            Assert.Equal(anonObj.enumDouble.ElementAt(2), newClass.enumDouble.ElementAt(2));
            Assert.Equal(anonObj.enumInterface.Length, newClass.enumInterface.Count());
            Assert.Equal(anonObj.enumInterface.ElementAt(0).firstField, newClass.enumInterface.ElementAt(0).firstField);
            Assert.Equal(anonObj.enumInterface.ElementAt(1).firstField, newClass.enumInterface.ElementAt(1).firstField);
            Assert.Equal(anonObj.enumInterface.ElementAt(0).secondField, newClass.enumInterface.ElementAt(0).secondField);
            Assert.Equal(anonObj.enumInterface.ElementAt(1).secondField, newClass.enumInterface.ElementAt(1).secondField);
            Assert.Equal(anonObj.enumStandard.Length, newClass.enumStandard.Count);

        }

        [Fact]
        public void deserializeDictionaryInterface()
        {
            var anonObj = new
            {
                dictString = new Dictionary<string, string> { { "stringKey", "stringValue" } },
                dictDouble = new Dictionary<string, double> { { "doubleKey", 3 } },
                dictInterface = new Dictionary<string, object>
                {
                    {"interfaceKey", new { firstField = 1, secondField = "two"} },        
                }

            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<dictionaryInterface>();

            Assert.IsAssignableFrom<dictionaryInterface>(newClass);
            Assert.Equal(anonObj.dictString.Count, newClass.dictString.Count);
            Assert.Equal(anonObj.dictString.ElementAt(0).Key, newClass.dictString.ElementAt(0).Key);
            Assert.Equal(anonObj.dictString.ElementAt(0).Value, newClass.dictString.ElementAt(0).Value);
            Assert.Equal(anonObj.dictDouble.Count, newClass.dictDouble.Count);
            Assert.Equal(anonObj.dictDouble.ElementAt(0).Key, newClass.dictDouble.ElementAt(0).Key);
            Assert.Equal(anonObj.dictDouble.ElementAt(0).Value, newClass.dictDouble.ElementAt(0).Value);
            Assert.Equal(anonObj.dictInterface.Count, newClass.dictInterface.Count);
            Assert.Equal(anonObj.dictInterface.ElementAt(0).Key, newClass.dictInterface.ElementAt(0).Key);
            Assert.Equal(1, newClass.dictInterface.ElementAt(0).Value.firstField);
            Assert.Equal("two", newClass.dictInterface.ElementAt(0).Value.secondField);

        }

        [Fact]
        public void deserializeDictListInterface()
        {
            var anonObj = new
            {

                dictOfList = new Dictionary<string, List<double>>
                {
                    {"theKey", new List<double> { 1,2,3 } },
                }

            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<dictoflistInterface>();

            Assert.IsAssignableFrom<dictoflistInterface>(newClass);
            //Try casting
            Dictionary<string, IList<double>> thisDict = new Dictionary<string, IList<double>>(newClass.dictOfList);
            //Assert.Equal(anonObj.dictString.Count, newClass.dictString.Count);
            //Assert.Equal(anonObj.dictString.ElementAt(0).Key, newClass.dictString.ElementAt(0).Key);
            //Assert.Equal(anonObj.dictString.ElementAt(0).Value, newClass.dictString.ElementAt(0).Value);
            //Assert.Equal(anonObj.dictDouble.Count, newClass.dictDouble.Count);
            //Assert.Equal(anonObj.dictDouble.ElementAt(0).Key, newClass.dictDouble.ElementAt(0).Key);
            //Assert.Equal(anonObj.dictDouble.ElementAt(0).Value, newClass.dictDouble.ElementAt(0).Value);
            //Assert.Equal(anonObj.dictInterface.Count, newClass.dictInterface.Count);
            //Assert.Equal(anonObj.dictInterface.ElementAt(0).Key, newClass.dictInterface.ElementAt(0).Key);
            //Assert.Equal(1, newClass.dictInterface.ElementAt(0).Value.firstField);
            //Assert.Equal("two", newClass.dictInterface.ElementAt(0).Value.secondField);

        }

        [Fact]
        public void ToConcreteClass()
        {
            var anonObj = new { firstField = 1, secondField = "two" };
                   
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<basicInterface>();
            var underlyingToken = ((JTokenExposable)newClass).getTargetJToken();
            var concrete = underlyingToken.ToObject<basicClass>();
            Assert.Equal(anonObj.firstField, concrete.firstField);
            Assert.Equal(anonObj.secondField, concrete.secondField);
        }

        [Fact]
        public void toConcreteComposed()
        {
            var anonObj = new
            {
                firstField = 1,
                secondField = "test",
                thirdField = 3,
                composedField = new
                {
                    firstField = 4,
                    secondField = "composed"
                }
            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<composedInterface>();
            var underlyingToken = ((JTokenExposable)newClass).getTargetJToken();
            var concrete = underlyingToken.ToObject<composedClass>();

            Assert.Equal(anonObj.firstField, concrete.firstField);
            Assert.Equal(anonObj.secondField, concrete.secondField);
            Assert.Equal(anonObj.thirdField, concrete.thirdField);
            Assert.Equal(anonObj.composedField.firstField, concrete.composedField.firstField);
            Assert.Equal(anonObj.composedField.secondField, concrete.composedField.secondField);


        }

        [Fact]
        public void jobjTest()
        {
            var anonObj = new
            {
                unParsed = new
                {
                    firstField = 4,
                    secondField = "composed"
                }
            };
            var myObject = JObject.FromObject(anonObj);
            var newClass = myObject.toProxy<jObjInterface>();
            
            
            Assert.Equal(anonObj.unParsed.firstField, newClass.unParsed["firstField"].Value<int>());
            Assert.Equal(anonObj.unParsed.secondField, newClass.unParsed["secondField"].Value<string>());


        }
    }

    
    //Also need test for IDictionary and maybe IList
    public enum TestEnum 
    {
        firstValue = 0,
        secondValue = 1
    }
    public interface basicInterface
    {
        int firstField { get; set; }
        string secondField { get; set; }
        TestEnum enumValue { get; set; }
    }

    public interface nullableInterface
    {
        int? firstField { get; set; }
        
    }

    public interface interfaceWithInheritance : basicInterface
    {
        int thirdField { get; }

    }

    public interface jObjInterface
    {
        JObject unParsed { get; }
    }

    public interface composedInterface : interfaceWithInheritance
    {
        basicInterface composedField { get; }
    }

    public interface enumerableInterface
    {
        IEnumerable<string> enumString { get; }
        IEnumerable<double> enumDouble { get; }
        IEnumerable<basicInterface> enumInterface { get; }
    }

    public interface dictionaryInterface
    {
        IDictionary<string, string> dictString { get; }
        IDictionary<string, double> dictDouble { get; }
        IDictionary<string, basicInterface> dictInterface { get; }
    }

    public interface listInterface
    {
        IList<string> enumString { get; }
        IList<double> enumDouble { get; }
        IList<basicInterface> enumInterface { get; }
        IList enumStandard { get; }
    }

    public interface listOfList
    {
        IList<IList<basicInterface>> listOfList { get; }
    }
   

    public interface dictoflistInterface
    {
        IDictionary<string, IList<double>> dictOfList { get; }
        
    }

    public class listClass : listInterface
    {

        public IList<string> enumString { get; set; }

        public IList<double> enumDouble { get; set; }

        public IList<basicInterface> enumInterface { get; set; }

        public IList enumStandard { get; set; }

        public listClass(List<string> enumString, List<double> enumDouble, List<basicClass> enumInterface)
        {
            this.enumString = enumString;
            this.enumDouble = enumDouble;
            this.enumInterface = enumInterface.ToList<basicInterface>();
        }
    }

    public class basicClass : basicInterface
    {
        public int firstField { get; set; }

        public string secondField { get; set; }
        public TestEnum enumValue { get; set; }
    }

    public class composedClass : composedInterface
    {
        public composedClass(basicClass composedField)
        {
            this.composedField = composedField;
        }
        public basicInterface composedField { get; set; }

        public int thirdField { get; set; }

        public int firstField { get; set; }

        public string secondField { get; set; }
        public TestEnum enumValue { get; set; }
    }

    public class composedWithInterface
    {
        public basicInterface composedField;
    }
}
