using System;
using System.Collections.Generic;
using System.Diagnostics;
using FluentAssertions;
using Xunit;

namespace net_core_utils_test
{
    struct AgeData
    {
        public int ExpectedAge;
        public DateTime DOB;
        public DateTime CurrentDate;
    }

    public class TestData
    {
        [Fact]
        public void TestCalculateAge()
        {
            var arr = new List<AgeData>
            {
                new AgeData { ExpectedAge = 46, DOB = new DateTime(1977, 4, 21), CurrentDate = new DateTime(2024, 1, 2) },
                new AgeData { ExpectedAge = 45, DOB = new DateTime(1977, 4, 21), CurrentDate = new DateTime(2023, 1, 2) },
                new AgeData { ExpectedAge = 46, DOB = new DateTime(1977, 3, 1), CurrentDate = new DateTime(2023, 3, 1) },
                new AgeData { ExpectedAge = 100, DOB = new DateTime(1977, 3, 1), CurrentDate = new DateTime(2077, 3, 1) },
                new AgeData { ExpectedAge = 0, DOB = new DateTime(1955, 3, 23), CurrentDate = new DateTime(1955, 3, 23) },
                new AgeData { ExpectedAge = 1, DOB = new DateTime(1955, 3, 23), CurrentDate = new DateTime(1956, 3, 23) },
                new AgeData { ExpectedAge = 0, DOB = new DateTime(1955, 3, 23), CurrentDate = new DateTime(1956, 3, 22) }
            };

            foreach (var item in arr)
            {
                Debug.WriteLine($"CurrentDate={item.CurrentDate}, DOB={item.DOB}, ExpectedAge={item.ExpectedAge}");
                CoreUtils.Data.CalculateAge(item.DOB, item.CurrentDate)
                    .Should().Be(item.ExpectedAge, $"DOB={item.DOB}, CurrentDate={item.CurrentDate}");
            }
        }

        [Fact]
        public void TestBase64EncodeDecode()
        {
            string teststring = "hello world";
            string encoded = CoreUtils.Data.Base64Encode(teststring);
            string decoded = CoreUtils.Data.Base64Decode(encoded);

            encoded.Should().NotBeNullOrEmpty();
            encoded.Should().NotBe(teststring);
            decoded.Should().NotBe(encoded);
            decoded.Should().Be(teststring);

            Action act = () => CoreUtils.Data.Base64Decode(teststring);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void TestCastIt()
        {
            object? val = null;
            CoreUtils.Data.CastIt<int>(val).Should().Be(0);

            val = DBNull.Value;
            CoreUtils.Data.CastIt<int>(val).Should().Be(0);

            val = 10M;
            CoreUtils.Data.CastIt<decimal>(val).Should().Be(10M);

            val = 5f;
            CoreUtils.Data.CastIt<float>(val).Should().Be(5f);

            val = -5; // type mismatch, default value
            CoreUtils.Data.CastIt<float>(val).Should().Be(0f);
        }

        [Fact]
        public void TestParseIt()
        {
            object? val = null;
            CoreUtils.Data.ParseIt<int>(val).Should().Be(0);

            val = DBNull.Value;
            CoreUtils.Data.ParseIt<int>(val).Should().Be(0);

            val = "123";
            CoreUtils.Data.ParseIt<int>(val).Should().Be(123);

            val = "";
            CoreUtils.Data.ParseIt<int?>(val).Should().BeNull();

            val = "-123.56";
            CoreUtils.Data.ParseIt<int?>(val).Should().BeNull();
            CoreUtils.Data.ParseIt<int>(val).Should().Be(0);

            val = "-456";
            CoreUtils.Data.ParseIt<int>(val).Should().Be(-456);

            val = "123";
            CoreUtils.Data.ParseIt<decimal>(val).Should().Be(123M);

            val = "123";
            CoreUtils.Data.ParseIt<float>(val).Should().Be(123f);

            val = 123f;
            CoreUtils.Data.ParseIt<float>(val).Should().Be(123f);

            val = 123;
            CoreUtils.Data.ParseIt<float>(val).Should().Be(123f);

            val = "123.7698";
            CoreUtils.Data.ParseIt<float>(val).Should().BeApproximately(123.7698f, 0.0001f);

            val = "123";
            CoreUtils.Data.ParseIt<DateTime>(val).Should().Be(DateTime.MinValue);

            val = "1/20/2021 9:48:05 PM";
            CoreUtils.Data.ParseIt<DateTime>(val).Should().Be(new DateTime(2021, 1, 20, 21, 48, 5));

            val = "";
            CoreUtils.Data.ParseIt<DateTime?>(val).Should().BeNull();

            val = "";
            CoreUtils.Data.ParseIt<DateTimeOffset?>(val).Should().BeNull();

            val = "a";
            CoreUtils.Data.ParseIt<DateTimeOffset?>(val).Should().BeNull();

             val = "";
            CoreUtils.Data.ParseIt<DateTimeOffset>(val).Should().Be(DateTimeOffset.MinValue);

            var cdate = DateTimeOffset.Parse("2024-01-02 10:17 PM");
            val = cdate.ToString();
            CoreUtils.Data.ParseIt<DateTimeOffset>(val).Should().Be(cdate);
        }
    }
}
