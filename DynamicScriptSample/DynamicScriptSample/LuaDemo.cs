using MoonSharp;
using MoonSharp.Interpreter;

public static class LuaDemo
{
    public static void Test()
    {
        UserData.RegisterType<Dto>();

        var sc = @"function convert(dto)
    local arr = {};
    if dto.Param0 ~= nil then
        table.insert(arr, 'catcher-' .. dto.Param0.p1);
    end
    
    table.insert(arr, dto.Param1);
    table.insert(arr, dto.Param2);
    return arr
end";

        
        Script script = new();
        script.DoString(sc);
        DynValue function = script.Globals.Get("convert");
        
        var res = script.Call(function, new Dto
        {
            Param0 = new Dictionary<string, string>
            {
                { "p1", "123"  },
                { "p2", "456"  },
            },
            Param1 = "520",
            Param2 = 1314
        });

        var arr = res.ToObject<List<string>>();

        foreach (var item in arr)
        {
            Console.WriteLine(item);
        }
    }

    public class Dto
    {
        public Dictionary<string, string> Param0 { get; set; }

        public string Param1 { get; set; }

        public int Param2 { get; set; }
    }
}