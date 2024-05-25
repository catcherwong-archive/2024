// See https://aka.ms/new-console-template for more information
using Jint;

public static class JintDemo
{
    public static void Test()
    {
        var sc = @"function convert(dto) {
            var arr = [];
            if (dto.hasOwnProperty('Param0')) {
                arr.push('catcher-' + dto['Param0']['p1']);
            }

            arr.push(dto['Param1'])
            arr.push(dto['Param2'])

            return arr
        }";
        var engine = new Engine()
                   .Execute(sc);

        var result = engine.Invoke("convert", new Dto
        {
            Param0 = new Dictionary<string, string>
            {
                { "p1", "123"  },
                { "p2", "456"  },
            },
            Param1 = "520",
            Param2 = 1314
        });

        var arr = result.AsArray();

        foreach (var item in arr)
        {
            Console.WriteLine(item);
        }
    }

    public class Dto
    { 
        public Dictionary<string, string> Param0 {  get; set; }

        public string Param1 { get; set; }

        public int Param2 { get; set; }
    }
}