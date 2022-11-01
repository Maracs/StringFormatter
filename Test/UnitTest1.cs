namespace Test;

public class Tests
{
   
    
    class User
    {
        public string FirstName { get; }
        public string LastName { get; }
        public string[] Orders { get; }
    
        public User(string firstName, string lastName, string[] orders)
        {
            FirstName = firstName;
            LastName = lastName;
            Orders = orders;
        }
        
        public string GetGreeting()
        {
            return StringFormatter.Core.StringFormatter.Shared.Format(
                "Привет, {FirstName} {LastName}!", this);
        }

        public string GetFirstOrderReportString()
        {
            return StringFormatter.Core.StringFormatter.Shared.Format(
                "{FirstName} {LastName} заказал(а) {Orders[0]}", this);
        }
        
        public string GetSecondOrderReportString()
        {
            return StringFormatter.Core.StringFormatter.Shared.Format(
                "{FirstName} {LastName} заказал(а) {Orders[1]}", this);
        }
        
    }

    class A
    {
        private int _size;

        public bool is_size = false;

        public string Str_size
        {
            get;
            set;
        } 

       public  A(int size)
        {
            _size = size;
           
        }

       public void SetStrSize()
       {
           Str_size = StringFormatter.Core.StringFormatter.Shared.Format("{{_size}}  {is_size}", this);
       }
    }
    
    
    
    
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void PublicCollectionAccessTest()
    {
        
        var user = new User("Петя", "Иванов", new []{"утюг"});
        
        var fullName = user.GetFirstOrderReportString();
        
        Assert.That(fullName,Is.EqualTo("Петя Иванов заказал(а) утюг"));
        
    }
    
    [Test]
    public void PublicCollectionOutOfRangeTest()
    {
        
        var user = new User("Илья", "Павлов", new []{"утюг"});
        
        Assert.Throws<Exception>(() =>   user.GetSecondOrderReportString());
        
    }
    
    
    [Test]
    public void PublicPropertyAccessTest()
    {
        
        var user = new User("Петя", "Иванов", new []{"утюг"});
        
        var fullName = user.GetGreeting(); 
        
        Assert.That(fullName,Is.EqualTo("Привет, Петя Иванов!"));
        
    }
    
    [Test]
    public void PrivateFieldAccessTest()
    {
        A test = new(10);

        string t = StringFormatter.Core.StringFormatter.Shared.Format("size is {_size}", test);

        Assert.That(t,Is.EqualTo("size is 10"));
    }
    
    [Test]
    public void InvalidSyntaxTest1()
    {
        A test = new(10);

        Assert.Throws<Exception>(() =>  StringFormatter.Core.StringFormatter.Shared.Format("size is {{_size} or {_size}}", test));

        
    }
    
    [Test]
    public void InvalidSyntaxTest2()
    {
        A test = new(10);

        Assert.Throws<Exception>(() =>  StringFormatter.Core.StringFormatter.Shared.Format("size is {{_size} ", test));

        
    }
    
    [Test]
    public void InvalidSyntaxTest3()
    {
        A test = new(10);

        Assert.Throws<Exception>(() =>  StringFormatter.Core.StringFormatter.Shared.Format("size {is {_size}}", test));
        
    }
    
    [Test]
    public void CommentTest()
    {
        A test = new(10);

        Assert.That(  StringFormatter.Core.StringFormatter.Shared.Format("{{_size}}  {_size}", test),Is.EqualTo("{_size}  10"));
        
    }
    
    [Test]
    public void MultiThreadTest()
    {
        A test = new(10);
        string a= StringFormatter.Core.StringFormatter.Shared.Format("{{_size}}  {_size}", test);

        string b="";
        
        Thread thread = new Thread(()=>
        {
            
             b= StringFormatter.Core.StringFormatter.Shared.Format("{{_size}}  {_size}", test);
            
            test.SetStrSize();
        });
        
        thread.Start();
        
        Thread.Sleep(100);
        
       
        Assert.That(a,Is.EqualTo("{_size}  10"));
        Assert.That(b,Is.EqualTo("{_size}  10"));
        Assert.That(test.Str_size,Is.EqualTo("{_size}  False"));
        
        
        
    }

    
}