using Castle.DynamicProxy;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonDuckTyper
{
  
    
    public static class JObjectExtension
    {
        internal static ProxyGenerator generator = new ProxyGenerator();

        public static InterfaceType toProxy<InterfaceType>(this JToken targetObject)
        {
            
            return (InterfaceType)targetObject.toProxy(typeof(InterfaceType));
        }

        
        public static object toProxy(this JToken targetToken, Type interfaceType)
        {
            if (interfaceType.GetTypeInfo().IsValueType || interfaceType.GetTypeInfo().IsPrimitive || interfaceType.Equals(typeof(string))  || interfaceType.GetTypeInfo().IsEnum)
            {
                return targetToken.ToObject(interfaceType);
            }

            if (interfaceType.Equals(typeof(decimal)))
            {
                //Due to a quirk in how newtonsoft handles large numbers, they need to be passed in as a string
                if (targetToken.Type == JTokenType.String)
                {
                    return Decimal.Parse(targetToken.Value<string>());
                }
                else
                {
                    return targetToken.ToObject(interfaceType);
                }
            }

            if (typeof(JContainer).IsAssignableFrom(interfaceType))
            {
                return targetToken;
            }

            if (interfaceType.GetTypeInfo().IsClass)
            {
                throw new Exception("Cannot Proxy " + interfaceType.FullName + " - It is a concrete class, only interfaces can be proxied.");
            }


            return generator.CreateInterfaceProxyWithoutTarget(interfaceType, new[] { typeof(JTokenExposable), typeof(ShouldSerialize) }, new JTokenInterceptor(targetToken, interfaceType));


        }

       
        
    }

    public interface JTokenExposable
    {
        JToken getTargetJToken();
    }

    public interface ShouldSerialize
    {
        //Control serialization of Dynamic Proxy meta data
        bool ShouldSerialize__interceptors();
        bool ShouldSerialize__target();
    }


    
    public class JTokenInterceptor : IInterceptor
    {
        private JToken _target;
        internal Type _proxyType;

        public JTokenInterceptor(JToken target, Type proxyType)
        {
            _target = target;
            _proxyType = proxyType;
        }
        public void Intercept(IInvocation invocation)
        {
            
            var methodName = invocation.Method.Name;

            if(methodName == "getTargetJToken")
            {
                invocation.ReturnValue = _target;
                return;
            }

            if(methodName == "get_Count")
            {
                invocation.ReturnValue = _target.Count();
                return;
            }

            if (methodName == "get_Item")
            {
                invocation.ReturnValue = _target[invocation.Arguments[0]].toProxy(invocation.Method.ReturnType);
                return;
            }

            if (methodName == "get_ChildrenTokens")
            {
                invocation.ReturnValue = _target.Children().ToList();
                return;
            }

            if (methodName == "GetEnumerator" && invocation.Method.ReturnType.IsConstructedGenericType )
            {

                var returnType = invocation.Method.ReturnType.GetGenericArguments()[0];
                
                Type enumeratorType = typeof(IEnumerator<>).MakeGenericType(returnType);
                if(_target.Type == JTokenType.Array)
                {
                    invocation.ReturnValue = JObjectExtension.generator.CreateInterfaceProxyWithoutTarget(enumeratorType,  new EnumeratorInterceptor(returnType, this,((JArray)_target).GetEnumerator()));
                }
                else
                {
                    invocation.ReturnValue = JObjectExtension.generator.CreateInterfaceProxyWithoutTarget(enumeratorType,  new EnumeratorInterceptor(returnType, this, ((JObject)_target).GetEnumerator()));
                }
                
                return;


            }

            if (methodName == "GetEnumerator" )
            {
                
                invocation.ReturnValue = JObjectExtension.generator.CreateInterfaceProxyWithoutTarget(typeof(IEnumerator), new EnumeratorInterceptor(typeof(IEnumerator), this,((JArray)_target).GetEnumerator()));
              
                return;


            }


            if (invocation.Method.IsSpecialName && methodName.StartsWith("get_"))
            {
                var returnType = invocation.Method.ReturnType;
                methodName = methodName.Substring(4);

                if (_target == null || !_target.HasValues || _target[methodName] == null)
                {
                    if (returnType.GetTypeInfo().IsPrimitive)
                    {
                        
                        invocation.ReturnValue = Activator.CreateInstance(returnType);
                        return;
                    }
                    
                    invocation.ReturnValue = null;
                    return;
                    
                    
                }

                if(_target[methodName] is JArray)
                {
                    invocation.ReturnValue = ((JArray)_target[methodName]).toProxy(returnType);
                    return;
                }


                invocation.ReturnValue = _target[methodName].toProxy(returnType);
                return;
   
            }

            if (invocation.Method.IsSpecialName && methodName.StartsWith("set_"))
            {
                methodName = methodName.Substring(4);
                var interfaceType = invocation.Arguments[0].GetType();
                
                if (interfaceType.GetTypeInfo().IsPrimitive || interfaceType.Equals(typeof(string)) || interfaceType.Equals(typeof(decimal)) || interfaceType.GetTypeInfo().IsEnum)
                {
                    _target[methodName] = new JValue(invocation.Arguments[0]);
                    return;
                }

                _target[methodName] = JObject.FromObject(invocation.Arguments[0]);
                return;
            }

            if(methodName == "ShouldSerialize__interceptors" || methodName ==  "ShouldSerialize__target")
            {
                // Prevent serialization of Castle Dynamic meta properties
                invocation.ReturnValue = false;
                return;
            }

            throw new NotImplementedException("Only get and set accessors are implemented in proxy");
            

        }
    }

    

    
    public class EnumeratorInterceptor : IInterceptor
    {
        private IEnumerator _targetEnumerator;
        private Type _enumeratorType;
        private JTokenInterceptor _parentInterceptor;

        public EnumeratorInterceptor(Type enumeratorType, JTokenInterceptor parentInterceptor, IEnumerator targetEnumerator )
        {
            _targetEnumerator = targetEnumerator;
            _enumeratorType = enumeratorType;
            _parentInterceptor = parentInterceptor;
        }
        public void Intercept(IInvocation invocation)
        {
            switch (invocation.Method.Name)
            {
                case "MoveNext":
                    invocation.ReturnValue = _targetEnumerator.MoveNext();
                    return;
                case "Dispose":
                    if(_enumeratorType.Name.StartsWith("KeyValuePair"))
                    {
                        ((IEnumerator<KeyValuePair<string, JToken>>)_targetEnumerator).Dispose();
                    }
                    else
                    {
                        ((IEnumerator<JToken>)_targetEnumerator).Dispose();
                    }
                   
                    return;
                case "get_Current":
                    var dictTypes = invocation.Method.ReturnType.GetGenericArguments();
                    if(dictTypes.Length <= 1)
                    {
                        var jTokenEnumerator = (IEnumerator<JToken>)_targetEnumerator;
            
                        invocation.ReturnValue = ((IEnumerator<JToken>)_targetEnumerator).Current.toProxy(_parentInterceptor._proxyType.GenericTypeArguments[0]);
                        return;
                    }

                    if (dictTypes.Length == 2)
                    {
                        if (dictTypes[0].Name == "String")
                        {
                            var kvpEnumerator = (IEnumerator<KeyValuePair<string, JToken>>)_targetEnumerator;
                            dynamic returnKVP = Activator.CreateInstance(invocation.Method.ReturnType, kvpEnumerator.Current.Key, kvpEnumerator.Current.Value.toProxy(dictTypes[1]));
                            invocation.ReturnValue = returnKVP;
                            return;
                        }
                        else
                        {
                            throw new NotImplementedException("Dictionaries must have a string key");
                        }
                    }

                    throw new NotImplementedException("Unknown Enumerator Type");

                default:
                    throw new NotImplementedException();

            }
        }

       
    }

    
}
