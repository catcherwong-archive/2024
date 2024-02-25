Console.WriteLine("Hello, World!");

internal interface MyInterface { }


internal sealed class MyObj<A> where A : class
{
    public A? value { get; set; }

    public void AMethod()
    {
        throw new NotImplementedException();
    }
}

internal sealed class myStruct { }
