using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace StringFormatter.Core;

public class StringFormatter
{
    public static readonly StringFormatter Shared = new StringFormatter();

    private ConcurrentDictionary<string, Func<object, string>> _cache;
    
    private ConcurrentDictionary<string, Func<object,int, string>> _cacheForCollection;
    
    private StringFormatter()
    {
        
        _cache = new ();
        _cacheForCollection = new();
    }


    private void CheckStringValidation(string input)
    {
        int sum=0;

        for(int i=0;i<input.Length;i++)
        {
           
                if (sum > 2 || sum < 0 ||
                    (sum==2 && input[i] == '}'&&(i+1>=input.Length || input[i+1]!='}')) ||
                    (sum==1 && input[i] == '{'&& input[i-1]!='{'))
                {
                    throw new Exception($"Invalid string at {i}");
                }

                if (input[i] == '{') {
                sum++;
              }
            
                if (input[i] == '}')
            {
                
                sum--;
            }
                
        }

        if (sum != 0)
        {
            throw new Exception($"Invalid string");
        }
        
    }

    private bool IsCollection(string input)
    {
        
        if (input.IndexOf("[") != -1 && input.IndexOf("]")!=-1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    
     private string GetCollectionString(string input, object target)
    {

        string cacheName = target.GetType().ToString()+"."+input;

        string name = input.Substring(0, input.IndexOf("["));
        
        string index = input.Substring(input.IndexOf("[") + 1, input.IndexOf("]") - input.IndexOf("[")-1);

        int ind;
        
        if (!Int32.TryParse(index, out ind))
        {
            throw new Exception($"Invalid index: "+input+" of obj: "+target.ToString() );
        }
        
        if (_cacheForCollection.ContainsKey(cacheName))
        {
            var func = _cacheForCollection[cacheName];
            
            return func(target,ind);
            
        }
        else
        {
            
            try{
                ParameterExpression indexExpr = Expression.Parameter(typeof(int), "index");
                
                ParameterExpression generalObjParam = Expression.Parameter(typeof(object), "obj");
                
                var curObjParam = Expression.PropertyOrField(Expression.TypeAs(generalObjParam, target.GetType()), name);
                
                var arrayAccessExpr = Expression.ArrayAccess(
                    curObjParam,
                    indexExpr
                );
                
                var b = Expression.Call(arrayAccessExpr, "ToString",null , null);

                Expression<Func<object,int, string>> exp =
                    Expression.Lambda<Func<object,int, string>>(b, new ParameterExpression[] { generalObjParam,indexExpr });
                
                Func<object,int, string> e = exp.Compile();
                
                _cacheForCollection.TryAdd(cacheName, e);

                var func = _cacheForCollection[cacheName];
                
                return func(target,ind);
            }
            catch (Exception exception)
            {
                       
                throw new Exception($"Can`t get field or property named: "+input+" of obj: "+target.ToString() );
            }
        }
    }
     
    private string GetObjectString(string input, object target)
    {
        

        string cacheName = target.GetType().ToString()+"."+input;

        if (_cache.ContainsKey(cacheName))
        {
            var func = _cache[cacheName];
            
            return func(target);
            
        }
        else
        {
            
            
            
                try
                {
                    ParameterExpression generalObjParam = Expression.Parameter(typeof(object), "obj");

                    var curObjParam = Expression.PropertyOrField(Expression.TypeAs(generalObjParam, target.GetType()), input);

                    var b = Expression.Call(curObjParam, "ToString", null, null);

                    Expression<Func<object, string>> exp =
                        Expression.Lambda<Func<object, string>>(b, new ParameterExpression[] { generalObjParam });

                    Func<object, string> e = exp.Compile();
                    
                    
                    
                    
                    
                    _cache.TryAdd(cacheName, e);

                    var func = _cache[cacheName];
                
                    return func(target);
                    
                }
                catch (Exception exception)
                {
                   
                    throw new Exception($"Can`t get field or property named: "+input+" of obj: "+target.ToString() );
                }
            
        }
        
       
    }

    private string ParseString(string input,object target)
    {
        string result="";

        bool isData = false, isComment = false;

        string workData="";
        
        for(int i=0; i<input.Length;i++)
        {
            if (input[i] == '{')
            {
                if (!isData)
                {
                    isData = true;
                    workData = "";

                }
                else
                {
                    isData = false;
                    isComment = true;
                    result += input[i];
                }
            }
            
            if (input[i] == '}')
            {
                if (isData)
                {
                    isData = false;
                    if (IsCollection(workData))
                    {
                        result += GetCollectionString(workData,target);
                    }
                    else
                    {
                        result += GetObjectString(workData,target);
                    }
                }

                if (isComment)
                {
                    isComment = false;
                    result += input[i];
                }
                
            }

            if (input[i] != '}' && input[i] != '{')
            {
                if (!isData)
                {
                    result += input[i];
                }

                {
                    workData += input[i];
                }
            }
        }

        return result;
    }
    
    public string Format(string template, object target) 
    {
        CheckStringValidation(template);
        
        return ParseString(template,target);
        
    }
    
   
    
}