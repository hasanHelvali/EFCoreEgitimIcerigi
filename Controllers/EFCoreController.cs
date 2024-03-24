using EFCoreEgitimi.Context;
using EFCoreEgitimi.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreEgitimi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EFCoreController : ControllerBase
    {
        private readonly NorthwndContext _context;

        public EFCoreController(NorthwndContext dbontext)
        {
            _context = dbontext;
        }

        /*EFCore da id yapıları eger bir mudahale edilmediyse direkt olarak identity yani otomatik artan sekilde db ye alınır.*/


        //Temel veri ekleme
        public async void VeriEkleme()
        {
            #region Veri Nasıl Eklenir
            Employee employee = new Employee();

            await _context.AddAsync(employee);//Bu sekilde ekleme tip guvensiz olarak veri girisi saglar ve object kabul eder.

            await _context.Customers.AddAsync(new Customer//Bu sekilde tablo adı vererek ekleme ise tip guvenli bir sekilde ekleme yapmamızı saglar.
            {//Bir obje olusuturulur.
                Address = "",
                City = "",
                CompanyName = ""
                //...
            });
            //Gelen veri _context icerisindeki her bir field a yani db icindeki her bir tabloya uygun formatta ise eklenebilir.
            #endregion

            #region SaveChanges Nedir
            await _context.SaveChangesAsync();
            /*Ilgili yapılar once belege birer sorgu olarak alınır.
            Bu yapıların db tarafına execute edilebilmesi icin saveChanges cagrılmak zorundandır. Yani suan aslında bellekte bir sorgu var.
            Lakin bu sorgunun db de calıstırılması gerekiyor. Iste bunun yolu da bu fonksiyondur.
            Tanıma gore Insert,update,delete sorgularını olusturup bir transaction esliginde db ye gonderip execute eden fonskiyondur.
            Bu fonksiyon olusturulan sorgulardan herhangi birinin basarısız oldugu durumda butun islemleri geri alır(rollback).
            Bu fonksiyon butun efcore operasyonlarının en sonunda kullanılmaldıır. Cunku bu fonksiyon her kullanıldıgınd abir transition olusur 
            ve bunun execute edilmesi maliyettir. Butun ekleme silme vs ne yapılacaksa oncesinden yapılıp en son tek transition ile bunları db ye 
            aktarmak icin saveChanges kullanmak en aantajlı yoldur.*/
            #endregion

            #region Birden cok veri ekleme
            Employee employee1 = new Employee(){/*...*/};
            _context.Employees.Add(employee1);
            Employee employee2 = new Employee(){/*...*/};
            _context.Employees.Add(employee2);
            Employee employee3 = new Employee(){/*...*/};

            _context.Employees.Add(employee3);
            _context.SaveChangesAsync();
            //Seklinde bir ekleme yapılabilirken

            _context.Employees.AddRange(employee1, employee2, employee3);
            _context.SaveChangesAsync();
            //Seklinde bir ekleme de yapılabilir. Cunku AddRange metodu T tipinde bir params bekler.
            #endregion

            #region Eklenen verinin id sini elde etme 
            Employee e = new()
            {
                //...
            };
            await _context.Employees.AddAsync(e);
            await _context.SaveChangesAsync();
            //Buraya kadar e adında bir employee nesnesi db ye eklendi.
            await Console.Out.WriteLineAsync(e.EmployeeId.ToString());
            /*Bu sekilde biz eklenen nesnenin id sini gorebiliyoruz. Evet nesnenin id sine programatik olarak midahale edilmemisken 
             hemen bir satır asagısında id sine ulasabiliyoruz. Bunun sebebi bildigimiz OOP ayrıcalıklarıdır. e nesnesinin bir referansı var. 
            Bu referans nereye giderse gitsin aynıdır. Bellekte bu nesneye execute edildikten sonra bir id atandıgı icin o nesnenin id sine 
            hemen sonrasında ulasabiliyoruz. Set edilen degere hemen ulasabiliyoruz.
            */
            #endregion

        }

        


    }
}
