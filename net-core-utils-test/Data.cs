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

    }
}