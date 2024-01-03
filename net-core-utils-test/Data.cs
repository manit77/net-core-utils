using System.Collections;
using System.Diagnostics;
using System.Dynamic;

namespace net_core_utils_test
{
    struct AgeData
    {
        public int ExpectedAge;
        public DateTime DOB;
        public DateTime CurrentDate;
    }

    [TestClass]
    public class TestData
    {
        [TestMethod]
        public void TestCalculateAge()
        {
            List<AgeData> arr = new List<AgeData>();
            var data = new AgeData();
            data.ExpectedAge = 46;
            data.DOB = new DateTime(1977, 4, 21);
            data.CurrentDate = new DateTime(2024, 1, 2);

            arr.Add(data);

            data = new AgeData();
            data.ExpectedAge = 45;
            data.DOB = new DateTime(1977, 4, 21);
            data.CurrentDate = new DateTime(2023, 1, 2);
            arr.Add(data);

            data = new AgeData();
            data.ExpectedAge = 46;
            data.DOB = new DateTime(1977, 3, 1);
            data.CurrentDate = new DateTime(2023, 3, 1);
            arr.Add(data);

            data = new AgeData();
            data.ExpectedAge = 100;
            data.DOB = new DateTime(1977, 3, 1);
            data.CurrentDate = new DateTime(2077, 3, 1);
            arr.Add(data);

            data = new AgeData();
            data.ExpectedAge = 0;
            data.DOB = new DateTime(1955, 3, 23);
            data.CurrentDate = new DateTime(1955, 3, 23);
            arr.Add(data);

            data = new AgeData();
            data.ExpectedAge = 1;
            data.DOB = new DateTime(1955, 3, 23);
            data.CurrentDate = new DateTime(1956, 3, 23);
            arr.Add(data);

            data = new AgeData();
            data.ExpectedAge = 0;
            data.DOB = new DateTime(1955, 3, 23);
            data.CurrentDate = new DateTime(1956, 3, 22);
            arr.Add(data);

            foreach (var item in arr)
            {
                Debug.WriteLine($" CurrentDate={item.CurrentDate}, DOB={item.DOB}, ExpectedAge= {item.ExpectedAge} ");
                Assert.AreEqual<int>(item.ExpectedAge, CoreUtils.Data.CalculateAge(item.DOB, item.CurrentDate));
            }

        }

        [TestMethod]
        public void TestBase64EncodeDecode()
        {
            string teststring = "hello world";
            string encoded = CoreUtils.Data.Base64Encode(teststring);
            string decoded = CoreUtils.Data.Base64Decode(encoded);

            Assert.IsNotNull(encoded);
            Assert.IsTrue(encoded.Length > 0);
            Assert.AreNotEqual(teststring, encoded);
            Assert.AreNotEqual(encoded, decoded);
            Assert.AreEqual(teststring, decoded);

            try
            {
                //should throw error
                CoreUtils.Data.Base64Decode(teststring);
                Assert.Fail("Error should be thrown in invalid base64 string");
            }
            catch { }

        }

        [TestMethod]
        public void TestCastIt() 
        {
            object? val = null;
            Assert.AreEqual(0, CoreUtils.Data.CastIt<int>(val));

            val = DBNull.Value;
            Assert.AreEqual(0, CoreUtils.Data.CastIt<int>(val));

            val = 10M;
            Assert.AreEqual(10M, CoreUtils.Data.CastIt<decimal>(val));

            val = 5f;
            Assert.AreEqual(5f, CoreUtils.Data.CastIt<float>(val));
                       
            //type mismatch will result in the default value
            val = -5; //int
            Assert.AreEqual(0, CoreUtils.Data.CastIt<float>(val));
        }

        [TestMethod]
        public void TestParseIt()
        {
            object? val = null;
            //null
            Assert.AreEqual(0, CoreUtils.Data.ParseIt<int>(val));            

            //DBNull
            val = DBNull.Value;
            Assert.AreEqual(0, CoreUtils.Data.ParseIt<int>(val));

            //int
            val = "123";
            Assert.AreEqual(123, CoreUtils.Data.ParseIt<int>(val));

            val = "";
            Assert.AreEqual(null, CoreUtils.Data.ParseIt<int?>(val));

            val = "-123.56"; //will result in null
            Assert.AreEqual(null, CoreUtils.Data.ParseIt<int?>(val));

            val = "-123.56"; //will result in 0
            Assert.AreEqual(0, CoreUtils.Data.ParseIt<int>(val));

            val = "-456";
            Assert.AreEqual(-456, CoreUtils.Data.ParseIt<int>(val));

            //decimal
            val = "123";
            Assert.AreEqual(123M, CoreUtils.Data.ParseIt<decimal>(val));

            //float
            val = "123"; //string to float
            Assert.AreEqual(123f, CoreUtils.Data.ParseIt<float>(val));

            val = 123f; //float to float
            Assert.AreEqual(123f, CoreUtils.Data.ParseIt<float>(val));

            val = 123; //int to float
            Assert.AreEqual(123f, CoreUtils.Data.ParseIt<float>(val));

            val = "123.7698"; //string to float
            Assert.AreEqual(123.7698f, CoreUtils.Data.ParseIt<float>(val));

            //DateTime            
            val = "123"; //cannot parse 123 to datetime, this will result in DateTime.MinValue
            Assert.AreEqual(DateTime.MinValue, CoreUtils.Data.ParseIt<DateTime>(val));

            val = "1/20/2021 9:48:05 PM"; //string to DateTime
            Assert.AreEqual(new DateTime(2021, 1, 20, 21, 48, 5), CoreUtils.Data.ParseIt<DateTime>(val));
                       
            val = ""; //cannot parse empty string, nullable DateTime will return a null
            Assert.AreEqual(null, CoreUtils.Data.ParseIt<DateTime?>(val));

            //DateTimeOffset
            val = ""; //cannot parse empty string
            Assert.AreEqual(null, CoreUtils.Data.ParseIt<DateTimeOffset?>(val));
                        
            var cdate = DateTimeOffset.Parse("2024-01-02 10:17 PM");
            val = cdate.ToString();            
            Assert.AreEqual(cdate, CoreUtils.Data.ParseIt<DateTimeOffset>(val));

        }

    }
}