using System.Reflection;
using Castle.DynamicProxy;

namespace Core.Utilities.Interceptors
{
    public class AspectInterceptorSelector : IInterceptorSelector
    {
        public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
        {
            var classAttributes = type.GetCustomAttributes<MethodInterceptionBaseAttribute>
                (true).ToList();
                
            // Metodu parametre tipleri ile birlikte bulmaya çalışalım
            MethodInfo classMethod = null;
            
            try
            {
                // Önce tam eşleşme ile metodu bulmaya çalışalım (metot adı ve parametre tipleri)
                var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                classMethod = type.GetMethod(method.Name, parameterTypes);
                
                // Eğer bulamazsak, sadece metot adıyla deneyelim
                if (classMethod == null)
                {
                    classMethod = type.GetMethod(method.Name);
                }
                
                // Eğer hala bulamazsak, tüm metotlar arasından isim eşleşmesine bakalım
                if (classMethod == null)
                {
                    classMethod = type.GetMethods()
                        .FirstOrDefault(m => string.Equals(m.Name, method.Name, StringComparison.Ordinal));
                }
            }
            catch
            {
                // Herhangi bir hata durumunda boş bir liste döndürelim
                return classAttributes.OrderBy(x => x.Priority).ToArray();
            }
            
            // Eğer metot bulunduysa, özniteliklerini ekleyelim
            if (classMethod != null)
            {
                var methodAttributes = classMethod.GetCustomAttributes<MethodInterceptionBaseAttribute>(true);
                classAttributes.AddRange(methodAttributes);
            }

            return classAttributes.OrderBy(x => x.Priority).ToArray();
        }
    }
}
