using EFCoreEgitimi.Context;
using EFCoreEgitimi.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;

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

        
        public async void VeriGuncelleme()
        {
            #region Veri Nasıl Güncellenir
            /*Verinin guncellenmesi demek davranıssal olarak eklenmis bir verinin elde edilip üzeirnde bazı degisiklikleirn yapılması demektir.
            Dolayısıyla oncesinde veriyi elde etmem gerekir. */
            Employee employee = await _context.Employees.FirstOrDefaultAsync(x => x.EmployeeId == 3);//3 id li ürün bana getirilir.
            //Bu fonskiyon verilen sarta uyan ilk degeri dondurur, sarta uyan bir yapı yoksa null dondurur. Burada nesne elde edildi
            employee.FirstName = "abc";
            employee.Notes = "abc";
            //Bu sekilde bazı guncellemeler yapılsın. Sımdi varolan nesne uzeirnde degisiklikler yapıldıgına gore ilgili yapı guncellenebilir.
            await _context.SaveChangesAsync();
            //Nesne uzeirnde yapılan degisiklikler efcore tarafından algılanır ve bu sekilde db ye yansıtılır.
            #endregion

            #region ChangeTracker nedir?
            /*Bu bir takip mekanizmasıdır ve context uzeirnden gelen verilerin takıbınden sorumludur. Bu takip mekanizması vesilesiyle
             bu context uzeirnden gelen verilerle ilgili islemler neticesinde update yahut delete sorgularının yapılıp yapılmayacagı 
            anlasılır.
            Az onceki veriler degistiginde efcore nasıl update yapacagını anladı? Bu mekanizma vesilesiyle anladı.
            
             Ozetle context ten gelen her bir veri kac tane olursa olsun default olarak ChangeTracker mekanizması ile kontrol edilir.*/
            #endregion

            #region Takip edilmeyen Nesneler Nasıl Guncellenir
            Customer customer = new Customer
            {
                CustomerId = "abc",
                CompanyName = "abc"
            };
            //Buradaki veri benim tarafımdan olusturuldu yani context uzeirnden gelmedi. Bu sebeple change tracker tarafından takip edilemiyor.
            //Takip edilmeyen verileri gucnellemek icin ise EFCore tarafında Update fonksiyonu gelistirilmiştir.
            _context.Customers.Update(customer);
            _context.SaveChanges();
            /*Burada abc id li bir veri var ve bu veri dbde varsa ilgili degerlerini benim veridigm gibi guncellenmesini istiyorum.
             Bu yuzden once update kullanıp daha sonra change tracker i cagırmıs oldum.
            Update fonksiyonunu kullanabilmem icin ilgili nesnede id nin verilmesi tabii ki gerekir.*/

            #endregion

            #region EntityState Nedir
            /*Bir Entity insance inin duurmunu ifade eden bir referanstır.
             Bir entity instance i demek db de bir tablonun icindeki veriyte yani satıra karsılık gelen bir nesne demektir.*/
            Customer customer1 = new Customer();
            var durum = _context.Entry(customer1).State;//Bir entity nin state i bu seilde elde edilir. Bu state ChangeTracker ile elde edilir.
            /*Burada durum üzerine detached degeri gelir. Yani suan bu nesne uzerinde bir degisiklik yapılmamıs haldedir. ChangeTracker tarafından takipsizdir. 
            Lakin ben bu nesneyi db ye insert edersem state inin added oldugunu gorebilirim. 
            Eger bu nesneyi guncellemeye tabii tutarsam state inin modified oldugunu gorebilirim. 
            Bu degerleri gormemin nedeni bu islemler sonucunda nesnenin change tracker ile takibe alınmasıdır. 
            */
            #endregion

            #region Efcore Acısından bir verinin guncellenmesi gerektigi nasıl anlasılır.
            var value = await _context.Customers.FirstOrDefaultAsync(x => x.CustomerId == "abc");
            Console.WriteLine(_context.Entry(value).State);//unchanged degeri gelir. Yani takip altında fakat degisiklik yok
            value.ContactTitle = "djlajlad";//Bir degisiklik yapıldı.
            Console.WriteLine(_context.Entry(value).State);//modified degeri gelir. Yani nesnede bir degisiklik oldu demektir.
            await _context.SaveChangesAsync();//db ye aktarım yapıldı. Butun degisiklikler kaydedildi.
            Console.WriteLine(_context.Entry(value).State);//unchanged degeri gelir. Cunku butun degisiklikler db ye aktarılmıstır.
            #endregion

            #region Birden fazla veri guncellerken nelere dikkat edilmesi gerekir?
            /*Soru aslında sudur: Her guncellemeden sonra mı SaveChanges cagrılır yoksa butun guncellemeler yapıldıktan sonra mı cagrılır?
             Bu sorunun cevabı cogu zaman aynıdır. Fazla transaction olusmasın diye en sonra saceChanges cagrılması daha iyidir. Performansı artırır.*/

            var values= await _context.Customers.ToListAsync();//Bu bir select*from sorgusudur.
            foreach (var item in values)
            {
                item.CompanyName += "*";//her name ozelliginin yanına bir * ekledim. Yani entity yi degistiriyorum.
                
                //await _context.SaveChangesAsync();//Eger ben saveChanges fonk. burada cagırırsam her dongude bir transaction olusturmus olurum.
                //1000 urun musteri varsa 1000 kez execute islemi yapılıyor demektir.
            }
            await _context.SaveChangesAsync();
            /*Lakin SaveChanges fonksiyonunu burada cagırırsam 1000 urun varsa ve bunlar degisiyorsa bir tane transaction ile bu veriler db ye
            aktarılmıs olur. Bu muazzam bir maliyet tasarrufu demektir. Tek sorgu icine butun guncellenmesi gereken nesneler alınır ve oyle 
            execute  yapılır. Bu yuzden tasarruf saglanır.*/
            #endregion

        }

    }
}
