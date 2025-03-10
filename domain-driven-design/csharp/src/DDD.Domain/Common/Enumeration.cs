namespace DDD.Domain.Common;

public abstract class Enumeration : IComparable
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    
    protected Enumeration(int id, string name) => (Id, Name) = (id, name);
    
    public override string ToString() => Name;
    
    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        return typeof(T)
            .GetFields(System.Reflection.BindingFlags.Public | 
                       System.Reflection.BindingFlags.Static | 
                       System.Reflection.BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>();
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
            return false;
            
        var typeMatches = GetType() == obj.GetType();
        var valueMatches = Id.Equals(otherValue.Id);
        
        return typeMatches && valueMatches;
    }
    
    public override int GetHashCode() => Id.GetHashCode();
    
    public static T FromId<T>(int id) where T : Enumeration
    {
        return GetAll<T>().First(item => item.Id == id);
    }
    
    public static T FromName<T>(string name) where T : Enumeration
    {
        return GetAll<T>().First(item => item.Name == name);
    }
    
    public int CompareTo(object? other) => Id.CompareTo(((Enumeration)other!).Id);
}
